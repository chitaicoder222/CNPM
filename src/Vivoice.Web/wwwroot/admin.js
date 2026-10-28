// Purpose: Operates the protected VIVOICE admin console using only authenticated SQL-backed API responses.
// Responsibility: Manages POIs, translations, ads, sponsors, agreements, rates and truthful dashboard data without fixtures.
(() => {
  const byId = id => document.getElementById(id);
  const state = { pois: [], selectedPoi: null, topics: [], selectedTopic: null, poiTranslations: [], topicTranslations: [], sponsors: [], ads: [], agreements: [], rates: [] };
  let antiforgeryToken;

  function requiresAntiforgery(method) { return !["GET", "HEAD", "OPTIONS", "TRACE"].includes(method.toUpperCase()); }
  async function getAntiforgeryToken() {
    if (antiforgeryToken) return antiforgeryToken;
    const response = await fetch("/api/admin/auth/antiforgery", { credentials: "same-origin", headers: { Accept: "application/json" } });
    if (!response.ok) throw new Error("Không thể chuẩn bị bảo vệ phiên quản trị.");
    const body = await response.json();
    if (!body.token) throw new Error("Không thể chuẩn bị bảo vệ phiên quản trị.");
    antiforgeryToken = body.token;
    return antiforgeryToken;
  }
  async function api(path, options = {}) {
    const method = (options.method || "GET").toUpperCase();
    const headers = { Accept: "application/json", ...(options.headers || {}) };
    if (requiresAntiforgery(method)) headers["X-VIVOICE-CSRF"] = await getAntiforgeryToken();
    const response = await fetch(path, { credentials: "same-origin", ...options, method, headers });
    if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message || `HTTP ${response.status}`); }
    return response.status === 204 ? null : response.json();
  }
  function writeStatus(id, message, stateName = "") { const target = byId(id); target.className = `status-line ${stateName}`; target.textContent = message; }
  function money(value, currency = "USD") { return new Intl.NumberFormat("en-US", { style: "currency", currency, maximumFractionDigits: 2 }).format(value || 0); }
  function utcInput(value) { return value ? new Date(value).toISOString().slice(0, 16) : ""; }
  function inputUtc(id) { const value = byId(id).value; return value ? new Date(value).toISOString() : null; }
  function todayInput(date) { return date.toISOString().slice(0, 10); }
  function buttonList(hostId, values, makeLabel, select) {
    const host = byId(hostId); host.replaceChildren();
    values.forEach(value => { const button = document.createElement("button"); button.type = "button"; button.className = "list-button"; const content = makeLabel(value); button.append(content); button.addEventListener("click", () => select(value)); host.append(button); });
    if (!values.length) host.textContent = "Chưa có dữ liệu.";
  }
  function option(selectId, value, label, selected = false) { const item = document.createElement("option"); item.value = String(value); item.textContent = label; item.selected = selected; byId(selectId).append(item); }

  async function checkSession() { try { await api("/api/admin/auth/me"); await showAdmin(); } catch { showLogin(); } }
  function showLogin() { byId("loginView").classList.remove("hidden"); byId("adminView").classList.add("hidden"); }
  async function showAdmin() {
    byId("loginView").classList.add("hidden"); byId("adminView").classList.remove("hidden");
    await Promise.all([loadDashboard(), loadPois(), loadSponsors(), loadAdvertisements(), loadAgreements(), loadRates()]);
    if (!state.selectedPoi && state.pois.length) await selectPoi(state.pois[0]);
  }

  function renderDashboard(data) {
    const values = [
      ["Lượt hợp lệ", data.validVisits], ["Khách duy nhất", data.uniqueVisitors], ["Phiên đang hoạt động", data.activeSessions],
      ["Thiết bị đang hoạt động", data.activeDevices], ["Ước tính traffic", money(data.trafficEstimate, data.currency)],
      ["Doanh thu hợp đồng", money(data.contractedRevenue, data.currency)], ["Đã nhận trong kỳ", money(data.receivedRevenue, data.currency)],
      ["Còn phải thu", money(data.outstandingRevenue, data.currency)], ["Đối tác hoạt động", data.activeSponsors]
    ];
    const metrics = byId("metrics"); metrics.replaceChildren();
    values.forEach(([label, value]) => { const card = document.createElement("div"); card.className = "metric"; const name = document.createElement("span"); name.textContent = label; const number = document.createElement("strong"); number.textContent = String(value); card.append(name, number); metrics.append(card); });
    const deviceText = Object.entries(data.activeDevicesByType).map(([type, count]) => `${type}: ${count}`).join(" · ");
    writeStatus("deviceStatus", deviceText || "Chưa có thiết bị đang hoạt động.");
    buttonList("visitTrend", data.visitsByDate, item => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = item.date; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${item.visits} lượt hợp lệ`; fragment.append(title, note); return fragment; }, () => undefined);
    byId("adMetrics").textContent = `Hiển thị: ${data.advertisementImpressions} · Nhấp: ${data.advertisementClicks}. Dữ liệu tính trong khoảng lọc.`;
  }
  async function loadDashboard() {
    const from = new Date(`${byId("fromDate").value}T00:00:00Z`).toISOString();
    const to = new Date(`${byId("toDate").value}T23:59:59Z`).toISOString();
    try { renderDashboard(await api(`/api/admin/dashboard?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`)); await loadActiveDevices(); }
    catch (error) { writeStatus("deviceStatus", error.message, "error"); }
  }
  async function loadActiveDevices() {
    const devices = await api("/api/admin/devices/active");
    buttonList("activeDevices", devices, item => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = item.deviceType; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${item.browserFamily} · ${item.operatingSystem} · hoạt động ${new Date(item.lastSeenAtUtc).toLocaleString("vi-VN")}`; fragment.append(title, note); return fragment; }, () => undefined);
  }

  function renderPois() {
    const query = byId("poiSearch").value.trim().toLowerCase();
    const filtered = state.pois.filter(poi => !query || poi.nameVi.toLowerCase().includes(query) || poi.slug.includes(query));
    buttonList("poiList", filtered, poi => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = `${poi.displayOrder}. ${poi.nameVi}`; const note = document.createElement("span"); note.className = "status-line"; note.textContent = poi.isActive ? "Đang hiển thị" : "Đang ẩn"; fragment.append(title, note); return fragment; }, selectPoi);
  }
  async function loadPois() { state.pois = await api("/api/admin/pois"); if (state.selectedPoi) state.selectedPoi = state.pois.find(poi => poi.id === state.selectedPoi.id) || null; renderPois(); }
  async function selectPoi(poi) {
    state.selectedPoi = poi; state.selectedTopic = null; fillPoi(poi); renderPois();
    await Promise.all([loadTopics(), loadPoiTranslations()]);
  }
  function fillPoi(poi) {
    byId("poiId").value = poi.id; byId("poiSlug").value = poi.slug; byId("poiOrder").value = poi.displayOrder; byId("poiLatitude").value = poi.latitude; byId("poiLongitude").value = poi.longitude; byId("poiImageUrl").value = poi.imageUrl || ""; byId("poiName").value = poi.nameVi; byId("poiSummary").value = poi.summaryVi; byId("poiActive").checked = poi.isActive;
  }
  function newPoi() {
    state.selectedPoi = null; state.selectedTopic = null; state.topics = []; state.poiTranslations = []; state.topicTranslations = [];
    byId("poiForm").reset(); byId("poiId").value = ""; byId("poiActive").checked = true; byId("topicList").replaceChildren(); byId("topicForm").classList.add("hidden"); clearTranslations(); renderPois(); writeStatus("poiStatus", "Nhập thông tin để tạo POI mới.");
  }
  async function savePoi(event) {
    event.preventDefault();
    const id = byId("poiId").value;
    const body = { slug: byId("poiSlug").value, displayOrder: Number(byId("poiOrder").value), latitude: Number(byId("poiLatitude").value), longitude: Number(byId("poiLongitude").value), imageUrl: byId("poiImageUrl").value || null, nameVi: byId("poiName").value, summaryVi: byId("poiSummary").value, isActive: byId("poiActive").checked };
    try { const saved = await api(`/api/admin/pois${id ? `/${id}` : ""}`, { method: id ? "PATCH" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await loadPois(); await selectPoi(state.pois.find(poi => poi.id === saved.id)); writeStatus("poiStatus", "Đã lưu POI.", "success"); } catch (error) { writeStatus("poiStatus", error.message, "error"); }
  }

  function clearTranslations() { ["poiTranslationName", "poiTranslationSummary", "topicTranslationTitle", "topicTranslationContent"].forEach(id => { byId(id).value = ""; }); }
  async function loadPoiTranslations() {
    if (!state.selectedPoi) { clearTranslations(); return; }
    state.poiTranslations = await api(`/api/admin/pois/${state.selectedPoi.id}/translations`); fillPoiTranslation();
  }
  function fillPoiTranslation() {
    const language = byId("poiTranslationLanguage").value; const translation = state.poiTranslations.find(item => item.languageCode === language);
    byId("poiTranslationName").value = translation?.name || ""; byId("poiTranslationSummary").value = translation?.summary || "";
  }
  async function savePoiTranslation(event) {
    event.preventDefault(); if (!state.selectedPoi) { writeStatus("poiTranslationStatus", "Hãy chọn POI trước.", "error"); return; }
    const language = byId("poiTranslationLanguage").value; const body = { name: byId("poiTranslationName").value, summary: byId("poiTranslationSummary").value };
    try { await api(`/api/admin/pois/${state.selectedPoi.id}/translations/${language}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await loadPoiTranslations(); writeStatus("poiTranslationStatus", "Đã lưu bản dịch POI.", "success"); } catch (error) { writeStatus("poiTranslationStatus", error.message, "error"); }
  }

  async function loadTopics() {
    if (!state.selectedPoi) { state.topics = []; byId("topicHint").textContent = "Chọn POI để quản lý nội dung."; byId("topicList").replaceChildren(); return; }
    state.topics = await api(`/api/admin/pois/${state.selectedPoi.id}/topics`); if (state.selectedTopic) state.selectedTopic = state.topics.find(topic => topic.id === state.selectedTopic.id) || null;
    byId("topicHint").textContent = `Quản lý nội dung cho ${state.selectedPoi.nameVi}.`;
    buttonList("topicList", state.topics, topic => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = topic.titleVi; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${topic.category} · ${topic.isActive ? "Đang xuất bản" : "Đang ẩn"}`; fragment.append(title, note); return fragment; }, selectTopic);
  }
  async function selectTopic(topic) { state.selectedTopic = topic; fillTopic(topic); await loadTopicTranslations(); }
  function fillTopic(topic) { byId("topicForm").classList.remove("hidden"); byId("topicId").value = topic.id; byId("topicCategory").value = topic.category; byId("topicOrder").value = topic.displayOrder; byId("topicTitle").value = topic.titleVi; byId("topicContent").value = topic.contentVi; byId("topicActive").checked = topic.isActive; }
  function newTopic() {
    if (!state.selectedPoi) { writeStatus("topicStatus", "Hãy chọn hoặc lưu POI trước.", "error"); return; }
    state.selectedTopic = null; byId("topicForm").reset(); byId("topicId").value = ""; byId("topicActive").checked = true; byId("topicForm").classList.remove("hidden"); byId("topicTranslationTitle").value = ""; byId("topicTranslationContent").value = "";
  }
  async function saveTopic(event) {
    event.preventDefault(); if (!state.selectedPoi) return;
    const id = byId("topicId").value; const body = { category: byId("topicCategory").value, displayOrder: Number(byId("topicOrder").value), titleVi: byId("topicTitle").value, contentVi: byId("topicContent").value, isActive: byId("topicActive").checked };
    try { const saved = await api(id ? `/api/admin/topics/${id}` : `/api/admin/pois/${state.selectedPoi.id}/topics`, { method: id ? "PATCH" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await loadTopics(); await selectTopic(state.topics.find(topic => topic.id === saved.id)); writeStatus("topicStatus", "Đã lưu chủ đề.", "success"); } catch (error) { writeStatus("topicStatus", error.message, "error"); }
  }
  async function loadTopicTranslations() {
    if (!state.selectedTopic) { byId("topicTranslationTitle").value = ""; byId("topicTranslationContent").value = ""; return; }
    state.topicTranslations = await api(`/api/admin/topics/${state.selectedTopic.id}/translations`); fillTopicTranslation();
  }
  function fillTopicTranslation() {
    const language = byId("topicTranslationLanguage").value; const translation = state.topicTranslations.find(item => item.languageCode === language);
    byId("topicTranslationTitle").value = translation?.title || ""; byId("topicTranslationContent").value = translation?.content || ""; byId("topicTranslationPublished").checked = translation?.isPublished ?? true;
  }
  async function saveTopicTranslation(event) {
    event.preventDefault(); if (!state.selectedTopic) { writeStatus("topicTranslationStatus", "Hãy chọn chủ đề trước.", "error"); return; }
    const language = byId("topicTranslationLanguage").value; const body = { title: byId("topicTranslationTitle").value, content: byId("topicTranslationContent").value, isPublished: byId("topicTranslationPublished").checked };
    try { await api(`/api/admin/topics/${state.selectedTopic.id}/translations/${language}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await loadTopicTranslations(); writeStatus("topicTranslationStatus", "Đã lưu bản dịch và nguồn TTS.", "success"); } catch (error) { writeStatus("topicTranslationStatus", error.message, "error"); }
  }

  function refreshSponsorOptions() {
    ["adSponsor", "agreementSponsor", "paymentAgreement"].forEach(id => byId(id).replaceChildren());
    option("adSponsor", "", "Không liên kết");
    state.sponsors.forEach(sponsor => { option("adSponsor", sponsor.id, sponsor.name); option("agreementSponsor", sponsor.id, sponsor.name); });
    state.agreements.forEach(agreement => option("paymentAgreement", agreement.id, `${agreement.sponsorName} — ${money(agreement.committedAmount)}`));
  }
  async function loadSponsors() {
    state.sponsors = await api("/api/admin/sponsors"); refreshSponsorOptions();
    buttonList("sponsorList", state.sponsors, sponsor => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = sponsor.name; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${sponsor.isActive ? "Đang hợp tác" : "Đang ẩn"} · ${sponsor.agreementCount} hợp đồng · ${money(sponsor.contractedAmount)}`; fragment.append(title, note); return fragment; }, fillSponsor);
  }
  function fillSponsor(sponsor) { byId("sponsorId").value = sponsor.id; byId("sponsorName").value = sponsor.name; byId("sponsorContact").value = sponsor.contactNote || ""; byId("sponsorActive").checked = sponsor.isActive; }
  function newSponsor() { byId("sponsorForm").reset(); byId("sponsorId").value = ""; byId("sponsorActive").checked = true; }
  async function saveSponsor(event) {
    event.preventDefault(); const id = byId("sponsorId").value; const body = { name: byId("sponsorName").value, contactNote: byId("sponsorContact").value || null, isActive: byId("sponsorActive").checked };
    try { await api(`/api/admin/sponsors${id ? `/${id}` : ""}`, { method: id ? "PATCH" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await loadSponsors(); writeStatus("sponsorStatus", "Đã lưu đối tác.", "success"); } catch (error) { writeStatus("sponsorStatus", error.message, "error"); }
  }

  async function loadAdvertisements() {
    state.ads = await api("/api/admin/advertisements");
    buttonList("adList", state.ads, ad => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = ad.name; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${ad.placement} · ${ad.isActive ? "Bật" : "Tắt"} · ${ad.sponsorName || "Không gắn đối tác"}`; fragment.append(title, note); return fragment; }, fillAd);
  }
  function fillAd(ad) { byId("adId").value = ad.id; byId("adName").value = ad.name; byId("adSponsor").value = ad.sponsorId || ""; byId("adPlacement").value = ad.placement; byId("adOrder").value = ad.displayOrder; byId("adImageUrl").value = ad.imageUrl; byId("adTargetUrl").value = ad.targetUrl; byId("adStartsAt").value = utcInput(ad.startsAtUtc); byId("adEndsAt").value = utcInput(ad.endsAtUtc); byId("adActive").checked = ad.isActive; }
  function newAd() { byId("adForm").reset(); byId("adId").value = ""; byId("adActive").checked = true; byId("adStartsAt").value = utcInput(new Date().toISOString()); }
  async function saveAd(event) {
    event.preventDefault(); const id = byId("adId").value; const sponsorValue = byId("adSponsor").value;
    const body = { sponsorId: sponsorValue ? Number(sponsorValue) : null, name: byId("adName").value, imageUrl: byId("adImageUrl").value, targetUrl: byId("adTargetUrl").value, placement: byId("adPlacement").value, startsAtUtc: inputUtc("adStartsAt"), endsAtUtc: inputUtc("adEndsAt"), displayOrder: Number(byId("adOrder").value), isActive: byId("adActive").checked };
    try { await api(`/api/admin/advertisements${id ? `/${id}` : ""}`, { method: id ? "PATCH" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await loadAdvertisements(); writeStatus("adStatus", "Đã lưu quảng cáo.", "success"); } catch (error) { writeStatus("adStatus", error.message, "error"); }
  }

  async function loadAgreements() {
    state.agreements = await api("/api/admin/sponsorship-agreements"); refreshSponsorOptions();
    buttonList("agreementList", state.agreements, agreement => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = agreement.sponsorName; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${agreement.status} · ${money(agreement.committedAmount)} · đã nhận ${money(agreement.receivedAmount)}`; fragment.append(title, note); return fragment; }, fillAgreement);
  }
  function fillAgreement(agreement) { byId("agreementId").value = agreement.id; byId("agreementSponsor").value = agreement.sponsorId; byId("agreementStatus").value = agreement.status; byId("agreementAmount").value = agreement.committedAmount; byId("agreementStartsAt").value = utcInput(agreement.startsAtUtc); byId("agreementEndsAt").value = utcInput(agreement.endsAtUtc); byId("paymentAgreement").value = agreement.id; }
  function newAgreement() { byId("agreementForm").reset(); byId("agreementId").value = ""; byId("agreementStatus").value = "draft"; byId("agreementStartsAt").value = utcInput(new Date().toISOString()); }
  async function saveAgreement(event) {
    event.preventDefault(); const id = byId("agreementId").value; const body = { sponsorId: Number(byId("agreementSponsor").value), committedAmount: Number(byId("agreementAmount").value), currency: "USD", status: byId("agreementStatus").value, startsAtUtc: inputUtc("agreementStartsAt"), endsAtUtc: inputUtc("agreementEndsAt") };
    try { await api(`/api/admin/sponsorship-agreements${id ? `/${id}` : ""}`, { method: id ? "PATCH" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await loadAgreements(); writeStatus("agreementStatusText", "Đã lưu hợp đồng.", "success"); } catch (error) { writeStatus("agreementStatusText", error.message, "error"); }
  }
  async function savePayment(event) {
    event.preventDefault(); const agreementId = byId("paymentAgreement").value; const body = { receivedAmount: Number(byId("paymentAmount").value), receivedAtUtc: inputUtc("paymentReceivedAt") };
    try { await api(`/api/admin/sponsorship-agreements/${agreementId}/payments`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); byId("paymentForm").reset(); await Promise.all([loadAgreements(), loadDashboard()]); writeStatus("paymentStatus", "Đã ghi nhận khoản tiền; lịch sử không bị ghi đè.", "success"); } catch (error) { writeStatus("paymentStatus", error.message, "error"); }
  }

  async function loadRates() {
    state.rates = await api("/api/admin/revenue-rates");
    buttonList("rateList", state.rates, rate => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = `${rate.amount} ${rate.currency} / lượt`; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `Từ ${new Date(rate.effectiveFromUtc).toLocaleString("vi-VN")}${rate.effectiveToUtc ? ` đến ${new Date(rate.effectiveToUtc).toLocaleString("vi-VN")}` : " · đang hiệu lực"}`; fragment.append(title, note); return fragment; }, () => undefined);
  }
  async function saveRate(event) {
    event.preventDefault(); const body = { amount: Number(byId("rateAmount").value), currency: "USD", effectiveFromUtc: inputUtc("rateEffectiveFrom") };
    try { await api("/api/admin/revenue-rates", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); byId("rateForm").reset(); await Promise.all([loadRates(), loadDashboard()]); writeStatus("rateStatus", "Đã thêm mức giá mới và đóng khoảng hiệu lực của mức trước.", "success"); } catch (error) { writeStatus("rateStatus", error.message, "error"); }
  }

  byId("loginForm").addEventListener("submit", async event => { event.preventDefault(); try { await api("/api/admin/auth/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ displayName: byId("displayName").value, phone: byId("phone").value, password: byId("password").value }) }); antiforgeryToken = undefined; await showAdmin(); } catch (error) { writeStatus("loginStatus", error.message, "error"); } });
  byId("logoutButton").addEventListener("click", async () => { try { await api("/api/admin/auth/logout", { method: "POST" }); showLogin(); } finally { antiforgeryToken = undefined; } });
  byId("dashboardFilter").addEventListener("submit", event => { event.preventDefault(); loadDashboard(); });
  byId("poiSearch").addEventListener("input", renderPois); byId("newPoiButton").addEventListener("click", newPoi); byId("poiForm").addEventListener("submit", savePoi);
  byId("poiTranslationLanguage").addEventListener("change", fillPoiTranslation); byId("poiTranslationForm").addEventListener("submit", savePoiTranslation);
  byId("newTopicButton").addEventListener("click", newTopic); byId("topicForm").addEventListener("submit", saveTopic); byId("cancelTopicButton").addEventListener("click", () => byId("topicForm").classList.add("hidden"));
  byId("topicTranslationLanguage").addEventListener("change", fillTopicTranslation); byId("topicTranslationForm").addEventListener("submit", saveTopicTranslation);
  byId("newAdButton").addEventListener("click", newAd); byId("adForm").addEventListener("submit", saveAd);
  byId("newSponsorButton").addEventListener("click", newSponsor); byId("sponsorForm").addEventListener("submit", saveSponsor);
  byId("newAgreementButton").addEventListener("click", newAgreement); byId("agreementForm").addEventListener("submit", saveAgreement); byId("paymentForm").addEventListener("submit", savePayment);
  byId("rateForm").addEventListener("submit", saveRate);
  const current = new Date(); byId("toDate").value = todayInput(current); current.setDate(current.getDate() - 6); byId("fromDate").value = todayInput(current); byId("paymentReceivedAt").value = utcInput(new Date().toISOString()); byId("rateEffectiveFrom").value = utcInput(new Date(Date.now() + 60000).toISOString());
  checkSession();
})();
