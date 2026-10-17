# Admin/Staff provisioning

No administrator password is stored in source, migrations, Compose, or defaults. Provisioning is opt-in at startup:

```powershell
$env:ADMIN_EMAIL = 'admin@example.test'
$secure = Read-Host 'Admin password' -AsSecureString
$env:ADMIN_PASSWORD = [System.Net.NetworkCredential]::new('', $secure).Password
# Keep these process variables ephemeral; use a secret manager in real deployments.
$env:STAFF_EMAIL = 'staff@example.test'
$secure = Read-Host 'Staff password' -AsSecureString
$env:STAFF_PASSWORD = [System.Net.NetworkCredential]::new('', $secure).Password
dotnet run --project src/TourGuide.Api
```

The application hashes configured passwords with ASP.NET Core `PasswordHasher`, creates only missing accounts, and never logs the values. In CI/staging use GitHub Environment Secrets (`ADMIN_EMAIL`, `ADMIN_PASSWORD`, etc.); do not put them in `.env.example` or workflow YAML. Self-registration always creates `tourist` and cannot elevate privileges.
