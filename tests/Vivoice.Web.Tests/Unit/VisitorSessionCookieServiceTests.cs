// Purpose: Verifies opaque browser-device cookies reject malformed client input without a web host or SQL Server.
// Responsibility: Protects one server-issued anonymous device identity from accidental reload duplication.
using Microsoft.AspNetCore.DataProtection;
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Unit;

public sealed class VisitorSessionCookieServiceTests
{
    [Fact]
    public void Issue_ProducesProtectedGuidThatCanBeReadOnlyByTheSameProvider()
    {
        var keyDirectory = CreateTemporaryKeyDirectory();
        try
        {
            var service = new VisitorSessionCookieService(DataProtectionProvider.Create(keyDirectory));

            var protectedCookie = service.Issue();

            Assert.True(service.TryRead(protectedCookie, out var sessionId));
            Assert.True(Guid.TryParseExact(sessionId, "N", out _));
        }
        finally
        {
            DeleteTemporaryKeyDirectory(keyDirectory);
        }
    }

    [Fact]
    public void TryRead_RejectsMissingAndTamperedCookies()
    {
        var keyDirectory = CreateTemporaryKeyDirectory();
        try
        {
            var service = new VisitorSessionCookieService(DataProtectionProvider.Create(keyDirectory));
            var protectedCookie = service.Issue();

            Assert.False(service.TryRead(null, out _));
            Assert.False(service.TryRead(protectedCookie + "tampered", out _));
        }
        finally
        {
            DeleteTemporaryKeyDirectory(keyDirectory);
        }
    }

    [Fact]
    public void ResolveOrIssue_ReusesOneValidCookieAcrossReloads()
    {
        var keyDirectory = CreateTemporaryKeyDirectory();
        try
        {
            var service = new VisitorSessionCookieService(DataProtectionProvider.Create(keyDirectory));
            var first = service.ResolveOrIssue(null);
            var reload = service.ResolveOrIssue(first.ProtectedValue);

            Assert.True(first.IsNew);
            Assert.False(reload.IsNew);
            Assert.Equal(first.ProtectedValue, reload.ProtectedValue);
            Assert.Equal(first.SessionId, reload.SessionId);
        }
        finally
        {
            DeleteTemporaryKeyDirectory(keyDirectory);
        }
    }

    [Fact]
    public void ResolveOrIssue_ReplacesAnInvalidCookieWithANewIdentity()
    {
        var keyDirectory = CreateTemporaryKeyDirectory();
        try
        {
            var service = new VisitorSessionCookieService(DataProtectionProvider.Create(keyDirectory));
            var original = service.Issue();
            var replacement = service.ResolveOrIssue(original + "tampered");

            Assert.True(replacement.IsNew);
            Assert.NotEqual(original, replacement.ProtectedValue);
            Assert.True(service.TryRead(replacement.ProtectedValue, out var resolvedId));
            Assert.Equal(replacement.SessionId, resolvedId);
        }
        finally
        {
            DeleteTemporaryKeyDirectory(keyDirectory);
        }
    }

    private static DirectoryInfo CreateTemporaryKeyDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "vivoice-tests", Guid.NewGuid().ToString("N"));
        return Directory.CreateDirectory(path);
    }

    private static void DeleteTemporaryKeyDirectory(DirectoryInfo keyDirectory)
    {
        if (keyDirectory.Exists)
        {
            keyDirectory.Delete(recursive: true);
        }
    }
}
