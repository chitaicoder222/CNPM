// Purpose: Provisions an initial administrator only when the owner supplies secrets outside source control.
// Responsibility: Normalizes the three login fields and hashes the bootstrap password before persistence.
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;

namespace Vivoice.Web.Services;

public sealed class BootstrapAdminService(VivoiceDbContext db, IConfiguration configuration)
{
    private readonly PasswordHasher<AdminUser> _hasher = new();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var displayName = configuration["BootstrapAdmin:DisplayName"];
        var phone = configuration["BootstrapAdmin:Phone"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var normalizedName = IdentityNormalizer.Name(displayName);
        var normalizedPhone = IdentityNormalizer.Phone(phone);
        if (await db.AdminUsers.AnyAsync(x => x.NormalizedDisplayName == normalizedName && x.NormalizedPhone == normalizedPhone, cancellationToken))
        {
            return;
        }

        var admin = new AdminUser
        {
            DisplayName = displayName.Trim(),
            NormalizedDisplayName = normalizedName,
            Phone = phone.Trim(),
            NormalizedPhone = normalizedPhone
        };
        admin.PasswordHash = _hasher.HashPassword(admin, password);
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public static class IdentityNormalizer
{
    public static string Name(string value) => value.Trim().ToUpperInvariant();

    public static string Phone(string value) => new(value.Where(char.IsDigit).ToArray());
}
