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
    public long? ParentJobId { get; set; }
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

public class Place
{
    public long Id { get; set; }
    [MaxLength(200)] public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    [MaxLength(120)] public string? City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class NarrationContent
{
    public long Id { get; set; }
    public long PlaceId { get; set; }
    [MaxLength(200)] public string Title { get; set; } = "";
    [MaxLength(80)] public string Topic { get; set; } = "";
    [MaxLength(20)] public string ContentType { get; set; } = "topic";
    public string SourceText { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AppUser
{
    public long Id { get; set; }
    [MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(500)] public string PasswordHash { get; set; } = "";
    [MaxLength(32)] public string Role { get; set; } = "tourist";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
