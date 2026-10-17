using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;
using TourGuide.Api.Providers;
using TourGuide.Api.Utils;

namespace TourGuide.Api.Services;

public class TtsService(
    ITtsProvider provider,
    IntegrationDbContext db,
    IConfiguration config,
    IWebHostEnvironment env)
{
    // Edge TTS nhận văn bản qua tham số dòng lệnh nên chia đoạn vừa phải, rồi ghép các file mp3.
    private const int MaxChunkLength = 2000;

    /// <summary>Thư mục gốc lưu file (cấu hình Storage:Root; đường dẫn tương đối tính từ thư mục project).</summary>
    public string StorageRoot => Path.GetFullPath(config["Storage:Root"] ?? "storage", env.ContentRootPath);

    /// <summary>
    /// Tạo audio cho văn bản và trả về đường dẫn tương đối (audio/{hash}.mp3).
    /// Cùng văn bản và cùng giọng thì dùng lại file cũ (cache audio, an toàn khi job bị chạy lại).
    /// </summary>
    public async Task<string> GenerateAsync(string text, string language, CancellationToken ct)
    {
        var code = language.ToLowerInvariant();

        // Giọng lấy từ bảng languages (Admin chỉnh được); thiếu thì dùng giọng mặc định.
        var voice = await db.Languages.AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => x.TtsVoice)
            .FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(voice)) voice = EdgeTtsProvider.DefaultVoiceFor(code);
        if (string.IsNullOrWhiteSpace(voice))
            throw new InvalidOperationException($"No TTS voice configured for language '{code}'");

        var relative = $"audio/{AudioHash(voice, text)}.mp3";
        var fullPath = Path.Combine(StorageRoot, relative);
        if (File.Exists(fullPath) && new FileInfo(fullPath).Length > 0) return relative;

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var tempDir = Path.Combine(Path.GetTempPath(), "tourguide-tts", Guid.NewGuid().ToString("N"));
        var partial = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        Directory.CreateDirectory(tempDir);

        try
        {
            var count = 0;
            await using (var output = File.Create(partial))
            {
                foreach (var chunk in TextChunker.Split(text, MaxChunkLength))
                {
                    var part = Path.Combine(tempDir, $"{count++}.mp3");
                    await provider.SynthesizeAsync(chunk, voice, part, ct);
                    await using var input = File.OpenRead(part);
                    await input.CopyToAsync(output, ct);
                }
            }

            if (count == 0) throw new InvalidOperationException("Nothing to synthesize");

            // Ghi ra file tạm rồi mới đổi tên, tránh để lại file dở dang bị coi là cache.
            File.Move(partial, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(partial)) File.Delete(partial);
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }

        return relative;
    }

    private static string AudioHash(string voice, string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(voice + ":" + text))).ToLowerInvariant();
}
