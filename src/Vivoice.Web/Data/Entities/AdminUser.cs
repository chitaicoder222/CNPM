// Purpose: Defines the persisted administrator principal for protected VIVOICE operations.
// Responsibility: Stores normalized identity fields and a framework-generated password hash only.
namespace Vivoice.Web.Data.Entities;

public sealed class AdminUser
{
    public long Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string NormalizedDisplayName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NormalizedPhone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
