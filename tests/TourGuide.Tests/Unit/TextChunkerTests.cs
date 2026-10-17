using TourGuide.Api.Utils;

namespace TourGuide.Tests.Unit;

public class TextChunkerTests
{
    [Fact]
    public void Split_EmptyText_ReturnsNoChunks()
    {
        Assert.Empty(TextChunker.Split("   "));
    }

    [Fact]
    public void Split_ShortText_ReturnsSingleChunk()
    {
        var chunks = TextChunker.Split("Chùa Một Cột nằm ở Hà Nội.");
        Assert.Single(chunks);
        Assert.Equal("Chùa Một Cột nằm ở Hà Nội.", chunks[0]);
    }

    [Fact]
    public void Split_PacksWholeSentences_WithoutCuttingThem()
    {
        var chunks = TextChunker.Split("Một. Hai. Ba.", 10);
        Assert.Equal(new[] { "Một. Hai.", "Ba." }, chunks);
    }

    [Fact]
    public void Split_NeverExceedsMaxLength_AndKeepsAllWords()
    {
        var text = string.Join(" ", Enumerable.Range(1, 300).Select(i => $"từ{i}"));
        var chunks = TextChunker.Split(text, 50);

        Assert.All(chunks, c => Assert.True(c.Length <= 50));
        Assert.Equal(text, string.Join(" ", chunks));
    }

    [Fact]
    public void Split_VeryLongWord_IsCutIntoPieces()
    {
        var chunks = TextChunker.Split(new string('x', 25), 10);
        Assert.Equal(new[] { 10, 10, 5 }, chunks.Select(c => c.Length));
    }

    [Fact]
    public void Split_DoesNotBreakOnDecimalPoint()
    {
        var chunks = TextChunker.Split("Cao 3.5 mét.", 7);
        Assert.DoesNotContain(chunks, c => c.EndsWith("3."));
    }

    [Fact]
    public void Split_InvalidMaxLength_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TextChunker.Split("abc", 0));
    }
}
