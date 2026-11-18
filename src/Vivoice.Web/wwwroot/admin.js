// Purpose: Operates the protected VIVOICE admin console using only authenticated SQL-backed API responses.
// Responsibility: Manages POIs, translations, ads, sponsor campaigns, append-only campaign prices and truthful dashboard data without fixtures.
(() => {
  const byId = id => document.getElementById(id);
  const state = { pois: [], selectedPoi: null, topics: [], selectedTopic: null, poiTranslations: [], topicTranslations: [], sponsors: [], ads: [], agreements: [] };
  function ensurePoiRadiusControl() {
    const longitudeField = byId("poiLongitude")?.closest(".field");
    if (longitudeField && !byId("poiTriggerRadius")) longitudeField.insertAdjacentHTML("afterend", '<div class="field"><label for="poiTriggerRadius">Bán kính tự phát (m)</label><input id="poiTriggerRadius" type="number" min="1" step="1" value="100" required aria-describedby="poiRadiusHelp"></div><p id="poiRadiusHelp" class="status-line">Phải là số nguyên lớn hơn 0. Thuyết minh tự phát khi GPS vào bán kính này.</p>');
    document.querySelectorAll('#poiTranslationLanguage option[value="th"], #topicTranslationLanguage option[value="th"]').forEach(option => { option.value = "en"; option.textContent = "English"; });
  }
  ensurePoiRadiusControl();
  let antiforgeryToken;
  const dashboardRefreshIntervalMs = 10_000;
  let dashboardRefreshController = null;
  const dashboardMetrics = window.VivoiceAdminDashboardMetrics;
  const adminDateInputs = window.VivoiceAdminDateInputs;
  if (!dashboardMetrics) throw new Error("Dashboard metric mapping was not loaded.");
  if (!adminDateInputs) throw new Error("Admin date-input helpers were not loaded.");

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
  function finiteNumber(value) { const number = Number(value); return Number.isFinite(number) ? number : null; }
  function count(value) { const number = finiteNumber(value); return number === null ? "—" : new Intl.NumberFormat("vi-VN").format(number); }
  function money(value, currency = "USD") {
    const number = finiteNumber(value);
    if (number === null) return "—";
    try { return new Intl.NumberFormat("en-US", { style: "currency", currency: currency || "USD", maximumFractionDigits: 2 }).format(number); }
    catch { return `${number.toFixed(2)} ${currency || "USD"}`; }
  }
  function localDateTime(value) { const date = value ? new Date(value) : null; return date && !Number.isNaN(date.getTime()) ? date.toLocaleString("vi-VN") : "Không xác định"; }
  function localDateTimeInput(value) { return adminDateInputs.toLocalDateTimeInput(value); }
  function inputUtc(id) { const value = byId(id).value; return value ? new Date(value).toISOString() : null; }
  function localDateInput(date) { return adminDateInputs.toLocalDateInput(date); }
  function buttonList(hostId, values, makeLabel, select) {
    const host = byId(hostId); host.replaceChildren();
    values.forEach(value => { const button = document.createElement("button"); button.type = "button"; button.className = "list-button"; const content = makeLabel(value); button.append(content); button.addEventListener("click", () => select(value)); host.append(button); });
    if (!values.length) host.textContent = "Chưa có dữ liệu.";
  }
  function option(selectId, value, label, selected = false) { const item = document.createElement("option"); item.value = String(value); item.textContent = label; item.selected = selected; byId(selectId).append(item); }

  async function checkSession() { await showAdmin(); }
  function stopDashboardPolling() {
    dashboardRefreshController?.stop();
  }
  function startDashboardPolling() {
    dashboardRefreshController?.start();
  }
  function showLogin() {
    stopDashboardPolling();
    byId("loginView").classList.remove("hidden");
    byId("adminView").classList.add("hidden");
  }
  async function showAdmin() {
    byId("loginView").classList.add("hidden"); byId("adminView").classList.remove("hidden");
    await Promise.all([refreshDashboard({ reason: "initial" }), loadPois(), loadSponsors(), loadAdvertisements(), loadAgreements()]);
    if (!state.selectedPoi && state.pois.length) await selectPoi(state.pois[0]);
    startDashboardPolling();
  }

  function renderDashboard(data) {
    const values = dashboardMetrics.buildDashboardMetrics(data, { count, money });
    const metrics = byId("metrics"); metrics.replaceChildren();
    values.forEach(({ label, value }) => { const card = document.createElement("div"); card.className = "metric"; const name = document.createElement("span"); name.textContent = label; const number = document.createElement("strong"); number.textContent = String(value); card.append(name, number); metrics.append(card); });
    writeStatus("visitRevenueStatus", dashboardMetrics.describeVisitRevenue(data, count));
    buttonList("visitTrend", Array.isArray(data.visitsByDate) ? data.visitsByDate : [], item => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = item.date || "Không xác định"; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${count(item.visits)} lượt truy cập`; fragment.append(title, note); return fragment; }, () => undefined);
    byId("adMetrics").textContent = `Hiển thị: ${count(data.advertisementImpressions)} · Nhấp: ${count(data.advertisementClicks)}. Dữ liệu tính trong khoảng lọc.`;
    renderCampaignRevenue(data);
  }
  function renderCampaignRevenue(data) {
    const host = byId("campaignRevenueBreakdown");
    const rows = Array.isArray(data?.campaignRevenue) ? data.campaignRevenue : [];
    host.replaceChildren();
    if (!rows.length) {
      byId("campaignRevenueHint").textContent = "Chưa có campaign nào nhận doanh thu traffic trong khoảng lọc. Dữ liệu giá cũ, nếu có, được tách riêng ở ghi chú doanh thu.";
      return;
    }
    byId("campaignRevenueHint").textContent = `${count(rows.length)} campaign được hiển thị theo dữ liệu SQL trong khoảng lọc.`;
    const table = document.createElement("table");
    const head = document.createElement("thead");
    const headRow = document.createElement("tr");
    ["Nhà tài trợ", "Campaign", "Giá/lượt hiện tại", "Lượt trong kỳ", "Doanh thu traffic ước tính", "Trạng thái"].forEach(label => { const cell = document.createElement("th"); cell.scope = "col"; cell.textContent = label; headRow.append(cell); });
    head.append(headRow); table.append(head);
    const body = document.createElement("tbody");
    rows.forEach(row => {
      const tr = document.createElement("tr");
      const hasRate = Boolean(row.currentRateEnabled) && finiteNumber(row.currentRateAmount) !== null && Number(row.currentRateAmount) > 0;
      const values = [
        row.sponsorName || "Không xác định",
        row.campaignName || "Không xác định",
        hasRate ? `${money(row.currentRateAmount, row.currency)} / lượt` : "Tắt hoặc chưa có giá hiệu lực",
        count(row.visitCount),
        money(row.estimatedRevenue, row.currency),
        row.status || "Không xác định"
      ];
      values.forEach(value => { const cell = document.createElement("td"); cell.textContent = String(value); tr.append(cell); });
      body.append(tr);
    });
    table.append(body); host.append(table);
  }
  function dashboardRange() {
    const fromDate = byId("fromDate").value;
    const toDate = byId("toDate").value;
    const from = new Date(`${fromDate}T00:00:00Z`);
    const to = new Date(`${toDate}T23:59:59Z`);
    if (Number.isNaN(from.getTime()) || Number.isNaN(to.getTime())) throw new Error("Hãy chọn đầy đủ khoảng ngày hợp lệ.");
    if (from > to) throw new Error("Ngày bắt đầu không thể sau ngày kết thúc.");
    return { from: from.toISOString(), to: to.toISOString() };
  }
  async function refreshDashboardData(reason = "manual") {
    const refreshButton = byId("dashboardRefreshButton");
    refreshButton.disabled = true;
    writeStatus("dashboardRefreshStatus", reason === "auto" ? "Đang tự cập nhật…" : "Đang cập nhật…");
    try {
      const { from, to } = dashboardRange();
      const dashboard = await api(`/api/admin/dashboard?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`);
      renderDashboard(dashboard);
      writeStatus("dashboardRefreshStatus", `Đã cập nhật ${localDateTime(new Date().toISOString())}. Tự làm mới mỗi 10 giây khi trang đang mở.`, "success");
      return true;
    } catch (error) {
      writeStatus("dashboardRefreshStatus", `Không thể cập nhật dashboard: ${error.message}`, "error");
      return false;
    } finally {
      refreshButton.disabled = false;
    }
  }
  function refreshDashboard({ reason = "manual" } = {}) {
    return dashboardRefreshController ? dashboardRefreshController.refreshNow(reason) : refreshDashboardData(reason);
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
    byId("poiId").value = poi.id; byId("poiSlug").value = poi.slug; byId("poiOrder").value = poi.displayOrder; byId("poiLatitude").value = poi.latitude; byId("poiLongitude").value = poi.longitude; byId("poiTriggerRadius").value = poi.triggerRadiusMeters; byId("poiImageUrl").value = poi.imageUrl || ""; byId("poiName").value = poi.nameVi; byId("poiSummary").value = poi.summaryVi; byId("poiActive").checked = poi.isActive;
  }
  function newPoi() {
    state.selectedPoi = null; state.selectedTopic = null; state.topics = []; state.poiTranslations = []; state.topicTranslations = [];
    byId("poiForm").reset(); byId("poiId").value = ""; byId("poiActive").checked = true; byId("topicList").replaceChildren(); byId("topicForm").classList.add("hidden"); clearTranslations(); renderPois(); writeStatus("poiStatus", "Nhập thông tin để tạo POI mới.");
  }
  async function savePoi(event) {
    event.preventDefault();
    const id = byId("poiId").value;
    const triggerRadiusMeters = Number(byId("poiTriggerRadius").value);
    if (!Number.isInteger(triggerRadiusMeters) || triggerRadiusMeters <= 0) { writeStatus("poiStatus", "Bán kính tự phát phải là số nguyên lớn hơn 0 mét.", "error"); return; }
    const body = { slug: byId("poiSlug").value, displayOrder: Number(byId("poiOrder").value), latitude: Number(byId("poiLatitude").value), longitude: Number(byId("poiLongitude").value), triggerRadiusMeters, imageUrl: byId("poiImageUrl").value || null, nameVi: byId("poiName").value, summaryVi: byId("poiSummary").value, isActive: byId("poiActive").checked };
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

  function refillSelect(id, items, makeLabel, { includeBlank = false, blankLabel = "" } = {}) {
    const select = byId(id); const previous = select.value; select.replaceChildren();
    if (includeBlank) option(id, "", blankLabel);
    items.forEach(item => option(id, item.id, makeLabel(item)));
    if ([...select.options].some(item => item.value === previous)) select.value = previous;
  }
  function refreshSponsorOptions() {
    refillSelect("adSponsor", state.sponsors, sponsor => sponsor.name, { includeBlank: true, blankLabel: "Không liên kết" });
    refillSelect("agreementSponsor", state.sponsors, sponsor => sponsor.name);
    const fixedAgreements = state.agreements.filter(agreement => agreement.billingMode === "fixed" && agreement.status !== "cancelled");
    refillSelect("paymentAgreement", fixedAgreements, agreement => `${agreement.sponsorName} — ${agreement.campaignName || "Campaign"}`, { includeBlank: true, blankLabel: fixedAgreements.length ? "Chọn hợp đồng cố định" : "Chưa có hợp đồng cố định" });
    byId("paymentForm").querySelector('button[type="submit"]').disabled = fixedAgreements.length === 0;
    refillSelect("ratePeriodAgreement", state.agreements.filter(agreement => agreement.billingMode === "per_visit"), agreement => `${agreement.sponsorName} — ${agreement.campaignName || "Campaign"}`);
  }
  async function loadSponsors() {
    state.sponsors = await api("/api/admin/sponsors"); refreshSponsorOptions();
    buttonList("sponsorList", state.sponsors, sponsor => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = sponsor.name; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${sponsor.isActive ? "Đang hợp tác" : "Đang tắt"} · ${count(sponsor.agreementCount)} campaign · ${money(sponsor.contractedAmount)} cam kết cố định`; fragment.append(title, note); return fragment; }, selectSponsor);
  }
  function selectSponsor(sponsor) {
    byId("agreementSponsor").value = String(sponsor.id);
    byId("sponsorProfileId").value = String(sponsor.id); byId("sponsorProfileName").value = sponsor.name; byId("sponsorProfileContact").value = sponsor.contactNote || ""; byId("sponsorProfileActive").checked = Boolean(sponsor.isActive);
    writeStatus("agreementStatusText", `Đã chọn ${sponsor.name}; nhập thông tin campaign bên dưới.`, "success");
  }
  async function saveSponsorProfile(event) {
    event.preventDefault(); const id = byId("sponsorProfileId").value;
    if (!id) { writeStatus("sponsorProfileStatus", "Hãy chọn một nhà tài trợ trong danh sách trước.", "error"); return; }
    const body = { name: byId("sponsorProfileName").value, contactNote: byId("sponsorProfileContact").value || null, isActive: byId("sponsorProfileActive").checked };
    try { await api(`/api/admin/sponsors/${id}`, { method: "PATCH", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await Promise.all([loadSponsors(), refreshDashboard({ reason: "mutation" })]); writeStatus("sponsorProfileStatus", "Đã cập nhật hồ sơ nhà tài trợ.", "success"); } catch (error) { writeStatus("sponsorProfileStatus", error.message, "error"); }
  }
  function setRateFieldState(enabledToggleId, fieldsId, amountId, effectiveFromId, enabled) {
    const fields = byId(fieldsId); const amount = byId(amountId); const effective = byId(effectiveFromId);
    fields.classList.toggle("hidden", !enabled); amount.disabled = !enabled; effective.disabled = !enabled;
    amount.required = enabled; effective.required = enabled;
    if (!enabled) { amount.value = ""; effective.value = ""; }
  }
  function syncSponsorCampaignPricingFields() {
    const enabled = byId("sponsorCampaignPerVisit").checked;
    setRateFieldState("sponsorCampaignPerVisit", "sponsorCampaignRateFields", "sponsorCampaignRateAmount", "sponsorCampaignRateEffectiveFrom", enabled);
    byId("sponsorCampaignCommittedAmount").disabled = enabled;
    if (enabled) byId("sponsorCampaignCommittedAmount").value = "0";
  }
  function syncAgreementPricingFields() {
    const isNew = !byId("agreementId").value;
    const perVisit = byId("agreementPerVisit").checked;
    setRateFieldState("agreementPerVisit", "agreementInitialRateFields", "agreementInitialRateAmount", "agreementInitialRateEffectiveFrom", isNew && perVisit);
    byId("agreementAmount").disabled = perVisit;
    byId("agreementAmount").closest(".field")?.classList.toggle("hidden", perVisit);
    if (perVisit) byId("agreementAmount").value = "0";
  }
  function syncRatePeriodFields() {
    const enabled = byId("ratePeriodEnabled").checked;
    const amount = byId("ratePeriodAmount"); amount.disabled = !enabled; amount.required = enabled;
    if (!enabled) amount.value = "0";
  }
  function firstRatePayload(enabledId, amountId, effectiveFromId) {
    const enabled = byId(enabledId).checked;
    const amount = enabled ? Number(byId(amountId).value) : null;
    const effectiveFromUtc = enabled ? inputUtc(effectiveFromId) : null;
    if (enabled && (!Number.isFinite(amount) || amount <= 0 || !effectiveFromUtc)) throw new Error("Khi bật tính theo lượt, hãy nhập giá USD dương và thời điểm hiệu lực.");
    return { initialRateEnabled: enabled, initialRateAmount: amount, initialRateEffectiveFromUtc: effectiveFromUtc };
  }
  function resetSponsorCampaignForm() {
    byId("sponsorCampaignForm").reset(); byId("sponsorCampaignActive").checked = true; byId("sponsorCampaignPerVisit").checked = true;
    const initial = localDateTimeInput(new Date()); byId("sponsorCampaignStartsAt").value = initial; byId("sponsorCampaignRateEffectiveFrom").value = initial; byId("sponsorCampaignStatus").value = "active";
    syncSponsorCampaignPricingFields(); writeStatus("sponsorCampaignStatusText", "Nhập một nhà tài trợ và campaign đầu tiên. Giá sẽ chỉ áp dụng cho lượt phát sinh từ thời điểm hiệu lực.");
  }
  async function saveSponsorCampaign(event) {
    event.preventDefault();
    try {
      const campaign = {
        campaignName: byId("sponsorCampaignTitle").value,
        billingMode: byId("sponsorCampaignPerVisit").checked ? "per_visit" : "fixed",
        committedAmount: byId("sponsorCampaignPerVisit").checked ? 0 : Number(byId("sponsorCampaignCommittedAmount").value || 0),
        currency: "USD", status: byId("sponsorCampaignStatus").value,
        startsAtUtc: inputUtc("sponsorCampaignStartsAt"), endsAtUtc: inputUtc("sponsorCampaignEndsAt"),
        ...firstRatePayload("sponsorCampaignPerVisit", "sponsorCampaignRateAmount", "sponsorCampaignRateEffectiveFrom")
      };
      if (campaign.billingMode === "fixed" && (!Number.isFinite(campaign.committedAmount) || campaign.committedAmount <= 0)) throw new Error("Campaign cố định cần giá trị cam kết USD dương.");
      const body = { sponsorName: byId("sponsorCampaignName").value, contactNote: byId("sponsorCampaignContact").value || null, sponsorIsActive: byId("sponsorCampaignActive").checked, ...campaign };
      await api("/api/admin/sponsor-campaigns", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) });
      await Promise.all([loadSponsors(), loadAgreements(), refreshDashboard({ reason: "mutation" })]); resetSponsorCampaignForm(); writeStatus("sponsorCampaignStatusText", "Đã tạo nhà tài trợ, campaign và giá ban đầu trong một thao tác.", "success");
    } catch (error) { writeStatus("sponsorCampaignStatusText", error.message, "error"); }
  }

  async function loadAdvertisements() {
    state.ads = await api("/api/admin/advertisements");
    buttonList("adList", state.ads, ad => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = ad.name; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `${ad.placement} · ${ad.isActive ? "Bật" : "Tắt"} · ${ad.sponsorName || "Không gắn đối tác"}`; fragment.append(title, note); return fragment; }, fillAd);
  }
  function fillAd(ad) { byId("adId").value = ad.id; byId("adName").value = ad.name; byId("adSponsor").value = ad.sponsorId || ""; byId("adPlacement").value = ad.placement; byId("adOrder").value = ad.displayOrder; byId("adImageUrl").value = ad.imageUrl; byId("adTargetUrl").value = ad.targetUrl; byId("adStartsAt").value = localDateTimeInput(ad.startsAtUtc); byId("adEndsAt").value = localDateTimeInput(ad.endsAtUtc); byId("adActive").checked = ad.isActive; }
  function newAd() { byId("adForm").reset(); byId("adId").value = ""; byId("adActive").checked = true; byId("adStartsAt").value = localDateTimeInput(new Date()); }
  async function saveAd(event) {
    event.preventDefault(); const id = byId("adId").value; const sponsorValue = byId("adSponsor").value;
    const body = { sponsorId: sponsorValue ? Number(sponsorValue) : null, name: byId("adName").value, imageUrl: byId("adImageUrl").value, targetUrl: byId("adTargetUrl").value, placement: byId("adPlacement").value, startsAtUtc: inputUtc("adStartsAt"), endsAtUtc: inputUtc("adEndsAt"), displayOrder: Number(byId("adOrder").value), isActive: byId("adActive").checked };
    try { await api(`/api/admin/advertisements${id ? `/${id}` : ""}`, { method: id ? "PATCH" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await Promise.all([loadAdvertisements(), refreshDashboard({ reason: "mutation" })]); writeStatus("adStatus", "Đã lưu quảng cáo.", "success"); } catch (error) { writeStatus("adStatus", error.message, "error"); }
  }

  async function loadAgreements() {
    state.agreements = await api("/api/admin/sponsorship-agreements"); refreshSponsorOptions();
    buttonList("agreementList", state.agreements, agreement => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = `${agreement.sponsorName} — ${agreement.campaignName || "Campaign"}`; const note = document.createElement("span"); note.className = "status-line"; const rate = agreement.currentRateEnabled && finiteNumber(agreement.currentRateAmount) !== null && Number(agreement.currentRateAmount) > 0 ? `${money(agreement.currentRateAmount, agreement.currency)} / lượt` : agreement.billingMode === "per_visit" ? "giá đang tắt/chưa hiệu lực" : `${money(agreement.committedAmount, agreement.currency)} cam kết`; const settlement = agreement.billingMode === "fixed" ? `đã nhận ${money(agreement.receivedAmount, agreement.currency)}` : "doanh thu tính tự động theo lượt"; note.textContent = `${agreement.status} · ${rate} · ${settlement}`; fragment.append(title, note); return fragment; }, fillAgreement);
    const selectedRateAgreement = state.agreements.find(agreement => String(agreement.id) === byId("ratePeriodAgreement").value) || state.agreements.find(agreement => agreement.billingMode === "per_visit");
    renderRatePeriods(selectedRateAgreement);
  }
  function fillAgreement(agreement) {
    byId("agreementId").value = agreement.id; byId("agreementSponsor").value = String(agreement.sponsorId); byId("agreementName").value = agreement.campaignName || ""; byId("agreementStatus").value = agreement.status; byId("agreementPerVisit").checked = agreement.billingMode === "per_visit"; byId("agreementPerVisit").disabled = true; byId("agreementAmount").value = agreement.committedAmount; byId("agreementStartsAt").value = localDateTimeInput(agreement.startsAtUtc); byId("agreementEndsAt").value = localDateTimeInput(agreement.endsAtUtc); if (agreement.billingMode === "fixed") byId("paymentAgreement").value = String(agreement.id); if (agreement.billingMode === "per_visit") byId("ratePeriodAgreement").value = String(agreement.id); syncAgreementPricingFields(); renderRatePeriods(agreement); writeStatus("agreementStatusText", "Đổi giá theo lượt bằng biểu mẫu mức giá mới; hệ thống không ghi đè lịch sử.");
  }
  function newAgreement() {
    byId("agreementForm").reset(); byId("agreementId").value = ""; byId("agreementStatus").value = "active"; byId("agreementPerVisit").checked = true; byId("agreementPerVisit").disabled = false; const initial = localDateTimeInput(new Date()); byId("agreementStartsAt").value = initial; byId("agreementInitialRateEffectiveFrom").value = initial; syncAgreementPricingFields(); writeStatus("agreementStatusText", "Tạo campaign mới cho nhà tài trợ đã có. Giá ban đầu chỉ tác động lượt về sau.");
  }
  async function saveAgreement(event) {
    event.preventDefault(); const id = byId("agreementId").value;
    try {
      const isNew = !id;
      const perVisit = byId("agreementPerVisit").checked;
      const body = {
        sponsorId: Number(byId("agreementSponsor").value), campaignName: byId("agreementName").value, billingMode: perVisit ? "per_visit" : "fixed", committedAmount: perVisit ? 0 : Number(byId("agreementAmount").value || 0), currency: "USD", status: byId("agreementStatus").value, startsAtUtc: inputUtc("agreementStartsAt"), endsAtUtc: inputUtc("agreementEndsAt"),
        ...(isNew ? firstRatePayload("agreementPerVisit", "agreementInitialRateAmount", "agreementInitialRateEffectiveFrom") : { initialRateEnabled: false, initialRateAmount: null, initialRateEffectiveFromUtc: null })
      };
      if (!perVisit && (!Number.isFinite(body.committedAmount) || body.committedAmount <= 0)) throw new Error("Campaign cố định cần giá trị cam kết USD dương.");
      await api(`/api/admin/sponsorship-agreements${id ? `/${id}` : ""}`, { method: id ? "PATCH" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); await Promise.all([loadAgreements(), refreshDashboard({ reason: "mutation" })]); writeStatus("agreementStatusText", "Đã lưu campaign. Giá cũ không bị ghi đè.", "success");
    } catch (error) { writeStatus("agreementStatusText", error.message, "error"); }
  }
  function renderRatePeriods(agreement) {
    const periods = Array.isArray(agreement?.ratePeriods) ? agreement.ratePeriods : [];
    buttonList("ratePeriodList", periods, period => { const fragment = document.createDocumentFragment(); const title = document.createElement("strong"); title.textContent = period.isEnabled ? `${money(period.amount, period.currency)} / lượt` : "Tạm dừng giá theo lượt"; const note = document.createElement("span"); note.className = "status-line"; note.textContent = `Từ ${localDateTime(period.effectiveFromUtc)}${period.effectiveToUtc ? ` đến ${localDateTime(period.effectiveToUtc)}` : " · đang hiệu lực"}`; fragment.append(title, note); return fragment; }, () => undefined);
  }
  function renderSelectedRatePeriods() { renderRatePeriods(state.agreements.find(agreement => String(agreement.id) === byId("ratePeriodAgreement").value)); }
  async function saveRatePeriod(event) {
    event.preventDefault(); const agreementId = byId("ratePeriodAgreement").value; const isEnabled = byId("ratePeriodEnabled").checked; const amount = isEnabled ? Number(byId("ratePeriodAmount").value) : 0;
    try {
      if (!agreementId) throw new Error("Hãy chọn campaign tính theo lượt.");
      if (isEnabled && (!Number.isFinite(amount) || amount <= 0)) throw new Error("Giá USD/lượt phải lớn hơn 0 khi đang bật.");
      const body = { isEnabled, amount, currency: "USD", effectiveFromUtc: inputUtc("ratePeriodEffectiveFrom") };
      if (!body.effectiveFromUtc) throw new Error("Hãy chọn thời điểm hiệu lực.");
      await api(`/api/admin/sponsorship-agreements/${agreementId}/visit-rate-periods`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) });
      await Promise.all([loadAgreements(), refreshDashboard({ reason: "mutation" })]); byId("ratePeriodForm").reset(); byId("ratePeriodEnabled").checked = true; byId("ratePeriodEffectiveFrom").value = localDateTimeInput(new Date()); syncRatePeriodFields(); writeStatus("ratePeriodStatus", "Đã thêm mức giá theo campaign; mức cũ chỉ còn là lịch sử.", "success");
    } catch (error) { writeStatus("ratePeriodStatus", error.message, "error"); }
  }
  async function savePayment(event) {
    event.preventDefault(); const agreementId = byId("paymentAgreement").value; if (!agreementId) { writeStatus("paymentStatus", "Hãy chọn campaign hợp đồng cố định trước khi ghi nhận khoản tiền.", "error"); return; } const body = { receivedAmount: Number(byId("paymentAmount").value), receivedAtUtc: inputUtc("paymentReceivedAt") };
    try { await api(`/api/admin/sponsorship-agreements/${agreementId}/payments`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }); byId("paymentForm").reset(); await Promise.all([loadAgreements(), refreshDashboard({ reason: "mutation" })]); writeStatus("paymentStatus", "Đã ghi nhận khoản tiền; lịch sử không bị ghi đè.", "success"); } catch (error) { writeStatus("paymentStatus", error.message, "error"); }
  }


  byId("loginForm").addEventListener("submit", async event => { event.preventDefault(); try { await api("/api/admin/auth/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ displayName: byId("displayName").value, phone: byId("phone").value, password: byId("password").value }) }); antiforgeryToken = undefined; await showAdmin(); } catch (error) { writeStatus("loginStatus", error.message, "error"); } });
  byId("logoutButton").addEventListener("click", async () => {
    try { await api("/api/admin/auth/logout", { method: "POST" }); }
    catch (error) { writeStatus("dashboardRefreshStatus", `Không thể xác nhận đăng xuất: ${error.message}`, "error"); }
    finally { antiforgeryToken = undefined; showLogin(); }
  });
  byId("dashboardFilter").addEventListener("submit", event => { event.preventDefault(); void refreshDashboard({ reason: "filter" }); });
  byId("dashboardRefreshButton").addEventListener("click", () => void refreshDashboard({ reason: "manual" }));
  byId("poiSearch").addEventListener("input", renderPois); byId("newPoiButton").addEventListener("click", newPoi); byId("poiForm").addEventListener("submit", savePoi);
  byId("poiTranslationLanguage").addEventListener("change", fillPoiTranslation); byId("poiTranslationForm").addEventListener("submit", savePoiTranslation);
  byId("newTopicButton").addEventListener("click", newTopic); byId("topicForm").addEventListener("submit", saveTopic); byId("cancelTopicButton").addEventListener("click", () => byId("topicForm").classList.add("hidden"));
  byId("topicTranslationLanguage").addEventListener("change", fillTopicTranslation); byId("topicTranslationForm").addEventListener("submit", saveTopicTranslation);
  byId("newAdButton").addEventListener("click", newAd); byId("adForm").addEventListener("submit", saveAd);
  byId("newSponsorCampaignButton").addEventListener("click", resetSponsorCampaignForm); byId("sponsorProfileForm").addEventListener("submit", saveSponsorProfile); byId("sponsorCampaignForm").addEventListener("submit", saveSponsorCampaign); byId("sponsorCampaignPerVisit").addEventListener("change", syncSponsorCampaignPricingFields);
  byId("newAgreementButton").addEventListener("click", newAgreement); byId("agreementForm").addEventListener("submit", saveAgreement); byId("agreementPerVisit").addEventListener("change", syncAgreementPricingFields); byId("paymentForm").addEventListener("submit", savePayment);
  byId("ratePeriodForm").addEventListener("submit", saveRatePeriod); byId("ratePeriodEnabled").addEventListener("change", syncRatePeriodFields); byId("ratePeriodAgreement").addEventListener("change", renderSelectedRatePeriods);
  dashboardRefreshController = window.VivoiceAdminDashboardRefresh?.createDashboardRefreshController({
    intervalMs: dashboardRefreshIntervalMs,
    isHidden: () => document.hidden,
    refresh: refreshDashboardData
  });
  if (!dashboardRefreshController) throw new Error("Dashboard refresh controller was not loaded.");

  document.addEventListener("visibilitychange", () => {
    if (!dashboardRefreshController) return;
    if (document.hidden) {
      dashboardRefreshController.pause();
      if (!dashboardRefreshController.isRefreshing()) writeStatus("dashboardRefreshStatus", "Tạm dừng tự cập nhật khi tab không hiển thị.");
      return;
    }
    dashboardRefreshController.resume();
    void refreshDashboard({ reason: "resume" });
  });
  window.addEventListener("pagehide", stopDashboardPolling);
  const current = new Date(); byId("toDate").value = localDateInput(current); current.setDate(current.getDate() - 6); byId("fromDate").value = localDateInput(current); byId("paymentReceivedAt").value = localDateTimeInput(new Date()); byId("ratePeriodEffectiveFrom").value = localDateTimeInput(new Date()); resetSponsorCampaignForm(); newAgreement(); syncRatePeriodFields();
  checkSession();
})();
