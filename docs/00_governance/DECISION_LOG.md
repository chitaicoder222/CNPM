# Decision Log

> **Document ID:** DOC-GOV-002 | **Purpose:** Lưu các quyết định có ảnh hưởng code, data và vận hành.  
> **Version / Status:** 0.1.0 / CURRENT | **Owner:** Product Owner  
> **Last updated:** 2026-10-26 | **Related:** [PRD](../01_requirements/PRD.md), [Architecture](../02_architecture/ARCHITECTURE.md)

| ID | Decision | Status | Consequence |
|---|---|---|---|
| ADR-001 | Use ASP.NET Core .NET 9 modular monolith, one host for public, admin and API. | CONFIRMED BY BASELINE | Fewest moving parts; easy file-level control. |
| ADR-002 | Use EF Core SQL Server for the authoritative store. | CONFIRMED BY BASELINE | Live migration/integration waits for a configured SQL Server instance. |
| ADR-003 | Use browser Web Speech API as the first TTS adapter. | CONFIRMED BY BASELINE | Topic selection triggers the attempt; no paid TTS service or false playback state. |
| ADR-004 | Use Leaflet + OpenStreetMap tiles with required attribution for local/demo use. | PROPOSED | Re-check provider policy before public launch. |
| ADR-005 | Valid visit = one anonymous browser session’s first accepted visit; session heartbeat every 30 seconds, active for 90 seconds. | PROVISIONAL | Rate and active-device KPI are deterministic but require Product Owner confirmation before production. |
| ADR-006 | `zh` uses a generic UI code and `zh-CN` for preferred browser voice matching when available. | PROVISIONAL | Product Owner may later choose simplified/traditional content policy. |
