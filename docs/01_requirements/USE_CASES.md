# Use Case Specification

> **Document ID:** DOC-REQ-003 | **Purpose:** Xác định actor, trigger và luồng nghiệp vụ chính.  
> **Version / Status:** 0.3.0 / REDESIGN RECONCILED — END-TO-END NOT VERIFIED | **Owner:** Product Owner<br>
> **Last updated:** 2026-10-28 | **Related:** [PRD](PRD.md), [Activity Design](../02_architecture/ARCHITECTURE.md)

| ID | Actor | Trigger | Success outcome | Exception/fallback |
|---|---|---|---|---|
| UC-PUB-001 | Guest | Opens page, selects map marker/card/topic | Selected narration is displayed in current language; after a user gesture, TTS is attempted in the actual narration language and default-on autoplay can continue through up to three topics. | Missing translation/voice leaves readable text plus status; stale callbacks cannot advance an old selection. |
| UC-PUB-002 | Guest | Changes language | UI labels/content update while selected POI remains when available. | Explicit fallback/unavailable message. |
| UC-PUB-003 | Guest browser | Loads public page and sends heartbeat | Anonymous non-bot public session/visit is recorded under dedupe policy. | Analytics failure never blocks public content; Admin/internal/bot requests are ignored. |
| UC-ADM-001 | Admin | Submits name, phone, password | Secure session/token is issued and audit event recorded. | Generic error, throttled failed attempts. |
| UC-ADM-002 | Admin | Opens dashboard/filter/refresh | SQL-backed KPI for a valid date range is rendered and dashboard-only regions refresh periodically while visible. | Empty data displays zero/empty state, never sample data; refresh error is shown without replacing an editor form. |
| UC-ADM-003 | Admin | Edits POI/topic/translation | Validation, audit and persisted content; public sees it on refresh/poll. | Error preserved and explained. |
| UC-ADM-004 | Admin | Manages rates/ads/sponsors | Future rate or scheduled business records are saved without historical corruption, and affected dashboard values refresh after API success. | Overlap/invalid URL/state denied. |
| UC-OPS-001 | Demo operator | Starts public-only Quick Tunnel launcher | Public homepage and public API are available; Admin surface is unavailable from the demo host. | If readiness or route probes fail, no demo URL is reported as successful. |

All success outcomes above remain acceptance targets, not claims of successful execution. Source-level security controls and automated tests do not substitute for isolated API, browser, audio and tunnel verification; see [Traceability](../08_traceability/TRACEABILITY.md).
