# API Specification and Error Contract

> **Document ID:** DOC-API-001 | **Purpose:** HTTP contracts for public data, protected admin operations and consistent errors.  
> **Version / Status:** 0.2.0 / IMPLEMENTED — SQL RUNTIME NOT VERIFIED | **Owner:** Backend Engineer  
> **Last updated:** 2026-10-26 | **Related:** [Database](../03_database/DATABASE_DESIGN.md), [Security](../06_security/SECURITY.md), [Traceability](../08_traceability/TRACEABILITY.md)

All routes start with /api. JSON is UTF-8 and timestamps are UTC ISO-8601. Public reads return only active/public content. Admin routes require the authenticated admin role. Error bodies are safe JSON with at least code and message; they never expose exception stacks, secrets, or password hashes.

| ID | Method and route | Access | Implemented purpose |
|---|---|---|---|
| API-PUB-001 | GET /public/pois?lang=vi | Public | Active map/card POIs with translation fallback. |
| API-PUB-002 | GET /public/pois/{id}/topics?lang=vi | Public | Active categories and published topic labels. |
| API-PUB-003 | GET /public/topics/{id}/narration?lang=vi | Public | Published text, language used and fallback state for browser TTS. |
| API-PUB-004 | GET /public/advertisements?placement=LEFT | Public | Scheduled, active left/right creative records. |
| API-PUB-005 | POST /public/sessions/heartbeat | Public | UUID-session heartbeat; first non-bot session produces one valid visit. |
| API-PUB-006 | POST /public/advertisements/{id}/events | Public | Validates and records an impression or click; optional session hash only. |
| API-ADM-001 | POST /admin/auth/login, logout, GET me | Public/Admin | Three-field login, cookie session, logout and state check. |
| API-ADM-002 | GET /admin/dashboard, GET /admin/devices/active | Admin | Date-filtered traffic/revenue/ad metrics and active session metadata. |
| API-ADM-003 | GET/POST /admin/pois, PATCH /admin/pois/{id} | Admin | POI list, create, edit, order, coordinates, image URL, hide/show. |
| API-ADM-004 | GET/POST /admin/pois/{id}/topics, PATCH /admin/topics/{id} | Admin | Topic create/edit, category, order and publish toggle. |
| API-ADM-005 | GET/PUT POI and topic translation routes | Admin | Upsert/read management for five permitted languages. |
| API-ADM-006 | GET/POST/PATCH advertisement routes | Admin | Creative, placement, schedule, sponsor link and active state. |
| API-ADM-007 | Sponsor, agreement and payment routes | Admin | Sponsor, contract value/status, received-payment history. |
| API-ADM-008 | GET/POST /admin/revenue-rates | Admin | Append-only USD visit-rate history; server closes prior rate interval. |

Write DTOs accept only documented fields. The server resolves the actor, normalizes language/place/status values, validates URLs/time ranges/amounts, applies rate history, and appends an audit log. Current money aggregation is explicitly USD only; no unsupported currency conversion is implied. Typical status codes are 400 validation, 401 missing/invalid authentication, 403 role denial, 404 absent entity, and 409 unique/rate-order conflict.

## Same-origin startup contract

The public client uses relative route paths such as /api/public/pois. ASP.NET Core serves both wwwroot and controllers from one origin through UseDefaultFiles, UseStaticFiles and MapControllers. A network-level Failed to fetch therefore means the browser could not reach the active application origin or that an endpoint failed before returning JSON; it is not sufficient evidence of CORS or SQL failure by itself. In the current environment, the backend cannot be safely started without an approved SQL connection string, so live health and POI endpoint tests remain blocked.
