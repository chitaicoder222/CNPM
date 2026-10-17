using Microsoft.AspNetCore.Mvc;
using TourGuide.Api.Services;

namespace TourGuide.Api.Controllers;

public record CreateTranslationRequest(string SourceText, List<string> Languages);

// TODO(auth): chỉ staff chủ địa điểm hoặc Admin; sau này lấy mô tả từ Place thay vì nhận sourceText.
[ApiController]
[Route("places/{placeId:long}/translate")]
public class TranslationController(JobService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(long placeId, [FromBody] CreateTranslationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SourceText) || request.Languages == null || request.Languages.Count == 0)
            return BadRequest("SourceText and Languages required");

        try
        {
            var jobs = await service.CreateAsync(placeId, request.SourceText, request.Languages, ct);
            return jobs.Count > 0 ? Accepted(jobs) : BadRequest("No job created");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
