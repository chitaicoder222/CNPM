// Purpose: Coordinates bounded Admin dashboard refreshes without touching editor forms or domain data.
// Responsibility: Owns one optional polling timer, serializes refresh work, and supports visibility/page cleanup.
(() => {
  function createDashboardRefreshController({ refresh, intervalMs = 10_000, setIntervalFn = globalThis.setInterval, clearIntervalFn = globalThis.clearInterval, isHidden = () => false } = {}) {
    if (typeof refresh !== "function") throw new TypeError("A dashboard refresh function is required.");

    let enabled = false;
    let timer = null;
    let refreshInFlight = false;

    function clearTimer() {
      if (timer === null) return;
      clearIntervalFn(timer);
      timer = null;
    }

    function ensureTimer() {
      if (!enabled || timer !== null) return;
      timer = setIntervalFn(() => {
        if (!isHidden()) void refreshNow("auto");
      }, intervalMs);
    }

    async function refreshNow(reason = "manual") {
      if (refreshInFlight) return false;
      refreshInFlight = true;
      try {
        await refresh(reason);
        return true;
      } finally {
        refreshInFlight = false;
      }
    }

    function start() {
      enabled = true;
      ensureTimer();
    }

    function pause() {
      clearTimer();
    }

    function resume() {
      ensureTimer();
    }

    function stop() {
      enabled = false;
      clearTimer();
    }

    return Object.freeze({ start, pause, resume, stop, refreshNow, isRunning: () => timer !== null, isRefreshing: () => refreshInFlight });
  }

  const api = Object.freeze({ createDashboardRefreshController });
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  globalThis.VivoiceAdminDashboardRefresh = api;
})();
