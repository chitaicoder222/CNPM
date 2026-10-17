using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Data;

/// <summary>Nạp sẵn ngôn ngữ khi bảng languages còn trống (sau này Tài có thể thay bằng seed chung).</summary>
public static class LanguageSeeder
{
    public static async Task SeedAsync(IntegrationDbContext db, CancellationToken ct = default)
    {
        if (!await db.Languages.AnyAsync(ct)) db.Languages.AddRange(
            new Language { Code = "vi", Name = "Tiếng Việt", TtsVoice = "vi-VN-HoaiMyNeural" },
            new Language { Code = "en", Name = "English", TtsVoice = "en-US-AriaNeural" },
            new Language { Code = "zh", Name = "中文", TtsVoice = "zh-CN-XiaoxiaoNeural" },
            new Language { Code = "ja", Name = "日本語", TtsVoice = "ja-JP-NanamiNeural" },
            new Language { Code = "ko", Name = "한국어", TtsVoice = "ko-KR-SunHiNeural" });

        if (!await db.Places.AnyAsync(ct))
            db.Places.Add(new Place { Name = "Dia diem demo", Description = "Dia diem mau cho ban trinh dien TourGuide Narration.", City = "Ho Chi Minh" });

        await db.SaveChangesAsync(ct);

        var place = await db.Places.OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        if (place is not null && !await db.NarrationContents.AnyAsync(x => x.PlaceId == place.Id, ct))
        {
            db.NarrationContents.AddRange(
                new NarrationContent { PlaceId = place.Id, Title = "Lịch sử và nguồn gốc", Topic = "Lịch sử", SourceText = "Thành phố Hồ Chí Minh là một trung tâm văn hóa và kinh tế năng động của Việt Nam, nơi giao thoa giữa lịch sử, kiến trúc và đời sống hiện đại." },
                new NarrationContent { PlaceId = place.Id, Title = "Văn hóa và kiến trúc", Topic = "Văn hóa", SourceText = "Hãy quan sát những công trình, con phố và không gian công cộng để cảm nhận nhịp sống đa dạng của Thành phố Hồ Chí Minh." },
                new NarrationContent { PlaceId = place.Id, Title = "Cơm tấm Sài Gòn", Topic = "Ẩm thực", ContentType = "food", SourceText = "Cơm tấm Sài Gòn thường dùng hạt gạo tấm nấu mềm, ăn cùng sườn nướng, bì, chả và nước mắm pha. Đây là món ăn quen thuộc của đời sống thành phố." },
                new NarrationContent { PlaceId = place.Id, Title = "Bánh mì Sài Gòn", Topic = "Ẩm thực", ContentType = "food", SourceText = "Bánh mì Sài Gòn kết hợp vỏ bánh giòn với nhân thịt, đồ chua, rau thơm và nước sốt. Món ăn nhỏ gọn nhưng thể hiện rõ sự giao thoa ẩm thực của thành phố." });
            await db.SaveChangesAsync(ct);
        }
    }
}
