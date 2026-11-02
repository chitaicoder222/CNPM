// Purpose: Provisions an initial administrator only when the owner supplies secrets outside source control.
// Responsibility: Normalizes the three login fields and hashes the bootstrap password before persistence.
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;

namespace Vivoice.Web.Services;

public sealed class BootstrapAdminService(VivoiceDbContext db, IConfiguration configuration)
    : IAdminBootstrapper
{
    private readonly PasswordHasher<AdminUser> _hasher = new();

    public async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        var hasAnyAdmin = await db.AdminUsers.AnyAsync(cancellationToken);
        if (!BootstrapAdminPolicy.ShouldCreateInitialAdmin(hasAnyAdmin))
        {
            return;
        }

        var bootstrap = BootstrapAdminConfiguration.Require(configuration);
        var normalizedName = IdentityNormalizer.Name(bootstrap.DisplayName);
        var normalizedPhone = IdentityNormalizer.Phone(bootstrap.Phone);

        var admin = new AdminUser
        {
            DisplayName = bootstrap.DisplayName.Trim(),
            NormalizedDisplayName = normalizedName,
            Phone = bootstrap.Phone.Trim(),
            NormalizedPhone = normalizedPhone
        };
        admin.PasswordHash = _hasher.HashPassword(admin, bootstrap.Password);
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public static class BootstrapAdminPolicy
{
    public static bool ShouldCreateInitialAdmin(bool hasAnyAdmin) => !hasAnyAdmin;
}

public sealed record BootstrapAdminConfiguration(string DisplayName, string Phone, string Password)
{
    public static BootstrapAdminConfiguration Require(IConfiguration configuration)
    {
        var displayName = configuration["BootstrapAdmin:DisplayName"];
        var phone = configuration["BootstrapAdmin:Phone"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
        {
            throw new BootstrapAdminConfigurationException();
        }

        return new BootstrapAdminConfiguration(displayName, phone, password);
    }
}

public sealed class BootstrapAdminConfigurationException()
    : InvalidOperationException("Bootstrap Admin requires DisplayName, Phone and Password when no Admin exists.");

public static class IdentityNormalizer
{
    public static string Name(string value) => value.Trim().ToUpperInvariant();

    public static string Phone(string value) => new(value.Where(char.IsDigit).ToArray());
}
