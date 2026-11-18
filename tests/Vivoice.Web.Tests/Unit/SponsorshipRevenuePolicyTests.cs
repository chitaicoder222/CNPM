// Purpose: Verifies deterministic sponsor-campaign visit-pricing eligibility without a web host or SQL Server.
// Responsibility: Protects effective-date, billing-mode and rate-enable rules from creating incorrect revenue attribution.
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Unit;

public sealed class SponsorshipRevenuePolicyTests
{
    private static readonly DateTime OccurredAtUtc = new(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("fixed", "fixed")]
    [InlineData(" PER_VISIT ", "per_visit")]
    [InlineData("per-visit", null)]
    [InlineData("hybrid", null)]
    [InlineData("", null)]
    public void BillingMode_NormalizesOnlySupportedValues(string value, string? expected)
    {
        Assert.Equal(expected, SponsorshipRevenuePolicy.NormalizeBillingMode(value));
    }

    [Fact]
    public void PerVisitCampaign_AttributesAtInclusiveStartAndBeforeEnd()
    {
        var result = SponsorshipRevenuePolicy.IsEligibleForVisit(
            sponsorIsActive: true,
            agreementStatus: "active",
            billingMode: "per_visit",
            agreementStartsAtUtc: OccurredAtUtc,
            agreementEndsAtUtc: OccurredAtUtc.AddDays(1),
            rateIsEnabled: true,
            rateAmount: 0.10m,
            rateEffectiveFromUtc: OccurredAtUtc,
            rateEffectiveToUtc: OccurredAtUtc.AddHours(1),
            occurredAtUtc: OccurredAtUtc);

        Assert.True(result);
    }

    [Theory]
    [InlineData(false, "active", "per_visit", true, 0.10)]
    [InlineData(true, "draft", "per_visit", true, 0.10)]
    [InlineData(true, "cancelled", "per_visit", true, 0.10)]
    [InlineData(true, "active", "fixed", true, 0.10)]
    [InlineData(true, "active", "per_visit", false, 0.10)]
    [InlineData(true, "active", "per_visit", true, 0)]
    public void IneligibleCampaignOrRate_NeverAttributes(bool sponsorIsActive, string status, string billingMode, bool rateEnabled, decimal amount)
    {
        var result = SponsorshipRevenuePolicy.IsEligibleForVisit(
            sponsorIsActive,
            status,
            billingMode,
            OccurredAtUtc.AddDays(-1),
            OccurredAtUtc.AddDays(1),
            rateEnabled,
            amount,
            OccurredAtUtc.AddHours(-1),
            OccurredAtUtc.AddHours(1),
            OccurredAtUtc);

        Assert.False(result);
    }

    [Fact]
    public void EndBoundaries_AreExclusiveForCampaignAndRate()
    {
        var agreementEnded = SponsorshipRevenuePolicy.IsEligibleForVisit(true, "active", "per_visit", OccurredAtUtc.AddDays(-1), OccurredAtUtc, true, 0.10m, OccurredAtUtc.AddDays(-1), null, OccurredAtUtc);
        var rateEnded = SponsorshipRevenuePolicy.IsEligibleForVisit(true, "active", "per_visit", OccurredAtUtc.AddDays(-1), null, true, 0.10m, OccurredAtUtc.AddDays(-1), OccurredAtUtc, OccurredAtUtc);

        Assert.False(agreementEnded);
        Assert.False(rateEnded);
    }

    [Fact]
    public void LaterRatePeriod_IsEligibleWhileClosedPriorPeriodIsNot()
    {
        var previousRateIsEligible = SponsorshipRevenuePolicy.IsEligibleForVisit(true, "active", "per_visit", OccurredAtUtc.AddDays(-1), null, true, 0.10m, OccurredAtUtc.AddDays(-1), OccurredAtUtc, OccurredAtUtc);
        var laterRateIsEligible = SponsorshipRevenuePolicy.IsEligibleForVisit(true, "active", "per_visit", OccurredAtUtc.AddDays(-1), null, true, 0.15m, OccurredAtUtc, null, OccurredAtUtc);

        Assert.False(previousRateIsEligible);
        Assert.True(laterRateIsEligible);
    }

    [Theory]
    [InlineData(true, 0.10, true)]
    [InlineData(true, 0, false)]
    [InlineData(false, 0, true)]
    [InlineData(false, 0.10, false)]
    public void RateInput_RequiresPositiveAmountOnlyWhenEnabled(bool isEnabled, decimal amount, bool expected)
    {
        Assert.Equal(expected, SponsorshipRevenuePolicy.IsValidRateInput(isEnabled, amount));
    }

    [Fact]
    public void RateInterval_MustRemainFullyInsideTheCampaignWindow()
    {
        var campaignStart = OccurredAtUtc.AddDays(-2);
        var campaignEnd = OccurredAtUtc.AddDays(2);

        Assert.True(SponsorshipRevenuePolicy.IsRateIntervalWithinCampaign(OccurredAtUtc.AddDays(-1), OccurredAtUtc.AddDays(1), campaignStart, campaignEnd));
        Assert.False(SponsorshipRevenuePolicy.IsRateIntervalWithinCampaign(OccurredAtUtc.AddDays(-3), OccurredAtUtc.AddDays(-1), campaignStart, campaignEnd));
        Assert.False(SponsorshipRevenuePolicy.IsRateIntervalWithinCampaign(OccurredAtUtc, null, campaignStart, campaignEnd));
        Assert.False(SponsorshipRevenuePolicy.IsRateIntervalWithinCampaign(OccurredAtUtc, OccurredAtUtc.AddDays(3), campaignStart, campaignEnd));
    }
}
