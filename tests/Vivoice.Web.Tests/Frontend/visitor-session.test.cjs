// Purpose: Verifies public telemetry can recover from a transient visitor-session issuance failure without duplicate successful issuance.
// Responsibility: Tests the browser-independent issuer used by heartbeat and advertisement telemetry.
const test = require("node:test");
const assert = require("node:assert/strict");
const { createSessionIssuer } = require("../../../src/Vivoice.Web/wwwroot/visitor-session.js");

test("concurrent callers share one in-flight visitor-session issuance request", async () => {
  let calls = 0;
  let finishIssue;
  const issuer = createSessionIssuer(() => {
    calls += 1;
    return new Promise(resolve => { finishIssue = resolve; });
  });

  const first = issuer();
  const second = issuer();
  await Promise.resolve();
  assert.equal(calls, 1);
  finishIssue();
  assert.equal(await first, true);
  assert.equal(await second, true);
});

test("a failed issuance is not cached and a later caller can retry", async () => {
  let calls = 0;
  const issuer = createSessionIssuer(() => {
    calls += 1;
    return calls === 1 ? Promise.reject(new Error("temporary failure")) : Promise.resolve();
  });

  assert.equal(await issuer(), false);
  assert.equal(await issuer(), true);
  assert.equal(calls, 2);
});

test("a successful issuance is cached and does not produce duplicate cookies", async () => {
  let calls = 0;
  const issuer = createSessionIssuer(() => { calls += 1; return Promise.resolve(); });

  assert.equal(await issuer(), true);
  assert.equal(await issuer(), true);
  assert.equal(calls, 1);
});
