# MAACO — детальный Milestone Plan реализации MVP с чеклистом

## Milestone 0 — Bootstrap репозитория

### Цель
Создать базовую структуру проекта MAACO.

### Checklist

- [x] Создать `MAACO.sln`
- [x] Создать структуру:

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

- [x] Подключить project references
- [x] Добавить `.gitignore`
- [x] Добавить `.editorconfig`
- [x] Добавить `Directory.Build.props`
- [x] Включить nullable reference types
- [x] Включить implicit usings
- [x] Добавить базовый `README.md`
- [x] Проверить `dotnet restore`
- [x] Проверить `dotnet build`
- [x] Проверить `dotnet test`

### Acceptance Criteria

- [x] Solution собирается без ошибок
- [x] Все проекты подключены
- [x] Тестовые проекты запускаются
- [x] README описывает назначение MAACO

---

# Milestone 1 — Core Domain Model

### Цель
Реализовать доменную модель MAACO.

### Checklist

## Base types

- [x] Создать `Entity`
- [x] Создать `AuditableEntity`
- [x] Добавить `Id`
- [x] Добавить `CreatedAt`
- [x] Добавить `UpdatedAt`
- [x] Добавить `Version`
- [x] Добавить `MetadataJson`

## Enums

- [x] Создать `TaskStatus`
- [x] Создать `WorkflowStatus`
- [x] Создать `WorkflowStepStatus`
- [x] Создать `AgentStatus`
- [x] Создать `ToolExecutionStatus`
- [x] Создать `ApprovalStatus`
- [x] Создать `ApprovalMode`
- [x] Создать `ArtifactType`
- [x] Создать `MemoryRecordType`
- [x] Создать `GitOperationType`
- [x] Создать `BuildRunStatus`
- [x] Создать `LogSeverity`

## Entities

- [x] Создать `Project`
- [x] Создать `TaskItem`
- [x] Создать `Workflow`
- [x] Создать `WorkflowStep`
- [x] Создать `AgentDefinition`
- [x] Создать `ToolExecution`
- [x] Создать `LogEvent`
- [x] Создать `Artifact`
- [x] Создать `GitOperation`
- [x] Создать `BuildRun`
- [x] Создать `MemoryRecord`
- [x] Создать `ApprovalRequest`
- [x] Создать `LlmCallLog`
- [x] Создать `ProjectContextSnapshot`

## Value objects

- [x] Создать `RepositoryPath`
- [x] Создать `CommandSpec`
- [x] Создать `LlmUsage`
- [x] Создать `PatchSummary`
- [x] Создать `ExecutionResult`
- [x] Создать `DetectedProjectStack`
- [x] Создать `FileChangeSummary`

## Domain events

- [x] Создать `TaskCreatedEvent`
- [x] Создать `WorkflowStartedEvent`
- [x] Создать `WorkflowStepStartedEvent`
- [x] Создать `WorkflowStepCompletedEvent`
- [x] Создать `WorkflowStepFailedEvent`
- [x] Создать `ApprovalRequestedEvent`
- [x] Создать `WorkflowCompletedEvent`
- [x] Создать `WorkflowFailedEvent`

### Acceptance Criteria

- [x] Все entities компилируются
- [x] Все enum покрывают MVP workflow
- [x] Есть unit tests для state transitions
- [x] Невалидные переходы статусов запрещены

---

# Milestone 2 — Persistence Layer / SQLite

### Цель
Реализовать хранение данных в SQLite через EF Core.

### Checklist

## EF Core setup

- [x] Создать `MaacoDbContext`
- [x] Добавить `DbSet<Project>`
- [x] Добавить `DbSet<TaskItem>`
- [x] Добавить `DbSet<Workflow>`
- [x] Добавить `DbSet<WorkflowStep>`
- [x] Добавить `DbSet<AgentDefinition>`
- [x] Добавить `DbSet<ToolExecution>`
- [x] Добавить `DbSet<LogEvent>`
- [x] Добавить `DbSet<Artifact>`
- [x] Добавить `DbSet<GitOperation>`
- [x] Добавить `DbSet<BuildRun>`
- [x] Добавить `DbSet<MemoryRecord>`
- [x] Добавить `DbSet<ApprovalRequest>`
- [x] Добавить `DbSet<LlmCallLog>`
- [x] Добавить `DbSet<ProjectContextSnapshot>`

## Mappings

- [x] Настроить primary keys
- [x] Настроить foreign keys
- [x] Настроить enum conversion в string или int
- [x] Настроить `MetadataJson`
- [x] Настроить `Version` как concurrency token
- [x] Настроить cascade rules

## Indexes

- [x] Индекс `Projects.RepositoryPath`
- [x] Индекс `TaskItems.ProjectId`
- [x] Индекс `Workflows.TaskId`
- [x] Индекс `WorkflowSteps.WorkflowId`
- [x] Индекс `ToolExecutions.WorkflowId`
- [x] Индекс `LogEvents.WorkflowId`
- [x] Индекс `Artifacts.TaskId`
- [x] Индекс `GitOperations.TaskId`
- [x] Индекс `BuildRuns.WorkflowId`
- [x] Индекс `MemoryRecords.ProjectId`
- [x] Индекс `ApprovalRequests.Status`

## Repositories

- [x] Создать `IProjectRepository`
- [x] Создать `ITaskRepository`
- [x] Создать `IWorkflowRepository`
- [x] Создать `ILogRepository`
- [x] Создать `IMemoryRepository`
- [x] Реализовать EF repositories

## Migrations

- [x] Создать initial migration
- [x] Добавить automatic migration на старте dev-режима
- [x] Добавить seed базовых агентов
- [x] Добавить seed базовых tools

### Acceptance Criteria

- [x] SQLite база создается автоматически
- [x] Initial migration применяется
- [x] Данные сохраняются и читаются
- [x] Unit/integration tests проходят

---

# Milestone 3 — Backend API Skeleton

### Цель
Создать Web API MAACO и базовые endpoints.

### Checklist

## API setup

- [x] Настроить ASP.NET Core Web API
- [x] Подключить DI
- [x] Подключить EF Core
- [x] Подключить Serilog
- [x] Подключить OpenTelemetry
- [x] Подключить Swagger
- [x] Настроить CORS для desktop UI
- [x] Добавить global exception middleware

## Endpoints Projects

- [x] `POST /api/projects`
- [x] `GET /api/projects`
- [x] `GET /api/projects/{id}`
- [x] `POST /api/projects/{id}/scan`

## Endpoints Tasks

- [x] `POST /api/tasks`
- [x] `GET /api/tasks`
- [x] `GET /api/tasks/{id}`
- [x] `POST /api/tasks/{id}/cancel`

## Endpoints Workflows

- [x] `GET /api/workflows/{id}`
- [x] `GET /api/workflows/{id}/steps`
- [x] `GET /api/workflows/{id}/logs`
- [x] `GET /api/workflows/{id}/artifacts`

## Endpoints Approvals

- [x] `GET /api/approvals/pending`
- [x] `POST /api/approvals/{id}/approve`
- [x] `POST /api/approvals/{id}/reject`

## Endpoints Git/Diff

- [x] `GET /api/tasks/{id}/diff`
- [x] `POST /api/tasks/{id}/commit`
- [x] `POST /api/tasks/{id}/rollback`

## Endpoints Settings

- [x] `GET /api/settings`
- [x] `PUT /api/settings`

### Acceptance Criteria

- [x] Swagger открывается
- [x] Все endpoints возвращают корректные DTO
- [x] Ошибки возвращаются в едином формате
- [x] API работает с SQLite

---

# Milestone 4 — Realtime Event Bus + SignalR

### Цель
Реализовать realtime обновления workflow для UI.

### Checklist

## Internal event bus

- [x] Создать `IEventBus`
- [x] Реализовать in-memory event bus
- [x] Добавить event handlers
- [x] Добавить persistence важных событий в `LogEvent`
- [x] Добавить correlation id
- [x] Добавить workflow id в события

## SignalR

- [x] Создать `WorkflowHub`
- [x] Добавить подключение `/workflowHub`
- [x] Реализовать group per workflow
- [x] Реализовать group per project
- [x] Публиковать `WorkflowStarted`
- [x] Публиковать `StepStarted`
- [x] Публиковать `StepCompleted`
- [x] Публиковать `StepFailed`
- [x] Публиковать `LogReceived`
- [x] Публиковать `ToolExecutionStarted`
- [x] Публиковать `ToolExecutionCompleted`
- [x] Публиковать `ApprovalRequested`
- [x] Публиковать `WorkflowCompleted`
- [x] Публиковать `WorkflowFailed`

### Acceptance Criteria

- [x] UI/тестовый клиент получает realtime события
- [x] События сохраняются в логах
- [x] События имеют correlation id
- [x] Workflow status обновляется корректно

---

# Milestone 5 — Project Scanner

### Цель
Научить MAACO анализировать локальный репозиторий.

### Checklist

## Repository validation

- [x] Проверять существование пути
- [x] Проверять доступность чтения
- [x] Проверять наличие `.git`
- [x] Запрещать системные директории
- [x] Нормализовать path

## File scanning

- [x] Игнорировать `.git`
- [x] Игнорировать `bin`
- [x] Игнорировать `obj`
- [x] Игнорировать `node_modules`
- [x] Игнорировать `.vs`
- [x] Игнорировать `.idea`
- [x] Игнорировать build artifacts
- [x] Ограничить максимальный размер файла
- [x] Ограничить максимальное число файлов

## Stack detection

- [x] Детектировать `.NET`
- [x] Детектировать `Node.js`
- [x] Детектировать `Python`
- [x] Детектировать generic project
- [x] Находить solution/project files
- [x] Находить package manifests

## Build/test command detection

- [x] Для `.NET`: `dotnet build`
- [x] Для `.NET`: `dotnet test`
- [x] Для Node.js: `npm test` или script из `package.json`
- [x] Для Python: `pytest`
- [x] Позволить override команд в settings

## Context snapshot

- [x] Создавать `ProjectContextSnapshot`
- [x] Сохранять список ключевых файлов
- [x] Сохранять detected stack
- [x] Сохранять build/test commands
- [x] Сохранять project summary

### Acceptance Criteria

- [x] Локальный repo добавляется
- [x] Project scan сохраняется в БД
- [x] Stack определяется корректно
- [x] Build/test команды предлагаются автоматически

---

# Milestone 6 — Tooling Core

### Цель
Реализовать plugin-like tools layer.

### Checklist

## Tool abstractions

- [x] Создать `IAgentTool`
- [x] Создать `ToolRequest`
- [x] Создать `ToolResult`
- [x] Создать `ToolPermission`
- [x] Создать `ToolRegistry`
- [x] Добавить DI registration tools
- [x] Добавить tool execution logging
- [x] Добавить timeout support
- [x] Добавить cancellation support

## Required tools

- [x] `FileSystemTool`
- [x] `ProjectScannerTool`
- [x] `CodePatchTool`
- [x] `BuildTool`
- [x] `TestTool`
- [x] `GitTool`
- [x] `DiffTool`
- [x] `LogAnalysisTool`

## Tool safety

- [x] Проверка permissions
- [x] Проверка workspace boundary
- [x] Логирование input/output
- [x] Redaction secrets
- [x] Max output length
- [x] Error normalization

### Acceptance Criteria

- [x] Tools вызываются через единый интерфейс
- [x] Все tool executions сохраняются
- [x] Tool failures не валят процесс без controlled error
- [x] Workspace boundary работает

---

# Milestone 7 — Sandbox Execution

### Цель
Реализовать безопасное выполнение команд.

### Checklist

## Sandbox abstractions

- [x] Создать `ISandboxExecutor`
- [x] Создать `SandboxRequest`
- [x] Создать `SandboxResult`
- [x] Создать `SandboxOptions`

## Local sandbox

- [x] Выполнение процесса в project directory
- [x] Timeout
- [x] CancellationToken
- [x] Capture stdout
- [x] Capture stderr
- [x] Capture exit code
- [x] Environment filtering
- [x] Working directory validation

## Command safety

- [x] Command allowlist для MVP
- [x] Blocklist опасных команд
- [x] Запрет доступа вне workspace
- [x] Запрет `.ssh`
- [x] Запрет `.aws`
- [x] Запрет системных путей
- [x] Redaction `.env` значений

## Docker abstraction

- [x] Создать Docker sandbox interface
- [x] Добавить базовый stub
- [x] Подготовить Docker.DotNet integration point

### Acceptance Criteria

- [x] Build/test команды выполняются
- [x] Timeout работает
- [x] Cancellation работает
- [x] Dangerous commands блокируются
- [x] Stdout/stderr сохраняются

---

# Milestone 8 — Git Integration

### Цель
Реализовать безопасный Git workflow.

### Checklist

## Git operations

- [x] Проверить, что repo является git repo
- [x] Получать current branch
- [x] Получать status
- [x] Создавать feature branch
- [x] Получать diff
- [x] Получать changed files
- [x] Создавать commit
- [x] Откатывать uncommitted changes
- [x] Генерировать patch artifact

## Safety

- [x] Не делать push в MVP
- [x] Не делать force reset без approval
- [x] Commit только после approval
- [x] Rollback только изменений MAACO
- [x] Сохранять `GitOperation`

## Branch naming

- [x] Генерировать branch name `maaco/task-{shortId}-{slug}`
- [x] Проверять конфликт имен
- [x] Создавать новый branch при необходимости

### Acceptance Criteria

- [x] Diff генерируется
- [x] Commit создается после approval
- [x] Reject откатывает изменения
- [x] Git operations сохраняются в БД

---

# Milestone 9 — LLM Gateway

### Цель
Реализовать LLM abstraction и провайдеры.

### Checklist

## Abstractions

- [x] Создать `ILlmProvider`
- [x] Создать `ILlmGateway`
- [x] Создать `LlmRequest`
- [x] Создать `LlmResponse`
- [x] Создать `LlmMessage`
- [x] Создать `LlmProviderOptions`
- [x] Создать `ModelRoutingPolicy`

## Providers

- [x] Реализовать `FakeLlmProvider`
- [x] Реализовать OpenAI-compatible provider
- [x] Реализовать Ollama provider
- [x] Добавить provider health check
- [x] Добавить retry transient errors
- [x] Добавить timeout

## Logging/cost

- [x] Логировать prompt с redaction
- [x] Логировать response с redaction
- [x] Сохранять `LlmCallLog`
- [x] Оценивать input tokens
- [x] Оценивать output tokens
- [x] Считать estimated cost

## Routing

- [x] Planning → configured strong model
- [x] Coding → configured strong model
- [x] Debugging → configured strong model
- [x] Summary/log analysis → cheap/local model if available
- [x] Fallback на default provider

### Acceptance Criteria

- [x] Fake provider работает без API key
- [x] OpenAI-compatible provider вызывается через config
- [x] Ollama provider вызывается через config
- [x] Все LLM calls сохраняются
- [x] Secrets не попадают в logs

---

# Milestone 10 — Agent Runtime

### Цель
Реализовать базовую агентную систему.

### Checklist

## Base abstractions

- [x] Создать `IAgent`
- [x] Создать `AgentContext`
- [x] Создать `AgentResult`
- [x] Создать `AgentCapability`
- [x] Создать `AgentRegistry`
- [x] Создать `AgentExecutionService`

## Agents

- [x] `OrchestratorAgent`
- [x] `TaskPlannerAgent`
- [x] `BackendDeveloperAgent`
- [x] `TestWriterAgent`
- [x] `DebuggingAgent`
- [x] `GitManagerAgent`
- [x] `DocumentationAgent`

## Agent constraints

- [x] Agents не пишут файлы напрямую
- [x] Agents используют tools
- [x] Agents получают scoped context
- [x] Agents логируют решения
- [x] Agents возвращают structured output
- [x] Agents поддерживают cancellation

## Prompts

- [x] Создать system prompt для Planner
- [x] Создать system prompt для Developer
- [x] Создать system prompt для Test Writer
- [x] Создать system prompt для Debugging
- [x] Создать system prompt для Documentation
- [x] Добавить response schema expectations

### Acceptance Criteria

- [x] Agent registry работает
- [x] Agents вызываются через runtime
- [x] Fake LLM позволяет прогнать demo workflow
- [x] Agent outputs сохраняются как artifacts/logs

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

- [x] Создать `IWorkflowOrchestrator`
- [x] Создать `WorkflowExecutionContext`
- [x] Создать `WorkflowStepExecutor`
- [x] Создать checkpoint после каждого step
- [x] Сохранять status workflow
- [x] Сохранять status step
- [x] Публиковать realtime events
- [x] Поддержать cancellation

## Step execution

- [x] ProjectScanStep
- [x] GitBranchStep
- [x] PlanningStep
- [x] CodeGenerationStep
- [x] PatchApplicationStep
- [x] TestGenerationStep
- [x] BuildStep
- [x] TestStep
- [x] DebugStep
- [x] DiffStep
- [x] ApprovalStep
- [x] CommitStep
- [x] RollbackStep
- [x] DocumentationStep
- [x] FinalReportStep

## Retry/debug loop

- [x] Max debug retries default 3
- [x] Build failure triggers DebuggingAgent
- [x] Test failure triggers DebuggingAgent
- [x] Debug patch applied through CodePatchTool
- [x] Stop after max retries
- [x] Mark workflow failed with logs

### Acceptance Criteria

- [x] Workflow создается из task
- [x] Steps выполняются последовательно
- [x] State сохраняется после каждого step
- [x] Failure не теряет контекст
- [x] Debug loop работает
- [x] Workflow можно cancel

---

# Milestone 12 — Memory / Context MVP

### Цель
Реализовать базовую память проекта.

### Checklist

## Memory service

- [x] Создать `IMemoryService`
- [x] Сохранять `ProjectSummary`
- [x] Сохранять `TaskSummary`
- [x] Сохранять `FileSummary`
- [x] Сохранять `BuildFailure`
- [x] Сохранять `Decision`
- [x] Сохранять `AgentNote`

## Retrieval

- [x] Получать memory по project id
- [x] Получать memory по task id
- [x] Keyword search
- [x] Limit top N records
- [x] Exclude stale records by status
- [x] Context assembly для agents

## Future-ready fields

- [x] `EmbeddingProvider`
- [x] `EmbeddingModel`
- [x] `EmbeddingHash`
- [x] `VectorRef`
- [x] `ContentHash`

### Acceptance Criteria

- [x] Agents получают project context
- [x] Build/test failures сохраняются
- [x] Previous task summaries доступны
- [x] Schema готова к vector search

---

# Milestone 13 — Build/Test Loop

### Цель
Довести автоматический цикл сборки, тестов и исправлений.

### Checklist

## Build

- [x] Выполнять detected build command
- [x] Сохранять `BuildRun`
- [x] Сохранять stdout/stderr artifact
- [x] Парсить exit code
- [x] Публиковать events

## Test

- [x] Выполнять detected test command
- [x] Сохранять test output
- [x] Определять failed tests
- [x] Публиковать events

## Log analysis

- [x] Извлекать compiler errors
- [x] Извлекать stack traces
- [x] Извлекать failed assertions
- [x] Передавать summary DebuggingAgent

## Debug cycle

- [x] Сформировать debug prompt
- [x] Получить patch
- [x] Validate patch
- [x] Apply patch
- [x] Повторить build/test
- [x] Остановиться при success/max retries

### Acceptance Criteria

- [x] Ошибка сборки приводит к debug iteration
- [x] Ошибка тестов приводит к debug iteration
- [x] Все итерации видны в logs
- [x] После success workflow идет к diff review

---

# Milestone 14 — Avalonia UI Shell

### Цель
Создать desktop UI MAACO.

### Checklist

## App shell

- [x] Создать Avalonia app
- [x] Настроить MVVM
- [x] Настроить DI
- [x] Настроить navigation service
- [x] Настроить API client
- [x] Настроить SignalR client

## Screens

- [x] Dashboard
- [x] Projects Screen
- [x] Task Creation Screen
- [x] Workflow Monitor
- [x] Diff Review Screen
- [x] Settings Screen
- [x] Logs Screen

## Layout

- [x] Sidebar navigation
- [x] Main content area
- [x] Status bar
- [x] Notifications/toasts
- [x] Loading/error states

### Acceptance Criteria

- [x] App запускается
- [x] Навигация работает
- [x] API client подключается к backend
- [x] SignalR получает events
- [x] Ошибки отображаются пользователю

---

# Milestone 15 — UI Project + Task Flow

### Цель
Дать пользователю возможность добавить repo и создать задачу.

### Checklist

## Projects UI

- [x] Выбрать folder
- [x] Добавить проект
- [x] Запустить scan
- [x] Показать detected stack
- [x] Показать build/test commands
- [x] Показать project files summary

## Task UI

- [x] Выбрать project
- [x] Ввести title
- [x] Ввести description
- [x] Ввести constraints
- [x] Выбрать approval mode
- [x] Запустить task
- [x] Перейти в workflow monitor

### Acceptance Criteria

- [x] Пользователь добавляет local repo через UI
- [x] Пользователь создает task через UI
- [x] Workflow стартует
- [x] UI показывает начальный статус workflow

---

# Milestone 16 — UI Workflow Monitor

### Цель
Визуализировать выполнение pipeline.

### Checklist

## Workflow timeline

- [x] Показать список steps
- [x] Показать текущий step
- [x] Показать statuses
- [x] Показать duration
- [x] Показать retry count

## Live logs

- [x] Stream logs через SignalR
- [x] Фильтр по severity
- [x] Фильтр по agent
- [x] Фильтр по tool
- [x] Copy log line
- [x] Save logs view

## Agent activity

- [x] Показывать активного agent
- [x] Показывать tool executions
- [x] Показывать LLM call status
- [x] Показывать token estimate

### Acceptance Criteria

- [x] Workflow progress виден realtime
- [x] Логи обновляются без refresh
- [x] Ошибки явно подсвечены
- [x] Пользователь понимает текущий этап

---

# Milestone 17 — Diff Review + Approval UI

### Цель
Реализовать human-in-the-loop approval.

### Checklist

## Diff UI

- [x] Получить diff по task
- [x] Показать changed files
- [x] Показать unified diff
- [x] Подсветить added/removed lines
- [x] Показать generated commit message
- [x] Позволить изменить commit message

## Approval

- [x] Approve button
- [x] Reject button
- [x] Reject with reason
- [x] Approve вызывает commit
- [x] Reject вызывает rollback
- [x] Показать результат операции

### Acceptance Criteria

- [x] Workflow останавливается на approval
- [x] Пользователь видит diff
- [x] Approve создает commit
- [x] Reject откатывает изменения
- [x] Результат виден в UI

---

# Milestone 18 — Settings & Configuration

### Цель
Реализовать настройки MAACO.

### Checklist

## Backend settings

- [x] Хранить настройки в appsettings + DB override
- [x] Endpoint `GET /api/settings`
- [x] Endpoint `PUT /api/settings`
- [x] Validate settings

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




























