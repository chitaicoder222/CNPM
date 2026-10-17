# TourGuide Narration – Proposal

**Nhóm:** An (nhóm trưởng, Backend Core), Bảo (Backend Integration), Tài (DevOps, Database, QA)

## 1. Bài toán

Một khu di tích hay điểm tham quan có nhiều địa điểm, du khách đến từ nhiều nước. Thuê hướng dẫn viên cho từng ngôn ngữ tốn kém. Nhân viên chỉ cần viết mô tả tiếng Việt một lần, hệ thống lo phần còn lại.

## 2. Giải pháp

Hệ thống thuyết minh tự động đa ngôn ngữ: **nhập → dịch → đọc → nghe**.

1. Nhân viên nhập mô tả địa điểm bằng tiếng Việt.
2. Hệ thống tự dịch sang nhiều ngôn ngữ (LibreTranslate) và tạo giọng đọc mp3 (Edge TTS), chạy nền bằng hàng đợi job có retry và cache.
3. Du khách chọn địa điểm, chọn ngôn ngữ, đọc và nghe thuyết minh.

## 3. Vai trò

| Vai trò | Làm được gì |
|---|---|
| Admin | Quản lý người dùng và ngôn ngữ hỗ trợ; xem trạng thái job; chạy lại job lỗi |
| Nhân viên (staff) | Tạo, sửa địa điểm; chọn ngôn ngữ đích; bấm dịch và tạo audio; theo dõi xử lý; sửa tay bản dịch |
| Du khách | Tìm địa điểm, chọn ngôn ngữ, đọc bản dịch, nghe audio; đăng nhập chỉ để lưu yêu thích (tùy chọn) |

## 4. Phạm vi

**Có làm:** backend REST API, worker chạy nền, cơ sở dữ liệu, CI/CD, Docker, Swagger, kiểm thử tự động; giao diện web tối thiểu để demo (hoặc Swagger/Postman); nội dung là văn bản thuần, chia đoạn nhỏ khi dịch và đọc.

**Không làm (hướng phát triển):** dịch và đọc trực tiếp video, phụ đề tự động từ giọng nói, thanh toán vé, ứng dụng di động riêng, bản đồ chỉ đường, ghi âm giọng người thật.

**Tùy chọn nếu dư thời gian:** mã QR cho từng địa điểm, danh sách yêu thích.

## 5. Công nghệ

ASP.NET Core Web API (.NET 9) chia 3 lớp; MySQL 8 + EF Core (Code-First); LibreTranslate (tự host); Edge TTS; hàng đợi bằng bảng `jobs` + BackgroundService; JWT + BCrypt; Swagger; xUnit + WebApplicationFactory; Docker, GitHub Actions, GHCR.

## 6. Phân công

| Thành viên | Phụ trách chính |
|---|---|
| An | Khung 3 lớp, quy ước code, Auth, User, Place, Favorite, API phát audio, Swagger, chuẩn response và mã lỗi, review Pull Request, tích hợp, đóng gói nộp bài |
| Bảo | Language, provider dịch và TTS, chia đoạn, TranslationService, TtsService, cache, retry, bảng jobs, worker nền, API job; thực nghiệm E1, E3 |
| Tài | ERD, Entity, DbContext, migration, seed; docker-compose, Dockerfile, CI/CD, secret, log, healthcheck, rollback; unit và integration test, coverage; thực nghiệm E2, E4 |

## 7. Mốc chính

| Mốc | Thời điểm | Kết quả |
|---|---|---|
| Mốc 1 | Cuối tuần 2 | ERD và danh sách API chốt |
| Mốc 2 | Cuối tuần 5 | Luồng end-to-end (nhập, dịch, đọc, nghe) chạy được |
| Nộp bài | Tuần 7 | Báo cáo, demo, tag v1.0 (tuần 6 chỉ tích hợp và đo thực nghiệm, tuần 7 không thêm tính năng) |

## 8. Rủi ro đã biết

- LibreTranslate có thể chưa dịch được từ tiếng Việt (xác nhận ở tuần 1); phương án dự phòng ở mục 3.7 của kế hoạch.
- Edge TTS là dịch vụ không chính thức; có phương án dự phòng (container Python hoặc Azure AI Speech).
- Máy dịch hay sai tên riêng và địa danh; cho nhân viên sửa tay và ghi vào phần hạn chế.
