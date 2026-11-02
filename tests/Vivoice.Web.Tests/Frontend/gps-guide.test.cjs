// Purpose: Exercises GPS entry, exit, accuracy and asynchronous narration lifecycle without real coordinates or a browser.
const test = require("node:test");
const assert = require("node:assert/strict");
const { distanceMeters, createTracker, createGuide } = require("../../../src/Vivoice.Web/wwwroot/gps-guide.js");

const poi = (id = 1, north = 0, radiusMeters = 100) => ({ id, latitude: north / 111194.9266, longitude: 0, radiusMeters, displayOrder: id });
const position = (north = 0, accuracy = 5) => ({ latitude: north / 111194.9266, longitude: 0, accuracy });
const flush = () => new Promise(resolve => setImmediate(resolve));

test("distance handles real meter units, identical positions and the international date line", () => {
  assert.equal(distanceMeters(position(), position()), 0);
  assert.ok(Math.abs(distanceMeters(position(), position(1000)) - 1000) < 0.01);
  assert.ok(Math.abs(distanceMeters({ latitude: 0, longitude: 179.999 }, { latitude: 0, longitude: -179.999 }) - 222.39) < 0.1);
  assert.equal(distanceMeters({ latitude: NaN, longitude: 0 }, position()), Infinity);
});

test("one entry is emitted per visit and GPS boundary jitter does not rearm playback", () => {
  const tracker = createTracker();
  const pois = [poi()];
  assert.equal(tracker.update(position(150), pois).entered.length, 0);
  assert.deepEqual(tracker.update(position(90), pois).entered, pois);
  assert.equal(tracker.update(position(80), pois).entered.length, 0);
  assert.equal(tracker.update(position(105), pois).exited.length, 0);
  assert.equal(tracker.update(position(95), pois).entered.length, 0);
  assert.deepEqual(tracker.update(position(120), pois).exited, [1]);
  assert.deepEqual(tracker.update(position(90), pois).entered, pois);
});

test("an initial fix inside a POI triggers; inaccurate fixes and nonpositive radii do not", () => {
  const tracker = createTracker();
  assert.equal(tracker.update(position(0, 200), [poi()]).entered.length, 0);
  assert.equal(tracker.update(position(), [poi(1, 0, 0), poi(2, 0, -1)]).entered.length, 0);
  assert.deepEqual(tracker.update(position(), [poi()]).entered.map(item => item.id), [1]);
  assert.equal(tracker.update(position(200, 200), [poi()]).exited.length, 0);
  assert.deepEqual(tracker.update(position(), []).exited, [1]);
});

test("overlapping POIs enter in nearest-first order", () => {
  const tracker = createTracker();
  assert.deepEqual(tracker.update(position(), [poi(1, 60), poi(2, 10)]).entered.map(item => item.id), [2, 1]);
});

function fixture(playOverride) {
  let success, failure;
  let clock = 100000;
  const plays = [], statuses = [], cleared = [], positions = [];
  let watching = 0;
  const guide = createGuide({
    isSecureContext: true,
    geolocation: {
      watchPosition(onSuccess, onError, options) { success = onSuccess; failure = onError; watching++; assert.equal(options.enableHighAccuracy, true); return watching; },
      clearWatch(id) { cleared.push(id); }
    },
    now: () => clock,
    play: (target, signal) => {
      const run = { target, signal };
      plays.push(run);
      return playOverride ? playOverride(run) : new Promise(resolve => { run.finish = resolve; });
    },
    cancel: () => {},
    onPosition: value => positions.push(value),
    onStatus: value => statuses.push(value)
  });
  return { guide, plays, statuses, cleared, positions,
    fix(north = 0, accuracy = 5, timestamp = clock) { success({ coords: position(north, accuracy), timestamp }); },
    fail(code) { failure({ code }); },
    advance(ms) { clock += ms; },
    get watching() { return watching; }
  };
}

test("overlap narrations are serialized and repeated fixes do not replay them", async () => {
  const f = fixture();
  f.guide.setPois([poi(1, 60), poi(2, 10)]); f.guide.start(); f.guide.start();
  assert.equal(f.watching, 1);
  f.fix(); await flush();
  assert.deepEqual(f.plays.map(run => run.target.id), [2]);
  f.fix(); f.plays[0].finish(); await flush();
  assert.deepEqual(f.plays.map(run => run.target.id), [2, 1]);
  f.plays[1].finish(); await flush(); f.fix(); await flush();
  assert.equal(f.plays.length, 2);
});

test("exit aborts pending content and a late completion cannot interfere with a new visit", async () => {
  const f = fixture();
  f.guide.setPois([poi()]); f.guide.start(); f.fix(); await flush();
  f.fix(200); assert.equal(f.plays[0].signal.aborted, true);
  f.fix(); await flush(); assert.equal(f.plays.length, 2);
  f.plays[0].finish(); await flush();
  assert.equal(f.plays[1].signal.aborted, false);
  f.guide.stop(); assert.equal(f.plays[1].signal.aborted, true);
  assert.deepEqual(f.cleared, [1]); assert.equal(f.positions.at(-1), null);
  f.fix(); await flush(); assert.equal(f.plays.length, 2);
});

test("GPS can be enabled before POIs finish loading; stale and out-of-order fixes are ignored", async () => {
  const f = fixture();
  f.guide.start(); f.fix(); f.guide.setPois([poi()]); await flush();
  assert.equal(f.plays.length, 1);
  f.fix(200, 5, 99000); assert.equal(f.plays[0].signal.aborted, false);
  f.advance(40000); f.fix(200, 5, 100000);
  assert.equal(f.guide.isWithinRange(1), false);
  f.fix(200); assert.equal(f.plays[0].signal.aborted, true);
});

test("permission denial clears the watch, cancels playback and permits an explicit retry", async () => {
  const f = fixture();
  f.guide.setPois([poi()]); f.guide.start(); f.fix(); await flush(); f.fail(1);
  assert.equal(f.guide.isActive(), false); assert.equal(f.statuses.at(-1), "denied");
  assert.equal(f.plays[0].signal.aborted, true); assert.deepEqual(f.cleared, [1]);
  f.guide.start(); f.fix(); await flush(); assert.equal(f.plays.length, 2);
});

test("insecure and unsupported browsers never start a position watch", () => {
  const statuses = [];
  const options = { play() {}, cancel() {}, onStatus: value => statuses.push(value) };
  createGuide({ ...options, isSecureContext: false }).start();
  createGuide({ ...options, isSecureContext: true }).start();
  assert.deepEqual(statuses, ["insecure", "unsupported"]);
});

test("manual interruption suppresses remaining POIs until a fresh entry; language refresh rearms inside POIs", async () => {
  const f = fixture();
  f.guide.setPois([poi()]); f.guide.start(); f.fix(); await flush();
  f.guide.interrupt(); f.fix(); await flush(); assert.equal(f.plays.length, 1);
  f.guide.refresh(); await flush(); assert.equal(f.plays.length, 2);
});
