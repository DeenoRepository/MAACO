# AGENTS.md

# MAACO — Multi-Agent Autonomous Coding Orchestrator

Этот файл содержит обязательные инструкции для AI coding agents (Codex, Cursor, Claude Code, Copilot, Windsurf, Gemini CLI и других AI-assisted development systems), работающих с репозиторием MAACO.

---

# 1. Source of Truth

Основными источниками истины проекта являются:

1. `AGENTS.md`
2. `MILESTONE_CHECKLIST.md`
3. `README.md`
4. Domain model в `MAACO.Core`

При конфликте приоритет:

```text
AGENTS.md
  > MILESTONE_CHECKLIST.md
    > README.md
      > inline comments
```

---

# 2. Mandatory Milestone Compliance

Все AI agents ОБЯЗАНЫ следовать плану реализации из:

```text
MILESTONE_CHECKLIST.md
```

---

## Mandatory Rules

### Agents MUST:

- реализовывать проект milestone-by-milestone;
- выполнять checklist items полностью;
- завершать текущий milestone перед следующим;
- проверять build/tests перед переходом дальше;
- соблюдать порядок реализации;
- сохранять архитектурную целостность.

---

## Agents MUST NOT:

- перепрыгивать через milestones;
- реализовывать future features раньше foundation layers;
- добавлять premature abstractions;
- реализовывать distributed architecture в MVP;
- реализовывать non-goals из MVP;
- менять stack без justification;
- ломать dependency direction;
- переписывать большие части системы без необходимости.

---

# 3. Required Development Workflow

Для каждой задачи агент ОБЯЗАН:

1. Найти соответствующий milestone.
2. Найти checklist item.
3. Реализовать минимально необходимое изменение.
4. Добавить/обновить tests.
5. Проверить `dotnet build`.
6. Проверить `dotnet test`.
7. Проверить отсутствие compiler warnings.
8. Проверить что secrets не логируются.
9. Обновить documentation если необходимо.

---

# 4. Milestone Completion Rules

Milestone считается завершенным только если:

- все checklist items выполнены;
- `dotnet build` проходит;
- `dotnet test` проходит;
- нет critical TODO;
- нет broken workflow paths;
- все migrations применяются;
- нет критических runtime exceptions;
- workflow recovery не сломан.

---

# 5. High-Level Mission

MAACO — production-grade autonomous software engineering platform.

Основная цель системы:

- autonomous SDLC;
- multi-agent orchestration;
- build/test/debug loops;
- git automation;
- patch-based development;
- sandboxed execution;
- observability-first workflows;
- human approval pipeline.

---

# 6. Core Architectural Principles

## 6.1 Clean Architecture

Соблюдать dependency direction:

```text
UI/API
  -> Infrastructure
    -> Core
```

---

## 6.2 Core Isolation

`MAACO.Core` НЕ должен зависеть от:

- ASP.NET;
- Avalonia;
- EF Core;
- SQLite;
- конкретных LLM providers;
- Docker;
- UI frameworks.

---

## 6.3 Interface-first Design

Все внешние системы должны скрываться за интерфейсами.

Обязательные abstraction layers:

- `ILlmProvider`
- `ILlmGateway`
- `IAgent`
- `IAgentTool`
- `IWorkflowOrchestrator`
- `ISandboxExecutor`
- `IMemoryService`
- `IGitService`
- `IEventBus`

---

## 6.4 Deterministic Workflow Execution

Workflow execution должен быть максимально deterministic.

Каждый step обязан:

- иметь persisted state;
- быть replayable;
- иметь retry policy;
- публиковать events;
- сохранять artifacts;
- поддерживать cancellation.

---

## 6.5 Security-by-Default

Никогда:

- не выполнять dangerous shell commands;
- не выходить за пределы workspace;
- не читать `.ssh`;
- не читать `.aws`;
- не логировать secrets;
- не выполнять auto-push;
- не выполнять auto-deploy.

---

# 7. Repository Structure

```text
src/
  MAACO.Api/
  MAACO.App/
  MAACO.Core/
  MAACO.Infrastructure/
  MAACO.Agents/
  MAACO.Tools/
  MAACO.Persistence/
  MAACO.Sandbox/

tests/
  MAACO.Core.Tests/
  MAACO.Agents.Tests/
  MAACO.Tools.Tests/
```

---

# 8. Tech Stack

## Backend

- .NET 8+
- ASP.NET Core
- MediatR
- FluentValidation
- Serilog
- OpenTelemetry
- SignalR

---

## UI

- Avalonia
- MVVM
- CommunityToolkit.Mvvm

---

## Persistence

- SQLite
- EF Core

---

## Git

- LibGit2Sharp

---

## Sandbox

- Docker.DotNet
- local process isolation

---

## LLM Providers

Supported:

- OpenAI-compatible APIs
- Ollama

Future:

- Anthropic
- Gemini
- vLLM

---

# 9. Coding Standards

## 9.1 C# Rules

Обязательно:

- nullable reference types;
- async/await;
- constructor injection;
- explicit access modifiers;
- file-scoped namespaces;
- immutable DTOs where possible.

---

## 9.2 Forbidden

НЕ использовать:

```csharp
.Result
.Wait()
Thread.Sleep()
async void
```

---

## 9.3 Cancellation Tokens

Все async methods должны принимать:

```csharp
CancellationToken cancellationToken
```

---

## 9.4 Structured Logging

Использовать structured logging.

Плохо:

```csharp
logger.LogInformation("Task failed");
```

Хорошо:

```csharp
logger.LogInformation(
    "Task {TaskId} failed at step {Step}",
    taskId,
    step);
```

---

## 9.5 Exception Handling

Запрещено:

```csharp
catch {}
```

или silent failures.

---

## 9.6 DTO Separation

Нельзя возвращать EF entities напрямую из API.

Использовать DTO/mapping layer.

---

## 9.7 Validation

Все input models валидируются через FluentValidation.

---

# 10. Workflow Rules

## Workflow statuses

```text
Created
Planned
Queued
Running
WaitingForApproval
Retrying
Failed
Completed
Cancelled
RolledBack
```

НЕ делать invalid state transitions.

---

## Workflow persistence

Каждый step обязан:

- сохранять state;
- сохранять logs;
- сохранять artifacts;
- сохранять execution duration;
- сохранять retry count.

---

# 11. Agent Rules

## Agents MUST NOT:

- напрямую писать файлы;
- напрямую выполнять shell;
- напрямую выполнять git operations;
- напрямую писать в SQLite.

---

## Agents MUST:

- использовать tools layer;
- возвращать structured outputs;
- поддерживать cancellation;
- логировать decisions;
- минимизировать token usage.

---

## Agent outputs

Использовать:

```csharp
AgentResult
```

НЕ использовать raw string outputs если нужен structured response.

---

# 12. Prompt Engineering Rules

Prompts должны:

- быть deterministic;
- быть concise;
- содержать explicit constraints;
- содержать expected schema;
- минимизировать hallucinations;
- минимизировать unnecessary reasoning.

---

## Avoid

- giant prompts;
- dumping entire repositories;
- repeated context;
- unnecessary verbose chain-of-thought.

---

# 13. Tooling Rules

Все tools обязаны:

- implement `IAgentTool`;
- поддерживать cancellation;
- поддерживать timeout;
- логировать execution;
- валидировать inputs;
- валидировать paths;
- возвращать structured results.

---

## Tool execution safety

Перед выполнением:

- validate workspace;
- validate permissions;
- validate commands;
- redact secrets.

---

## FileSystem restrictions

Нельзя:

- писать вне workspace;
- читать system directories;
- удалять файлы без approval.

---

## Dangerous commands blocklist

```text
rm -rf /
sudo
format
mkfs
del /s
curl | sh
wget | sh
shutdown
reboot
```

---

# 14. Git Rules

## Allowed operations

- status
- diff
- branch
- commit
- rollback uncommitted changes

---

## Forbidden operations

- auto push
- force push
- deleting branches without approval
- credential operations
- modifying global git config

---

## Commit Rules

Commits разрешены только после approval.

Commit format:

```text
feat(maaco): short summary

- change 1
- change 2
```

---

# 15. Database Rules

SQLite — source of truth для MVP.

Использовать:

- EF Core migrations;
- optimistic concurrency;
- indexed foreign keys;
- audit timestamps.

---

## Forbidden

- dynamic SQL;
- blocking DB calls;
- raw SQL без необходимости.

---

# 16. UI Rules

UI ОБЯЗАН:

- показывать workflow progress;
- показывать realtime logs;
- показывать diff review;
- показывать approval state;
- показывать retry/failure states.

---

## UX Requirements

Всегда показывать:

- loading state;
- error state;
- retry option;
- workflow status.

---

# 17. Security Rules

## NEVER:

- expose secrets in logs;
- expose API keys in UI;
- send secrets to LLM;
- allow unrestricted filesystem access;
- allow unrestricted shell access.

---

## MUST:

- redact secrets;
- sanitize prompts;
- validate commands;
- validate patches;
- validate paths.

---

# 18. Observability Rules

Обязательно логировать:

- workflow lifecycle;
- step lifecycle;
- tool executions;
- agent decisions;
- retries;
- failures;
- approvals;
- LLM calls.

---

## MUST support:

- structured logs;
- correlation ids;
- workflow tracing;
- replayable execution history.

---

# 19. Performance Rules

## Avoid:

- rescanning entire repositories repeatedly;
- loading huge files into prompts;
- blocking IO;
- unnecessary serialization;
- giant object graphs.

---

## Prefer:

- caching;
- incremental scans;
- bounded memory;
- streaming logs;
- lightweight DTOs.

---

# 20. File Modification Rules

Перед изменением файлов ОБЯЗАТЕЛЬНО:

1. Generate patch/diff.
2. Validate patch.
3. Save artifact.
4. Apply patch.
5. Verify result.

---

## Forbidden

НЕ overwrite files blindly.

---

# 21. Approval Rules

Human approval required for:

- git commit;
- rollback;
- deleting files;
- modifying configuration;
- destructive operations.

---

# 22. Testing Rules

## Required

Каждый critical component обязан иметь tests.

---

## Frameworks

- xUnit
- FluentAssertions
- NSubstitute/Moq

---

## Must Test

- workflow transitions;
- retry logic;
- patch validation;
- git rollback;
- sandbox restrictions;
- approval flow;
- tool boundaries.

---

# 23. Definition of Done

Task считается завершенной только если:

- build succeeds;
- tests pass;
- logs persisted;
- workflow completed;
- diff generated;
- approval completed;
- no critical errors remain.

---

# 24. Non-Goals (MVP)

НЕ реализовывать:

- distributed agents;
- RBAC;
- cloud sync;
- auto deploy;
- auto push;
- enterprise infra;
- multi-user collaboration.

---

# 25. Preferred Development Order

1. Core
2. Persistence
3. API
4. Realtime events
5. Scanner
6. Tools
7. Sandbox
8. Git
9. LLM Gateway
10. Agents
11. Workflow Engine
12. Memory
13. Build/Test loop
14. UI
15. Approval flow
16. Security hardening
17. Reliability
18. Testing
19. Documentation

---

# 26. Commands

## Build

```bash
dotnet build
```

---

## Tests

```bash
dotnet test
```

---

## Run API

```bash
dotnet run --project src/MAACO.Api
```

---

## Run UI

```bash
dotnet run --project src/MAACO.App
```

---

# 27. Final Instructions for AI Agents

При работе с репозиторием:

- делай минимально необходимые изменения;
- не ломай архитектуру;
- не ломай dependency direction;
- не добавляй heavy dependencies без justification;
- не дублируй код;
- обновляй tests;
- обновляй documentation;
- сохраняй backward compatibility where possible.

---

## If Task Is Ambiguous

1. choose safest implementation;
2. minimize blast radius;
3. leave TODO/comment if needed;
4. avoid destructive assumptions;
5. follow current milestone strictly.



---

# 28. Autonomous Commit & Task Continuation Rules

## Mandatory External Repository Commits

После каждого успешно завершенного изменения агент ОБЯЗАН:

1. Проверить успешность:
   - `dotnet build`
   - `dotnet test`
   - validation checks
   - workflow step completion

2. Создать git commit.

3. Выполнить push во внешний repository.

---

## Required Git Flow

### For every completed task:

```text
change
  -> validate
    -> build
      -> tests
        -> commit
          -> push
            -> update progress
```

---

## Commit Requirements

Каждый commit ОБЯЗАН:

- быть atomic;
- содержать только изменения текущей задачи;
- иметь structured commit message;
- содержать milestone reference;
- содержать checklist reference если возможно.

---

## Commit Message Format

```text
type(scope): short summary

Milestone: M{number}
Task: {task-name}

- completed item 1
- completed item 2
- completed item 3
```

Пример:

```text
feat(orchestrator): implement workflow step persistence

Milestone: M11
Task: workflow-engine-persistence

- added workflow checkpoint store
- added step execution persistence
- added retry metadata tracking
```

---

## Push Rules

После успешного commit агент ОБЯЗАН:

- выполнить push в remote repository;
- проверить что push succeeded;
- обновить progress state;
- зафиксировать hash commit в logs/artifacts.

---

## Forbidden

Запрещено:

- оставлять completed work uncommitted;
- накапливать большие незакоммиченные изменения;
- смешивать несколько milestones в одном commit;
- force push;
- rewrite public history.

---

# 29. Task Continuation Protocol

После завершения задачи агент ОБЯЗАН автоматически определить следующую задачу.

---

## Mandatory Completion Sequence

После завершения task агент ОБЯЗАН:

1. Mark current task completed.
2. Commit changes.
3. Push changes.
4. Update workflow state.
5. Analyze current milestone.
6. Найти следующий incomplete checklist item.
7. Сформировать next task recommendation.
8. Продолжить execution если auto-mode enabled.

---

## Next Task Selection Rules

Следующая задача выбирается по приоритету:

1. remaining checklist item текущего milestone;
2. blocking dependency;
3. failed retry item;
4. следующий milestone;
5. technical debt generated by current change.

---

## Agents MUST NOT

- перескакивать через checklist items;
- начинать новый milestone до completion текущего;
- выполнять unrelated refactoring;
- генерировать speculative tasks вне roadmap.

---

## Required Next Task Output

После завершения задачи агент ОБЯЗАН выводить:

```text
Completed:
- current task summary

Commit:
- commit hash
- branch name

Next Recommended Task:
- milestone
- checklist item
- dependency reason
- estimated scope
```

---

## Autonomous Execution Mode

Если autonomous mode enabled:

```text
complete task
  -> commit
    -> push
      -> select next task
        -> continue execution
```

---

## Human Approval Mode

Если approval mode enabled:

```text
complete task
  -> commit
    -> push
      -> generate next task
        -> wait for approval
```

---

# 30. Progress Tracking Rules

## Agents MUST maintain:

- completed checklist items;
- current milestone status;
- current branch status;
- latest commit hash;
- failed tasks;
- retry attempts;
- pending approvals.

---

## Progress Artifacts

После каждого completed task сохранять:

- commit hash;
- diff artifact;
- task summary;
- workflow summary;
- next task recommendation;
- build/test results.

