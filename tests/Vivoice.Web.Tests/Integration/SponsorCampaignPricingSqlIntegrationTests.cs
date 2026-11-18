// Purpose: Verifies sponsor-campaign price snapshots against an explicitly supplied isolated SQL Server database.
// Responsibility: Proves add-only migration, multi-campaign attribution and legacy-revenue separation without using the VIVOICE development database.
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Integration;

[Collection("SponsorCampaignSql")]
public sealed class SponsorCampaignPricingSqlIntegrationTests
{
    private const string ConnectionEnvironmentKey = "VIVOICE_SPONSOR_INTEGRATION_CONNECTION";

    [IsolatedSqlFact]
    public async Task MigrationAndVisitPricing_PreserveLegacyAndSnapshotEveryEligibleCampaign()
    {
        var connectionString = RequireIsolatedConnectionString();
        var options = new DbContextOptionsBuilder<VivoiceDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var db = new VivoiceDbContext(options);

        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
        Assert.Contains("20261028172836_AddSponsorCampaignVisitPricing", appliedMigrations);

        var nowUtc = DateTime.UtcNow;
        var nonce = Guid.NewGuid().ToString("N");
        var activeSponsor = new Sponsor { Name = $"integration-active-{nonce}", IsActive = true };
        var inactiveSponsor = new Sponsor { Name = $"integration-inactive-{nonce}", IsActive = false };
        var campaignA = CreatePerVisitCampaign(activeSponsor, $"campaign-a-{nonce}", nowUtc.AddHours(-2), 0.10m, nowUtc.AddHours(-2));
        var campaignB = CreatePerVisitCampaign(activeSponsor, $"campaign-b-{nonce}", nowUtc.AddHours(-2), 0.05m, nowUtc.AddHours(-2));
        var inactiveCampaign = CreatePerVisitCampaign(inactiveSponsor, $"campaign-inactive-{nonce}", nowUtc.AddHours(-2), 0.90m, nowUtc.AddHours(-2));
        var fixedCampaign = new SponsorshipAgreement
        {
            CampaignName = $"campaign-fixed-{nonce}",
            BillingMode = SponsorshipRevenuePolicy.FixedBillingMode,
            CommittedAmount = 500m,
            Currency = "USD",
            Status = "active",
            StartsAtUtc = nowUtc.AddHours(-2)
        };
        activeSponsor.Agreements.Add(campaignA);
        activeSponsor.Agreements.Add(campaignB);
        activeSponsor.Agreements.Add(fixedCampaign);
        inactiveSponsor.Agreements.Add(inactiveCampaign);
        db.Sponsors.AddRange(activeSponsor, inactiveSponsor);
        await db.SaveChangesAsync();

        var analytics = new AnalyticsService(db, new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SessionTracking:ActiveSeconds"] = "300" })
            .Build());
        var firstSessionId = Guid.NewGuid().ToString();
        Assert.True(await analytics.RecordPageVisitAsync(firstSessionId, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/130.0", CancellationToken.None));

        var firstVisit = await FindVisitAsync(db, firstSessionId);
        Assert.Equal(0m, firstVisit.AppliedRateAmount);
        Assert.Null(firstVisit.VisitRevenueRateId);
        Assert.Equal(2, firstVisit.SponsorRevenues.Count);
        Assert.Contains(firstVisit.SponsorRevenues, item => item.SponsorshipAgreementId == campaignA.Id && item.AppliedAmount == 0.10m);
        Assert.Contains(firstVisit.SponsorRevenues, item => item.SponsorshipAgreementId == campaignB.Id && item.AppliedAmount == 0.05m);
        Assert.DoesNotContain(firstVisit.SponsorRevenues, item => item.SponsorshipAgreementId == inactiveCampaign.Id || item.SponsorshipAgreementId == fixedCampaign.Id);

        var newRateStartUtc = DateTime.UtcNow;
        var priorRate = campaignA.VisitRates.Single();
        priorRate.EffectiveToUtc = newRateStartUtc;
        db.SponsorshipAgreementVisitRates.Add(new SponsorshipAgreementVisitRate
        {
            SponsorshipAgreementId = campaignA.Id,
            IsEnabled = true,
            Amount = 0.20m,
            Currency = "USD",
            EffectiveFromUtc = newRateStartUtc
        });
        await db.SaveChangesAsync();

        var secondSessionId = Guid.NewGuid().ToString();
        Assert.True(await analytics.RecordPageVisitAsync(secondSessionId, "Mozilla/5.0 (iPhone; CPU iPhone OS 18_0) Mobile Safari/604.1", CancellationToken.None));
        var secondVisit = await FindVisitAsync(db, secondSessionId);
        Assert.Equal(2, secondVisit.SponsorRevenues.Count);
        Assert.Contains(secondVisit.SponsorRevenues, item => item.SponsorshipAgreementId == campaignA.Id && item.AppliedAmount == 0.20m);
        Assert.Contains(secondVisit.SponsorRevenues, item => item.SponsorshipAgreementId == campaignB.Id && item.AppliedAmount == 0.05m);

        var legacySession = new VisitorSession
        {
            SessionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))),
            DeviceType = "Desktop",
            BrowserFamily = "Other",
            OperatingSystem = "Other",
            FirstSeenAtUtc = DateTime.UtcNow,
            LastSeenAtUtc = DateTime.UtcNow,
            Visits = [new Visit { AppliedRateAmount = 0.25m, Currency = "USD", OccurredAtUtc = DateTime.UtcNow, IsValid = true }, new Visit { AppliedRateAmount = 0m, Currency = "USD", OccurredAtUtc = DateTime.UtcNow, IsValid = true }]
        };
        db.VisitorSessions.Add(legacySession);
        await db.SaveChangesAsync();

        var dashboard = await analytics.GetDashboardAsync(nowUtc.AddMinutes(-5), DateTime.UtcNow.AddMinutes(1), CancellationToken.None);
        Assert.Equal(0.40m, dashboard.SponsoredVisitRevenueEstimate);
        Assert.Equal(0.25m, dashboard.LegacyVisitRevenueEstimate);
        Assert.Equal(0.65m, dashboard.VisitRevenueEstimate);
        Assert.Equal(1, dashboard.UnpricedVisitCount);
        Assert.Contains(dashboard.CampaignRevenue, item => item.SponsorshipAgreementId == campaignA.Id && item.VisitCount == 2 && item.EstimatedRevenue == 0.30m);
        Assert.Contains(dashboard.CampaignRevenue, item => item.SponsorshipAgreementId == campaignB.Id && item.VisitCount == 2 && item.EstimatedRevenue == 0.10m);
    }

    private static string RequireIsolatedConnectionString()
    {
        var value = Environment.GetEnvironmentVariable(ConnectionEnvironmentKey);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"{ConnectionEnvironmentKey} was absent after test discovery. This test must only run with an isolated SQL Server database.");
    }

    private static SponsorshipAgreement CreatePerVisitCampaign(Sponsor sponsor, string campaignName, DateTime startsAtUtc, decimal rateAmount, DateTime effectiveFromUtc) => new()
    {
        Sponsor = sponsor,
        CampaignName = campaignName,
        BillingMode = SponsorshipRevenuePolicy.PerVisitBillingMode,
        CommittedAmount = 0m,
        Currency = "USD",
        Status = "active",
        StartsAtUtc = startsAtUtc,
        VisitRates = [new SponsorshipAgreementVisitRate { IsEnabled = true, Amount = rateAmount, Currency = "USD", EffectiveFromUtc = effectiveFromUtc }]
    };

    private static async Task<Visit> FindVisitAsync(VivoiceDbContext db, string sessionId)
    {
        var sessionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)));
        return await db.Visits
            .Include(visit => visit.SponsorRevenues)
            .Where(visit => visit.VisitorSession.SessionHash == sessionHash)
            .SingleAsync();
    }
}

[CollectionDefinition("SponsorCampaignSql", DisableParallelization = true)]
public sealed class SponsorCampaignSqlCollection;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class IsolatedSqlFactAttribute : FactAttribute
{
    public IsolatedSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VIVOICE_SPONSOR_INTEGRATION_CONNECTION")))
            Skip = "Set VIVOICE_SPONSOR_INTEGRATION_CONNECTION only for an isolated SQL Server database to run this test.";
    }
}
