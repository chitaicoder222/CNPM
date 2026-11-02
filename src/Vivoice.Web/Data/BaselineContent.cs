// Purpose: Loads the curated Ho Chi Minh City POI baseline and its multilingual narration.
// Responsibility: Keeps one embedded JSON catalog shared by the add-only seeder and the explicit demo replacement tool.
using System.Text.Json;

namespace Vivoice.Web.Data;

public sealed record BaselinePoiTranslation(string Name, string Summary);
public sealed record BaselineTopicTranslation(string Title, string Content);
public sealed record BaselineTopicDefinition(string Category, int DisplayOrder, IReadOnlyDictionary<string, BaselineTopicTranslation> Translations);
public sealed record BaselinePoiDefinition(string Slug, decimal Latitude, decimal Longitude, double RadiusMeters, int DisplayOrder, IReadOnlyDictionary<string, BaselinePoiTranslation> Translations, IReadOnlyList<BaselineTopicDefinition> Topics, IReadOnlyList<string> SourceUrls);

public static class VivoiceBaselineContent
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<string> SupportedLanguages { get; } =
        Array.AsReadOnly(["vi", "ko", "ja", "zh", "en"]);

    public static IReadOnlyList<BaselinePoiDefinition> Pois { get; } = LoadPois();

    private static IReadOnlyList<BaselinePoiDefinition> LoadPois()
    {
        var assembly = typeof(VivoiceBaselineContent).Assembly;
        using var stream = assembly.GetManifestResourceStream("Vivoice.Web.Data.BaselineContent.json")
            ?? throw new InvalidOperationException("The VIVOICE baseline content resource is missing.");

        var pois = JsonSerializer.Deserialize<BaselinePoiDefinition[]>(stream, SerializerOptions);
        if (pois is null || pois.Length == 0)
            throw new InvalidOperationException("The VIVOICE baseline content is empty.");

        return Array.AsReadOnly(pois);
    }
}
