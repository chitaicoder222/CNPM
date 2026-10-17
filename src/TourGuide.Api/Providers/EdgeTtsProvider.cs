using System.ComponentModel;
using System.Diagnostics;

namespace TourGuide.Api.Providers;

/// <summary>
/// Gọi CLI Python edge-tts (cài bằng: pip install edge-tts).
/// Có thể đổi lệnh bằng cấu hình Tts:EdgeCommand (ví dụ đường dẫn tuyệt đối tới edge-tts).
/// </summary>
public class EdgeTtsProvider(IConfiguration config) : ITtsProvider
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(90);
    private readonly string _command = config["Tts:EdgeCommand"] ?? "python";

    private static readonly Dictionary<string, string> DefaultVoices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vi"] = "vi-VN-HoaiMyNeural",
        ["en"] = "en-US-AriaNeural",
        ["zh"] = "zh-CN-XiaoxiaoNeural",
        ["ja"] = "ja-JP-NanamiNeural",
        ["ko"] = "ko-KR-SunHiNeural"
    };

    /// <summary>Giọng mặc định theo mã ngôn ngữ (dùng khi bảng languages chưa có giọng).</summary>
    public static string? DefaultVoiceFor(string langCode)
    {
        var key = langCode.Split('-')[0];
        return DefaultVoices.TryGetValue(key, out var voice) ? voice : null;
    }

    public async Task SynthesizeAsync(string text, string voice, string outPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text is empty", nameof(text));
        if (string.IsNullOrWhiteSpace(voice)) throw new ArgumentException("Voice is empty", nameof(voice));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);

        var psi = new ProcessStartInfo(_command)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        var commandName = Path.GetFileNameWithoutExtension(_command);
        if (commandName.Equals("python", StringComparison.OrdinalIgnoreCase)
            || commandName.Equals("python3", StringComparison.OrdinalIgnoreCase)
            || commandName.Equals("py", StringComparison.OrdinalIgnoreCase))
        {
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add("edge_tts");
        }
        psi.ArgumentList.Add("--voice");
        psi.ArgumentList.Add(voice);
        psi.ArgumentList.Add($"--text={text}");
        psi.ArgumentList.Add("--write-media");
        psi.ArgumentList.Add(outPath);

        using var process = new Process { StartInfo = psi };
        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(
                $"Cannot start '{_command}'. Cài bằng: pip install edge-tts (và đảm bảo lệnh nằm trong PATH).", ex);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(ProcessTimeout);

        // Đọc cả stdout và stderr để tiến trình không bị treo khi đầy bộ đệm.
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            await Task.WhenAll(stderr, stdout);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            if (ct.IsCancellationRequested) throw;
            throw new TimeoutException($"edge-tts quá {ProcessTimeout.TotalSeconds:0}s chưa xong");
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException("edge-tts failed: " + stderr.Result);

        if (!File.Exists(outPath) || new FileInfo(outPath).Length == 0)
            throw new InvalidOperationException("edge-tts did not produce an audio file");
    }

    private static void TryKill(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch { /* tiến trình đã thoát */ }
    }
}
