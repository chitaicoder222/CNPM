// Purpose: Resolves the one required VIVOICE database connection string without exposing its value.
// Responsibility: Gives runtime and EF Core design-time paths the same fail-closed missing-configuration behavior.
using Microsoft.Extensions.Configuration;

namespace Vivoice.Web.Data;

public static class VivoiceConnectionString
{
    public static string Require(IConfiguration configuration, string? operation = null)
    {
        var connectionString = configuration.GetConnectionString("Vivoice");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var context = string.IsNullOrWhiteSpace(operation) ? string.Empty : $" {operation}";
            throw new InvalidOperationException(
                $"ConnectionStrings:Vivoice is required{context}. Configure User Secrets or environment variables.");
        }

        return connectionString;
    }
}
