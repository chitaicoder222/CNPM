using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Repositories;

public interface IPlaceRepository
{
    Task<IReadOnlyList<Place>> ListAsync(string? query, bool includeInactive, CancellationToken ct);
    Task<Place?> GetAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<NarrationContent>> ListContentsAsync(long placeId, CancellationToken ct);
    Task<NarrationContent?> GetContentAsync(long placeId, long contentId, CancellationToken ct);
    Task AddContentAsync(NarrationContent content, CancellationToken ct);
    Task AddAsync(Place place, CancellationToken ct);
    Task DeleteAsync(Place place, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}
