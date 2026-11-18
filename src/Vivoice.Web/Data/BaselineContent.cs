// Purpose: Defines the curated Ho Chi Minh City POI baseline and its Vietnamese and English narration.
// Responsibility: Keeps the public catalog deterministic after the catalog-reset migration.
namespace Vivoice.Web.Data;

public sealed record BaselinePoiTranslation(string Name, string Summary);
public sealed record BaselineTopicTranslation(string Title, string Content);
public sealed record BaselineTopicDefinition(string Category, int DisplayOrder, IReadOnlyDictionary<string, BaselineTopicTranslation> Translations);
public sealed record BaselinePoiDefinition(string Slug, decimal Latitude, decimal Longitude, int TriggerRadiusMeters, int DisplayOrder, IReadOnlyDictionary<string, BaselinePoiTranslation> Translations, IReadOnlyList<BaselineTopicDefinition> Topics);

public static class VivoiceBaselineContent
{
    public static IReadOnlyList<string> SupportedLanguages { get; } = ["vi", "en", "ko", "ja", "zh"];

    public static IReadOnlyList<BaselinePoiDefinition> Pois { get; } =
    [
        CreatePoi("lotte-mart-district-7", 10.729690m, 106.721480m, 700, 1, "Lotte Mart Quận 7", "Điểm mua sắm tại khu đô thị Nam Sài Gòn; bán kính rộng để khách đang ở đây có thể nghe ngay.", "Lotte Mart District 7", "A South Saigon shopping destination with a practical immediate-listening radius."),
        CreatePoi("independence-palace", 10.777041m, 106.695447m, 180, 2, "Dinh Độc Lập", "Công trình ghi dấu nhiều thời khắc quan trọng của lịch sử Việt Nam hiện đại.", "Independence Palace", "A landmark associated with pivotal moments in modern Vietnamese history."),
        CreatePoi("ben-thanh-market", 10.772079m, 106.698271m, 150, 3, "Chợ Bến Thành", "Khu chợ biểu tượng ở trung tâm thành phố, giàu nhịp sống và hương vị địa phương.", "Ben Thanh Market", "An iconic central market known for its lively local atmosphere and flavours."),
        CreatePoi("saigon-notre-dame", 10.779783m, 106.699018m, 120, 4, "Nhà thờ Đức Bà Sài Gòn", "Kiến trúc gạch đỏ tiêu biểu trong không gian lịch sử trung tâm thành phố.", "Saigon Notre Dame Cathedral", "A red-brick architectural landmark in the historic city centre."),
        CreatePoi("central-post-office", 10.779731m, 106.699031m, 100, 5, "Bưu điện Trung tâm Sài Gòn", "Không gian bưu chính cổ điển vẫn hoạt động giữa khu trung tâm.", "Saigon Central Post Office", "A working historic post office in the heart of the city."),
        CreatePoi("war-remnants-museum", 10.779327m, 106.692704m, 150, 6, "Bảo tàng Chứng tích Chiến tranh", "Không gian tư liệu giúp nhìn lại hậu quả chiến tranh và giá trị của hòa bình.", "War Remnants Museum", "A documentary museum reflecting on the consequences of war and the value of peace."),
        CreatePoi("nguyen-hue-walking-street", 10.773375m, 106.703853m, 180, 7, "Phố đi bộ Nguyễn Huệ", "Trục đi bộ kết nối kiến trúc trung tâm, sông Sài Gòn và đời sống đô thị.", "Nguyen Hue Walking Street", "A pedestrian avenue connecting central architecture, the Saigon River and city life."),
        CreatePoi("opera-house", 10.776579m, 106.703950m, 100, 8, "Nhà hát Thành phố", "Nhà hát lịch sử với kiến trúc Pháp, nằm ở khu vực sôi động của Quận 1.", "Municipal Theatre", "A historic French-style theatre in lively District 1."),
        CreatePoi("bui-vien-walking-street", 10.767790m, 106.693785m, 120, 9, "Phố đi bộ Bùi Viện", "Khu phố đêm sôi động với ẩm thực, âm nhạc và trải nghiệm giao lưu quốc tế.", "Bui Vien Walking Street", "A lively night street with food, music and an international social scene."),
        CreatePoi("landmark-81", 10.794989m, 106.721826m, 200, 10, "Landmark 81", "Tòa tháp cao nổi bật bên sông Sài Gòn, đại diện cho diện mạo đô thị hiện đại.", "Landmark 81", "A prominent riverside tower representing modern Ho Chi Minh City.")
    ];

    private static BaselinePoiDefinition CreatePoi(string slug, decimal latitude, decimal longitude, int radius, int order, string viName, string viSummary, string enName, string enSummary)
    {
        var translations = new Dictionary<string, BaselinePoiTranslation>(StringComparer.Ordinal)
        {
            ["vi"] = new(viName, viSummary), ["en"] = new(enName, enSummary),
            ["ko"] = new(enName, enSummary), ["ja"] = new(enName, enSummary), ["zh"] = new(enName, enSummary)
        };
        var topics = new List<BaselineTopicDefinition>
        {
            new("culture", 1, new Dictionary<string, BaselineTopicTranslation>(StringComparer.Ordinal)
            {
                ["vi"] = new("Thuyết minh địa điểm", $"Bạn đang ở gần {viName}. {viSummary} Hãy quan sát không gian xung quanh và di chuyển cẩn thận, tôn trọng quy định tại điểm đến."),
                ["en"] = new("Location narration", $"You are near {enName}. {enSummary} Please observe the surroundings, move safely, and respect the rules at this destination."),
                ["ko"] = new("Location narration", $"You are near {enName}. {enSummary}"),
                ["ja"] = new("Location narration", $"You are near {enName}. {enSummary}"),
                ["zh"] = new("Location narration", $"You are near {enName}. {enSummary}")
            })
        };
        return new BaselinePoiDefinition(slug, latitude, longitude, radius, order, translations, topics);
    }
}
