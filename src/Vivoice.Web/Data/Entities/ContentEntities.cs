// Purpose: Defines POI, localized content and advertisement entities used by public exploration and admin management.
// Responsibility: Keeps user-facing travel content data-driven instead of hardcoded in browser JavaScript.
namespace Vivoice.Web.Data.Entities;

public sealed class Poi
{
    public long Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<PoiTranslation> Translations { get; set; } = [];
    public List<PoiTopic> Topics { get; set; } = [];
}

public sealed class PoiTranslation
{
    public long Id { get; set; }
    public long PoiId { get; set; }
    public string LanguageCode { get; set; } = "vi";
    public string Name { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public Poi Poi { get; set; } = null!;
}

public sealed class PoiTopic
{
    public long Id { get; set; }
    public long PoiId { get; set; }
    public string Category { get; set; } = "culture";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Poi Poi { get; set; } = null!;
    public List<TopicTranslation> Translations { get; set; } = [];
}

public sealed class TopicTranslation
{
    public long Id { get; set; }
    public long PoiTopicId { get; set; }
    public string LanguageCode { get; set; } = "vi";
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public PoiTopic PoiTopic { get; set; } = null!;
}

public sealed class Advertisement
{
    public long Id { get; set; }
    public long? SponsorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string Placement { get; set; } = "LEFT";
    public DateTime StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Sponsor? Sponsor { get; set; }
    public List<AdvertisementEvent> Events { get; set; } = [];
}

public sealed class AdvertisementEvent
{
    public long Id { get; set; }
    public long AdvertisementId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? SessionHash { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public Advertisement Advertisement { get; set; } = null!;
}
