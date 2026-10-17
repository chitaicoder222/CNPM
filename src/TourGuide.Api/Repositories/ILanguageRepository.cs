using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Repositories;

public interface ILanguageRepository
{
    Task<bool> ExistsAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Language>> ListAsync(CancellationToken ct);
    Task AddAsync(Language language, CancellationToken ct);
    Task<Language?> GetByIdAsync(int id, CancellationToken ct);
    Task UpdateAsync(Language language, CancellationToken ct);
}
