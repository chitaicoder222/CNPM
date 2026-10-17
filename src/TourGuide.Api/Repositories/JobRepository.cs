using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TourGuide.Api.Data;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Repositories;

public class JobRepository : IJobRepository
{
    private readonly IntegrationDbContext _db;
    public JobRepository(IntegrationDbContext db) => _db = db;

    public async Task<TranslationJob?> GetByIdAsync(long id, CancellationToken ct)
    {
        return await _db.Jobs.FindAsync(new object[] { id }, ct);
    }

    public Task<TranslationJob?> GetTtsChildAsync(long parentId, CancellationToken ct) =>
        _db.Jobs.AsNoTracking().FirstOrDefaultAsync(x => x.ParentJobId == parentId && x.Type == "tts", ct);

    public async Task<IReadOnlyList<TranslationJob>> ListAsync(CancellationToken ct)
    {
        return await _db.Jobs.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(TranslationJob job, CancellationToken ct)
    {
        await _db.Jobs.AddAsync(job, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<TranslationJob> jobs, CancellationToken ct)
    {
        await _db.Jobs.AddRangeAsync(jobs, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<TranslationJob?> FindCachedAsync(long placeId, string targetLanguage, string sourceHash, CancellationToken ct) =>
        _db.Jobs.Where(x => x.PlaceId == placeId && x.TargetLanguage == targetLanguage && x.SourceHash == sourceHash && x.Type == "translate" && x.Status != "failed").OrderByDescending(x => x.Id).FirstOrDefaultAsync(ct);

    public async Task UpdateAsync(TranslationJob job, CancellationToken ct)
    {
        _db.Jobs.Update(job);
        await _db.SaveChangesAsync(ct);
    }
}
