// Purpose: Verifies static VIVOICE baseline translation coverage without requiring SQL Server.
// Responsibility: Guards the six-POI, eighteen-topic, five-language seed contract and prevents accidental baseline gaps.
using Vivoice.Web.Data;

namespace Vivoice.Web.Tests.Unit;

public sealed class BaselineContentTests
{
    [Fact]
    public void BaselineContainsSixPoisAndFiveTranslationsForEachPoi()
    {
        Assert.Equal(6, VivoiceBaselineContent.Pois.Count);
        Assert.All(VivoiceBaselineContent.Pois, poi =>
        {
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Count, poi.Translations.Count);
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Order(), poi.Translations.Keys.Order());
        });
    }

    [Fact]
    public void BaselineContainsThreeTopicsPerPoiAndFiveTranslationsForEachTopic()
    {
        Assert.Equal(18, VivoiceBaselineContent.Pois.Sum(poi => poi.Topics.Count));
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
    public void BaselineHasNinetySixNonVietnameseTranslationRecords()
    {
        var poiTranslations = VivoiceBaselineContent.Pois.Sum(poi => poi.Translations.Keys.Count(language => language != "vi"));
        var topicTranslations = VivoiceBaselineContent.Pois.Sum(poi => poi.Topics.Sum(topic => topic.Translations.Keys.Count(language => language != "vi")));

        Assert.Equal(24, poiTranslations);
        Assert.Equal(72, topicTranslations);
        Assert.Equal(96, poiTranslations + topicTranslations);
    }

    [Fact]
    public void VietnameseNarrationsMeetTheSubstantiveSourceLengthTarget()
    {
        var VietnameseNarrations = VivoiceBaselineContent.Pois
            .SelectMany(poi => poi.Topics)
            .Select(topic => topic.Translations["vi"].Content)
            .ToList();

        Assert.Equal(18, VietnameseNarrations.Count);
        Assert.Equal(18, VietnameseNarrations.Distinct(StringComparer.Ordinal).Count());
        Assert.All(VietnameseNarrations, narration =>
        {
            var wordCount = narration.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.InRange(wordCount, 350, 520);
        });
    }
}
