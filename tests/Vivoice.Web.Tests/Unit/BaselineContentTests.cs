// Purpose: Verifies static VIVOICE baseline translation coverage without requiring SQL Server.
// Responsibility: Guards the Ho Chi Minh City baseline and prevents accidental coverage gaps.
using Vivoice.Web.Data;

namespace Vivoice.Web.Tests.Unit;

public sealed class BaselineContentTests
{
    [Fact]
    public void BaselineContainsElevenPoisAndFiveTranslationsForEachPoi()
    {
        Assert.Equal(new[] { "vi", "ko", "ja", "zh", "en" }, VivoiceBaselineContent.SupportedLanguages);
        Assert.Equal(11, VivoiceBaselineContent.Pois.Count);
        Assert.All(VivoiceBaselineContent.Pois, poi =>
        {
            Assert.NotEmpty(poi.SourceUrls);
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Count, poi.Translations.Count);
            Assert.Equal(VivoiceBaselineContent.SupportedLanguages.Order(), poi.Translations.Keys.Order());
        });
    }

    [Fact]
    public void BaselineContainsThreeTopicsPerPoiAndFiveTranslationsForEachTopic()
    {
        Assert.Equal(33, VivoiceBaselineContent.Pois.Sum(poi => poi.Topics.Count));
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
    public void BaselineHasOneHundredSeventySixNonVietnameseTranslationRecords()
    {
        var poiTranslations = VivoiceBaselineContent.Pois.Sum(poi => poi.Translations.Keys.Count(language => language != "vi"));
        var topicTranslations = VivoiceBaselineContent.Pois.Sum(poi => poi.Topics.Sum(topic => topic.Translations.Keys.Count(language => language != "vi")));

        Assert.Equal(44, poiTranslations);
        Assert.Equal(132, topicTranslations);
        Assert.Equal(176, poiTranslations + topicTranslations);
    }

    [Fact]
    public void BaselineContainsSpecificLandmarksInHoChiMinhCityWithPositiveGpsRadii()
    {
        Assert.Equal(11, VivoiceBaselineContent.Pois.Select(poi => poi.Slug).Distinct().Count());
        Assert.Equal(Enumerable.Range(1, 11), VivoiceBaselineContent.Pois.Select(poi => poi.DisplayOrder));
        Assert.All(VivoiceBaselineContent.Pois, poi =>
        {
            Assert.InRange(poi.Latitude, 10.7m, 10.9m);
            Assert.InRange(poi.Longitude, 106.6m, 106.8m);
            Assert.True(double.IsFinite(poi.RadiusMeters) && poi.RadiusMeters > 0);
            Assert.Equal(new[] { "culture", "food", "tourism" }, poi.Topics.Select(topic => topic.Category));
            Assert.All(poi.Translations.Values, translation =>
            {
                Assert.False(string.IsNullOrWhiteSpace(translation.Name));
                Assert.False(string.IsNullOrWhiteSpace(translation.Summary));
            });
        });
        Assert.Contains(VivoiceBaselineContent.Pois, poi => poi.Slug == "dinh-doc-lap");
        Assert.Contains(VivoiceBaselineContent.Pois, poi => poi.Slug == "landmark-81");
        Assert.Contains(VivoiceBaselineContent.Pois, poi => poi.Slug == "lotte-mart-quan-7");
        Assert.DoesNotContain(VivoiceBaselineContent.Pois, poi => new[] { "ha-noi", "ho-chi-minh-city", "da-nang", "hue", "hoi-an", "nha-trang" }.Contains(poi.Slug));
    }

    [Fact]
    public void VietnameseNarrationsAreDistinctForEveryCurrentTopic()
    {
        var VietnameseNarrations = VivoiceBaselineContent.Pois
            .SelectMany(poi => poi.Topics)
            .Select(topic => topic.Translations["vi"].Content)
            .ToList();

        Assert.Equal(33, VietnameseNarrations.Count);
        Assert.Equal(33, VietnameseNarrations.Distinct(StringComparer.Ordinal).Count());
    }
}
