# Product Requirements Document — VIVOICE

> **Document ID:** DOC-REQ-001 | **Purpose:** Nguồn yêu cầu sản phẩm chính thức cho Public Website và Admin Website.  
> **Version / Status:** 0.3.0 / UI CORRECTION AND LOCAL SQL PLAN BASELINED | **Owner:** Product Owner  
> **Last updated:** 2026-10-26 | **Related:** [Requirements](REQUIREMENTS.md), [Use Cases](USE_CASES.md), [Traceability](../08_traceability/TRACEABILITY.md)

## Vision and scope

VIVOICE — Vietnam Voice là website giới thiệu văn hóa, đặc sản và điểm đến Việt Nam bằng năm ngôn ngữ: Việt (`vi`), Hàn (`ko`), Nhật (`ja`), Trung (`zh`) và Thái (`th`). Khách dùng trang public không đăng nhập. Họ chọn marker, POI card hoặc topic; khi chọn topic, văn bản hiện ra và browser tự thử đọc nội dung, không có nút riêng tên “Thuyết minh”.

Admin quản lý POI/nội dung/ngôn ngữ/ads/sponsor, theo dõi analytics và doanh thu. Không có quản lý tài khoản khách public. Mọi nội dung và KPI được đọc từ SQL Server, không hardcode frontend.

## Product constraints

- Initial seed has six active POI; admin may add/hide/reorder without frontend code change.
- Mỗi POI có ba category: `culture`, `food` và `tourism`; category thiếu nội dung sẽ bị ẩn kèm empty-state phù hợp.
- Hai ad rails chỉ xuất hiện trên desktop rộng; mobile ưu tiên map, cards và narration.
- Default traffic estimate is USD 0.10 per valid visit; rate changes are effective-dated and historical visits keep their applied rate.
- Admin sign-in requires display name, phone and password. Bootstrap secret is supplied only by User Secrets/environment variables and is never stored in source, SQL script or document.
- The application is local-only unless the owner manually follows the Cloudflare guide after verification.
- Public header contains the VIVOICE logo and global language selector only. It must not expose an Admin navigation link. Admin remains available only at the existing direct route, admin.html, and its APIs remain protected by server-side authentication and authorization.
- Vietnamese public category labels are: Văn hóa đặc sắc, Ẩm thực nổi tiếng, and Điểm du lịch nổi bật. Their stable data values remain culture, food, and tourism.
- The public UI is served by the ASP.NET Core host and calls relative /api/public routes on the same origin. Opening the HTML from a file path or unrelated static server is unsupported and must show a truthful API-unavailable state rather than fixture data.
- Docker Desktop preflight is complete. A VIVOICE-owned SQL Server development container is planned but not yet created, started, or connected; see the Docker SQL Server Runbook.

## Acceptance summary

Public is accepted when real POI/topic/translation data appear on map/cards, topic selection triggers a truthful TTS attempt, language and responsive states work, and ads are configuration-driven. Admin is accepted when protected APIs supply real SQL-backed dashboard/POI/content/ad/sponsor data. Database/security acceptance requires SQL Server migration, hashed password, protected admin routes and no secret in tracked files.

## Current implementation boundary

The current source has public/admin flows, two generated migrations, and five narrow passing unit tests, but it is **not yet an accepted deployment**. SQL Server is unverified, so real POIs, login, dashboard, CRUD persistence, map, TTS and responsive-browser behavior remain unverified. The initial seed contains Vietnamese content only; five-language UI and translation tables support a documented fallback until reviewed translations are entered by an admin. The detailed evidence, defects, and next sequence are maintained in [Implementation Audit](../11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md).
