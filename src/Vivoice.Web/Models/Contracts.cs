// Purpose: Declares explicit HTTP request and response contracts shared by VIVOICE controllers and browser clients.
// Responsibility: Prevents database entities and internal fields from being returned directly by API endpoints.
namespace Vivoice.Web.Models;

public sealed record PoiCardDto(long Id, string Slug, string Name, string Summary, decimal Latitude, decimal Longitude, string? ImageUrl, int DisplayOrder, bool FallbackUsed);
public sealed record TopicDto(long Id, string Category, string Title, int DisplayOrder, bool FallbackUsed);
public sealed record NarrationDto(long TopicId, string Category, string Title, string Content, string LanguageCode, bool FallbackUsed);
public sealed record AdvertisementDto(long Id, string Name, string ImageUrl, string TargetUrl, string Placement);
public sealed record AdvertisementEventRequest(string EventType, string? SessionId);
public sealed record HeartbeatResponse(bool VisitRecorded, string DeviceType, int ActiveForSeconds);
public sealed record AdminLoginRequest(string DisplayName, string Phone, string Password);
public sealed record PoiWriteRequest(string Slug, decimal Latitude, decimal Longitude, string? ImageUrl, int DisplayOrder, bool IsActive, string NameVi, string SummaryVi);
public sealed record TopicWriteRequest(string Category, int DisplayOrder, bool IsActive, string TitleVi, string ContentVi);
public sealed record PoiTranslationWriteRequest(string Name, string Summary);
public sealed record TopicTranslationWriteRequest(string Title, string Content, bool IsPublished);
public sealed record AdvertisementWriteRequest(long? SponsorId, string Name, string ImageUrl, string TargetUrl, string Placement, DateTime StartsAtUtc, DateTime? EndsAtUtc, int DisplayOrder, bool IsActive);
public sealed record SponsorWriteRequest(string Name, string? ContactNote, bool IsActive);
public sealed record SponsorshipAgreementWriteRequest(long SponsorId, decimal CommittedAmount, string Currency, string Status, DateTime StartsAtUtc, DateTime? EndsAtUtc);
public sealed record SponsorshipPaymentWriteRequest(decimal ReceivedAmount, DateTime ReceivedAtUtc);
public sealed record RevenueRateWriteRequest(decimal Amount, string Currency, DateTime EffectiveFromUtc);
public sealed record DailyVisitsDto(DateOnly Date, int Visits);
public sealed record DashboardDto(DateTime FromUtc, DateTime ToUtc, int ValidVisits, int UniqueVisitors, int ActiveSessions, int ActiveDevices, IReadOnlyDictionary<string, int> ActiveDevicesByType, IReadOnlyList<DailyVisitsDto> VisitsByDate, decimal TrafficEstimate, decimal ContractedRevenue, decimal ReceivedRevenue, decimal OutstandingRevenue, int ActiveSponsors, int AdvertisementImpressions, int AdvertisementClicks, string Currency);
