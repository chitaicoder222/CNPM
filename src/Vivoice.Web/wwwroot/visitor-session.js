// Purpose: Safely issues one server-owned visitor cookie for public telemetry without permanently caching a transient failure.
// Responsibility: Shares an in-flight issue request, caches success, and permits a later heartbeat or ad event to retry after failure.
(() => {
  function createSessionIssuer(issueSession) {
    if (typeof issueSession !== "function") throw new TypeError("issueSession must be a function.");

    let sessionIssued = false;
    let inFlight;

    return function ensureSession() {
      if (sessionIssued) return Promise.resolve(true);
      if (inFlight) return inFlight;

      inFlight = Promise.resolve()
        .then(issueSession)
        .then(() => {
          sessionIssued = true;
          return true;
        })
        .catch(() => false)
        .then(result => {
          if (!result) inFlight = undefined;
          return result;
        });
      return inFlight;
    };
  }

  const api = Object.freeze({ createSessionIssuer });
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  globalThis.VivoiceVisitorSession = api;
})();
