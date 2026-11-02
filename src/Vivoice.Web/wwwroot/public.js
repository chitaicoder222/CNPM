// Purpose: Drives the public VIVOICE map, GPS-triggered multilingual narration and anonymous heartbeat.
// Responsibility: Synchronizes API state safely, keeps narration language separate from UI language and never hardcodes POI content.
(() => {
  const locales = {
    vi: { name: "Tiếng Việt", eyebrow: "Hành trình Việt Nam đa ngôn ngữ", heroTitle: "Chạm vào một địa điểm. Lắng nghe một câu chuyện.", heroCopy: "Khám phá văn hóa, hương vị và những hành trình đặc sắc của Việt Nam theo ngôn ngữ của bạn.", heroCard: "Bản đồ thật. Nội dung thật. Giọng đọc được thử phát tự động.", mapKicker: "Khám phá", mapTitle: "Khám phá Thành phố Hồ Chí Minh", narrationKicker: "Thuyết minh tự động", narrationTitle: "Chọn một chủ đề để bắt đầu", topics: "Chủ đề", narrationEmptyTitle: "Nội dung sẽ xuất hiện tại đây", narrationEmptyText: "Chọn một POI, sau đó chọn chủ đề bạn muốn khám phá.", narrationLanguage: "Ngôn ngữ nội dung", pause: "Tạm dừng", stop: "Dừng", replay: "Đọc lại", retry: "Thử lại", footer: "Khám phá có trách nhiệm, lắng nghe bằng ngôn ngữ của bạn.", loading: "Đang tải dữ liệu điểm đến…", apiUnavailable: "Không thể kết nối dữ liệu VIVOICE. Hãy kiểm tra kết nối rồi thử lại.", mapUnavailable: "Không thể tải bản đồ. Hãy kiểm tra kết nối rồi thử lại.", noPois: "Hiện chưa có địa điểm công khai.", noTopics: "Địa điểm này chưa có chủ đề công khai.", fallback: "Nội dung đang hiển thị bản tiếng Việt.", speechPreparing: "Đang chuẩn bị giọng đọc…", speaking: "Đang đọc thuyết minh…", speechPaused: "Đã tạm dừng giọng đọc.", speechResumed: "Đang tiếp tục giọng đọc…", speechStopped: "Đã dừng giọng đọc.", speechEmpty: "Nội dung thuyết minh trống nên không thể đọc.", speechUnavailable: "Trình duyệt chưa thể phát giọng đọc; bạn vẫn có thể đọc nội dung.", speechEnded: "Đã đọc xong.", categories: { culture: "Văn hóa đặc sắc", food: "Ẩm thực nổi tiếng", tourism: "Điểm du lịch nổi bật" } },
    ko: { name: "한국어", eyebrow: "다국어 베트남 여행", heroTitle: "장소를 선택하고, 이야기를 들어보세요.", heroCopy: "원하는 언어로 베트남의 문화와 맛, 여행지를 만나보세요.", heroCard: "실제 지도와 데이터로 안내를 시작합니다.", mapKicker: "탐색", mapTitle: "호찌민시 둘러보기", narrationKicker: "자동 안내", narrationTitle: "주제를 선택하세요", topics: "주제", narrationEmptyTitle: "여기에 안내 내용이 표시됩니다", narrationEmptyText: "여행지와 주제를 선택하세요.", narrationLanguage: "콘텐츠 언어", pause: "일시 정지", stop: "중지", replay: "다시 듣기", retry: "다시 시도", footer: "책임 있는 여행, 당신의 언어로 듣기.", loading: "여행지 정보를 불러오는 중…", apiUnavailable: "VIVOICE 데이터에 연결할 수 없습니다. 연결을 확인한 뒤 다시 시도하세요.", mapUnavailable: "지도를 불러올 수 없습니다. 연결을 확인한 뒤 다시 시도하세요.", noPois: "현재 공개된 여행지가 없습니다.", noTopics: "공개된 주제가 없습니다.", fallback: "베트남어 원문이 표시됩니다.", speechPreparing: "음성 읽기를 준비하는 중…", speaking: "안내를 읽는 중…", speechPaused: "음성 읽기가 일시 정지되었습니다.", speechResumed: "음성 읽기를 계속합니다…", speechStopped: "음성 읽기를 중지했습니다.", speechEmpty: "읽을 안내 내용이 없습니다.", speechUnavailable: "브라우저에서 음성을 재생하지 못했습니다. 텍스트는 계속 읽을 수 있습니다.", speechEnded: "읽기가 끝났습니다.", categories: { culture: "특색 있는 문화", food: "유명한 음식", tourism: "유명한 관광지" } },
    ja: { name: "日本語", eyebrow: "多言語のベトナム旅", heroTitle: "場所を選び、物語を聴く。", heroCopy: "お好みの言語でベトナムの文化、味、旅先を発見しましょう。", heroCard: "地図とデータから案内を始めます。", mapKicker: "探索", mapTitle: "ホーチミン市を探索", narrationKicker: "自動ガイド", narrationTitle: "テーマを選択", topics: "テーマ", narrationEmptyTitle: "ここにガイドが表示されます", narrationEmptyText: "目的地とテーマを選択してください。", narrationLanguage: "コンテンツの言語", pause: "一時停止", stop: "停止", replay: "もう一度聞く", retry: "再試行", footer: "責任ある旅を、あなたの言葉で。", loading: "目的地を読み込み中…", apiUnavailable: "VIVOICE データに接続できません。接続を確認して再試行してください。", mapUnavailable: "地図を読み込めません。接続を確認して再試行してください。", noPois: "公開中の目的地はありません。", noTopics: "公開中のテーマはありません。", fallback: "ベトナム語の原文を表示しています。", speechPreparing: "音声読み上げを準備しています…", speaking: "ガイドを読み上げています…", speechPaused: "読み上げを一時停止しました。", speechResumed: "読み上げを再開しています…", speechStopped: "読み上げを停止しました。", speechEmpty: "読み上げる内容がありません。", speechUnavailable: "ブラウザで音声を再生できません。テキストは読めます。", speechEnded: "読み上げが終わりました。", categories: { culture: "特色ある文化", food: "有名な名物", tourism: "有名な観光地" } },
    zh: { name: "中文", eyebrow: "多语言越南之旅", heroTitle: "选择地点，聆听故事。", heroCopy: "用您的语言探索越南的文化、美食和旅行目的地。", heroCard: "真实地图与真实数据，自动尝试语音讲解。", mapKicker: "探索", mapTitle: "探索胡志明市", narrationKicker: "自动讲解", narrationTitle: "请选择主题", topics: "主题", narrationEmptyTitle: "讲解内容将显示在这里", narrationEmptyText: "请选择目的地和主题。", narrationLanguage: "内容语言", pause: "暂停", stop: "停止", replay: "重新朗读", retry: "重试", footer: "负责任地旅行，用您的语言聆听。", loading: "正在加载目的地…", apiUnavailable: "无法连接到 VIVOICE 数据。请检查连接后重试。", mapUnavailable: "无法加载地图。请检查连接后重试。", noPois: "目前没有公开地点。", noTopics: "此地点没有公开主题。", fallback: "正在显示越南语原文。", speechPreparing: "正在准备语音朗读…", speaking: "正在朗读讲解…", speechPaused: "语音朗读已暂停。", speechResumed: "正在继续语音朗读…", speechStopped: "语音朗读已停止。", speechEmpty: "没有可朗读的讲解内容。", speechUnavailable: "浏览器无法播放语音；您仍可阅读文字。", speechEnded: "朗读完成。", categories: { culture: "特色文化", food: "著名美食", tourism: "著名旅游地" } },
    en: {"name":"English","eyebrow":"Explore Vietnam in your language","heroTitle":"Choose a place. Listen to its story.","heroCopy":"Discover Vietnam's culture, flavors and destinations in your language.","heroCard":"Real maps. Real stories. Browser narration.","mapKicker":"Explore","mapTitle":"Explore Ho Chi Minh City","narrationKicker":"Automatic narration","narrationTitle":"Choose a topic to begin","topics":"Topics","narrationEmptyTitle":"Your story will appear here","narrationEmptyText":"Choose a place, then a topic to explore.","narrationLanguage":"Content language","pause":"Pause","stop":"Stop","replay":"Play again","retry":"Try again","footer":"Travel responsibly. Listen in your language.","loading":"Loading destinations…","apiUnavailable":"Cannot connect to VIVOICE. Check your connection and try again.","mapUnavailable":"Cannot load the map. Check your connection and try again.","noPois":"No public places are available yet.","noTopics":"No public topics are available for this place.","fallback":"Showing the Vietnamese version.","speechPreparing":"Preparing narration…","speaking":"Playing narration…","speechPaused":"Narration paused.","speechResumed":"Resuming narration…","speechStopped":"Narration stopped.","speechEmpty":"There is no narration text to read.","speechUnavailable":"Your browser could not play the narration. You can still read the text.","speechEnded":"Narration finished.","categories":{"culture":"Culture","food":"Food","tourism":"Places to visit"}}
  };
  const gpsLocales = {
    vi: { gpsTitle: "Tự động thuyết minh theo vị trí", gpsEnable: "Bật định vị", gpsDisable: "Tắt định vị", gpsHelp: "Cho phép định vị để tự nghe khi đi vào vòng bán kính của địa điểm. Tọa độ chỉ được xử lý trên thiết bị khi bạn mở trang.", gpsOff: "Định vị đang tắt. Bạn có thể xem bản đồ và đọc nội dung.", gpsLocating: "Đang xác định vị trí…", gpsWatching: "Đang theo dõi vị trí. Hãy đi vào vòng bán kính của một địa điểm.", gpsNearby: "Bạn đang trong vùng thuyết minh của {poi}.", gpsDenied: "Bạn chưa cho phép định vị. Hãy cấp quyền trong trình duyệt rồi bật lại.", gpsUnsupported: "Trình duyệt này chưa hỗ trợ định vị.", gpsInsecure: "Định vị cần kết nối HTTPS hoặc localhost.", gpsTimeout: "Chưa nhận được vị trí. Hãy bật định vị để thử lại.", gpsUnavailable: "Không thể xác định vị trí. Hãy kiểm tra GPS rồi bật lại.", gpsInaccurate: "Vị trí chưa đủ chính xác để xác định vùng POI. Hãy chờ tín hiệu GPS tốt hơn.", gpsContentError: "Chưa tải được thuyết minh của {poi}. Bạn có thể tắt và bật lại định vị để thử lại.", heroTitle: "Đến một địa điểm. Lắng nghe câu chuyện nơi đó.", heroCard: "Bật định vị. Đi vào vòng bán kính. Tự động nghe thuyết minh.", narrationTitle: "Thuyết minh tự động khi bạn đến gần", narrationEmptyText: "Bật định vị để tự nghe khi đến gần POI. Chọn địa điểm hoặc chủ đề để xem trước văn bản." },
    ko: { gpsTitle: "위치 기반 자동 안내", gpsEnable: "위치 켜기", gpsDisable: "위치 끄기", gpsHelp: "위치를 허용하면 장소 반경에 들어갈 때 안내합니다. 좌표는 페이지가 열려 있는 동안 기기에서만 처리합니다.", gpsOff: "위치가 꺼져 있습니다. 지도와 글은 볼 수 있습니다.", gpsLocating: "위치를 찾는 중…", gpsWatching: "장소의 반경 안으로 이동하세요.", gpsNearby: "{poi} 안내 구역에 있습니다.", gpsDenied: "브라우저에서 위치 권한을 허용한 후 다시 켜세요.", gpsUnsupported: "이 브라우저는 위치를 지원하지 않습니다.", gpsInsecure: "위치에는 HTTPS 또는 localhost가 필요합니다.", gpsTimeout: "위치를 받지 못했습니다. 다시 켜서 시도하세요.", gpsUnavailable: "위치를 확인할 수 없습니다. GPS를 확인하세요.", gpsInaccurate: "위치 정확도가 부족합니다. 더 나은 GPS 신호를 기다리세요.", gpsContentError: "{poi} 안내를 불러올 수 없습니다. 위치를 껐다 켜서 다시 시도하세요.", heroTitle: "장소에 도착하고, 그 이야기를 들어보세요.", heroCard: "위치를 켜고 반경 안에 들어가면 자동으로 안내합니다.", narrationTitle: "가까이 가면 자동 안내", narrationEmptyText: "위치를 켜면 가까운 장소를 안내합니다. 장소나 주제를 선택하면 글을 미리 볼 수 있습니다." },
    ja: { gpsTitle: "位置情報による自動ガイド", gpsEnable: "位置情報をオン", gpsDisable: "位置情報をオフ", gpsHelp: "位置情報を許可すると地点の範囲に入ったときに案内します。座標はページを開いている間、端末内だけで処理します。", gpsOff: "位置情報はオフです。地図と文章は閲覧できます。", gpsLocating: "位置を確認中…", gpsWatching: "地点の円の範囲内に移動してください。", gpsNearby: "{poi}の案内エリアにいます。", gpsDenied: "ブラウザで位置情報を許可してから再度オンにしてください。", gpsUnsupported: "このブラウザは位置情報に対応していません。", gpsInsecure: "位置情報にはHTTPSまたはlocalhostが必要です。", gpsTimeout: "位置を取得できませんでした。再度オンにしてください。", gpsUnavailable: "位置を確認できません。GPSを確認してください。", gpsInaccurate: "位置の精度が不足しています。GPS信号をお待ちください。", gpsContentError: "{poi}の案内を取得できません。位置情報をオフにしてから再度オンにしてください。", heroTitle: "その場所に着き、物語を聴く。", heroCard: "位置情報をオンにして範囲に入ると自動で案内します。", narrationTitle: "近づくと自動ガイド", narrationEmptyText: "位置情報をオンにすると近くの地点を案内します。地点やテーマを選ぶと文章を閲覧できます。" },
    zh: { gpsTitle: "按位置自动讲解", gpsEnable: "开启定位", gpsDisable: "关闭定位", gpsHelp: "允许定位后，进入地点半径即自动讲解。坐标仅在页面打开时在设备上处理。", gpsOff: "定位已关闭，仍可查看地图和文字。", gpsLocating: "正在定位…", gpsWatching: "正在跟踪位置，请进入地点的半径范围。", gpsNearby: "您已进入{poi}的讲解区域。", gpsDenied: "请在浏览器中允许定位，然后重新开启。", gpsUnsupported: "此浏览器不支持定位。", gpsInsecure: "定位需要HTTPS或localhost。", gpsTimeout: "尚未获取位置，请重新开启定位重试。", gpsUnavailable: "无法获取位置，请检查GPS。", gpsInaccurate: "定位精度不足，请等待更好的GPS信号。", gpsContentError: "无法加载{poi}的讲解，请关闭并重新开启定位重试。", heroTitle: "到达一个地点，聆听当地故事。", heroCard: "开启定位，进入地点半径，自动聆听讲解。", narrationTitle: "靠近地点自动讲解", narrationEmptyText: "开启定位后，靠近地点即可自动聆听。选择地点或主题可预览文字。" },
    en: {"gpsTitle":"Automatic narration by location","gpsEnable":"Enable location","gpsDisable":"Disable location","gpsHelp":"Allow location access to listen when you enter a place's radius. Coordinates are processed only on your device while this page is open.","gpsOff":"Location is off. You can still browse the map and read the stories.","gpsLocating":"Finding your location…","gpsWatching":"Following your location. Enter a place's radius to listen.","gpsNearby":"You are within the narration area for {poi}.","gpsDenied":"Location permission is blocked. Allow it in your browser, then enable location again.","gpsUnsupported":"This browser does not support location.","gpsInsecure":"Location requires HTTPS or localhost.","gpsTimeout":"No location was received. Enable location again to retry.","gpsUnavailable":"Cannot find your location. Check GPS and try again.","gpsInaccurate":"Location is not accurate enough to detect nearby places. Wait for a better GPS signal.","gpsContentError":"Could not load the story for {poi}. Disable and enable location to try again.","heroTitle":"Arrive at a place. Listen to its story.","heroCard":"Enable location. Enter the radius. Listen automatically.","narrationTitle":"Hear the story when you arrive nearby","narrationEmptyText":"Enable location to listen near a place. Select a place or topic to preview the text."}
  };
  Object.entries(gpsLocales).forEach(([language, messages]) => Object.assign(locales[language], messages));
  const userRangeLocales = {
    vi: { userRadiusLabel: "Bán kính quanh bạn", userRadiusLegend: "Phạm vi quanh bạn", poiRadiusLegend: "Vùng thuyết minh POI", accuracyLegend: "Sai số GPS", accuracyValue: "Sai số GPS: ±{accuracy} m", positionWaiting: "Chưa có vị trí", centerOnMe: "Về vị trí của tôi", userRadiusHelp: "Vòng xanh thể hiện khoảng cách quanh bạn. Thuyết minh tự phát khi bạn đi vào vòng đỏ của địa điểm." },
    ko: { userRadiusLabel: "내 주변 반경", userRadiusLegend: "내 주변 범위", poiRadiusLegend: "장소 안내 구역", accuracyLegend: "GPS 오차", accuracyValue: "GPS 오차: ±{accuracy} m", positionWaiting: "아직 위치가 없습니다", centerOnMe: "내 위치로 이동", userRadiusHelp: "초록색 원은 내 주변 거리를 나타냅니다. 장소의 빨간색 원 안에 들어가면 자동으로 안내합니다." },
    ja: { userRadiusLabel: "自分の周囲の半径", userRadiusLegend: "自分の周囲の範囲", poiRadiusLegend: "地点のガイド範囲", accuracyLegend: "GPSの誤差", accuracyValue: "GPSの誤差: ±{accuracy} m", positionWaiting: "位置情報がありません", centerOnMe: "現在地へ移動", userRadiusHelp: "緑の円は自分の周囲の距離を示します。地点の赤い円に入ると自動ガイドが始まります。" },
    zh: { userRadiusLabel: "您周围的半径", userRadiusLegend: "您的周边范围", poiRadiusLegend: "地点讲解区域", accuracyLegend: "GPS误差", accuracyValue: "GPS误差：±{accuracy} m", positionWaiting: "尚无位置信息", centerOnMe: "返回我的位置", userRadiusHelp: "绿色圆圈表示您周围的距离。进入地点的红色圆圈后会自动讲解。" },
    en: {"userRadiusLabel":"Radius around you","userRadiusLegend":"Area around you","poiRadiusLegend":"POI narration area","accuracyLegend":"GPS accuracy","accuracyValue":"GPS accuracy: ±{accuracy} m","positionWaiting":"Location not available yet","centerOnMe":"Back to my location","userRadiusHelp":"The green circle shows the distance around you. Narration starts when you enter a place's red circle."}
  };
  Object.entries(userRangeLocales).forEach(([language, messages]) => Object.assign(locales[language], messages));
  const mapLocales = {
    vi: { heroAction: "Khám phá địa điểm", mapRetry: "Thử lại bản đồ", mapTilesUnavailable: "Chưa tải được nền bản đồ. Các điểm và nội dung vẫn sẵn sàng." },
    ko: { heroAction: "장소 둘러보기", mapRetry: "지도 다시 시도", mapTilesUnavailable: "지도 배경을 불러오지 못했습니다. 장소와 안내 내용은 계속 이용할 수 있습니다." },
    ja: { heroAction: "場所を探す", mapRetry: "地図を再読み込み", mapTilesUnavailable: "背景地図を読み込めません。地点とガイドの内容は引き続き利用できます。" },
    zh: { heroAction: "探索地点", mapRetry: "重新加载地图", mapTilesUnavailable: "暂时无法加载地图底图，地点和讲解内容仍可使用。" },
    en: { heroAction: "Explore places", mapRetry: "Retry the map", mapTilesUnavailable: "The map background could not load. Places and stories are still available." }
  };
  Object.entries(mapLocales).forEach(([language, messages]) => Object.assign(locales[language], messages));
  const ariaLabels = {
    vi: { language: "Chọn ngôn ngữ", map: "Bản đồ các điểm thuyết minh VIVOICE", leftAds: "Quảng cáo bên trái", rightAds: "Quảng cáo bên phải" },
    ko: { language: "언어 선택", map: "VIVOICE 안내 장소 지도", leftAds: "왼쪽 광고", rightAds: "오른쪽 광고" },
    ja: { language: "言語を選択", map: "VIVOICE 案内スポットの地図", leftAds: "左側の広告", rightAds: "右側の広告" },
    zh: { language: "选择语言", map: "VIVOICE 讲解地点地图", leftAds: "左侧广告", rightAds: "右侧广告" },
    en: {"language":"Choose a language","map":"VIVOICE narrated places map","leftAds":"Left-side advertisements","rightAds":"Right-side advertisements"}
  };
  const sponsoredLabels = { vi: "Quảng cáo / Tài trợ", ko: "광고 / 후원", ja: "広告 / 協賛", zh: "广告 / 赞助", en: "Advertisement / Sponsored" };

  const elements = {
    language: document.getElementById("languageSelect"), status: document.getElementById("publicStatus"), map: document.getElementById("map"), poiGrid: document.getElementById("poiGrid"), topicGrid: document.getElementById("topicGrid"), narrationPanel: document.getElementById("narrationPanel"), narrationCategory: document.getElementById("narrationCategory"), narrationLanguage: document.getElementById("narrationLanguage"), narrationHeading: document.getElementById("narrationHeading"), narrationText: document.getElementById("narrationText"), narrationNotice: document.getElementById("narrationNotice"), speechStatus: document.getElementById("speechStatus"), pause: document.getElementById("pauseSpeech"), stop: document.getElementById("stopSpeech"), replay: document.getElementById("replaySpeech"), retry: document.getElementById("retryPublic"), leftAds: document.getElementById("leftAds"), rightAds: document.getElementById("rightAds")
  };
  const requestControllers = new Map();
  const gpsButton = document.getElementById("toggleGps");
  const gpsStatus = document.getElementById("gpsStatus");
  const userRadiusSelect = document.getElementById("userRadiusSelect");
  const centerOnMe = document.getElementById("centerOnMe");
  const positionAccuracy = document.getElementById("positionAccuracy");
  const userRadiusOptions = [50, 100, 200, 500];
  let userRadiusMeters = 100;
  let userRadiusCircle;
  let hasFocusedPosition = false;
  let gpsStatusKey = "off";
  let gpsStatusPoi = null;
  let currentPosition = null;
  let positionMarker;
  let accuracyCircle;
  const radiusCircles = new Map();
  const ttsLanguage = window.VivoiceTtsLanguage;
  const savedLanguage = readStoredLanguage();
  let currentLanguage = normalizeUiLanguage(savedLanguage === "th" ? "en" : savedLanguage);
  if (savedLanguage === "th") persistLanguage(currentLanguage);
  let selectedPoiId = null;
  let selectedTopicId = null;
  let selectedNarration = null;
  let narrationCanReplay = false;
  let pois = [];
  let topics = [];
  let map;
  let mapTileLayer;
  let mapResizeObserver;
  let mapResizeTimer;
  let heartbeatTimer;
  let markers = new Map();
  const createSessionIssuer = window.VivoiceVisitorSession?.createSessionIssuer;
  const issueVisitorSession = typeof createSessionIssuer === "function"
    ? createSessionIssuer(() => api("/api/public/sessions", { method: "POST" }))
    : () => api("/api/public/sessions", { method: "POST" }).then(() => true).catch(() => false);
  let retryAction = null;
  let languageChangeToken = 0;
  const gpsGuide = window.VivoiceGpsGuide.createGuide({
    geolocation: navigator.geolocation,
    isSecureContext: window.isSecureContext,
    play: playGpsPoi,
    cancel: () => stopSpeech(),
    onStatus: (key, poi) => { gpsStatusKey = key; gpsStatusPoi = poi; renderGpsStatus(); },
    onPosition: position => { currentPosition = position; renderPosition(); }
  });
  const narrationController = window.VivoiceNarrationController.create({
    elements, text, ttsLanguage, window, setSpeechStatus, setSpeechControls,
    getSelectedNarration: () => selectedNarration,
    setNarrationCanReplay: value => { narrationCanReplay = value; },
    isReplayAllowed: () => gpsGuide.isWithinRange(selectedPoiId)
  });

  function readStoredLanguage() { try { return localStorage.getItem("vivoice_language"); } catch { return "vi"; } }
  function persistLanguage(languageCode) { try { localStorage.setItem("vivoice_language", languageCode); } catch { /* Keep the language for this page when storage is unavailable. */ } }
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
    elements.replay.classList.toggle("hidden", !(canReplay && narrationCanReplay && gpsGuide.isWithinRange(selectedPoiId)));
  }
  function setNarrationLanguage(languageCode) {
    elements.narrationLanguage.textContent = `${text("narrationLanguage")}: ${languageName(languageCode)}`;
    elements.narrationLanguage.classList.remove("hidden");
  }
  function clearNarration() {
    selectedNarration = null;
    narrationCanReplay = false;
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
    renderGpsStatus();
    renderUserRangeControls();
  }
  function renderGpsStatus() {
    gpsButton.textContent = text(gpsGuide.isActive() ? "gpsDisable" : "gpsEnable");
    gpsButton.setAttribute("aria-pressed", String(gpsGuide.isActive()));
    const key = `gps${gpsStatusKey[0].toUpperCase()}${gpsStatusKey.slice(1)}`;
    gpsStatus.textContent = text(key).replace("{poi}", gpsStatusPoi?.name || "");
  }
  function renderUserRangeControls() {
    userRadiusSelect.value = String(userRadiusMeters);
    centerOnMe.disabled = !currentPosition || !map;
    const accuracyText = currentPosition
      ? text("accuracyValue").replace("{accuracy}", String(Math.round(currentPosition.accuracy)))
      : text("positionWaiting");
    if (positionAccuracy.textContent !== accuracyText) positionAccuracy.textContent = accuracyText;
  }
  function focusUserPosition(poi = null) {
    if (!map || !currentPosition || !userRadiusCircle) return;
    const bounds = userRadiusCircle.getBounds();
    if (poi) bounds.extend([poi.latitude, poi.longitude]);
    map.fitBounds(bounds, { padding: [28, 28], maxZoom: 17 });
  }
  function renderPosition() {
    renderUserRangeControls();
    if (!map) return;
    if (!currentPosition) {
      positionMarker?.remove(); accuracyCircle?.remove(); userRadiusCircle?.remove();
      positionMarker = null; accuracyCircle = null; userRadiusCircle = null; hasFocusedPosition = false;
      return;
    }
    const location = [currentPosition.latitude, currentPosition.longitude];
    if (!accuracyCircle) accuracyCircle = L.circle(location, { radius: currentPosition.accuracy, weight: 1, dashArray: "4 6", fillOpacity: 0.035, interactive: false, className: "gps-accuracy" }).addTo(map);
    else accuracyCircle.setLatLng(location).setRadius(currentPosition.accuracy);
    if (!userRadiusCircle) userRadiusCircle = L.circle(location, { radius: userRadiusMeters, weight: 2, fillOpacity: 0.12, interactive: false, className: "user-radius" }).addTo(map);
    else userRadiusCircle.setLatLng(location).setRadius(userRadiusMeters);
    if (!positionMarker) positionMarker = L.circleMarker(location, { radius: 7, className: "user-location" }).addTo(map);
    else positionMarker.setLatLng(location);
    if (!hasFocusedPosition) { focusUserPosition(); hasFocusedPosition = true; }
  }
  function showMapNotice(key, retryable = false) {
    const host = document.getElementById("mapNotice"); host.replaceChildren();
    const message = document.createElement("span"); message.textContent = text(key); host.append(message);
    if (retryable) { const retry = document.createElement("button"); retry.type = "button"; retry.className = "map-retry"; retry.textContent = text("mapRetry"); retry.addEventListener("click", () => { mapTileLayer?.redraw(); scheduleMapResize(); }); host.append(retry); }
    host.classList.remove("hidden");
  }
  function scheduleMapResize() {
    window.clearTimeout(mapResizeTimer);
    mapResizeTimer = window.setTimeout(() => map?.invalidateSize({ pan: false, animate: false }), 80);
  }
  function createMap() {
    if (!window.L) {
      showMapNotice("mapUnavailable");
      setStatus(text("mapUnavailable"), "error");
      setRetry(() => window.location.reload());
      return false;
    }
    if (map) return true;
    map = L.map("map", { scrollWheelZoom: false }).setView([16.3, 107.4], 5.5);
    mapTileLayer = L.tileLayer("https://{s}.tile.openstreetmap.de/{z}/{x}/{y}.png", { maxZoom: 18, attribution: "© OpenStreetMap contributors" })
      .on("tileerror", () => showMapNotice("mapTilesUnavailable", true))
      .on("load", () => document.getElementById("mapNotice").classList.add("hidden"))
      .addTo(map);
    if (typeof window.ResizeObserver === "function") { mapResizeObserver = new window.ResizeObserver(scheduleMapResize); mapResizeObserver.observe(elements.map); }
    window.addEventListener("resize", scheduleMapResize, { passive: true });
    scheduleMapResize();
    return true;
  }
  function createPoiImage(poi) {
    const visual = document.createElement("span"); visual.className = "poi-visual";
    const fallback = () => { visual.replaceChildren(); visual.classList.add("is-fallback"); visual.textContent = poi.name; };
    if (!poi.imageUrl) { fallback(); return visual; }
    const image = document.createElement("img"); image.src = poi.imageUrl; image.alt = poi.name; image.loading = "lazy"; image.decoding = "async";
    image.addEventListener("error", fallback, { once: true }); visual.append(image); return visual;
  }
  function renderPois() {
    elements.poiGrid.replaceChildren();
    markers.forEach(marker => marker.remove());
    radiusCircles.forEach(circle => circle.remove()); radiusCircles.clear();
    markers = new Map();
    pois.forEach((poi, index) => {
      const card = document.createElement("button"); card.type = "button"; card.className = "poi-card"; card.setAttribute("aria-pressed", String(poi.id === selectedPoiId));
      const body = document.createElement("span"); body.className = "poi-card-body";
      const number = document.createElement("span"); number.className = "number"; number.textContent = String(index + 1).padStart(2, "0");
      const heading = document.createElement("h3"); heading.textContent = poi.name;
      const summary = document.createElement("p"); summary.textContent = poi.summary;
      body.append(number, heading, summary); card.append(createPoiImage(poi), body); card.addEventListener("click", () => { gpsGuide.interrupt(); void selectPoi(poi.id, true); }); elements.poiGrid.append(card);
      if (map) {
        const marker = L.marker([poi.latitude, poi.longitude]).addTo(map).bindTooltip(poi.name);
        marker.on("click", () => { gpsGuide.interrupt(); void selectPoi(poi.id, true); }); markers.set(poi.id, marker);
        if (Number.isFinite(poi.radiusMeters) && poi.radiusMeters > 0) radiusCircles.set(poi.id, L.circle([poi.latitude, poi.longitude], { radius: poi.radiusMeters, weight: 1, fillOpacity: 0.06, className: "poi-radius" }).addTo(map));
      }
    });
  }
  function renderTopics() {
    elements.topicGrid.replaceChildren();
    if (!topics.length) { const empty = document.createElement("p"); empty.className = "status-line"; empty.textContent = text("noTopics"); elements.topicGrid.append(empty); return; }
    topics.forEach((topic, index) => {
      const button = document.createElement("button"); button.type = "button"; button.className = "topic-card"; button.setAttribute("aria-pressed", String(topic.id === selectedTopicId));
      const order = document.createElement("span"); order.className = "topic-order"; order.textContent = String(index + 1).padStart(2, "0");
      const copy = document.createElement("span"); copy.className = "topic-card-copy";
      const category = document.createElement("small"); category.textContent = text("categories")[topic.category] || topic.category;
      const title = document.createElement("strong"); title.textContent = topic.title;
      copy.append(category, title); button.append(order, copy); button.addEventListener("click", () => { gpsGuide.interrupt(); void selectTopic(topic.id); }); elements.topicGrid.append(button);
    });
  }
  async function loadPois(keepSelection = true) {
    const request = beginRequest("pois"); const requestLanguage = currentLanguage;
    setBusy(elements.poiGrid, true); setStatus(text("loading"));
    try {
      const response = await api(`/api/public/pois?lang=${encodeURIComponent(requestLanguage)}`, {}, request.signal);
      if (!isCurrentRequest("pois", request) || requestLanguage !== currentLanguage) return;
      pois = response;
      if (!pois.length) { gpsGuide.setPois([]); selectedPoiId = null; topics = []; renderPois(); renderTopics(); clearNarration(); setStatus(text("noPois"), "error"); return; }
      if (!keepSelection || !pois.some(poi => poi.id === selectedPoiId)) selectedPoiId = pois[0].id;
      renderPois();
      if (!keepSelection && map && !currentPosition) map.fitBounds(pois.map(poi => [poi.latitude, poi.longitude]), { padding: [24, 24], maxZoom: 15 });
      await selectPoi(selectedPoiId, false);
      if (isCurrentRequest("pois", request)) {
        gpsGuide.setPois(pois);
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
      clearRetry(); setSpeechControls({ canReplay: false });
    } catch (error) {
      if (isAbort(error) || !isCurrentRequest("narration", request)) return;
      elements.narrationNotice.textContent = ""; setSpeechStatus("", "error"); elements.narrationText.textContent = publicError(error); setRetry(() => selectTopic(topicId));
    } finally { if (isCurrentRequest("narration", request)) setBusy(elements.narrationPanel, false); }
  }
  function stopSpeech(announce = false) {
    return narrationController.stop(announce);
  }
  function speakNarration(narration) {
    return narrationController.speak(narration);
  }
  async function playGpsPoi(poi, signal) {
    const language = currentLanguage;
    requestControllers.get("topics")?.abort(); requestControllers.get("narration")?.abort();
    stopSpeech(); selectedPoiId = poi.id; selectedTopicId = null; topics = []; clearNarration(); renderPois(); renderTopics();
    focusUserPosition(poi);
    const stopOnExit = () => stopSpeech();
    signal.addEventListener("abort", stopOnExit, { once: true });
    const stillCurrent = () => !signal.aborted && language === currentLanguage;
    try {
      setBusy(elements.topicGrid, true);
      const result = await api(`/api/public/pois/${poi.id}/topics?lang=${encodeURIComponent(language)}`, {}, signal);
      if (!stillCurrent()) return;
      topics = result; renderTopics(); setBusy(elements.topicGrid, false);
      for (const topic of result) {
        if (!stillCurrent()) return;
        selectedTopicId = topic.id; clearNarration(); renderTopics(); setBusy(elements.narrationPanel, true);
        const narration = await api(`/api/public/topics/${topic.id}/narration?lang=${encodeURIComponent(language)}`, {}, signal);
        if (!stillCurrent()) return;
        selectedNarration = narration; narrationCanReplay = true;
        elements.narrationCategory.textContent = text("categories")[narration.category] || narration.category;
        elements.narrationHeading.textContent = narration.title; elements.narrationText.textContent = narration.content;
        setNarrationLanguage(narration.languageCode); elements.narrationNotice.textContent = narration.fallbackUsed ? text("fallback") : "";
        setBusy(elements.narrationPanel, false);
        if (!await speakNarration(narration)) return;
      }
    } finally {
      signal.removeEventListener("abort", stopOnExit);
      if (selectedPoiId === poi.id) { setBusy(elements.topicGrid, false); setBusy(elements.narrationPanel, false); }
    }
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
  function startHeartbeat() { const send = () => ensureVisitorSession().then(issued => issued ? api("/api/public/sessions/heartbeat", { method: "POST", headers: { "Content-Type": "application/json" }, body: "{}" }) : undefined).catch(() => undefined); send(); heartbeatTimer = window.setInterval(send, 30000); }

  elements.language.addEventListener("change", async () => {
    const changeToken = ++languageChangeToken;
    gpsGuide.interrupt();
    currentLanguage = normalizeUiLanguage(elements.language.value); persistLanguage(currentLanguage); stopSpeech(); selectedNarration = null; renderStaticLanguage(); clearNarration();
    try { await Promise.all([loadPois(true), renderAds("LEFT", elements.leftAds), renderAds("RIGHT", elements.rightAds)]); if (changeToken === languageChangeToken) gpsGuide.refresh(); } catch (error) { if (!isAbort(error)) setStatus(publicError(error), "error"); }
  });
  elements.pause.addEventListener("click", () => { if (!narrationController.isActive() || !("speechSynthesis" in window)) return; if (window.speechSynthesis.paused) window.speechSynthesis.resume(); else window.speechSynthesis.pause(); });
  elements.stop.addEventListener("click", () => { stopSpeech(true); gpsGuide.interrupt(); setSpeechStatus("speechStopped", "stopped"); });
  elements.replay.addEventListener("click", () => { if (selectedNarration && narrationCanReplay && gpsGuide.isWithinRange(selectedPoiId)) { gpsGuide.interrupt(); void speakNarration(selectedNarration); } });
  elements.retry.addEventListener("click", () => { const action = retryAction; clearRetry(); if (action) void action(); });
  gpsButton.addEventListener("click", () => { if (gpsGuide.isActive()) gpsGuide.stop(); else gpsGuide.start(); });
  userRadiusSelect.addEventListener("change", () => {
    const radius = Number(userRadiusSelect.value);
    if (!userRadiusOptions.includes(radius)) { userRadiusSelect.value = String(userRadiusMeters); return; }
    userRadiusMeters = radius; renderPosition(); focusUserPosition();
  });
  centerOnMe.addEventListener("click", () => focusUserPosition());
  window.addEventListener("pagehide", () => { gpsGuide.stop(); mapResizeObserver?.disconnect(); window.clearTimeout(mapResizeTimer); if (heartbeatTimer != null) window.clearInterval(heartbeatTimer); });

  Object.entries(locales).forEach(([code, locale]) => { const option = document.createElement("option"); option.value = code; option.textContent = locale.name; elements.language.append(option); });
  renderStaticLanguage(); narrationController.initializeVoices(); createMap(); void Promise.all([loadPois(false), renderAds("LEFT", elements.leftAds), renderAds("RIGHT", elements.rightAds)]); startHeartbeat();
})();
