// Purpose: Verifies the Admin dashboard uses clear non-duplicated visitor metrics.
// Responsibility: Covers card labels and transparent handling of campaign-attributed, legacy and unpriced visits.
const test = require("node:test");
const assert = require("node:assert/strict");
const { buildDashboardMetrics, describeVisitRevenue } = require("../../../src/Vivoice.Web/wwwroot/admin-dashboard-metrics.js");

const count = value => String(value);
const money = value => `$${Number(value || 0).toFixed(2)}`;

test("dashboard shows visits and active devices without duplicate session cards", () => {
  const metrics = buildDashboardMetrics({
    visitCount: 31,
    activeDeviceCount: 1,
    visitRevenueEstimate: 2.8,
    sponsoredVisitRevenueEstimate: 1.5,
    legacyVisitRevenueEstimate: 1.3,
    contractedRevenue: 0,
    receivedRevenue: 0,
    outstandingRevenue: 0,
    activeSponsors: 0,
    currency: "USD"
  }, { count, money });

  assert.deepEqual(metrics.map(metric => metric.label), [
    "Lượt truy cập",
    "Thiết bị đang hoạt động",
    "Doanh thu lượt truy cập (ước tính)",
    "Cam kết cố định",
    "Đã nhận (hợp đồng cố định)",
    "Còn phải thu (hợp đồng cố định)",
    "Đối tác hoạt động"
  ]);
  assert.equal(metrics[0].value, "31");
  assert.equal(metrics[1].value, "1");
  assert.equal(metrics.some(metric => /Khách duy nhất|Phiên đang hoạt động|Lượt hợp lệ/.test(metric.label)), false);
});

test("dashboard explains campaign attribution, legacy revenue and visits without a price truthfully", () => {
  assert.equal(
    describeVisitRevenue({ sponsoredVisitRevenueEstimate: 1.5, legacyVisitRevenueEstimate: 1.3, unpricedVisitCount: 3 }, count),
    "Doanh thu campaign được chốt theo giá tại thời điểm từng lượt truy cập phát sinh; tiền đã nhận/còn phải thu bên trên chỉ áp dụng cho hợp đồng cố định. Một phần là lịch sử giá toàn cục cũ, chưa gắn với nhà tài trợ nào. 3 lượt truy cập chưa có giá tại thời điểm ghi nhận nên chưa được cộng vào doanh thu ước tính."
  );
  assert.equal(
    describeVisitRevenue({ sponsoredVisitRevenueEstimate: 0, legacyVisitRevenueEstimate: 0, unpricedVisitCount: 0 }, count),
    "Mọi lượt truy cập trong khoảng lọc đã có giá hoặc chưa phát sinh doanh thu campaign."
  );
});
