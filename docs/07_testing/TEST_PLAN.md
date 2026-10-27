# Test Strategy, Cases and Results

> **Document ID:** DOC-TEST-001 | **Purpose:** Xác định evidence cần có trước khi một tính năng được gọi VERIFIED.  
> **Version / Status:** 0.5.0 / SQL RUNTIME VERIFIED; CONNECTION-SOURCE ALIGNMENT STATICALLY VERIFIED | **Owner:** QA Engineer  
> **Last updated:** 2026-10-26 | **Related:** [Requirements](../01_requirements/REQUIREMENTS.md), [Traceability](../08_traceability/TRACEABILITY.md)

## Test layers

Current unit tests cover only normalization and three coarse device categories. Planned integration tests cover EF Core SQL Server migrations, API authorization, rate history, payment rules and dashboard aggregation. Browser/manual checks cover Leaflet marker-card sync, SpeechSynthesis fallback, five locales, responsive rails and admin forms. Security checks must cover anonymous admin API denial, invalid input/URL, rate limiting, CSRF and secret scan.

| ID | Scenario | Expected result | Status |
|---|---|---|---|
| TC-PUB-001 | SQL seed/API returns active POIs | Six initial markers/cards; inactive POI omitted. | NOT RUN |
| TC-PUB-002 | Marker/card/topic selection | Selected POI/topic is synchronized and content is current-language. | NOT RUN |
| TC-PUB-003 | Browser TTS success/failure | Prior speech cancels; text persists and fallback is honest. | NOT RUN |
| TC-PUB-004 | Locale switch | `vi/ko/ja/zh/th` labels/content and fallback work. | NOT RUN |
| TC-PUB-005 | Desktop/mobile ads | Valid schedule appears only at appropriate placement/viewport. | NOT RUN |
| TC-ADM-001 | Three-field login and protected route | Valid admin enters; bad/anonymous request is denied. | NOT RUN |
| TC-ADM-002 | Dashboard range/rate | Counts and rate-based estimate match seeded SQL rows. | NOT RUN |
| TC-ADM-003 | Four phone sessions | Dashboard displays four independent `Phone` sessions inside TTL. | NOT RUN |
| TC-ADM-004 | POI/topic/translation CRUD | Edit persists; public GET returns changed data. | NOT RUN |
| TC-ADM-005 | Ads/sponsors/rates | Validation rejects invalid URL, rate-order conflict, payment above commitment and payment on cancelled agreement. | NOT RUN |
| TC-SEC-001 | Secret and input controls | No real credential in source; unsafe input/auth bypass, rate limit and CSRF controls fail safely. | PARTIAL — generic test fixture and targeted static scan PASS; input/auth/rate-limit/CSRF runtime checks NOT RUN. |
| TC-UX-001 | Vietnamese font and encoding | Required Vietnamese strings render without missing glyphs, replacement characters, clipping or unexpected wrapping at desktop/mobile widths. | STATIC UTF-8 CHECK PASSED; browser visual check NOT RUN. |
| TC-PUB-006 | Public navigation and failed fetch | Header/footer contain no Admin link; public API succeeds when loaded from the ASP.NET Core origin and reports a truthful error when unavailable. | NOT RUN — backend blocked by SQL. |
| TC-DKR-001 | Docker SQL Server preflight | Docker CLI/daemon, WSL 2, virtualization and resource checks pass; existing unrelated containers remain untouched. | PASS — preflight only. |
| TC-DKR-002 | VIVOICE SQL container readiness | New pinned image container becomes healthy, data volume persists, port binds only loopback, and no secret appears in logs. | NOT RUN — container requires explicit approval before creation. |
| TC-CON-001 | Runtime/design-time connection source alignment | Runtime and EF factory accept the same private `ConnectionStrings:Vivoice` providers, and missing configuration fails without LocalDB fallback. | PASS — static source assertions and build; no secret/configuration execution or database operation. |

## Results

Executed evidence on 2026-10-26: build passed with 0 warnings/errors; test passed 5/5; Node syntax checks passed for public.js and admin.js. UTF-8 strict decoding passed for 27 application source files, and a targeted scan found no public Admin link, deprecated decorative font declaration, or real bootstrap fixture in source/test. Docker Desktop preflight and Compose secret-resolution checks passed; the isolated SQL container is running healthy with loopback TCP and a container-local authenticated SQL connection verified. TC-CON-001 static assertions confirm JSON, Development User Secrets and environment-variable providers, explicit missing-config failure, and removal of LocalDB fallback; the web host and EF commands were not run. No EF migration, seed, application start, browser/API integration or admin-cookie test ran. See [Implementation Audit](../11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md) for the evidence boundary and missing test coverage.
