# Use Case Specification

> **Document ID:** DOC-REQ-003 | **Purpose:** Xác định actor, trigger và luồng nghiệp vụ chính.  
> **Version / Status:** 0.2.0 / RECONCILED — END-TO-END NOT VERIFIED | **Owner:** Product Owner  
> **Last updated:** 2026-10-26 | **Related:** [PRD](PRD.md), [Activity Design](../02_architecture/ARCHITECTURE.md)

| ID | Actor | Trigger | Success outcome | Exception/fallback |
|---|---|---|---|---|
| UC-PUB-001 | Guest | Opens page, selects map marker/card/topic | Selected narration is displayed in current language and TTS is attempted. | Missing translation/voice leaves readable text plus status. |
| UC-PUB-002 | Guest | Changes language | UI labels/content update while selected POI remains when available. | Explicit fallback/unavailable message. |
| UC-PUB-003 | Guest browser | Loads page and sends heartbeat | Anonymous session/visit is recorded under dedupe policy. | Analytics failure never blocks public content. |
| UC-ADM-001 | Admin | Submits name, phone, password | Secure session/token is issued and audit event recorded. | Generic error, throttled failed attempts. |
| UC-ADM-002 | Admin | Opens dashboard/filter | SQL-backed KPI for date range is rendered. | Empty data displays zero/empty state, never sample data. |
| UC-ADM-003 | Admin | Edits POI/topic/translation | Validation, audit and persisted content; public sees it on refresh/poll. | Error preserved and explained. |
| UC-ADM-004 | Admin | Manages rates/ads/sponsors | Future rate or scheduled business records are saved without historical corruption. | Overlap/invalid URL/state denied. |

All success outcomes above remain acceptance targets, not claims of successful execution. The audit found missing rate-limit/CSRF controls and payment-integrity rules; see [Implementation Audit](../11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md).
