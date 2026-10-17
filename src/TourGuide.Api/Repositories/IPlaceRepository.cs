using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Repositories;

public interface IPlaceRepository
{
    Task<IReadOnlyList<Place>> ListAsync(string? query, CancellationToken ct);
    Task<Place?> GetAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<NarrationContent>> ListContentsAsync(long placeId, CancellationToken ct);
    Task AddAsync(Place place, CancellationToken ct);
    Task DeleteAsync(Place place, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
