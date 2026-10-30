// Purpose: Keeps public narration autoplay deterministic across POI, topic and browser speech changes.
// Responsibility: Exposes a small state machine so stale speech callbacks can never advance an obsolete sequence.
(() => {
  function uniqueTopicIds(topicIds) {
    return Array.isArray(topicIds)
      ? [...new Set(topicIds.filter(topicId => Number.isInteger(topicId) || (typeof topicId === "string" && topicId.length > 0)))]
      : [];
  }

  function createAutoplaySequence(initiallyEnabled = true) {
    let enabled = Boolean(initiallyEnabled);
    let generation = 0;
    let run = null;

    function invalidate() {
      generation += 1;
      run = null;
      return generation;
    }

    function setEnabled(nextEnabled) {
      enabled = Boolean(nextEnabled);
      if (!enabled) invalidate();
      return enabled;
    }

    function begin(poiId, topicIds, startTopicId) {
      const orderedTopicIds = uniqueTopicIds(topicIds);
      const startIndex = Math.max(0, orderedTopicIds.indexOf(startTopicId));
      const token = ++generation;
      run = enabled && poiId != null && orderedTopicIds.length > 0
        ? { token, poiId, topicIds: orderedTopicIds, index: startIndex }
        : null;
      return run ? { token: run.token, poiId: run.poiId, topicId: run.topicIds[run.index], index: run.index, total: run.topicIds.length } : null;
    }

    function current() {
      return run ? { token: run.token, poiId: run.poiId, topicId: run.topicIds[run.index], index: run.index, total: run.topicIds.length } : null;
    }

    function nextAfterEnd(token, poiId, topicId) {
      if (!enabled || !run || token !== run.token || poiId !== run.poiId || topicId !== run.topicIds[run.index]) return null;
      const nextIndex = run.index + 1;
      if (nextIndex >= run.topicIds.length) {
        run = null;
        return null;
      }

      run.index = nextIndex;
      return { token: run.token, poiId: run.poiId, topicId: run.topicIds[run.index], index: run.index, total: run.topicIds.length };
    }

    return Object.freeze({ setEnabled, begin, current, nextAfterEnd, invalidate, isEnabled: () => enabled });
  }

  const api = Object.freeze({ createAutoplaySequence });
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  globalThis.VivoiceAutoplaySequence = api;
})();
