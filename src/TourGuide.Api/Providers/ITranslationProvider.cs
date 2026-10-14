namespace TourGuide.Api.Providers;

/// <summary>Một ngôn ngữ mà nhà cung cấp dịch hỗ trợ, kèm danh sách ngôn ngữ đích dịch được.</summary>
public record ProviderLanguage(string Code, string Name, IReadOnlyList<string> Targets);

public interface ITranslationProvider
{
    Task<string> TranslateAsync(string text, string source, string target, CancellationToken ct);

    /// <summary>Lấy danh sách ngôn ngữ nhà cung cấp hỗ trợ (dùng để kiểm tra vi → en/zh/ja).</summary>
    Task<IReadOnlyList<ProviderLanguage>> GetLanguagesAsync(CancellationToken ct);
}
