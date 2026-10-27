# Functional and Non-functional Requirements

> **Document ID:** DOC-REQ-002 | **Purpose:** Stable requirement IDs and measurable acceptance statements.  
> **Version / Status:** 0.1.0 / BASELINED | **Owner:** Product Owner  
> **Last updated:** 2026-10-26 | **Related:** [PRD](PRD.md), [API](../04_api/API_SPECIFICATION.md), [Test Plan](../07_testing/TEST_PLAN.md)

| ID | Requirement | Acceptance evidence |
|---|---|---|
| FR-PUB-001 | Public page is open, map and active POIs are SQL/API-driven. | Public API and browser check. |
| FR-PUB-002 | Six initial POIs, marker/card selection synchronized. | Seed query and UI test. |
| FR-PUB-003 | POI exposes culture, food and tourism topics where content exists. | API/UI empty-state test. |
| FR-PUB-004 | Selecting a topic renders text and immediately attempts browser TTS with honest status. | Browser SpeechSynthesis test/fallback test. |
| FR-PUB-005 | UI/content supports `vi`, `ko`, `ja`, `zh`, `th` with documented fallback. | Translation API and language-switch test. |
| FR-PUB-006 | Active scheduled left/right ads are SQL/API-driven and responsive. | API/UI date and viewport test. |
| FR-ADM-001 | Admin login validates name, phone and password; server protects admin routes. | Auth API/security test. |
| FR-ADM-002 | Dashboard filters date range and shows visits, active device types and revenue classes. | Aggregation/API test. |
| FR-ADM-003 | Admin manages POIs, topics, translations and visibility without public-user management. | CRUD test. |
| FR-ADM-004 | Admin manages ads, sponsors, agreements, payments and visit-rate history. | CRUD/validation test. |
| FR-DATA-001 | Approved SQL data changes are returned on the next request; dashboard may poll, never use fake values. | SQL/API integration test. |
| NFR-SEC-001 | Passwords/secrets are protected; inputs and URLs are validated; admin writes are audited. | Static/security/API tests. |
| NFR-UX-001 | Palette, contrast, keyboard focus and responsive behavior meet documented design system. | Manual responsive/a11y evidence. |
| NFR-MAINT-001 | Every source file starts with a purpose comment; changes are traceable to docs/tests. | Code review and Code Map. |
