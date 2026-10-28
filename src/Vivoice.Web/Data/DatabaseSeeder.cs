// Purpose: Adds non-sensitive VIVOICE reference content through the explicit database initializer.
// Responsibility: Seeds an add-only rate/POI/topic/translation baseline without overwriting Administrator-managed content.
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data.Entities;

namespace Vivoice.Web.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(VivoiceDbContext db, CancellationToken cancellationToken = default)
    {
        var definitionsBySlug = VivoiceBaselineContent.Pois.ToDictionary(item => item.Slug, StringComparer.Ordinal);
        var existingPois = await db.Pois
            .Include(poi => poi.Translations)
            .Include(poi => poi.Topics)
            .ThenInclude(topic => topic.Translations)
            .Where(poi => definitionsBySlug.Keys.Contains(poi.Slug))
            .ToListAsync(cancellationToken);
        var poisBySlug = existingPois.ToDictionary(item => item.Slug, StringComparer.Ordinal);
        var hasRevenueRate = await db.VisitRevenueRates.AnyAsync(cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!hasRevenueRate)
            {
                db.VisitRevenueRates.Add(new VisitRevenueRate
                {
                    Amount = 0.10m,
                    Currency = "USD",
                    EffectiveFromUtc = DateTime.UtcNow.Date
                });
            }

            foreach (var definition in VivoiceBaselineContent.Pois)
            {
                if (!poisBySlug.TryGetValue(definition.Slug, out var existingPoi))
                {
                    db.Pois.Add(CreatePoi(definition));
                    continue;
                }

                AddMissingTranslations(existingPoi, definition);
            }

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Poi CreatePoi(BaselinePoiDefinition definition) => new()
    {
        Slug = definition.Slug,
        Latitude = definition.Latitude,
        Longitude = definition.Longitude,
        DisplayOrder = definition.DisplayOrder,
        Translations = definition.Translations.Select(pair => new PoiTranslation
        {
            LanguageCode = pair.Key,
            Name = pair.Value.Name,
            Summary = pair.Value.Summary
        }).ToList(),
        Topics = definition.Topics.Select(CreateTopic).ToList()
    };

    private static PoiTopic CreateTopic(BaselineTopicDefinition definition) => new()
    {
        Category = definition.Category,
        DisplayOrder = definition.DisplayOrder,
        Translations = definition.Translations.Select(pair => new TopicTranslation
        {
            LanguageCode = pair.Key,
            Title = pair.Value.Title,
            Content = pair.Value.Content,
            IsPublished = true
        }).ToList()
    };

    private static void AddMissingTranslations(Poi existingPoi, BaselinePoiDefinition definition)
    {
        foreach (var translation in definition.Translations.Where(pair => existingPoi.Translations.All(existing => !string.Equals(existing.LanguageCode, pair.Key, StringComparison.OrdinalIgnoreCase))))
        {
            existingPoi.Translations.Add(new PoiTranslation
            {
                LanguageCode = translation.Key,
                Name = translation.Value.Name,
                Summary = translation.Value.Summary
            });
        }

        foreach (var topicDefinition in definition.Topics)
        {
            var existingTopic = existingPoi.Topics.FirstOrDefault(topic => string.Equals(topic.Category, topicDefinition.Category, StringComparison.OrdinalIgnoreCase));
            if (existingTopic is null)
            {
                existingPoi.Topics.Add(CreateTopic(topicDefinition));
                continue;
            }

            foreach (var translation in topicDefinition.Translations.Where(pair => existingTopic.Translations.All(existing => !string.Equals(existing.LanguageCode, pair.Key, StringComparison.OrdinalIgnoreCase))))
            {
                existingTopic.Translations.Add(new TopicTranslation
                {
                    LanguageCode = translation.Key,
                    Title = translation.Value.Title,
                    Content = translation.Value.Content,
                    IsPublished = true
                });
            }
        }
    }
}

public readonly record struct ContentSeedPlan(bool CreateRevenueRate, bool CreatePois)
{
    public bool HasWork => CreateRevenueRate || CreatePois;

    public static ContentSeedPlan Create(bool hasRevenueRate, bool hasPoi) => new(!hasRevenueRate, !hasPoi);
}
