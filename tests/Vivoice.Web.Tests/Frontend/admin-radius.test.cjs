// Purpose: Verifies the real admin form sends editable radius values and blocks zero/negative input before API writes.
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

function fixture() {
  const elements = new Map(), writes = [];
  let stored;
  const byId = id => {
    if (!elements.has(id)) elements.set(id, {
      value: "", checked: true, textContent: "", children: [], listeners: {},
      classList: { add() {}, remove() {} },
      addEventListener(event, callback) { this.listeners[event] = callback; },
      append(...children) { this.children.push(...children); }, replaceChildren() { this.children = []; },
      reset() {}, focus() {}
    });
    return elements.get(id);
  };
  const response = (body, status = 200) => ({ ok: status < 400, status, json: async () => body });
  const context = {
    document: { getElementById: byId, createElement: () => byId(`generated-${elements.size}`), createDocumentFragment: () => byId(`fragment-${elements.size}`), addEventListener() {} },
    addEventListener() {}, setInterval() {}, clearInterval() {},
    fetch: async (url, options = {}) => {
      if (options.method === "POST" || options.method === "PATCH") {
        writes.push({ url, ...options }); const data = JSON.parse(options.body);
        stored = { ...data, id: 1 }; return response(stored);
      }
      if (url.endsWith("/antiforgery")) return response({ token: "test-only-token" });
      if (url.endsWith("/me")) return response({}, 401);
      if (url === "/api/admin/pois") return response(stored ? [stored] : []);
      return response([]);
    }
  };
  context.window = context;
  vm.createContext(context);
  vm.runInContext(fs.readFileSync(path.resolve(__dirname, "../../../src/Vivoice.Web/wwwroot/admin-dashboard-refresh.js"), "utf8"), context);
  vm.runInContext(fs.readFileSync(path.resolve(__dirname, "../../../src/Vivoice.Web/wwwroot/admin.js"), "utf8"), context);
  byId("poiSlug").value = "sample"; byId("poiOrder").value = "1"; byId("poiLatitude").value = "21"; byId("poiLongitude").value = "105";
  byId("poiName").value = "Địa điểm"; byId("poiSummary").value = "Mô tả";
  return { byId, writes, submit: () => byId("poiForm").listeners.submit({ preventDefault() {} }) };
}

test("creating and editing a POI sends radiusMeters and restores the returned radius into the form", async () => {
  const f = fixture();
  f.byId("poiRadius").value = "100"; await f.submit();
  assert.equal(f.writes[0].method, "POST");
  assert.equal(JSON.parse(f.writes[0].body).radiusMeters, 100);
  assert.equal(f.byId("poiRadius").value, 100);
  f.byId("poiRadius").value = "37.5"; await f.submit();
  assert.equal(f.writes[1].method, "PATCH");
  assert.equal(f.writes[1].url, "/api/admin/pois/1");
  assert.equal(JSON.parse(f.writes[1].body).radiusMeters, 37.5);
  assert.equal(f.byId("poiRadius").value, 37.5);
});

test("zero, negative and invalid radius values never reach an admin write endpoint", async () => {
  const f = fixture();
  for (const value of ["0", "-10", "", "NaN", "Infinity"]) {
    f.byId("poiRadius").value = value; await f.submit();
    assert.match(f.byId("poiStatus").textContent, /lớn hơn 0/);
  }
  assert.equal(f.writes.length, 0);
});
