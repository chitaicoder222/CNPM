// Purpose: Drives the public VIVOICE map, multilingual POI exploration, truthful browser TTS and anonymous heartbeat.
// Responsibility: Synchronizes API state safely, keeps narration language separate from UI language and never hardcodes POI content.
(() => {
  const locales = {
    vi: { name: "Tiếng Việt", eyebrow: "Hành trình Việt Nam đa ngôn ngữ", heroTitle: "Chạm vào một địa điểm. Lắng nghe một câu chuyện.", heroCopy: "Khám phá văn hóa, hương vị và những hành trình đặc sắc của Việt Nam theo ngôn ngữ của bạn.", heroCard: "Bản đồ thật. Nội dung thật. Giọng đọc được thử phát tự động.", mapKicker: "Khám phá", mapTitle: "Sáu điểm đến nổi bật", narrationKicker: "Thuyết minh tự động", narrationTitle: "Chọn một chủ đề để bắt đầu", topics: "Chủ đề", narrationEmptyTitle: "Nội dung sẽ xuất hiện tại đây", narrationEmptyText: "Chọn một POI, sau đó chọn chủ đề bạn muốn khám phá.", narrationLanguage: "Ngôn ngữ nội dung", pause: "Tạm dừng", stop: "Dừng", replay: "Đọc lại", retry: "Thử lại", footer: "Khám phá có trách nhiệm, lắng nghe bằng ngôn ngữ của bạn.", loading: "Đang tải dữ liệu điểm đến…", apiUnavailable: "Không thể kết nối dữ liệu VIVOICE. Hãy kiểm tra kết nối rồi thử lại.", mapUnavailable: "Không thể tải bản đồ. Hãy kiểm tra kết nối rồi thử lại.", noPois: "Hiện chưa có địa điểm công khai.", noTopics: "Địa điểm này chưa có chủ đề công khai.", fallback: "Nội dung đang hiển thị bản tiếng Việt.", speechPreparing: "Đang chuẩn bị giọng đọc…", speaking: "Đang đọc thuyết minh…", speechPaused: "Đã tạm dừng giọng đọc.", speechResumed: "Đang tiếp tục giọng đọc…", speechStopped: "Đã dừng giọng đọc.", speechEmpty: "Nội dung thuyết minh trống nên không thể đọc.", speechUnavailable: "Trình duyệt chưa thể phát giọng đọc; bạn vẫn có thể đọc nội dung.", speechEnded: "Đã đọc xong.", categories: { culture: "Văn hóa đặc sắc", food: "Ẩm thực nổi tiếng", tourism: "Điểm du lịch nổi bật" } },
    ko: { name: "한국어", eyebrow: "다국어 베트남 여행", heroTitle: "장소를 선택하고, 이야기를 들어보세요.", heroCopy: "원하는 언어로 베트남의 문화와 맛, 여행지를 만나보세요.", heroCard: "실제 지도와 데이터로 안내를 시작합니다.", mapKicker: "탐색", mapTitle: "주요 여행지", narrationKicker: "자동 안내", narrationTitle: "주제를 선택하세요", topics: "주제", narrationEmptyTitle: "여기에 안내 내용이 표시됩니다", narrationEmptyText: "여행지와 주제를 선택하세요.", narrationLanguage: "콘텐츠 언어", pause: "일시 정지", stop: "중지", replay: "다시 듣기", retry: "다시 시도", footer: "책임 있는 여행, 당신의 언어로 듣기.", loading: "여행지 정보를 불러오는 중…", apiUnavailable: "VIVOICE 데이터에 연결할 수 없습니다. 연결을 확인한 뒤 다시 시도하세요.", mapUnavailable: "지도를 불러올 수 없습니다. 연결을 확인한 뒤 다시 시도하세요.", noPois: "현재 공개된 여행지가 없습니다.", noTopics: "공개된 주제가 없습니다.", fallback: "베트남어 원문이 표시됩니다.", speechPreparing: "음성 읽기를 준비하는 중…", speaking: "안내를 읽는 중…", speechPaused: "음성 읽기가 일시 정지되었습니다.", speechResumed: "음성 읽기를 계속합니다…", speechStopped: "음성 읽기를 중지했습니다.", speechEmpty: "읽을 안내 내용이 없습니다.", speechUnavailable: "브라우저에서 음성을 재생하지 못했습니다. 텍스트는 계속 읽을 수 있습니다.", speechEnded: "읽기가 끝났습니다.", categories: { culture: "특색 있는 문화", food: "유명한 음식", tourism: "유명한 관광지" } },
    ja: { name: "日本語", eyebrow: "多言語のベトナム旅", heroTitle: "場所を選び、物語を聴く。", heroCopy: "お好みの言語でベトナムの文化、味、旅先を発見しましょう。", heroCard: "地図とデータから案内を始めます。", mapKicker: "探索", mapTitle: "注目の目的地", narrationKicker: "自動ガイド", narrationTitle: "テーマを選択", topics: "テーマ", narrationEmptyTitle: "ここにガイドが表示されます", narrationEmptyText: "目的地とテーマを選択してください。", narrationLanguage: "コンテンツの言語", pause: "一時停止", stop: "停止", replay: "もう一度聞く", retry: "再試行", footer: "責任ある旅を、あなたの言葉で。", loading: "目的地を読み込み中…", apiUnavailable: "VIVOICE データに接続できません。接続を確認して再試行してください。", mapUnavailable: "地図を読み込めません。接続を確認して再試行してください。", noPois: "公開中の目的地はありません。", noTopics: "公開中のテーマはありません。", fallback: "ベトナム語の原文を表示しています。", speechPreparing: "音声読み上げを準備しています…", speaking: "ガイドを読み上げています…", speechPaused: "読み上げを一時停止しました。", speechResumed: "読み上げを再開しています…", speechStopped: "読み上げを停止しました。", speechEmpty: "読み上げる内容がありません。", speechUnavailable: "ブラウザで音声を再生できません。テキストは読めます。", speechEnded: "読み上げが終わりました。", categories: { culture: "特色ある文化", food: "有名な名物", tourism: "有名な観光地" } },
    zh: { name: "中文", eyebrow: "多语言越南之旅", heroTitle: "选择地点，聆听故事。", heroCopy: "用您的语言探索越南的文化、美食和旅行目的地。", heroCard: "真实地图与真实数据，自动尝试语音讲解。", mapKicker: "探索", mapTitle: "热门目的地", narrationKicker: "自动讲解", narrationTitle: "请选择主题", topics: "主题", narrationEmptyTitle: "讲解内容将显示在这里", narrationEmptyText: "请选择目的地和主题。", narrationLanguage: "内容语言", pause: "暂停", stop: "停止", replay: "重新朗读", retry: "重试", footer: "负责任地旅行，用您的语言聆听。", loading: "正在加载目的地…", apiUnavailable: "无法连接到 VIVOICE 数据。请检查连接后重试。", mapUnavailable: "无法加载地图。请检查连接后重试。", noPois: "目前没有公开地点。", noTopics: "此地点没有公开主题。", fallback: "正在显示越南语原文。", speechPreparing: "正在准备语音朗读…", speaking: "正在朗读讲解…", speechPaused: "语音朗读已暂停。", speechResumed: "正在继续语音朗读…", speechStopped: "语音朗读已停止。", speechEmpty: "没有可朗读的讲解内容。", speechUnavailable: "浏览器无法播放语音；您仍可阅读文字。", speechEnded: "朗读完成。", categories: { culture: "特色文化", food: "著名美食", tourism: "著名旅游地" } },
    th: { name: "ไทย", eyebrow: "การเดินทางเวียดนามหลายภาษา", heroTitle: "เลือกสถานที่ แล้วฟังเรื่องราว", heroCopy: "ค้นพบวัฒนธรรม รสชาติ และสถานที่ของเวียดนามในภาษาของคุณ", heroCard: "แผนที่และข้อมูลจริง พร้อมลองอ่านออกเสียงอัตโนมัติ", mapKicker: "สำรวจ", mapTitle: "จุดหมายเด่น", narrationKicker: "คำบรรยายอัตโนมัติ", narrationTitle: "เลือกหัวข้อ", topics: "หัวข้อ", narrationEmptyTitle: "เนื้อหาจะแสดงที่นี่", narrationEmptyText: "เลือกสถานที่และหัวข้อที่ต้องการ", narrationLanguage: "ภาษาของเนื้อหา", pause: "หยุดชั่วคราว", stop: "หยุด", replay: "อ่านอีกครั้ง", retry: "ลองอีกครั้ง", footer: "ท่องเที่ยวอย่างรับผิดชอบ ฟังในภาษาของคุณ", loading: "กำลังโหลดจุดหมาย…", apiUnavailable: "ไม่สามารถเชื่อมต่อข้อมูล VIVOICE ได้ โปรดตรวจสอบการเชื่อมต่อแล้วลองอีกครั้ง", mapUnavailable: "ไม่สามารถโหลดแผนที่ได้ โปรดตรวจสอบการเชื่อมต่อแล้วลองอีกครั้ง", noPois: "ยังไม่มีสถานที่สาธารณะ", noTopics: "ไม่มีหัวข้อสาธารณะ", fallback: "กำลังแสดงเนื้อหาภาษาเวียดนาม", speechPreparing: "กำลังเตรียมการอ่านออกเสียง…", speaking: "กำลังอ่านคำบรรยาย…", speechPaused: "หยุดการอ่านออกเสียงชั่วคราวแล้ว", speechResumed: "กำลังอ่านออกเสียงต่อ…", speechStopped: "หยุดการอ่านออกเสียงแล้ว", speechEmpty: "ไม่มีเนื้อหาสำหรับอ่านออกเสียง", speechUnavailable: "เบราว์เซอร์เล่นเสียงไม่ได้ แต่ยังอ่านข้อความได้", speechEnded: "อ่านจบแล้ว", categories: { culture: "วัฒนธรรมเด่น", food: "อาหารขึ้นชื่อ", tourism: "สถานที่ท่องเที่ยวเด่น" } }
  };
  const ariaLabels = {
    vi: { language: "Chọn ngôn ngữ", map: "Bản đồ các điểm thuyết minh VIVOICE", leftAds: "Quảng cáo bên trái", rightAds: "Quảng cáo bên phải" },
    ko: { language: "언어 선택", map: "VIVOICE 안내 장소 지도", leftAds: "왼쪽 광고", rightAds: "오른쪽 광고" },
    ja: { language: "言語を選択", map: "VIVOICE 案内スポットの地図", leftAds: "左側の広告", rightAds: "右側の広告" },
    zh: { language: "选择语言", map: "VIVOICE 讲解地点地图", leftAds: "左侧广告", rightAds: "右侧广告" },
    th: { language: "เลือกภาษา", map: "แผนที่จุดบรรยาย VIVOICE", leftAds: "โฆษณาด้านซ้าย", rightAds: "โฆษณาด้านขวา" }
  };
  const sponsoredLabels = { vi: "Quảng cáo / Tài trợ", ko: "광고 / 후원", ja: "広告 / 協賛", zh: "广告 / 赞助", th: "โฆษณา / ผู้สนับสนุน" };

  const elements = {
    language: document.getElementById("languageSelect"), status: document.getElementById("publicStatus"), map: document.getElementById("map"), poiGrid: document.getElementById("poiGrid"), topicGrid: document.getElementById("topicGrid"), narrationPanel: document.getElementById("narrationPanel"), narrationCategory: document.getElementById("narrationCategory"), narrationLanguage: document.getElementById("narrationLanguage"), narrationHeading: document.getElementById("narrationHeading"), narrationText: document.getElementById("narrationText"), narrationNotice: document.getElementById("narrationNotice"), speechStatus: document.getElementById("speechStatus"), pause: document.getElementById("pauseSpeech"), stop: document.getElementById("stopSpeech"), replay: document.getElementById("replaySpeech"), retry: document.getElementById("retryPublic"), leftAds: document.getElementById("leftAds"), rightAds: document.getElementById("rightAds")
  };
  const requestControllers = new Map();
  const ttsLanguage = window.VivoiceTtsLanguage;
  let currentLanguage = normalizeUiLanguage(localStorage.getItem("vivoice_language"));
  let selectedPoiId = null;
  let selectedTopicId = null;
  let selectedNarration = null;
  let pois = [];
  let topics = [];
  let map;
  let markers = new Map();
  const createSessionIssuer = window.VivoiceVisitorSession?.createSessionIssuer;
  const issueVisitorSession = typeof createSessionIssuer === "function"
    ? createSessionIssuer(() => api("/api/public/sessions", { method: "POST" }))
    : () => api("/api/public/sessions", { method: "POST" }).then(() => true).catch(() => false);
  let retryAction = null;
  let availableVoices = [];
  let speechToken = 0;
  let activeSpeech = null;
  let speechStartTimer = null;

  function normalizeUiLanguage(languageCode) {
    const normalized = typeof languageCode === "string" ? languageCode.trim().replaceAll("_", "-").toLowerCase().split("-")[0] : "";
    return locales[normalized] ? normalized : "vi";
  }
  function text(key) { return locales[currentLanguage][key] ?? locales.vi[key] ?? key; }
  function languageName(languageCode) { return locales[normalizeUiLanguage(languageCode)].name; }
  function setStatus(message, state = "") { elements.status.className = `status-line ${state}`; elements.status.textContent = message; }
  function setBusy(element, busy) { element.setAttribute("aria-busy", String(busy)); }
  function isAbort(error) { return error?.name === "AbortError"; }
  function publicError(error) { return error instanceof TypeError || error?.message === "Failed to fetch" ? text("apiUnavailable") : error?.message || text("apiUnavailable"); }
  async function api(path, options = {}, signal) {
    const response = await fetch(path, { credentials: "same-origin", headers: { Accept: "application/json", ...(options.headers || {}) }, signal, ...options });
    if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message || `HTTP ${response.status}`); }
    return response.status === 204 ? null : response.json();
  }
  function beginRequest(key) {
    requestControllers.get(key)?.abort();
    const controller = new AbortController();
    requestControllers.set(key, controller);
    return controller;
  }
  function isCurrentRequest(key, controller) { return requestControllers.get(key) === controller; }
  function setRetry(action) { retryAction = action; elements.retry.classList.remove("hidden"); }
  function clearRetry() { retryAction = null; elements.retry.classList.add("hidden"); }
  function setSpeechStatus(key, state = "") {
    elements.speechStatus.className = `status-line ${state}`;
    if (state) elements.speechStatus.dataset.state = state; else delete elements.speechStatus.dataset.state;
    elements.speechStatus.textContent = key ? text(key) : "";
  }
  function setSpeechControls({ canPause = false, canStop = false, canReplay = Boolean(selectedNarration) } = {}) {
    elements.pause.classList.toggle("hidden", !canPause);
    elements.stop.classList.toggle("hidden", !canStop);
    elements.replay.classList.toggle("hidden", !canReplay);
  }
  function setNarrationLanguage(languageCode) {
    elements.narrationLanguage.textContent = `${text("narrationLanguage")}: ${languageName(languageCode)}`;
    elements.narrationLanguage.classList.remove("hidden");
  }
  function clearNarration() {
    selectedNarration = null;
    elements.narrationCategory.textContent = "";
    elements.narrationLanguage.textContent = "";
    elements.narrationLanguage.classList.add("hidden");
    elements.narrationHeading.textContent = text("narrationEmptyTitle");
    elements.narrationText.textContent = text("narrationEmptyText");
    elements.narrationNotice.textContent = "";
    setSpeechControls({ canReplay: false });
    setSpeechStatus();
  }

  function renderStaticLanguage() {
    document.documentElement.lang = currentLanguage;
    document.querySelectorAll("[data-i18n]").forEach(element => { element.textContent = text(element.dataset.i18n); });
    elements.language.value = currentLanguage;
    elements.language.setAttribute("aria-label", ariaLabels[currentLanguage].language);
    elements.map.setAttribute("aria-label", ariaLabels[currentLanguage].map);
    elements.leftAds.setAttribute("aria-label", ariaLabels[currentLanguage].leftAds);
    elements.rightAds.setAttribute("aria-label", ariaLabels[currentLanguage].rightAds);
  }
  function createMap() {
    if (!window.L) {
      setStatus(text("mapUnavailable"), "error");
      setRetry(() => window.location.reload());
      return false;
    }
    if (map) return true;
    map = L.map("map", { scrollWheelZoom: false }).setView([16.3, 107.4], 5.5);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { maxZoom: 18, attribution: "© OpenStreetMap contributors" })
      .on("tileerror", () => { setStatus(text("mapUnavailable"), "error"); setRetry(() => window.location.reload()); })
      .addTo(map);
    return true;
  }
  function renderPois() {
    elements.poiGrid.replaceChildren();
    markers.forEach(marker => marker.remove());
    markers = new Map();
    pois.forEach((poi, index) => {
      const card = document.createElement("button"); card.type = "button"; card.className = "poi-card"; card.setAttribute("aria-pressed", String(poi.id === selectedPoiId));
      const number = document.createElement("span"); number.className = "number"; number.textContent = String(index + 1).padStart(2, "0");
      const heading = document.createElement("h3"); heading.textContent = poi.name;
      const summary = document.createElement("p"); summary.textContent = poi.summary;
      card.append(number, heading, summary); card.addEventListener("click", () => { void selectPoi(poi.id, true); }); elements.poiGrid.append(card);
      if (map) { const marker = L.marker([poi.latitude, poi.longitude]).addTo(map).bindTooltip(poi.name); marker.on("click", () => { void selectPoi(poi.id, true); }); markers.set(poi.id, marker); }
    });
  }
  function renderTopics() {
    elements.topicGrid.replaceChildren();
    if (!topics.length) { const empty = document.createElement("p"); empty.className = "status-line"; empty.textContent = text("noTopics"); elements.topicGrid.append(empty); return; }
    topics.forEach(topic => {
      const button = document.createElement("button"); button.type = "button"; button.className = "topic-card"; button.setAttribute("aria-pressed", String(topic.id === selectedTopicId));
      const category = document.createElement("small"); category.textContent = text("categories")[topic.category] || topic.category;
      const title = document.createElement("strong"); title.textContent = topic.title;
      button.append(category, title); button.addEventListener("click", () => { void selectTopic(topic.id); }); elements.topicGrid.append(button);
    });
  }
  async function loadPois(keepSelection = true) {
    const request = beginRequest("pois"); const requestLanguage = currentLanguage;
    setBusy(elements.poiGrid, true); setStatus(text("loading"));
    try {
      const response = await api(`/api/public/pois?lang=${encodeURIComponent(requestLanguage)}`, {}, request.signal);
      if (!isCurrentRequest("pois", request) || requestLanguage !== currentLanguage) return;
      pois = response;
      if (!pois.length) { selectedPoiId = null; topics = []; renderPois(); renderTopics(); clearNarration(); setStatus(text("noPois"), "error"); return; }
      if (!keepSelection || !pois.some(poi => poi.id === selectedPoiId)) selectedPoiId = pois[0].id;
      renderPois(); await selectPoi(selectedPoiId, false);
      if (isCurrentRequest("pois", request)) {
        if (!map && !window.L) setStatus(text("mapUnavailable"), "error");
        else if (!retryAction) setStatus("");
      }
    } catch (error) {
      if (isAbort(error) || !isCurrentRequest("pois", request)) return;
      setStatus(publicError(error), "error"); setRetry(() => loadPois(keepSelection));
    } finally { if (isCurrentRequest("pois", request)) setBusy(elements.poiGrid, false); }
  }
  async function selectPoi(poiId, focusMap) {
    const poi = pois.find(item => item.id === poiId); if (!poi) return;
    stopSpeech(); selectedPoiId = poiId; selectedTopicId = null; topics = []; clearNarration(); renderPois(); renderTopics();
    if (focusMap && map) map.setView([poi.latitude, poi.longitude], 11, { animate: true });
    const request = beginRequest("topics"); const requestLanguage = currentLanguage; setBusy(elements.topicGrid, true);
    try {
      const response = await api(`/api/public/pois/${poiId}/topics?lang=${encodeURIComponent(requestLanguage)}`, {}, request.signal);
      if (!isCurrentRequest("topics", request) || selectedPoiId !== poiId || requestLanguage !== currentLanguage) return;
      topics = response; renderTopics(); clearRetry();
    } catch (error) {
      if (isAbort(error) || !isCurrentRequest("topics", request)) return;
      topics = []; renderTopics(); setStatus(publicError(error), "error"); setRetry(() => selectPoi(poiId, focusMap));
    } finally { if (isCurrentRequest("topics", request)) setBusy(elements.topicGrid, false); }
  }
  async function selectTopic(topicId) {
    if (!topics.some(topic => topic.id === topicId)) return;
    stopSpeech(); selectedTopicId = topicId; clearNarration(); renderTopics(); setBusy(elements.narrationPanel, true);
    const request = beginRequest("narration"); const requestLanguage = currentLanguage;
    try {
      const narration = await api(`/api/public/topics/${topicId}/narration?lang=${encodeURIComponent(requestLanguage)}`, {}, request.signal);
      if (!isCurrentRequest("narration", request) || selectedTopicId !== topicId || requestLanguage !== currentLanguage) return;
      if (!narration || !String(narration.content || "").trim()) { elements.narrationHeading.textContent = text("narrationEmptyTitle"); elements.narrationText.textContent = text("speechEmpty"); setSpeechControls({ canReplay: false }); setSpeechStatus("speechEmpty", "error"); return; }
      selectedNarration = narration;
      elements.narrationCategory.textContent = text("categories")[narration.category] || narration.category;
      elements.narrationHeading.textContent = narration.title;
      elements.narrationText.textContent = narration.content;
      setNarrationLanguage(narration.languageCode);
      elements.narrationNotice.textContent = narration.fallbackUsed ? text("fallback") : "";
      clearRetry(); speakNarration(narration);
    } catch (error) {
      if (isAbort(error) || !isCurrentRequest("narration", request)) return;
      elements.narrationNotice.textContent = ""; setSpeechStatus("", "error"); elements.narrationText.textContent = publicError(error); setRetry(() => selectTopic(topicId));
    } finally { if (isCurrentRequest("narration", request)) setBusy(elements.narrationPanel, false); }
  }
  function refreshVoices() { try { availableVoices = typeof window.speechSynthesis?.getVoices === "function" ? window.speechSynthesis.getVoices() : []; } catch { availableVoices = []; } }
  function initializeVoices() {
    if (!("speechSynthesis" in window)) return;
    refreshVoices();
    if (typeof window.speechSynthesis.addEventListener === "function") window.speechSynthesis.addEventListener("voiceschanged", refreshVoices);
    else window.speechSynthesis.onvoiceschanged = refreshVoices;
  }
  function stopSpeech(announce = false) {
    const wasActive = Boolean(activeSpeech) || Boolean(window.speechSynthesis?.speaking) || Boolean(window.speechSynthesis?.pending);
    speechToken += 1; activeSpeech = null; window.clearTimeout(speechStartTimer); speechStartTimer = null;
    if ("speechSynthesis" in window) window.speechSynthesis.cancel();
    setSpeechControls({ canReplay: Boolean(selectedNarration) });
    if (announce && wasActive) setSpeechStatus("speechStopped", "stopped"); else if (!announce) setSpeechStatus();
  }
  function showSpeechUnavailable() { speechToken += 1; activeSpeech = null; window.clearTimeout(speechStartTimer); speechStartTimer = null; setSpeechControls({ canReplay: Boolean(selectedNarration) }); setSpeechStatus("speechUnavailable", "error"); }
  function speakNarration(narration) {
    stopSpeech();
    if (!String(narration?.content || "").trim()) { setSpeechStatus("speechEmpty", "error"); return; }
    if (!("speechSynthesis" in window) || typeof window.SpeechSynthesisUtterance !== "function" || !ttsLanguage) { showSpeechUnavailable(); return; }
    const token = ++speechToken; const isCurrentSpeech = () => token === speechToken;
    try {
      const utterance = new SpeechSynthesisUtterance(narration.content);
      utterance.lang = ttsLanguage.resolveSpeechLocale(narration.languageCode);
      const matchingVoice = ttsLanguage.chooseVoice(availableVoices, utterance.lang);
      if (matchingVoice) utterance.voice = matchingVoice;
      activeSpeech = { token, utterance };
      utterance.onstart = () => { if (!isCurrentSpeech()) return; window.clearTimeout(speechStartTimer); speechStartTimer = null; setSpeechControls({ canPause: true, canStop: true }); setSpeechStatus("speaking", "speaking"); };
      utterance.onpause = () => { if (!isCurrentSpeech()) return; elements.pause.textContent = "▶"; setSpeechStatus("speechPaused", "paused"); };
      utterance.onresume = () => { if (!isCurrentSpeech()) return; elements.pause.textContent = text("pause"); setSpeechStatus("speechResumed", "resumed"); };
      utterance.onend = () => { if (!isCurrentSpeech()) return; window.clearTimeout(speechStartTimer); speechStartTimer = null; activeSpeech = null; setSpeechControls({ canReplay: true }); setSpeechStatus("speechEnded", "ended"); };
      utterance.onerror = () => { if (!isCurrentSpeech()) return; speechToken += 1; window.clearTimeout(speechStartTimer); speechStartTimer = null; activeSpeech = null; setSpeechControls({ canReplay: true }); setSpeechStatus("speechUnavailable", "error"); };
      elements.pause.textContent = text("pause"); setSpeechControls({ canPause: true, canStop: true, canReplay: true }); setSpeechStatus("speechPreparing", "preparing");
      window.speechSynthesis.speak(utterance);
      speechStartTimer = window.setTimeout(() => {
        if (!isCurrentSpeech()) return;
        speechToken += 1; activeSpeech = null; window.speechSynthesis.cancel(); setSpeechControls({ canReplay: true }); setSpeechStatus("speechUnavailable", "error");
      }, 3000);
    } catch { if (isCurrentSpeech()) showSpeechUnavailable(); }
  }
  async function renderAds(placement, host) {
    const request = beginRequest(`ads-${placement}`);
    try {
      const ads = await api(`/api/public/advertisements?placement=${placement}`, {}, request.signal);
      if (!isCurrentRequest(`ads-${placement}`, request)) return;
      host.replaceChildren(); if (!ads.length) return;
      const label = document.createElement("span"); label.className = "ad-label"; label.textContent = sponsoredLabels[currentLanguage] || sponsoredLabels.vi; host.append(label);
      ads.forEach(ad => { const link = document.createElement("a"); link.className = "ad-item"; link.href = ad.targetUrl; link.target = "_blank"; link.rel = "noopener noreferrer"; const image = document.createElement("img"); image.src = ad.imageUrl; image.alt = ad.name; link.append(image); link.addEventListener("click", () => recordAdEvent(ad.id, "click")); host.append(link); recordAdEvent(ad.id, "impression"); });
    } catch (error) { if (!isAbort(error) && isCurrentRequest(`ads-${placement}`, request)) host.replaceChildren(); }
  }
  function ensureVisitorSession() {
    return issueVisitorSession();
  }
  function recordAdEvent(advertisementId, eventType) { return ensureVisitorSession().then(issued => issued ? api(`/api/public/advertisements/${advertisementId}/events`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ eventType }) }) : undefined).catch(() => undefined); }
  function startHeartbeat() { const send = () => ensureVisitorSession().then(issued => issued ? api("/api/public/sessions/heartbeat", { method: "POST", headers: { "Content-Type": "application/json" }, body: "{}" }) : undefined).catch(() => undefined); send(); window.setInterval(send, 30000); }

  elements.language.addEventListener("change", async () => {
    currentLanguage = normalizeUiLanguage(elements.language.value); localStorage.setItem("vivoice_language", currentLanguage); stopSpeech(); selectedNarration = null; renderStaticLanguage(); clearNarration();
    try { await Promise.all([loadPois(true), renderAds("LEFT", elements.leftAds), renderAds("RIGHT", elements.rightAds)]); } catch (error) { if (!isAbort(error)) setStatus(publicError(error), "error"); }
  });
  elements.pause.addEventListener("click", () => { if (!activeSpeech || !("speechSynthesis" in window)) return; if (window.speechSynthesis.paused) window.speechSynthesis.resume(); else window.speechSynthesis.pause(); });
  elements.stop.addEventListener("click", () => stopSpeech(true));
  elements.replay.addEventListener("click", () => { if (selectedNarration) speakNarration(selectedNarration); });
  elements.retry.addEventListener("click", () => { const action = retryAction; clearRetry(); if (action) void action(); });

  Object.entries(locales).forEach(([code, locale]) => { const option = document.createElement("option"); option.value = code; option.textContent = locale.name; elements.language.append(option); });
  renderStaticLanguage(); initializeVoices(); createMap(); void Promise.all([loadPois(false), renderAds("LEFT", elements.leftAds), renderAds("RIGHT", elements.rightAds)]); startHeartbeat();
})();
