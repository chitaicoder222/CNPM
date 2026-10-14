using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;
using TourGuide.Api.Services;

namespace TourGuide.Api.Controllers;

// TODO(auth): staff chỉ xem job của mình; retry chỉ Admin.
[ApiController]
[Route("jobs")]
public class JobsController(IntegrationDbContext db, JobService service) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return job == null ? NotFound() : Ok(job);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    {
        var query = db.Jobs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        return Ok(await query.OrderByDescending(x => x.Id).Take(100).ToListAsync(ct));
    }

    [HttpPost("{id:long}/retry")]
    public async Task<IActionResult> Retry(long id, CancellationToken ct) =>
        await service.RetryAsync(id, ct)
            ? Ok(new { message = "Job queued" })
            : BadRequest("Job not found or not failed");
}
