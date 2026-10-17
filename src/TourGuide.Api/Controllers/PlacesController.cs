using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data.Entities;
using TourGuide.Api.Services;

namespace TourGuide.Api.Controllers;

public record PlaceRequest([Required, MaxLength(200)] string Name, string? Description, [MaxLength(120)] string? City);

[ApiController]
[Route("places")]
public class PlacesController(PlaceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q, CancellationToken ct)
    {
        return Ok(await service.ListAsync(q, ct));
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

    [HttpPost]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Create(PlaceRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var place = await service.CreateAsync(request.Name, request.Description, request.City, ct);
        return CreatedAtAction(nameof(Get), new { id = place.Id }, place);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> Update(long id, PlaceRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!await service.UpdateAsync(id, request.Name, request.Description, request.City, ct)) return NotFound();
        return Ok(await service.GetAsync(id, ct));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        return await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
