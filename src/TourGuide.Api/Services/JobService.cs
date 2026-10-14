using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Services;

public class JobService(IntegrationDbContext db)
{
    public const int MaxSourceLength = 20000;

    /// <summary>Tạo mỗi ngôn ngữ một job translate; nội dung không đổi thì dùng lại job cũ (cache).</summary>
    public async Task<List<TranslationJob>> CreateAsync(
        long placeId, string sourceText, IEnumerable<string> languages, CancellationToken ct)
    {
        if (sourceText.Length > MaxSourceLength)
            throw new ArgumentException($"SourceText vượt quá {MaxSourceLength} ký tự");

        var result = new List<TranslationJob>();
        var hash = TranslationService.Hash(sourceText);

        var codes = languages
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct();

        foreach (var code in codes)
        {
            if (code == TranslationService.SourceLanguage)
                throw new ArgumentException($"'{code}' là ngôn ngữ gốc, không cần dịch");

            if (!await db.Languages.AnyAsync(x => x.Code == code && x.IsActive, ct))
                throw new ArgumentException($"Unsupported language: {code}");

            var existing = await db.Jobs
                .Where(x => x.PlaceId == placeId && x.TargetLanguage == code && x.SourceHash == hash
                            && x.Type == "translate" && x.Status != "failed")
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(ct);

            if (existing != null)
            {
                result.Add(existing);
                continue;
            }

            var job = new TranslationJob
            {
                PlaceId = placeId,
                SourceText = sourceText,
                TargetLanguage = code,
                SourceHash = hash
            };
            db.Jobs.Add(job);
            result.Add(job);
        }

        await db.SaveChangesAsync(ct);
        return result;
    }

    /// <summary>Admin cho chạy lại job đã failed.</summary>
    public async Task<bool> RetryAsync(long id, CancellationToken ct)
    {
        var job = await db.Jobs.FindAsync(new object[] { id }, ct);
        if (job == null || job.Status != "failed") return false;

        job.Status = "pending";
        job.RetryCount = 0;
        job.LastError = null;
        job.RunAfter = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
