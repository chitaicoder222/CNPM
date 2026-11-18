// Purpose: Makes public-map tile availability deterministic without coupling UI state to a specific network provider.
// Responsibility: Tracks tile success, provider failover and local-fallback state for public.js and browser-independent tests.
(() => {
  function createMapTileState(providerCount, errorThreshold = 3) {
    if (!Number.isInteger(providerCount) || providerCount < 1) throw new TypeError("providerCount must be a positive integer.");
    if (!Number.isInteger(errorThreshold) || errorThreshold < 1) throw new TypeError("errorThreshold must be a positive integer.");

    let providerIndex = 0;
    let successfulTiles = 0;
    let failedTiles = 0;
    let mode = "loading";

    function snapshot() {
      return Object.freeze({ providerIndex, successfulTiles, failedTiles, mode });
    }

    function beginProvider(nextProviderIndex) {
      if (!Number.isInteger(nextProviderIndex) || nextProviderIndex < 0 || nextProviderIndex >= providerCount) {
        throw new RangeError("Provider index is outside the configured range.");
      }

      providerIndex = nextProviderIndex;
      successfulTiles = 0;
      failedTiles = 0;
      mode = "loading";
      return snapshot();
    }

    function recordTileLoad() {
      successfulTiles += 1;
      mode = "online";
      return Object.freeze({ action: "online", state: snapshot() });
    }

    function recordTileError() {
      failedTiles += 1;

      if (successfulTiles > 0) {
        mode = "partial";
        return Object.freeze({ action: "partial", state: snapshot() });
      }

      if (failedTiles < errorThreshold) {
        return Object.freeze({ action: "waiting", state: snapshot() });
      }

      if (providerIndex < providerCount - 1) {
        mode = "switching";
        return Object.freeze({ action: "switch", providerIndex: providerIndex + 1, state: snapshot() });
      }

      mode = "offline";
      return Object.freeze({ action: "offline", state: snapshot() });
    }

    function retry() {
      return Object.freeze({ action: "retry", providerIndex: 0, state: beginProvider(0) });
    }

    return Object.freeze({ beginProvider, recordTileLoad, recordTileError, retry, snapshot });
  }

  window.VivoiceMapTileState = Object.freeze({ createMapTileState });
})();
