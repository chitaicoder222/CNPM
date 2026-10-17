# TourGuide Narration

He thong thuyet minh tu dong da ngon ngu cho dia diem du lich (ASP.NET Core .NET 9).

Thanh vien: An, Bao, Tai.

## Cau truc

```
src/TourGuide.Api/
  Controllers/   Nhan request, tra response (khong chua nghiep vu)
  Services/      Nghiep vu
  Repositories/  Truy cap database
  Providers/     Goi dich vu ngoai (dich, TTS)
  Workers/       Tac vu chay nen
  Data/Entities/ Entity va DbContext
  Dtos/          Request/response models
  Middlewares/   Xu ly loi chung
  Utils/         Ham dung chung, AppException
tests/TourGuide.Tests/
  Unit/  Integration/
docs/            So do kiem truc, bang API
```

Luong mot chieu: Controller -> Service -> Repository / Provider.

## Chay lan dau

```
git clone <link-repo>
cd <ten-repo>  # repo hien tai tren GitHub: CNPM
copy .env.example .env
dotnet restore
dotnet user-secrets init --project src/TourGuide.Api
dotnet user-secrets set "ConnectionStrings:Default" "server=localhost;port=3306;database=tourguide;user=tourguide;password=<MYSQL_PASSWORD>;SslMode=None" --project src/TourGuide.Api
dotnet user-secrets set "Jwt:Secret" "<random-secret-at-least-32-characters>" --project src/TourGuide.Api
dotnet build
dotnet run --project src/TourGuide.Api
```

Nếu MySQL local yêu cầu TLS tắt trong môi trường phát triển, thêm `;SslMode=None`
vào connection string user-secrets (không commit chuỗi này). Mở
`http://localhost:5130/` để xem trang demo hoặc `http://localhost:5130/swagger` để xem API.

Docker Compose chạy API ở cổng `8080`; trong container, LibreTranslate dùng
`http://libretranslate:5000/` và TTS cần Python cùng package `edge-tts`.

MySQL volume note: `MYSQL_PASSWORD` is used when the volume is initialized. If `db_data` already exists, it must match the password already configured for the database user; Compose does not change that password on restart. Do not delete the volume to work around a mismatch.

Tai lieu: `PROPOSAL.md`, `docs/API.md`, `docs/ARCHITECTURE.md`, `docs/DEMO_CHECKLIST.md`, `docs/MYSQL_VOLUME.md`, `docs/PROVISIONING.md`, `docs/CD_STAGING.md`, `docs/E1_E4_RUNBOOK.md`.

## Quy uoc lam viec

- Khong commit thang vao `main`. Nhanh tich hop la `develop`.
- Moi viec lam tren nhanh `feature/ten-viec`, roi tao Pull Request vao `develop`.
- Commit theo kieu: `Auth: them dang nhap`.
- Moi Pull Request can nguoi khac review. Ghi `Closes #so-issue` de gan voi Issue.
