// Purpose: Issues and validates opaque visitor-session cookies for anonymous analytics.
// Responsibility: Ensures a heartbeat can use only a server-protected session identifier, never a client-invented UUID.
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Vivoice.Web.Services;

public sealed class VisitorSessionCookieService(IDataProtectionProvider dataProtectionProvider)
{
    public const string CookieName = "vivoice_visitor";

    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector("Vivoice.Web.VisitorSession.v1");

    public string Issue() => protector.Protect(Guid.NewGuid().ToString("N"));

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
