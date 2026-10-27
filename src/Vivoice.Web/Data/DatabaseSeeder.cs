// Purpose: Adds non-sensitive initial VIVOICE reference data after the database migration completes.
// Responsibility: Seeds six Vietnamese POIs, Vietnamese narration and the configured initial visit rate idempotently.
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data.Entities;

namespace Vivoice.Web.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(VivoiceDbContext db, CancellationToken cancellationToken = default)
    {
        var plan = ContentSeedPlan.Create(
            hasRevenueRate: await db.VisitRevenueRates.AnyAsync(cancellationToken),
            hasPoi: await db.Pois.AnyAsync(cancellationToken));
        if (!plan.HasWork)
        {
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (plan.CreateRevenueRate)
            {
                db.VisitRevenueRates.Add(new VisitRevenueRate
                {
                    Amount = 0.10m,
                    Currency = "USD",
                    EffectiveFromUtc = DateTime.UtcNow.Date
                });
            }

            if (plan.CreatePois)
            {
                AddInitialPois(db);
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static void AddInitialPois(VivoiceDbContext db)
    {
        var seeds = new[]
        {
            ("ha-noi", "Hà Nội", "Thủ đô nghìn năm văn hiến, nơi giao thoa lịch sử và đời sống đương đại.", 21.028511m, 105.804817m),
            ("ho-chi-minh-city", "Thành phố Hồ Chí Minh", "Nhịp sống năng động với những lớp ký ức đô thị Nam Bộ.", 10.823099m, 106.629664m),
            ("da-nang", "Đà Nẵng", "Thành phố biển nằm giữa núi, sông và những cây cầu hiện đại.", 16.054407m, 108.202167m),
            ("hue", "Huế", "Không gian di sản cố đô với âm nhạc, kiến trúc và ẩm thực cung đình.", 16.463713m, 107.590866m),
            ("hoi-an", "Hội An", "Phố cổ ven sông với nếp sống thương cảng và những chiếc đèn lồng.", 15.880058m, 108.338047m),
            ("nha-trang", "Nha Trang", "Thành phố biển với vịnh xanh, nhịp sống ven bờ và văn hóa Chăm.", 12.238791m, 109.196749m)
        };

        for (var index = 0; index < seeds.Length; index++)
        {
            var seed = seeds[index];
            var poi = new Poi
            {
                Slug = seed.Item1,
                Latitude = seed.Item4,
                Longitude = seed.Item5,
                DisplayOrder = index + 1,
                Translations = [new PoiTranslation { LanguageCode = "vi", Name = seed.Item2, Summary = seed.Item3 }],
                Topics =
                [
                    CreateTopic("culture", 1, "Văn hóa đặc sắc", $"{seed.Item2} lưu giữ những câu chuyện văn hóa gắn với cộng đồng địa phương. Khi tham quan, hãy quan sát kiến trúc, nếp sống và ứng xử để hiểu thêm bản sắc của nơi đây."),
                    CreateTopic("food", 2, "Ẩm thực nổi tiếng", $"Ẩm thực tại {seed.Item2} phản ánh nguyên liệu, khí hậu và nhịp sống của vùng đất. Hãy ưu tiên cơ sở rõ nguồn gốc và tôn trọng không gian chung khi thưởng thức."),
                    CreateTopic("tourism", 3, "Điểm du lịch nổi bật", $"Từ {seed.Item2}, du khách có thể kết hợp những điểm tham quan phù hợp theo thời gian, thời tiết và quy định địa phương. Nội dung cần được admin cập nhật khi có thông tin chính thức mới.")
                ]
            };
            db.Pois.Add(poi);
        }

    }

    private static PoiTopic CreateTopic(string category, int order, string title, string content) => new()
    {
        Category = category,
        DisplayOrder = order,
        Translations = [new TopicTranslation { LanguageCode = "vi", Title = title, Content = content }]
    };
}

public readonly record struct ContentSeedPlan(bool CreateRevenueRate, bool CreatePois)
{
    public bool HasWork => CreateRevenueRate || CreatePois;

    public static ContentSeedPlan Create(bool hasRevenueRate, bool hasPoi) => new(!hasRevenueRate, !hasPoi);
}
