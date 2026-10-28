// Purpose: Exposes anonymous VIVOICE read APIs for map, narration, ads and privacy-minimal session heartbeats.
// Responsibility: Delegates data access to services and never exposes admin or database-only fields.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vivoice.Web.Models;
using Vivoice.Web.Services;

namespace Vivoice.Web.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController(PublicContentService content, AnalyticsService analytics, VisitorSessionCookieService visitorSessions) : ControllerBase
{
    [HttpGet("pois")]
    public Task<IReadOnlyList<PoiCardDto>> GetPois([FromQuery] string? lang = "vi", CancellationToken cancellationToken = default) =>
        content.GetPoisAsync(NormalizeLanguage(lang), cancellationToken);

    [HttpGet("pois/{poiId:long}/topics")]
    public async Task<ActionResult<IReadOnlyList<TopicDto>>> GetTopics(long poiId, [FromQuery] string? lang = "vi", CancellationToken cancellationToken = default)
    {
        var result = await content.GetTopicsAsync(poiId, NormalizeLanguage(lang), cancellationToken);
        return result is null ? NotFound(new { code = "POI_NOT_FOUND", message = "Không tìm thấy địa điểm đang hiển thị." }) : Ok(result);
    }

    [HttpGet("topics/{topicId:long}/narration")]
    public async Task<ActionResult<NarrationDto>> GetNarration(long topicId, [FromQuery] string? lang = "vi", CancellationToken cancellationToken = default)
    {
        var result = await content.GetNarrationAsync(topicId, NormalizeLanguage(lang), cancellationToken);
        return result is null ? NotFound(new { code = "NARRATION_NOT_FOUND", message = "Nội dung chưa sẵn sàng." }) : Ok(result);
    }

    [HttpGet("advertisements")]
    public async Task<ActionResult<IReadOnlyList<AdvertisementDto>>> GetAdvertisements([FromQuery] string? placement, CancellationToken cancellationToken)
    {
        if (!PublicRequestValidation.IsAdvertisementPlacement(placement)) return BadRequest(new { code = "INVALID_PLACEMENT", message = "Vị trí quảng cáo phải là LEFT hoặc RIGHT." });
        return Ok(await content.GetAdvertisementsAsync(placement!, DateTime.UtcNow, cancellationToken));
    }

    [EnableRateLimiting("public-telemetry")]
    [HttpPost("sessions")]
    public IActionResult IssueVisitorSession()
    {
        Response.Cookies.Append(VisitorSessionCookieService.CookieName, visitorSessions.Issue(), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Path = "/",
            SameSite = SameSiteMode.Strict,
            Secure = Request.IsHttps
        });
        return NoContent();
    }

    [EnableRateLimiting("public-telemetry")]
    [HttpPost("sessions/heartbeat")]
    public async Task<ActionResult<HeartbeatResponse>> Heartbeat(CancellationToken cancellationToken)
    {
        if (!TryGetServerIssuedSessionId(out var sessionId)) return BadRequest(new { code = "SESSION_REQUIRED", message = "Phiên truy cập hợp lệ là bắt buộc." });
        var result = await analytics.RecordHeartbeatAsync(sessionId, Request.Headers.UserAgent.ToString(), cancellationToken);
        return result is null
            ? BadRequest(new { code = "INVALID_SESSION", message = "Phiên truy cập không hợp lệ." })
            : Ok(result);
    }

    [EnableRateLimiting("public-telemetry")]
    [HttpPost("advertisements/{advertisementId:long}/events")]
    public async Task<IActionResult> RecordAdvertisementEvent(long advertisementId, AdvertisementEventRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.EventType)) return BadRequest(new { code = "INVALID_AD_EVENT", message = "Sự kiện quảng cáo không hợp lệ." });
        TryGetServerIssuedSessionId(out var sessionId);
        var recorded = await analytics.RecordAdvertisementEventAsync(advertisementId, request.EventType, sessionId, cancellationToken);
        return recorded ? NoContent() : BadRequest(new { code = "INVALID_AD_EVENT", message = "Sự kiện quảng cáo không hợp lệ." });
    }

    private static string NormalizeLanguage(string? languageCode) => languageCode?.Trim().ToLowerInvariant() switch
    {
        "vi" or "ko" or "ja" or "zh" or "th" => languageCode.Trim().ToLowerInvariant(),
        _ => "vi"
    };

    private bool TryGetServerIssuedSessionId(out string sessionId) =>
        visitorSessions.TryRead(Request.Cookies[VisitorSessionCookieService.CookieName], out sessionId);
}

public static class PublicRequestValidation
{
    public static bool IsAdvertisementPlacement(string? placement) => placement?.Trim().ToUpperInvariant() is "LEFT" or "RIGHT";
}
