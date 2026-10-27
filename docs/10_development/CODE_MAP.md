# Code Map and Implementation Status

> **Document ID:** DOC-DEV-001 | **Purpose:** Bản đồ file-level để người vận hành biết thay đổi requirement nằm ở đâu.  
> **Version / Status:** 0.7.0 / SQL RUNTIME VERIFIED; CONNECTION-SOURCE ALIGNMENT STATICALLY VERIFIED | **Owner:** Technical Lead  
> **Last updated:** 2026-10-26 | **Related:** [Traceability](../08_traceability/TRACEABILITY.md), [Implementation Audit](../11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md)

Source files begin with purpose/responsibility comments; JSON remains valid JSON without comments. The project file and JSON configuration are documented format exceptions. This map identifies where a controlled change must start; it does not declare runtime success.

| Change request | Primary files | Data/API impact | Current status |
|---|---|---|---|
| Startup, SQL configuration and design-time factory | src/Vivoice.Web/Program.cs; Data/VivoiceDbContextFactory.cs; appsettings files | SQL Server runtime and EF Core tooling | IMPLEMENTED BUT NOT VERIFIED — shared private configuration sources, explicit missing-config failure, no LocalDB fallback; no connection/migration run |
| Public map, POI cards and narration | wwwroot/index.html; public.js; site.css; PublicController.cs; PublicContentService.cs | API-PUB-001..003 | IMPLEMENTED BUT NOT VERIFIED |
| Add/change a POI | AdminController.cs; admin.js; ContentEntities.cs | Pois and PoiTranslations; API-ADM-003 | IMPLEMENTED BUT NOT VERIFIED |
| Add/change a topic or its TTS text | AdminController.cs; admin.js; PublicContentService.cs | PoiTopics and TopicTranslations; API-ADM-004/005 | IMPLEMENTED BUT NOT VERIFIED |
| Add/change a translation | AdminController.cs; admin.js; public.js | translation tables; API-ADM-005 | IMPLEMENTED BUT NOT VERIFIED — initial seed is Vietnamese |
| Ad rails and performance | public.js; PublicController.cs; AdminController.cs; ContentEntities.cs | Advertisements and AdvertisementEvents; API-PUB-004/006, API-ADM-006 | IMPLEMENTED BUT NOT VERIFIED |
| Active sessions and dashboard | AnalyticsService.cs; AdminController.cs; admin.js; OperationsEntities.cs | VisitorSessions, Visits; API-ADM-002 | PARTIALLY IMPLEMENTED — TTL/abuse controls open |
| Sponsors, contracts and payments | AdminController.cs; admin.js; OperationsEntities.cs | Sponsors, agreements, payments; API-ADM-007 | PARTIALLY IMPLEMENTED — financial integrity rules open |
| Per-visit rate history | AdminController.cs; AnalyticsService.cs; admin.js | VisitRevenueRates; API-ADM-008 | IMPLEMENTED BUT NOT VERIFIED |
| EF Core schema | VIVOICE DbContext; entities; Data/Migrations | InitialCreate plus AddOperationsManagement | BLOCKED — generated, never applied |
| Regression checks | tests/Vivoice.Web.Tests/Unit; Test Plan | 5 unit tests | IMPLEMENTED AND VERIFIED for narrow unit scope |
| Public font/copy/navigation correction | site.css; index.html; public.js; admin.html; DatabaseSeeder.cs | no API/schema change | IMPLEMENTED BUT NOT VERIFIED — static UTF-8, syntax and navigation checks pass; browser visual/API check remains blocked by SQL |
| Local Docker SQL Server | docker-compose.yml; .env.example; .gitignore; Docker SQL Server Runbook | SQL Server 2022 container and loopback connection | IMPLEMENTED AND VERIFIED — healthy VIVOICE-only container, loopback TCP and container-local SQL connection; no migration/seed |

For a new POI, use the protected admin flow after SQL verification, or edit DatabaseSeeder only for a new clean-development seed. Do not add POIs to JavaScript. For a new rate, create a later effective rate through the protected endpoint; do not edit historical visits or browser constants. Before changing a row above, update PRD/API/Database/Test Plan first, then change code, test, and return here plus Traceability.
