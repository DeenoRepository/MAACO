# MAACO

MAACO (Multi-Agent Autonomous Coding Orchestrator) is a production-oriented platform for autonomous SDLC workflows.

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
