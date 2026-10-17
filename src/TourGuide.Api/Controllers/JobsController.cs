using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;
using TourGuide.Api.Services;
using TourGuide.Api.Repositories;

namespace TourGuide.Api.Controllers;

[ApiController]
[Route("jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _repository;
    private readonly JobService _service;
    private readonly IWebHostEnvironment _env;

    public JobsController(IJobRepository repository, JobService service, IWebHostEnvironment env)
    {
        _repository = repository;
        _service = service;
        _env = env;
    }

    // TODO(auth): staff chỉ xem job của mình; retry chỉ Admin.
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var job = await _repository.GetByIdAsync(id, ct);
        if (job == null) return NotFound();
        if (job.Type == "translate")
        {
            var tts = await _repository.GetTtsChildAsync(id, ct);
            var workflowStatus = job.Status == "failed" ? "failed"
                : tts?.Status == "failed" ? "failed"
                : tts?.Status == "done" && !string.IsNullOrWhiteSpace(job.AudioPath) ? "done"
                : tts?.Status ?? "processing";
            return Ok(new { job.Id, job.ParentJobId, job.PlaceId, job.TargetLanguage, job.SourceText, job.SourceHash, job.Type, status = workflowStatus, translationStatus = job.Status, job.RetryCount, job.RunAfter, job.StartedAt, job.LastError, job.Result, job.AudioPath, job.CreatedAt, ttsStatus = tts?.Status, ttsError = tts?.LastError, ttsJobId = tts?.Id });
        }
        return Ok(job);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    {
        var jobs = await _repository.ListAsync(ct);
        if (!string.IsNullOrWhiteSpace(status))
            jobs = jobs.Where(x => x.Status == status).ToList();
        return Ok(jobs.OrderByDescending(x => x.Id).Take(100));
    }

    [HttpGet("{id:long}/audio")]
    public async Task<IActionResult> Audio(long id, CancellationToken ct)
    {
        var job = await _repository.GetByIdAsync(id, ct);
        if (job is null || string.IsNullOrWhiteSpace(job.AudioPath)) return NotFound();
        // Resolve against the same content root used by TtsService/static files.
        var configured = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Storage:Root"] ?? "storage";
        var root = Path.GetFullPath(configured, _env.ContentRootPath);
        var path = Path.GetFullPath(Path.Combine(root, job.AudioPath.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return BadRequest("Invalid audio path");
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, "audio/mpeg", enableRangeProcessing: true);
    }

    [HttpPost("{id:long}/retry")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Retry(long id, CancellationToken ct) =>
        await _service.RetryAsync(id, ct)
            ? Ok(new { message = "Job queued" })
            : BadRequest("Job not found or not failed");
}
