# MAACO — детальный Milestone Plan реализации MVP с чеклистом

## Milestone 0 — Bootstrap репозитория

### Цель
Создать базовую структуру проекта MAACO.

### Checklist

- [ ] Создать `MAACO.sln`
- [ ] Создать структуру:

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

- [ ] Подключить project references
- [ ] Добавить `.gitignore`
- [ ] Добавить `.editorconfig`
- [ ] Добавить `Directory.Build.props`
- [ ] Включить nullable reference types
- [ ] Включить implicit usings
- [ ] Добавить базовый `README.md`
- [ ] Проверить `dotnet restore`
- [ ] Проверить `dotnet build`
- [ ] Проверить `dotnet test`

### Acceptance Criteria

- [ ] Solution собирается без ошибок
- [ ] Все проекты подключены
- [ ] Тестовые проекты запускаются
- [ ] README описывает назначение MAACO

---

# Milestone 1 — Core Domain Model

### Цель
Реализовать доменную модель MAACO.

### Checklist

## Base types

- [ ] Создать `Entity`
- [ ] Создать `AuditableEntity`
- [ ] Добавить `Id`
- [ ] Добавить `CreatedAt`
- [ ] Добавить `UpdatedAt`
- [ ] Добавить `Version`
- [ ] Добавить `MetadataJson`

## Enums

- [ ] Создать `TaskStatus`
- [ ] Создать `WorkflowStatus`
- [ ] Создать `WorkflowStepStatus`
- [ ] Создать `AgentStatus`
- [ ] Создать `ToolExecutionStatus`
- [ ] Создать `ApprovalStatus`
- [ ] Создать `ApprovalMode`
- [ ] Создать `ArtifactType`
- [ ] Создать `MemoryRecordType`
- [ ] Создать `GitOperationType`
- [ ] Создать `BuildRunStatus`
- [ ] Создать `LogSeverity`

## Entities

- [ ] Создать `Project`
- [ ] Создать `TaskItem`
- [ ] Создать `Workflow`
- [ ] Создать `WorkflowStep`
- [ ] Создать `AgentDefinition`
- [ ] Создать `ToolExecution`
- [ ] Создать `LogEvent`
- [ ] Создать `Artifact`
- [ ] Создать `GitOperation`
- [ ] Создать `BuildRun`
- [ ] Создать `MemoryRecord`
- [ ] Создать `ApprovalRequest`
- [ ] Создать `LlmCallLog`
- [ ] Создать `ProjectContextSnapshot`

## Value objects

- [ ] Создать `RepositoryPath`
- [ ] Создать `CommandSpec`
- [ ] Создать `LlmUsage`
- [ ] Создать `PatchSummary`
- [ ] Создать `ExecutionResult`
- [ ] Создать `DetectedProjectStack`
- [ ] Создать `FileChangeSummary`

## Domain events

- [ ] Создать `TaskCreatedEvent`
- [ ] Создать `WorkflowStartedEvent`
- [ ] Создать `WorkflowStepStartedEvent`
- [ ] Создать `WorkflowStepCompletedEvent`
- [ ] Создать `WorkflowStepFailedEvent`
- [ ] Создать `ApprovalRequestedEvent`
- [ ] Создать `WorkflowCompletedEvent`
- [ ] Создать `WorkflowFailedEvent`

### Acceptance Criteria

- [ ] Все entities компилируются
- [ ] Все enum покрывают MVP workflow
- [ ] Есть unit tests для state transitions
- [ ] Невалидные переходы статусов запрещены

---

# Milestone 2 — Persistence Layer / SQLite

### Цель
Реализовать хранение данных в SQLite через EF Core.

### Checklist

## EF Core setup

- [ ] Создать `MaacoDbContext`
- [ ] Добавить `DbSet<Project>`
- [ ] Добавить `DbSet<TaskItem>`
- [ ] Добавить `DbSet<Workflow>`
- [ ] Добавить `DbSet<WorkflowStep>`
- [ ] Добавить `DbSet<AgentDefinition>`
- [ ] Добавить `DbSet<ToolExecution>`
- [ ] Добавить `DbSet<LogEvent>`
- [ ] Добавить `DbSet<Artifact>`
- [ ] Добавить `DbSet<GitOperation>`
- [ ] Добавить `DbSet<BuildRun>`
- [ ] Добавить `DbSet<MemoryRecord>`
- [ ] Добавить `DbSet<ApprovalRequest>`
- [ ] Добавить `DbSet<LlmCallLog>`
- [ ] Добавить `DbSet<ProjectContextSnapshot>`

## Mappings

- [ ] Настроить primary keys
- [ ] Настроить foreign keys
- [ ] Настроить enum conversion в string или int
- [ ] Настроить `MetadataJson`
- [ ] Настроить `Version` как concurrency token
- [ ] Настроить cascade rules

## Indexes

- [ ] Индекс `Projects.RepositoryPath`
- [ ] Индекс `TaskItems.ProjectId`
- [ ] Индекс `Workflows.TaskId`
- [ ] Индекс `WorkflowSteps.WorkflowId`
- [ ] Индекс `ToolExecutions.WorkflowId`
- [ ] Индекс `LogEvents.WorkflowId`
- [ ] Индекс `Artifacts.TaskId`
- [ ] Индекс `GitOperations.TaskId`
- [ ] Индекс `BuildRuns.WorkflowId`
- [ ] Индекс `MemoryRecords.ProjectId`
- [ ] Индекс `ApprovalRequests.Status`

## Repositories

- [ ] Создать `IProjectRepository`
- [ ] Создать `ITaskRepository`
- [ ] Создать `IWorkflowRepository`
- [ ] Создать `ILogRepository`
- [ ] Создать `IMemoryRepository`
- [ ] Реализовать EF repositories

## Migrations

- [ ] Создать initial migration
- [ ] Добавить automatic migration на старте dev-режима
- [ ] Добавить seed базовых агентов
- [ ] Добавить seed базовых tools

### Acceptance Criteria

- [ ] SQLite база создается автоматически
- [ ] Initial migration применяется
- [ ] Данные сохраняются и читаются
- [ ] Unit/integration tests проходят

---

# Milestone 3 — Backend API Skeleton

### Цель
Создать Web API MAACO и базовые endpoints.

### Checklist

## API setup

- [ ] Настроить ASP.NET Core Web API
- [ ] Подключить DI
- [ ] Подключить EF Core
- [ ] Подключить Serilog
- [ ] Подключить OpenTelemetry
- [ ] Подключить Swagger
- [ ] Настроить CORS для desktop UI
- [ ] Добавить global exception middleware

## Endpoints Projects

- [ ] `POST /api/projects`
- [ ] `GET /api/projects`
- [ ] `GET /api/projects/{id}`
- [ ] `POST /api/projects/{id}/scan`

## Endpoints Tasks

- [ ] `POST /api/tasks`
- [ ] `GET /api/tasks`
- [ ] `GET /api/tasks/{id}`
- [ ] `POST /api/tasks/{id}/cancel`

## Endpoints Workflows

- [ ] `GET /api/workflows/{id}`
- [ ] `GET /api/workflows/{id}/steps`
- [ ] `GET /api/workflows/{id}/logs`
- [ ] `GET /api/workflows/{id}/artifacts`

## Endpoints Approvals

- [ ] `GET /api/approvals/pending`
- [ ] `POST /api/approvals/{id}/approve`
- [ ] `POST /api/approvals/{id}/reject`

## Endpoints Git/Diff

- [ ] `GET /api/tasks/{id}/diff`
- [ ] `POST /api/tasks/{id}/commit`
- [ ] `POST /api/tasks/{id}/rollback`

## Endpoints Settings

- [ ] `GET /api/settings`
- [ ] `PUT /api/settings`

### Acceptance Criteria

- [ ] Swagger открывается
- [ ] Все endpoints возвращают корректные DTO
- [ ] Ошибки возвращаются в едином формате
- [ ] API работает с SQLite

---

# Milestone 4 — Realtime Event Bus + SignalR

### Цель
Реализовать realtime обновления workflow для UI.

### Checklist

## Internal event bus

- [ ] Создать `IEventBus`
- [ ] Реализовать in-memory event bus
- [ ] Добавить event handlers
- [ ] Добавить persistence важных событий в `LogEvent`
- [ ] Добавить correlation id
- [ ] Добавить workflow id в события

## SignalR

- [ ] Создать `WorkflowHub`
- [ ] Добавить подключение `/workflowHub`
- [ ] Реализовать group per workflow
- [ ] Реализовать group per project
- [ ] Публиковать `WorkflowStarted`
- [ ] Публиковать `StepStarted`
- [ ] Публиковать `StepCompleted`
- [ ] Публиковать `StepFailed`
- [ ] Публиковать `LogReceived`
- [ ] Публиковать `ToolExecutionStarted`
- [ ] Публиковать `ToolExecutionCompleted`
- [ ] Публиковать `ApprovalRequested`
- [ ] Публиковать `WorkflowCompleted`
- [ ] Публиковать `WorkflowFailed`

### Acceptance Criteria

- [ ] UI/тестовый клиент получает realtime события
- [ ] События сохраняются в логах
- [ ] События имеют correlation id
- [ ] Workflow status обновляется корректно

---

# Milestone 5 — Project Scanner

### Цель
Научить MAACO анализировать локальный репозиторий.

### Checklist

## Repository validation

- [ ] Проверять существование пути
- [ ] Проверять доступность чтения
- [ ] Проверять наличие `.git`
- [ ] Запрещать системные директории
- [ ] Нормализовать path

## File scanning

- [ ] Игнорировать `.git`
- [ ] Игнорировать `bin`
- [ ] Игнорировать `obj`
- [ ] Игнорировать `node_modules`
- [ ] Игнорировать `.vs`
- [ ] Игнорировать `.idea`
- [ ] Игнорировать build artifacts
- [ ] Ограничить максимальный размер файла
- [ ] Ограничить максимальное число файлов

## Stack detection

- [ ] Детектировать `.NET`
- [ ] Детектировать `Node.js`
- [ ] Детектировать `Python`
- [ ] Детектировать generic project
- [ ] Находить solution/project files
- [ ] Находить package manifests

## Build/test command detection

- [ ] Для `.NET`: `dotnet build`
- [ ] Для `.NET`: `dotnet test`
- [ ] Для Node.js: `npm test` или script из `package.json`
- [ ] Для Python: `pytest`
- [ ] Позволить override команд в settings

## Context snapshot

- [ ] Создавать `ProjectContextSnapshot`
- [ ] Сохранять список ключевых файлов
- [ ] Сохранять detected stack
- [ ] Сохранять build/test commands
- [ ] Сохранять project summary

### Acceptance Criteria

- [ ] Локальный repo добавляется
- [ ] Project scan сохраняется в БД
- [ ] Stack определяется корректно
- [ ] Build/test команды предлагаются автоматически

---

# Milestone 6 — Tooling Core

### Цель
Реализовать plugin-like tools layer.

### Checklist

## Tool abstractions

- [ ] Создать `IAgentTool`
- [ ] Создать `ToolRequest`
- [ ] Создать `ToolResult`
- [ ] Создать `ToolPermission`
- [ ] Создать `ToolRegistry`
- [ ] Добавить DI registration tools
- [ ] Добавить tool execution logging
- [ ] Добавить timeout support
- [ ] Добавить cancellation support

## Required tools

- [ ] `FileSystemTool`
- [ ] `ProjectScannerTool`
- [ ] `CodePatchTool`
- [ ] `BuildTool`
- [ ] `TestTool`
- [ ] `GitTool`
- [ ] `DiffTool`
- [ ] `LogAnalysisTool`

## Tool safety

- [ ] Проверка permissions
- [ ] Проверка workspace boundary
- [ ] Логирование input/output
- [ ] Redaction secrets
- [ ] Max output length
- [ ] Error normalization

### Acceptance Criteria

- [ ] Tools вызываются через единый интерфейс
- [ ] Все tool executions сохраняются
- [ ] Tool failures не валят процесс без controlled error
- [ ] Workspace boundary работает

---

# Milestone 7 — Sandbox Execution

### Цель
Реализовать безопасное выполнение команд.

### Checklist

## Sandbox abstractions

- [ ] Создать `ISandboxExecutor`
- [ ] Создать `SandboxRequest`
- [ ] Создать `SandboxResult`
- [ ] Создать `SandboxOptions`

## Local sandbox

- [ ] Выполнение процесса в project directory
- [ ] Timeout
- [ ] CancellationToken
- [ ] Capture stdout
- [ ] Capture stderr
- [ ] Capture exit code
- [ ] Environment filtering
- [ ] Working directory validation

## Command safety

- [ ] Command allowlist для MVP
- [ ] Blocklist опасных команд
- [ ] Запрет доступа вне workspace
- [ ] Запрет `.ssh`
- [ ] Запрет `.aws`
- [ ] Запрет системных путей
- [ ] Redaction `.env` значений

## Docker abstraction

- [ ] Создать Docker sandbox interface
- [ ] Добавить базовый stub
- [ ] Подготовить Docker.DotNet integration point

### Acceptance Criteria

- [ ] Build/test команды выполняются
- [ ] Timeout работает
- [ ] Cancellation работает
- [ ] Dangerous commands блокируются
- [ ] Stdout/stderr сохраняются

---

# Milestone 8 — Git Integration

### Цель
Реализовать безопасный Git workflow.

### Checklist

## Git operations

- [ ] Проверить, что repo является git repo
- [ ] Получать current branch
- [ ] Получать status
- [ ] Создавать feature branch
- [ ] Получать diff
- [ ] Получать changed files
- [ ] Создавать commit
- [ ] Откатывать uncommitted changes
- [ ] Генерировать patch artifact

## Safety

- [ ] Не делать push в MVP
- [ ] Не делать force reset без approval
- [ ] Commit только после approval
- [ ] Rollback только изменений MAACO
- [ ] Сохранять `GitOperation`

## Branch naming

- [ ] Генерировать branch name `maaco/task-{shortId}-{slug}`
- [ ] Проверять конфликт имен
- [ ] Создавать новый branch при необходимости

### Acceptance Criteria

- [ ] Diff генерируется
- [ ] Commit создается после approval
- [ ] Reject откатывает изменения
- [ ] Git operations сохраняются в БД

---

# Milestone 9 — LLM Gateway

### Цель
Реализовать LLM abstraction и провайдеры.

### Checklist

## Abstractions

- [ ] Создать `ILlmProvider`
- [ ] Создать `ILlmGateway`
- [ ] Создать `LlmRequest`
- [ ] Создать `LlmResponse`
- [ ] Создать `LlmMessage`
- [ ] Создать `LlmProviderOptions`
- [ ] Создать `ModelRoutingPolicy`

## Providers

- [ ] Реализовать `FakeLlmProvider`
- [ ] Реализовать OpenAI-compatible provider
- [ ] Реализовать Ollama provider
- [ ] Добавить provider health check
- [ ] Добавить retry transient errors
- [ ] Добавить timeout

## Logging/cost

- [ ] Логировать prompt с redaction
- [ ] Логировать response с redaction
- [ ] Сохранять `LlmCallLog`
- [ ] Оценивать input tokens
- [ ] Оценивать output tokens
- [ ] Считать estimated cost

## Routing

- [ ] Planning → configured strong model
- [ ] Coding → configured strong model
- [ ] Debugging → configured strong model
- [ ] Summary/log analysis → cheap/local model if available
- [ ] Fallback на default provider

### Acceptance Criteria

- [ ] Fake provider работает без API key
- [ ] OpenAI-compatible provider вызывается через config
- [ ] Ollama provider вызывается через config
- [ ] Все LLM calls сохраняются
- [ ] Secrets не попадают в logs

---

# Milestone 10 — Agent Runtime

### Цель
Реализовать базовую агентную систему.

### Checklist

## Base abstractions

- [ ] Создать `IAgent`
- [ ] Создать `AgentContext`
- [ ] Создать `AgentResult`
- [ ] Создать `AgentCapability`
- [ ] Создать `AgentRegistry`
- [ ] Создать `AgentExecutionService`

## Agents

- [ ] `OrchestratorAgent`
- [ ] `TaskPlannerAgent`
- [ ] `BackendDeveloperAgent`
- [ ] `TestWriterAgent`
- [ ] `DebuggingAgent`
- [ ] `GitManagerAgent`
- [ ] `DocumentationAgent`

## Agent constraints

- [ ] Agents не пишут файлы напрямую
- [ ] Agents используют tools
- [ ] Agents получают scoped context
- [ ] Agents логируют решения
- [ ] Agents возвращают structured output
- [ ] Agents поддерживают cancellation

## Prompts

- [ ] Создать system prompt для Planner
- [ ] Создать system prompt для Developer
- [ ] Создать system prompt для Test Writer
- [ ] Создать system prompt для Debugging
- [ ] Создать system prompt для Documentation
- [ ] Добавить response schema expectations

### Acceptance Criteria

- [ ] Agent registry работает
- [ ] Agents вызываются через runtime
- [ ] Fake LLM позволяет прогнать demo workflow
- [ ] Agent outputs сохраняются как artifacts/logs

---

# Milestone 11 — Workflow Orchestrator

### Цель
Реализовать deterministic MVP pipeline.

### Pipeline

```text
User Task
  -> Project Scan
  -> Git Branch
  -> Task Planning
  -> Code Generation
  -> Patch Application
  -> Test Generation
  -> Build Execution
  -> Test Execution
  -> Debug Loop
  -> Diff Generation
  -> Approval Request
  -> Git Commit or Rollback
  -> Documentation Update
  -> Final Report
```

### Checklist

## Workflow engine

- [ ] Создать `IWorkflowOrchestrator`
- [ ] Создать `WorkflowExecutionContext`
- [ ] Создать `WorkflowStepExecutor`
- [ ] Создать checkpoint после каждого step
- [ ] Сохранять status workflow
- [ ] Сохранять status step
- [ ] Публиковать realtime events
- [ ] Поддержать cancellation

## Step execution

- [ ] ProjectScanStep
- [ ] GitBranchStep
- [ ] PlanningStep
- [ ] CodeGenerationStep
- [ ] PatchApplicationStep
- [ ] TestGenerationStep
- [ ] BuildStep
- [ ] TestStep
- [ ] DebugStep
- [ ] DiffStep
- [ ] ApprovalStep
- [ ] CommitStep
- [ ] RollbackStep
- [ ] DocumentationStep
- [ ] FinalReportStep

## Retry/debug loop

- [ ] Max debug retries default 3
- [ ] Build failure triggers DebuggingAgent
- [ ] Test failure triggers DebuggingAgent
- [ ] Debug patch applied through CodePatchTool
- [ ] Stop after max retries
- [ ] Mark workflow failed with logs

### Acceptance Criteria

- [ ] Workflow создается из task
- [ ] Steps выполняются последовательно
- [ ] State сохраняется после каждого step
- [ ] Failure не теряет контекст
- [ ] Debug loop работает
- [ ] Workflow можно cancel

---

# Milestone 12 — Memory / Context MVP

### Цель
Реализовать базовую память проекта.

### Checklist

## Memory service

- [ ] Создать `IMemoryService`
- [ ] Сохранять `ProjectSummary`
- [ ] Сохранять `TaskSummary`
- [ ] Сохранять `FileSummary`
- [ ] Сохранять `BuildFailure`
- [ ] Сохранять `Decision`
- [ ] Сохранять `AgentNote`

## Retrieval

- [ ] Получать memory по project id
- [ ] Получать memory по task id
- [ ] Keyword search
- [ ] Limit top N records
- [ ] Exclude stale records by status
- [ ] Context assembly для agents

## Future-ready fields

- [ ] `EmbeddingProvider`
- [ ] `EmbeddingModel`
- [ ] `EmbeddingHash`
- [ ] `VectorRef`
- [ ] `ContentHash`

### Acceptance Criteria

- [ ] Agents получают project context
- [ ] Build/test failures сохраняются
- [ ] Previous task summaries доступны
- [ ] Schema готова к vector search

---

# Milestone 13 — Build/Test Loop

### Цель
Довести автоматический цикл сборки, тестов и исправлений.

### Checklist

## Build

- [ ] Выполнять detected build command
- [ ] Сохранять `BuildRun`
- [ ] Сохранять stdout/stderr artifact
- [ ] Парсить exit code
- [ ] Публиковать events

## Test

- [ ] Выполнять detected test command
- [ ] Сохранять test output
- [ ] Определять failed tests
- [ ] Публиковать events

## Log analysis

- [ ] Извлекать compiler errors
- [ ] Извлекать stack traces
- [ ] Извлекать failed assertions
- [ ] Передавать summary DebuggingAgent

## Debug cycle

- [ ] Сформировать debug prompt
- [ ] Получить patch
- [ ] Validate patch
- [ ] Apply patch
- [ ] Повторить build/test
- [ ] Остановиться при success/max retries

### Acceptance Criteria

- [ ] Ошибка сборки приводит к debug iteration
- [ ] Ошибка тестов приводит к debug iteration
- [ ] Все итерации видны в logs
- [ ] После success workflow идет к diff review

---

# Milestone 14 — Avalonia UI Shell

### Цель
Создать desktop UI MAACO.

### Checklist

## App shell

- [ ] Создать Avalonia app
- [ ] Настроить MVVM
- [ ] Настроить DI
- [ ] Настроить navigation service
- [ ] Настроить API client
- [ ] Настроить SignalR client

## Screens

- [ ] Dashboard
- [ ] Projects Screen
- [ ] Task Creation Screen
- [ ] Workflow Monitor
- [ ] Diff Review Screen
- [ ] Settings Screen
- [ ] Logs Screen

## Layout

- [ ] Sidebar navigation
- [ ] Main content area
- [ ] Status bar
- [ ] Notifications/toasts
- [ ] Loading/error states

### Acceptance Criteria

- [ ] App запускается
- [ ] Навигация работает
- [ ] API client подключается к backend
- [ ] SignalR получает events
- [ ] Ошибки отображаются пользователю

---

# Milestone 15 — UI Project + Task Flow

### Цель
Дать пользователю возможность добавить repo и создать задачу.

### Checklist

## Projects UI

- [ ] Выбрать folder
- [ ] Добавить проект
- [ ] Запустить scan
- [ ] Показать detected stack
- [ ] Показать build/test commands
- [ ] Показать project files summary

## Task UI

- [ ] Выбрать project
- [ ] Ввести title
- [ ] Ввести description
- [ ] Ввести constraints
- [ ] Выбрать approval mode
- [ ] Запустить task
- [ ] Перейти в workflow monitor

### Acceptance Criteria

- [ ] Пользователь добавляет local repo через UI
- [ ] Пользователь создает task через UI
- [ ] Workflow стартует
- [ ] UI показывает начальный статус workflow

---

# Milestone 16 — UI Workflow Monitor

### Цель
Визуализировать выполнение pipeline.

### Checklist

## Workflow timeline

- [ ] Показать список steps
- [ ] Показать текущий step
- [ ] Показать statuses
- [ ] Показать duration
- [ ] Показать retry count

## Live logs

- [ ] Stream logs через SignalR
- [ ] Фильтр по severity
- [ ] Фильтр по agent
- [ ] Фильтр по tool
- [ ] Copy log line
- [ ] Save logs view

## Agent activity

- [ ] Показывать активного agent
- [ ] Показывать tool executions
- [ ] Показывать LLM call status
- [ ] Показывать token estimate

### Acceptance Criteria

- [ ] Workflow progress виден realtime
- [ ] Логи обновляются без refresh
- [ ] Ошибки явно подсвечены
- [ ] Пользователь понимает текущий этап

---

# Milestone 17 — Diff Review + Approval UI

### Цель
Реализовать human-in-the-loop approval.

### Checklist

## Diff UI

- [ ] Получить diff по task
- [ ] Показать changed files
- [ ] Показать unified diff
- [ ] Подсветить added/removed lines
- [ ] Показать generated commit message
- [ ] Позволить изменить commit message

## Approval

- [ ] Approve button
- [ ] Reject button
- [ ] Reject with reason
- [ ] Approve вызывает commit
- [ ] Reject вызывает rollback
- [ ] Показать результат операции

### Acceptance Criteria

- [ ] Workflow останавливается на approval
- [ ] Пользователь видит diff
- [ ] Approve создает commit
- [ ] Reject откатывает изменения
- [ ] Результат виден в UI

---

# Milestone 18 — Settings & Configuration

### Цель
Реализовать настройки MAACO.

### Checklist

## Backend settings

- [ ] Хранить настройки в appsettings + DB override
- [ ] Endpoint `GET /api/settings`
- [ ] Endpoint `PUT /api/settings`
- [ ] Validate settings

## UI settings

- [ ] Provider selection
- [ ] OpenAI-compatible base URL
- [ ] API key input
- [ ] OpenAI-compatible model
- [ ] Ollama base URL
- [ ] Ollama model
- [ ] Default build timeout
- [ ] Max debug retries
- [ ] Approval mode default

## Security

- [ ] API key не показывать полностью
- [ ] API key redaction
- [ ] Test connection button
- [ ] Не логировать secrets

### Acceptance Criteria

- [ ] Пользователь настраивает provider
- [ ] Настройки сохраняются
- [ ] Fake provider доступен всегда
- [ ] Secrets не видны в logs

---

# Milestone 19 — Security Hardening

### Цель
Закрыть основные риски MVP.

### Checklist

## Filesystem security

- [ ] Path traversal protection
- [ ] Workspace allowlist
- [ ] Запрет доступа вне repo
- [ ] Запрет системных директорий
- [ ] Запрет чтения `.ssh`
- [ ] Запрет чтения `.aws`
- [ ] Controlled access to `.env`

## Prompt security

- [ ] Secret redaction before LLM
- [ ] Prompt injection warning detection
- [ ] Ignore untrusted repo instructions by default
- [ ] Mark external docs as untrusted
- [ ] Log model decisions

## Command security

- [ ] Dangerous command blocklist
- [ ] Command allowlist
- [ ] Timeout all commands
- [ ] Max output size
- [ ] No auto push
- [ ] No auto deploy

## Audit

- [ ] Audit all file writes
- [ ] Audit all command executions
- [ ] Audit all git operations
- [ ] Audit all approval decisions
- [ ] Audit all LLM calls

### Acceptance Criteria

- [ ] Unsafe commands blocked
- [ ] Secrets redacted
- [ ] File access outside repo blocked
- [ ] Audit trail complete

---

# Milestone 20 — Reliability & Recovery

### Цель
Сделать MVP устойчивым к сбоям.

### Checklist

## Checkpoints

- [ ] Checkpoint после каждого workflow step
- [ ] Save step inputs
- [ ] Save step outputs
- [ ] Save artifacts
- [ ] Save errors

## Recovery

- [ ] App restart сохраняет workflows
- [ ] Failed workflow можно открыть
- [ ] Failed workflow показывает причину
- [ ] Cancelled workflow сохраняет logs
- [ ] Pending approval сохраняется после restart

## Retry policies

- [ ] Retry LLM transient errors
- [ ] Retry tool transient errors
- [ ] No retry for validation errors
- [ ] Exponential backoff
- [ ] Max retry count

## Rollback

- [ ] Rollback unapplied patch
- [ ] Rollback uncommitted changes
- [ ] Rollback rejected workflow
- [ ] Preserve failure artifacts

### Acceptance Criteria

- [ ] Restart не теряет состояние
- [ ] Failed workflow диагностируем
- [ ] Rollback работает
- [ ] Retry не зацикливается

---

# Milestone 21 — Testing

### Цель
Покрыть критичную логику тестами.

### Checklist

## Core tests

- [ ] State transitions
- [ ] Entity validation
- [ ] Value objects
- [ ] Approval status transitions

## Tools tests

- [ ] ProjectScannerTool
- [ ] FileSystemTool workspace boundary
- [ ] CodePatchTool apply valid patch
- [ ] CodePatchTool reject invalid patch
- [ ] BuildTool timeout
- [ ] GitTool diff
- [ ] GitTool commit
- [ ] Secret redaction

## Agents tests

- [ ] TaskPlannerAgent with fake LLM
- [ ] BackendDeveloperAgent parses structured output
- [ ] DebuggingAgent handles build error
- [ ] DocumentationAgent produces artifact

## Workflow tests

- [ ] Happy path workflow
- [ ] Build failure debug loop
- [ ] Max retries failure
- [ ] Approval approve path
- [ ] Approval reject path
- [ ] Cancellation path

### Acceptance Criteria

- [ ] `dotnet test` проходит
- [ ] Critical path покрыт тестами
- [ ] Fake LLM используется для deterministic tests
- [ ] Tests не требуют реального API key

---

# Milestone 22 — Documentation & Demo

### Цель
Подготовить проект к использованию и демонстрации.

### Checklist

## README

- [ ] Что такое MAACO
- [ ] Архитектура
- [ ] Требования
- [ ] Как запустить backend
- [ ] Как запустить UI
- [ ] Как настроить OpenAI-compatible provider
- [ ] Как настроить Ollama
- [ ] Как использовать Fake provider
- [ ] Как добавить repo
- [ ] Как создать task
- [ ] Как approve/reject changes

## Developer docs

- [ ] Solution structure
- [ ] Domain model overview
- [ ] Agent runtime overview
- [ ] Tooling architecture
- [ ] Workflow pipeline
- [ ] Security model
- [ ] Testing guide

## Demo

- [ ] Добавить sample project
- [ ] Добавить demo task
- [ ] Добавить screenshots или описание flow
- [ ] Добавить troubleshooting section

### Acceptance Criteria

- [ ] Новый разработчик может запустить MAACO по README
- [ ] Есть понятный demo flow
- [ ] Документация соответствует реализации

---

# Milestone 23 — MVP Release Hardening

### Цель
Подготовить первый стабильный MVP build.

### Checklist

## Build

- [ ] Проверить clean build
- [ ] Проверить tests
- [ ] Проверить migrations
- [ ] Проверить app startup
- [ ] Проверить logs directory
- [ ] Проверить config templates

## Packaging

- [ ] Publish backend
- [ ] Publish Avalonia app
- [ ] Создать release notes
- [ ] Создать known limitations
- [ ] Создать `.env.example` или `appsettings.example.json`

## Manual QA

- [ ] Add repo
- [ ] Scan repo
- [ ] Create task
- [ ] Run workflow
- [ ] Generate diff
- [ ] Approve commit
- [ ] Reject rollback
- [ ] Restart app during pending approval
- [ ] Run with Fake provider
- [ ] Run with Ollama provider

### Acceptance Criteria

- [ ] MVP можно собрать и запустить локально
- [ ] Основной SDLC pipeline работает
- [ ] Нет блокирующих ошибок
- [ ] Все non-goals явно задокументированы

---

# Definition of Done для каждого milestone

- [ ] Код реализован
- [ ] Код отформатирован
- [ ] Нет compiler errors
- [ ] Нет critical warnings
- [ ] Добавлены тесты, если применимо
- [ ] Обновлена документация, если применимо
- [ ] Логи не содержат secrets
- [ ] `dotnet build` проходит
- [ ] `dotnet test` проходит
- [ ] Изменения можно объяснить через README или комментарии

---

# Рекомендуемый порядок реализации

1. Milestone 0 — Bootstrap
2. Milestone 1 — Core Domain
3. Milestone 2 — Persistence
4. Milestone 3 — Backend API
5. Milestone 4 — Realtime
6. Milestone 5 — Project Scanner
7. Milestone 6 — Tools
8. Milestone 7 — Sandbox
9. Milestone 8 — Git
10. Milestone 9 — LLM Gateway
11. Milestone 10 — Agents
12. Milestone 11 — Orchestrator
13. Milestone 12 — Memory
14. Milestone 13 — Build/Test Loop
15. Milestone 14 — UI Shell
16. Milestone 15 — UI Project/Task Flow
17. Milestone 16 — UI Workflow Monitor
18. Milestone 17 — Diff Review
19. Milestone 18 — Settings
20. Milestone 19 — Security
21. Milestone 20 — Reliability
22. Milestone 21 — Testing
23. Milestone 22 — Documentation
24. Milestone 23 — MVP Release
