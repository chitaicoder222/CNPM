// Purpose: Exposes anonymous VIVOICE read APIs for map, narration, ads and privacy-minimal session heartbeats.
// Responsibility: Delegates data access to services and never exposes admin or database-only fields.
using Microsoft.AspNetCore.Mvc;
using Vivoice.Web.Models;
using Vivoice.Web.Services;

namespace Vivoice.Web.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController(PublicContentService content, AnalyticsService analytics) : ControllerBase
{
    [HttpGet("pois")]
    public Task<IReadOnlyList<PoiCardDto>> GetPois([FromQuery] string lang = "vi", CancellationToken cancellationToken = default) =>
        content.GetPoisAsync(NormalizeLanguage(lang), cancellationToken);

    [HttpGet("pois/{poiId:long}/topics")]
    public async Task<ActionResult<IReadOnlyList<TopicDto>>> GetTopics(long poiId, [FromQuery] string lang = "vi", CancellationToken cancellationToken = default)
    {
        var result = await content.GetTopicsAsync(poiId, NormalizeLanguage(lang), cancellationToken);
        return result is null ? NotFound(new { code = "POI_NOT_FOUND", message = "Không tìm thấy địa điểm đang hiển thị." }) : Ok(result);
    }

    [HttpGet("topics/{topicId:long}/narration")]
    public async Task<ActionResult<NarrationDto>> GetNarration(long topicId, [FromQuery] string lang = "vi", CancellationToken cancellationToken = default)
    {
        var result = await content.GetNarrationAsync(topicId, NormalizeLanguage(lang), cancellationToken);
        return result is null ? NotFound(new { code = "NARRATION_NOT_FOUND", message = "Nội dung chưa sẵn sàng." }) : Ok(result);
    }

    [HttpGet("advertisements")]
    public Task<IReadOnlyList<AdvertisementDto>> GetAdvertisements([FromQuery] string placement, CancellationToken cancellationToken) =>
        content.GetAdvertisementsAsync(placement, DateTime.UtcNow, cancellationToken);

    [HttpPost("sessions/heartbeat")]
    public async Task<ActionResult<HeartbeatResponse>> Heartbeat(HeartbeatRequest request, CancellationToken cancellationToken)
    {
        var result = await analytics.RecordHeartbeatAsync(request.SessionId, Request.Headers.UserAgent.ToString(), cancellationToken);
        return result is null
            ? BadRequest(new { code = "INVALID_SESSION", message = "Mã phiên không hợp lệ." })
            : Ok(result);
    }

    [HttpPost("advertisements/{advertisementId:long}/events")]
    public async Task<IActionResult> RecordAdvertisementEvent(long advertisementId, AdvertisementEventRequest request, CancellationToken cancellationToken)
    {
        var recorded = await analytics.RecordAdvertisementEventAsync(advertisementId, request.EventType, request.SessionId, cancellationToken);
        return recorded ? NoContent() : BadRequest(new { code = "INVALID_AD_EVENT", message = "Sự kiện quảng cáo không hợp lệ." });
    }

    private static string NormalizeLanguage(string languageCode) => languageCode.Trim().ToLowerInvariant() switch
    {
        "vi" or "ko" or "ja" or "zh" or "th" => languageCode.Trim().ToLowerInvariant(),
        _ => "vi"
    };
}
