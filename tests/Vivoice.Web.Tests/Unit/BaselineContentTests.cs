// Purpose: Verifies static VIVOICE baseline translation coverage without requiring SQL Server.
// Responsibility: Guards the six-POI, eighteen-topic, five-language seed contract and prevents accidental baseline gaps.
using Vivoice.Web.Data;

namespace Vivoice.Web.Tests.Unit;

public sealed class BaselineContentTests
{
    [Fact]
    public void BaselineContainsTenHoChiMinhCityPoisAndFiveTranslationsForEachPoi()
    {
        Assert.Equal(10, VivoiceBaselineContent.Pois.Count);
        Assert.Contains(VivoiceBaselineContent.Pois, poi => poi.Slug == "lotte-mart-district-7" && poi.TriggerRadiusMeters == 700);
        Assert.All(VivoiceBaselineContent.Pois, poi =>
        {
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Count, poi.Translations.Count);
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Order(), poi.Translations.Keys.Order());
        });
    }

    [Fact]
    public void BaselineContainsNarrationForEveryPoiAndFiveTranslationsForEachTopic()
    {
        Assert.Equal(10, VivoiceBaselineContent.Pois.Sum(poi => poi.Topics.Count));
        Assert.All(VivoiceBaselineContent.Pois.SelectMany(poi => poi.Topics), topic =>
        {
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Count, topic.Translations.Count);
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Order(), topic.Translations.Keys.Order());
            Assert.All(topic.Translations.Values, translation =>
            {
                Assert.False(string.IsNullOrWhiteSpace(translation.Title));
                Assert.False(string.IsNullOrWhiteSpace(translation.Content));
            });
        });
    }

    [Fact]
    public void BaselineHasEightyNonVietnameseTranslationRecords()
    {
        var poiTranslations = VivoiceBaselineContent.Pois.Sum(poi => poi.Translations.Keys.Count(language => language != "vi"));
        var topicTranslations = VivoiceBaselineContent.Pois.Sum(poi => poi.Topics.Sum(topic => topic.Translations.Keys.Count(language => language != "vi")));

        Assert.Equal(40, poiTranslations);
        Assert.Equal(40, topicTranslations);
        Assert.Equal(80, poiTranslations + topicTranslations);
    }

    [Fact]
    public void LocalizedBaselineTextIsNotVietnameseFallbackCopy()
    {
        foreach (var poi in VivoiceBaselineContent.Pois)
        {
            var VietnamesePoi = poi.Translations["vi"];
            foreach (var language in VivoiceBaselineContent.SupportedLanguages.Where(language => language != "vi"))
            {
                var localizedPoi = poi.Translations[language];
                Assert.True(VietnamesePoi.Name != localizedPoi.Name || VietnamesePoi.Summary != localizedPoi.Summary);
            }

            foreach (var topic in poi.Topics)
            {
                var VietnameseTopic = topic.Translations["vi"];
                foreach (var language in VivoiceBaselineContent.SupportedLanguages.Where(language => language != "vi"))
                {
                    var localizedTopic = topic.Translations[language];
                    Assert.NotEqual(VietnameseTopic.Title, localizedTopic.Title);
                    Assert.NotEqual(VietnameseTopic.Content, localizedTopic.Content);
                }
            }
        }
    }

    [Fact]
    public void VietnameseNarrationsArePresentAndUnique()
    {
        var VietnameseNarrations = VivoiceBaselineContent.Pois
            .SelectMany(poi => poi.Topics)
            .Select(topic => topic.Translations["vi"].Content)
            .ToList();

        Assert.Equal(10, VietnameseNarrations.Count);
        Assert.Equal(10, VietnameseNarrations.Distinct(StringComparer.Ordinal).Count());
        Assert.All(VietnameseNarrations, narration =>
        {
            var wordCount = narration.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.InRange(wordCount, 12, 120);
        });
    }
}
