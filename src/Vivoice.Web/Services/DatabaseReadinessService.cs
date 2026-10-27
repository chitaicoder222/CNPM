// Purpose: Reports generic database readiness without exposing connection, migration or schema details through HTTP.
// Responsibility: Distinguishes a live web process from a database that is unreachable or has pending EF Core migrations.
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;

namespace Vivoice.Web.Services;

public static class DatabaseReadinessPolicy
{
    public static bool IsReady(bool canConnect, bool hasPendingMigrations) => canConnect && !hasPendingMigrations;
}

public interface IDatabaseReadinessService
{
    Task<bool> IsReadyAsync(CancellationToken cancellationToken = default);
}

public sealed class DatabaseReadinessService(VivoiceDbContext db) : IDatabaseReadinessService
{
    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return false;
            }

            var hasPendingMigrations = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any();
            return DatabaseReadinessPolicy.IsReady(canConnect, hasPendingMigrations);
        }
        catch
        {
            return false;
        }
    }
}
