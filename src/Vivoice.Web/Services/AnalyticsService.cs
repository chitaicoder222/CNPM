// Purpose: Records privacy-minimal anonymous sessions and calculates SQL-backed dashboard metrics.
// Responsibility: Applies valid-visit rate history on the server and classifies device type without fingerprinting.
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;
using Vivoice.Web.Models;

namespace Vivoice.Web.Services;

public sealed class AnalyticsService(VivoiceDbContext db, IConfiguration configuration)
{
    public async Task<HeartbeatResponse?> RecordHeartbeatAsync(string sessionId, string? userAgent, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(sessionId, out _))
        {
            return null;
        }

        var nowUtc = DateTime.UtcNow;
        var sessionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)));
        var device = DeviceClassifier.Describe(userAgent);
        if (device.IsLikelyBot)
        {
            return new HeartbeatResponse(false, device.DeviceType, ActiveSeconds());
        }

        var session = await db.VisitorSessions.FirstOrDefaultAsync(x => x.SessionHash == sessionHash, cancellationToken);
        var visitRecorded = false;
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
            var rate = await db.VisitRevenueRates
                .Where(x => x.EffectiveFromUtc <= nowUtc && (x.EffectiveToUtc == null || x.EffectiveToUtc > nowUtc))
                .OrderByDescending(x => x.EffectiveFromUtc)
                .FirstOrDefaultAsync(cancellationToken);
            db.Visits.Add(new Visit { VisitorSession = session, VisitRevenueRate = rate, AppliedRateAmount = rate?.Amount ?? 0m, Currency = rate?.Currency ?? "USD", OccurredAtUtc = nowUtc, IsValid = true });
            visitRecorded = true;
        }
        else
        {
            session.LastSeenAtUtc = nowUtc;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new HeartbeatResponse(visitRecorded, session.DeviceType, ActiveSeconds());
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
        var validVisits = await visits.CountAsync(cancellationToken);
        var uniqueVisitors = await visits.Select(x => x.VisitorSessionId).Distinct().CountAsync(cancellationToken);
        var trafficEstimate = await visits.SumAsync(x => (decimal?)x.AppliedRateAmount, cancellationToken) ?? 0m;
        var visitsByDate = (await visits.GroupBy(x => x.OccurredAtUtc.Date)
            .Select(group => new { Date = group.Key, Visits = group.Count() })
            .OrderBy(item => item.Date)
            .ToListAsync(cancellationToken))
            .Select(item => new DailyVisitsDto(DateOnly.FromDateTime(item.Date), item.Visits)).ToList();
        var activeSessionsQuery = db.VisitorSessions.AsNoTracking()
            .Where(x => x.LastSeenAtUtc >= activeAfterUtc && x.Visits.Any(visit => visit.IsValid));
        var activeByType = await activeSessionsQuery
            .GroupBy(x => x.DeviceType)
            .Select(group => new { DeviceType = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.DeviceType, x => x.Count, cancellationToken);
        var agreementIds = await db.SponsorshipAgreements.AsNoTracking()
            .Where(x => x.Status != "cancelled" && x.StartsAtUtc <= toUtc && (x.EndsAtUtc == null || x.EndsAtUtc >= fromUtc))
            .Select(x => x.Id).ToListAsync(cancellationToken);
        var contracted = await db.SponsorshipAgreements.AsNoTracking().Where(x => agreementIds.Contains(x.Id)).SumAsync(x => (decimal?)x.CommittedAmount, cancellationToken) ?? 0m;
        var received = await db.SponsorshipPayments.AsNoTracking().Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= toUtc).SumAsync(x => (decimal?)x.ReceivedAmount, cancellationToken) ?? 0m;
        var receivedAgainstAgreements = await db.SponsorshipPayments.AsNoTracking().Where(x => agreementIds.Contains(x.SponsorshipAgreementId)).SumAsync(x => (decimal?)x.ReceivedAmount, cancellationToken) ?? 0m;
        var activeSponsors = await db.Sponsors.AsNoTracking().CountAsync(x => x.IsActive, cancellationToken);
        var adEvents = db.AdvertisementEvents.AsNoTracking().Where(x => x.OccurredAtUtc >= fromUtc && x.OccurredAtUtc <= toUtc);
        var impressions = await adEvents.CountAsync(x => x.EventType == "impression", cancellationToken);
        var clicks = await adEvents.CountAsync(x => x.EventType == "click", cancellationToken);
        var activeSessions = activeByType.Values.Sum();
        return new DashboardDto(fromUtc, toUtc, validVisits, uniqueVisitors, activeSessions, activeSessions, activeByType, visitsByDate, trafficEstimate, contracted, received, Math.Max(0m, contracted - receivedAgainstAgreements), activeSponsors, impressions, clicks, "USD", ActiveSeconds());
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
