# Product Requirements Document — VIVOICE

> **Document ID:** DOC-REQ-001 | **Purpose:** Nguồn yêu cầu sản phẩm chính thức cho Public Website và Admin Website.  
> **Version / Status:** 0.4.0 / REDESIGN IMPLEMENTED AND STATICALLY VALIDATED; BROWSER AND ISOLATED-DB RECHECK PENDING | **Owner:** Product Owner<br>
> **Last updated:** 2026-10-28 | **Related:** [Requirements](REQUIREMENTS.md), [Use Cases](USE_CASES.md), [Traceability](../08_traceability/TRACEABILITY.md)

## Vision and scope

VIVOICE — Vietnam Voice là website giới thiệu văn hóa, đặc sản và điểm đến Việt Nam bằng năm ngôn ngữ: Việt (`vi`), Hàn (`ko`), Nhật (`ja`), Trung (`zh`) và Thái (`th`). Khách dùng trang public không đăng nhập. Họ chọn marker, POI card hoặc topic; khi chọn topic, văn bản hiện ra và browser tự thử đọc nội dung, không có nút riêng tên “Thuyết minh”. Giao diện public dùng hình ảnh địa điểm do dự án quản lý, có cờ trong bộ chọn ngôn ngữ, và luôn chọn TTS theo ngôn ngữ thực tế mà API trả về cho narration — không theo riêng ngôn ngữ giao diện.

Admin quản lý POI/nội dung/ngôn ngữ/ads/sponsor, theo dõi analytics và doanh thu. Không có quản lý tài khoản khách public. Mọi nội dung và KPI được đọc từ SQL Server, không hardcode frontend.

## Product constraints

- Initial seed has six active POI; admin may add/hide/reorder without frontend code change.
- Mỗi POI có ba category: `culture`, `food` và `tourism`; category thiếu nội dung sẽ bị ẩn kèm empty-state phù hợp.
- Hai ad rails chỉ xuất hiện trên desktop rộng; mobile ưu tiên map, cards và narration.
- Hero và sáu card POI dùng asset cục bộ có alt text và visual fallback; nếu ảnh quảng cáo hoặc ảnh POI không tải được, giao diện không được hiện biểu tượng ảnh hỏng hay URL thô.
- Tự động phát mặc định bật, nhưng không được tự phát âm thanh khi lần đầu tải trang. Sau thao tác người dùng đầu tiên, nó chỉ đọc tuần tự tối đa ba chủ đề của POI đang chọn; đổi POI/topic/ngôn ngữ hoặc tắt công tắc phải vô hiệu callback speech cũ.
- Default traffic estimate is USD 0.10 per valid visit; rate changes are effective-dated and historical visits keep their applied rate.
- Một lượt/thiết bị analytics chỉ là phiên khách public, không phải danh tính thật. Admin, API quản trị, health/readiness và bot có thể nhận diện không được tạo visitor telemetry. Active-device TTL được cấu hình thống nhất mặc định là 300 giây.
- Admin sign-in requires display name, phone and password. Bootstrap secret is supplied only by User Secrets/environment variables and is never stored in source, SQL script or document.
- The application is local-only unless the owner manually follows the Cloudflare guide after verification.
- Quick Tunnel demo phải khởi động host `VivoiceDemo:PublicOnly=true`: public routes còn hoạt động nhưng `/admin`, `/admin.html`, `/admin.js` và `/api/admin` trả 404. Chế độ này không dựa vào địa chỉ IP do proxy cung cấp.
- Public header contains the VIVOICE logo and global language selector only. It must not expose an Admin navigation link. Admin remains available only at the existing direct route, admin.html, and its APIs remain protected by server-side authentication and authorization.
- Vietnamese public category labels are: Văn hóa đặc sắc, Ẩm thực nổi tiếng, and Điểm du lịch nổi bật. Their stable data values remain culture, food, and tourism.
- The public UI is served by the ASP.NET Core host and calls relative /api/public routes on the same origin. Opening the HTML from a file path or unrelated static server is unsupported and must show a truthful API-unavailable state rather than fixture data.
- Docker Desktop preflight is complete. A VIVOICE-owned SQL Server development container is planned but not yet created, started, or connected; see the Docker SQL Server Runbook.

## Acceptance summary

Public is accepted when real POI/topic/translation data appear on map/cards, topic selection triggers a truthful TTS attempt in the actual narration language, autoplay follows the defined three-topic rule after a user gesture, language and responsive states work, and ads are configuration-driven. Admin is accepted when protected APIs supply real SQL-backed dashboard/POI/content/ad/sponsor data, definitions of visitor metrics are visible, and refresh updates dashboard-only regions without clobbering forms. Database/security acceptance requires SQL Server migration, hashed password, protected admin routes, no secret in tracked files and a successful public-only tunnel check before any link is shared.

## Current implementation boundary

The current source has public/admin flows, two generated migrations, a controlled initializer, 54 passing .NET unit tests and 16 passing browser-independent Node tests. Baseline source now defines 6 POIs × 3 topics × 5 language records; all 18 Vietnamese narrations pass a static 350–520-word target, while the four non-Vietnamese reference versions require qualified native-language editorial review before production. Seed remains add-only and will not overwrite Admin-edited content, so existing deployed records are not silently upgraded. The redesigned browser, real speech output, responsive map tiles, fresh isolated-database coverage and public tunnel remain unverified in this implementation pass. See [Content Coverage](../11_reports/CONTENT_COVERAGE_2026-10-28.md), [Content Source Register](../11_reports/CONTENT_SOURCE_REGISTER.md), [Asset Manifest](../11_reports/ASSET_MANIFEST.md) and [Traceability](../08_traceability/TRACEABILITY.md).
