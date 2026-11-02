// Purpose: Verifies normalization rules used by the three-field administrator login.
// Responsibility: Ensures equivalent name/phone formatting resolves to deterministic database keys.
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Unit;

public sealed class IdentityNormalizerTests
{
    [Fact]
    public void Name_TrimsAndUppercases() => Assert.Equal("DEMO ADMIN", IdentityNormalizer.Name("  Demo Admin "));

    [Fact]
    public void Phone_KeepsOnlyDigits() => Assert.Equal("0123456789", IdentityNormalizer.Phone("0123 456 789"));
}
