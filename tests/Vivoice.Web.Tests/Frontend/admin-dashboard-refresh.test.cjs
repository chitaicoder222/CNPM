// Purpose: Verifies Admin dashboard polling has one timer and never overlaps a dashboard refresh.
// Responsibility: Covers timer lifecycle independently from browser forms, APIs or a database.
const test = require("node:test");
const assert = require("node:assert/strict");
const { createDashboardRefreshController } = require("../../../src/Vivoice.Web/wwwroot/admin-dashboard-refresh.js");

test("dashboard polling owns one timer and pauses/resumes without duplication", () => {
  let timerId = 0;
  const timers = new Map();
  const controller = createDashboardRefreshController({
    refresh: async () => {},
    setIntervalFn: callback => { const id = ++timerId; timers.set(id, callback); return id; },
    clearIntervalFn: id => timers.delete(id)
  });

  controller.start();
  controller.start();
  assert.equal(timers.size, 1);
  assert.equal(controller.isRunning(), true);

  controller.pause();
  assert.equal(timers.size, 0);
  controller.resume();
  controller.resume();
  assert.equal(timers.size, 1);

  controller.stop();
  assert.equal(timers.size, 0);
  assert.equal(controller.isRunning(), false);
});

test("dashboard refresh does not overlap an in-flight request", async () => {
  let resolveRefresh;
  let calls = 0;
  const controller = createDashboardRefreshController({
    refresh: () => { calls += 1; return new Promise(resolve => { resolveRefresh = resolve; }); }
  });

  const first = controller.refreshNow("manual");
  const second = await controller.refreshNow("auto");
  assert.equal(second, false);
  assert.equal(calls, 1);
  assert.equal(controller.isRefreshing(), true);

  resolveRefresh();
  assert.equal(await first, true);
  assert.equal(controller.isRefreshing(), false);
});
