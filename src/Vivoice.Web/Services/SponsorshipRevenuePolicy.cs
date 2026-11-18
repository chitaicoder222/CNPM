// Purpose: Centralizes non-persistent sponsor-campaign pricing rules for VIVOICE.
// Responsibility: Keeps eligibility and billing-mode decisions consistent across writes, visit recording and tests.
namespace Vivoice.Web.Services;

public static class SponsorshipRevenuePolicy
{
    public const string FixedBillingMode = "fixed";
    public const string PerVisitBillingMode = "per_visit";

    public static string? NormalizeBillingMode(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        FixedBillingMode => FixedBillingMode,
        PerVisitBillingMode => PerVisitBillingMode,
        _ => null
    };

    public static bool IsEligibleForVisit(
        bool sponsorIsActive,
        string? agreementStatus,
        string? billingMode,
        DateTime agreementStartsAtUtc,
        DateTime? agreementEndsAtUtc,
        bool rateIsEnabled,
        decimal rateAmount,
        DateTime rateEffectiveFromUtc,
        DateTime? rateEffectiveToUtc,
        DateTime occurredAtUtc) =>
        sponsorIsActive
        && string.Equals(agreementStatus, "active", StringComparison.OrdinalIgnoreCase)
        && string.Equals(NormalizeBillingMode(billingMode), PerVisitBillingMode, StringComparison.Ordinal)
        && agreementStartsAtUtc <= occurredAtUtc
        && (agreementEndsAtUtc is null || agreementEndsAtUtc > occurredAtUtc)
        && rateIsEnabled
        && rateAmount > 0m
        && rateEffectiveFromUtc <= occurredAtUtc
        && (rateEffectiveToUtc is null || rateEffectiveToUtc > occurredAtUtc);

    public static bool IsValidRateInput(bool isEnabled, decimal? amount) =>
        isEnabled ? amount is > 0m : amount is null or 0m;

    public static bool IsRateIntervalWithinCampaign(
        DateTime rateEffectiveFromUtc,
        DateTime? rateEffectiveToUtc,
        DateTime campaignStartsAtUtc,
        DateTime? campaignEndsAtUtc) =>
        rateEffectiveFromUtc >= campaignStartsAtUtc
        && (campaignEndsAtUtc is null
            || (rateEffectiveFromUtc < campaignEndsAtUtc
                && rateEffectiveToUtc is not null
                && rateEffectiveToUtc <= campaignEndsAtUtc));
}
