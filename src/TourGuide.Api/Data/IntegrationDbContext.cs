using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Data;

public class IntegrationDbContext(
    DbContextOptions<IntegrationDbContext> options
) : DbContext(options)
{
    public DbSet<Language> Languages => Set<Language>();

    public DbSet<TranslationJob> Jobs => Set<TranslationJob>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Bang ngon ngu
        b.Entity<Language>().ToTable("languages");

        // Bang Job rieng cho module dich thuat va TTS
        b.Entity<TranslationJob>().ToTable("translation_jobs");

        b.Entity<Language>()
            .HasIndex(x => x.Code)
            .IsUnique();

        b.Entity<TranslationJob>()
            .HasIndex(x => new
            {
                x.Status,
                x.RunAfter
            });

        b.Entity<TranslationJob>()
            .HasIndex(x => new
            {
                x.PlaceId,
                x.TargetLanguage,
                x.SourceHash,
                x.Type
            });
    }
}