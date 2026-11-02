// Purpose: Verifies public narration autoplay state without requiring a browser, speech voice or database.
// Responsibility: Covers ordered progression, toggle behavior and invalidation of stale speech callbacks.
const test = require("node:test");
const assert = require("node:assert/strict");
const { createAutoplaySequence } = require("../../../src/Vivoice.Web/wwwroot/autoplay-sequence.js");

test("autoplay progresses exactly through three ordered topics and then stops", () => {
  const sequence = createAutoplaySequence(true);
  const first = sequence.begin(8, [101, 102, 103], 101);
  assert.deepEqual(first, { token: 1, poiId: 8, topicId: 101, index: 0, total: 3 });
  assert.deepEqual(sequence.nextAfterEnd(first.token, 8, 101), { token: 1, poiId: 8, topicId: 102, index: 1, total: 3 });
  assert.deepEqual(sequence.nextAfterEnd(first.token, 8, 102), { token: 1, poiId: 8, topicId: 103, index: 2, total: 3 });
  assert.equal(sequence.nextAfterEnd(first.token, 8, 103), null);
  assert.equal(sequence.current(), null);
});

test("turning autoplay off cancels the active sequence without altering the selected topic externally", () => {
  const sequence = createAutoplaySequence(true);
  const first = sequence.begin(8, [101, 102, 103], 101);
  assert.equal(sequence.setEnabled(false), false);
  assert.equal(sequence.nextAfterEnd(first.token, 8, 101), null);
  assert.equal(sequence.current(), null);
});

test("a stale callback from a prior POI or manual selection cannot advance a newer sequence", () => {
  const sequence = createAutoplaySequence(true);
  const previous = sequence.begin(8, [101, 102, 103], 101);
  const current = sequence.begin(9, [201, 202, 203], 202);
  assert.equal(sequence.nextAfterEnd(previous.token, 8, 101), null);
  assert.deepEqual(sequence.current(), current);
  assert.deepEqual(sequence.nextAfterEnd(current.token, 9, 202), { token: current.token, poiId: 9, topicId: 203, index: 2, total: 3 });
});

test("manual topic selection can begin from the chosen topic without replaying older items", () => {
  const sequence = createAutoplaySequence(true);
  const manual = sequence.begin(8, [101, 102, 103], 102);
  assert.equal(manual.topicId, 102);
  assert.deepEqual(sequence.nextAfterEnd(manual.token, 8, 102), { token: manual.token, poiId: 8, topicId: 103, index: 2, total: 3 });
});
