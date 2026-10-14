using System.ComponentModel.DataAnnotations;

namespace TourGuide.Api.Data.Entities;

public class Language
{
    public int Id { get; set; }

    [MaxLength(12)]
    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string TtsVoice { get; set; } = "";

    public bool IsActive { get; set; } = true;
}

/// <summary>Hàng đợi việc dịch (type = translate) và đọc (type = tts).</summary>
public class TranslationJob
{
    public long Id { get; set; }
    public long PlaceId { get; set; }
    public string TargetLanguage { get; set; } = "";
    public string SourceText { get; set; } = "";
    public string SourceHash { get; set; } = "";
    public string Type { get; set; } = "translate";
    public string Status { get; set; } = "pending";
    public int RetryCount { get; set; }
    public DateTime RunAfter { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public string? LastError { get; set; }
    public string? Result { get; set; }
    public string? AudioPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
