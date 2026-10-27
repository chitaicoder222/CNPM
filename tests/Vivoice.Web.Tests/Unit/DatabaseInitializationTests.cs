// Purpose: Verifies explicit database-initialization policies without connecting to SQL Server.
// Responsibility: Guards normal startup, phase ordering, bootstrap safety, idempotent seed decisions and readiness semantics.
using Microsoft.Extensions.Configuration;
using Vivoice.Web.Data;
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Unit;

public sealed class DatabaseInitializationTests
{
    [Fact]
    public async Task NormalStartupCommand_DoesNotInvokeInitializer()
    {
        var initializer = new RecordingInitializer();
        var runner = new DatabaseInitializationCommandRunner(initializer);

        await runner.RunIfRequestedAsync(DatabaseInitializationCommand.Parse([]));

        Assert.False(initializer.WasCalled);
    }

    [Fact]
    public void BootstrapFlagWithoutInitializer_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => DatabaseInitializationCommand.Parse(["--bootstrap-admin"]));
    }

    [Fact]
    public async Task Initializer_RunsMigrationThenSeedThenRequestedBootstrap()
    {
        var order = new List<string>();
        var initializer = new DatabaseInitializer(
            new RecordingMigration(order),
            new RecordingContentSeed(order),
            new RecordingBootstrap(order));

        await initializer.InitializeAsync(bootstrapAdmin: true);

        Assert.Equal(["migration", "seed", "bootstrap"], order);
    }

    [Fact]
    public async Task MigrationFailure_StopsContentSeedAndBootstrap()
    {
        var order = new List<string>();
        var initializer = new DatabaseInitializer(
            new ThrowingMigration(order),
            new RecordingContentSeed(order),
            new RecordingBootstrap(order));

        await Assert.ThrowsAsync<InvalidOperationException>(() => initializer.InitializeAsync(bootstrapAdmin: true));

        Assert.Equal(["migration"], order);
    }

    [Fact]
    public void MissingConnectionString_FailsWithoutReadingASecret()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() => VivoiceConnectionString.Require(configuration));

        Assert.Contains("ConnectionStrings:Vivoice", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingBootstrapConfiguration_IsRejectedWhenBootstrapIsNeeded()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<BootstrapAdminConfigurationException>(() => BootstrapAdminConfiguration.Require(configuration));

        Assert.Contains("Bootstrap Admin requires", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExistingAdmin_PreventsAutomaticBootstrap()
    {
        Assert.False(BootstrapAdminPolicy.ShouldCreateInitialAdmin(hasAnyAdmin: true));
        Assert.True(BootstrapAdminPolicy.ShouldCreateInitialAdmin(hasAnyAdmin: false));
    }

    [Fact]
    public void ExistingSeedData_DoesNotScheduleDuplicateContent()
    {
        var plan = ContentSeedPlan.Create(hasRevenueRate: true, hasPoi: true);

        Assert.False(plan.HasWork);
        Assert.False(plan.CreateRevenueRate);
        Assert.False(plan.CreatePois);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    public void Readiness_RequiresConnectionAndNoPendingMigrations(bool canConnect, bool hasPendingMigrations, bool expected)
    {
        Assert.Equal(expected, DatabaseReadinessPolicy.IsReady(canConnect, hasPendingMigrations));
    }

    private sealed class RecordingInitializer : IDatabaseInitializer
    {
        public bool WasCalled { get; private set; }

        public Task InitializeAsync(bool bootstrapAdmin, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingMigration(List<string> order) : IDatabaseMigrationApplier
    {
        public Task ApplyAsync(CancellationToken cancellationToken = default)
        {
            order.Add("migration");
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingMigration(List<string> order) : IDatabaseMigrationApplier
    {
        public Task ApplyAsync(CancellationToken cancellationToken = default)
        {
            order.Add("migration");
            throw new InvalidOperationException("Simulated migration failure.");
        }
    }

    private sealed class RecordingContentSeed(List<string> order) : IContentSeedRunner
    {
        public Task SeedAsync(CancellationToken cancellationToken = default)
        {
            order.Add("seed");
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingBootstrap(List<string> order) : IAdminBootstrapper
    {
        public Task BootstrapAsync(CancellationToken cancellationToken = default)
        {
            order.Add("bootstrap");
            return Task.CompletedTask;
        }
    }
}
