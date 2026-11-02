// Purpose: Detects POI geofence entries and coordinates one GPS narration at a time.
// Responsibility: Keeps coordinates in browser memory, rejects unreliable fixes and cancels work on exit or stop.
(() => {
  const radians = degrees => degrees * Math.PI / 180;
  function validCoordinates(value) {
    return value && Number.isFinite(value.latitude) && Math.abs(value.latitude) <= 90
      && Number.isFinite(value.longitude) && Math.abs(value.longitude) <= 180;
  }
  function distanceMeters(a, b) {
    if (!validCoordinates(a) || !validCoordinates(b)) return Infinity;
    const latitude = radians(b.latitude - a.latitude);
    const longitude = radians(b.longitude - a.longitude);
    const haversine = Math.sin(latitude / 2) ** 2
      + Math.cos(radians(a.latitude)) * Math.cos(radians(b.latitude)) * Math.sin(longitude / 2) ** 2;
    return 6371000 * 2 * Math.asin(Math.sqrt(Math.min(1, Math.max(0, haversine))));
  }
  function validPoi(poi) {
    return poi && validCoordinates(poi) && Number.isFinite(poi.radiusMeters) && poi.radiusMeters > 0;
  }
  function reliableFor(position, poi) {
    return validCoordinates(position) && Number.isFinite(position.accuracy)
      && position.accuracy >= 0 && position.accuracy <= poi.radiusMeters;
  }
  function createTracker() {
    const inside = new Set();
    return {
      reset() { inside.clear(); },
      update(position, pois) {
        const entered = [];
        const exited = [];
        const activePois = pois.filter(validPoi);
        const activeIds = new Set(activePois.map(poi => poi.id));
        for (const id of inside) {
          if (!activeIds.has(id)) { inside.delete(id); exited.push(id); }
        }
        for (const poi of activePois) {
          if (!reliableFor(position, poi)) continue;
          const distance = distanceMeters(position, poi);
          if (inside.has(poi.id)) {
            if (distance > poi.radiusMeters + Math.max(10, poi.radiusMeters * 0.1)) {
              inside.delete(poi.id); exited.push(poi.id);
            }
          } else if (distance <= poi.radiusMeters) {
            inside.add(poi.id); entered.push({ poi, distance });
          }
        }
        entered.sort((a, b) => a.distance - b.distance || (a.poi.displayOrder || 0) - (b.poi.displayOrder || 0) || a.poi.id - b.poi.id);
        return { entered: entered.map(item => item.poi), exited };
      }
    };
  }
  function createGuide({ geolocation, isSecureContext, play, cancel, onStatus = () => {}, onPosition = () => {}, now = Date.now }) {
    const tracker = createTracker();
    let pois = [];
    let latestPosition = null;
    let watchId = null;
    let enabled = false;
    let generation = 0;
    let queue = [];
    let current = null;

    function isWithinRange(poiId) {
      const poi = pois.find(item => item.id === poiId);
      return enabled && poi && latestPosition && now() - latestPosition.timestamp <= 30000
        && reliableFor(latestPosition, poi) && distanceMeters(latestPosition, poi) <= poi.radiusMeters;
    }
    function interrupt() {
      queue = [];
      if (current) {
        const previous = current;
        current = null;
        previous.controller.abort();
      }
      cancel();
    }
    function pump() {
      if (!enabled || current) return;
      let poi;
      while (queue.length && !poi) {
        const candidate = queue.shift();
        if (isWithinRange(candidate.id)) poi = pois.find(item => item.id === candidate.id);
      }
      if (!poi) return;
      const run = { poi, controller: new AbortController() };
      current = run;
      onStatus("nearby", poi);
      Promise.resolve().then(() => {
        if (!run.controller.signal.aborted) return play(poi, run.controller.signal);
      }).catch(error => {
        if (!run.controller.signal.aborted && error?.name !== "AbortError") onStatus("contentError", poi);
      }).finally(() => {
        if (current !== run) return;
        current = null;
        pump();
      });
    }
    function evaluate() {
      if (!enabled || !latestPosition || now() - latestPosition.timestamp > 30000) return;
      const { entered, exited } = tracker.update(latestPosition, pois);
      queue = queue.filter(poi => !exited.includes(poi.id));
      if (current && exited.includes(current.poi.id)) {
        const previous = current;
        current = null;
        previous.controller.abort();
        cancel();
      }
      queue.push(...entered);
      if (!current) onStatus(pois.some(poi => validPoi(poi) && reliableFor(latestPosition, poi)) || !pois.length ? "watching" : "inaccurate");
      pump();
    }
    function stop(status = "off") {
      enabled = false;
      generation += 1;
      if (watchId !== null) geolocation.clearWatch(watchId);
      watchId = null;
      interrupt();
      tracker.reset();
      latestPosition = null;
      onPosition(null);
      onStatus(status);
    }
    return {
      isActive: () => enabled,
      isWithinRange,
      interrupt,
      setPois(value) { pois = Array.isArray(value) ? value : []; evaluate(); },
      refresh() { interrupt(); tracker.reset(); evaluate(); },
      stop,
      start() {
        if (enabled) return;
        if (!isSecureContext) { onStatus("insecure"); return; }
        if (!geolocation || typeof geolocation.watchPosition !== "function") { onStatus("unsupported"); return; }
        enabled = true;
        const session = ++generation;
        onStatus("locating");
        try {
          watchId = geolocation.watchPosition(position => {
            if (!enabled || session !== generation) return;
            const timestamp = position.timestamp;
            const coords = position.coords;
            if (!validCoordinates(coords) || !Number.isFinite(coords.accuracy) || coords.accuracy < 0
              || !Number.isFinite(timestamp) || now() - timestamp > 30000 || timestamp > now() + 5000
              || (latestPosition && timestamp < latestPosition.timestamp)) return;
            latestPosition = { latitude: coords.latitude, longitude: coords.longitude, accuracy: coords.accuracy, timestamp };
            onPosition(latestPosition);
            evaluate();
          }, error => {
            if (enabled && session === generation) stop(error.code === 1 ? "denied" : error.code === 3 ? "timeout" : "unavailable");
          }, { enableHighAccuracy: true, maximumAge: 5000, timeout: 15000 });
        } catch { stop("unavailable"); }
      }
    };
  }
  const api = Object.freeze({ distanceMeters, createTracker, createGuide });
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  globalThis.VivoiceGpsGuide = api;
})();
