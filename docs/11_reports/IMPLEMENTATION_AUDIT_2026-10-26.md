# Implementation Audit and Documentation Reconciliation

> **Document ID:** DOC-RPT-002 | **Purpose:** Đối chiếu yêu cầu VIVOICE với mã nguồn và bằng chứng kiểm thử thực tế trước khi triển khai tiếp.  
> **Version / Status:** 0.3.0 / SQL RUNTIME VERIFIED; CONNECTION FACTORY ALIGNMENT STATICALLY VERIFIED — NO MIGRATION | **Owner:** Technical Lead and QA Engineer  
> **Audit date:** 2026-10-26 | **Scope:** VIVOICE only; no access to Draft.

## Audit method and evidence boundary

> **Historical-audit note (2026-10-28):** This report preserves the original 2026-10-26 audit evidence. Later isolated-runtime work added five-language baseline translations, CSRF/rate limits, protected visitor cookies, HTTPS media validation, controlled initializer verification and TTS language tests. Read the current [Test Plan](../07_testing/TEST_PLAN.md), [Traceability](../08_traceability/TRACEABILITY.md) and [Live Demo Verification](LIVE_DEMO_VERIFICATION_2026-10-28.md) for current evidence. `Authentication:CookieSecret` was removed because it was never used; persistent Data Protection key-ring design remains open for production.

The audit read PRD, Requirements, Use Cases, Architecture, Database Design/ERD, API Specification, Security, Test Plan, Traceability Matrix, Code Map, application source, migrations, and the two unit-test files. No conclusion below treats a page, controller, service, or migration as live proof by itself.

Executed evidence:

| Check | Actual result | What it proves |
|---|---|---|
| dotnet build src/Vivoice.Web/Vivoice.Web.csproj --no-restore --configuration Debug | PASS; 0 warnings, 0 errors | Current C# source compiles. |
| dotnet test tests/Vivoice.Web.Tests/Vivoice.Web.Tests.csproj --no-restore --configuration Debug --verbosity minimal | PASS; 5 passed, 0 failed | Only normalization and coarse device-category unit cases exercised. |
| node --check public.js and admin.js | PASS | JavaScript parses; it is not browser/API evidence. |
| Static source/secret/UI scan after correction | PASS: strict UTF-8 decode for 27 application files; no public Admin link, deprecated decorative font declaration, or real bootstrap fixture in source/test. | Source encoding/navigation/font constraints and AUD-001 remediation are present; this is not browser rendering proof. |
| Docker Compose static validation | PASS: `docker compose config --no-interpolate`; loopback bind, digest-pinned image and named volume are declared. | The configuration parses without creating a container, volume, network, image pull or database. |
| Docker SQL runtime | PASS: the isolated VIVOICE container is running healthy; loopback TCP and a container-local authenticated SQL query succeed. | SQL Server is ready, but no VIVOICE application connection, EF migration, seed or web-host execution ran. |
| Design-time factory alignment | PASS: source now loads JSON, Development User Secrets and environment variables; static checks find no LocalDB fallback; `dotnet build` passes. | Static evidence only; no private connection string or EF command was executed. |
| SQL tooling discovery | sqlcmd and sqllocaldb are not found. | No local SQL command-line verification path is available. |

## Functional audit

| Area | Classification | Code/data evidence | Verification gap or finding |
|---|---|---|---|
| Build and five existing unit tests | IMPLEMENTED AND VERIFIED | Current build, IdentityNormalizerTests, DeviceClassifierTests | Scope is narrow; it does not prove database, HTTP, browser, or security integration. |
| Brand palette | IMPLEMENTED AND VERIFIED | site.css defines the five exact approved hex values; Design System matches | Static token check only; responsive visual QA still needs a running browser. |
| Admin authentication and authorization | PARTIALLY IMPLEMENTED | AdminAuthController uses name + phone + password, PasswordHasher, role claim; AdminController has an admin role requirement | Database/cookie flow is BLOCKED by SQL. Login throttling, CSRF protection, persistent data-protection key management, and use of the configured CookieSecret are absent. |
| POI management | IMPLEMENTED BUT NOT VERIFIED | AdminController creates/updates/lists POIs; handles coordinates, image URL, display order and IsActive; admin.js has search/edit UI | Requires SQL API integration. Display order and string length validation are incomplete. |
| Topic, narration and translations | IMPLEMENTED BUT NOT VERIFIED | Topic CRUD, hide/show, five-language translation upserts, public fallback selection, and browser SpeechSynthesis call are present | Requires SQL/browser verification. Seed content is Vietnamese only; UI fallback is explicit for narration but not consistently surfaced on map/card fallback. |
| Public map and automatic narration | PARTIALLY IMPLEMENTED | Leaflet/OSM code, markers/cards, topics and automatic TTS attempt exist in public.js | Application cannot start safely without a verified SQL configuration. Browser TTS and map/network behavior have not run. |
| UI font, Vietnamese copy, public navigation and unavailable API message | IMPLEMENTED BUT NOT VERIFIED | Local Arial/Segoe UI font stack, non-clipping headings, no public Admin link, canonical topic labels, and localized same-origin API-unavailable messages are in source | JavaScript/UTF-8/static scans pass. A browser visual test and actual same-origin API test require approved SQL startup. |
| Dashboard and statistics | PARTIALLY IMPLEMENTED | AnalyticsService calculates valid visits, unique session count, active categories, date buckets and revenue values; admin.js renders backend response | Requires SQL data. Date-range order is not validated. Unique visitors are actually unique VisitorSession values, not people. |
| Active sessions and device classification | PARTIALLY IMPLEMENTED | UUID session hash, heartbeat, Phone/Tablet/Desktop plus browser/OS classification, 90-second activity view | Device type unit cases pass. Endpoint uses fixed 90 seconds while dashboard reads configured ActiveSeconds, creating a TTL divergence. No live four-device scenario ran. |
| Advertisements | IMPLEMENTED BUT NOT VERIFIED | Scheduled left/right public reads; admin CRUD; AdvertisementEvents for impression/click; public.js emits events | SQL/browser evidence missing. Events have no rate limit or deduplication, so performance can be inflated by repeated client calls. |
| Sponsors, agreements, payments and revenue rate | PARTIALLY IMPLEMENTED | Protected sponsor/agreement/payment endpoints, rate-history endpoint, EF tables, dashboard summaries | SQL evidence missing. Payments are not capped against committed value and are allowed for cancelled agreements; money aggregation is USD-only by design. |
| EF Core, schema and migrations | IMPLEMENTED BUT NOT VERIFIED | DbContext plus InitialCreate and AddOperationsManagement migrations compile; the design-time factory now fails closed rather than selecting LocalDB | SQL Server is ready but no private application connection string, migration or schema inspection has been approved. |
| Validation, errors, audit log and privacy | PARTIALLY IMPLEMENTED | DTOs, URL/date/category checks, safe exception response, server-side audit calls, hashed anonymous session IDs | See AUD-001 through AUD-006. Audit is not transactionally coupled to each business write; input limits and abuse controls are incomplete. |
| Cloudflare exposure | NOT IMPLEMENTED | Local setup guide only | Correctly deferred until local SQL, auth, HTTPS and browser tests pass; no tunnel was created. |

## Defects and incomplete work

| ID | Severity | Finding | Evidence and safe next action |
|---|---|---|---|
| AUD-001 | P0 — RESOLVED (static) | A user-supplied bootstrap account identifier previously appeared in a unit test. | The test now uses generic non-real fixtures, and a targeted source/test scan finds no prohibited identifier. Runtime security tests remain separate. |
| AUD-002 | P1 | No login, heartbeat, or advertisement-event rate limiting is configured. | Program.cs has no rate-limiter registration; controllers do not apply policies. Add named policies and integration tests. |
| AUD-003 | P1 | Cookie-secret configuration is unused; Data Protection key persistence is not configured. | Program.cs reads no Authentication:CookieSecret and uses default key storage. Define an approved local key-protection approach before public deployment. |
| AUD-004 | P1 | State-changing cookie-authenticated admin API calls have no antiforgery/CSRF control. | POST, PATCH and PUT routes accept cookie authentication only. Add antiforgery strategy or another documented same-origin protection, then test it. |
| AUD-005 | P1 | Financial and audit integrity is incomplete. | Payments can exceed a commitment or apply to a cancelled agreement; writes and AuditAsync use separate SaveChanges calls. Add transactional business rules and tests. |
| AUD-006 | P2 | Active-session TTL differs by endpoint. | AnalyticsService uses configured ActiveSeconds; AdminController devices route hardcodes 90 seconds. Use one shared value and test it. |
| AUD-007 | P2 | Validation and semantic telemetry controls are incomplete. | Invalid dashboard date order, POI display order, maximum input length, ad-event duplication and session-abuse limits lack explicit server rules. Add targeted validation. |
| AUD-008 | P2 | Five language UI choices exist, but initial data only has Vietnamese content. | DatabaseSeeder creates Vietnamese translations; public fallback is expected, not a complete multilingual dataset. Add reviewed translations through the admin workflow and verify fallback messaging. |

## Environment blockers

1. The tracked connection string is empty. User Secrets/environment may contain private values, but they were not read because that could disclose secrets.
2. The VIVOICE Docker SQL Server is healthy and accepts a container-local authenticated connection. Host `sqlcmd` and LocalDB tooling are not installed or discoverable, but neither is needed for the verified container path.
3. No private application connection string has been configured for the web host; therefore Program.cs cannot be started without entering its automatic migration/seed path.
4. Leaflet/OSM and browser speech require a started application and a browser/network check. Neither ran.
5. Cloudflare remains intentionally blocked pending successful local health, authentication and HTTPS checks.

## Docker Desktop preflight update

Docker Desktop is installed and its daemon is available at version 29.8.1. WSL 2, virtualization, memory and disk checks are suitable for a local SQL Server development container. Existing unrelated containers, including an unrelated SQL Server container, remain out of scope. The digest-pinned VIVOICE Compose project now runs a healthy loopback-only SQL Server container with its VIVOICE-named volume; no migration, seed, reset, public exposure or Cloudflare tunnel was created. The design-time factory now uses private configuration sources and no longer falls back to LocalDB. The remaining approval gate is private application connection configuration followed by an explicit migration decision.

## PRD → API → database → test → code matrix

| PRD requirement | API / component | Database | Existing test evidence | Principal code location | Audit classification |
|---|---|---|---|---|---|
| FR-PUB-001/002 map and six POIs | API-PUB-001 | Pois, PoiTranslations | Build only; TC-PUB-001/002 NOT RUN | PublicController, PublicContentService, public.js, DatabaseSeeder | IMPLEMENTED BUT NOT VERIFIED |
| FR-PUB-003 topics | API-PUB-002 | PoiTopics, TopicTranslations | TC-PUB-002 NOT RUN | PublicContentService, AdminController, public.js | IMPLEMENTED BUT NOT VERIFIED |
| FR-PUB-004 automatic narration | API-PUB-003 | TopicTranslations | TC-PUB-003 NOT RUN | public.js SpeechSynthesis integration | PARTIALLY IMPLEMENTED |
| FR-PUB-005 five languages | API-PUB-001..003 | translation tables | TC-PUB-004 NOT RUN | public.js, AdminController translation routes | PARTIALLY IMPLEMENTED |
| FR-PUB-006 ads | API-PUB-004/006 | Advertisements, AdvertisementEvents | TC-PUB-005 NOT RUN | PublicController, AdminController, public.js | IMPLEMENTED BUT NOT VERIFIED |
| FR-ADM-001 login and protection | API-ADM-001 | AdminUsers, AuditLogs | Normalizer unit tests only; TC-ADM-001 NOT RUN | AdminAuthController, BootstrapAdminService, Program | PARTIALLY IMPLEMENTED |
| FR-ADM-002 dashboard/devices | API-ADM-002 | Visits, VisitorSessions, rates, agreements, payments, ad events | Device category unit tests only; TC-ADM-002/003 NOT RUN | AnalyticsService, AdminController, admin.js | PARTIALLY IMPLEMENTED |
| FR-ADM-003 POI/topic/translation | API-ADM-003..005 | POI/topic translation tables, AuditLogs | TC-ADM-004 NOT RUN | AdminController, admin.js | IMPLEMENTED BUT NOT VERIFIED |
| FR-ADM-004 commerce/rates | API-ADM-006..008 | ads, sponsors, agreements, payments, rates | TC-ADM-005 NOT RUN | AdminController, AnalyticsService, admin.js | PARTIALLY IMPLEMENTED |
| FR-DATA-001 reload from SQL | request-time service/controller queries | all runtime tables | SQL integration NOT RUN | PublicContentService, AnalyticsService | BLOCKED |
| NFR-SEC-001 | auth/error controls | hashes/audits | static audit plus 5 narrow unit tests | Program, controllers, services | PARTIALLY IMPLEMENTED |
| NFR-MAINT-001 | Documentation-first and file headers | migrations/docs | static scan; project/JSON format exceptions | docs, source headers | PARTIALLY IMPLEMENTED |
| NFR-UX-002 local font/UTF-8/public navigation | same-origin public frontend | no API/schema change | strict UTF-8, JavaScript syntax and static navigation scan PASS; browser test NOT RUN | site.css, index.html, public.js, admin.html, DatabaseSeeder.cs | IMPLEMENTED BUT NOT VERIFIED |
| NFR-OPS-001 isolated local Docker SQL configuration | no API until runtime approval | VIVOICE SQL Server named volume | Docker preflight, Compose validation, healthy container, loopback TCP and authenticated container-local SQL query PASS; migration NOT RUN | docker-compose.yml, .env.example, .gitignore, Docker SQL Runbook | IMPLEMENTED AND VERIFIED for SQL runtime |

## Priority-ordered implementation plan after audit approval

1. COMPLETE: replace the real bootstrap fixture with a generic fixture and rerun the targeted static scan.
2. COMPLETE (static/runtime boundary): create the isolated Docker SQL container/volume, verify SQL readiness, and replace the design-time LocalDB fallback with private configuration sources.
3. P1: configure the private application connection string, verify the intended target database without starting the web host, then obtain explicit approval to apply migrations.
4. P1: repair security/integrity gaps: rate limits, CSRF strategy, key protection, transactional auditing, payment/contract rules, and shared active-session TTL. Update PRD/API/ERD/Test Plan before each code change.
5. P1: add integration tests for auth denial/login, POI/topic/translation CRUD, rate ordering, payment limits, dashboard aggregation and ad-event controls.
6. P2: run browser tests for local font rendering, map/card synchronization, five-language fallback, TTS success/failure, responsive ad rails and active-device display.
7. P2: add reviewed non-Vietnamese content through the protected admin workflow, not frontend constants.
