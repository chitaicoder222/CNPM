using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Data;

public static class LanguageSeeder
{
    public static async Task SeedAsync(IntegrationDbContext db, CancellationToken ct = default)
    {
        if (!await db.Languages.AnyAsync(ct))
            db.Languages.AddRange(
                new Language { Code = "vi", Name = "Tiếng Việt", TtsVoice = "vi-VN-HoaiMyNeural" },
                new Language { Code = "en", Name = "English", TtsVoice = "en-US-AriaNeural" },
                new Language { Code = "zh", Name = "中文", TtsVoice = "zh-CN-XiaoxiaoNeural" },
                new Language { Code = "ja", Name = "日本語", TtsVoice = "ja-JP-NanamiNeural" },
                new Language { Code = "ko", Name = "한국어", TtsVoice = "ko-KR-SunHiNeural" });

        await db.SaveChangesAsync(ct);

        var demoPlace = await db.Places.FirstOrDefaultAsync(x => x.Name == "Dia diem demo", ct);
        if (demoPlace is not null && !demoPlace.IsActive)
        {
            demoPlace.IsActive = true;
            demoPlace.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        var places = new[]
        {
            new Place { Name = "Chợ Bến Thành", City = "Ho Chi Minh", Category = "Chợ và khu phố", Address = "Lê Lợi, Quận 1, TP. Hồ Chí Minh", Latitude = 10.7721m, Longitude = 106.6980m, Description = "Khu chợ biểu tượng của trung tâm Thành phố Hồ Chí Minh, nổi tiếng với ẩm thực và hoạt động mua bán sôi động." },
            new Place { Name = "Dinh Độc Lập", City = "Ho Chi Minh", Category = "Di tích lịch sử", Address = "135 Nam Kỳ Khởi Nghĩa, Quận 1, TP. Hồ Chí Minh", Latitude = 10.7770m, Longitude = 106.6953m, Description = "Công trình lịch sử giữa trung tâm thành phố, gắn với nhiều dấu mốc quan trọng của Việt Nam hiện đại." },
            new Place { Name = "Bưu điện Thành phố", City = "Ho Chi Minh", Category = "Công trình văn hóa", Address = "2 Công trường Công xã Paris, Quận 1, TP. Hồ Chí Minh", Latitude = 10.7798m, Longitude = 106.6990m, Description = "Công trình kiến trúc nổi bật với không gian mái vòm, các chi tiết cổ điển và vị trí cạnh Nhà thờ Đức Bà." },
            new Place { Name = "Nhà thờ Đức Bà", City = "Ho Chi Minh", Category = "Công trình văn hóa", Address = "Công trường Công xã Paris, Quận 1, TP. Hồ Chí Minh", Latitude = 10.7797m, Longitude = 106.6990m, Description = "Một công trình kiến trúc nổi bật tại khu trung tâm, thường được ghé thăm trong hành trình khám phá thành phố." },
            new Place { Name = "Bảo tàng Chứng tích Chiến tranh", City = "Ho Chi Minh", Category = "Bảo tàng", Address = "28 Võ Văn Tần, Quận 3, TP. Hồ Chí Minh", Latitude = 10.7794m, Longitude = 106.6921m, Description = "Không gian trưng bày tư liệu và hiện vật giúp khách tham quan tìm hiểu về lịch sử và hậu quả của chiến tranh." },
            new Place { Name = "Phố đi bộ Nguyễn Huệ", City = "Ho Chi Minh", Category = "Không gian đô thị", Address = "Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh", Latitude = 10.7741m, Longitude = 106.7030m, Description = "Quảng trường đi bộ trung tâm với các hoạt động cộng đồng, sự kiện văn hóa và tầm nhìn về sông Sài Gòn." }
        };

        foreach (var candidate in places)
        {
            if (!await db.Places.AnyAsync(x => x.Name == candidate.Name && x.City == candidate.City, ct))
                db.Places.Add(candidate);
        }
        await db.SaveChangesAsync(ct);

        var allPlaces = await db.Places.AsNoTracking().Where(x => x.IsActive).ToListAsync(ct);
        foreach (var place in allPlaces)
        {
            if (await db.NarrationContents.AnyAsync(x => x.PlaceId == place.Id, ct)) continue;
            db.NarrationContents.AddRange(
                new NarrationContent { PlaceId = place.Id, Title = "Giới thiệu và điểm nổi bật", Topic = "Khám phá", SourceText = place.Description },
                new NarrationContent { PlaceId = place.Id, Title = "Trải nghiệm tham quan", Topic = "Trải nghiệm", SourceText = $"Khi ghé thăm {place.Name}, du khách nên dành thời gian quan sát không gian, tìm hiểu câu chuyện địa phương và tôn trọng các quy định tại điểm tham quan." });
        }
        await db.SaveChangesAsync(ct);
    }
}
