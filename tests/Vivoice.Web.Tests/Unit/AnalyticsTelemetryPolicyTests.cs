// Purpose: Verifies analytics policies that prevent Admin and automated traffic from becoming public visitor metrics.
// Responsibility: Exercises pure telemetry, time-window, reporting-range and public-demo access rules without SQL Server.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vivoice.Web.Data;
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Unit;

public sealed class AnalyticsTelemetryPolicyTests
{
    [Theory]
    [InlineData(false, "Mozilla/5.0 (Windows NT 10.0; Win64; x64)", true)]
    [InlineData(true, "Mozilla/5.0 (Windows NT 10.0; Win64; x64)", false)]
    [InlineData(false, "Googlebot/2.1", false)]
    [InlineData(false, null, false)]
    public void AnonymousTelemetry_TracksOnlyPublicNonBotRequests(bool isAuthenticated, string? userAgent, bool expected)
    {
        Assert.Equal(expected, VisitorTelemetryPolicy.ShouldRecordAnonymousTelemetry(isAuthenticated, userAgent));
        Assert.Equal(expected, VisitorTelemetryPolicy.ShouldIssueVisitorSession(isAuthenticated, userAgent));
    }

    [Theory]
    [InlineData(null, 300)]
    [InlineData(10, 30)]
    [InlineData(300, 300)]
    [InlineData(1200, 900)]
    public void ActiveWindow_IsNormalizedOnceForEveryAnalyticsConsumer(int? configuredSeconds, int expected)
    {
        Assert.Equal(expected, AnalyticsWindowPolicy.Normalize(configuredSeconds));
    }

    [Theory]
    [InlineData("/admin.html", true)]
    [InlineData("/admin.js", true)]
    [InlineData("/api/admin/dashboard", true)]
    [InlineData("/api/admin", true)]
    [InlineData("/api/public/pois", false)]
    [InlineData("/administer", false)]
    public void PublicOnlyMode_BlocksOnlyAdminSurface(string path, bool expected)
    {
        Assert.Equal(expected, PublicDemoAccessPolicy.IsAdminSurface(path));
    }

    [Fact]
    public void DashboardRange_RejectsAnInvertedTimeWindow()
    {
        var start = new DateTime(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(-1);

        Assert.False(DashboardRangePolicy.IsValid(start, end));
        Assert.True(DashboardRangePolicy.IsValid(end, start));
    }

    [Fact]
    public async Task LikelyBotHeartbeat_ReturnsWithoutCreatingTrackedAnalyticsEntities()
    {
        var options = new DbContextOptionsBuilder<VivoiceDbContext>().Options;
        await using var db = new VivoiceDbContext(options);
        var service = new AnalyticsService(db, new ConfigurationBuilder().Build());

        var result = await service.RecordHeartbeatAsync(Guid.NewGuid().ToString("N"), "Googlebot/2.1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.VisitRecorded);
    }
}
