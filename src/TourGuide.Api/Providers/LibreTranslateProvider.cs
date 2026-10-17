using System.Net.Http.Json;
using System.Text.Json;

namespace TourGuide.Api.Providers;

public class LibreTranslateProvider(HttpClient client, IConfiguration config) : ITranslationProvider
{
    // Tự host không cần api key; để trống nếu không dùng.
    private readonly string? _apiKey = config["LibreTranslate:ApiKey"];

    public async Task<string> TranslateAsync(string text, string source, string target, CancellationToken ct)
    {
        // LibreTranslate commonly exposes Simplified Chinese as zh-Hans while
        // the application keeps the short zh code for backwards compatibility.
        var providerTarget = target.Equals("zh", StringComparison.OrdinalIgnoreCase) ? "zh-Hans" : target;
        var body = new Dictionary<string, string>
        {
            ["q"] = text,
            ["source"] = source,
            ["target"] = providerTarget,
            ["format"] = "text"
        };
        if (!string.IsNullOrWhiteSpace(_apiKey)) body["api_key"] = _apiKey;

        using var response = await client.PostAsJsonAsync("translate", body, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        // Kèm nội dung lỗi để LastError của job cho biết lý do (ví dụ cặp ngôn ngữ không hỗ trợ).
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"LibreTranslate {(int)response.StatusCode}: {json}");

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("translatedText", out var result))
            throw new InvalidOperationException("LibreTranslate response missing translatedText");

        return result.GetString() ?? throw new InvalidOperationException("Empty translation");
    }

    public async Task<IReadOnlyList<ProviderLanguage>> GetLanguagesAsync(CancellationToken ct)
    {
        using var response = await client.GetAsync("languages", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"LibreTranslate {(int)response.StatusCode}: {json}");

        using var doc = JsonDocument.Parse(json);
        var list = new List<ProviderLanguage>();
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var targets = new List<string>();
            if (item.TryGetProperty("targets", out var t))
                foreach (var x in t.EnumerateArray())
                    if (x.GetString() is { } code) targets.Add(code);

            list.Add(new ProviderLanguage(
                item.GetProperty("code").GetString() ?? "",
                item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                targets));
        }
        return list;
    }
}
