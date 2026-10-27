// Purpose: Builds the EF Core design-time context from the same private connection-string sources used by runtime.
// Responsibility: Fails closed when ConnectionStrings:Vivoice is absent and never falls back to LocalDB or another database.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Vivoice.Web.Data;

public sealed class VivoiceDbContextFactory : IDesignTimeDbContextFactory<VivoiceDbContext>
{
    public VivoiceDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environments.Production;

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false);

        if (string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
        {
            configurationBuilder.AddUserSecrets<VivoiceDbContextFactory>(optional: true);
        }

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();
        var connectionString = VivoiceConnectionString.Require(configuration, "for EF Core design-time operations");

        var options = new DbContextOptionsBuilder<VivoiceDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new VivoiceDbContext(options);
    }
}
