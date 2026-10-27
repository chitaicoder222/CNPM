# Local Setup, SQL Server and Cloudflare Guide

> **Document ID:** DOC-OPS-001 | **Purpose:** Cài đặt local có thể lặp lại, giữ secret an toàn và chuẩn bị tunnel thủ công.  
> **Version / Status:** 0.5.0 / SQL CONTAINER HEALTHY — CONNECTION CONFIGURATION ALIGNMENT APPLIED | **Owner:** Operations Engineer  
> **Last updated:** 2026-10-26 | **Related:** [Security](../06_security/SECURITY.md), [Database](../03_database/DATABASE_DESIGN.md)

## Prerequisites

Install .NET SDK 9, SQL Server Express/Developer or an approved SQL Server instance, and optionally sqlcmd for diagnostics. At audit time, this machine has .NET 9 but no discovered host SQL command-line tooling. Docker Desktop now runs the isolated VIVOICE SQL Server container, with healthy status, loopback port `127.0.0.1:14333` and a successful container-local authenticated connection. No VIVOICE migration, seed, database reset or application startup has occurred.

## Expected local configuration

Create User Secrets for `src/Vivoice.Web` with: `ConnectionStrings:Vivoice`, `BootstrapAdmin:DisplayName`, `BootstrapAdmin:Phone`, `BootstrapAdmin:Password`, and `Authentication:CookieSecret`. The application and its EF Core design-time factory use the same precedence: base JSON, optional environment JSON, Development User Secrets, then process environment variables. `ConnectionStrings__Vivoice` is the environment-variable equivalent. The factory fails explicitly if the connection string is absent and has no LocalDB fallback. Use safe placeholders in tracked `appsettings*.json`; never paste real values into a command history, README, SQL file or commit.

For the current local Docker SQL Server, choose the intended VIVOICE database name explicitly in the private connection string and target `127.0.0.1:14333`. Do not place that application connection string in Docker `.env`; that file is reserved for the SQL Server container password. Do not run `dotnet run` merely to test a connection: the current startup path automatically runs migrations and seed operations.

After source exists: run `dotnet restore`, `dotnet ef database update --project src/Vivoice.Web`, then `dotnet run --project src/Vivoice.Web`. Confirm health/API, public page and protected admin page. If an operation fails, stop and inspect configuration; never delete an existing database just to retry a migration.

## Cloudflare Tunnel (manual future operation)

Only after local health/security verification, the owner may run a manually authenticated `cloudflared tunnel --url http://localhost:<port>`. Expose only the application HTTP port, never SQL Server. Verify HTTPS/public URL and admin authentication, then stop the process after demo. No token, DNS setting or tunnel is created by this project.

## Docker local development

The tracked `docker-compose.yml` and `.env.example` express the reviewed topology: a digest-pinned SQL Server Developer image, a VIVOICE-only named volume and the loopback bind `127.0.0.1:14333`. The container is running healthy, and the named volume exists. The template has exactly one placeholder, `MSSQL_SA_PASSWORD`; legacy MySQL, JWT and Admin environment names are intentionally absent because VIVOICE does not use them. `.env` is ignored and holds the private SQL Server password; do not put it in User Secrets commands, documents, source or terminal output. Admin bootstrap values stay in User Secrets or private process environment using the `BootstrapAdmin` configuration keys, never in Docker `.env`. See [Docker SQL Server Runbook](DOCKER_SQL_RUNBOOK.md) for the verified runtime evidence, secret handling, and the approval point before migration.
