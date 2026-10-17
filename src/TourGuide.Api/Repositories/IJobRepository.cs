using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TourGuide.Api.Data;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Repositories;

public interface IJobRepository
{
    Task<TranslationJob?> GetByIdAsync(long id, CancellationToken ct);
    Task<TranslationJob?> GetTtsChildAsync(long parentId, CancellationToken ct);
    Task<IReadOnlyList<TranslationJob>> ListAsync(CancellationToken ct);
    Task AddAsync(TranslationJob job, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<TranslationJob> jobs, CancellationToken ct);
    Task<TranslationJob?> FindCachedAsync(long placeId, string targetLanguage, string sourceHash, CancellationToken ct);
    Task UpdateAsync(TranslationJob job, CancellationToken ct);
}
