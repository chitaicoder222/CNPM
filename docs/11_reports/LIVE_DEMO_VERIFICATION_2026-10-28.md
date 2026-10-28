# VIVOICE Live Demo Verification — 2026-10-28

> **Document ID:** DOC-REPORT-003 | **Purpose:** Record only the executed isolated-runtime, security and public-demo evidence for this delivery.
>
> **Status:** VERIFIED WITH EXPLICIT BROWSER LIMITS | **Scope:** VIVOICE feature branch; no existing VIVOICE database, volume or primary container was changed.

## Demo endpoint

Temporary Cloudflare Quick Tunnel at verification time: [https://assure-guide-roman-telecom.trycloudflare.com](https://assure-guide-roman-telecom.trycloudflare.com)

The address was verified during the recorded run and is inherently transient. The local host/tunnel were stopped when the test session was interrupted, so this report does **not** claim that the URL is currently live. It was a demo link, not a production deployment.

## Isolation boundary

| Resource | Verified value | Boundary |
|---|---|---|
| Test container | `vivoice-sqlserver-integration-20261028` | Separate from `vivoice-sqlserver-dev` |
| Test volume | `vivoice_sqlserver_integration_20261028` | Separate named volume |
| Test host bind | `127.0.0.1:15433` | Separate from local port 14333 |
| Test database | `VivoiceIntegration20261028` | Separate from existing `Vivoice` database |
| Web host | `http://127.0.0.1:5088` | Loopback only; tunnel forwards only web traffic |

No secret, connection string, password, token or cookie value is included in this report.

## Executed results

| Check | Result | Evidence |
|---|---|---|
| Release build | PASS | 0 warnings, 0 errors |
| .NET unit tests | PASS | 37/37 |
| Frontend Node tests | PASS | 10/10 |
| JavaScript syntax | PASS | `public.js`, `admin.js`, `tts-language.js` |
| Whitespace check | PASS | `git diff --check` completed without violations |
| EF migrations + initialization | PASS | 2 existing migrations applied to the isolated database only |
| Five-language baseline | PASS | 6 POIs, 30 POI translations, 18 topics, 90 topic translations |
| Add-only seed | PASS | Edited Korean translation survived rerun; an intentionally absent baseline topic restored from 17 to 18 without overwriting existing records |
| Existing database boundary | PASS | Read-only primary-container query found the pre-existing `Vivoice` database with 2 migration rows and 6 POIs; no initializer, migration, seed or write operation targeted it |
| Host readiness | PASS | Local `/health` and `/ready` returned HTTP 200 after initialization |
| Public language API | PASS | `vi`, `ko`, `ja`, `zh`, `th` each returned 6 POIs, 3 topics and matching narration language metadata with no baseline fallback |
| Admin protection | PASS | Anonymous dashboard returned HTTP 401; CSRF/token/login/dashboard flow returned expected HTTP 200 results |
| Admin content API CRUD | PASS | Isolated API created a POI, Korean translations and one culture topic; duplicate normalized culture category returned HTTP 409; deactivation removed it from public output |
| Visitor-session integrity | PASS | No-cookie, arbitrary client-body and tampered-cookie heartbeat requests returned HTTP 400; a valid server-issued cookie recorded one visit and a repeat did not duplicate it |
| Visitor-session retry policy | PASS | Node tests verify an in-flight issue call is shared, a transient failure can retry later, and a success is cached |
| HTTPS media policy | PASS | HTTP image/target URL probes returned HTTP 400 before persistence |
| Tunnel public surface | PASS | Home and public API returned HTTP 200; anonymous Admin route returned HTTP 401 |
| Tunnel cookies | PASS | Visitor and Admin cookies were observed as Secure and HTTP-only; tunnel CSRF/login/dashboard succeeded |

## What is still not proven

- Browser-rendered Leaflet marker/card synchronization, tile-error recovery and responsive layout.
- Actual audible browser/device TTS, available-voice behavior and mobile gesture restrictions.
- Browser-rendered Admin CRUD UI for POIs, topics, translations, advertisements, sponsorships, payment/rate history and dashboard aggregation. The isolated POI/topic/translation API path is verified, but the Admin UI is not.
- Four concurrent phone sessions rendered in the Admin UI.
- Controller-level financial concurrency under simultaneous agreement reduction and payment writes. The source uses serializable transactions, but the final live probe was interrupted by the session boundary and is not marked verified.
- Abuse resistance beyond the configured per-IP rate limit, including distributed automated issuance of new visitor cookies.
- Production Data Protection key storage, non-DDL runtime database permissions, monitoring, stable named tunnel and production deployment hardening.
- Visual QA and synchronized update of the corresponding Word deliverables are blocked in this environment: no approved DOCX render/edit session or compliant renderer is currently available. The Markdown documents are the current reviewed source of truth; do not regenerate Word files without a render-and-inspect workflow.

The dashboard's traffic amount is an estimate for the demo; it is not fraud-proof financial accounting.

## Operator hand-off

1. Open the temporary tunnel link while the local demo processes are alive.
2. For a persistent deployment, create a protected named Cloudflare Tunnel and configure production secrets/key storage outside Git.
3. Perform the browser/manual cases above before representing the product as production-ready.
4. Keep the isolated test container and volume until their owner explicitly approves cleanup; do not run destructive Docker cleanup commands against shared resources.
