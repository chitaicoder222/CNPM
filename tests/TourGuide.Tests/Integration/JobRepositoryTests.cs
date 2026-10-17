using TourGuide.Api.Data.Entities;
using TourGuide.Api.Repositories;

namespace TourGuide.Tests.Integration;

public class JobRepositoryTests
{
    [Fact]
    public async Task TtsChild_IsResolvedByParentId()
    {
        await using var db = new TestDb();
        var repository = new JobRepository(db.Context);
        var parent = new TranslationJob { PlaceId = 1, TargetLanguage = "en", SourceText = "xin chao", SourceHash = "h", Status = "done" };
        db.Context.Jobs.Add(parent);
        await db.Context.SaveChangesAsync();
        db.Context.Jobs.Add(new TranslationJob { ParentJobId = parent.Id, PlaceId = 1, TargetLanguage = "en", SourceText = "hello", SourceHash = "h", Type = "tts", Status = "pending" });
        await db.Context.SaveChangesAsync();

        var child = await repository.GetTtsChildAsync(parent.Id, CancellationToken.None);
        Assert.NotNull(child);
        Assert.Equal(parent.Id, child!.ParentJobId);
        Assert.Equal("tts", child.Type);
    }
}
