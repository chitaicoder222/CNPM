// Purpose: Maps dashboard API data to the small set of operator-facing metrics.
// Responsibility: Keeps visit, active-device, campaign-revenue and legacy-revenue labels deterministic and testable without a browser or database.
(() => {
  function finiteNumber(value) {
    const number = Number(value);
    return Number.isFinite(number) ? number : null;
  }

  function buildDashboardMetrics(data, { count, money }) {
    if (typeof count !== "function" || typeof money !== "function") throw new TypeError("Dashboard formatters are required.");
    return Object.freeze([
      Object.freeze({ label: "Lượt truy cập", value: count(data?.visitCount) }),
      Object.freeze({ label: "Thiết bị đang hoạt động", value: count(data?.activeDeviceCount) }),
      Object.freeze({ label: "Doanh thu lượt truy cập (ước tính)", value: money(data?.visitRevenueEstimate, data?.currency) }),
      Object.freeze({ label: "Cam kết cố định", value: money(data?.contractedRevenue, data?.currency) }),
      Object.freeze({ label: "Đã nhận (hợp đồng cố định)", value: money(data?.receivedRevenue, data?.currency) }),
      Object.freeze({ label: "Còn phải thu (hợp đồng cố định)", value: money(data?.outstandingRevenue, data?.currency) }),
      Object.freeze({ label: "Đối tác hoạt động", value: count(data?.activeSponsors) })
    ]);
  }

  function describeVisitRevenue(data, count) {
    if (typeof count !== "function") throw new TypeError("A count formatter is required.");
    const unpricedVisitCount = finiteNumber(data?.unpricedVisitCount);
    if (unpricedVisitCount === null) return "Chưa xác định được các lượt chưa có mức giá.";
    const sponsoredVisitRevenueEstimate = finiteNumber(data?.sponsoredVisitRevenueEstimate) ?? 0;
    const legacyVisitRevenueEstimate = finiteNumber(data?.legacyVisitRevenueEstimate) ?? 0;
    const messages = [];
    if (sponsoredVisitRevenueEstimate > 0) messages.push("Doanh thu campaign được chốt theo giá tại thời điểm từng lượt truy cập phát sinh; tiền đã nhận/còn phải thu bên trên chỉ áp dụng cho hợp đồng cố định.");
    if (legacyVisitRevenueEstimate > 0) messages.push("Một phần là lịch sử giá toàn cục cũ, chưa gắn với nhà tài trợ nào.");
    if (unpricedVisitCount > 0) messages.push(`${count(unpricedVisitCount)} lượt truy cập chưa có giá tại thời điểm ghi nhận nên chưa được cộng vào doanh thu ước tính.`);
    return messages.length ? messages.join(" ") : "Mọi lượt truy cập trong khoảng lọc đã có giá hoặc chưa phát sinh doanh thu campaign.";
  }

  const api = Object.freeze({ buildDashboardMetrics, describeVisitRevenue });
  if (typeof module !== "undefined" && module.exports) module.exports = api;
  globalThis.VivoiceAdminDashboardMetrics = api;
})();
