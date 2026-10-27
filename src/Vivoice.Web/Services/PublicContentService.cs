// Purpose: Reads only active VIVOICE public content from SQL Server and applies documented language fallback.
// Responsibility: Maps persistence entities to safe browser DTOs without exposing admin-only fields.
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;
using Vivoice.Web.Models;

namespace Vivoice.Web.Services;

public sealed class PublicContentService(VivoiceDbContext db)
{
    public async Task<IReadOnlyList<PoiCardDto>> GetPoisAsync(string languageCode, CancellationToken cancellationToken)
    {
        var pois = await db.Pois.AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Translations)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return pois.Select(poi =>
        {
            var translation = FindPoiTranslation(poi.Translations, languageCode);
            return new PoiCardDto(poi.Id, poi.Slug, translation.Value.Name, translation.Value.Summary, poi.Latitude, poi.Longitude, poi.ImageUrl, poi.DisplayOrder, translation.FallbackUsed);
        }).ToList();
    }

    public async Task<IReadOnlyList<TopicDto>?> GetTopicsAsync(long poiId, string languageCode, CancellationToken cancellationToken)
    {
        var poi = await db.Pois.AsNoTracking()
            .Include(x => x.Topics.Where(topic => topic.IsActive))
            .ThenInclude(topic => topic.Translations.Where(translation => translation.IsPublished))
            .FirstOrDefaultAsync(x => x.Id == poiId && x.IsActive, cancellationToken);
        if (poi is null)
        {
            return null;
        }

        return poi.Topics.OrderBy(x => x.DisplayOrder).Select(topic =>
        {
            var translation = FindTopicTranslation(topic.Translations, languageCode);
            return new TopicDto(topic.Id, topic.Category, translation.Value.Title, topic.DisplayOrder, translation.FallbackUsed);
        }).ToList();
    }

    public async Task<NarrationDto?> GetNarrationAsync(long topicId, string languageCode, CancellationToken cancellationToken)
    {
        var topic = await db.PoiTopics.AsNoTracking()
            .Include(x => x.Translations.Where(translation => translation.IsPublished))
            .FirstOrDefaultAsync(x => x.Id == topicId && x.IsActive && x.Poi.IsActive, cancellationToken);
        if (topic is null || topic.Translations.Count == 0)
        {
            return null;
        }

        var translation = FindTopicTranslation(topic.Translations, languageCode);
        return new NarrationDto(topic.Id, topic.Category, translation.Value.Title, translation.Value.Content, translation.Value.LanguageCode, translation.FallbackUsed);
    }

    public async Task<IReadOnlyList<AdvertisementDto>> GetAdvertisementsAsync(string placement, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var normalizedPlacement = placement.Trim().ToUpperInvariant();
        return await db.Advertisements.AsNoTracking()
            .Where(ad => ad.IsActive && ad.Placement == normalizedPlacement && ad.StartsAtUtc <= nowUtc && (ad.EndsAtUtc == null || ad.EndsAtUtc > nowUtc))
            .OrderBy(ad => ad.DisplayOrder)
            .Select(ad => new AdvertisementDto(ad.Id, ad.Name, ad.ImageUrl, ad.TargetUrl, ad.Placement))
            .ToListAsync(cancellationToken);
    }

    private static (PoiTranslation Value, bool FallbackUsed) FindPoiTranslation(IEnumerable<PoiTranslation> translations, string languageCode)
    {
        var requested = translations.FirstOrDefault(item => item.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        var fallback = requested ?? translations.FirstOrDefault(item => item.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase)) ?? translations.First();
        return (fallback, !ReferenceEquals(fallback, requested));
    }

    private static (TopicTranslation Value, bool FallbackUsed) FindTopicTranslation(IEnumerable<TopicTranslation> translations, string languageCode)
    {
        var requested = translations.FirstOrDefault(item => item.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        var fallback = requested ?? translations.FirstOrDefault(item => item.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase)) ?? translations.First();
        return (fallback, !ReferenceEquals(fallback, requested));
    }
}
