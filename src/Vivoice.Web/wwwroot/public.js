// Purpose: Renders the public POI catalog and starts narration when GPS enters each POI's configured radius.
// Responsibility: Keeps user location in the browser; only public POI content is fetched from the API.
(() => {
  const el = id => document.getElementById(id);
  const ui = {
    language: el("languageSelect"), status: el("publicStatus"), map: el("map"), grid: el("poiGrid"), topics: el("topicGrid"),
    heading: el("narrationHeading"), text: el("narrationText"), category: el("narrationCategory"), notice: el("narrationNotice"),
    speech: el("speechStatus"), pause: el("pauseSpeech"), stop: el("stopSpeech"), replay: el("replaySpeech"), retry: el("retryPublic"), autoplay: el("autoplayToggle"), demoStatus: el("voiceDemoStatus")
  };
  const copy = {
    vi: { name: "Tiếng Việt", location: "Thuyết minh tự động bằng GPS", waiting: "Cho phép vị trí GPS để tự phát khi bạn đi vào bán kính POI.", tracking: "Đang theo dõi GPS. Vòng tròn xanh là độ chính xác vị trí hiện tại.", denied: "Không thể dùng GPS. Hãy cho phép quyền vị trí trong trình duyệt.", inside: "Bạn đã vào bán kính {name}; đang phát thuyết minh.", noPois: "Chưa có POI công khai.", gpsUnsupported: "Trình duyệt này không hỗ trợ GPS.", radius: "Bán kính tự phát: {radius} m", replay: "Đọc lại", pause: "Tạm dừng", stop: "Dừng" },
    en: { name: "English", location: "GPS auto narration", waiting: "Allow GPS location to play narration when you enter a POI radius.", tracking: "Tracking GPS. The blue circle shows current location accuracy.", denied: "GPS is unavailable. Allow location permission in your browser.", inside: "You entered the radius for {name}; narration is starting.", noPois: "No public POIs are available.", gpsUnsupported: "This browser does not support GPS.", radius: "Auto-play radius: {radius} m", replay: "Replay", pause: "Pause", stop: "Stop" },
    ja: { name: "日本語", location: "GPS 自動音声ガイド", waiting: "POI の範囲に入ると音声を再生するため、GPS を許可してください。", tracking: "GPS を追跡中です。青い円は現在位置の精度です。", denied: "GPS を使用できません。ブラウザーで位置情報を許可してください。", inside: "{name} の範囲に入りました。音声ガイドを開始します。", noPois: "公開中の POI はありません。", gpsUnsupported: "このブラウザーは GPS に対応していません。", radius: "自動再生範囲: {radius} m", replay: "もう一度聞く", pause: "一時停止", stop: "停止" },
    ko: { name: "한국어", location: "GPS 자동 음성 안내", waiting: "POI 반경에 들어가면 음성을 재생할 수 있도록 GPS를 허용해 주세요.", tracking: "GPS를 추적하고 있습니다. 파란 원은 현재 위치의 정확도입니다.", denied: "GPS를 사용할 수 없습니다. 브라우저에서 위치 권한을 허용해 주세요.", inside: "{name} 반경 안에 들어왔습니다. 음성 안내를 시작합니다.", noPois: "공개된 POI가 없습니다.", gpsUnsupported: "이 브라우저는 GPS를 지원하지 않습니다.", radius: "자동 재생 반경: {radius}m", replay: "다시 듣기", pause: "일시 정지", stop: "중지" },
    zh: { name: "中文", location: "GPS 自动语音导览", waiting: "请允许 GPS 定位，进入 POI 范围时将自动播放导览。", tracking: "正在追踪 GPS。蓝色圆圈表示当前位置精度。", denied: "无法使用 GPS。请在浏览器中允许位置权限。", inside: "您已进入 {name} 的范围，正在播放导览。", noPois: "暂无公开 POI。", gpsUnsupported: "此浏览器不支持 GPS。", radius: "自动播放半径：{radius} 米", replay: "重新播放", pause: "暂停", stop: "停止" }
  };
  const supportedLanguages = ["vi", "en", "ja", "ko", "zh"];
  const presentationCopy = Object.freeze({
    vi: { languages: { vi: "Tiếng Việt", en: "Tiếng Anh", ja: "Tiếng Nhật", ko: "Tiếng Hàn", zh: "Tiếng Trung" }, voiceKicker: "Nghe thử giọng", voiceTitle: "Nghe thử giọng đọc", voiceDescription: "Chọn một ngôn ngữ để nghe bản demo thuyết minh.", playing: "Đang phát: {language}", voiceError: "Không thể phát giọng đọc trên thiết bị này." },
    en: { languages: { vi: "Vietnamese", en: "English", ja: "Japanese", ko: "Korean", zh: "Chinese" }, voiceKicker: "Voice preview", voiceTitle: "Narration voice preview", voiceDescription: "Choose a language to hear a narration sample.", playing: "Playing: {language}", voiceError: "This device cannot play the selected voice." },
    ja: { languages: { vi: "ベトナム語", en: "英語", ja: "日本語", ko: "韓国語", zh: "中国語" }, voiceKicker: "音声プレビュー", voiceTitle: "音声ガイドを試聴", voiceDescription: "言語を選択して音声ガイドのサンプルを聞くことができます。", playing: "再生中: {language}", voiceError: "この端末では選択した音声を再生できません。" },
    ko: { languages: { vi: "베트남어", en: "영어", ja: "일본어", ko: "한국어", zh: "중국어" }, voiceKicker: "음성 미리 듣기", voiceTitle: "음성 안내 들어보기", voiceDescription: "언어를 선택하여 음성 안내 샘플을 들어 보세요.", playing: "재생 중: {language}", voiceError: "이 기기에서는 선택한 음성을 재생할 수 없습니다." },
    zh: { languages: { vi: "越南语", en: "英语", ja: "日语", ko: "韩语", zh: "中文" }, voiceKicker: "语音试听", voiceTitle: "试听语音导览", voiceDescription: "选择一种语言，试听语音导览样本。", playing: "正在播放：{language}", voiceError: "此设备无法播放所选语音。" }
  });
  const demoSamples = Object.freeze({
    vi: "Xin chào. Đây là bản nghe thử giọng thuyết minh VIVOICE bằng tiếng Việt.",
    en: "Hello. This is a VIVOICE narration voice sample in English.",
    ja: "こんにちは。これは日本語のVIVOICE音声ガイドのサンプルです。",
    ko: "안녕하세요. 이것은 한국어 VIVOICE 음성 안내 샘플입니다.",
    zh: "您好。这是中文 VIVOICE 语音导览试听。"
  });
  let language = supportedLanguages.includes(localStorage.getItem("vivoice_language")) ? localStorage.getItem("vivoice_language") : "vi";
  let pois = [], topics = [], selectedPoiId = null, selectedTopicId = null, narration = null;
  let map, tileLayer, markers = new Map(), poiRadiusCircles = new Map(), userMarker, userAccuracyCircle, watchId = null, activeUtterance = null;
  const enteredPoiIds = new Set();

  const t = key => copy[language][key] || copy.vi[key] || key;
  const present = key => presentationCopy[language]?.[key] ?? presentationCopy.vi[key];
  const setStatus = (message, kind = "") => { ui.status.className = `status-line ${kind}`; ui.status.textContent = message; };
  const api = async path => { const response = await fetch(path, { credentials: "same-origin", headers: { Accept: "application/json" } }); if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message || `HTTP ${response.status}`); } return response.status === 204 ? null : response.json(); };
  const validCoordinate = poi => Number.isFinite(Number(poi.latitude)) && Number.isFinite(Number(poi.longitude));
  const distanceMeters = (a, b, c, d) => { const r = 6371000, toRad = value => value * Math.PI / 180; const x = toRad(c - a), y = toRad(d - b); const h = Math.sin(x / 2) ** 2 + Math.cos(toRad(a)) * Math.cos(toRad(c)) * Math.sin(y / 2) ** 2; return 2 * r * Math.asin(Math.sqrt(h)); };

  function initializeMap() {
    if (!window.L || map) return;
    map = L.map(ui.map, { scrollWheelZoom: true, zoomControl: true, preferCanvas: true, zoomAnimation: true, fadeAnimation: true }).setView([10.7769, 106.7009], 13);
    installTiles();
    window.setTimeout(() => map.invalidateSize({ pan: false, animate: false }), 120);
  }
  function installTiles() {
    tileLayer?.remove();
    // Standard OpenStreetMap tiles require no vendor key. CARTO returns a key-required image here.
    tileLayer = L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { maxZoom: 19, minZoom: 3, subdomains: "abc", attribution: "© OpenStreetMap contributors", keepBuffer: 8, updateWhenIdle: true });
    tileLayer.addTo(map);
  }
  function renderLanguage() {
    ui.language.replaceChildren();
    supportedLanguages.forEach(code => { const option = document.createElement("option"); option.value = code; option.textContent = present("languages")[code]; ui.language.append(option); });
    ui.language.value = language; document.documentElement.lang = language;
    ui.pause.textContent = t("pause"); ui.stop.textContent = t("stop"); ui.replay.textContent = t("replay");
    el("voiceDemoKicker").textContent = present("voiceKicker"); el("voiceDemoTitle").textContent = present("voiceTitle"); el("voiceDemoDescription").textContent = present("voiceDescription");
    document.querySelectorAll("[data-demo-language]").forEach(button => { button.textContent = present("languages")[button.dataset.demoLanguage]; });
  }
  function renderPois() {
    ui.grid.replaceChildren(); markers.forEach(marker => marker.remove()); poiRadiusCircles.forEach(circle => circle.remove()); markers = new Map(); poiRadiusCircles = new Map();
    pois.forEach((poi, index) => {
      const card = document.createElement("button"); card.type = "button"; card.className = "poi-card"; card.setAttribute("aria-pressed", String(poi.id === selectedPoiId));
      const body = document.createElement("span"); body.className = "poi-card-body";
      const number = document.createElement("span"); number.className = "number"; number.textContent = String(index + 1).padStart(2, "0");
      const title = document.createElement("h3"); title.textContent = poi.name;
      const summary = document.createElement("p"); summary.textContent = poi.summary;
      const radius = document.createElement("small"); radius.textContent = t("radius").replace("{radius}", poi.triggerRadiusMeters);
      body.append(number, title, summary, radius); card.append(body); card.addEventListener("click", () => selectPoi(poi.id, true)); ui.grid.append(card);
      if (map && validCoordinate(poi)) {
        const point = [poi.latitude, poi.longitude];
        const circle = L.circle(point, { radius: Number(poi.triggerRadiusMeters) || 100, color: "#2563eb", weight: 2, opacity: .85, fillColor: "#60a5fa", fillOpacity: .14, interactive: false }).addTo(map);
        const marker = L.marker(point).addTo(map).bindTooltip(`${poi.name} (${poi.triggerRadiusMeters}m)`); marker.on("click", () => selectPoi(poi.id, true)); markers.set(poi.id, marker); poiRadiusCircles.set(poi.id, circle);
      }
    });
  }
  function renderTopics() {
    ui.topics.replaceChildren();
    topics.forEach(topic => { const button = document.createElement("button"); button.type = "button"; button.className = "topic-card"; button.setAttribute("aria-pressed", String(topic.id === selectedTopicId)); button.textContent = topic.title; button.addEventListener("click", () => selectTopic(topic.id, true)); ui.topics.append(button); });
  }
  async function loadPois() {
    setStatus("Loading...");
    try {
      pois = await api(`/api/public/pois?lang=${encodeURIComponent(language)}`);
      if (!pois.length) { setStatus(t("noPois"), "error"); renderPois(); return; }
      selectedPoiId = pois[0].id; renderPois(); await selectPoi(selectedPoiId, false); startLocationTracking(); setStatus("");
    } catch (error) { setStatus(error.message, "error"); }
  }
  async function selectPoi(id, focus) {
    const poi = pois.find(item => item.id === id); if (!poi) return;
    selectedPoiId = id; selectedTopicId = null; stopSpeech(); renderPois();
    if (focus && map && validCoordinate(poi)) map.setView([poi.latitude, poi.longitude], 16, { animate: true });
    try { topics = await api(`/api/public/pois/${id}/topics?lang=${encodeURIComponent(language)}`); renderTopics(); } catch (error) { setStatus(error.message, "error"); }
  }
  async function selectTopic(id, speak) {
    selectedTopicId = id; renderTopics();
    try {
      narration = await api(`/api/public/topics/${id}/narration?lang=${encodeURIComponent(language)}`);
      ui.category.textContent = narration.category; ui.heading.textContent = narration.title; ui.text.textContent = narration.content; ui.replay.classList.remove("hidden");
      if (speak) speakNarration();
    } catch (error) { ui.text.textContent = error.message; }
  }
  function stopSpeech() { window.speechSynthesis?.cancel(); activeUtterance = null; ui.pause.classList.add("hidden"); ui.stop.classList.add("hidden"); }
  function applyVoice(utterance, languageCode) {
    const tts = window.VivoiceTtsLanguage;
    utterance.lang = tts?.resolveSpeechLocale(languageCode) || "vi-VN";
    const voice = tts?.chooseVoice(window.speechSynthesis?.getVoices?.() || [], utterance.lang);
    if (voice) utterance.voice = voice;
  }
  function speakNarration() {
    if (!narration?.content || !("speechSynthesis" in window)) return;
    stopSpeech(); const utterance = new SpeechSynthesisUtterance(narration.content); applyVoice(utterance, language);
    utterance.onstart = () => { ui.speech.textContent = ""; ui.pause.classList.remove("hidden"); ui.stop.classList.remove("hidden"); };
    utterance.onend = () => { activeUtterance = null; ui.pause.classList.add("hidden"); ui.stop.classList.add("hidden"); };
    activeUtterance = utterance; window.speechSynthesis.speak(utterance);
  }
  function speakDemo(languageCode) {
    if (!("speechSynthesis" in window) || !demoSamples[languageCode]) { if (ui.demoStatus) ui.demoStatus.textContent = t("gpsUnsupported"); return; }
    stopSpeech(); const utterance = new SpeechSynthesisUtterance(demoSamples[languageCode]); applyVoice(utterance, languageCode);
    utterance.onstart = () => { if (ui.demoStatus) ui.demoStatus.textContent = present("playing").replace("{language}", present("languages")[languageCode]); };
    utterance.onend = () => { if (ui.demoStatus) ui.demoStatus.textContent = ""; };
    utterance.onerror = () => { if (ui.demoStatus) ui.demoStatus.textContent = present("voiceError"); };
    window.speechSynthesis.speak(utterance);
  }
  function onLocation(position) {
    const { latitude, longitude, accuracy } = position.coords; const point = [latitude, longitude];
    if (map) { if (!userMarker) userMarker = L.marker(point, { title: "Your location" }).addTo(map); else userMarker.setLatLng(point); if (!userAccuracyCircle) userAccuracyCircle = L.circle(point, { radius: accuracy, color: "#1d4ed8", fillColor: "#60a5fa", fillOpacity: .16, weight: 2 }).addTo(map); else userAccuracyCircle.setLatLng(point).setRadius(accuracy); }
    setStatus(t("tracking"), "success");
    const nearby = pois.filter(poi => validCoordinate(poi) && Number(poi.triggerRadiusMeters) > 0 && distanceMeters(latitude, longitude, Number(poi.latitude), Number(poi.longitude)) <= Number(poi.triggerRadiusMeters)).sort((a, b) => Number(a.triggerRadiusMeters) - Number(b.triggerRadiusMeters))[0];
    if (nearby && !enteredPoiIds.has(nearby.id)) { enteredPoiIds.add(nearby.id); setStatus(t("inside").replace("{name}", nearby.name), "success"); selectPoi(nearby.id, true).then(() => { if (topics[0]) selectTopic(topics[0].id, true); }); }
  }
  function startLocationTracking() {
    if (!navigator.geolocation) { setStatus(t("gpsUnsupported"), "error"); return; }
    if (watchId !== null) return;
    setStatus(t("waiting"));
    watchId = navigator.geolocation.watchPosition(onLocation, () => setStatus(t("denied"), "error"), { enableHighAccuracy: true, maximumAge: 10000, timeout: 20000 });
  }
  ui.language.addEventListener("change", async () => { language = ui.language.value; localStorage.setItem("vivoice_language", language); renderLanguage(); await loadPois(); });
  ui.pause.addEventListener("click", () => { if (!window.speechSynthesis) return; window.speechSynthesis.paused ? window.speechSynthesis.resume() : window.speechSynthesis.pause(); });
  ui.stop.addEventListener("click", stopSpeech); ui.replay.addEventListener("click", speakNarration);
  document.querySelectorAll("[data-demo-language]").forEach(button => button.addEventListener("click", () => speakDemo(button.dataset.demoLanguage)));
  window.addEventListener("pagehide", () => { if (watchId !== null) navigator.geolocation?.clearWatch(watchId); stopSpeech(); }, { once: true });
  el("autoplayToggle")?.closest(".autoplay-control")?.remove();
  renderLanguage(); initializeMap(); void loadPois();
})();
