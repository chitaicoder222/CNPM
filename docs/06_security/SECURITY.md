# Security, Authentication and Privacy Requirements

> **Document ID:** DOC-SEC-001 | **Purpose:** Bảo vệ admin, secrets, inputs, database và telemetry.  
> **Version / Status:** 0.2.0 / AUDITED — REMEDIATION REQUIRED | **Owner:** Security Engineer  
> **Last updated:** 2026-10-26 | **Related:** [API](../04_api/API_SPECIFICATION.md), [Database](../03_database/DATABASE_DESIGN.md), [Local Setup](../09_operations/LOCAL_SETUP.md)

## Controls

- Bootstrap admin values are read from User Secrets/environment only; password is hashed with ASP.NET Core `PasswordHasher` and never logged, returned or committed.
- Login requires normalized display name + phone + password. Failed login returns a generic error; admin authorization is enforced server-side on every `/api/admin/*` route.
- Prefer secure HTTP-only, SameSite auth cookies for the one-host deployment; do not place a password or long-lived admin token in `localStorage`.
- All query/write data uses EF Core parameterization, DTO allowlists and server validation. Target URLs permit only `https`/`http`; image URLs and strings are output encoded.
- SQL Server is private; connection string/JWT/cookie secrets stay in User Secrets/environment. `appsettings.Development.json` has placeholders only.
- Session analytics uses random anonymous session ID stored by browser, hashes it server-side, records coarse device category and expires inactive sessions. No fingerprinting, exact location or raw IP retention is required.
- Write actions record admin ID, action, entity and safe summary to `AuditLogs`. Audit cannot be queried publicly.

## Known verification limit

No SQL Server instance or bootstrap secret is configured at baseline, so live authentication, migration, cookie settings and database authorization remain **NOT VERIFIED** until the setup guide is completed.

## Audit findings requiring remediation

- Login, heartbeat and advertisement-event rate limiting are not implemented despite being required for abuse resistance.
- State-changing cookie-authenticated admin endpoints have no antiforgery/CSRF strategy.
- Authentication:CookieSecret is a tracked placeholder but is not consumed by the current host; persistent Data Protection key handling is not configured.
- A prohibited bootstrap-account fixture was found in a unit test. The value is intentionally not repeated here; remove it before handoff.
- Audit writes occur after the business SaveChanges call, so the two records are not yet one database transaction.

These findings are classified and prioritized in [Implementation Audit](../11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md). They must be corrected and integration-tested before Cloudflare exposure.
