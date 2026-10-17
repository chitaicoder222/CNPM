using TourGuide.Api.Data.Entities;
using TourGuide.Api.Repositories;

namespace TourGuide.Api.Services;

public sealed class PlaceService(IPlaceRepository repository)
{
    public Task<IReadOnlyList<Place>> ListAsync(string? query, CancellationToken ct) => repository.ListAsync(query, ct);
    public Task<Place?> GetAsync(long id, CancellationToken ct) => repository.GetAsync(id, ct);
    public Task<IReadOnlyList<NarrationContent>> ListContentsAsync(long placeId, CancellationToken ct) => repository.ListContentsAsync(placeId, ct);
    public async Task<Place> CreateAsync(string name, string? description, string? city, CancellationToken ct)
    {
        var place = new Place { Name = name.Trim(), Description = description?.Trim() ?? "", City = city?.Trim() };
        await repository.AddAsync(place, ct); await repository.SaveAsync(ct); return place;
    }
    public async Task<bool> UpdateAsync(long id, string name, string? description, string? city, CancellationToken ct)
    {
        var place = await repository.GetAsync(id, ct); if (place is null) return false;
        place.Name = name.Trim(); place.Description = description?.Trim() ?? ""; place.City = city?.Trim(); place.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct); return true;
    }
    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var place = await repository.GetAsync(id, ct); if (place is null) return false;
        await repository.DeleteAsync(place, ct); await repository.SaveAsync(ct); return true;
    }
}
