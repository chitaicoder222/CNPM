# Danh sách Issue

Nguồn: bảng lộ trình 7 tuần (mục 5), phân việc (mục 4), thực nghiệm (mục 7) của file kế hoạch.

Cách tạo: tab **Issues** → **New issue**, dán tiêu đề và nội dung, gán Assignee, thêm Label (`tuan-1` … `tuan-7`, `an`, `bao`, `tai`, `moc-1`, `moc-2`), đưa vào Project board cột Todo. Pull Request ghi `Closes #số`.

**Hai mốc không được trễ:** cuối tuần 2 (ERD và danh sách API chốt), cuối tuần 5 (end-to-end chạy được).

---

## Tuần 1: Chốt đề tài, dựng môi trường

| # | Tiêu đề | Người | Checklist |
|---|---|---|---|
| 1 | Khung solution 3 lớp, global.json | An | Đã xong: solution, API, test, middleware lỗi, Swagger |
| 2 | Chốt chức năng, 3 vai trò, use case | An | Vẽ use case; chốt phạm vi theo mục 1.3 |
| 3 | PROPOSAL.md (1 trang) | An | Bản nháp đã có, rà lại |
| 4 | Tạo Issues và Project board | An | Board 3 cột; tạo các Issue này |
| 5 | Repo, mời thành viên và giảng viên | Tài | Mời Tài, Bảo, giảng viên (kế hoạch ghi Tài tạo repo) |
| 6 | Quy ước nhánh, commit | Tài | Ghi vào README; bật branch protection cho `main` |
| 7 | docker-compose MySQL 8.0 + LibreTranslate | Tài | `.env.example`; kiểm tra `mysql:8.0` |
| 8 | Hướng dẫn DBeaver | Tài | `allowPublicKeyRetrieval=true` |
| 9 | Chạy thử LibreTranslate bằng Docker | Bảo | Gọi `GET /languages`; **có tiếng Việt không? vi→en, zh, ja dịch được không?** |
| 10 | Thử Edge TTS cho từng ngôn ngữ | Bảo | Tìm package .NET (chưa kiểm chứng), giọng từng ngôn ngữ; dự phòng container Python hoặc Azure |
| 11 | Ghi bảng ngôn ngữ, độ trễ, lỗi | Bảo | Kết quả #9, #10 thành bảng (dùng cho mục 3.7) |

## Tuần 2: Thiết kế (MỐC 1)

| # | Tiêu đề | Người | Checklist |
|---|---|---|---|
| 12 | Sơ đồ kiến trúc 3 lớp | An | draw.io, ảnh vào `docs/` |
| 13 | Chốt bảng API | An | `docs/API.md` đối chiếu mục 3.6; cả nhóm duyệt các mục [ĐỀ XUẤT] |
| 14 | Dựng khung ASP.NET Core, middleware lỗi | An | Xong (#1); thêm quy ước code vào README |
| 15 | ERD, Entity, DbContext | Tài | Bảng theo mục 3.5; cài EF Core + Pomelo (bản tương thích EF Core 9) |
| 16 | Migration đầu, seed 3 địa điểm mẫu | Tài | Seed cả tài khoản admin và danh sách `languages` |
| 17 | CI khung | Tài | restore, build, format (`.github/workflows/ci.yml`) |
| 18 | Sequence diagram luồng dịch rồi đọc | Bảo | Dùng cho chương 4 |
| 19 | Thiết kế bảng jobs, trạng thái, retry | Bảo | pending, processing, done, failed; index `(status, run_after)` |
| 20 | Viết ITranslationProvider, ITtsProvider | Bảo | Interface trong `Providers/` |

## Tuần 3: Auth, Place, provider dịch

| # | Tiêu đề | Người | Checklist |
|---|---|---|---|
| 21 | Auth: đăng ký, đăng nhập, JWT, BCrypt | An | `/auth/register`, `/auth/login`, `/auth/me`; `[Authorize(Roles=...)]`; JWT key trong user-secrets |
| 22 | CRUD Place + phân quyền chủ địa điểm | An | `/places`; staff chỉ sửa địa điểm của mình |
| 23 | Test Auth | Tài | Viết cùng An |
| 24 | CI thêm xUnit và coverage | Tài | `--collect:"XPlat Code Coverage"` |
| 25 | Dockerfile nhiều giai đoạn | Tài | |
| 26 | CRUD Language | Bảo | `/languages`; chỉ admin sửa |
| 27 | LibreTranslateProvider | Bảo | Timeout, xử lý lỗi |
| 28 | Unit test provider bằng mock | Bảo | |

## Tuần 4: Dịch được 1 địa điểm sang nhiều ngôn ngữ

| # | Tiêu đề | Người | Checklist |
|---|---|---|---|
| 29 | API lấy địa điểm theo ngôn ngữ | An | `?lang=`, chưa có thì quay về tiếng Việt, trả `translationStatus` |
| 30 | Tìm kiếm theo tên, danh mục | An | `keyword`, `category`, phân trang |
| 31 | TextChunker chia đoạn | Bảo | Trong `Utils/` |
| 32 | TranslationService | Bảo | Dịch, lưu, `source_hash` cache, retry |
| 33 | Endpoint tạo job dịch | Bảo | `POST /places/{id}/translate` trả 202 |
| 34 | CD lên staging | Tài | Build image, đẩy GHCR, SSH, healthcheck; cần có server (xem rủi ro) |
| 35 | Integration test Place với MySQL thật | Tài | WebApplicationFactory |

## Tuần 5: End-to-end chạy trọn (MỐC 2)

| # | Tiêu đề | Người | Checklist |
|---|---|---|---|
| 36 | API phát audio | An | `GET /places/{id}/audio?lang=`, stream mp3, Range |
| 37 | Hoàn thiện Swagger | An | Mô tả, ví dụ, nút Authorize JWT |
| 38 | (Tùy chọn) QR, yêu thích | An | Làm nếu dư thời gian |
| 39 | EdgeTtsProvider, TtsService, lưu mp3 | Bảo | Lưu đường dẫn vào `audio_files` |
| 40 | JobWorker (SKIP LOCKED), khôi phục job kẹt | Bảo | Job `processing` quá 10 phút về `pending` |
| 41 | API tra cứu job | Bảo | `/jobs/{id}`, `/jobs?status=`, `/jobs/{id}/retry` |
| 42 | Quản lý .env và GitHub Secrets | Tài | Không commit secret |
| 43 | Log bằng Serilog | Tài | |
| 44 | Test dịch, TTS bằng mock; test worker khi job lỗi | Tài | Cả hai worker song song |

## Tuần 6: Tích hợp, đo thực nghiệm

| # | Tiêu đề | Người | Checklist |
|---|---|---|---|
| 45 | Tích hợp end-to-end, kiểm tra phân quyền, sửa lỗi | An | |
| 46 | Nháp chương 1, 3, kiến trúc, API | An | |
| 47 | Giới hạn job song song, tối ưu cache | Bảo | |
| 48 | Thực nghiệm E1 (chất lượng dịch), E3 (cache) | Bảo | E1: 20-30 câu, chấm bởi ít nhất 2 người |
| 49 | Nháp chương 2 (dịch, TTS) | Bảo | |
| 50 | Deploy production, thử rollback | Tài | Chụp ảnh làm minh chứng |
| 51 | Test tải E4, thực nghiệm E2, chốt coverage | Tài | |
| 52 | Nháp chương 6, 7 | Tài | |
| 53 | Kiểm thử thủ công kịch bản demo | Cả nhóm | Postman, từ đầu đến cuối |

## Tuần 7: Báo cáo, demo, nộp bài (không thêm tính năng)

| # | Tiêu đề | Người | Checklist |
|---|---|---|---|
| 54 | Tổng hợp báo cáo, viết chương 8 | An | |
| 55 | Đóng gói nộp bài | An | Theo checklist mục 8.4 |
| 56 | Dẫn thuyết trình, tập ít nhất 2 lần | An | |
| 57 | Slide 12-15 trang | Bảo | |
| 58 | Kịch bản demo, dữ liệu mẫu 5 địa điểm dịch sẵn | Bảo | |
| 59 | Môi trường demo dự phòng, video quay màn hình | Tài | Local và online |
| 60 | Kiểm tra lần cuối, tag v1.0 | Tài | |

## Tùy chọn (bỏ nếu thiếu thời gian)

| # | Tiêu đề | Người |
|---|---|---|
| 61 | E5: so sánh giọng đọc, khảo sát 5-10 người | Bảo |
