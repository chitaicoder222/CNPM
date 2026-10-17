using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TourGuide.Api.Data.Entities;
using TourGuide.Api.Providers;
using TourGuide.Api.Services;

namespace TourGuide.Api.Controllers;

public class LanguageRequest
{
    [Required, RegularExpression("^[A-Za-z]{2,3}(-[A-Za-z0-9]+)?$", ErrorMessage = "Code không hợp lệ (ví dụ en, zh, zh-Hans)")]
    public string Code { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    /// <summary>Để trống thì dùng giọng mặc định theo mã ngôn ngữ.</summary>
    public string? TtsVoice { get; set; }

    public bool IsActive { get; set; } = true;
}

// TODO(auth): GET công khai; POST/PUT chỉ Admin (thêm [Authorize(Roles = "admin")] khi An xong JWT).
[ApiController]
[Route("languages")]
public class LanguagesController(LanguageService service, ITranslationProvider translator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await service.ListAsync(ct));

    /// <summary>
    /// Kiểm tra LibreTranslate có dịch được từ tiếng Việt sang từng ngôn ngữ đang bật hay không
    /// (rủi ro lớn nhất của đề tài, xem mục 3.7 của kế hoạch).
    /// </summary>
    [HttpGet("provider-check")]
    public async Task<IActionResult> ProviderCheck(CancellationToken ct)
    {
        IReadOnlyList<ProviderLanguage> provider;
        try
        {
            provider = await translator.GetLanguagesAsync(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            return StatusCode(502, new { error = "Không gọi được LibreTranslate: " + ex.Message });
        }

        var source = provider.FirstOrDefault(x => x.Code == TranslationService.SourceLanguage);
        var active = (await service.ListAsync(ct)).Where(x => x.IsActive && x.Code != TranslationService.SourceLanguage).ToList();

        bool SupportsFromVietnamese(string code) =>
            source?.Targets.Contains(code, StringComparer.OrdinalIgnoreCase) == true
            || (code.Equals("zh", StringComparison.OrdinalIgnoreCase)
                && (source?.Targets.Contains("zh-Hans", StringComparer.OrdinalIgnoreCase) == true
                    || source?.Targets.Contains("zh-Hant", StringComparer.OrdinalIgnoreCase) == true));

        return Ok(new
        {
            sourceSupported = source != null,
            viTargets = source?.Targets ?? new List<string>(),
            languages = active.Select(l => new
            {
                l.Code,
                canTranslateFromVi = SupportsFromVietnamese(l.Code),
                ttsVoice = l.TtsVoice
            }),
            providerLanguages = provider.Select(p => p.Code)
        });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] LanguageRequest request, CancellationToken ct)
    {
        var code = request.Code.Trim().ToLowerInvariant();
        if (await service.ExistsAsync(code, ct))
            return Conflict($"Ngôn ngữ '{code}' đã tồn tại");

        var voice = string.IsNullOrWhiteSpace(request.TtsVoice)
            ? EdgeTtsProvider.DefaultVoiceFor(code) ?? ""
            : request.TtsVoice.Trim();

        var lang = await service.CreateAsync(code, request.Name, voice, request.IsActive, ct);
        return CreatedAtAction(nameof(List), lang);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] LanguageRequest request, CancellationToken ct)
    {
        var lang = await service.GetAsync(id, ct);
        if (lang == null) return NotFound();

        // Mã ngôn ngữ không đổi được vì job và bản dịch đang tham chiếu theo mã.
        await service.UpdateAsync(id, request.Name, request.TtsVoice, request.IsActive, ct);
        lang = await service.GetAsync(id, ct);
        return Ok(lang);
    }
}
