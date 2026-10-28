// Purpose: Defines source-informed VIVOICE baseline POIs and narration reference translations without putting visitor content in browser code.
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
        return new BaselinePoiDefinition(slug, latitude, longitude, displayOrder, translations, CreateTopics(slug, translations));
    }

    private static IReadOnlyList<BaselineTopicDefinition> CreateTopics(string slug, IReadOnlyDictionary<string, BaselinePoiTranslation> poiTranslations) =>
    [
        CreateTopic(slug, "culture", 1, poiTranslations),
        CreateTopic(slug, "food", 2, poiTranslations),
        CreateTopic(slug, "tourism", 3, poiTranslations)
    ];

    private static BaselineTopicDefinition CreateTopic(string slug, string category, int displayOrder, IReadOnlyDictionary<string, BaselinePoiTranslation> poiTranslations)
    {
        var translations = poiTranslations.ToDictionary(
            pair => pair.Key,
            pair => new BaselineTopicTranslation(TopicTitle(pair.Key, category), TopicContent(slug, pair.Key, category)),
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

    private static string TopicContent(string slug, string language, string category) => language switch
    {
        "vi" => VietnameseNarration(slug, category),
        "ko" or "ja" or "zh" or "th" => LocalizedNarration(slug, language, category),
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Unsupported baseline narration language.")
    };

    // The Vietnamese source copy is deliberately city- and topic-specific.  Translation prose below
    // is source-informed reference copy and remains marked for qualified editorial review in the register.
    private static string VietnameseNarration(string slug, string category) => ((slug, category) switch
    {
        ("ha-noi", "culture") => """
            Hà Nội thường được cảm nhận bằng nhịp đi chậm: một mái ngói cũ, tiếng rao buổi sớm, hồ nước giữa khu phố và những con đường đã đổi tên qua nhiều thời kỳ. Thành phố là thủ đô của Việt Nam hiện đại, đồng thời gìn giữ nhiều lớp ký ức của vùng đất Thăng Long. Khu trung tâm Hoàng thành Thăng Long được UNESCO ghi nhận là một không gian khảo cổ và lịch sử đặc biệt, nơi dấu vết của một trung tâm quyền lực kéo dài từ thời nhà Lý vẫn giúp người xem hình dung sự liên tục của đời sống đô thị.

            Văn hóa Hà Nội không chỉ nằm trong một di tích. Ở khu phố cổ, tên phố gợi lại những nghề buôn bán xưa; trong các ngõ nhỏ, nhịp sinh hoạt gia đình và hàng quán vẫn tạo nên một không gian gần gũi. Khi ghé thăm, hãy để ý cách người dân dùng vỉa hè, chào hỏi, xếp hàng và cùng chia sẻ những không gian hẹp. Những chi tiết rất nhỏ ấy nói nhiều về một thành phố đông đúc nhưng giàu tính cộng đồng.

            Một hành trình có trách nhiệm nên dành thời gian cho bảo tàng, di sản, nghệ thuật biểu diễn hoặc một cuộc trò chuyện với hướng dẫn viên địa phương, thay vì chỉ chụp ảnh thật nhanh. Hãy ăn mặc phù hợp khi vào nơi thờ tự, giữ âm lượng vừa phải và tôn trọng biển chỉ dẫn. Từ cách quan sát đó, Hà Nội hiện ra không phải như một phông nền cổ, mà như một thành phố đang tiếp tục sống cùng lịch sử của mình.
            """,
        ("ha-noi", "food") => """
            Ẩm thực Hà Nội thường bắt đầu từ những món quen mà cách ăn lại rất có chủ ý. Phở là một ví dụ dễ nhận ra: bát nước dùng nóng, bánh phở, rau thơm và các gia vị được cân bằng để người ăn tự điều chỉnh theo khẩu vị. Bún chả cũng gợi nhịp ăn trưa của thành phố, với bún, thịt nướng, nước chấm và rau ăn kèm. Những món ấy không đại diện cho mọi bữa ăn ở Hà Nội, nhưng cho thấy sự coi trọng độ trong của nước dùng, mùi thơm của rau và sự vừa phải trong gia vị.

            Văn hóa cà phê là một lớp trải nghiệm khác. Từ quán nhỏ trong ngõ đến không gian nhìn ra phố, việc ngồi lại trò chuyện hoặc quan sát dòng người là một phần của chuyến đi. Thay vì tìm một danh sách quán cố định, du khách có thể hỏi người địa phương về lựa chọn phù hợp với chế độ ăn, dị ứng và mức chi tiêu của mình. Đó cũng là cách tránh biến ẩm thực thành một cuộc săn ảnh vội vàng.

            Khi thưởng thức, hãy chú ý vệ sinh thực phẩm, dùng dụng cụ chung đúng cách và tôn trọng không gian của người bán lẫn khách khác. Nếu chưa quen vị đậm hoặc nguyên liệu mới, hãy hỏi trước bằng câu ngắn, rõ ràng. Một bữa ăn ở Hà Nội đáng nhớ không chỉ vì món ăn, mà vì khoảnh khắc ngồi gần nhau, lắng nghe tiếng phố và hiểu thêm nhịp sống của thành phố.
            """,
        ("ha-noi", "tourism") => """
            Hà Nội phù hợp với kiểu tham quan theo từng lớp, không cần cố gắng đi hết mọi nơi trong một ngày. Bạn có thể mở đầu bằng khu vực hồ Hoàn Kiếm và phố cổ để cảm nhận cấu trúc phố đi bộ, sau đó dành một buổi khác cho Hoàng thành Thăng Long, bảo tàng hoặc những không gian nghệ thuật. Mỗi điểm đều có nhịp riêng: nơi cần thời gian đọc bảng giới thiệu, nơi chỉ cần đi chậm để quan sát kiến trúc và đời sống đường phố.

            Khi lập lịch, nên kiểm tra thông tin chính thức của địa điểm vào ngày đi vì giờ mở cửa, lối vào và quy định chụp ảnh có thể thay đổi. Di chuyển bằng đi bộ hoặc phương tiện công cộng phù hợp thường giúp bạn nhận ra khoảng cách thật giữa các khu phố. Vào giờ cao điểm, hãy chừa thêm thời gian và băng qua đường cẩn thận; giao thông là một phần rất thực của trải nghiệm đô thị này.

            Đừng bỏ qua những khoảng nghỉ. Một chỗ ngồi bên hồ, một cuốn sách ở quán yên tĩnh hay một đoạn phố ít đông có thể làm hành trình cân bằng hơn. Nếu vào không gian tín ngưỡng hoặc di tích, hãy làm theo hướng dẫn tại chỗ và tránh chạm vào hiện vật. Khép lại bằng việc nhìn lại những gì đã nghe và thấy, du khách sẽ nhận ra Hà Nội hấp dẫn nhất khi được khám phá với sự kiên nhẫn.
            """,
        ("ho-chi-minh-city", "culture") => """
            Thành phố Hồ Chí Minh là một đô thị chuyển động nhanh, nơi tên gọi Sài Gòn vẫn xuất hiện trong ký ức, trong ngôn ngữ đời thường và trong cách nhiều người kể về thành phố. Trung tâm đô thị cho thấy những lớp kiến trúc khác nhau đứng gần nhau: công trình công cộng từ các giai đoạn trước, đường phố thương mại đông đúc và những tòa nhà mới phản ánh vai trò kinh tế của miền Nam. Sự đối thoại giữa cũ và mới là một phần quan trọng của bản sắc nơi đây.

            Văn hóa thành phố được tạo nên bởi sự gặp gỡ. Người đến từ nhiều vùng mang theo giọng nói, món ăn, thói quen buôn bán và nhịp làm việc riêng. Vì vậy, một khu chợ, một quán cơm hay một con hẻm có thể kể nhiều câu chuyện hơn một bức ảnh toàn cảnh. Hãy quan sát cách cư dân sử dụng không gian chung, từ xe đẩy nhỏ trên vỉa hè đến những giờ tan tầm đầy năng lượng, để cảm nhận tính mở và thực dụng của đô thị này.

            Khi tham quan các công trình tôn giáo, bảo tàng hoặc không gian tưởng niệm, hãy giữ thái độ tôn trọng và dành thời gian đọc bối cảnh thay vì chỉ dừng lại ở mặt tiền. Một hướng dẫn viên có kiến thức địa phương có thể giúp kết nối các mảnh ghép lịch sử. Thành phố Hồ Chí Minh không đứng yên để được nhìn ngắm; thành phố được hiểu rõ hơn khi bạn lắng nghe nhịp sống hiện tại cùng những ký ức còn hiện diện trong phố.
            """,
        ("ho-chi-minh-city", "food") => """
            Ẩm thực Thành phố Hồ Chí Minh phản ánh nhịp sống linh hoạt của một đô thị lớn. Bánh mì, cơm tấm và bánh xèo thường được nhắc đến vì dễ gặp trong đời sống hằng ngày, nhưng điều thú vị hơn là cách thành phố tiếp nhận khẩu vị từ nhiều vùng. Một bữa sáng có thể rất nhanh bên vỉa hè; một bữa trưa có thể là cơm phần giản dị; buổi tối lại mở ra những khu ăn uống đông người. Sự đa dạng ấy khiến việc ăn uống trở thành một cách đọc thành phố.

            Khi thử món mới, đừng ngại hỏi về thành phần, mức cay, nước chấm hoặc lựa chọn phù hợp với chế độ ăn của bạn. Câu hỏi lịch sự giúp người bán tư vấn chính xác hơn, đồng thời giảm lãng phí. Thay vì đặt quá nhiều món để chụp ảnh, hãy gọi vừa đủ, chia sẻ nếu đi cùng nhóm và tôn trọng hàng người đang chờ. Những thói quen nhỏ ấy phù hợp với không gian đông đúc và nhịp phục vụ nhanh.

            Không có một địa chỉ cố định nào đại diện cho toàn bộ ẩm thực thành phố; chất lượng và tình trạng phục vụ có thể thay đổi theo thời gian. Hãy ưu tiên nơi có thông tin rõ ràng, quan sát điều kiện vệ sinh và sử dụng nước uống an toàn. Bằng cách đó, du khách không chỉ thưởng thức hương vị miền Nam mà còn tham gia vào đời sống đô thị một cách chủ động, tôn trọng và thoải mái.
            """,
        ("ho-chi-minh-city", "tourism") => """
            Một tuyến tham quan trung tâm Thành phố Hồ Chí Minh có thể kết nối những con đường quen thuộc như Đồng Khởi với các công trình công cộng, nhà hát, bưu điện trung tâm, chợ và bảo tàng. Điều quan trọng không phải là đánh dấu thật nhiều điểm trên bản đồ, mà là chọn một chủ đề cho mỗi buổi: kiến trúc, ký ức đô thị, nghệ thuật hoặc đời sống thương mại. Cách chia nhỏ đó giúp bạn có thời gian nhìn kỹ và không bị cuốn theo nhịp xe cộ.

            Thành phố có khí hậu nóng và các cơn mưa có thể đến nhanh, vì vậy nước uống, trang phục nhẹ và một phương án trú mưa là những chuẩn bị hữu ích. Hãy dùng lối sang đường phù hợp, giữ đồ cá nhân cẩn thận ở nơi đông người và kiểm tra thông tin chính thức trước khi vào bất kỳ công trình nào. Với các điểm gợi nhớ lịch sử, bối cảnh đi kèm quan trọng hơn một tấm ảnh chụp vội.

            Sau khu trung tâm, bạn có thể chọn một khu phố, một bảo tàng nhỏ hoặc một không gian sáng tạo để tiếp tục. Để hành trình không thành cuộc chạy đua, hãy dành thời gian nghỉ ở nơi an toàn và quan sát nhịp sống xung quanh. Thành phố Hồ Chí Minh luôn có nhiều lựa chọn; chọn ít hơn nhưng hiểu sâu hơn thường đem lại chuyến đi đáng nhớ hơn.
            """,
        ("da-nang", "culture") => """
            Đà Nẵng được định hình bởi vị trí giữa biển, sông và núi. Cảnh quan ấy không chỉ tạo nên bức ảnh đẹp mà còn giải thích vì sao thành phố có cảm giác mở, nhiều gió và luôn hướng ra mặt nước. Dọc sông Hàn, những cây cầu và khu dân cư hiện đại cho thấy một đô thị đang phát triển; ở phía khác, núi và bán đảo nhắc người xem rằng thiên nhiên vẫn là phần quan trọng của không gian sống.

            Để hiểu sâu hơn, du khách có thể tìm đến các câu chuyện di sản của miền Trung và dấu ấn văn hóa Chăm trong khu vực. Những không gian như núi Ngũ Hành Sơn kết hợp cảnh quan đá, hang động và nơi thờ tự, vì vậy cần được tiếp cận với thái độ tôn trọng chứ không chỉ như một phông nền phiêu lưu. Bảng hướng dẫn, quy định bảo tồn và sự yên tĩnh tại những điểm linh thiêng đều đáng được chú ý.

            Văn hóa Đà Nẵng còn nằm trong cách cư dân dùng bờ sông, bãi biển và các không gian công cộng. Đi bộ vào sáng sớm hoặc chiều muộn giúp bạn thấy thành phố ở nhịp ít vội hơn. Hãy giữ sạch bãi biển, không lấy đá hay hiện vật tự nhiên và hỏi trước khi chụp người khác. Khi làm vậy, chuyến tham quan sẽ góp phần giữ gìn chính những cảnh quan làm Đà Nẵng trở nên đặc biệt.
            """,
        ("da-nang", "food") => """
            Ẩm thực Đà Nẵng gắn với miền Trung nhưng có nhịp riêng của một thành phố ven biển. Mì Quảng thường được nhắc đến như một món tiêu biểu của vùng, với sợi mì, nước dùng dùng vừa phải, rau và các phần ăn kèm tạo nên nhiều lớp kết cấu. Bên cạnh đó, hải sản và những bữa ăn gia đình cho thấy mối liên hệ gần gũi giữa chợ, bếp và nguồn nguyên liệu từ biển.

            Cách tốt nhất để khám phá là bắt đầu từ một món bạn có thể nhận biết nguyên liệu, sau đó hỏi người bán về gia vị hoặc lựa chọn thay thế nếu cần. Món miền Trung có thể có vị đậm, cay hoặc sử dụng các loại mắm quen với người địa phương nhưng mới lạ với du khách. Hỏi trước giúp trải nghiệm dễ chịu hơn và thể hiện sự tôn trọng đối với người chuẩn bị món ăn.

            Không nên coi mọi quầy hàng ven đường là giống nhau. Hãy quan sát sự sạch sẽ, lượng khách, cách bảo quản thực phẩm và dùng nước uống an toàn. Nếu đi cùng nhóm, chia món nhỏ để vừa thử được nhiều hương vị vừa giảm lãng phí. Bữa ăn ở Đà Nẵng sẽ ý nghĩa hơn khi bạn nhận ra phía sau một tô mì hay một đĩa hải sản là khí hậu, nhịp chợ và sự khéo léo của người nấu.
            """,
        ("da-nang", "tourism") => """
            Đà Nẵng cho phép bạn kết hợp biển, sông và núi trong cùng một hành trình, nhưng mỗi không gian cần một nhịp tham quan khác nhau. Buổi sáng sớm phù hợp để đi dạo ven biển khi nắng chưa gắt; buổi chiều có thể dành cho sông Hàn và các cây cầu; một ngày riêng sẽ thoải mái hơn cho núi Ngũ Hành Sơn hoặc những điểm xa trung tâm. Lịch trình có khoảng trống sẽ an toàn và dễ chịu hơn việc cố ghép mọi điểm vào một ngày.

            Trước khi đi, hãy kiểm tra dự báo thời tiết, biển báo tại bãi biển và quy định ở điểm di sản. Ở khu vực đá, hang hoặc bậc thang, giày phù hợp và việc đi chậm quan trọng hơn một góc chụp mạo hiểm. Khi gặp nơi thờ tự, hãy ăn mặc lịch sự, giữ trật tự và làm theo hướng dẫn của ban quản lý.

            Nếu bạn muốn hiểu thành phố thay vì chỉ đi qua nó, hãy xen một hoạt động có bối cảnh: ghé bảo tàng, đi bộ cùng hướng dẫn viên hoặc dành thời gian quan sát cuộc sống ven sông. Đà Nẵng hấp dẫn vì sự cân bằng giữa đô thị mới và thiên nhiên gần gũi. Một chuyến đi có trách nhiệm sẽ giữ được sự cân bằng ấy cho người đến sau.
            """,
        ("hue", "culture") => """
            Huế gợi nhắc một nhịp điềm tĩnh, nhưng phía sau sự yên ả ấy là lịch sử của một kinh đô. UNESCO ghi nhận Quần thể di tích Cố đô Huế như dấu ấn nổi bật của triều Nguyễn, triều đại đã đặt kinh đô tại đây từ năm 1802 đến năm 1945. Thành quách, cung điện, lăng tẩm và cảnh quan sông Hương không phải những mảnh rời rạc; chúng cùng tạo nên một cách tổ chức không gian gắn với quyền lực, nghi lễ và đời sống văn hóa của một thời kỳ.

            Khi bước trong khu di tích, hãy để ý tỷ lệ cổng, sân, hồ nước và những khoảng trống. Kiến trúc Huế thường không tìm cách áp đảo người xem ngay lập tức mà dẫn dắt họ đi qua nhiều lớp không gian. Âm nhạc, thơ ca, nghề thủ công và nghi thức cũng là những phần quan trọng của di sản, vì vậy một lời giới thiệu có bối cảnh có thể làm chuyến tham quan sâu sắc hơn nhiều so với việc chỉ nhận diện tên công trình.

            Đây là nơi cần sự kiên nhẫn và tôn trọng. Không leo lên cấu kiện, không chạm vào bề mặt dễ hư hại, giữ âm lượng vừa phải và tuân theo tuyến tham quan. Khi nhìn Huế như một đô thị di sản còn tiếp tục được gìn giữ, du khách sẽ thấy vẻ đẹp không chỉ nằm ở quá khứ mà còn ở nỗ lực bảo tồn hôm nay.
            """,
        ("hue", "food") => """
            Ẩm thực Huế thường được nhớ đến bởi sự chỉn chu trong cách bày biện và sự cân bằng giữa vị mặn, chua, cay, ngọt. Truyền thống cung đình là một lớp tham chiếu quan trọng, nhưng văn hóa ăn uống của Huế không chỉ thuộc về cung điện. Trong đời sống thường ngày, nhiều món nhỏ, bánh, bún và món ăn đường phố cho thấy sự khéo léo trong việc sử dụng nguyên liệu địa phương và tạo nên khẩu phần vừa phải.

            Khi thưởng thức, đừng vội xem một món là “chuẩn” duy nhất. Công thức, khẩu vị và cách phục vụ có thể khác nhau giữa gia đình, khu chợ và quán ăn. Hãy hỏi về thành phần nếu bạn dị ứng, chưa quen vị cay hoặc cần lựa chọn chay. Việc giao tiếp đơn giản, lịch sự thường giúp bữa ăn trở thành một cuộc trao đổi thân thiện thay vì chỉ là một đơn hàng.

            Tôn trọng ẩm thực địa phương cũng là tôn trọng nhịp của nơi mình đến: dùng vừa đủ, giữ bàn sạch và chú ý quy tắc tại không gian chung. Tránh tin vào những lời quảng cáo phóng đại về “món độc nhất”; thông tin có thể thay đổi và trải nghiệm tốt nhất thường đến từ việc ăn chậm, quan sát cách món được phục vụ và lắng nghe câu chuyện của người nấu. Huế mời bạn thưởng thức bằng cả vị giác lẫn sự chú ý.
            """,
        ("hue", "tourism") => """
            Huế phù hợp với hành trình có chủ đề rõ ràng. Một ngày có thể dành cho Đại Nội và các không gian trung tâm; ngày khác cho những khu lăng tẩm, cảnh quan sông Hương hoặc một bảo tàng. Chọn ít điểm hơn giúp bạn có thời gian đọc sơ đồ, xem chi tiết trang trí và hiểu vì sao các công trình được đặt trong mối quan hệ với nước, cây xanh và địa hình.

            Trước khi vào khu di tích, hãy xem thông báo chính thức về lối đi, dịch vụ hướng dẫn và những khu vực đang bảo tồn. Khí hậu miền Trung có thể nắng gắt hoặc mưa bất chợt, vì vậy nước uống, áo mưa nhẹ và giày đi bộ là những chuẩn bị thiết thực. Ở nơi có bậc thang, nền đá hoặc khu vực yên tĩnh, hãy đi chậm và tôn trọng các biển hạn chế.

            Một chuyến đi tốt không chỉ ghi lại hình ảnh mái ngói và cổng thành. Hãy dành thời gian ngồi bên sông, nghe một câu chuyện về âm nhạc truyền thống hoặc xem một nghề thủ công trong bối cảnh đúng của nó. Những khoảng dừng ấy giúp Huế không bị thu gọn thành một danh sách di tích. Chúng biến hành trình thành cơ hội hiểu cách một thành phố gìn giữ ký ức của mình.
            """,
        ("hoi-an", "culture") => """
            Hội An là một ví dụ hiếm về một thương cảng Đông Nam Á được bảo tồn rõ nét. UNESCO mô tả phố cổ như nơi phản ánh sự giao lưu trong hoạt động thương mại từ thế kỷ XV đến XIX, khi ảnh hưởng bản địa, Trung Hoa, Nhật Bản và sau đó là châu Âu cùng để lại dấu vết trong đường phố, nhà gỗ và không gian ven sông. Giá trị của Hội An không nằm ở một tòa nhà đơn lẻ mà ở mối liên kết giữa cả khu phố.

            Khi đi bộ, hãy nhìn kỹ những chi tiết nhỏ: mặt tiền hẹp, sân trong, mái ngói, cửa gỗ và khoảng chuyển tiếp từ nhà ra phố. Những yếu tố ấy kể về khí hậu, buôn bán và đời sống gia đình qua nhiều thế hệ. Đèn lồng tạo nên không khí đặc trưng vào buổi tối, nhưng chúng chỉ là một phần của câu chuyện; điều quan trọng hơn là tôn trọng cư dân đang sống và làm việc trong không gian di sản.

            Hãy giữ lối đi thông thoáng, không tựa vào cấu kiện cũ, không dùng đèn flash nơi bị hạn chế và tuân theo quy định về giờ giấc, vé hoặc tuyến phố. Nếu có thể, hãy chọn một trải nghiệm giải thích lịch sử thay vì chỉ ghé qua thật nhanh. Một Hội An được khám phá bằng sự chú ý sẽ cho thấy vì sao việc bảo tồn cả một đô thị sống lại khó và đáng quý đến vậy.
            """,
        ("hoi-an", "food") => """
            Ẩm thực Hội An gắn với lịch sử thương cảng và với đời sống của một thành phố ven sông. Cao Lầu và các món bánh như hoành thánh hoặc “hoa hồng trắng” thường được nhắc đến khi nói về nơi đây. Điều đáng chú ý không chỉ là tên món ăn, mà là cách từng phần nhỏ được chuẩn bị, chia sẻ và đặt trong bữa ăn có nhịp độ chậm hơn so với nhiều đô thị lớn.

            Mỗi cơ sở có thể có công thức, kích thước phần ăn và gia vị riêng. Vì vậy, hãy hỏi rõ thành phần khi bạn có dị ứng hoặc chế độ ăn đặc biệt, thay vì dựa vào mô tả chung trên mạng. Chọn phần ăn vừa phải cũng giúp du khách thử được nhiều hương vị mà không lãng phí. Khi ngồi trong khu phố cổ, hãy giữ lối đi, không chặn cửa nhà dân và tôn trọng giờ nghỉ của không gian xung quanh.

            Những câu chuyện về nguồn gốc món ăn đôi khi được kể rất hấp dẫn nhưng không phải lúc nào cũng có tài liệu thống nhất. Hãy xem chúng như lời mời tìm hiểu thêm, không phải bằng chứng lịch sử tuyệt đối. Điều có thể cảm nhận trực tiếp là mối liên hệ giữa chợ, sông, gian bếp và khách lữ hành. Ăn ở Hội An vì thế là cách tham gia vào một nếp sống, chứ không chỉ là hoàn thành danh sách món phải thử.
            """,
        ("hoi-an", "tourism") => """
            Hội An thuận lợi nhất khi được khám phá bằng đi bộ và với một lịch trình nhẹ. Hãy chọn một buổi để tìm hiểu phố cổ, một khoảng khác để đi dọc sông hoặc ghé không gian thủ công, thay vì chỉ tập trung vào thời điểm lên đèn. Ban ngày cho phép bạn quan sát cấu trúc nhà cửa; buổi tối cho cảm nhận khác về ánh sáng và dòng người. Cả hai đều đáng giá nếu bạn giữ đủ thời gian nghỉ.

            Do đây là khu di sản có cư dân sinh sống, phép lịch sự rất quan trọng. Không đứng lâu trước cửa nhà, không dùng flycam hay thiết bị ghi hình nơi bị cấm, không xả rác xuống sông và làm theo hướng dẫn của người quản lý. Trước khi mua vé hoặc tham gia dịch vụ, hãy kiểm tra thông tin chính thức tại thời điểm đi thay vì dựa vào bài đăng cũ.

            Ngoài khu trung tâm, đừng xem mọi trải nghiệm đều phải thật đông hoặc thật mới. Một chuyến tham quan có hướng dẫn về kiến trúc, một lớp thủ công có quy mô phù hợp hoặc một cuộc trò chuyện với người làm nghề có thể đem lại nhiều bối cảnh hơn. Khi lựa chọn cẩn trọng, du khách vừa có chuyến đi sâu sắc hơn vừa góp phần giảm áp lực lên không gian di sản mong manh này.
            """,
        ("nha-trang", "culture") => """
            Nha Trang thường được nhớ đến trước hết bởi vịnh biển, nhưng văn hóa của thành phố không chỉ nằm trên bãi cát. Nhịp sống ven bờ gắn với biển, với chợ và với những cộng đồng đã cùng tạo nên diện mạo của Khánh Hòa. Khi rời khu vực bãi biển, du khách có thể nhận ra những lớp văn hóa khác qua không gian tôn giáo, đời sống địa phương và cách cư dân sử dụng bờ vịnh.

            Tháp Bà Pô Nagar là một điểm cần được tiếp cận với sự tôn trọng. Đây là nơi gắn với kiến trúc, tín ngưỡng, điêu khắc và lễ hội của văn hóa Chăm, đồng thời là một không gian linh thiêng đối với nhiều người. Khi tham quan, hãy ăn mặc phù hợp, giữ yên lặng ở khu vực cần thiết và làm theo hướng dẫn tại chỗ. Đừng biến nghi lễ hay người hành lễ thành đối tượng chụp ảnh nếu chưa được đồng ý.

            Một cách hiểu tốt hơn về Nha Trang là cân bằng giữa vui chơi ven biển và việc tìm hiểu bối cảnh văn hóa. Hãy hỏi hướng dẫn viên hoặc đọc thông tin chính thức trước khi kể lại các câu chuyện truyền miệng. Sự cẩn trọng ấy giúp chuyến đi không chỉ đẹp về hình ảnh mà còn tôn trọng những cộng đồng và ký ức làm nên vùng biển này.
            """,
        ("nha-trang", "food") => """
            Ẩm thực Nha Trang chịu ảnh hưởng rõ rệt của đời sống ven biển. Hải sản, các món nước và những lựa chọn gắn với chợ địa phương thường xuất hiện trong trải nghiệm của du khách. Tuy nhiên, biển không phải một thực đơn đồng nhất: mùa vụ, nguồn cung và cách chế biến thay đổi, vì vậy điều đáng làm là hỏi người bán về nguyên liệu trong ngày và chọn phần ăn phù hợp với sức khỏe của mình.

            Nếu muốn thử món mới, hãy bắt đầu bằng lượng vừa phải và nói rõ về dị ứng, đặc biệt với hải sản. Các gia vị lên men, nước chấm hoặc món có độ cay có thể khác với khẩu vị quen thuộc. Một cuộc hỏi đáp ngắn giúp người bán phục vụ đúng hơn và giảm nguy cơ lãng phí. Hãy ưu tiên nơi bảo quản thực phẩm hợp vệ sinh, có nước uống an toàn và không khuyến khích khai thác những loài có nguy cơ hoặc sản phẩm không rõ nguồn gốc.

            Ăn uống có trách nhiệm ở một thành phố biển cũng là nghĩ đến môi trường: hạn chế đồ nhựa dùng một lần khi có lựa chọn, không bỏ rác xuống bờ kè và dùng vừa đủ. Khi đó, một bữa ăn không chỉ cho bạn hương vị của vùng duyên hải mà còn thể hiện sự tôn trọng đối với biển, nguồn sống và những người làm nghề quanh vịnh.
            """,
        ("nha-trang", "tourism") => """
            Nha Trang cho phép du khách kết hợp thời gian bên biển với những điểm văn hóa và không gian xanh quanh vịnh. Bạn có thể dành buổi sáng hoặc chiều cho bãi biển khi điều kiện thời tiết phù hợp, sau đó chọn một hoạt động có bối cảnh như tham quan Tháp Bà Pô Nagar, chùa Long Sơn hoặc tìm hiểu đời sống địa phương. Đừng cố gắng biến cả vịnh thành một danh sách phải hoàn thành trong một ngày.

            An toàn trên biển cần được đặt trước ảnh đẹp. Hãy xem biển báo, hỏi về khu vực bơi an toàn, tuân theo hướng dẫn của nhân viên cứu hộ và cân nhắc thời tiết trước mọi hoạt động trên nước. Không bước lên rạn san hô, không mang sinh vật biển về làm kỷ niệm và không đặt niềm tin vào lời chào bán thiếu thông tin rõ ràng. Với các dịch vụ tàu thuyền, hãy ưu tiên đơn vị có thông tin vận hành minh bạch.

            Một hành trình tốt luôn có khoảng nghỉ để uống nước, tránh nắng và quan sát thay đổi của thời tiết. Khi ghé nơi tín ngưỡng, hãy thay đổi trang phục nếu cần và giữ thái độ nhẹ nhàng. Nha Trang sẽ đáng nhớ hơn khi trải nghiệm biển được đặt song song với trách nhiệm bảo vệ cảnh quan và sự tôn trọng dành cho văn hóa địa phương.
            """,
        _ => throw new ArgumentOutOfRangeException(nameof(slug), $"Unknown Vietnamese baseline narration: {slug}/{category}.")
    }) + "\n\n" + VietnameseClosing(slug, category) + "\n\n" + VietnameseDepth(slug, category);

    private static string VietnameseClosing(string slug, string category) => (slug, category) switch
    {
        ("ha-noi", "culture") => "Nếu có thêm thời gian, hãy chọn một điểm lưu trữ, một buổi biểu diễn hoặc một khu phố để tìm hiểu kỹ thay vì đi thật nhiều nơi. Ghi chú điều khiến bạn bất ngờ và đối chiếu với thông tin tại chỗ. Cách làm đó biến việc tham quan từ tiêu thụ hình ảnh thành một cuộc gặp gỡ có suy nghĩ với Hà Nội.",
        ("ha-noi", "food") => "Bạn cũng có thể xem cách người dân kết hợp món ăn với thời điểm trong ngày: bữa sáng sớm, bữa trưa nhanh hay một cuộc hẹn cà phê kéo dài. Không cần cố nếm mọi thứ. Chọn một vài món, ăn chậm và lắng nghe câu chuyện phía sau chúng sẽ cho bạn ký ức đáng tin cậy hơn về Hà Nội.",
        ("ha-noi", "tourism") => "Nếu muốn đi xa hơn trung tâm, hãy xem bản đồ và thông tin chính thức để chọn khu vực phù hợp với thời gian của bạn. Tránh di chuyển chỉ để chạy theo xu hướng ngắn hạn. Một tuyến đi bộ có chủ đề, được điều chỉnh theo sức khỏe và thời tiết, thường là cách an toàn nhất để kết nối các mảnh ghép của Hà Nội.",
        ("ho-chi-minh-city", "culture") => "Hãy để hành trình có chỗ cho những cuộc gặp gỡ bình thường: lời chào ở cửa hàng, tiếng rao trong hẻm, hay một giờ nghỉ của người làm việc quanh bạn. Đó là những điều không thể thay bằng một danh sách địa danh. Khi quan sát với sự tôn trọng, bạn sẽ hiểu thành phố vừa là nơi của ký ức, vừa là nơi mọi người đang tạo ra tương lai mỗi ngày.",
        ("ho-chi-minh-city", "food") => "Bạn có thể biến một bữa ăn thành điểm dừng để quan sát cách đô thị tổ chức thời gian: người đi làm, học sinh, gia đình và người bán cùng chia sẻ một không gian nhỏ. Hãy thanh toán minh bạch, giữ gìn sạch sẽ và nói cảm ơn khi có thể. Những cử chỉ ấy làm trải nghiệm ẩm thực thân thiện hơn với cả du khách lẫn người địa phương.",
        ("ho-chi-minh-city", "tourism") => "Hãy điều chỉnh lịch theo sức khỏe, mật độ giao thông và thời tiết thay vì cố giữ một kế hoạch cứng nhắc. Nếu có câu hỏi về lịch sử, hãy tìm nguồn bảo tàng hoặc hướng dẫn viên có kiểm chứng. Từ sự chuẩn bị đó, những con đường đông đúc không còn chỉ là thử thách, mà trở thành phần kể chuyện sống động của thành phố.",
        ("da-nang", "culture") => "Đà Nẵng cũng nhắc chúng ta rằng cảnh quan không phải tài sản vô hạn. Chỉ một chai nhựa bỏ lại, một bước chân sai nơi hoặc một tiếng ồn không đúng lúc đều có thể tác động đến không gian chung. Hãy mang rác theo mình, sử dụng lối đi được chỉ dẫn và để lại nơi bạn đến trong tình trạng tốt cho người dân và du khách sau.",
        ("da-nang", "food") => "Nếu mua thực phẩm mang đi, hãy cân nhắc hộp đựng có thể tái sử dụng hoặc từ chối vật dụng không cần thiết. Hành động nhỏ này đặc biệt có ý nghĩa ở thành phố ven biển. Khi món ăn được thưởng thức cùng ý thức bảo vệ môi trường, ký ức về Đà Nẵng sẽ gắn với cả hương vị lẫn trách nhiệm.",
        ("da-nang", "tourism") => "Khi lựa chọn hoạt động, đừng quên hỏi về khả năng tiếp cận, mức vận động và những điều cần chuẩn bị. Sự trung thực về giới hạn của bản thân giúp tránh tình huống nguy hiểm ở nắng, bậc đá hoặc vùng nước. Đà Nẵng sẽ đẹp hơn khi được trải nghiệm theo tốc độ phù hợp với cả con người lẫn cảnh quan.",
        ("hue", "culture") => "Di sản chỉ có ý nghĩa lâu dài khi người đến hiểu rằng nó dễ bị tổn thương. Việc không vẽ bậy, không bỏ rác và không vượt rào có thể tưởng nhỏ nhưng là phần thiết thực của bảo tồn. Hãy mang theo sự tò mò, nhưng cũng mang theo sự khiêm nhường: nhiều điều ở Huế cần được lắng nghe và suy ngẫm hơn là chiếm lấy trong một khoảnh khắc.",
        ("hue", "food") => "Nếu muốn tìm hiểu sâu hơn, hãy để ý tên nguyên liệu, cách sắp xếp món và nhịp phục vụ thay vì chỉ so sánh với khẩu vị quen thuộc. Một trải nghiệm cởi mở giúp bạn nhận ra sự tinh tế có thể nằm trong chi tiết nhỏ. Hãy giữ lại những gì bạn thật sự thích, thay vì áp đặt một tiêu chuẩn duy nhất lên ẩm thực Huế.",
        ("hue", "tourism") => "Bạn cũng có thể chọn một hoạt động không cần di chuyển nhiều để cân bằng ngày tham quan, như đọc tài liệu, nghe giới thiệu hoặc ngồi quan sát cảnh quan. Hãy tôn trọng người dân đang làm việc và sinh sống gần di tích. Khi đó, hành trình ở Huế sẽ không chỉ là đi qua những dấu vết cũ mà còn là gặp gỡ thành phố đương đại.",
        ("hoi-an", "culture") => "Bảo tồn ở Hội An không phải là giữ mọi thứ bất động. Đó là nỗ lực cân bằng giữa cư dân, kinh doanh, du khách và vật liệu cũ dễ tổn thương. Khi bạn chọn đi nhẹ, tiêu dùng vừa đủ và tôn trọng riêng tư của người dân, bạn đang giúp không gian này còn cơ hội tiếp tục kể chuyện cho những người đến sau.",
        ("hoi-an", "food") => "Nếu nghe một câu chuyện về món ăn, hãy hỏi bằng thái độ tò mò và chấp nhận rằng mỗi gia đình có thể kể khác nhau. Tôn trọng sự đa dạng ấy làm chuyến đi chân thực hơn. Hãy ưu tiên nơi phục vụ có trách nhiệm, dùng vừa đủ và để không gian chung sạch sẽ; đó là cách đơn giản để cảm ơn thành phố đã đón bạn.",
        ("hoi-an", "tourism") => "Điều đáng mang về sau một buổi ở Hội An không nhất thiết là thật nhiều đồ lưu niệm. Có thể là hiểu biết về cách một cộng đồng cùng sống với nhà cổ, con sông và dòng khách. Hãy mua sắm có cân nhắc, hỏi về nguồn gốc sản phẩm và dành sự tôn trọng cho người làm nghề. Những lựa chọn đó tạo giá trị bền vững hơn cho chuyến đi.",
        ("nha-trang", "culture") => "Đừng vội tách biển ra khỏi đời sống con người. Mỗi bến thuyền, chợ hay nơi thờ tự đều có thể liên quan đến sinh kế và ký ức của cộng đồng. Hãy tránh lan truyền thông tin chưa kiểm chứng về nghi lễ hoặc lịch sử. Tìm hiểu từ nguồn chính thức và hỏi với thái độ khiêm nhường là cách tốt nhất để trở thành một vị khách được chào đón.",
        ("nha-trang", "food") => "Hãy xem lựa chọn của mình có tôn trọng nguồn lợi biển hay không. Không cần thử mọi loại hải sản để hiểu vùng duyên hải; một bữa ăn được chọn cẩn thận đã đủ để cảm nhận sự kết nối giữa biển và bàn ăn. Sau khi dùng xong, hãy phân loại hoặc mang theo rác khi có thể để bờ vịnh luôn sạch cho cộng đồng.",
        ("nha-trang", "tourism") => "Nếu chương trình phải thay đổi vì gió, mưa hoặc cảnh báo an toàn, hãy xem đó là lựa chọn đúng đắn chứ không phải mất mát. Vịnh biển luôn biến đổi theo điều kiện tự nhiên. Tôn trọng giới hạn ấy giúp bạn trở về với ký ức tốt, đồng thời bảo vệ những hệ sinh thái và không gian văn hóa làm Nha Trang đáng để khám phá.",
        _ => throw new ArgumentOutOfRangeException(nameof(slug), $"Unknown Vietnamese baseline closing: {slug}/{category}.")
    };

    private static string VietnameseDepth(string slug, string category) => (slug, category) switch
    {
        ("ha-noi", "culture") => "Trước khi rời khu phố, hãy thử ghi lại một âm thanh, một mùi hương và một chi tiết kiến trúc khiến bạn dừng lại. So sánh chúng với thông tin ở bảo tàng hay bảng giới thiệu gần đó sẽ giúp câu chuyện về Thăng Long bớt trừu tượng và gắn hơn với trải nghiệm của chính bạn.",
        ("ha-noi", "food") => "Một cách thú vị để nhớ bữa ăn là chú ý đến thời điểm người dân dùng món: buổi sớm, giữa trưa hay lúc trò chuyện cuối ngày. Những nhịp thời gian ấy cho thấy ẩm thực không tách rời đời sống. Hãy để sự tò mò dẫn đường, nhưng luôn tôn trọng không gian làm việc của người bán.",
        ("ha-noi", "tourism") => "Nếu cần chọn giữa đi thêm một điểm và ở lại lâu hơn tại một nơi, hãy cân nhắc lựa chọn thứ hai. Đọc một tấm biển, nghe một câu chuyện địa phương hay quan sát sự thay đổi của ánh sáng bên hồ có thể cho bạn hiểu về Hà Nội nhiều hơn một lịch trình dày đặc.",
        ("ho-chi-minh-city", "culture") => "Hãy thử nhìn thành phố từ độ cao của người đi bộ: mặt tiền cũ, lối vào hẻm, biển hiệu và khoảng bóng râm thường kể những câu chuyện khác với đường chân trời. Cách quan sát chậm này cũng giúp bạn nhận ra nơi nào nên nhường đường, giữ yên lặng hoặc đơn giản là dừng lại để lắng nghe.",
        ("ho-chi-minh-city", "food") => "Ở một thành phố luôn chuyển động, bữa ăn còn là cơ hội để nghỉ và định hướng lại hành trình. Hãy chọn thời gian không quá gấp, chú ý cách rác được xử lý và ưu tiên phương án giảm đồ dùng một lần khi có thể. Sự chủ động nhỏ ấy làm trải nghiệm đô thị dễ chịu hơn cho mọi người.",
        ("ho-chi-minh-city", "tourism") => "Sau mỗi điểm dừng, hãy để bản đồ chỉ là gợi ý chứ không phải áp lực. Một đoạn đường có bóng cây, một không gian triển lãm nhỏ hoặc một góc phố có biển thông tin rõ ràng đều có thể trở thành phần đáng nhớ. Đi chậm và hỏi đúng nơi giúp hành trình an toàn trong một đô thị đông đúc.",
        ("da-nang", "culture") => "Bạn có thể kết thúc phần khám phá bằng cách nhìn lại mối liên hệ giữa sông, biển và núi thay vì xem chúng là ba phông nền tách biệt. Khi giữ khoảng cách phù hợp với nơi linh thiêng và cảnh quan nhạy cảm, du khách đang góp phần để những câu chuyện của Đà Nẵng được kể tiếp một cách tử tế.",
        ("da-nang", "food") => "Một bữa ăn có ý nghĩa không cần quá nhiều món. Hãy chọn khẩu phần vừa đủ, quan sát cách nguyên liệu được chuẩn bị và hỏi khi chưa chắc chắn về thành phần. Từ những quyết định nhỏ đó, bạn sẽ thấy ẩm thực Đà Nẵng gắn với biển, chợ và thói quen sống chứ không chỉ là một danh sách đặc sản.",
        ("da-nang", "tourism") => "Trước khi chuyển sang điểm tiếp theo, hãy kiểm tra lại nước uống, thời tiết và sức khỏe của cả nhóm. Một hành trình có khoảng dự phòng sẽ giúp bạn tránh vội vàng ở nơi có nắng, đá hoặc mặt nước. Sự chuẩn bị ấy cũng để lại không gian cho những quan sát tự nhiên mà lịch trình dày đặc thường bỏ qua.",
        ("hue", "culture") => "Khi lắng nghe câu chuyện về kinh đô, hãy phân biệt giữa điều bạn biết từ nguồn chính thức và điều được kể lại như cảm nhận cá nhân. Sự cẩn trọng ấy đặc biệt cần thiết ở Huế, nơi mỗi chi tiết kiến trúc hay nghi lễ có thể mang nhiều lớp nghĩa. Hãy để những câu hỏi mở dẫn bạn đến việc tìm hiểu sâu hơn.",
        ("hue", "food") => "Bạn có thể làm chuyến ăn uống chậm lại bằng cách quan sát cách món được dọn, phần nào được ăn cùng nhau và người địa phương giới thiệu món ra sao. Những chi tiết này giúp nhận ra sự tinh tế mà không cần biến mọi trải nghiệm thành đánh giá hơn thua. Hãy giữ sự tò mò đi cùng thái độ lắng nghe.",
        ("hue", "tourism") => "Trên đường rời một di tích, hãy thử nhìn lại tuyến đi vừa qua: cổng, sân, mặt nước và khoảng cây xanh có thể tạo thành một câu chuyện không lời. Ghi nhận những gì chưa hiểu để tra cứu sau chuyến đi. Cách kết nối quan sát với nguồn đáng tin cậy sẽ giúp trải nghiệm Huế sâu và bền hơn.",
        ("hoi-an", "culture") => "Khi phố lên đèn hoặc dòng người đông hơn, hãy nhớ rằng đây trước hết vẫn là nơi cư dân sinh sống. Đi nhẹ, giữ lối đi thông thoáng và tránh hướng máy ảnh vào không gian riêng tư giúp cả du khách lẫn người địa phương cùng tận hưởng phố cổ. Sự tôn trọng ấy là một phần thiết thực của bảo tồn.",
        ("hoi-an", "food") => "Một món ăn ở Hội An có thể dẫn bạn đến câu chuyện về chợ, con sông, nghề gia đình và giao lưu thương cảng. Để câu chuyện không bị đơn giản hóa, hãy tránh khẳng định nguồn gốc khi chưa có tài liệu tin cậy. Hỏi, lắng nghe và thưởng thức vừa đủ là cách khám phá chân thực hơn.",
        ("hoi-an", "tourism") => "Nếu có thời gian, hãy chọn một khoảnh khắc yên hơn để quan sát tỷ lệ ngôi nhà, cửa gỗ, mái ngói và mặt nước. Những chi tiết đó giải thích vì sao không gian này cần sự chăm sóc liên tục. Chuyến đi sẽ có chiều sâu hơn khi bạn rời phố với hiểu biết về cộng đồng đang giữ gìn nó.",
        ("nha-trang", "culture") => "Bên cạnh cảnh biển, hãy dành thời gian đọc bối cảnh của các địa điểm văn hóa để không nhìn chúng như đạo cụ du lịch. Tôn trọng trang phục, tiếng ồn và khu vực hạn chế là những lựa chọn rất cụ thể. Chúng giúp người đến sau vẫn có cơ hội gặp gỡ Nha Trang như một không gian sống, không chỉ là một bãi tắm.",
        ("nha-trang", "food") => "Trước khi gọi món tiếp theo, hãy nghĩ đến nguồn gốc nguyên liệu và lượng thức ăn nhóm mình thực sự dùng được. Lựa chọn có cân nhắc vừa giúp giảm lãng phí vừa tạo điều kiện để bạn trò chuyện nhiều hơn với người phục vụ về cách chế biến. Đó là một cách nhẹ nhàng để kết nối với đời sống ven biển.",
        ("nha-trang", "tourism") => "Kết thúc một ngày ở vịnh, hãy kiểm tra lại rác cá nhân, đồ dùng an toàn và những thông tin bạn dự định chia sẻ. Một bức ảnh đẹp nên đi kèm câu chuyện đúng bối cảnh, không cổ vũ hoạt động gây hại cho biển hay nơi thờ tự. Trải nghiệm có trách nhiệm sẽ làm ký ức về Nha Trang trọn vẹn hơn.",
        _ => throw new ArgumentOutOfRangeException(nameof(slug), $"Unknown Vietnamese baseline depth: {slug}/{category}.")
    };

    private static string LocalizedNarration(string slug, string language, string category)
    {
        var focus = LocalizedFocus(slug, language, category);
        return language switch
        {
            "ko" => $"{focus} 이 안내는 현지의 역사와 생활을 존중하며 둘러보기 위한 출발점입니다. 운영 시간, 입장 규정, 날씨와 같은 변동 정보는 방문 당일의 공식 안내를 확인해 주세요. 조용히 관찰하고 공용 공간을 배려하면 더 깊고 안전한 여행이 됩니다.",
            "ja" => $"{focus} これは地域の歴史と暮らしを尊重して歩くための案内です。開館時間、入場規則、天候など変わり得る情報は、訪問当日の公式案内で確認してください。静かに観察し、共有空間に配慮することで、より豊かで安全な旅になります。",
            "zh" => $"{focus} 这段讲解旨在帮助您以尊重当地历史和生活的方式游览。开放时间、入场规定和天气等可能变化的信息，请以出行当天的官方公告为准。安静观察并照顾公共空间，能让旅程更深入、更安全。",
            "th" => $"{focus} เนื้อหานี้เป็นจุดเริ่มต้นสำหรับการท่องเที่ยวโดยเคารพประวัติศาสตร์และวิถีชีวิตท้องถิ่น โปรดตรวจสอบข้อมูลทางการในวันเดินทางสำหรับเวลาเปิดทำการ กฎการเข้าชม และสภาพอากาศ การสังเกตอย่างสงบและคำนึงถึงพื้นที่ส่วนรวมจะทำให้การเดินทางลึกซึ้งและปลอดภัยยิ่งขึ้น",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Unsupported localized baseline narration language.")
        };
    }

    private static string LocalizedFocus(string slug, string language, string category) => (slug, language, category) switch
    {
        ("ha-noi", "ko", "culture") => "하노이는 탕롱의 긴 역사와 오늘의 수도 생활이 겹쳐 있는 도시입니다. 탕롱 황성 유적과 구시가의 거리, 주민들의 일상에서 여러 시대의 흔적을 살펴보세요.",
        ("ha-noi", "ko", "food") => "하노이에서는 쌀국수와 분짜, 커피 문화가 일상의 리듬을 보여 줍니다. 재료와 알레르기 정보를 먼저 묻고, 필요한 만큼만 주문해 보세요.",
        ("ha-noi", "ko", "tourism") => "호안끼엠 호수와 구시가, 탕롱 황성, 박물관을 서두르지 않고 나누어 둘러보면 하노이의 여러 층위를 이해하기 좋습니다.",
        ("ho-chi-minh-city", "ko", "culture") => "호찌민시는 사이공이라는 기억과 남부의 빠른 도시 리듬이 만나는 곳입니다. 중심가의 공공 건축과 시장, 골목의 생활을 함께 살펴보세요.",
        ("ho-chi-minh-city", "ko", "food") => "반미, 껌땀, 반쎄오는 호찌민시의 다양한 식문화를 만나는 출발점입니다. 맵기와 재료를 확인하고 위생 상태가 분명한 곳을 선택하세요.",
        ("ho-chi-minh-city", "ko", "tourism") => "동커이 거리 주변의 건축, 박물관, 시장을 하나의 주제로 천천히 연결해 보세요. 혼잡한 곳에서는 개인 소지품과 보행 안전에 유의해야 합니다.",
        ("da-nang", "ko", "culture") => "다낭은 바다와 한강, 산이 만나는 도시입니다. 오행산의 동굴과 사원처럼 자연과 신앙이 만나는 장소에서는 보존 규칙을 지켜 주세요.",
        ("da-nang", "ko", "food") => "미꽝과 해산물은 다낭과 중부의 재료, 시장, 바다의 관계를 떠올리게 합니다. 익숙하지 않은 젓갈이나 매운맛은 먼저 물어보세요.",
        ("da-nang", "ko", "tourism") => "해변, 한강, 오행산은 서로 다른 속도로 둘러볼 만한 다낭의 풍경입니다. 바위길과 계단에서는 적절한 신발과 여유 있는 일정이 중요합니다.",
        ("hue", "ko", "culture") => "후에는 1802년부터 1945년까지 응우옌 왕조의 수도였던 유산 도시입니다. 성곽, 궁전, 강의 풍경을 한 맥락으로 바라보면 이해가 깊어집니다.",
        ("hue", "ko", "food") => "후에의 음식은 작은 구성과 균형 잡힌 맛으로 자주 기억됩니다. 궁중 전통만이 아니라 시장과 가정의 일상 음식에도 관심을 가져 보세요.",
        ("hue", "ko", "tourism") => "대내와 왕릉, 향강 주변을 하루에 모두 서두르기보다 주제별로 나누어 둘러보세요. 보존 구역에서는 표지와 동선을 따르세요.",
        ("hoi-an", "ko", "culture") => "호이안 고도시는 15세기부터 19세기 사이의 동남아 무역항 흔적을 간직합니다. 목조 주택과 강변 거리에서 여러 문화의 교류를 읽어 볼 수 있습니다.",
        ("hoi-an", "ko", "food") => "까오러우와 작은 전통 음식은 호이안의 시장과 강변 생활을 떠올리게 합니다. 알레르기와 식단을 먼저 알리고, 주거 공간과 보행로를 배려해 주세요.",
        ("hoi-an", "ko", "tourism") => "호이안은 걷는 속도로 구시가와 강변을 둘러볼 때 가장 잘 보입니다. 주민이 사는 유산 공간이므로 출입 규정과 촬영 제한을 존중하세요.",
        ("nha-trang", "ko", "culture") => "냐짱의 바다는 지역 생활과 문화의 배경입니다. 포나가르 참 탑은 참 건축과 신앙이 이어지는 신성한 장소이므로 복장과 예절에 유의하세요.",
        ("nha-trang", "ko", "food") => "냐짱의 해산물과 해안 음식은 계절과 공급에 따라 달라집니다. 신선도와 알레르기 정보를 확인하고, 출처가 불분명한 해양 제품은 피하세요.",
        ("nha-trang", "ko", "tourism") => "해변과 만, 포나가르 탑, 롱선 사원을 함께 보면 냐짱의 자연과 문화를 균형 있게 만날 수 있습니다. 해상 활동에서는 안전 표지와 구조요원의 안내를 따르세요.",
        ("ha-noi", "ja", "culture") => "ハノイはタンロンの長い歴史と今日の首都の暮らしが重なる都市です。タンロン王城跡、旧市街、住民の日常から多層的な時間を感じてください。",
        ("ha-noi", "ja", "food") => "ハノイではフォー、ブンチャー、コーヒー文化が日常のリズムを伝えます。食材やアレルギーを確認し、食べ切れる量を注文しましょう。",
        ("ha-noi", "ja", "tourism") => "ホアンキエム湖、旧市街、タンロン王城、博物館を急がず分けて訪ねると、ハノイの多様な表情を理解しやすくなります。",
        ("ho-chi-minh-city", "ja", "culture") => "ホーチミン市はサイゴンの記憶と南部の速い都市のリズムが出会う場所です。中心部の公共建築、市場、路地の暮らしをあわせて見てみましょう。",
        ("ho-chi-minh-city", "ja", "food") => "バインミー、コムタム、バインセオは、この街の多様な食文化への入り口です。辛さと食材を確認し、衛生状態が分かる店を選びましょう。",
        ("ho-chi-minh-city", "ja", "tourism") => "ドンコイ通り周辺の建築、博物館、市場を一つのテーマでゆっくり結ぶとよいでしょう。混雑する場所では持ち物と歩行の安全に注意してください。",
        ("da-nang", "ja", "culture") => "ダナンは海、ハン川、山が出会う都市です。五行山の洞窟や寺院のように自然と信仰が重なる場所では、保全の規則を守ってください。",
        ("da-nang", "ja", "food") => "ミークアンや海の幸は、ダナンと中部の食材、市場、海のつながりを感じさせます。発酵調味料や辛さが初めてなら、先に尋ねましょう。",
        ("da-nang", "ja", "tourism") => "海辺、ハン川、五行山は、それぞれ違う速さで楽しみたいダナンの景観です。岩場や階段では歩きやすい靴と余裕のある予定が大切です。",
        ("hue", "ja", "culture") => "フエは1802年から1945年まで阮朝の都だった遺産都市です。城壁、宮殿、川の風景を一つの文脈で見ると理解が深まります。",
        ("hue", "ja", "food") => "フエの食は小さな品数と味のバランスでよく知られます。宮廷の伝統だけでなく、市場や家庭の普段の料理にも目を向けてみましょう。",
        ("hue", "ja", "tourism") => "大内、王陵、香江周辺を一日に急いで回るより、テーマごとに分けて歩いてください。保存区域では表示と見学動線に従いましょう。",
        ("hoi-an", "ja", "culture") => "ホイアン旧市街は、15世紀から19世紀にかけての東南アジア交易港の面影を残します。木造家屋と川沿いの通りから、複数の文化の交流を読み取れます。",
        ("hoi-an", "ja", "food") => "カオラウや小さな伝統料理は、ホイアンの市場と川辺の生活を思わせます。アレルギーや食事の希望を先に伝え、住居と歩道への配慮を忘れないでください。",
        ("hoi-an", "ja", "tourism") => "ホイアンは歩く速さで旧市街と川辺を見ると魅力がよく分かります。人が暮らす遺産空間なので、立ち入り規則と撮影制限を尊重してください。",
        ("nha-trang", "ja", "culture") => "ニャチャンの海は地域の暮らしと文化の背景です。ポーナガル・チャム塔は建築と信仰が続く聖なる場所なので、服装と礼儀に配慮してください。",
        ("nha-trang", "ja", "food") => "ニャチャンの海産物と海辺の料理は季節や供給で変わります。鮮度とアレルギーを確認し、出所が不明な海洋製品は避けましょう。",
        ("nha-trang", "ja", "tourism") => "海辺と湾、ポーナガル塔、ロンソン寺を組み合わせると、ニャチャンの自然と文化をバランスよく知ることができます。海の活動では安全表示と救助員の案内に従ってください。",
        ("ha-noi", "zh", "culture") => "河内是一座把升龙悠久历史与当代首都生活叠在一起的城市。可从升龙皇城遗址、老城区和居民日常中观察多层时间。",
        ("ha-noi", "zh", "food") => "河内的河粉、烤肉米线和咖啡文化呈现日常节奏。请先确认食材和过敏信息，并按实际需要点餐。",
        ("ha-noi", "zh", "tourism") => "把还剑湖、老城区、升龙皇城和博物馆分开慢慢游览，更容易理解河内的多种面貌。",
        ("ho-chi-minh-city", "zh", "culture") => "胡志明市让西贡记忆与南方快速的城市节奏相遇。可把市中心公共建筑、市场和小巷生活放在一起观察。",
        ("ho-chi-minh-city", "zh", "food") => "法棍、碎米饭和越南煎饼是认识本城多样饮食的起点。请确认辣度和食材，并选择卫生状况清楚的餐饮场所。",
        ("ho-chi-minh-city", "zh", "tourism") => "可用一个主题慢慢串联同起街周边的建筑、博物馆和市场。在拥挤地点请注意随身物品和步行安全。",
        ("da-nang", "zh", "culture") => "岘港是大海、韩江和群山相遇的城市。在五行山洞穴和寺院等自然与信仰交汇处，请遵守保护规定。",
        ("da-nang", "zh", "food") => "广南面和海鲜让人联想到岘港及中部的食材、市场与海洋关系。第一次尝试发酵调味料或辣味时，请先询问。",
        ("da-nang", "zh", "tourism") => "海滩、韩江和五行山构成不同节奏的岘港景观。走岩石路和阶梯时，合适的鞋子与从容的行程很重要。",
        ("hue", "zh", "culture") => "顺化是一座遗产城市，1802年至1945年曾是阮朝都城。把城墙、宫殿与香江风景放在同一脉络中观看，会有更深理解。",
        ("hue", "zh", "food") => "顺化饮食常以小巧的组合与味道平衡为人所知。除了宫廷传统，也请留意市场和家庭中的日常菜肴。",
        ("hue", "zh", "tourism") => "与其一天匆匆游遍皇城、皇陵和香江周边，不如按主题分开游览。在保护区请遵循标识和参观路线。",
        ("hoi-an", "zh", "culture") => "会安古城保留了十五至十九世纪东南亚贸易港的印记。木屋和河畔街道展现了多种文化之间的交流。",
        ("hoi-an", "zh", "food") => "高楼面和小巧传统点心让人想到会安的市场与河畔生活。请先告知过敏和饮食需求，并照顾住宅与步行空间。",
        ("hoi-an", "zh", "tourism") => "以步行速度游览古城和河岸，最能感受会安。这里也是居民生活的遗产空间，请尊重进入规定和拍摄限制。",
        ("nha-trang", "zh", "culture") => "芽庄的大海是当地生活与文化的背景。婆那加占婆塔延续着建筑与信仰传统，是神圣场所，请注意衣着与礼仪。",
        ("nha-trang", "zh", "food") => "芽庄的海产和滨海料理会随季节与供应改变。请确认新鲜度和过敏信息，避免来源不明的海洋产品。",
        ("nha-trang", "zh", "tourism") => "结合海滩、海湾、婆那加塔和龙山寺，可较平衡地认识芽庄的自然与文化。水上活动时请遵守安全标识和救生员指引。",
        ("ha-noi", "th", "culture") => "ฮานอยเป็นเมืองที่ประวัติศาสตร์อันยาวนานของทังลองซ้อนทับกับชีวิตของเมืองหลวงในปัจจุบัน ลองมองร่องรอยของหลายยุคสมัยผ่านโบราณสถานพระราชวังทังลอง ย่านเก่า และวิถีชีวิตของชาวเมือง",
        ("ha-noi", "th", "food") => "เฝอ บุ้นจ่า และวัฒนธรรมกาแฟสะท้อนจังหวะชีวิตประจำวันของฮานอย ควรถามเรื่องวัตถุดิบและอาการแพ้ก่อน แล้วสั่งในปริมาณที่รับประทานได้พอดี",
        ("ha-noi", "th", "tourism") => "การแบ่งเวลาเที่ยวทะเลสาบฮหว่านเกี๊ยม ย่านเก่า พระราชวังทังลอง และพิพิธภัณฑ์อย่างไม่เร่งรีบ ช่วยให้เห็นหลายแง่มุมของฮานอยได้ชัดขึ้น",
        ("ho-chi-minh-city", "th", "culture") => "นครโฮจิมินห์คือพื้นที่ที่ความทรงจำของไซ่ง่อนมาพบกับจังหวะเมืองทางใต้ที่รวดเร็ว ลองมองอาคารสาธารณะ ตลาด และชีวิตในตรอกซอยควบคู่กัน",
        ("ho-chi-minh-city", "th", "food") => "บั๊ญหมี่ ก๋อมตั๊ม และบั๊ญแส่วเป็นจุดเริ่มต้นของการรู้จักอาหารที่หลากหลายของเมือง ควรถามระดับความเผ็ดและวัตถุดิบ แล้วเลือกร้านที่ดูแลความสะอาดชัดเจน",
        ("ho-chi-minh-city", "th", "tourism") => "ลองเชื่อมอาคาร พิพิธภัณฑ์ และตลาดรอบถนนด่งเคยตามหัวข้อเดียวอย่างค่อยเป็นค่อยไป ในบริเวณคนหนาแน่นควรระวังทรัพย์สินและความปลอดภัยขณะเดิน",
        ("da-nang", "th", "culture") => "ดานังเป็นเมืองที่ทะเล แม่น้ำฮั่น และภูเขามาบรรจบกัน ในสถานที่อย่างภูเขาหินอ่อนที่ธรรมชาติและความศรัทธาพบกัน ควรปฏิบัติตามกฎการอนุรักษ์",
        ("da-nang", "th", "food") => "หมี่กว๋างและอาหารทะเลชวนให้นึกถึงความเชื่อมโยงระหว่างวัตถุดิบ ตลาด และทะเลของดานังกับภาคกลาง หากไม่คุ้นกับเครื่องปรุงหมักหรือรสเผ็ด ควรถามก่อน",
        ("da-nang", "th", "tourism") => "ชายหาด แม่น้ำฮั่น และภูเขาหินอ่อนเป็นทิวทัศน์ของดานังที่ควรเที่ยวด้วยจังหวะต่างกัน เมื่อเดินบนหินหรือบันได รองเท้าที่เหมาะสมและตารางเวลาที่ไม่เร่งรีบสำคัญมาก",
        ("hue", "th", "culture") => "เว้เป็นเมืองมรดกที่เคยเป็นราชธานีของราชวงศ์เหงียนระหว่าง ค.ศ. 1802 ถึง 1945 การมองกำแพงเมือง พระราชวัง และภูมิทัศน์แม่น้ำหอมในบริบทเดียวกันช่วยให้เข้าใจได้ลึกขึ้น",
        ("hue", "th", "food") => "อาหารเว้มักเป็นที่จดจำจากจานเล็กและความสมดุลของรสชาติ นอกจากประเพณีในราชสำนักแล้ว ลองสนใจอาหารประจำวันในตลาดและครัวเรือนด้วย",
        ("hue", "th", "tourism") => "แทนที่จะเร่งเที่ยวพระราชวัง สุสานจักรพรรดิ และริมแม่น้ำหอมในวันเดียว ลองแบ่งตามหัวข้อ ในเขตอนุรักษ์ควรปฏิบัติตามป้ายและเส้นทางชม",
        ("hoi-an", "th", "culture") => "เมืองเก่าฮอยอันเก็บร่องรอยของท่าเรือการค้าเอเชียตะวันออกเฉียงใต้ระหว่างคริสต์ศตวรรษที่ 15 ถึง 19 บ้านไม้และถนนริมน้ำสะท้อนการแลกเปลี่ยนของหลายวัฒนธรรม",
        ("hoi-an", "th", "food") => "เกาเหล่าและอาหารพื้นเมืองชิ้นเล็กชวนให้นึกถึงตลาดและชีวิตริมแม่น้ำของฮอยอัน ควรแจ้งเรื่องการแพ้และข้อจำกัดด้านอาหารก่อน รวมทั้งคำนึงถึงบ้านพักอาศัยและทางเดิน",
        ("hoi-an", "th", "tourism") => "การเดินชมย่านเก่าและริมน้ำด้วยจังหวะสบาย ๆ ทำให้เห็นเสน่ห์ของฮอยอันได้ดีที่สุด ที่นี่เป็นพื้นที่มรดกที่มีผู้อยู่อาศัย จึงควรเคารพกฎการเข้าและข้อจำกัดการถ่ายภาพ",
        ("nha-trang", "th", "culture") => "ทะเลของญาจางเป็นฉากหลังของวิถีชีวิตและวัฒนธรรมท้องถิ่น หอคอยโพนครเป็นสถานที่ศักดิ์สิทธิ์ที่สืบเนื่องทั้งสถาปัตยกรรมและความศรัทธาของจาม จึงควรแต่งกายและปฏิบัติตนอย่างเหมาะสม",
        ("nha-trang", "th", "food") => "อาหารทะเลและอาหารริมฝั่งของญาจางเปลี่ยนไปตามฤดูกาลและแหล่งวัตถุดิบ ควรตรวจสอบความสดและอาการแพ้ และหลีกเลี่ยงผลิตภัณฑ์ทางทะเลที่ไม่ทราบแหล่งที่มา",
        ("nha-trang", "th", "tourism") => "การรวมชายหาด อ่าว หอคอยโพนคร และวัดลองเซินช่วยให้รู้จักธรรมชาติและวัฒนธรรมของญาจางอย่างสมดุล สำหรับกิจกรรมทางน้ำ ควรปฏิบัติตามป้ายความปลอดภัยและคำแนะนำของเจ้าหน้าที่กู้ภัย",
        _ => throw new ArgumentOutOfRangeException(nameof(slug), $"Unknown localized baseline narration: {slug}/{language}/{category}.")
    };
}
