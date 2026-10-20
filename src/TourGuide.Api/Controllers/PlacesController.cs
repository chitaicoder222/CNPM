using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data.Entities;
using TourGuide.Api.Services;

namespace TourGuide.Api.Controllers;

public record PlaceRequest([Required, MaxLength(200)] string Name, string? Description, [MaxLength(120)] string? City, [MaxLength(80)] string? Category, [MaxLength(240)] string? Address, decimal? Latitude, decimal? Longitude, [MaxLength(500)] string? ImageUrl, bool IsActive = true);
public record NarrationContentRequest([Required, MaxLength(200)] string Title, [Required, MaxLength(80)] string Topic, [MaxLength(20)] string? ContentType, [Required] string SourceText);

[ApiController]
[Route("places")]
public class PlacesController(PlaceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        if (includeInactive && !User.IsInRole("admin")) return Forbid();
        return Ok(await service.ListAsync(q, includeInactive, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct) =>
        (await service.GetAsync(id, ct)) is { } place ? Ok(place) : NotFound();

    [HttpGet("{id:long}/contents")]
    public async Task<IActionResult> Contents(long id, CancellationToken ct)
    {
        if (await service.GetAsync(id, ct) is null) return NotFound();
        return Ok(await service.ListContentsAsync(id, ct));
    }

    [HttpPost("{id:long}/contents")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateContent(long id, NarrationContentRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var content = await service.CreateContentAsync(id, request.Title, request.Topic, request.ContentType ?? "topic", request.SourceText, ct);
        return content is null ? NotFound() : CreatedAtAction(nameof(Contents), new { id }, content);
    }

    [HttpPut("{id:long}/contents/{contentId:long}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateContent(long id, long contentId, NarrationContentRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        return await service.UpdateContentAsync(id, contentId, request.Title, request.Topic, request.ContentType ?? "topic", request.SourceText, ct) ? NoContent() : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Create(PlaceRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var place = await service.CreateAsync(request.Name, request.Description, request.City, request.Category, request.Address, request.Latitude, request.Longitude, request.ImageUrl, ct);
        return CreatedAtAction(nameof(Get), new { id = place.Id }, place);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Update(long id, PlaceRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!await service.UpdateAsync(id, request.Name, request.Description, request.City, request.Category, request.Address, request.Latitude, request.Longitude, request.ImageUrl, request.IsActive, ct)) return NotFound();
        return Ok(await service.GetAsync(id, ct));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        return await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
