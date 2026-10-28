// Purpose: Defines the boundary between anonymous public telemetry and authenticated or automated traffic.
// Responsibility: Keeps Admin, health-like clients and likely bots out of visitor-session analytics before persistence.
namespace Vivoice.Web.Services;

public static class VisitorTelemetryPolicy
{
    public static bool ShouldIssueVisitorSession(bool isAuthenticated, string? userAgent) =>
        !isAuthenticated && !DeviceClassifier.Describe(userAgent).IsLikelyBot;

    public static bool ShouldRecordAnonymousTelemetry(bool isAuthenticated, string? userAgent) =>
        ShouldIssueVisitorSession(isAuthenticated, userAgent);
}
