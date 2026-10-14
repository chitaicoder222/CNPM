using TourGuide.Api.Providers;
using TourGuide.Api.Services;

namespace TourGuide.Tests.Unit;

public class TranslationServiceTests
{
    private sealed class FakeProvider : ITranslationProvider
    {
        public List<(string Text, string Source, string Target)> Calls { get; } = new();

        public Task<string> TranslateAsync(string text, string source, string target, CancellationToken ct)
        {
            Calls.Add((text, source, target));
            return Task.FromResult($"[{target}]{text}");
        }

        public Task<IReadOnlyList<ProviderLanguage>> GetLanguagesAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ProviderLanguage>>(new List<ProviderLanguage>());
    }

    [Fact]
    public async Task Translate_ShortText_CallsProviderOnceWithViSource()
    {
        var provider = new FakeProvider();
        var result = await new TranslationService(provider).TranslateAsync("Xin chào", "en", CancellationToken.None);

        Assert.Equal("[en]Xin chào", result);
        var call = Assert.Single(provider.Calls);
        Assert.Equal("vi", call.Source);
        Assert.Equal("en", call.Target);
    }

    [Fact]
    public async Task Translate_LongText_IsSplitIntoSeveralProviderCalls()
    {
        var provider = new FakeProvider();
        var text = string.Join(" ", Enumerable.Range(1, 400).Select(i => $"Câu số {i}."));

        await new TranslationService(provider).TranslateAsync(text, "en", CancellationToken.None);

        Assert.True(provider.Calls.Count > 1);
        Assert.All(provider.Calls, c => Assert.True(c.Text.Length <= 900));
    }

    [Fact]
    public async Task Translate_Chinese_JoinsChunksWithoutSpaces()
    {
        var provider = new FakeProvider();
        var text = string.Join(" ", Enumerable.Range(1, 400).Select(i => $"Câu số {i}."));

        var result = await new TranslationService(provider).TranslateAsync(text, "zh", CancellationToken.None);

        Assert.Contains(".[zh]", result);
        Assert.DoesNotContain(". [zh]", result);
    }

    [Fact]
    public void Hash_IsStable_AndChangesWithText()
    {
        Assert.Equal(TranslationService.Hash("abc"), TranslationService.Hash("abc"));
        Assert.NotEqual(TranslationService.Hash("abc"), TranslationService.Hash("abd"));
    }
}
