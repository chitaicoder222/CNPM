namespace TourGuide.Api.Providers;

public interface ITtsProvider
{
    /// <param name="voice">Tên giọng đọc của nhà cung cấp, ví dụ en-US-AriaNeural.</param>
    Task SynthesizeAsync(string text, string voice, string outPath, CancellationToken ct);
}
