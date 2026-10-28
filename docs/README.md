# VIVOICE Documentation Index

> **Document ID:** DOC-ROOT-001 | **Purpose:** Điểm bắt đầu cho tài liệu và quy tắc documentation-first của VIVOICE.  
> **Version / Status:** 0.1.0 / BASELINED | **Owner:** Project Maintainer  
> **Last updated:** 2026-10-26 | **Related:** [Repository Audit](00_governance/REPOSITORY_AUDIT.md), [PRD](01_requirements/PRD.md), [Code Map](10_development/CODE_MAP.md)

Tài liệu Markdown là nguồn kiểm soát có thể truy vết; thư mục `WORD_DELIVERABLES/` sẽ chứa bản Word cùng cấu trúc sau khi baseline được kiểm tra định dạng. Mọi thay đổi code phải cập nhật tài liệu có liên quan trước, sau đó chạy kiểm thử và cập nhật Code Map/Traceability.

| Nhóm | Tài liệu | Mục đích |
|---|---|---|
| Governance | [Audit](00_governance/REPOSITORY_AUDIT.md), [Decisions](00_governance/DECISION_LOG.md) | Hiện trạng và quyết định kiến trúc |
| Requirements | [PRD](01_requirements/PRD.md), [Requirements](01_requirements/REQUIREMENTS.md), [Use Cases](01_requirements/USE_CASES.md) | Scope và tiêu chí nhận |
| Architecture | [Architecture](02_architecture/ARCHITECTURE.md) | Component, activity, deployment |
| Data | [Database Design](03_database/DATABASE_DESIGN.md) | ERD, dictionary, seed/migration policy |
| API | [API Specification](04_api/API_SPECIFICATION.md) | Contract, auth, error response |
| UI/UX | [Design System](05_ui_ux/DESIGN_SYSTEM.md) | Palette và quy tắc public/admin |
| Security | [Security](06_security/SECURITY.md) | Auth, secrets, privacy |
| Quality | [Test Plan](07_testing/TEST_PLAN.md) | Strategy, cases, evidence |
| Traceability | [Traceability](08_traceability/TRACEABILITY.md) | Requirement to code/data/API/test |
| Operations | [Local Setup](09_operations/LOCAL_SETUP.md) | Local SQL Server và Cloudflare future runbook |
| Docker | [Docker SQL Server Runbook](09_operations/DOCKER_SQL_RUNBOOK.md) | Verified preflight, planned compose topology and approval gate |
| Development | [Code Map](10_development/CODE_MAP.md) | File-level control map |
| Reports | [Phase 2 Verification](11_reports/PHASE_2_VERIFICATION.md) | Build/test evidence and known limits |
| Live demo | [Live Demo Verification](11_reports/LIVE_DEMO_VERIFICATION_2026-10-28.md) | Isolated runtime, tunnel, security and explicit browser-limit evidence |
| Audit | [Implementation Audit](11_reports/IMPLEMENTATION_AUDIT_2026-10-26.md) | Requirement-by-requirement reconciliation, blockers and remediation priority |
