// Purpose: Runs explicit, operator-invoked database initialization without making normal web startup mutate schema or data.
// Responsibility: Sequences EF migrations, idempotent content seed and optional first-Admin bootstrap with no implicit privilege escalation.
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;

namespace Vivoice.Web.Services;

public sealed record DatabaseInitializationCommand(bool IsRequested, bool BootstrapAdmin)
{
    public static DatabaseInitializationCommand Parse(string[] args)
    {
        var initializeDatabase = args.Contains("--initialize-database", StringComparer.Ordinal);
        var bootstrapAdmin = args.Contains("--bootstrap-admin", StringComparer.Ordinal);
        if (bootstrapAdmin && !initializeDatabase)
        {
            throw new ArgumentException("--bootstrap-admin requires --initialize-database.");
        }

        return new DatabaseInitializationCommand(initializeDatabase, bootstrapAdmin);
    }
}

public interface IDatabaseMigrationApplier
{
    Task ApplyAsync(CancellationToken cancellationToken = default);
}

public sealed class EfCoreDatabaseMigrationApplier(VivoiceDbContext db) : IDatabaseMigrationApplier
{
    public Task ApplyAsync(CancellationToken cancellationToken = default) => db.Database.MigrateAsync(cancellationToken);
}

public interface IContentSeedRunner
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public sealed class ContentSeedRunner(VivoiceDbContext db) : IContentSeedRunner
{
    public Task SeedAsync(CancellationToken cancellationToken = default) => DatabaseSeeder.SeedAsync(db, cancellationToken);
}

public interface IAdminBootstrapper
{
    Task BootstrapAsync(CancellationToken cancellationToken = default);
}

public interface IDatabaseInitializer
{
    Task InitializeAsync(bool bootstrapAdmin, CancellationToken cancellationToken = default);
}

public sealed class DatabaseInitializer(
    IDatabaseMigrationApplier migrationApplier,
    IContentSeedRunner contentSeedRunner,
    IAdminBootstrapper adminBootstrapper) : IDatabaseInitializer
{
    public async Task InitializeAsync(bool bootstrapAdmin, CancellationToken cancellationToken = default)
    {
        await migrationApplier.ApplyAsync(cancellationToken);
        await contentSeedRunner.SeedAsync(cancellationToken);

        if (bootstrapAdmin)
        {
            await adminBootstrapper.BootstrapAsync(cancellationToken);
        }
    }
}

public sealed class DatabaseInitializationCommandRunner(IDatabaseInitializer initializer)
{
    public Task RunIfRequestedAsync(DatabaseInitializationCommand command, CancellationToken cancellationToken = default) =>
        command.IsRequested
            ? initializer.InitializeAsync(command.BootstrapAdmin, cancellationToken)
            : Task.CompletedTask;
}
