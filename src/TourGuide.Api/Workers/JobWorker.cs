using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;
using TourGuide.Api.Data.Entities;
using TourGuide.Api.Services;

namespace TourGuide.Api.Workers;

public class JobWorker(IServiceScopeFactory scopes, ILogger<JobWorker> logger) : BackgroundService
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan StuckAfter = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var worked = false;
            try
            {
                worked = await ProcessOne(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job worker error");
            }

            // Còn việc thì làm tiếp ngay; hết việc hoặc lỗi hệ thống thì nghỉ một chút.
            if (!worked) await Task.Delay(IdleDelay, stoppingToken);
        }
    }

    /// <returns>true nếu đã xử lý một job.</returns>
    private async Task<bool> ProcessOne(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();

        var now = DateTime.UtcNow;
        var stuckBefore = now - StuckAfter;

        // Job processing quá 10 phút coi là kẹt, đưa về pending.
        await db.Jobs
            .Where(x => x.Status == "processing" && x.StartedAt < stuckBefore)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "pending").SetProperty(x => x.RunAfter, now), ct);

        var candidate = await db.Jobs
            .Where(x => x.Status == "pending" && x.RunAfter <= now)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);
        if (candidate == 0) return false;

        // Nhận việc bằng UPDATE có điều kiện: hai worker không thể cùng nhận một job.
        var claimed = await db.Jobs
            .Where(x => x.Id == candidate && x.Status == "pending")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "processing").SetProperty(x => x.StartedAt, now), ct);
        if (claimed != 1) return true;

        var job = await db.Jobs.FindAsync(new object[] { candidate }, ct);
        if (job == null) return true;

        try
        {
            switch (job.Type)
            {
                case "translate":
                    var translation = await scope.ServiceProvider.GetRequiredService<TranslationService>()
                        .TranslateAsync(job.SourceText, job.TargetLanguage, ct);
                    job.Result = translation;
                    await EnqueueTtsAsync(db, job, translation, ct);
                    break;

                case "tts":
                    job.AudioPath = await scope.ServiceProvider.GetRequiredService<TtsService>()
                        .GenerateAsync(job.SourceText, job.TargetLanguage, ct);
                    if (job.ParentJobId is long parentId)
                    {
                        var parent = await db.Jobs.FindAsync(new object[] { parentId }, ct);
                        if (parent != null)
                        {
                            parent.AudioPath = job.AudioPath;
                            parent.Status = "done";
                        }
                    }
                    break;

                default:
                    throw new InvalidOperationException("Unknown job type");
            }

            job.Status = "done";
            job.LastError = null;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            job.RetryCount++;
            job.LastError = ex.Message;
            job.Status = job.RetryCount >= MaxRetries ? "failed" : "pending";
            job.RunAfter = DateTime.UtcNow.AddSeconds(10 * Math.Pow(2, job.RetryCount));
            logger.LogWarning(ex, "Job {Id} failed (lần {Count})", job.Id, job.RetryCount);
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    // Dịch xong thì tạo job đọc; không tạo trùng nếu job đọc cho cùng nội dung đã có.
    private static async Task EnqueueTtsAsync(IntegrationDbContext db, TranslationJob job, string text, CancellationToken ct)
    {
        var exists = await db.Jobs.AnyAsync(x =>
            x.Type == "tts" && x.PlaceId == job.PlaceId && x.TargetLanguage == job.TargetLanguage
            && x.SourceHash == job.SourceHash && x.Status != "failed", ct);
        if (exists) return;

        db.Jobs.Add(new TranslationJob
        {
            ParentJobId = job.Id,
            PlaceId = job.PlaceId,
            TargetLanguage = job.TargetLanguage,
            SourceText = text,
            SourceHash = job.SourceHash,
            Type = "tts"
        });
    }
}
