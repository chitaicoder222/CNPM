// Purpose: Configures the EF Core SQL Server schema and integrity rules for VIVOICE.
// Responsibility: Owns database sets, indexes, relationships and SQL-friendly column precision.
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data.Entities;

namespace Vivoice.Web.Data;

public sealed class VivoiceDbContext(DbContextOptions<VivoiceDbContext> options) : DbContext(options)
{
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Poi> Pois => Set<Poi>();
    public DbSet<PoiTranslation> PoiTranslations => Set<PoiTranslation>();
    public DbSet<PoiTopic> PoiTopics => Set<PoiTopic>();
    public DbSet<TopicTranslation> TopicTranslations => Set<TopicTranslation>();
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<AdvertisementEvent> AdvertisementEvents => Set<AdvertisementEvent>();
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();
    public DbSet<SponsorshipAgreement> SponsorshipAgreements => Set<SponsorshipAgreement>();
    public DbSet<SponsorshipPayment> SponsorshipPayments => Set<SponsorshipPayment>();
    public DbSet<VisitRevenueRate> VisitRevenueRates => Set<VisitRevenueRate>();
    public DbSet<VisitorSession> VisitorSessions => Set<VisitorSession>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasIndex(x => new { x.NormalizedDisplayName, x.NormalizedPhone }).IsUnique();
            entity.Property(x => x.DisplayName).HasMaxLength(120);
            entity.Property(x => x.NormalizedDisplayName).HasMaxLength(120);
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.NormalizedPhone).HasMaxLength(20);
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
        });

        modelBuilder.Entity<Poi>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Slug).HasMaxLength(140);
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
        });
        modelBuilder.Entity<PoiTranslation>().HasIndex(x => new { x.PoiId, x.LanguageCode }).IsUnique();
        modelBuilder.Entity<PoiTranslation>().Property(x => x.LanguageCode).HasMaxLength(10);
        modelBuilder.Entity<PoiTopic>().HasIndex(x => new { x.PoiId, x.Category, x.DisplayOrder }).IsUnique();
        modelBuilder.Entity<PoiTopic>().Property(x => x.Category).HasMaxLength(20);
        modelBuilder.Entity<TopicTranslation>().HasIndex(x => new { x.PoiTopicId, x.LanguageCode }).IsUnique();
        modelBuilder.Entity<TopicTranslation>().Property(x => x.LanguageCode).HasMaxLength(10);

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.TargetUrl).HasMaxLength(500);
            entity.Property(x => x.Placement).HasMaxLength(10);
            entity.HasIndex(x => new { x.Placement, x.IsActive, x.StartsAtUtc });
            entity.HasOne(x => x.Sponsor).WithMany().HasForeignKey(x => x.SponsorId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<AdvertisementEvent>(entity =>
        {
            entity.Property(x => x.EventType).HasMaxLength(16);
            entity.Property(x => x.SessionHash).HasMaxLength(64);
            entity.HasIndex(x => new { x.AdvertisementId, x.OccurredAtUtc });
            entity.HasIndex(x => new { x.EventType, x.OccurredAtUtc });
        });

        modelBuilder.Entity<Sponsor>().Property(x => x.Name).HasMaxLength(180);
        modelBuilder.Entity<SponsorshipAgreement>().Property(x => x.CommittedAmount).HasPrecision(18, 2);
        modelBuilder.Entity<SponsorshipAgreement>().Property(x => x.Currency).HasMaxLength(3);
        modelBuilder.Entity<SponsorshipAgreement>().Property(x => x.Status).HasMaxLength(30);
        modelBuilder.Entity<SponsorshipPayment>().Property(x => x.ReceivedAmount).HasPrecision(18, 2);
        modelBuilder.Entity<VisitRevenueRate>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 4);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.HasIndex(x => x.EffectiveFromUtc).IsUnique();
        });

        modelBuilder.Entity<VisitorSession>(entity =>
        {
            entity.HasIndex(x => x.SessionHash).IsUnique();
            entity.Property(x => x.SessionHash).HasMaxLength(64);
            entity.Property(x => x.DeviceType).HasMaxLength(12);
            entity.Property(x => x.BrowserFamily).HasMaxLength(24);
            entity.Property(x => x.OperatingSystem).HasMaxLength(24);
            entity.HasIndex(x => x.LastSeenAtUtc);
        });
        modelBuilder.Entity<Visit>(entity =>
        {
            entity.Property(x => x.AppliedRateAmount).HasPrecision(18, 4);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.HasIndex(x => new { x.VisitorSessionId, x.OccurredAtUtc });
        });
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.Action).HasMaxLength(120);
            entity.Property(x => x.EntityType).HasMaxLength(80);
            entity.Property(x => x.EntityId).HasMaxLength(80);
            entity.Property(x => x.SafeDetails).HasMaxLength(1000);
            entity.HasIndex(x => x.OccurredAtUtc);
        });
    }
}
