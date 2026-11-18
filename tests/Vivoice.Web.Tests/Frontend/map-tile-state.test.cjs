// Purpose: Verifies deterministic map-provider failover without requiring a network tile service or browser map instance.
// Responsibility: Guards the local fallback so a remote tile outage cannot leave the public map state ambiguous.
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");
const vm = require("node:vm");

const source = fs.readFileSync(path.join(__dirname, "../../../src/Vivoice.Web/wwwroot/map-tile-state.js"), "utf8");
const context = { window: {} };
vm.runInNewContext(source, context, { filename: "map-tile-state.js" });
const { createMapTileState } = context.window.VivoiceMapTileState;

test("switches to the next provider only after the configured tile-error threshold", () => {
  const state = createMapTileState(2, 3);

  assert.equal(state.recordTileError().action, "waiting");
  assert.equal(state.recordTileError().action, "waiting");
  const transition = state.recordTileError();

  assert.equal(transition.action, "switch");
  assert.equal(transition.providerIndex, 1);
  assert.equal(transition.state.mode, "switching");
});

test("falls back locally after every configured provider reaches its error threshold", () => {
  const state = createMapTileState(2, 2);

  state.recordTileError();
  const firstSwitch = state.recordTileError();
  state.beginProvider(firstSwitch.providerIndex);
  state.recordTileError();
  const fallback = state.recordTileError();

  assert.equal(fallback.action, "offline");
  assert.equal(fallback.state.mode, "offline");
});

test("a successful tile restores online state and a subsequent failure is only partial", () => {
  const state = createMapTileState(2, 2);

  assert.equal(state.recordTileLoad().state.mode, "online");
  const partial = state.recordTileError();

  assert.equal(partial.action, "partial");
  assert.equal(partial.state.mode, "partial");
});

test("retry returns to the first provider in a clean loading state", () => {
  const state = createMapTileState(2, 1);

  state.recordTileError();
  const retry = state.retry();

  assert.equal(retry.action, "retry");
  assert.equal(retry.providerIndex, 0);
  assert.equal(retry.state.providerIndex, 0);
  assert.equal(retry.state.successfulTiles, 0);
  assert.equal(retry.state.failedTiles, 0);
  assert.equal(retry.state.mode, "loading");
});
