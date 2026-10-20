using TourGuide.Api.Data.Entities;
using TourGuide.Api.Repositories;

namespace TourGuide.Api.Services;

public sealed class PlaceService(IPlaceRepository repository)
{
    public Task<Place> CreateAsync(string name, string? description, string? city, CancellationToken ct) =>
        CreateAsync(name, description, city, null, null, null, null, null, ct);

    public Task<IReadOnlyList<Place>> ListAsync(string? query, bool includeInactive, CancellationToken ct) => repository.ListAsync(query, includeInactive, ct);
    public Task<IReadOnlyList<Place>> ListAsync(string? query, CancellationToken ct) => repository.ListAsync(query, false, ct);
    public Task<Place?> GetAsync(long id, CancellationToken ct) => repository.GetAsync(id, ct);
    public Task<IReadOnlyList<NarrationContent>> ListContentsAsync(long placeId, CancellationToken ct) => repository.ListContentsAsync(placeId, ct);
    public async Task<NarrationContent?> CreateContentAsync(long placeId, string title, string topic, string contentType, string sourceText, CancellationToken ct)
    {
        if (await repository.GetAsync(placeId, ct) is null) return null;
        var content = new NarrationContent { PlaceId = placeId, Title = title.Trim(), Topic = topic.Trim(), ContentType = string.IsNullOrWhiteSpace(contentType) ? "topic" : contentType.Trim().ToLowerInvariant(), SourceText = sourceText.Trim() };
        await repository.AddContentAsync(content, ct); await repository.SaveAsync(ct); return content;
    }
    public async Task<bool> UpdateContentAsync(long placeId, long contentId, string title, string topic, string contentType, string sourceText, CancellationToken ct)
    {
        var content = await repository.GetContentAsync(placeId, contentId, ct); if (content is null) return false;
        content.Title = title.Trim(); content.Topic = topic.Trim(); content.ContentType = string.IsNullOrWhiteSpace(contentType) ? "topic" : contentType.Trim().ToLowerInvariant(); content.SourceText = sourceText.Trim();
        await repository.SaveAsync(ct); return true;
    }
    public async Task<Place> CreateAsync(string name, string? description, string? city, string? category, string? address, decimal? latitude, decimal? longitude, string? imageUrl, CancellationToken ct)
    {
        var place = new Place { Name = name.Trim(), Description = description?.Trim() ?? "", City = city?.Trim(), Category = category?.Trim(), Address = address?.Trim(), Latitude = latitude, Longitude = longitude, ImageUrl = imageUrl?.Trim() };
        await repository.AddAsync(place, ct); await repository.SaveAsync(ct); return place;
    }
    public async Task<bool> UpdateAsync(long id, string name, string? description, string? city, string? category, string? address, decimal? latitude, decimal? longitude, string? imageUrl, bool isActive, CancellationToken ct)
    {
        var place = await repository.GetAsync(id, ct); if (place is null) return false;
        place.Name = name.Trim(); place.Description = description?.Trim() ?? ""; place.City = city?.Trim(); place.Category = category?.Trim(); place.Address = address?.Trim(); place.Latitude = latitude; place.Longitude = longitude; place.ImageUrl = imageUrl?.Trim(); place.IsActive = isActive; place.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct); return true;
    }

    public Task<bool> UpdateAsync(long id, string name, string? description, string? city, CancellationToken ct) =>
        UpdateAsync(id, name, description, city, null, null, null, null, null, true, ct);
    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var place = await repository.GetAsync(id, ct); if (place is null) return false;
        await repository.DeleteAsync(place, ct); await repository.SaveAsync(ct); return true;
    }
}
