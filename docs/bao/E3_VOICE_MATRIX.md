# E3 — giọng đọc theo thiết bị

Nguồn code: main `a39128650e5e780dc4a2fded45a74b81a50f1c1e`. Ngày chạy test: 02/11/2026.

## Kết quả tự động

Chạy tại gốc gói:

```powershell
node --test tests/Vivoice.Web.Tests/Frontend/tts-language.test.cjs
```

7/7 test PASS: locale của narration thay vì UI; năm ngôn ngữ; chuẩn hóa mã; metadata thiếu/sai; ưu tiên giọng exact locale; giọng cùng ngôn ngữ; thiếu giọng trả null. Đây là kiểm thử logic Node, không thực hiện SpeechSynthesis và không xác nhận nghe được âm thanh.

| Nội dung | Trạng thái |
| --- | --- |
| Ánh xạ vi-VN/ko-KR/ja-JP/zh-CN/en-US | PASS bằng unit test |
| Không có voice khớp | PASS logic chọn voice trả null |
| Phát audio, pause/resume/stop | Chưa thử trên thiết bị |
| Nội dung fallback được đọc đúng tiếng | Chưa thử toàn luồng |
| Ra vùng/tắt GPS hủy giọng | Chưa thử toàn luồng |

## Cách đo trên bản tích hợp

1. Ghi commit ứng dụng, ngày, thiết bị, OS và phiên bản browser; dùng bản tích hợp main qua HTTPS/localhost.
2. Bật định vị và vào vùng POI (ghi rõ nếu giả lập). Chọn từng ngôn ngữ, ghi languageCode API thực tế, locale và danh sách voice thiết bị có.
3. Nghe tên riêng và narration, ghi giọng đã dùng và lỗi thực tế; không ghi giọng yêu cầu là giọng thực tế nếu không xác nhận được.
4. Thử pause/resume/stop; đổi ngôn ngữ, ra vùng/tắt GPS và xác nhận lượt cũ không tiếp tục phát.
5. Với trường hợp thiếu giọng/fallback, dùng môi trường thử phù hợp và ghi rõ dữ liệu mô phỏng. Không xóa bản dịch trên DB thật để tạo lỗi. Kiểm tra văn bản vẫn đọc được khi audio không khả dụng.

## Ma trận chờ đo thật

| Ngày/commit | Thiết bị/OS/browser | Ngôn ngữ yêu cầu/thực tế | Voice có sẵn/voice dùng | Nghe/pause/resume/stop | Fallback/thiếu giọng | Bằng chứng |
| --- | --- | --- | --- | --- | --- | --- |
| Chưa đo | Chưa ghi nhận | vi | Chưa ghi nhận | Chưa thử | Chưa thử | Chưa có |
| Chưa đo | Chưa ghi nhận | ko | Chưa ghi nhận | Chưa thử | Chưa thử | Chưa có |
| Chưa đo | Chưa ghi nhận | ja | Chưa ghi nhận | Chưa thử | Chưa thử | Chưa có |
| Chưa đo | Chưa ghi nhận | zh | Chưa ghi nhận | Chưa thử | Chưa thử | Chưa có |
| Chưa đo | Chưa ghi nhận | en | Chưa ghi nhận | Chưa thử | Chưa thử | Chưa có |

Thiếu thử nghe thực tế là phần nghiệm thu còn mở, không phải lỗi được giải quyết chỉ bằng bổ sung tài liệu.
