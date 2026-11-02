# API Specification and Error Contract

> **Document ID:** DOC-API-001 | **Purpose:** HTTP contracts for public data, protected admin operations and consistent errors.  
> **Version / Status:** 0.4.0 / ISOLATED RUNTIME VERIFIED; SERVER ISSUED VISITOR SESSION CONTRACT | **Owner:** Backend Engineer
> **Last updated:** 2026-10-28 | **Related:** [Database](../03_database/DATABASE_DESIGN.md), [Security](../06_security/SECURITY.md), [Traceability](../08_traceability/TRACEABILITY.md)

All routes start with /api. JSON is UTF-8 and timestamps are UTC ISO-8601. Public reads return only active/public content. Admin routes require the authenticated admin role. Error bodies are safe JSON with at least code and message; they never expose exception stacks, secrets, or password hashes.

| ID | Method and route | Access | Implemented purpose |
|---|---|---|---|
| API-PUB-001 | GET /public/pois?lang=vi | Public | Active map/card POIs with translation fallback. |
| API-PUB-002 | GET /public/pois/{id}/topics?lang=vi | Public | Active categories and published topic labels. |
| API-PUB-003 | GET /public/topics/{id}/narration?lang=vi | Public | Published text, language used and fallback state for browser TTS. |
| API-PUB-004 | GET /public/advertisements?placement=LEFT | Public | Scheduled, active left/right creative records. |
| API-PUB-005 | POST /public/sessions | Public visitor only | Issues an opaque, protected, HTTP-only same-origin visitor-session cookie; an authenticated Admin request is ignored and no visit is created. |
| API-PUB-006 | POST /public/sessions/heartbeat | Public visitor only | Requires the server-issued session cookie; the first non-bot public heartbeat may produce one valid estimated visit. Authenticated Admin and likely-bot telemetry is ignored without creating a visitor/session row. |
| API-PUB-007 | POST /public/advertisements/{id}/events | Public visitor only | Validates and records an impression or click only for an active advertisement with a valid server-issued public session; authenticated Admin telemetry is ignored. |
| API-ADM-001 | GET /admin/auth/csrf; POST /admin/auth/login, logout; GET me | Public/Admin | Same-origin antiforgery token, three-field login, cookie session, logout and state check. |
| API-ADM-002 | GET /admin/dashboard, GET /admin/devices/active | Admin | Date-filtered traffic/revenue/ad metrics and active session metadata. |
| API-ADM-003 | GET/POST /admin/pois, PATCH /admin/pois/{id} | Admin | POI list, create, edit, order, coordinates, image URL, hide/show. |
| API-ADM-004 | GET/POST /admin/pois/{id}/topics, PATCH /admin/topics/{id} | Admin | Topic create/edit, one `culture|food|tourism` category per POI, order and publish toggle. |
| API-ADM-005 | GET/PUT POI and topic translation routes | Admin | Upsert/read management for five permitted languages. |
| API-ADM-006 | GET/POST/PATCH advertisement routes | Admin | Creative, placement, schedule, sponsor link and active state. |
| API-ADM-007 | Sponsor, agreement and payment routes | Admin | Sponsor, contract value/status, received-payment history. |
| API-ADM-008 | GET/POST /admin/revenue-rates | Admin | Append-only USD visit-rate history; server closes prior rate interval. |

Write DTOs accept only documented fields. The server resolves the actor, normalizes language/place/status values, applies the same HTTPS-only external-media validator to POI images and advertisement image/target URLs, validates time ranges/amounts, applies rate history, and appends an audit log. Current money aggregation is explicitly USD only; no unsupported currency conversion is implied. Agreement commitment updates and payment creation now both use serializable database transactions; an isolated controller-level concurrency probe remains required before this behavior is promoted beyond implementation status. State-changing Admin requests, including login/logout, obtain a same-origin request token first and return 400 when that token is missing or invalid. Login and public telemetry may return 429 when their rate limit is exhausted. In `VivoiceDemo:PublicOnly=true` mode, `/admin` and `/api/admin` are deliberately unavailable, including authentication routes, so a Quick Tunnel cannot expose Admin operations. Typical status codes are 400 validation/antiforgery/session-required, 401 missing/invalid authentication, 403 role denial, 404 absent entity, 409 unique/rate-order conflict and 429 rate limit.

`GET /admin/dashboard` rejects an inverted `from`/`to` range. Its `activeWindowSeconds` response value is the same configured window used by `GET /admin/devices/active`; clients must display it as a session activity window, not a promise of real-time device identity.

## Metric definitions

- **Valid visit:** the first accepted public, non-bot heartbeat for a protected visitor-session cookie. It is not a dashboard page load, health check, Admin request or arbitrary API request.
- **Unique visitor:** distinct public visitor-session identifiers associated with valid visits in the selected date range. It is not a verified real-world identity.
- **Active device:** a public, non-bot visitor session whose most recent accepted heartbeat falls within the same configured active window in the dashboard and device list (300 seconds by default, bounded server-side to 30–900 seconds).
- **Traffic estimate:** sum of the visit-rate amount stored at each valid-visit event; it is an estimate, not fraud-proof accounting.
- **Advertisement impression/click:** an accepted event for an active advertisement tied to a valid protected public visitor session. Dashboard refreshes do not create advertisement events.

## Same-origin startup contract

The public client uses relative route paths such as /api/public/pois. ASP.NET Core serves both wwwroot and controllers from one origin through UseDefaultFiles, UseStaticFiles and MapControllers. A network-level Failed to fetch therefore means the browser could not reach the active application origin or that an endpoint failed before returning JSON; it is not sufficient evidence of CORS or SQL failure by itself.

On 2026-10-28, an isolated SQL Server database verified `/health`, `/ready`, public POI/topic/narration reads for all five languages, anonymous Admin denial, token-to-login flow, protected Admin dashboard access, missing-token rejection and rate-limit rejection. The visitor-session endpoint returned 204, while no-cookie, arbitrary-body and tampered-cookie heartbeats returned 400; the valid server-issued cookie recorded only its first heartbeat. A Cloudflare Quick Tunnel independently returned HTTP 200 for the public page/API and CSRF/login/dashboard, HTTP 401 for an anonymous Admin request, and Secure/HTTP-only visitor and Admin cookies. Browser visual and device-TTS behavior remain unverified.
