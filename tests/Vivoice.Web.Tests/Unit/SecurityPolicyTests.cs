// Purpose: Verifies pure security and financial-integrity policies without a web host or SQL Server.
// Responsibility: Protects request validation and sponsorship-payment rules from regressions.
using Vivoice.Web.Controllers;

namespace Vivoice.Web.Tests.Unit;

public sealed class SecurityPolicyTests
{
    [Theory]
    [InlineData("LEFT", true)]
    [InlineData(" right ", true)]
    [InlineData("TOP", false)]
    [InlineData("", false)]
    public void AdvertisementPlacement_OnlyAllowsConfiguredPublicRails(string value, bool expected)
    {
        Assert.Equal(expected, PublicRequestValidation.IsAdvertisementPlacement(value));
    }

    [Fact]
    public void AdvertisementPlacement_RejectsNullInput()
    {
        Assert.False(PublicRequestValidation.IsAdvertisementPlacement(null));
    }

    [Theory]
    [InlineData("https://cdn.example.test/image.jpg", true)]
    [InlineData("http://cdn.example.test/image.jpg", false)]
    [InlineData("not-a-url", false)]
    public void ExternalMediaUrl_RequiresHttps(string value, bool expected)
    {
        Assert.Equal(expected, ExternalHttpsUrlPolicy.IsValid(value));
    }

    [Theory]
    [InlineData(" culture ", "culture")]
    [InlineData("FOOD", "food")]
    [InlineData("tourism", "tourism")]
    [InlineData("other", null)]
    public void TopicCategory_NormalizesOnlyAllowedValues(string value, string? expected)
    {
        Assert.Equal(expected, TopicCategoryPolicy.Normalize(value));
    }

    [Fact]
    public void PaymentPolicy_RejectsCancelledAgreement()
    {
        var result = SponsorshipPaymentPolicy.Validate("cancelled", 100m, 0m, 10m);

        Assert.Equal(SponsorshipPaymentValidation.AgreementCancelled, result);
    }

    [Fact]
    public void PaymentPolicy_RejectsPaymentBeyondRemainingCommitment()
    {
        var result = SponsorshipPaymentPolicy.Validate("active", 100m, 90m, 20m);

        Assert.Equal(SponsorshipPaymentValidation.CommitmentExceeded, result);
    }

    [Fact]
    public void PaymentPolicy_AcceptsPaymentWithinRemainingCommitment()
    {
        var result = SponsorshipPaymentPolicy.Validate("active", 100m, 90m, 10m);

        Assert.Equal(SponsorshipPaymentValidation.Accepted, result);
    }

    [Fact]
    public void PaymentPolicy_PreventsReducingCommitmentBelowReceivedPayments()
    {
        Assert.True(SponsorshipPaymentPolicy.IsCommitmentBelowReceived(100m, 110m));
        Assert.False(SponsorshipPaymentPolicy.IsCommitmentBelowReceived(100m, 100m));
    }
}
