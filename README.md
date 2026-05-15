# MAACO

MAACO (Multi-Agent Autonomous Coding Orchestrator) is a production-oriented autonomous SDLC platform.
It orchestrates scanning, planning, patching, build/test loops, diff review, and human approval for code changes.

## Architecture

- `MAACO.Api` - ASP.NET Core backend API + workflow endpoints + SignalR hub.
- `MAACO.App` - Avalonia desktop client (projects, tasks, monitor, diff, settings).
- `MAACO.Core` - domain model, enums, abstractions, workflow contracts.
- `MAACO.Infrastructure` - workflow engine, LLM gateway, memory service, event handlers.
- `MAACO.Persistence` - EF Core + SQLite, repositories, migrations.
- `MAACO.Tools` - tool execution layer (filesystem, patch, build/test, git, scanner, logs).
- `MAACO.Sandbox` - local sandbox execution and safety controls.
- `MAACO.Agents` - agent runtime, registry, prompts, fake/demo flow support.

## Requirements

- .NET SDK 10.0+
- Windows (primary dev target in current setup)
- SQLite (embedded via EF Core)
- Optional:
  - OpenAI-compatible endpoint for cloud LLM
  - Ollama for local LLM

## Run Backend

```bash
dotnet restore
dotnet run --project src/MAACO.Api
```

Default local API URL is typically from `launchSettings.json` (for example `http://localhost:5000` or `https://localhost:5001`).

## Run UI

```bash
dotnet run --project src/MAACO.App
```

Before starting UI, ensure backend API is already running.

## UI Experience

The desktop shell is optimized for enterprise usage:

- structured sidebar navigation for key SDLC stages;
- context-aware top header with current screen and connection state chips;
- card-based layouts for forms, monitor panels, and diff review;
- persistent status bar with busy indicator and live notifications;
- consistent loading/error surfaces across workflows.

## Configure Providers

Open **Settings** screen in UI.

### OpenAI-compatible

1. Select provider: `OpenAI-compatible`.
2. Set `Base URL`.
3. Set `API Key`.
4. Set model name.
5. Click **Test connection**.
6. Save settings.

### Ollama

1. Select provider: `Ollama`.
2. Set `Base URL` (usually `http://localhost:11434`).
3. Set model name available in local Ollama.
4. Click **Test connection**.
5. Save settings.

Provider selection is also read from config/env at API startup (`Maaco:Settings:LlmProvider`, `Maaco:Settings:LlmModel`).
For Ollama, local daemon must be running and reachable on configured `Ollama:BaseUrl`.

### Fake provider

Use `Fake` provider for deterministic local tests and development without external API keys.

## User Flow

### Add repository

1. Open **Projects**.
2. Add local repository path.
3. Run scan.
4. Review detected stack and build/test command suggestions.

### Create task

1. Open **Task Creation**.
2. Select project.
3. Enter title/description/constraints.
4. Choose approval mode.
5. Start task.

### Approve or reject changes

1. Open **Diff Review** when workflow reaches approval step.
2. Review changed files/unified diff.
3. Approve to proceed with commit path.
4. Reject to trigger rollback path (with rollback artifact/logging).

## Demo Flow

Sample demo task:

- Project: this repository (`MAACO`)
- Task title: `Improve workflow diagnostics for failed runs`
- Expected flow:
  - scan project
  - run planning/code/debug loop
  - produce diff
  - stop on approval
  - approve or reject and inspect resulting logs/artifacts

## Troubleshooting

- LLM connection test fails:
  - verify provider URL/model/API key and network access.
- Git operation tests fail to find `git`:
  - ensure `git.exe` is installed and accessible (Windows typical path: `C:\Program Files\Git\cmd\git.exe`).
- Workflow stuck at approval:
  - this is expected for human-in-the-loop mode; proceed via approve/reject endpoints or UI actions.

## Build & Test

```bash
dotnet build
dotnet test
```

Publish outputs used in Milestone 23:

```bash
dotnet publish src/MAACO.Api/MAACO.Api.csproj -c Release -o artifacts/publish/api
dotnet publish src/MAACO.App/MAACO.App.csproj -c Release -o artifacts/publish/app
```

API runtime logs are written to `logs/` under the API app base directory.

## Release Artifacts

- Release notes: `docs/RELEASE_NOTES_MVP.md`
- Known limitations: `docs/KNOWN_LIMITATIONS.md`
- API config template: `src/MAACO.Api/appsettings.example.json`

For current implementation progress and milestone truth see `MILESTONE_CHECKLIST.md`.
