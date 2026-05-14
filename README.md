# MAACO

MAACO (Multi-Agent Autonomous Coding Orchestrator) is a production-oriented platform for autonomous SDLC workflows.

## Current Progress

- Completed: `Milestone 0 (Bootstrap)`
- Completed: `Milestone 1 (Core Domain Model)`
- Completed: `Milestone 2 (Persistence Layer / SQLite)`
- Completed: `Milestone 3 (Backend API Skeleton)`
- In progress: `Milestone 4 (Realtime Event Bus + SignalR)`
- M4 progress: added `IEventBus` and in-memory event bus with DI registration
- M4 progress: added event handlers with `LogEvent` persistence, `workflowId`, and `correlationId` support
- M4 progress: added SignalR WorkflowHub (`/workflowHub`), workflow/project groups, and realtime event publishers
- M4 progress: added workflow status projection handlers from events (`Running`, `WaitingForApproval`, `Completed`, `Failed`)
- M2 progress: `DbContext`, `DbSet<>`, mappings, indexes, and repository interfaces/implementations added
- M2 progress: `InitialCreate` migration added and applied, auto-migrate + seed on API startup enabled
- M3 progress: API skeleton wired (DI, EF Core, Serilog, OpenTelemetry, Swagger, CORS, global exception middleware)
- M3 progress: implemented `POST /api/projects` and `GET /api/projects` with DTO + FluentValidation
- M3 progress: implemented `GET /api/projects/{id}`
- M3 progress: implemented `POST /api/projects/{id}/scan` (queued stub for scanner integration in M5)
- M3 progress: implemented Tasks endpoints (`POST /api/tasks`, `GET /api/tasks`, `GET /api/tasks/{id}`, `POST /api/tasks/{id}/cancel`)
- M3 progress: implemented Workflows endpoints (`GET /api/workflows/{id}`, `/steps`, `/logs`, `/artifacts`)
- M3 progress: implemented Approvals endpoints (`GET /api/approvals/pending`, `POST /api/approvals/{id}/approve`, `POST /api/approvals/{id}/reject`)
- M3 progress: implemented Git/Diff task endpoints (`GET /api/tasks/{id}/diff`, `POST /api/tasks/{id}/commit`, `POST /api/tasks/{id}/rollback`)
- M3 progress: implemented Settings endpoints (`GET /api/settings`, `PUT /api/settings`)
- M3 acceptance: Swagger + DTO endpoints + unified error format + SQLite integration completed
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
