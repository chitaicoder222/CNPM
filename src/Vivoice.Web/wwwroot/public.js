// Purpose: Drives the public VIVOICE map, multilingual POI exploration, truthful browser TTS and anonymous heartbeat.
// Responsibility: Fetches only API data, synchronizes selected marker/card/topic and never hardcodes POI content.
(() => {
  const locales = {
    vi: { name: "Tiếng Việt", speech: "vi-VN", eyebrow: "Hành trình Việt Nam đa ngôn ngữ", heroTitle: "Chạm vào một địa điểm. Lắng nghe một câu chuyện.", heroCopy: "Khám phá văn hóa, hương vị và những hành trình đặc sắc của Việt Nam theo ngôn ngữ của bạn.", heroCard: "Bản đồ thật. Nội dung thật. Giọng đọc được thử phát tự động.", mapKicker: "Khám phá", mapTitle: "Sáu điểm đến nổi bật", narrationKicker: "Thuyết minh tự động", narrationTitle: "Chọn một chủ đề để bắt đầu", topics: "Chủ đề", narrationEmptyTitle: "Nội dung sẽ xuất hiện tại đây", narrationEmptyText: "Chọn một POI, sau đó chọn chủ đề bạn muốn khám phá.", pause: "Tạm dừng", replay: "Đọc lại", footer: "Khám phá có trách nhiệm, lắng nghe bằng ngôn ngữ của bạn.", loading: "Đang tải dữ liệu điểm đến…", apiUnavailable: "Không thể kết nối dữ liệu VIVOICE. Hãy mở trang từ ứng dụng VIVOICE đang chạy.", noTopics: "Địa điểm này chưa có chủ đề công khai.", fallback: "Nội dung đang hiển thị bản tiếng Việt.", speaking: "Đang thử phát giọng đọc…", speechUnavailable: "Trình duyệt chưa thể phát giọng đọc; bạn vẫn có thể đọc nội dung.", speechEnded: "Đã đọc xong.", categories: { culture: "Văn hóa đặc sắc", food: "Ẩm thực nổi tiếng", tourism: "Điểm du lịch nổi bật" } },
    ko: { name: "한국어", speech: "ko-KR", eyebrow: "다국어 베트남 여행", heroTitle: "장소를 선택하고, 이야기를 들어보세요.", heroCopy: "원하는 언어로 베트남의 문화와 맛, 여행지를 만나보세요.", heroCard: "실제 지도와 데이터로 안내를 시작합니다.", mapKicker: "탐색", mapTitle: "주요 여행지", narrationKicker: "자동 안내", narrationTitle: "주제를 선택하세요", topics: "주제", narrationEmptyTitle: "여기에 안내 내용이 표시됩니다", narrationEmptyText: "여행지와 주제를 선택하세요.", pause: "일시 정지", replay: "다시 듣기", footer: "책임 있는 여행, 당신의 언어로 듣기.", loading: "여행지 정보를 불러오는 중…", apiUnavailable: "VIVOICE 데이터에 연결할 수 없습니다. 실행 중인 VIVOICE 앱에서 페이지를 여세요.", noTopics: "공개된 주제가 없습니다.", fallback: "베트남어 원문이 표시됩니다.", speaking: "음성 읽기를 시도하는 중…", speechUnavailable: "브라우저에서 음성을 재생하지 못했습니다. 텍스트는 계속 읽을 수 있습니다.", speechEnded: "읽기가 끝났습니다.", categories: { culture: "특색 있는 문화", food: "유명한 음식", tourism: "유명한 관광지" } },
    ja: { name: "日本語", speech: "ja-JP", eyebrow: "多言語のベトナム旅", heroTitle: "場所を選び、物語を聴く。", heroCopy: "お好みの言語でベトナムの文化、味、旅先を発見しましょう。", heroCard: "地図とデータから案内を始めます。", mapKicker: "探索", mapTitle: "注目の目的地", narrationKicker: "自動ガイド", narrationTitle: "テーマを選択", topics: "テーマ", narrationEmptyTitle: "ここにガイドが表示されます", narrationEmptyText: "目的地とテーマを選択してください。", pause: "一時停止", replay: "もう一度聞く", footer: "責任ある旅を、あなたの言葉で。", loading: "目的地を読み込み中…", apiUnavailable: "VIVOICE データに接続できません。起動中の VIVOICE アプリから開いてください。", noTopics: "公開中のテーマはありません。", fallback: "ベトナム語の原文を表示しています。", speaking: "音声読み上げを試しています…", speechUnavailable: "ブラウザで音声を再生できません。テキストは読めます。", speechEnded: "読み上げが終わりました。", categories: { culture: "特色ある文化", food: "有名な名物", tourism: "有名な観光地" } },
    zh: { name: "中文", speech: "zh-CN", eyebrow: "多语言越南之旅", heroTitle: "选择地点，聆听故事。", heroCopy: "用您的语言探索越南的文化、美食和旅行目的地。", heroCard: "真实地图与真实数据，自动尝试语音讲解。", mapKicker: "探索", mapTitle: "热门目的地", narrationKicker: "自动讲解", narrationTitle: "请选择主题", topics: "主题", narrationEmptyTitle: "讲解内容将显示在这里", narrationEmptyText: "请选择目的地和主题。", pause: "暂停", replay: "重新朗读", footer: "负责任地旅行，用您的语言聆听。", loading: "正在加载目的地…", apiUnavailable: "无法连接到 VIVOICE 数据。请从正在运行的 VIVOICE 应用中打开页面。", noTopics: "此地点没有公开主题。", fallback: "正在显示越南语原文。", speaking: "正在尝试语音朗读…", speechUnavailable: "浏览器无法播放语音；您仍可阅读文字。", speechEnded: "朗读完成。", categories: { culture: "特色文化", food: "著名美食", tourism: "著名旅游地" } },
    th: { name: "ไทย", speech: "th-TH", eyebrow: "การเดินทางเวียดนามหลายภาษา", heroTitle: "เลือกสถานที่ แล้วฟังเรื่องราว", heroCopy: "ค้นพบวัฒนธรรม รสชาติ และสถานที่ของเวียดนามในภาษาของคุณ", heroCard: "แผนที่และข้อมูลจริง พร้อมลองอ่านออกเสียงอัตโนมัติ", mapKicker: "สำรวจ", mapTitle: "จุดหมายเด่น", narrationKicker: "คำบรรยายอัตโนมัติ", narrationTitle: "เลือกหัวข้อ", topics: "หัวข้อ", narrationEmptyTitle: "เนื้อหาจะแสดงที่นี่", narrationEmptyText: "เลือกสถานที่และหัวข้อที่ต้องการ", pause: "หยุดชั่วคราว", replay: "อ่านอีกครั้ง", footer: "ท่องเที่ยวอย่างรับผิดชอบ ฟังในภาษาของคุณ", loading: "กำลังโหลดจุดหมาย…", apiUnavailable: "ไม่สามารถเชื่อมต่อข้อมูล VIVOICE ได้ โปรดเปิดหน้านี้จากแอป VIVOICE ที่กำลังทำงานอยู่", noTopics: "ไม่มีหัวข้อสาธารณะ", fallback: "กำลังแสดงเนื้อหาภาษาเวียดนาม", speaking: "กำลังลองเล่นเสียง…", speechUnavailable: "เบราว์เซอร์เล่นเสียงไม่ได้ แต่ยังอ่านข้อความได้", speechEnded: "อ่านจบแล้ว", categories: { culture: "วัฒนธรรมเด่น", food: "อาหารขึ้นชื่อ", tourism: "สถานที่ท่องเที่ยวเด่น" } }
  };

  const elements = {
    language: document.getElementById("languageSelect"), status: document.getElementById("publicStatus"), poiGrid: document.getElementById("poiGrid"), topicGrid: document.getElementById("topicGrid"), narrationCategory: document.getElementById("narrationCategory"), narrationHeading: document.getElementById("narrationHeading"), narrationText: document.getElementById("narrationText"), speechStatus: document.getElementById("speechStatus"), pause: document.getElementById("pauseSpeech"), replay: document.getElementById("replaySpeech"), leftAds: document.getElementById("leftAds"), rightAds: document.getElementById("rightAds")
  };
  let currentLanguage = localStorage.getItem("vivoice_language") || "vi";
  let selectedPoiId = null;
  let selectedTopicId = null;
  let selectedNarration = null;
  let pois = [];
  let map;
  let markers = new Map();
  let sessionId;

  function text(key) { return locales[currentLanguage][key]; }
  function setStatus(message, state = "") { elements.status.className = `status-line ${state}`; elements.status.textContent = message; }
  function publicError(error) { return error instanceof TypeError || error?.message === "Failed to fetch" ? text("apiUnavailable") : error?.message || text("apiUnavailable"); }
  async function api(path, options = {}) {
    const response = await fetch(path, { headers: { "Accept": "application/json", ...(options.headers || {}) }, ...options });
    if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message || `HTTP ${response.status}`); }
    return response.status === 204 ? null : response.json();
  }

  function renderStaticLanguage() {
    document.documentElement.lang = currentLanguage;
    document.querySelectorAll("[data-i18n]").forEach(element => { element.textContent = text(element.dataset.i18n); });
    elements.language.value = currentLanguage;
  }

  function createMap() {
    if (!window.L) { setStatus("Không thể tải thư viện bản đồ.", "error"); return; }
    map = L.map("map", { scrollWheelZoom: false }).setView([16.3, 107.4], 5.5);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { maxZoom: 18, attribution: "© OpenStreetMap contributors" }).addTo(map);
  }

  function renderPois() {
    elements.poiGrid.replaceChildren();
    markers.forEach(marker => marker.remove());
    markers = new Map();
    pois.forEach((poi, index) => {
      const card = document.createElement("button");
      card.type = "button"; card.className = "poi-card"; card.setAttribute("aria-pressed", String(poi.id === selectedPoiId));
      const number = document.createElement("span"); number.className = "number"; number.textContent = String(index + 1).padStart(2, "0");
      const heading = document.createElement("h3"); heading.textContent = poi.name;
      const summary = document.createElement("p"); summary.textContent = poi.summary;
      card.append(number, heading, summary); card.addEventListener("click", () => { void selectPoi(poi.id, true).catch(error => setStatus(publicError(error), "error")); }); elements.poiGrid.append(card);
      if (map) { const marker = L.marker([poi.latitude, poi.longitude]).addTo(map).bindTooltip(poi.name); marker.on("click", () => { void selectPoi(poi.id, true).catch(error => setStatus(publicError(error), "error")); }); markers.set(poi.id, marker); }
    });
  }

  async function loadPois(keepSelection = true) {
    setStatus(text("loading"));
    pois = await api(`/api/public/pois?lang=${encodeURIComponent(currentLanguage)}`);
    if (!pois.length) { setStatus("Không có địa điểm đang hiển thị.", "error"); return; }
    if (!keepSelection || !pois.some(poi => poi.id === selectedPoiId)) selectedPoiId = pois[0].id;
    renderPois();
    await selectPoi(selectedPoiId, false);
    setStatus("");
  }

  async function selectPoi(poiId, focusMap) {
    selectedPoiId = poiId; selectedTopicId = null; cancelSpeech();
    renderPois();
    const poi = pois.find(item => item.id === poiId);
    if (focusMap && poi && map) map.setView([poi.latitude, poi.longitude], 11, { animate: true });
    elements.topicGrid.replaceChildren();
    const topics = await api(`/api/public/pois/${poiId}/topics?lang=${encodeURIComponent(currentLanguage)}`);
    if (!topics.length) { const empty = document.createElement("p"); empty.className = "status-line"; empty.textContent = text("noTopics"); elements.topicGrid.append(empty); return; }
    topics.forEach(topic => {
      const button = document.createElement("button"); button.type = "button"; button.className = "topic-card"; button.setAttribute("aria-pressed", "false"); button.dataset.topicId = topic.id;
      const category = document.createElement("small"); category.textContent = text("categories")[topic.category] || topic.category;
      const title = document.createElement("strong"); title.textContent = topic.title;
      button.append(category, title); button.addEventListener("click", () => selectTopic(topic.id)); elements.topicGrid.append(button);
    });
    elements.narrationCategory.textContent = poi?.name || ""; elements.narrationHeading.textContent = text("narrationEmptyTitle"); elements.narrationText.textContent = text("narrationEmptyText");
  }

  async function selectTopic(topicId) {
    selectedTopicId = topicId; cancelSpeech();
    document.querySelectorAll(".topic-card").forEach(button => button.setAttribute("aria-pressed", String(Number(button.dataset.topicId) === topicId)));
    elements.speechStatus.textContent = text("speaking");
    try {
      const narration = await api(`/api/public/topics/${topicId}/narration?lang=${encodeURIComponent(currentLanguage)}`);
      selectedNarration = narration;
      elements.narrationCategory.textContent = text("categories")[narration.category] || narration.category;
      elements.narrationHeading.textContent = narration.title;
      elements.narrationText.textContent = narration.content;
      if (narration.fallbackUsed) elements.speechStatus.textContent = text("fallback");
      speakNarration(narration);
    } catch (error) { elements.speechStatus.textContent = publicError(error); elements.speechStatus.className = "status-line error"; }
  }

  function speakNarration(narration) {
    if (!("speechSynthesis" in window)) { showSpeechUnavailable(); return; }
    const utterance = new SpeechSynthesisUtterance(narration.content); utterance.lang = locales[currentLanguage].speech; utterance.onend = () => { elements.speechStatus.textContent = text("speechEnded"); elements.pause.classList.add("hidden"); }; utterance.onerror = showSpeechUnavailable;
    try { window.speechSynthesis.cancel(); window.speechSynthesis.speak(utterance); elements.pause.classList.remove("hidden"); elements.replay.classList.remove("hidden"); elements.speechStatus.className = "status-line"; if (!narration.fallbackUsed) elements.speechStatus.textContent = text("speaking"); } catch { showSpeechUnavailable(); }
  }

  function showSpeechUnavailable() { elements.speechStatus.className = "status-line error"; elements.speechStatus.textContent = text("speechUnavailable"); elements.pause.classList.add("hidden"); }
  function cancelSpeech() { if ("speechSynthesis" in window) window.speechSynthesis.cancel(); elements.pause.classList.add("hidden"); }

  async function renderAds(placement, host) {
    try {
      const ads = await api(`/api/public/advertisements?placement=${placement}`); host.replaceChildren();
      if (!ads.length) return;
      const label = document.createElement("span"); label.className = "ad-label"; label.textContent = currentLanguage === "vi" ? "Quảng cáo / Tài trợ" : "Sponsored"; host.append(label);
      ads.forEach(ad => { const link = document.createElement("a"); link.className = "ad-item"; link.href = ad.targetUrl; link.target = "_blank"; link.rel = "noopener noreferrer"; const image = document.createElement("img"); image.src = ad.imageUrl; image.alt = ad.name; link.append(image); link.addEventListener("click", () => recordAdEvent(ad.id, "click")); host.append(link); recordAdEvent(ad.id, "impression"); });
    } catch { host.replaceChildren(); }
  }

  function getSessionId() {
    if (sessionId) return sessionId;
    sessionId = sessionStorage.getItem("vivoice_session_id");
    if (!sessionId) { sessionId = crypto.randomUUID(); sessionStorage.setItem("vivoice_session_id", sessionId); }
    return sessionId;
  }
  function recordAdEvent(advertisementId, eventType) {
    return api(`/api/public/advertisements/${advertisementId}/events`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ eventType, sessionId: getSessionId() }) }).catch(() => undefined);
  }

  function startHeartbeat() {
    const send = () => api("/api/public/sessions/heartbeat", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ sessionId: getSessionId() }) }).catch(() => undefined);
    send(); window.setInterval(send, 30000);
  }

  elements.language.addEventListener("change", async () => { currentLanguage = elements.language.value; localStorage.setItem("vivoice_language", currentLanguage); renderStaticLanguage(); try { await Promise.all([loadPois(true), renderAds("LEFT", elements.leftAds), renderAds("RIGHT", elements.rightAds)]); } catch (error) { setStatus(publicError(error), "error"); } });
  elements.pause.addEventListener("click", () => { if (!window.speechSynthesis) return; if (window.speechSynthesis.paused) { window.speechSynthesis.resume(); elements.pause.textContent = text("pause"); } else { window.speechSynthesis.pause(); elements.pause.textContent = "▶"; } });
  elements.replay.addEventListener("click", () => { if (selectedNarration) speakNarration(selectedNarration); });

  Object.entries(locales).forEach(([code, locale]) => { const option = document.createElement("option"); option.value = code; option.textContent = locale.name; elements.language.append(option); });
  renderStaticLanguage(); createMap(); Promise.all([loadPois(false), renderAds("LEFT", elements.leftAds), renderAds("RIGHT", elements.rightAds)]).catch(error => setStatus(publicError(error), "error")); startHeartbeat();
})();
