// Purpose: Issues and resolves opaque anonymous browser-device cookies for public analytics.
// Responsibility: Reuses one protected browser identity across reloads and never accepts a client-invented UUID.
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Vivoice.Web.Services;

public sealed class VisitorSessionCookieService(IDataProtectionProvider dataProtectionProvider)
{
    public const string CookieName = "vivoice_visitor";

    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector("Vivoice.Web.VisitorSession.v1");

    public string Issue() => protector.Protect(Guid.NewGuid().ToString("N"));

    public VisitorSessionTicket ResolveOrIssue(string? protectedValue)
    {
        if (TryRead(protectedValue, out var existingSessionId))
        {
            return new VisitorSessionTicket(protectedValue!, existingSessionId, false);
        }

        var sessionId = Guid.NewGuid().ToString("N");
        return new VisitorSessionTicket(protector.Protect(sessionId), sessionId, true);
    }

    public bool TryRead(string? protectedValue, out string sessionId)
    {
        sessionId = string.Empty;
        if (string.IsNullOrWhiteSpace(protectedValue))
        {
            return false;
        }

        try
        {
            var unprotectedValue = protector.Unprotect(protectedValue);
            if (!Guid.TryParseExact(unprotectedValue, "N", out _))
            {
                return false;
            }

            sessionId = unprotectedValue;
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}

public sealed record VisitorSessionTicket(string ProtectedValue, string SessionId, bool IsNew);
