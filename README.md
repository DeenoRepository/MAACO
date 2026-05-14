# MAACO

MAACO (Multi-Agent Autonomous Coding Orchestrator) is a production-oriented platform for autonomous SDLC workflows.

## Current Progress

- Completed: `Milestone 0 (Bootstrap)`
- Completed: `Milestone 1 (Core Domain Model)`
- Completed: `Milestone 2 (Persistence Layer / SQLite)`
- In progress: `Milestone 3 (Backend API Skeleton)`
- M2 progress: `DbContext`, `DbSet<>`, mappings, indexes, and repository interfaces/implementations added
- M2 progress: `InitialCreate` migration added and applied, auto-migrate + seed on API startup enabled
- M3 progress: API skeleton wired (DI, EF Core, Serilog, OpenTelemetry, Swagger, CORS, global exception middleware)
- M3 progress: implemented `POST /api/projects` and `GET /api/projects` with DTO + FluentValidation
- M3 progress: implemented `GET /api/projects/{id}`
- M3 progress: implemented `POST /api/projects/{id}/scan` (queued stub for scanner integration in M5)
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
