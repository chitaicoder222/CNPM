using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Data;

/// <summary>Nạp sẵn ngôn ngữ khi bảng languages còn trống (sau này Tài có thể thay bằng seed chung).</summary>
public static class LanguageSeeder
{
    public static async Task SeedAsync(IntegrationDbContext db, CancellationToken ct = default)
    {
        if (await db.Languages.AnyAsync(ct)) return;

        db.Languages.AddRange(
            new Language { Code = "vi", Name = "Tiếng Việt", TtsVoice = "vi-VN-HoaiMyNeural" },
            new Language { Code = "en", Name = "English", TtsVoice = "en-US-AriaNeural" },
            new Language { Code = "zh", Name = "中文", TtsVoice = "zh-CN-XiaoxiaoNeural" },
            new Language { Code = "ja", Name = "日本語", TtsVoice = "ja-JP-NanamiNeural" },
            new Language { Code = "ko", Name = "한국어", TtsVoice = "ko-KR-SunHiNeural" });

        await db.SaveChangesAsync(ct);
    }
}
