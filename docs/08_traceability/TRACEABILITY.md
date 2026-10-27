# Requirements Traceability Matrix

> **Document ID:** DOC-TRACE-001 | **Purpose:** Liên kết yêu cầu đến design, data, API, test và file code thực tế.  
> **Version / Status:** 0.7.0 / SQL RUNTIME VERIFIED; CONNECTION-SOURCE ALIGNMENT STATICALLY VERIFIED | **Owner:** Project Maintainer  
> **Last updated:** 2026-10-26 | **Related:** [Requirements](../01_requirements/REQUIREMENTS.md), [Code Map](../10_development/CODE_MAP.md), [Implementation Audit](../11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md)

Status labels mean: VERIFIED has direct executed evidence; IMPLEMENTED BUT NOT VERIFIED has source but lacks relevant runtime proof; PARTIALLY IMPLEMENTED has a documented gap; BLOCKED depends on unavailable environment.

| Requirement | API / data | Existing evidence | Code location | Audit status |
|---|---|---|---|---|
| FR-PUB-001/002 public map and POIs | API-PUB-001; Pois/PoiTranslations | Build and JS syntax only; TC-PUB-001/002 NOT RUN | PublicController.cs, PublicContentService.cs, public.js, index.html, DatabaseSeeder.cs | IMPLEMENTED BUT NOT VERIFIED |
| FR-PUB-003 topic categories | API-PUB-002; PoiTopics/TopicTranslations | TC-PUB-002 NOT RUN | PublicContentService.cs, AdminController.cs, public.js | IMPLEMENTED BUT NOT VERIFIED |
| FR-PUB-004 automatic narration | API-PUB-003; TopicTranslations | TC-PUB-003 NOT RUN | public.js, PublicContentService.cs | PARTIALLY IMPLEMENTED — browser/TTS unverified |
| FR-PUB-005 five languages | API-PUB-001..003; translation tables | TC-PUB-004 NOT RUN | public.js, AdminController.cs | PARTIALLY IMPLEMENTED — Vietnamese seed only |
| FR-PUB-006 ads | API-PUB-004/006; Advertisements/AdvertisementEvents | TC-PUB-005 NOT RUN | PublicController.cs, AdminController.cs, public.js | IMPLEMENTED BUT NOT VERIFIED |
| FR-ADM-001 authentication | API-ADM-001; AdminUsers/AuditLogs | 2 normalizer assertions; TC-ADM-001 NOT RUN | AdminAuthController.cs, BootstrapAdminService.cs, Program.cs | PARTIALLY IMPLEMENTED — security findings open |
| FR-ADM-002 dashboard/devices | API-ADM-002; visits/sessions/rates/agreements/payments/events | 3 device category assertions; TC-ADM-002/003 NOT RUN | AnalyticsService.cs, AdminController.cs, admin.js | PARTIALLY IMPLEMENTED |
| FR-ADM-003 content management | API-ADM-003..005; POI/topic translations/AuditLogs | TC-ADM-004 NOT RUN | AdminController.cs, admin.js | IMPLEMENTED BUT NOT VERIFIED |
| FR-ADM-004 commerce/rates | API-ADM-006..008; ads/sponsors/agreements/payments/rates | TC-ADM-005 NOT RUN | AdminController.cs, AnalyticsService.cs, admin.js | PARTIALLY IMPLEMENTED — payment integrity open |
| FR-DATA-001 request-time data freshness | SQL request-time queries | SQL integration NOT RUN | PublicContentService.cs, AnalyticsService.cs | BLOCKED |
| NFR-SEC-001 | cookie/auth/hash/validation/audit | static audit plus five narrow unit tests | Program.cs, controllers, services | PARTIALLY IMPLEMENTED |
| NFR-UX-001 | Design tokens and responsive CSS | static palette check only | site.css, index.html, admin.html | PARTIALLY IMPLEMENTED |
| NFR-MAINT-001 | headers/docs/code map | static scan; JSON exception documented | docs/, source headers | PARTIALLY IMPLEMENTED |
| UI font/encoding and public navigation | Design System typography/public rules | UTF-8 static check, JavaScript syntax and no-Admin-link scan PASS; browser visual test NOT RUN | site.css, index.html, public.js, admin.html, DatabaseSeeder.cs | IMPLEMENTED BUT NOT VERIFIED |
| Local Docker SQL environment | Docker SQL Server Runbook | TC-DKR-001 PASS; TC-DKR-002 PASS for container health, loopback TCP and container-local SQL query; migration NOT RUN | docker-compose.yml, .env.example, .gitignore | IMPLEMENTED AND VERIFIED for SQL runtime only |
| Runtime/design-time connection source | `ConnectionStrings:Vivoice`; no schema/API change | TC-CON-001 PASS for static source/build; no EF/database execution | Program.cs, VivoiceDbContextFactory.cs, User Secrets/environment | IMPLEMENTED BUT NOT VERIFIED — no private connection configured or migration run |

Do not promote a row based only on a page, controller, or migration. Update this matrix only when the specific evidence column gains a matching test or live verification record.
