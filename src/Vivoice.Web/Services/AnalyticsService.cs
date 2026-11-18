// Purpose: Records privacy-minimal anonymous browser-device activity and calculates SQL-backed dashboard metrics.
// Responsibility: Separates page visits from heartbeats, snapshots eligible sponsor-campaign revenue and avoids fingerprinting.
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;
using Vivoice.Web.Models;

namespace Vivoice.Web.Services;

public sealed class AnalyticsService(VivoiceDbContext db, IConfiguration configuration)
{
    public async Task<bool> RecordPageVisitAsync(string sessionId, string? userAgent, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(sessionId, out _))
        {
            return false;
        }

        var nowUtc = DateTime.UtcNow;
        var sessionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)));
        var device = DeviceClassifier.Describe(userAgent);
        if (device.IsLikelyBot)
        {
            return false;
        }

        var session = await db.VisitorSessions.FirstOrDefaultAsync(x => x.SessionHash == sessionHash, cancellationToken);
        if (session is null)
        {
            session = new VisitorSession
            {
                SessionHash = sessionHash,
                DeviceType = device.DeviceType,
                BrowserFamily = device.BrowserFamily,
                OperatingSystem = device.OperatingSystem,
                FirstSeenAtUtc = nowUtc,
                LastSeenAtUtc = nowUtc
            };
            db.VisitorSessions.Add(session);
        }
        else
        {
            session.LastSeenAtUtc = nowUtc;
        }

        var visit = new Visit
        {
            VisitorSession = session,
            // New visits are priced exclusively through campaign attributions.
            // Legacy global-rate fields stay at zero to preserve old history without creating new global pricing.
            AppliedRateAmount = 0m,
            Currency = "USD",
            OccurredAtUtc = nowUtc,
            IsValid = true
        };
        db.Visits.Add(visit);

        var candidateRates = await db.SponsorshipAgreementVisitRates.AsNoTracking()
            .Where(rate => rate.Currency == "USD"
                && rate.EffectiveFromUtc <= nowUtc
                && (rate.EffectiveToUtc == null || rate.EffectiveToUtc > nowUtc))
            .Select(rate => new
            {
                rate.Id,
                rate.SponsorshipAgreementId,
                rate.IsEnabled,
                rate.Amount,
                rate.Currency,
                rate.EffectiveFromUtc,
                rate.EffectiveToUtc,
                AgreementStatus = rate.SponsorshipAgreement.Status,
                AgreementBillingMode = rate.SponsorshipAgreement.BillingMode,
                AgreementStartsAtUtc = rate.SponsorshipAgreement.StartsAtUtc,
                AgreementEndsAtUtc = rate.SponsorshipAgreement.EndsAtUtc,
                SponsorIsActive = rate.SponsorshipAgreement.Sponsor.IsActive
            })
            .ToListAsync(cancellationToken);
        var eligibleRates = candidateRates
            .Where(rate => SponsorshipRevenuePolicy.IsEligibleForVisit(
                rate.SponsorIsActive,
                rate.AgreementStatus,
                rate.AgreementBillingMode,
                rate.AgreementStartsAtUtc,
                rate.AgreementEndsAtUtc,
                rate.IsEnabled,
                rate.Amount,
                rate.EffectiveFromUtc,
                rate.EffectiveToUtc,
                nowUtc))
            .GroupBy(rate => rate.SponsorshipAgreementId)
            .Select(group => group
                .OrderByDescending(rate => rate.EffectiveFromUtc)
                .ThenByDescending(rate => rate.Id)
                .First())
            .ToList();

        foreach (var rate in eligibleRates)
        {
            visit.SponsorRevenues.Add(new VisitSponsorshipRevenue
            {
                SponsorshipAgreementId = rate.SponsorshipAgreementId,
                SponsorshipAgreementVisitRateId = rate.Id,
                AppliedAmount = rate.Amount,
                Currency = rate.Currency
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<HeartbeatResponse?> RecordHeartbeatAsync(string sessionId, string? userAgent, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(sessionId, out _))
        {
            return null;
        }

        var device = DeviceClassifier.Describe(userAgent);
        if (device.IsLikelyBot)
        {
            return new HeartbeatResponse(device.DeviceType, ActiveSeconds());
        }

        var sessionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)));
        var session = await db.VisitorSessions.FirstOrDefaultAsync(x => x.SessionHash == sessionHash, cancellationToken);
        if (session is null)
        {
            return null;
        }

        session.LastSeenAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return new HeartbeatResponse(session.DeviceType, ActiveSeconds());
    }

    public async Task<bool> RecordAdvertisementEventAsync(long advertisementId, string eventType, string? sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(eventType) || !Guid.TryParse(sessionId, out _)) return false;
        var normalizedEvent = eventType.Trim().ToLowerInvariant();
        if (normalizedEvent is not ("impression" or "click")) return false;
        var nowUtc = DateTime.UtcNow;
        var advertisementIsActive = await db.Advertisements.AnyAsync(ad => ad.Id == advertisementId && ad.IsActive && ad.StartsAtUtc <= nowUtc && (ad.EndsAtUtc == null || ad.EndsAtUtc > nowUtc), cancellationToken);
        if (!advertisementIsActive) return false;
        var sessionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId!)));
        db.AdvertisementEvents.Add(new AdvertisementEvent { AdvertisementId = advertisementId, EventType = normalizedEvent, SessionHash = sessionHash, OccurredAtUtc = nowUtc });
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DashboardDto> GetDashboardAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var activeAfterUtc = DateTime.UtcNow.AddSeconds(-ActiveSeconds());
        var visits = db.Visits.AsNoTracking().Where(x => x.IsValid && x.OccurredAtUtc >= fromUtc && x.OccurredAtUtc <= toUtc);
        var visitCount = await visits.CountAsync(cancellationToken);
        var sponsoredRevenue = db.VisitSponsorshipRevenues.AsNoTracking()
            .Where(revenue => revenue.Visit.IsValid && revenue.Visit.OccurredAtUtc >= fromUtc && revenue.Visit.OccurredAtUtc <= toUtc);
        var sponsoredVisitRevenueEstimate = await sponsoredRevenue.SumAsync(x => (decimal?)x.AppliedAmount, cancellationToken) ?? 0m;
        var legacyVisitRevenueEstimate = await visits
            .Where(visit => !visit.SponsorRevenues.Any())
            .SumAsync(visit => (decimal?)visit.AppliedRateAmount, cancellationToken) ?? 0m;
        var visitRevenueEstimate = sponsoredVisitRevenueEstimate + legacyVisitRevenueEstimate;
        var unpricedVisitCount = await visits.CountAsync(visit => !visit.SponsorRevenues.Any() && visit.AppliedRateAmount <= 0m, cancellationToken);
        var visitsByDate = (await visits.GroupBy(x => x.OccurredAtUtc.Date)
            .Select(group => new { Date = group.Key, Visits = group.Count() })
            .OrderBy(item => item.Date)
            .ToListAsync(cancellationToken))
            .Select(item => new DailyVisitsDto(DateOnly.FromDateTime(item.Date), item.Visits)).ToList();
        var activeDevicesQuery = db.VisitorSessions.AsNoTracking()
            .Where(x => x.LastSeenAtUtc >= activeAfterUtc && x.Visits.Any(visit => visit.IsValid));
        var activeDeviceCount = await activeDevicesQuery.CountAsync(cancellationToken);
        var agreementIds = await db.SponsorshipAgreements.AsNoTracking()
            .Where(x => x.Status != "cancelled" && x.StartsAtUtc <= toUtc && (x.EndsAtUtc == null || x.EndsAtUtc >= fromUtc))
            .Select(x => x.Id).ToListAsync(cancellationToken);
        var fixedAgreementIds = await db.SponsorshipAgreements.AsNoTracking()
            .Where(x => agreementIds.Contains(x.Id) && x.BillingMode == SponsorshipRevenuePolicy.FixedBillingMode)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var contracted = await db.SponsorshipAgreements.AsNoTracking()
            .Where(x => fixedAgreementIds.Contains(x.Id))
            .SumAsync(x => (decimal?)x.CommittedAmount, cancellationToken) ?? 0m;
        var fixedPayments = db.SponsorshipPayments.AsNoTracking().Where(x => fixedAgreementIds.Contains(x.SponsorshipAgreementId));
        var received = await fixedPayments.Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= toUtc).SumAsync(x => (decimal?)x.ReceivedAmount, cancellationToken) ?? 0m;
        var receivedAgainstFixedAgreements = await fixedPayments.SumAsync(x => (decimal?)x.ReceivedAmount, cancellationToken) ?? 0m;
        var activeSponsors = await db.Sponsors.AsNoTracking().CountAsync(x => x.IsActive, cancellationToken);
        var adEvents = db.AdvertisementEvents.AsNoTracking().Where(x => x.OccurredAtUtc >= fromUtc && x.OccurredAtUtc <= toUtc);
        var impressions = await adEvents.CountAsync(x => x.EventType == "impression", cancellationToken);
        var clicks = await adEvents.CountAsync(x => x.EventType == "click", cancellationToken);
        var campaignRevenue = await BuildCampaignRevenueAsync(sponsoredRevenue, cancellationToken);
        return new DashboardDto(fromUtc, toUtc, visitCount, activeDeviceCount, visitsByDate, visitRevenueEstimate, sponsoredVisitRevenueEstimate, legacyVisitRevenueEstimate, unpricedVisitCount, campaignRevenue, contracted, received, Math.Max(0m, contracted - receivedAgainstFixedAgreements), activeSponsors, impressions, clicks, "USD", ActiveSeconds());
    }

    public async Task<IReadOnlyList<ActiveDeviceDto>> GetActiveDevicesAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-ActiveSeconds());
        return await db.VisitorSessions.AsNoTracking()
            .Where(x => x.LastSeenAtUtc >= cutoff && x.Visits.Any(visit => visit.IsValid))
            .OrderByDescending(x => x.LastSeenAtUtc)
            .Select(x => new ActiveDeviceDto(x.Id, x.DeviceType, x.BrowserFamily, x.OperatingSystem, x.FirstSeenAtUtc, x.LastSeenAtUtc))
            .ToListAsync(cancellationToken);
    }

    private int ActiveSeconds() => AnalyticsWindowPolicy.Normalize(configuration.GetValue<int?>("SessionTracking:ActiveSeconds"));

    private async Task<IReadOnlyList<CampaignRevenueDto>> BuildCampaignRevenueAsync(IQueryable<VisitSponsorshipRevenue> sponsoredRevenue, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var aggregateByAgreement = await sponsoredRevenue
            .GroupBy(revenue => revenue.SponsorshipAgreementId)
            .Select(group => new { AgreementId = group.Key, VisitCount = group.Count(), EstimatedRevenue = group.Sum(item => item.AppliedAmount) })
            .ToDictionaryAsync(item => item.AgreementId, cancellationToken);
        var campaigns = await db.SponsorshipAgreements.AsNoTracking()
            .Where(agreement => agreement.BillingMode == SponsorshipRevenuePolicy.PerVisitBillingMode)
            .Include(agreement => agreement.Sponsor)
            .Include(agreement => agreement.VisitRates)
            .OrderBy(agreement => agreement.Sponsor.Name)
            .ThenBy(agreement => agreement.CampaignName)
            .ToListAsync(cancellationToken);

        return campaigns
            .Select(campaign =>
            {
                var currentRate = campaign.VisitRates
                    .Where(rate => rate.EffectiveFromUtc <= nowUtc && (rate.EffectiveToUtc == null || rate.EffectiveToUtc > nowUtc))
                    .OrderByDescending(rate => rate.EffectiveFromUtc)
                    .FirstOrDefault();
                aggregateByAgreement.TryGetValue(campaign.Id, out var totals);
                return new CampaignRevenueDto(
                    campaign.Id,
                    campaign.SponsorId,
                    campaign.Sponsor.Name,
                    campaign.CampaignName,
                    campaign.BillingMode,
                    currentRate?.IsEnabled == true ? currentRate.Amount : null,
                    currentRate?.IsEnabled == true,
                    totals?.VisitCount ?? 0,
                    totals?.EstimatedRevenue ?? 0m,
                    campaign.Currency,
                    campaign.Status);
            })
            .ToList();
    }
}

public static class AnalyticsWindowPolicy
{
    public const int DefaultActiveSeconds = 300;

    public static int Normalize(int? configuredSeconds) => Math.Clamp(configuredSeconds ?? DefaultActiveSeconds, 30, 900);
}

public static class DeviceClassifier
{
    public static DeviceMetadata Describe(string? userAgent)
    {
        var value = userAgent?.ToUpperInvariant() ?? string.Empty;
        var deviceType = value.Contains("IPAD") || value.Contains("TABLET") ? "Tablet" : value.Contains("MOBILE") || value.Contains("IPHONE") || value.Contains("ANDROID") ? "Phone" : "Desktop";
        var browser = value.Contains("EDG/") ? "Edge" : value.Contains("FIREFOX/") ? "Firefox" : value.Contains("CHROME/") ? "Chrome" : value.Contains("SAFARI/") ? "Safari" : "Other";
        var operatingSystem = value.Contains("ANDROID") ? "Android" : value.Contains("IPHONE") || value.Contains("IPAD") ? "iOS" : value.Contains("WINDOWS") ? "Windows" : value.Contains("MAC OS") ? "macOS" : value.Contains("LINUX") ? "Linux" : "Other";
        var isLikelyBot = string.IsNullOrWhiteSpace(value) || value.Contains("BOT") || value.Contains("CRAWLER") || value.Contains("SPIDER");
        return new DeviceMetadata(deviceType, browser, operatingSystem, isLikelyBot);
    }

    public static string FromUserAgent(string? userAgent)
    {
        return Describe(userAgent).DeviceType;
    }
}

public sealed record DeviceMetadata(string DeviceType, string BrowserFamily, string OperatingSystem, bool IsLikelyBot);
