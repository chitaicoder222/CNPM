# Local Setup, SQL Server and Cloudflare Guide

> **Document ID:** DOC-OPS-001 | **Purpose:** Cài đặt local có thể lặp lại, giữ secret an toàn và chuẩn bị tunnel thủ công.  
> **Version / Status:** 0.7.0 / ISOLATED INITIALIZER, LOCAL HOST, PROTECTED SESSION AND QUICK TUNNEL VERIFIED | **Owner:** Operations Engineer
> **Last updated:** 2026-10-28 | **Related:** [Security](../06_security/SECURITY.md), [Database](../03_database/DATABASE_DESIGN.md)

## Prerequisites

Install .NET SDK 9, SQL Server Express/Developer or an approved SQL Server instance, and optionally sqlcmd for diagnostics. At audit time, this machine has .NET 9 but no discovered host SQL command-line tooling. Docker Desktop now runs the local VIVOICE SQL Server container, with healthy status, loopback port `127.0.0.1:14333` and a successful container-local authenticated connection. No migration, seed, database reset or application startup has occurred against the existing VIVOICE database; later verification uses the distinct topology documented below.

## Expected local configuration

Create User Secrets for `src/Vivoice.Web` with: `ConnectionStrings:Vivoice`, `BootstrapAdmin:DisplayName`, `BootstrapAdmin:Phone` and `BootstrapAdmin:Password`. The application and its EF Core design-time factory use the same precedence: base JSON, optional environment JSON, Development User Secrets, then process environment variables. `ConnectionStrings__Vivoice` is the environment-variable equivalent. The factory fails explicitly if the connection string is absent and has no LocalDB fallback. Use safe placeholders in tracked `appsettings*.json`; never paste real values into a command history, README, SQL file or commit.

`Authentication:CookieSecret` is not an application setting and must not be created. Cookie and protected visitor-session material use ASP.NET Core Data Protection keys, not a hand-rolled cookie secret. Default local key storage is suitable only for local development. Before a multi-instance or durable production deployment, configure one protected, persistent shared Data Protection key ring with least-privilege access and encryption at rest; that production design is not implemented by this local setup.

For the current local Docker SQL Server, choose the intended VIVOICE database name explicitly in the private connection string and target `127.0.0.1:14333`. Do not place that application connection string in Docker `.env`; that file is reserved for the SQL Server container password. Normal `dotnet run` does not migrate, seed, or bootstrap an Admin.

## Controlled database initialization

The database initializer is an explicit operator command, never a normal web-startup side effect. From the repository root, run `dotnet run --project src/Vivoice.Web -- --initialize-database` to apply pending migrations and seed the non-sensitive reference content. Add `--bootstrap-admin` only when a first Admin must be created: `dotnet run --project src/Vivoice.Web -- --initialize-database --bootstrap-admin`.

The initializer runs migration, then content seed, then optional Admin bootstrap. It stops at the first failed phase and exits non-zero. It does not start the web host. Run it once at a time with the privileged development/deployment database identity. A normal web process should use an identity without DDL permission. Migration, content seed and bootstrap are separate durable phases: a successful migration is not rolled back if a later phase fails. Content seed uses one database transaction for the rate and all initial POIs/topics; a failed content seed rolls back that phase only. Bootstrap uses its own insert/save operation after content seed succeeds.

If no Admin exists, `--bootstrap-admin` requires all three private BootstrapAdmin configuration keys and fails without creating a partial account when any key is absent. If any Admin already exists, the command makes no password or account change. Content seed creates missing baseline data only and does not overwrite POIs, topics, translations, or rates already managed by an Admin.

After initialization, run `dotnet run --project src/Vivoice.Web`. `/health` is a generic liveness endpoint. `/ready` returns success only when the database can be reached and no EF migrations are pending; neither endpoint returns connection or schema details. If an operation fails, stop and inspect protected configuration; never delete an existing database just to retry a migration.

## Isolated integration verification

Use a separate container, named volume, loopback port and disposable database for integration work. The verified 2026-10-28 topology used container `vivoice-sqlserver-integration-20261028`, volume `vivoice_sqlserver_integration_20261028`, host bind `127.0.0.1:15433` and database `VivoiceIntegration20261028`. It never reused port 14333, `vivoice-sqlserver-dev`, `vivoice_sqlserver_data` or the existing `Vivoice` database.

Before an isolated test, inspect names and port ownership, then create a new uniquely named resource only if all targets are absent. Use a temporary, untracked test login and temporary bootstrap values in the process environment. Test the explicit initializer, schema/counts, idempotent rerun and restoration of a deliberately absent baseline category without changing any existing content. Do not delete an existing integration database automatically; retain it until the owner confirms cleanup. For a production-like database, first make and verify a restorable backup, then use a privileged initializer identity once and a non-DDL web identity afterward.

## Cloudflare Quick Tunnel demo

After local `/health` and `/ready` are verified, start a temporary tunnel from the repository root with `cloudflared tunnel --no-autoupdate --url http://127.0.0.1:<web-port>`. Expose only the web port; never expose SQL Server, Docker port 14333 or the isolated SQL test port. Cloudflared prints a transient `trycloudflare.com` URL. Verify that URL returns HTTP 200 for `/` and a public POI API request, and HTTP 401 for an anonymous Admin request. The 2026-10-28 Quick Tunnel passed those checks with CSP and HSTS headers.

Quick Tunnels are transient, unauthenticated demos with no uptime guarantee. Keep the terminal process alive only while demonstrating; stop it with `Ctrl+C` when done. Do not treat its URL as a production deployment, a stable public address or a substitute for named-tunnel access control. The 2026-10-28 HTTP smoke test verified Secure/HTTP-only visitor and Admin cookies plus CSRF login/dashboard through HTTPS; browser visual rendering and device TTS audio still require separate manual verification.

### One-click launcher

The tracked launcher is `tools/Start-VIVOICE-Demo.bat`, which invokes `tools/Start-VIVOICE-Demo.ps1`. It defaults to loopback port `5078`, sets `ASPNETCORE_ENVIRONMENT` and `DOTNET_ENVIRONMENT` to `Development`, starts the normal web host only when that port is free, and waits for `/ready` before it considers the host usable. It never runs the initializer, Docker Compose, a migration or a seed command.

The launcher keeps its own process IDs, origin and Quick Tunnel URL in the ignored `.vivoice-demo/demo-state.json` file. It can reuse a previous tunnel only when that state identifies a live `cloudflared` process with the expected loopback origin and all three HTTP probes still pass: `/` 200, public POI API 200 and anonymous dashboard 401. It never kills an arbitrary `cloudflared` process. If another unowned process already targets the same origin, the launcher fails safely instead of creating a duplicate. `Start-VIVOICE-Demo.bat -Stop` stops only state-file processes whose executable and command line still match launcher ownership; it never manages Docker or a database.

The launcher is safe to commit because it contains no connection string or credential. It relies on existing Development User Secrets. Before distributing a link, confirm that `/ready` succeeds and that the launcher reports all public/admin probes as successful. A Quick Tunnel link is intentionally temporary and must not be presented as a production deployment.

## Docker local development

The tracked `docker-compose.yml` and `.env.example` express the reviewed topology: a digest-pinned SQL Server Developer image, a VIVOICE-only named volume and the loopback bind `127.0.0.1:14333`. The container is running healthy, and the named volume exists. The template has exactly one placeholder, `MSSQL_SA_PASSWORD`; legacy MySQL, JWT and Admin environment names are intentionally absent because VIVOICE does not use them. `.env` is ignored and holds the private SQL Server password; do not put it in User Secrets commands, documents, source or terminal output. Admin bootstrap values stay in User Secrets or private process environment using the `BootstrapAdmin` configuration keys, never in Docker `.env`. See [Docker SQL Server Runbook](DOCKER_SQL_RUNBOOK.md) for the verified runtime evidence, secret handling, and the approval point before migration.
