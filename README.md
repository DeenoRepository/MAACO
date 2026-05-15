# MAACO

MAACO (Multi-Agent Autonomous Coding Orchestrator) is a production-oriented platform for autonomous SDLC workflows.

## Current Progress

- Completed: `Milestone 0 (Bootstrap)`
- Completed: `Milestone 1 (Core Domain Model)`
- Completed: `Milestone 2 (Persistence Layer / SQLite)`
- Completed: `Milestone 3 (Backend API Skeleton)`
- Completed: `Milestone 4 (Realtime Event Bus + SignalR)`
- Completed: `Milestone 5 (Project Scanner)`
- Completed: `Milestone 6 (Tooling Core)`
- Completed: `Milestone 7 (Sandbox Execution)`
- Completed: `Milestone 8 (Git Integration)`
- Completed: `Milestone 9 (LLM Gateway)`
- Completed: `Milestone 10 (Agent Runtime)`
- Completed: `Milestone 11 (Workflow Orchestrator)`
- Completed: `Milestone 12 (Memory / Context MVP)`
- Completed: `Milestone 13 (Build/Test Loop)`
- Completed: `Milestone 14 (Avalonia UI Shell)`
- Completed: `Milestone 15 (UI Project + Task Flow)`
- Completed: `Milestone 16 (UI Workflow Monitor)`
- Completed: `Milestone 17 (Diff Review + Approval UI)`
- In progress: `Milestone 18 (Settings & Configuration)`
- M18 progress: settings UI implemented (provider/model/base URLs, API key masked input, timeout/retries, default approval mode)
- M18 progress: settings API extended + validation + connection test endpoint (`POST /api/settings/test-connection`)
- In progress: `Milestone 19 (Security Hardening)`
- M19 progress: filesystem boundary hardening (path traversal/system dirs/.ssh/.aws blocks) + tests
- M19 progress: controlled `.env` access in `FileSystemTool` (value redaction on read)
- M19 progress: prompt security hardening (injection indicators, untrusted external links handling, metadata logging)
- M19 progress: command security hardening (dangerous blocklist incl. auto deploy patterns, output size limit) + tests
- M19 progress: file write audit trail in tools (`.maaco/audit/file-writes.log`)
- M19 progress: sandbox command execution audit trail (`.maaco/audit/command-executions.log`)
- M19 progress: approval decision audit logging added for `approve/reject` actions (persisted as `LogEvent` with workflow and trace correlation)
- In progress: `Milestone 20 (Reliability & Recovery)`
- M20 progress: workflow step checkpoint logs expanded to persist step input/output/error payloads
- M20 progress: pending approval requests are now persisted during `ApprovalStep` and survive app restart/re-scope
- M20 progress: failed workflows are recoverable after restart via API and now expose failure reason in workflow DTO
- M20 progress: cancelled workflow execution keeps persisted logs available for post-mortem diagnostics
- M20 progress: LLM retry policy hardened with bounded max retries, exponential backoff, and no-retry behavior for validation failures
- M20 progress: tool execution now retries transient failures with bounded exponential backoff and no-retry behavior for validation errors
- M20 progress: rollback-on-reject now persists rollback snapshot artifact and keeps failure diagnostics artifacts available
- M20 progress: rollback now also cleans up unapplied debug patch files and records cleanup in workflow logs
- M20 progress: restart/re-scope recovery verified for failed workflows with persisted steps/logs and API-level diagnostic reason
- Completed: `Milestone 21 (Testing)`
- M21 progress: core/tools/agents/workflow test matrix aligned with checklist; deterministic Fake LLM test paths validated
- Source of milestone truth: `MILESTONE_CHECKLIST.md`

## Repository Layout

- `src/MAACO.Api` - ASP.NET Core API
- `src/MAACO.App` - Avalonia desktop app
- `src/MAACO.Core` - domain and contracts
- `src/MAACO.Infrastructure` - infrastructure adapters
- `src/MAACO.Agents` - agent orchestration layer
- `src/MAACO.Tools` - tool execution layer
- `src/MAACO.Persistence` - data access and EF Core
- `src/MAACO.Sandbox` - sandboxed execution
- `tests/*` - unit/integration test projects

## Build

```bash
dotnet restore
dotnet build
```

## Test

```bash
dotnet test
```
