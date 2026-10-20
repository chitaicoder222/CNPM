using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Repositories;

public sealed class PlaceRepository(IntegrationDbContext db) : IPlaceRepository
{
    public async Task<IReadOnlyList<Place>> ListAsync(string? query, bool includeInactive, CancellationToken ct)
    {
        var places = db.Places.AsNoTracking();
        if (!includeInactive) places = places.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(query)) places = places.Where(x => x.Name.Contains(query) || (x.City != null && x.City.Contains(query)));
        return await places.OrderBy(x => x.Name).ToListAsync(ct);
    }
    public Task<Place?> GetAsync(long id, CancellationToken ct) => db.Places.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<NarrationContent>> ListContentsAsync(long placeId, CancellationToken ct) =>
        await db.NarrationContents.AsNoTracking().Where(x => x.PlaceId == placeId).OrderBy(x => x.ContentType).ThenBy(x => x.Title).ToListAsync(ct);
    public Task<NarrationContent?> GetContentAsync(long placeId, long contentId, CancellationToken ct) =>
        db.NarrationContents.FirstOrDefaultAsync(x => x.PlaceId == placeId && x.Id == contentId, ct);
    public async Task AddContentAsync(NarrationContent content, CancellationToken ct) => await db.NarrationContents.AddAsync(content, ct);
    public async Task AddAsync(Place place, CancellationToken ct) => await db.Places.AddAsync(place, ct);
    public Task DeleteAsync(Place place, CancellationToken ct) { db.Places.Remove(place); return Task.CompletedTask; }
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
