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
dotnet build
dotnet run --project src/TourGuide.Api
```

Mo `http://localhost:5080/swagger` de xem API.

Tai lieu: `PROPOSAL.md`, `docs/API.md` (bang API), `docs/ISSUES.md` (danh sach cong viec).

## Quy uoc lam viec

- Khong commit thang vao `main`. Nhanh tich hop la `develop`.
- Moi viec lam tren nhanh `feature/ten-viec`, roi tao Pull Request vao `develop`.
- Commit theo kieu: `Auth: them dang nhap`.
- Moi Pull Request can nguoi khac review. Ghi `Closes #so-issue` de gan voi Issue.
