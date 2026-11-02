// Purpose: Runs the real public client against simulated DOM, GPS, APIs and speech to verify the GPS-to-audio flow.
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");
const root = path.resolve(__dirname, "../../../src/Vivoice.Web/wwwroot");
const flush = () => new Promise(resolve => setImmediate(resolve));

class Element {
  constructor() { this.value = ""; this.textContent = ""; this.className = ""; this.dataset = {}; this.children = []; this.listeners = {}; this.attributes = {}; this.classes = new Set(); this.classList = { add: value => this.classes.add(value), remove: value => this.classes.delete(value), toggle: (value, force) => force ? this.classes.add(value) : this.classes.delete(value), contains: value => this.classes.has(value) }; }
  setAttribute(key, value) { this.attributes[key] = value; }
  append(...values) { this.children.push(...values); }
  replaceChildren(...values) { this.children = values; }
  addEventListener(key, callback) { this.listeners[key] = callback; }
  dispatch(key) { return this.listeners[key]?.({ preventDefault() {} }); }
}

function fixture(t, { delayNarration = false, delayPois = false, storedLanguage = "vi" } = {}) {
  const elements = new Map();
  const byId = id => { if (!elements.has(id)) elements.set(id, new Element()); return elements.get(id); };
  const i18nElements = [...fs.readFileSync(path.join(root, "index.html"), "utf8").matchAll(/data-i18n="([^"]+)"/g)].map(([, key]) => {
    const element = byId(`i18n:${key}`); element.dataset.i18n = key; return element;
  });
  const savedPreferences = [];
  const spoken = [], requests = [], cleared = [], circles = [], fittedBounds = [], circleLayers = [], pointLayers = [];
  const windowEvents = {};
  let success, failure, delayedResponse, delayedPoisResponse;
  const response = (value, status = 200) => ({ ok: status < 400, status, json: async () => value });
  const layer = (location, options = {}) => ({
    location, options: { ...options }, removed: false,
    addTo() { return this; }, bindTooltip() { return this; }, on() { return this; },
    remove() { this.removed = true; },
    setLatLng(value) { this.location = value; return this; },
    setRadius(value) { this.options.radius = value; return this; },
    getBounds() { return { circle: this, extend(point) { this.extendedPoint = point; return this; } }; }
  });
  const timers = new Map();
  let timerId = 0;
  const context = {
    console, AbortController, Promise, Date,
    navigator: { geolocation: { watchPosition(onSuccess, onFailure) { success = onSuccess; failure = onFailure; return 10; }, clearWatch(id) { cleared.push(id); } } },
    isSecureContext: true,
    document: { documentElement: {}, getElementById: byId, querySelectorAll: () => i18nElements, createElement: () => new Element() },
    localStorage: { getItem: () => storedLanguage, setItem(key, value) { savedPreferences.push({ key, value }); } },
    addEventListener: (key, callback) => { windowEvents[key] = callback; },
    setTimeout: callback => { const id = ++timerId; timers.set(id, callback); return id; }, clearTimeout: id => timers.delete(id), setInterval() {},
    speechSynthesis: { getVoices: () => [], addEventListener() {}, cancel() {}, speak(utterance) { spoken.push(utterance); utterance.onstart?.(); } },
    SpeechSynthesisUtterance: class { constructor(text) { this.text = text; } },
    L: {
      map: () => ({ setView() { return this; }, fitBounds(bounds) { fittedBounds.push(bounds); return this; } }),
      tileLayer: layer, marker: layer,
      circleMarker: (location, options) => { const item = layer(location, options); pointLayers.push(item); return item; },
      circle: (location, options) => { circles.push(options.radius); const item = layer(location, options); circleLayers.push(item); return item; }
    },
    fetch: async (url, options = {}) => {
      requests.push({ url, options });
      if (url.includes("/narration")) {
        const lang = new URL(url, "https://example.test").searchParams.get("lang");
        const body = { title: "Câu chuyện", content: "Nội dung thuyết minh", languageCode: lang, category: "culture", fallbackUsed: false };
        if (delayNarration) return new Promise(resolve => { delayedResponse = () => resolve(response(body)); });
        return response(body);
      }
      if (url.includes("/topics")) return response([1, 2, 3].map(id => ({ id, title: `Chủ đề ${id}`, category: "culture", displayOrder: id })));
      if (url.startsWith("/api/public/pois")) {
        const body = [{ id: 1, name: "Hà Nội", summary: "Thủ đô", latitude: 21, longitude: 105, radiusMeters: 100, displayOrder: 1 }];
        if (delayPois) return new Promise(resolve => { delayedPoisResponse = () => resolve(response(body)); });
        return response(body);
      }
      if (url.includes("advertisements")) return response([]);
      return response(null, 204);
    }
  };
  context.window = context;
  vm.createContext(context);
  for (const file of ["tts-language.js", "visitor-session.js", "gps-guide.js", "public.js"]) vm.runInContext(fs.readFileSync(path.join(root, file), "utf8"), context, { filename: file });
  t.after(() => windowEvents.pagehide?.());
  return { byId, spoken, requests, circles, cleared, fittedBounds, circleLayers, pointLayers, savedPreferences, i18nElements, documentElement: context.document.documentElement,
    fix(north = 0, accuracy = 5) { success({ coords: { latitude: 21 + north / 111194.9266, longitude: 105, accuracy }, timestamp: Date.now() }); },
    deny() { failure({ code: 1 }); },
    finish() { spoken.at(-1).onend(); },
    resolveNarration() { delayedResponse(); },
    resolvePois() { delayedPoisResponse(); }
  };
}

test("manual selections only preview text; entering GPS radius plays all topics in sequence once", async t => {
  const f = fixture(t); await flush();
  assert.equal(f.fittedBounds[0][0][0], 21);
  assert.equal(f.fittedBounds[0][0][1], 105);
  await f.byId("poiGrid").children[0].dispatch("click");
  await flush();
  await f.byId("topicGrid").children[0].dispatch("click"); await flush();
  assert.equal(f.spoken.length, 0); assert.equal(f.byId("narrationText").textContent, "Nội dung thuyết minh");
  assert.ok(f.circles.includes(100));
  f.byId("toggleGps").dispatch("click"); f.fix(200); await flush(); assert.equal(f.spoken.length, 0);
  f.fix(); await flush(); assert.equal(f.spoken.length, 1); assert.equal(f.spoken[0].lang, "vi-VN");
  f.fix(); await flush(); assert.equal(f.spoken.length, 1);
  f.finish(); await flush(); assert.equal(f.spoken.length, 2);
  f.finish(); await flush(); assert.equal(f.spoken.length, 3);
  f.finish(); await flush(); f.fix(); await flush(); assert.equal(f.spoken.length, 3);
  assert.ok(f.requests.every(request => !request.options.body || !/latitude|longitude|accuracy/.test(request.options.body)));
});

test("leaving the region cancels audio; entering again rearms it; disabling GPS clears its watch", async t => {
  const f = fixture(t); await flush();
  f.byId("toggleGps").dispatch("click"); f.fix(); await flush();
  const first = f.spoken[0];
  f.fix(200); first.onend(); await flush(); assert.equal(f.spoken.length, 1);
  f.fix(); await flush(); assert.equal(f.spoken.length, 2);
  f.byId("toggleGps").dispatch("click"); f.spoken.at(-1).onend(); await flush();
  assert.deepEqual(f.cleared, [10]); assert.equal(f.byId("toggleGps").attributes["aria-pressed"], "false");
  assert.equal(f.spoken.length, 2);
});

test("a late narration response after geofence exit must never start speech", async t => {
  const f = fixture(t, { delayNarration: true }); await flush();
  f.byId("toggleGps").dispatch("click"); f.fix(); await flush();
  f.fix(200); f.resolveNarration(); await flush(); assert.equal(f.spoken.length, 0);
});

test("language changes cancel old playback and use the new narration language", async t => {
  const f = fixture(t); await flush();
  f.byId("toggleGps").dispatch("click"); f.fix(); await flush();
  f.byId("languageSelect").value = "ja";
  await f.byId("languageSelect").dispatch("change"); await flush();
  assert.equal(f.spoken.at(-1).lang, "ja-JP");
});

test("English replaces Thai in the selector, renders the UI and requests English GPS narration", async t => {
  const f = fixture(t, { storedLanguage: "en" }); await flush();
  assert.deepEqual(f.byId("languageSelect").children.map(option => option.value), ["vi", "ko", "ja", "zh", "en"]);
  assert.equal(f.byId("languageSelect").children.at(-1).textContent, "English");
  assert.equal(f.documentElement.lang, "en");
  assert.equal(f.byId("toggleGps").textContent, "Enable location");
  assert.equal(f.byId("i18n:userRadiusLabel").textContent, "Radius around you");
  assert.ok(f.i18nElements.every(element => element.textContent && element.textContent !== element.dataset.i18n));
  f.byId("toggleGps").dispatch("click"); f.fix(); await flush();
  assert.equal(f.spoken[0].lang, "en-US");
  assert.match(f.requests.find(request => request.url.includes("/narration")).url, /lang=en$/);
});

test("a previously saved Thai preference is migrated to English", async t => {
  const f = fixture(t, { storedLanguage: "th" }); await flush();
  assert.equal(f.byId("languageSelect").value, "en");
  assert.equal(f.documentElement.lang, "en");
  assert.deepEqual(f.savedPreferences, [{ key: "vivoice_language", value: "en" }]);
});

test("blocked speech shows an honest error and replay; GPS denial stops the guide", async t => {
  const f = fixture(t); await flush();
  f.byId("toggleGps").dispatch("click"); f.fix(); await flush();
  f.spoken[0].onerror(); await flush();
  assert.equal(f.byId("speechStatus").dataset.state, "error");
  assert.equal(f.byId("replaySpeech").classList.contains("hidden"), false);
  f.byId("replaySpeech").dispatch("click"); assert.equal(f.spoken.length, 2);
  f.deny(); await flush();
  assert.match(f.byId("gpsStatus").textContent, /chưa cho phép/);
  assert.equal(f.byId("toggleGps").attributes["aria-pressed"], "false");
});

test("the 100m user circle follows GPS independently of accuracy and can be resized without restarting speech", async t => {
  const f = fixture(t); await flush();
  assert.equal(f.byId("userRadiusSelect").value, "100");
  assert.equal(f.byId("centerOnMe").disabled, true);
  assert.equal(f.circleLayers.some(item => item.options.className === "user-radius"), false);
  f.byId("toggleGps").dispatch("click"); f.fix(); await flush();
  const range = f.circleLayers.find(item => item.options.className === "user-radius");
  const accuracy = f.circleLayers.find(item => item.options.className === "gps-accuracy");
  assert.equal(range.options.radius, 100); assert.equal(accuracy.options.radius, 5);
  assert.equal(range.options.interactive, false);
  assert.equal(f.byId("centerOnMe").disabled, false);
  f.fix(50, 25); await flush();
  assert.ok(Math.abs(range.location[0] - (21 + 50 / 111194.9266)) < 0.000001);
  assert.equal(range.options.radius, 100); assert.equal(accuracy.options.radius, 25);
  assert.match(f.byId("positionAccuracy").textContent, /25/);
  f.byId("userRadiusSelect").value = "500"; f.byId("userRadiusSelect").dispatch("change");
  assert.equal(range.options.radius, 500); assert.equal(accuracy.options.radius, 25);
  assert.equal(f.spoken.length, 1);
  assert.equal(f.circleLayers.filter(item => item.options.className === "user-radius").length, 1);
  f.byId("centerOnMe").dispatch("click");
  assert.equal(f.fittedBounds.at(-1).circle, range);
});

test("the range can be chosen before GPS starts while invalid sizes leave the last valid range intact", async t => {
  const f = fixture(t); await flush();
  f.byId("userRadiusSelect").value = "0"; f.byId("userRadiusSelect").dispatch("change");
  assert.equal(f.byId("userRadiusSelect").value, "100");
  f.byId("userRadiusSelect").value = "200"; f.byId("userRadiusSelect").dispatch("change");
  assert.equal(f.circleLayers.some(item => item.options.className === "user-radius"), false);
  f.byId("toggleGps").dispatch("click"); f.fix(200); await flush();
  assert.equal(f.circleLayers.find(item => item.options.className === "user-radius").options.radius, 200);
  assert.equal(f.spoken.length, 0);
});

test("turning GPS off or denying permission removes the location and both user circles", async t => {
  const f = fixture(t); await flush();
  f.byId("toggleGps").dispatch("click"); f.fix(200); await flush();
  const range = f.circleLayers.find(item => item.options.className === "user-radius");
  const accuracy = f.circleLayers.find(item => item.options.className === "gps-accuracy");
  f.byId("toggleGps").dispatch("click");
  assert.equal(range.removed, true); assert.equal(accuracy.removed, true); assert.equal(f.pointLayers[0].removed, true);
  assert.equal(f.byId("centerOnMe").disabled, true);
  assert.equal(f.byId("positionAccuracy").textContent, "Chưa có vị trí");
  f.byId("toggleGps").dispatch("click"); f.fix(200); await flush(); f.deny();
  assert.equal(f.circleLayers.filter(item => item.options.className === "user-radius" && !item.removed).length, 0);
  assert.equal(f.circleLayers.filter(item => item.options.className === "gps-accuracy" && !item.removed).length, 0);
  assert.equal(f.byId("centerOnMe").disabled, true);
});

test("a slow POI response does not move the map away from the first GPS circle", async t => {
  const f = fixture(t, { delayPois: true }); await flush();
  f.byId("toggleGps").dispatch("click"); f.fix(2000); await flush();
  const range = f.circleLayers.find(item => item.options.className === "user-radius");
  assert.equal(f.fittedBounds.at(-1).circle, range);
  f.resolvePois(); await flush();
  assert.equal(f.fittedBounds.at(-1).circle, range);
  assert.equal(f.spoken.length, 0);
});
