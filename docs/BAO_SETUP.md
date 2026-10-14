# Module Bảo - Backend Integration (dịch, TTS, job, worker)

## Yêu cầu
.NET 9 SDK, Docker (MySQL 8 + LibreTranslate), Python 3 với `edge-tts`.

```
pip install edge-tts
edge-tts --list-voices        # kiểm tra lệnh chạy được và xem tên giọng
```

## Chạy lần đầu
```
dotnet tool restore                      # cài dotnet-ef đúng phiên bản (dotnet-tools.json)
dotnet restore
dotnet user-secrets set "ConnectionStrings:Default" "server=localhost;port=3306;database=tourguide;user=tourguide;password=<MYSQL_PASSWORD trong .env>" --project src/TourGuide.Api
dotnet ef database update --project src/TourGuide.Api
dotnet run --project src/TourGuide.Api
```
- Chuỗi kết nối không nằm trong code hay appsettings.json. Chạy trong Docker thì đặt biến môi trường `ConnectionStrings__Default`, hoặc bộ `MYSQL_HOST/PORT/DATABASE/USER/PASSWORD`.
- Swagger: `http://localhost:5130/swagger`. Các request mẫu nằm trong `src/TourGuide.Api/TourGuide.Api.http`.
- Khi khởi động, nếu bảng `languages` trống thì app nạp sẵn vi, en, zh, ja, ko.
- Audio lưu ở `storage/audio/` (thư mục gốc repo, đã nằm trong .gitignore). Đổi bằng `Storage:Root`.
- Nếu đã chạy bản migration cũ trên máy mình (thiếu bảng `languages`): `dotnet ef database drop -f --project src/TourGuide.Api` rồi chạy lại `database update`.

## Kiểm tra rủi ro lớn nhất (tuần 1)
1. `docker run -d -p 5000:5000 libretranslate/libretranslate` (hoặc dùng docker compose của Tài).
2. `GET /languages/provider-check`: cho biết LibreTranslate có tiếng Việt không (`sourceSupported`), và từng ngôn ngữ đang bật có dịch được từ vi không (`canTranslateFromVi`). Cũng liệt kê mã ngôn ngữ thật của LibreTranslate (ví dụ tiếng Trung có thể là `zh` hoặc `zh-Hans`; nếu khác thì thêm ngôn ngữ đúng mã qua `POST /languages`).
3. `POST /places/1/translate` với `{"sourceText":"Xin chào, chào mừng bạn đến với Việt Nam.","languages":["en","zh","ja"]}`, sau đó `GET /jobs?status=failed` xem `lastError`.
4. Ghi kết quả vào docs/ rồi báo lại nhóm.

## Luồng xử lý
`POST /places/{id}/translate` tạo mỗi ngôn ngữ một job translate. Worker chia đoạn theo câu, dịch từng đoạn, lưu `Result`, rồi tạo job tts. Job tts chia đoạn, gọi Edge TTS từng đoạn, ghép mp3 và lưu `AudioPath`. Cùng nội dung thì dùng lại job và file audio cũ (cache). Lỗi thì thử lại tối đa 3 lần (chờ 20 giây, 40 giây, 80 giây), sau đó `failed`; Admin gọi `POST /jobs/{id}/retry`. Job `processing` quá 10 phút được đưa về `pending`. Giọng đọc lấy từ cột `TtsVoice` của bảng `languages`.

## Còn phải tích hợp với An và Tài
- Endpoint đang nhận `sourceText` trực tiếp; sau này lấy `description` từ Place của An và kiểm tra staff có phải chủ địa điểm.
- `IntegrationDbContext` và bảng `translation_jobs` đang tách riêng. Khi Tài có `AppDbContext`, gộp entity `Language`, `TranslationJob` vào đó (tên bảng theo ERD, kế hoạch gọi là `jobs`), gộp migration, rồi xóa `IntegrationDbContext`. Sau khi gộp, lệnh `dotnet ef` cần thêm `--context` nếu còn hai DbContext.
- Lưu `Result` và `AudioPath` vào `place_translations` / `audio_files` của Tài thay vì giữ trong job.
- Chưa có JWT nên các endpoint chưa phân quyền (đã ghi TODO trong controller). Không deploy public.
- Worker nhận job bằng UPDATE có điều kiện, chưa dùng `SKIP LOCKED` như kế hoạch; đủ an toàn cho nhiều worker, xem lại nếu cần.
- An làm API phát audio: dùng cùng `Storage:Root` (xem `TtsService.StorageRoot`) và đường dẫn tương đối `audio/{hash}.mp3`.
- Edge TTS dùng CLI Python (phương án dự phòng trong kế hoạch). Docker image của API phải cài Python và `pip install edge-tts`.
