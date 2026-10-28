# Security, Authentication and Privacy Requirements

> **Document ID:** DOC-SEC-001 | **Purpose:** Bảo vệ admin, secrets, inputs, database và telemetry.  
> **Version / Status:** 0.5.0 / ISOLATED RUNTIME, PROTECTED SESSION AND TUNNEL COOKIE VERIFIED | **Owner:** Security Engineer
> **Last updated:** 2026-10-28 | **Related:** [API](../04_api/API_SPECIFICATION.md), [Database](../03_database/DATABASE_DESIGN.md), [Local Setup](../09_operations/LOCAL_SETUP.md)

## Controls

- Bootstrap admin values are read from User Secrets/environment only; password is hashed with ASP.NET Core `PasswordHasher` and never logged, returned or committed.
- Login requires normalized display name + phone + password. Failed login returns a generic error; admin authorization is enforced server-side on every `/api/admin/*` route.
- Prefer secure HTTP-only, SameSite auth cookies for the one-host deployment; do not place a password or long-lived admin token in `localStorage`.
- All query/write data uses EF Core parameterization, DTO allowlists and server validation. Public image/advertisement target URLs must use `https`; strings are output encoded.
- SQL Server is private; the connection string and bootstrap Admin credentials stay in User Secrets/environment. Cookie and protected visitor-session material use ASP.NET Core Data Protection keys rather than an `Authentication:CookieSecret` setting. `appsettings.Development.json` has placeholders only.
- Session analytics uses a server-issued, protected, HTTP-only same-origin cookie. Its `Secure` flag follows the request scheme after trusted loopback forwarded-header processing, so HTTPS tunnel requests receive a Secure cookie without making local loopback development unusable. The server hashes the embedded anonymous identifier, records coarse device category and stops counting inactive sessions as active based on `LastSeenAtUtc`; it does not claim automatic row deletion. No fingerprinting, exact location or raw IP retention is required.
- Write actions record admin ID, action, entity and safe summary to `AuditLogs`. Audit cannot be queried publicly.
- Before any cookie-authenticated state change, the same-origin client obtains an antiforgery request token from the host and sends it in the configured request header. Admin APIs reject missing/invalid tokens.
- Login and public telemetry use bounded server-side rate limits. Limits are abuse controls, not a substitute for authentication or business validation.
- Sponsorship commitment updates and payment creation now use serializable database transactions. This is the intended protection against a commitment accepted below concurrently recorded payments; an isolated controller-level concurrency probe is still pending before it is treated as runtime-verified. A deadlock or lock failure must not be represented as a successful financial write.
- The tunnel-facing host accepts forwarded protocol/client headers only from loopback Cloudflare Tunnel connections, applies generic security response headers, and never exposes a database port through the tunnel.

## Runtime verification evidence

The disposable isolated SQL Server database verified migration, five-language content seed, idempotent rerun, missing-topic restoration, generic health/readiness, anonymous Admin denial, antiforgery token/login/dashboard/logout flow, missing-token HTTP 400 and login-policy HTTP 429. The visitor-session smoke test rejected no-cookie, arbitrary client-body and tampered-cookie heartbeats with HTTP 400; a valid protected cookie recorded the first heartbeat and not the second. The Cloudflare Quick Tunnel independently verified public-page/API HTTP 200, secure-cookie Admin CSRF/login/dashboard HTTP 200, anonymous Admin HTTP 401, HTTPS-only media HTTP 400, CSP and HSTS response headers. No secret, production credential, existing VIVOICE database, volume or container was read or changed.

The test did not automate a browser visual pass, mobile device TTS, a non-loopback header-forgery scenario, or a distributed abuse test. Those remain explicit limits rather than passing claims.

## Audit findings requiring remediation

- Visitor-session issuance remains rate-limited but is not a fraud-proof financial control: automated callers can still request fresh cookies up to the policy limit. Add abuse heuristics/CAPTCHA or separate verified accounting before production revenue claims.
- Demonstrate no accidental forwarded-header trust from a non-loopback caller.
- Persistent Data Protection key handling remains a deployment decision; a temporary Quick Tunnel demo intentionally does not claim durable session continuity across host restarts.
- Sponsor, advertisement and revenue-rate write/audit pairs are not all one database transaction. Agreement commitment updates and payment creation are serializable, but broader financial production workflows still need concurrency/failure integration coverage.

These findings are classified and prioritized in [Implementation Audit](../11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md). They must be corrected and integration-tested before production or named-tunnel exposure.
