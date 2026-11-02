// Purpose: Identifies Admin-only routes that must be unavailable from a public-only VIVOICE demo host.
// Responsibility: Enforces an explicit route boundary without making trust decisions from proxy-supplied client IP addresses.
namespace Vivoice.Web.Services;

public static class PublicDemoAccessPolicy
{
    public static bool IsAdminSurface(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return path.Equals("/admin", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/admin.html", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/admin.js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/admin/", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/admin/", StringComparison.OrdinalIgnoreCase);
    }
}
