// Purpose: Verifies privacy-minimal browser user-agent classification for active-device dashboard statistics.
// Responsibility: Keeps phone/tablet/desktop grouping deterministic without device fingerprinting.
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Unit;

public sealed class DeviceClassifierTests
{
    [Theory]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) Mobile", "Phone")]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X)", "Tablet")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64)", "Desktop")]
    public void FromUserAgent_ReturnsCoarseCategory(string userAgent, string expected) => Assert.Equal(expected, DeviceClassifier.FromUserAgent(userAgent));
}
