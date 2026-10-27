# Repository Audit Report

> **Document ID:** DOC-GOV-001 | **Purpose:** Bằng chứng Phase 0 trước khi tạo source code.  
> **Version / Status:** 0.1.0 / COMPLETE | **Owner:** Technical Lead  
> **Last updated:** 2026-10-26 | **Related:** [Decision Log](DECISION_LOG.md), [Architecture](../02_architecture/ARCHITECTURE.md)

## Scope

Audit chỉ thực hiện tại `CNPM/VIVOICE`. Thư mục `Draft` không được đọc, quét, copy hoặc dùng làm nguồn tham chiếu.

## Findings

| Check | Result | Consequence |
|---|---|---|
| Existing source/configuration | Empty workspace; no Git repository | New modular-monolith project can be created cleanly |
| .NET runtime | SDK 9.0.318 and 10.0.402 available | Use `net9.0` for a student-control-friendly stack |
| SQL Server local tooling | `sqlcmd` and `sqllocaldb` not found | SQL Server schema/runtime cannot be verified until user provisions SQL Server or an approved instance |
| Browser/map/TTS | Not started | Must not claim UI, map or TTS as verified before later test evidence |
| Cloudflare | Not started | No public tunnel is created; only manual future guidance is allowed |

## Audit decision

The documented target is one ASP.NET Core .NET 9 modular monolith using EF Core SQL Server, static HTML/CSS/JavaScript clients, and SQL-backed data. This is selected because the new workspace has no existing framework to preserve and the product instruction specifies SQL Server. The missing local SQL Server is an environment blocker for live data verification, not permission to switch databases silently.
