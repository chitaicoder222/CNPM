# Functional and Non-functional Requirements

> **Document ID:** DOC-REQ-002 | **Purpose:** Stable requirement IDs and measurable acceptance statements.  
> **Version / Status:** 0.2.0 / REDESIGN REQUIREMENTS RECONCILED | **Owner:** Product Owner<br>
> **Last updated:** 2026-10-28 | **Related:** [PRD](PRD.md), [API](../04_api/API_SPECIFICATION.md), [Test Plan](../07_testing/TEST_PLAN.md)

| ID | Requirement | Acceptance evidence |
|---|---|---|
| FR-PUB-001 | Public page is open, map and active POIs are SQL/API-driven. | Public API and browser check. |
| FR-PUB-002 | Six initial API/database POIs, marker/card selection synchronized, each card has a managed image, accurate alt text and non-broken fallback. | Seed/API query, asset check and browser UI test. |
| FR-PUB-003 | POI exposes culture, food and tourism topics where content exists. | API/UI empty-state test. |
| FR-PUB-004 | Selecting a topic renders text and, after a user gesture, attempts browser TTS with honest status. TTS locale follows returned narration metadata, not UI language. | Unit mapping/sequencing tests and browser SpeechSynthesis/fallback test. |
| FR-PUB-005 | UI/content supports `vi`, `ko`, `ja`, `zh`, `th` with flags, translated labels and documented actual-language fallback. | Translation API, language-switch and fallback test. |
| FR-PUB-007 | Default-on autoplay reads no more than the three ordered topics for the selected POI, never starts audio on initial page load, and ignores stale speech callbacks. | State-machine test and browser audio test. |
| FR-PUB-006 | Active scheduled left/right ads are SQL/API-driven and responsive. | API/UI date and viewport test. |
| FR-ADM-001 | Admin login validates name, phone and password; server protects admin routes. | Auth API/security test. |
| FR-ADM-002 | Dashboard filters a valid date range, states metric definitions, excludes Admin/internal telemetry from public visitor metrics, and refreshes dashboard-only data on one bounded timer. | Policy/unit/API/browser-admin test. |
| FR-ADM-003 | Admin manages POIs, topics, translations and visibility without public-user management. | CRUD test. |
| FR-ADM-004 | Admin manages ads, sponsors, agreements, payments and visit-rate history. | CRUD/validation test. |
| FR-DATA-001 | Approved SQL data changes are returned on the next request; dashboard may poll, never use fake values. | SQL/API integration test. |
| NFR-SEC-001 | Passwords/secrets are protected; inputs and URLs are validated; admin writes are audited; a public demo host blocks all Admin surfaces independently of proxy IP. | Static/security/API/tunnel tests. |
| NFR-UX-001 | Palette, contrast, keyboard focus, managed imagery, image fallbacks and responsive behavior meet documented design system. | Static asset checks and manual responsive/a11y evidence. |
| NFR-MAINT-001 | Every source file starts with a purpose comment; changes are traceable to docs/tests. | Code review and Code Map. |
