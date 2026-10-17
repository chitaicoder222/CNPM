using TourGuide.Api.Data.Entities;
using TourGuide.Api.Providers;
using TourGuide.Api.Repositories;

namespace TourGuide.Api.Services;

public sealed class LanguageService(ILanguageRepository repository)
{
    public Task<IReadOnlyList<Language>> ListAsync(CancellationToken ct) => repository.ListAsync(ct);
    public Task<bool> ExistsAsync(string code, CancellationToken ct) => repository.ExistsAsync(code, ct);
    public Task<Language?> GetAsync(int id, CancellationToken ct) => repository.GetByIdAsync(id, ct);
    public async Task<Language> CreateAsync(string code, string name, string? voice, bool active, CancellationToken ct)
    {
        var language = new Language { Code = code.Trim().ToLowerInvariant(), Name = name.Trim(), TtsVoice = string.IsNullOrWhiteSpace(voice) ? EdgeTtsProvider.DefaultVoiceFor(code) ?? "" : voice.Trim(), IsActive = active };
        await repository.AddAsync(language, ct); return language;
    }
    public async Task<bool> UpdateAsync(int id, string name, string? voice, bool active, CancellationToken ct)
    {
        var language = await repository.GetByIdAsync(id, ct); if (language is null) return false;
        language.Name = name.Trim(); if (!string.IsNullOrWhiteSpace(voice)) language.TtsVoice = voice.Trim(); language.IsActive = active;
        await repository.UpdateAsync(language, ct); return true;
    }
}
