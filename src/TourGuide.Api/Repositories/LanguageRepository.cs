using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TourGuide.Api.Data;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Repositories;

public class LanguageRepository : ILanguageRepository
{
    private readonly IntegrationDbContext _db;
    public LanguageRepository(IntegrationDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(string code, CancellationToken ct) =>
        await _db.Languages.AnyAsync(l => l.Code == code && l.IsActive, ct);

    public async Task<IReadOnlyList<Language>> ListAsync(CancellationToken ct) =>
        await _db.Languages.AsNoTracking().OrderBy(l => l.Code).ToListAsync(ct);

    public async Task AddAsync(Language language, CancellationToken ct)
    {
        await _db.Languages.AddAsync(language, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Language?> GetByIdAsync(int id, CancellationToken ct) =>
        await _db.Languages.FindAsync(new object[] { id }, ct);

    public async Task UpdateAsync(Language language, CancellationToken ct)
    {
        _db.Languages.Update(language);
        await _db.SaveChangesAsync(ct);
    }
}
