# Bảng API – TourGuide Narration

Nguồn: mục 3.6 của file kế hoạch. Mục nào **không có** trong kế hoạch mà mình đề xuất thêm thì đánh dấu **[ĐỀ XUẤT]** để cả nhóm quyết định giữ hay bỏ.

Local: `http://localhost:5080`, Swagger: `/swagger`.

## 1. Quy ước chung

**Response** (mọi API):

```json
{ "success": true,  "data": { }, "error": null }
{ "success": false, "data": null, "error": { "code": "NOT_FOUND", "message": "Không tìm thấy địa điểm." } }
```

**Xác thực:** header `Authorization: Bearer <JWT>`.

**Vai trò:** `admin`, `staff`, `tourist` (đúng giá trị cột `users.role` ở mục 3.5).
Cột Quyền: Công khai = không cần đăng nhập; Đăng nhập = mọi vai trò.

**Phân trang** (API danh sách): `?page=1&pageSize=20`, trả `{ "items": [], "page": 1, "pageSize": 20, "total": 0 }`.

**Ngôn ngữ:** `lang` là `languages.code` (`vi`, `en`, `zh`, `ja`, `ko`). Mặc định `vi`. Chưa có bản dịch hoặc audio thì xem mục 4.

## 2. Mã lỗi

| HTTP | Code | Khi nào |
|---|---|---|
| 400 | `BAD_REQUEST` | Dữ liệu vào sai hoặc thiếu (kèm danh sách trường lỗi trong `message`) |
| 401 | `UNAUTHORIZED` | Chưa đăng nhập, token sai/hết hạn, sai mật khẩu |
| 403 | `FORBIDDEN` | Không đủ quyền, hoặc staff sửa địa điểm của người khác |
| 404 | `NOT_FOUND` | Không có tài nguyên |
| 409 | `CONFLICT` | Trùng dữ liệu (email, mã ngôn ngữ) |
| 429 | `TOO_MANY_REQUESTS` | Vượt giới hạn tần suất (rate limiting, mục 3.8) |
| 500 | `INTERNAL_ERROR` | Lỗi hệ thống |

## 3. Danh sách endpoint

| Nhóm | Method và đường dẫn | Quyền | Người làm |
|---|---|---|---|
| **Auth** | `POST /auth/register` | Công khai | An |
| | `POST /auth/login` | Công khai | An |
| | `GET /auth/me` | Đăng nhập | An |
| **Place** | `GET /places?keyword=&category=&page=&pageSize=` | Mọi người | An |
| | `GET /places/{id}?lang=en` | Mọi người | An |
| | `POST /places` | staff, admin | An |
| | `PUT /places/{id}` | staff chủ địa điểm, admin | An |
| | `DELETE /places/{id}` | staff chủ địa điểm, admin | An |
| **Audio** | `GET /places/{id}/audio?lang=en` (stream mp3) | Mọi người | An |
| **Translation** | `POST /places/{id}/translate` (body: danh sách ngôn ngữ) | staff chủ địa điểm, admin | Bảo |
| | `PUT /places/{id}/translations/{lang}` **[ĐỀ XUẤT]** sửa tay bản dịch | staff chủ địa điểm, admin | Bảo |
| **Job** | `GET /jobs/{id}` | staff (job của mình), admin | Bảo |
| | `GET /jobs?status=failed` | staff (job của mình), admin | Bảo |
| | `POST /jobs/{id}/retry` | admin | Bảo |
| **Language** | `GET /languages` | Mọi người | Bảo |
| | `POST /languages`, `PUT /languages/{id}` | admin | Bảo |
| **Favorite (tùy chọn)** | `POST /places/{id}/favorite` | tourist đã đăng nhập | An |
| | `DELETE /places/{id}/favorite` **[ĐỀ XUẤT]** | tourist đã đăng nhập | An |
| | `GET /me/favorites` | tourist đã đăng nhập | An |
| **Hệ thống** | `GET /health` | Công khai | Tài |
| | `GET /swagger` | Công khai | Tài (An hoàn thiện mô tả) |
| **User (admin)** | `GET /users`, `PUT /users/{id}/role` **[ĐỀ XUẤT]** | admin | An |

### Các chỗ kế hoạch còn thiếu (cần nhóm chốt trước Mốc 1)

1. **Tạo tài khoản staff bằng cách nào?** `register` chỉ nên tạo `tourist`. Mục 1.2 nói Admin "quản lý người dùng" nhưng 3.6 không có API nào cho việc này. Mình đề xuất `PUT /users/{id}/role` và tài khoản admin đầu tiên tạo bằng seed.
2. **Sửa tay bản dịch:** mục 1.2 cho staff "sửa tay bản dịch chưa đúng" và mục 3.7 dựa vào đó để giảm lỗi tên riêng, nhưng 3.6 không có endpoint. Cần thêm.
3. **Bỏ yêu thích:** chỉ có `POST` và `GET`, thiếu cách bỏ.

## 4. Hành vi quan trọng

- **Dịch chạy nền (mục 3.2):** `POST /places/{id}/translate` **không** chờ dịch xong. Trả `202 Accepted` kèm danh sách job, mỗi ngôn ngữ một job `translate` (pending). Worker dịch xong tự tạo job `tts`.
- **Cache (mục 3.3):** nội dung không đổi (`source_hash` giống) thì không tạo job mới; trả lại bản dịch và audio cũ.
- **Lấy địa điểm khi chưa có bản dịch:** `GET /places/{id}?lang=ja` trả tiếng Việt, kèm `translationStatus` là `done`, `processing` (có job đang chạy) hoặc `none`.
- **Phát audio:** `audio/mpeg`, hỗ trợ `Range`. Chưa có audio thì `404 NOT_FOUND`.
- **Phân quyền sở hữu:** staff chỉ sửa, xóa, dịch địa điểm có `places.created_by` là mình (mục 3.8); admin làm được tất cả.

## 5. Ví dụ

### POST /auth/register

```json
// Request
{ "fullName": "Nguyen Van A", "email": "a@example.com", "password": "Matkhau@123" }

// 200 (role luôn là tourist)
{ "success": true, "data": { "id": 1, "fullName": "Nguyen Van A", "email": "a@example.com", "role": "tourist" }, "error": null }

// 409
{ "success": false, "data": null, "error": { "code": "CONFLICT", "message": "Email đã được đăng ký." } }
```

### POST /auth/login

```json
// Request
{ "email": "a@example.com", "password": "Matkhau@123" }

// 200
{ "success": true, "data": { "token": "<jwt>", "expiresAt": "2026-10-15T08:00:00Z", "role": "tourist" }, "error": null }

// 401
{ "success": false, "data": null, "error": { "code": "UNAUTHORIZED", "message": "Email hoặc mật khẩu không đúng." } }
```

### GET /places?keyword=chùa&category=di-tích&page=1&pageSize=10

```json
{
  "success": true,
  "data": {
    "items": [ { "id": 5, "name": "Chùa Một Cột", "category": "di-tích", "address": "...", "imageUrl": "..." } ],
    "page": 1, "pageSize": 10, "total": 1
  },
  "error": null
}
```

### GET /places/5?lang=en

```json
// Đã có bản dịch và audio
{ "success": true, "data": { "id": 5, "language": "en", "name": "One Pillar Pagoda", "description": "...",
    "translationStatus": "done", "audioUrl": "/places/5/audio?lang=en" }, "error": null }

// Chưa dịch ngôn ngữ ja: quay về tiếng Việt
{ "success": true, "data": { "id": 5, "language": "vi", "name": "Chùa Một Cột", "description": "...",
    "translationStatus": "processing", "audioUrl": null }, "error": null }
```

### POST /places

```json
// Request (staff, admin)
{ "name": "Chùa Một Cột", "category": "di-tích", "address": "Ba Đình, Hà Nội",
  "latitude": 21.0359, "longitude": 105.8335, "description": "Mô tả tiếng Việt...", "imageUrl": null }

// 200 (created_by lấy từ JWT, không nhận từ client)
{ "success": true, "data": { "id": 5, "name": "Chùa Một Cột" }, "error": null }

// 403 (tourist)
{ "success": false, "data": null, "error": { "code": "FORBIDDEN", "message": "Bạn không có quyền thực hiện thao tác này." } }
```

### POST /places/5/translate

```json
// Request
{ "languages": ["en", "zh", "ja"] }

// 202
{ "success": true, "data": { "jobs": [ { "id": 11, "language": "en", "type": "translate", "status": "pending" },
                                       { "id": 12, "language": "zh", "type": "translate", "status": "pending" },
                                       { "id": 13, "language": "ja", "type": "translate", "status": "pending" } ] }, "error": null }

// 400 (ngôn ngữ chưa bật)
{ "success": false, "data": null, "error": { "code": "BAD_REQUEST", "message": "Ngôn ngữ 'ko' chưa được bật." } }
```

### GET /jobs/11

```json
{ "success": true, "data": { "id": 11, "type": "translate", "status": "failed", "retryCount": 3,
    "lastError": "LibreTranslate timeout", "runAfter": null, "startedAt": "2026-10-14T08:00:00Z" }, "error": null }
```

Trạng thái job: `pending`, `processing`, `done`, `failed` (mục 3.2).

### POST /jobs/11/retry (admin)

```json
{ "success": true, "data": { "id": 11, "status": "pending" }, "error": null }
```

### GET /languages

```json
{ "success": true, "data": [ { "id": 1, "code": "vi", "name": "Tiếng Việt", "ttsVoice": "vi-VN-HoaiMyNeural", "isActive": true },
                             { "id": 2, "code": "en", "name": "English", "ttsVoice": "en-US-AriaNeural", "isActive": true } ], "error": null }
```

> Tên giọng đọc chỉ là ví dụ minh họa. Giọng thật phải chọn sau khi Bảo thử Edge TTS ở tuần 1.

### GET /health

```json
{ "status": "Healthy", "database": "ok" }
```
