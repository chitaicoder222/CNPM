// Purpose: Defines the reviewed VIVOICE baseline POIs and draft translations without putting visitor content in browser code.
// Responsibility: Supplies deterministic, add-only seed definitions for five supported languages; editorial review remains required before production use.
namespace Vivoice.Web.Data;

public sealed record BaselinePoiTranslation(string Name, string Summary);
public sealed record BaselineTopicTranslation(string Title, string Content);
public sealed record BaselineTopicDefinition(string Category, int DisplayOrder, IReadOnlyDictionary<string, BaselineTopicTranslation> Translations);
public sealed record BaselinePoiDefinition(string Slug, decimal Latitude, decimal Longitude, int DisplayOrder, IReadOnlyDictionary<string, BaselinePoiTranslation> Translations, IReadOnlyList<BaselineTopicDefinition> Topics);

public static class VivoiceBaselineContent
{
    public static IReadOnlyList<string> SupportedLanguages { get; } = ["vi", "ko", "ja", "zh", "th"];

    public static IReadOnlyList<BaselinePoiDefinition> Pois { get; } =
    [
        CreatePoi("ha-noi", 21.028511m, 105.804817m, 1,
            new("Hà Nội", "Thủ đô nghìn năm văn hiến, nơi giao thoa lịch sử và đời sống đương đại."),
            new("하노이", "천년의 문화유산을 간직한 수도로, 역사와 현대 생활이 만나는 곳입니다."),
            new("ハノイ", "千年の文化を受け継ぐ首都。歴史と現代の暮らしが交差します。"),
            new("河内", "拥有千年文化底蕴的首都，历史与现代生活在此交汇。"),
            new("ฮานอย", "เมืองหลวงที่มีมรดกวัฒนธรรมยาวนานนับพันปี ซึ่งประวัติศาสตร์และวิถีชีวิตสมัยใหม่มาบรรจบกัน")),
        CreatePoi("ho-chi-minh-city", 10.823099m, 106.629664m, 2,
            new("Thành phố Hồ Chí Minh", "Nhịp sống năng động với những lớp ký ức đô thị Nam Bộ."),
            new("호찌민시", "남부 도시의 기억과 활기찬 리듬이 어우러진 곳입니다."),
            new("ホーチミン市", "南部の都市の記憶と活気あるリズムが重なる街です。"),
            new("胡志明市", "南方都市记忆与充满活力的生活节奏交织于此。"),
            new("นครโฮจิมินห์", "เมืองที่ผสานความทรงจำของเมืองทางใต้เข้ากับจังหวะชีวิตที่มีชีวิตชีวา")),
        CreatePoi("da-nang", 16.054407m, 108.202167m, 3,
            new("Đà Nẵng", "Thành phố biển nằm giữa núi, sông và những cây cầu hiện đại."),
            new("다낭", "산과 강, 현대적인 다리 사이에 자리한 해변 도시입니다."),
            new("ダナン", "山、川、そして近代的な橋に囲まれた海辺の都市です。"),
            new("岘港", "一座坐落于山脉、河流与现代桥梁之间的海滨城市。"),
            new("ดานัง", "เมืองชายทะเลที่ตั้งอยู่ท่ามกลางภูเขา แม่น้ำ และสะพานสมัยใหม่")),
        CreatePoi("hue", 16.463713m, 107.590866m, 4,
            new("Huế", "Không gian di sản cố đô với âm nhạc, kiến trúc và ẩm thực cung đình."),
            new("후에", "음악, 건축, 궁중 음식이 어우러진 옛 수도의 유산 공간입니다."),
            new("フエ", "音楽、建築、宮廷料理が息づく古都の文化遺産です。"),
            new("顺化", "古都遗产空间中汇聚着音乐、建筑与宫廷美食。"),
            new("เว้", "พื้นที่มรดกของราชธานีเก่าที่มีดนตรี สถาปัตยกรรม และอาหารราชสำนัก")),
        CreatePoi("hoi-an", 15.880058m, 108.338047m, 5,
            new("Hội An", "Phố cổ ven sông với nếp sống thương cảng và những chiếc đèn lồng."),
            new("호이안", "강변의 옛 항구 도시로, 상업의 생활상과 등불이 어우러집니다."),
            new("ホイアン", "川沿いの古い港町。商港の暮らしとランタンが残ります。"),
            new("会安", "河畔古城保留着商港生活的气息与一盏盏灯笼。"),
            new("ฮอยอัน", "เมืองเก่าริมแม่น้ำที่ยังคงวิถีชีวิตของเมืองท่าและโคมไฟหลากสี")),
        CreatePoi("nha-trang", 12.238791m, 109.196749m, 6,
            new("Nha Trang", "Thành phố biển với vịnh xanh, nhịp sống ven bờ và văn hóa Chăm."),
            new("냐짱", "푸른 만과 해안의 생활, 참 문화가 어우러진 해변 도시입니다."),
            new("ニャチャン", "青い湾、海辺の暮らし、そしてチャム文化が調和する都市です。"),
            new("芽庄", "一座拥有碧蓝海湾、滨海生活与占族文化的海滨城市。"),
            new("ญาจาง", "เมืองชายทะเลที่มีอ่าวสีฟ้า วิถีชีวิตริมฝั่ง และวัฒนธรรมจาม"))
    ];

    private static BaselinePoiDefinition CreatePoi(
        string slug,
        decimal latitude,
        decimal longitude,
        int displayOrder,
        BaselinePoiTranslation vi,
        BaselinePoiTranslation ko,
        BaselinePoiTranslation ja,
        BaselinePoiTranslation zh,
        BaselinePoiTranslation th)
    {
        var translations = new Dictionary<string, BaselinePoiTranslation>(StringComparer.Ordinal)
        {
            ["vi"] = vi,
            ["ko"] = ko,
            ["ja"] = ja,
            ["zh"] = zh,
            ["th"] = th
        };
        return new BaselinePoiDefinition(slug, latitude, longitude, displayOrder, translations, CreateTopics(translations));
    }

    private static IReadOnlyList<BaselineTopicDefinition> CreateTopics(IReadOnlyDictionary<string, BaselinePoiTranslation> poiTranslations) =>
    [
        CreateTopic("culture", 1, poiTranslations),
        CreateTopic("food", 2, poiTranslations),
        CreateTopic("tourism", 3, poiTranslations)
    ];

    private static BaselineTopicDefinition CreateTopic(string category, int displayOrder, IReadOnlyDictionary<string, BaselinePoiTranslation> poiTranslations)
    {
        var translations = poiTranslations.ToDictionary(
            pair => pair.Key,
            pair => new BaselineTopicTranslation(TopicTitle(pair.Key, category), TopicContent(pair.Key, category, pair.Value.Name)),
            StringComparer.Ordinal);
        return new BaselineTopicDefinition(category, displayOrder, translations);
    }

    private static string TopicTitle(string language, string category) => (language, category) switch
    {
        ("vi", "culture") => "Văn hóa đặc sắc",
        ("vi", "food") => "Ẩm thực nổi tiếng",
        ("vi", "tourism") => "Điểm du lịch nổi bật",
        ("ko", "culture") => "특색 있는 문화",
        ("ko", "food") => "유명한 음식",
        ("ko", "tourism") => "주요 관광지",
        ("ja", "culture") => "特色ある文化",
        ("ja", "food") => "有名な料理",
        ("ja", "tourism") => "主な観光地",
        ("zh", "culture") => "特色文化",
        ("zh", "food") => "著名美食",
        ("zh", "tourism") => "热门旅游景点",
        ("th", "culture") => "วัฒนธรรมเด่น",
        ("th", "food") => "อาหารขึ้นชื่อ",
        ("th", "tourism") => "สถานที่ท่องเที่ยวเด่น",
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    private static string TopicContent(string language, string category, string name) => (language, category) switch
    {
        ("vi", "culture") => $"{name} lưu giữ những câu chuyện văn hóa gắn với cộng đồng địa phương. Khi tham quan, hãy quan sát kiến trúc, nếp sống và ứng xử để hiểu thêm bản sắc của nơi đây.",
        ("vi", "food") => $"Ẩm thực tại {name} phản ánh nguyên liệu, khí hậu và nhịp sống của vùng đất. Hãy ưu tiên cơ sở rõ nguồn gốc và tôn trọng không gian chung khi thưởng thức.",
        ("vi", "tourism") => $"Từ {name}, du khách có thể kết hợp những điểm tham quan phù hợp theo thời gian, thời tiết và quy định địa phương. Nội dung cần được admin cập nhật khi có thông tin chính thức mới.",
        ("ko", "culture") => $"{name}에는 지역 공동체와 이어진 문화 이야기가 남아 있습니다. 여행할 때 건축, 생활 방식, 사람들의 예절을 살펴보며 이곳의 정체성을 더 깊이 느껴 보세요.",
        ("ko", "food") => $"{name}의 음식은 재료, 기후, 지역의 생활 리듬을 담고 있습니다. 출처가 분명한 식당을 선택하고 공용 공간을 배려하며 즐겨 보세요.",
        ("ko", "tourism") => $"{name}에서는 시간, 날씨, 현지 규정에 맞춰 여러 관광지를 함께 둘러볼 수 있습니다. 새로운 공식 정보가 있으면 관리자가 내용을 업데이트합니다.",
        ("ja", "culture") => $"{name}には、地域の人々と結び付いた文化の物語が残っています。建築や暮らし、ふるまいに目を向け、この土地らしさを感じてみてください。",
        ("ja", "food") => $"{name}の料理には、食材、気候、地域の暮らしのリズムが映し出されています。由来が分かる店を選び、共有空間に配慮して楽しみましょう。",
        ("ja", "tourism") => $"{name}では、時間、天候、現地の規則に合わせて周辺の見どころを組み合わせられます。新しい公式情報があれば、管理者が内容を更新します。",
        ("zh", "culture") => $"{name}保留着与当地社区相连的文化故事。游览时可留意建筑、生活方式和待人礼仪，感受这片土地的特色。",
        ("zh", "food") => $"{name}的美食反映了食材、气候和当地生活节奏。请选择来源清晰的餐饮场所，并在用餐时尊重公共空间。",
        ("zh", "tourism") => $"从{name}出发，游客可根据时间、天气和当地规定组合合适的景点。出现新的官方信息时，管理员应更新内容。",
        ("th", "culture") => $"{name}เก็บรักษาเรื่องราวทางวัฒนธรรมที่เชื่อมโยงกับชุมชนท้องถิ่นไว้ ระหว่างการเยี่ยมชม ลองสังเกตสถาปัตยกรรม วิถีชีวิต และมารยาท เพื่อเข้าใจอัตลักษณ์ของที่นี่ให้ลึกซึ้งขึ้น",
        ("th", "food") => $"อาหารของ{name}สะท้อนวัตถุดิบ ภูมิอากาศ และจังหวะชีวิตของพื้นที่ เลือกร้านที่มีแหล่งที่มาชัดเจนและคำนึงถึงพื้นที่ส่วนรวมขณะรับประทานอาหาร",
        ("th", "tourism") => $"จาก{name} นักท่องเที่ยวสามารถวางแผนเที่ยวสถานที่ต่าง ๆ ให้เหมาะกับเวลา สภาพอากาศ และข้อกำหนดท้องถิ่นได้ เมื่อมีข้อมูลทางการใหม่ ผู้ดูแลควรอัปเดตเนื้อหา",
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };
}
