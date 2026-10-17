using System.Security.Cryptography;
using System.Text;
using TourGuide.Api.Providers;
using TourGuide.Api.Utils;

namespace TourGuide.Api.Services;

public class TranslationService(ITranslationProvider provider)
{
    /// <summary>Ngôn ngữ gốc của mô tả địa điểm.</summary>
    public const string SourceLanguage = "vi";

    /// <summary>SHA-256 của (ngôn ngữ nguồn + nội dung gốc), dùng làm khóa cache.</summary>
    public static string Hash(string text, string source = SourceLanguage) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source + ":" + text))).ToLowerInvariant();

    public async Task<string> TranslateAsync(string text, string target, CancellationToken ct)
    {
        var parts = new List<string>();
        foreach (var chunk in TextChunker.Split(text))
            parts.Add(await provider.TranslateAsync(chunk, SourceLanguage, target, ct));

        if (parts.Count == 0) throw new InvalidOperationException("Nothing to translate");

        // Tiếng Trung và Nhật không dùng khoảng trắng giữa các câu.
        var noSpace = target.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                   || target.StartsWith("ja", StringComparison.OrdinalIgnoreCase);
        return string.Join(noSpace ? "" : " ", parts);
    }
}
