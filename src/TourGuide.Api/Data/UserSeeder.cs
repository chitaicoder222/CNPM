using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Data;

public static class UserSeeder
{
    /// <summary>Provisioning is opt-in and reads credentials only from environment variables.</summary>
    public static async Task SeedConfiguredAsync(IntegrationDbContext db, IConfiguration config, CancellationToken ct = default)
    {
        var definitions = new[]
        {
            (Email: config["ADMIN_EMAIL"], Password: config["ADMIN_PASSWORD"], Role: "admin"),
            (Email: config["STAFF_EMAIL"], Password: config["STAFF_PASSWORD"], Role: "staff")
        };
        var hasher = new PasswordHasher<AppUser>();
        foreach (var definition in definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Email) || string.IsNullOrWhiteSpace(definition.Password)) continue;
            var email = definition.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(x => x.Email == email, ct)) continue;
            var user = new AppUser { Email = email, Role = definition.Role };
            user.PasswordHash = hasher.HashPassword(user, definition.Password);
            db.Users.Add(user);
        }
        await db.SaveChangesAsync(ct);
    }
}
