// Purpose: Defines sponsorship, analytics, rate-history and audit entities for VIVOICE operations.
// Responsibility: Separates traffic estimates from contracted and received sponsorship revenue.
namespace Vivoice.Web.Data.Entities;

public sealed class Sponsor
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactNote { get; set; }
    public bool IsActive { get; set; } = true;
    public List<SponsorshipAgreement> Agreements { get; set; } = [];
}

public sealed class SponsorshipAgreement
{
    public long Id { get; set; }
    public long SponsorId { get; set; }
    public decimal CommittedAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "active";
    public DateTime StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public Sponsor Sponsor { get; set; } = null!;
    public List<SponsorshipPayment> Payments { get; set; } = [];
}

public sealed class SponsorshipPayment
{
    public long Id { get; set; }
    public long SponsorshipAgreementId { get; set; }
    public decimal ReceivedAmount { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public SponsorshipAgreement SponsorshipAgreement { get; set; } = null!;
}

public sealed class VisitRevenueRate
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
}

public sealed class VisitorSession
{
    public long Id { get; set; }
    public string SessionHash { get; set; } = string.Empty;
    public string DeviceType { get; set; } = "Desktop";
    public string BrowserFamily { get; set; } = "Other";
    public string OperatingSystem { get; set; } = "Other";
    public DateTime FirstSeenAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;
    public List<Visit> Visits { get; set; } = [];
}

public sealed class Visit
{
    public long Id { get; set; }
    public long VisitorSessionId { get; set; }
    public long? VisitRevenueRateId { get; set; }
    public decimal AppliedRateAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsValid { get; set; } = true;
    public VisitorSession VisitorSession { get; set; } = null!;
    public VisitRevenueRate? VisitRevenueRate { get; set; }
}

public sealed class AuditLog
{
    public long Id { get; set; }
    public long AdminUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? SafeDetails { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public AdminUser AdminUser { get; set; } = null!;
}
