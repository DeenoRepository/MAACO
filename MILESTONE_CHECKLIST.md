# MAACO вЂ” РґРµС‚Р°Р»СЊРЅС‹Р№ Milestone Plan СЂРµР°Р»РёР·Р°С†РёРё MVP СЃ С‡РµРєР»РёСЃС‚РѕРј

## Milestone 0 вЂ” Bootstrap СЂРµРїРѕР·РёС‚РѕСЂРёСЏ

### Р¦РµР»СЊ
РЎРѕР·РґР°С‚СЊ Р±Р°Р·РѕРІСѓСЋ СЃС‚СЂСѓРєС‚СѓСЂСѓ РїСЂРѕРµРєС‚Р° MAACO.

### Checklist

- [x] РЎРѕР·РґР°С‚СЊ `MAACO.sln`
- [x] РЎРѕР·РґР°С‚СЊ СЃС‚СЂСѓРєС‚СѓСЂСѓ:

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

- [x] РџРѕРґРєР»СЋС‡РёС‚СЊ project references
- [x] Р”РѕР±Р°РІРёС‚СЊ `.gitignore`
- [x] Р”РѕР±Р°РІРёС‚СЊ `.editorconfig`
- [x] Р”РѕР±Р°РІРёС‚СЊ `Directory.Build.props`
- [x] Р’РєР»СЋС‡РёС‚СЊ nullable reference types
- [x] Р’РєР»СЋС‡РёС‚СЊ implicit usings
- [x] Р”РѕР±Р°РІРёС‚СЊ Р±Р°Р·РѕРІС‹Р№ `README.md`
- [x] РџСЂРѕРІРµСЂРёС‚СЊ `dotnet restore`
- [x] РџСЂРѕРІРµСЂРёС‚СЊ `dotnet build`
- [x] РџСЂРѕРІРµСЂРёС‚СЊ `dotnet test`

### Acceptance Criteria

- [x] Solution СЃРѕР±РёСЂР°РµС‚СЃСЏ Р±РµР· РѕС€РёР±РѕРє
- [x] Р’СЃРµ РїСЂРѕРµРєС‚С‹ РїРѕРґРєР»СЋС‡РµРЅС‹
- [x] РўРµСЃС‚РѕРІС‹Рµ РїСЂРѕРµРєС‚С‹ Р·Р°РїСѓСЃРєР°СЋС‚СЃСЏ
- [x] README РѕРїРёСЃС‹РІР°РµС‚ РЅР°Р·РЅР°С‡РµРЅРёРµ MAACO

---

# Milestone 1 вЂ” Core Domain Model

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ РґРѕРјРµРЅРЅСѓСЋ РјРѕРґРµР»СЊ MAACO.

### Checklist

## Base types

- [x] РЎРѕР·РґР°С‚СЊ `Entity`
- [x] РЎРѕР·РґР°С‚СЊ `AuditableEntity`
- [x] Р”РѕР±Р°РІРёС‚СЊ `Id`
- [x] Р”РѕР±Р°РІРёС‚СЊ `CreatedAt`
- [x] Р”РѕР±Р°РІРёС‚СЊ `UpdatedAt`
- [x] Р”РѕР±Р°РІРёС‚СЊ `Version`
- [x] Р”РѕР±Р°РІРёС‚СЊ `MetadataJson`

## Enums

- [x] РЎРѕР·РґР°С‚СЊ `TaskStatus`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowStatus`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowStepStatus`
- [x] РЎРѕР·РґР°С‚СЊ `AgentStatus`
- [x] РЎРѕР·РґР°С‚СЊ `ToolExecutionStatus`
- [x] РЎРѕР·РґР°С‚СЊ `ApprovalStatus`
- [x] РЎРѕР·РґР°С‚СЊ `ApprovalMode`
- [x] РЎРѕР·РґР°С‚СЊ `ArtifactType`
- [x] РЎРѕР·РґР°С‚СЊ `MemoryRecordType`
- [x] РЎРѕР·РґР°С‚СЊ `GitOperationType`
- [x] РЎРѕР·РґР°С‚СЊ `BuildRunStatus`
- [x] РЎРѕР·РґР°С‚СЊ `LogSeverity`

## Entities

- [x] РЎРѕР·РґР°С‚СЊ `Project`
- [x] РЎРѕР·РґР°С‚СЊ `TaskItem`
- [x] РЎРѕР·РґР°С‚СЊ `Workflow`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowStep`
- [x] РЎРѕР·РґР°С‚СЊ `AgentDefinition`
- [x] РЎРѕР·РґР°С‚СЊ `ToolExecution`
- [x] РЎРѕР·РґР°С‚СЊ `LogEvent`
- [x] РЎРѕР·РґР°С‚СЊ `Artifact`
- [x] РЎРѕР·РґР°С‚СЊ `GitOperation`
- [x] РЎРѕР·РґР°С‚СЊ `BuildRun`
- [x] РЎРѕР·РґР°С‚СЊ `MemoryRecord`
- [x] РЎРѕР·РґР°С‚СЊ `ApprovalRequest`
- [x] РЎРѕР·РґР°С‚СЊ `LlmCallLog`
- [x] РЎРѕР·РґР°С‚СЊ `ProjectContextSnapshot`

## Value objects

- [x] РЎРѕР·РґР°С‚СЊ `RepositoryPath`
- [x] РЎРѕР·РґР°С‚СЊ `CommandSpec`
- [x] РЎРѕР·РґР°С‚СЊ `LlmUsage`
- [x] РЎРѕР·РґР°С‚СЊ `PatchSummary`
- [x] РЎРѕР·РґР°С‚СЊ `ExecutionResult`
- [x] РЎРѕР·РґР°С‚СЊ `DetectedProjectStack`
- [x] РЎРѕР·РґР°С‚СЊ `FileChangeSummary`

## Domain events

- [x] РЎРѕР·РґР°С‚СЊ `TaskCreatedEvent`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowStartedEvent`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowStepStartedEvent`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowStepCompletedEvent`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowStepFailedEvent`
- [x] РЎРѕР·РґР°С‚СЊ `ApprovalRequestedEvent`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowCompletedEvent`
- [x] РЎРѕР·РґР°С‚СЊ `WorkflowFailedEvent`

### Acceptance Criteria

- [x] Р’СЃРµ entities РєРѕРјРїРёР»РёСЂСѓСЋС‚СЃСЏ
- [x] Р’СЃРµ enum РїРѕРєСЂС‹РІР°СЋС‚ MVP workflow
- [x] Р•СЃС‚СЊ unit tests РґР»СЏ state transitions
- [ ] РќРµРІР°Р»РёРґРЅС‹Рµ РїРµСЂРµС…РѕРґС‹ СЃС‚Р°С‚СѓСЃРѕРІ Р·Р°РїСЂРµС‰РµРЅС‹

---

# Milestone 2 вЂ” Persistence Layer / SQLite

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ С…СЂР°РЅРµРЅРёРµ РґР°РЅРЅС‹С… РІ SQLite С‡РµСЂРµР· EF Core.

### Checklist

## EF Core setup

- [x] РЎРѕР·РґР°С‚СЊ `MaacoDbContext`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<Project>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<TaskItem>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<Workflow>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<WorkflowStep>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<AgentDefinition>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<ToolExecution>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<LogEvent>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<Artifact>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<GitOperation>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<BuildRun>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<MemoryRecord>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<ApprovalRequest>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<LlmCallLog>`
- [x] Р”РѕР±Р°РІРёС‚СЊ `DbSet<ProjectContextSnapshot>`

## Mappings

- [x] РќР°СЃС‚СЂРѕРёС‚СЊ primary keys
- [x] РќР°СЃС‚СЂРѕРёС‚СЊ foreign keys
- [x] РќР°СЃС‚СЂРѕРёС‚СЊ enum conversion РІ string РёР»Рё int
- [x] РќР°СЃС‚СЂРѕРёС‚СЊ `MetadataJson`
- [x] РќР°СЃС‚СЂРѕРёС‚СЊ `Version` РєР°Рє concurrency token
- [x] РќР°СЃС‚СЂРѕРёС‚СЊ cascade rules

## Indexes

- [x] РРЅРґРµРєСЃ `Projects.RepositoryPath`
- [x] РРЅРґРµРєСЃ `TaskItems.ProjectId`
- [x] РРЅРґРµРєСЃ `Workflows.TaskId`
- [x] РРЅРґРµРєСЃ `WorkflowSteps.WorkflowId`
- [x] РРЅРґРµРєСЃ `ToolExecutions.WorkflowId`
- [x] РРЅРґРµРєСЃ `LogEvents.WorkflowId`
- [x] РРЅРґРµРєСЃ `Artifacts.TaskId`
- [x] РРЅРґРµРєСЃ `GitOperations.TaskId`
- [x] РРЅРґРµРєСЃ `BuildRuns.WorkflowId`
- [x] РРЅРґРµРєСЃ `MemoryRecords.ProjectId`
- [x] РРЅРґРµРєСЃ `ApprovalRequests.Status`

## Repositories

- [x] РЎРѕР·РґР°С‚СЊ `IProjectRepository`
- [x] РЎРѕР·РґР°С‚СЊ `ITaskRepository`
- [x] РЎРѕР·РґР°С‚СЊ `IWorkflowRepository`
- [x] РЎРѕР·РґР°С‚СЊ `ILogRepository`
- [x] РЎРѕР·РґР°С‚СЊ `IMemoryRepository`
- [x] Р РµР°Р»РёР·РѕРІР°С‚СЊ EF repositories

## Migrations

- [ ] РЎРѕР·РґР°С‚СЊ initial migration
- [ ] Р”РѕР±Р°РІРёС‚СЊ automatic migration РЅР° СЃС‚Р°СЂС‚Рµ dev-СЂРµР¶РёРјР°
- [ ] Р”РѕР±Р°РІРёС‚СЊ seed Р±Р°Р·РѕРІС‹С… Р°РіРµРЅС‚РѕРІ
- [ ] Р”РѕР±Р°РІРёС‚СЊ seed Р±Р°Р·РѕРІС‹С… tools

### Acceptance Criteria

- [ ] SQLite Р±Р°Р·Р° СЃРѕР·РґР°РµС‚СЃСЏ Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРё
- [ ] Initial migration РїСЂРёРјРµРЅСЏРµС‚СЃСЏ
- [ ] Р”Р°РЅРЅС‹Рµ СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ Рё С‡РёС‚Р°СЋС‚СЃСЏ
- [ ] Unit/integration tests РїСЂРѕС…РѕРґСЏС‚

---

# Milestone 3 вЂ” Backend API Skeleton

### Р¦РµР»СЊ
РЎРѕР·РґР°С‚СЊ Web API MAACO Рё Р±Р°Р·РѕРІС‹Рµ endpoints.

### Checklist

## API setup

- [ ] РќР°СЃС‚СЂРѕРёС‚СЊ ASP.NET Core Web API
- [ ] РџРѕРґРєР»СЋС‡РёС‚СЊ DI
- [ ] РџРѕРґРєР»СЋС‡РёС‚СЊ EF Core
- [ ] РџРѕРґРєР»СЋС‡РёС‚СЊ Serilog
- [ ] РџРѕРґРєР»СЋС‡РёС‚СЊ OpenTelemetry
- [ ] РџРѕРґРєР»СЋС‡РёС‚СЊ Swagger
- [ ] РќР°СЃС‚СЂРѕРёС‚СЊ CORS РґР»СЏ desktop UI
- [ ] Р”РѕР±Р°РІРёС‚СЊ global exception middleware

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

- [ ] Swagger РѕС‚РєСЂС‹РІР°РµС‚СЃСЏ
- [ ] Р’СЃРµ endpoints РІРѕР·РІСЂР°С‰Р°СЋС‚ РєРѕСЂСЂРµРєС‚РЅС‹Рµ DTO
- [ ] РћС€РёР±РєРё РІРѕР·РІСЂР°С‰Р°СЋС‚СЃСЏ РІ РµРґРёРЅРѕРј С„РѕСЂРјР°С‚Рµ
- [ ] API СЂР°Р±РѕС‚Р°РµС‚ СЃ SQLite

---

# Milestone 4 вЂ” Realtime Event Bus + SignalR

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ realtime РѕР±РЅРѕРІР»РµРЅРёСЏ workflow РґР»СЏ UI.

### Checklist

## Internal event bus

- [ ] РЎРѕР·РґР°С‚СЊ `IEventBus`
- [ ] Р РµР°Р»РёР·РѕРІР°С‚СЊ in-memory event bus
- [ ] Р”РѕР±Р°РІРёС‚СЊ event handlers
- [ ] Р”РѕР±Р°РІРёС‚СЊ persistence РІР°Р¶РЅС‹С… СЃРѕР±С‹С‚РёР№ РІ `LogEvent`
- [ ] Р”РѕР±Р°РІРёС‚СЊ correlation id
- [ ] Р”РѕР±Р°РІРёС‚СЊ workflow id РІ СЃРѕР±С‹С‚РёСЏ

## SignalR

- [ ] РЎРѕР·РґР°С‚СЊ `WorkflowHub`
- [ ] Р”РѕР±Р°РІРёС‚СЊ РїРѕРґРєР»СЋС‡РµРЅРёРµ `/workflowHub`
- [ ] Р РµР°Р»РёР·РѕРІР°С‚СЊ group per workflow
- [ ] Р РµР°Р»РёР·РѕРІР°С‚СЊ group per project
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `WorkflowStarted`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `StepStarted`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `StepCompleted`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `StepFailed`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `LogReceived`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `ToolExecutionStarted`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `ToolExecutionCompleted`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `ApprovalRequested`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `WorkflowCompleted`
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ `WorkflowFailed`

### Acceptance Criteria

- [ ] UI/С‚РµСЃС‚РѕРІС‹Р№ РєР»РёРµРЅС‚ РїРѕР»СѓС‡Р°РµС‚ realtime СЃРѕР±С‹С‚РёСЏ
- [ ] РЎРѕР±С‹С‚РёСЏ СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ РІ Р»РѕРіР°С…
- [ ] РЎРѕР±С‹С‚РёСЏ РёРјРµСЋС‚ correlation id
- [ ] Workflow status РѕР±РЅРѕРІР»СЏРµС‚СЃСЏ РєРѕСЂСЂРµРєС‚РЅРѕ

---

# Milestone 5 вЂ” Project Scanner

### Р¦РµР»СЊ
РќР°СѓС‡РёС‚СЊ MAACO Р°РЅР°Р»РёР·РёСЂРѕРІР°С‚СЊ Р»РѕРєР°Р»СЊРЅС‹Р№ СЂРµРїРѕР·РёС‚РѕСЂРёР№.

### Checklist

## Repository validation

- [ ] РџСЂРѕРІРµСЂСЏС‚СЊ СЃСѓС‰РµСЃС‚РІРѕРІР°РЅРёРµ РїСѓС‚Рё
- [ ] РџСЂРѕРІРµСЂСЏС‚СЊ РґРѕСЃС‚СѓРїРЅРѕСЃС‚СЊ С‡С‚РµРЅРёСЏ
- [ ] РџСЂРѕРІРµСЂСЏС‚СЊ РЅР°Р»РёС‡РёРµ `.git`
- [ ] Р—Р°РїСЂРµС‰Р°С‚СЊ СЃРёСЃС‚РµРјРЅС‹Рµ РґРёСЂРµРєС‚РѕСЂРёРё
- [ ] РќРѕСЂРјР°Р»РёР·РѕРІР°С‚СЊ path

## File scanning

- [ ] РРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ `.git`
- [ ] РРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ `bin`
- [ ] РРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ `obj`
- [ ] РРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ `node_modules`
- [ ] РРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ `.vs`
- [ ] РРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ `.idea`
- [ ] РРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ build artifacts
- [ ] РћРіСЂР°РЅРёС‡РёС‚СЊ РјР°РєСЃРёРјР°Р»СЊРЅС‹Р№ СЂР°Р·РјРµСЂ С„Р°Р№Р»Р°
- [ ] РћРіСЂР°РЅРёС‡РёС‚СЊ РјР°РєСЃРёРјР°Р»СЊРЅРѕРµ С‡РёСЃР»Рѕ С„Р°Р№Р»РѕРІ

## Stack detection

- [ ] Р”РµС‚РµРєС‚РёСЂРѕРІР°С‚СЊ `.NET`
- [ ] Р”РµС‚РµРєС‚РёСЂРѕРІР°С‚СЊ `Node.js`
- [ ] Р”РµС‚РµРєС‚РёСЂРѕРІР°С‚СЊ `Python`
- [ ] Р”РµС‚РµРєС‚РёСЂРѕРІР°С‚СЊ generic project
- [ ] РќР°С…РѕРґРёС‚СЊ solution/project files
- [ ] РќР°С…РѕРґРёС‚СЊ package manifests

## Build/test command detection

- [ ] Р”Р»СЏ `.NET`: `dotnet build`
- [ ] Р”Р»СЏ `.NET`: `dotnet test`
- [ ] Р”Р»СЏ Node.js: `npm test` РёР»Рё script РёР· `package.json`
- [ ] Р”Р»СЏ Python: `pytest`
- [ ] РџРѕР·РІРѕР»РёС‚СЊ override РєРѕРјР°РЅРґ РІ settings

## Context snapshot

- [ ] РЎРѕР·РґР°РІР°С‚СЊ `ProjectContextSnapshot`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ СЃРїРёСЃРѕРє РєР»СЋС‡РµРІС‹С… С„Р°Р№Р»РѕРІ
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ detected stack
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ build/test commands
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ project summary

### Acceptance Criteria

- [ ] Р›РѕРєР°Р»СЊРЅС‹Р№ repo РґРѕР±Р°РІР»СЏРµС‚СЃСЏ
- [ ] Project scan СЃРѕС…СЂР°РЅСЏРµС‚СЃСЏ РІ Р‘Р”
- [ ] Stack РѕРїСЂРµРґРµР»СЏРµС‚СЃСЏ РєРѕСЂСЂРµРєС‚РЅРѕ
- [ ] Build/test РєРѕРјР°РЅРґС‹ РїСЂРµРґР»Р°РіР°СЋС‚СЃСЏ Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРё

---

# Milestone 6 вЂ” Tooling Core

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ plugin-like tools layer.

### Checklist

## Tool abstractions

- [ ] РЎРѕР·РґР°С‚СЊ `IAgentTool`
- [ ] РЎРѕР·РґР°С‚СЊ `ToolRequest`
- [ ] РЎРѕР·РґР°С‚СЊ `ToolResult`
- [ ] РЎРѕР·РґР°С‚СЊ `ToolPermission`
- [ ] РЎРѕР·РґР°С‚СЊ `ToolRegistry`
- [ ] Р”РѕР±Р°РІРёС‚СЊ DI registration tools
- [ ] Р”РѕР±Р°РІРёС‚СЊ tool execution logging
- [ ] Р”РѕР±Р°РІРёС‚СЊ timeout support
- [ ] Р”РѕР±Р°РІРёС‚СЊ cancellation support

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

- [ ] РџСЂРѕРІРµСЂРєР° permissions
- [ ] РџСЂРѕРІРµСЂРєР° workspace boundary
- [ ] Р›РѕРіРёСЂРѕРІР°РЅРёРµ input/output
- [ ] Redaction secrets
- [ ] Max output length
- [ ] Error normalization

### Acceptance Criteria

- [ ] Tools РІС‹Р·С‹РІР°СЋС‚СЃСЏ С‡РµСЂРµР· РµРґРёРЅС‹Р№ РёРЅС‚РµСЂС„РµР№СЃ
- [ ] Р’СЃРµ tool executions СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ
- [ ] Tool failures РЅРµ РІР°Р»СЏС‚ РїСЂРѕС†РµСЃСЃ Р±РµР· controlled error
- [ ] Workspace boundary СЂР°Р±РѕС‚Р°РµС‚

---

# Milestone 7 вЂ” Sandbox Execution

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ Р±РµР·РѕРїР°СЃРЅРѕРµ РІС‹РїРѕР»РЅРµРЅРёРµ РєРѕРјР°РЅРґ.

### Checklist

## Sandbox abstractions

- [ ] РЎРѕР·РґР°С‚СЊ `ISandboxExecutor`
- [ ] РЎРѕР·РґР°С‚СЊ `SandboxRequest`
- [ ] РЎРѕР·РґР°С‚СЊ `SandboxResult`
- [ ] РЎРѕР·РґР°С‚СЊ `SandboxOptions`

## Local sandbox

- [ ] Р’С‹РїРѕР»РЅРµРЅРёРµ РїСЂРѕС†РµСЃСЃР° РІ project directory
- [ ] Timeout
- [ ] CancellationToken
- [ ] Capture stdout
- [ ] Capture stderr
- [ ] Capture exit code
- [ ] Environment filtering
- [ ] Working directory validation

## Command safety

- [ ] Command allowlist РґР»СЏ MVP
- [ ] Blocklist РѕРїР°СЃРЅС‹С… РєРѕРјР°РЅРґ
- [ ] Р—Р°РїСЂРµС‚ РґРѕСЃС‚СѓРїР° РІРЅРµ workspace
- [ ] Р—Р°РїСЂРµС‚ `.ssh`
- [ ] Р—Р°РїСЂРµС‚ `.aws`
- [ ] Р—Р°РїСЂРµС‚ СЃРёСЃС‚РµРјРЅС‹С… РїСѓС‚РµР№
- [ ] Redaction `.env` Р·РЅР°С‡РµРЅРёР№

## Docker abstraction

- [ ] РЎРѕР·РґР°С‚СЊ Docker sandbox interface
- [ ] Р”РѕР±Р°РІРёС‚СЊ Р±Р°Р·РѕРІС‹Р№ stub
- [ ] РџРѕРґРіРѕС‚РѕРІРёС‚СЊ Docker.DotNet integration point

### Acceptance Criteria

- [ ] Build/test РєРѕРјР°РЅРґС‹ РІС‹РїРѕР»РЅСЏСЋС‚СЃСЏ
- [ ] Timeout СЂР°Р±РѕС‚Р°РµС‚
- [ ] Cancellation СЂР°Р±РѕС‚Р°РµС‚
- [ ] Dangerous commands Р±Р»РѕРєРёСЂСѓСЋС‚СЃСЏ
- [ ] Stdout/stderr СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ

---

# Milestone 8 вЂ” Git Integration

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ Р±РµР·РѕРїР°СЃРЅС‹Р№ Git workflow.

### Checklist

## Git operations

- [ ] РџСЂРѕРІРµСЂРёС‚СЊ, С‡С‚Рѕ repo СЏРІР»СЏРµС‚СЃСЏ git repo
- [ ] РџРѕР»СѓС‡Р°С‚СЊ current branch
- [ ] РџРѕР»СѓС‡Р°С‚СЊ status
- [ ] РЎРѕР·РґР°РІР°С‚СЊ feature branch
- [ ] РџРѕР»СѓС‡Р°С‚СЊ diff
- [ ] РџРѕР»СѓС‡Р°С‚СЊ changed files
- [ ] РЎРѕР·РґР°РІР°С‚СЊ commit
- [ ] РћС‚РєР°С‚С‹РІР°С‚СЊ uncommitted changes
- [ ] Р“РµРЅРµСЂРёСЂРѕРІР°С‚СЊ patch artifact

## Safety

- [ ] РќРµ РґРµР»Р°С‚СЊ push РІ MVP
- [ ] РќРµ РґРµР»Р°С‚СЊ force reset Р±РµР· approval
- [ ] Commit С‚РѕР»СЊРєРѕ РїРѕСЃР»Рµ approval
- [ ] Rollback С‚РѕР»СЊРєРѕ РёР·РјРµРЅРµРЅРёР№ MAACO
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `GitOperation`

## Branch naming

- [ ] Р“РµРЅРµСЂРёСЂРѕРІР°С‚СЊ branch name `maaco/task-{shortId}-{slug}`
- [ ] РџСЂРѕРІРµСЂСЏС‚СЊ РєРѕРЅС„Р»РёРєС‚ РёРјРµРЅ
- [ ] РЎРѕР·РґР°РІР°С‚СЊ РЅРѕРІС‹Р№ branch РїСЂРё РЅРµРѕР±С…РѕРґРёРјРѕСЃС‚Рё

### Acceptance Criteria

- [ ] Diff РіРµРЅРµСЂРёСЂСѓРµС‚СЃСЏ
- [ ] Commit СЃРѕР·РґР°РµС‚СЃСЏ РїРѕСЃР»Рµ approval
- [ ] Reject РѕС‚РєР°С‚С‹РІР°РµС‚ РёР·РјРµРЅРµРЅРёСЏ
- [ ] Git operations СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ РІ Р‘Р”

---

# Milestone 9 вЂ” LLM Gateway

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ LLM abstraction Рё РїСЂРѕРІР°Р№РґРµСЂС‹.

### Checklist

## Abstractions

- [ ] РЎРѕР·РґР°С‚СЊ `ILlmProvider`
- [ ] РЎРѕР·РґР°С‚СЊ `ILlmGateway`
- [ ] РЎРѕР·РґР°С‚СЊ `LlmRequest`
- [ ] РЎРѕР·РґР°С‚СЊ `LlmResponse`
- [ ] РЎРѕР·РґР°С‚СЊ `LlmMessage`
- [ ] РЎРѕР·РґР°С‚СЊ `LlmProviderOptions`
- [ ] РЎРѕР·РґР°С‚СЊ `ModelRoutingPolicy`

## Providers

- [ ] Р РµР°Р»РёР·РѕРІР°С‚СЊ `FakeLlmProvider`
- [ ] Р РµР°Р»РёР·РѕРІР°С‚СЊ OpenAI-compatible provider
- [ ] Р РµР°Р»РёР·РѕРІР°С‚СЊ Ollama provider
- [ ] Р”РѕР±Р°РІРёС‚СЊ provider health check
- [ ] Р”РѕР±Р°РІРёС‚СЊ retry transient errors
- [ ] Р”РѕР±Р°РІРёС‚СЊ timeout

## Logging/cost

- [ ] Р›РѕРіРёСЂРѕРІР°С‚СЊ prompt СЃ redaction
- [ ] Р›РѕРіРёСЂРѕРІР°С‚СЊ response СЃ redaction
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `LlmCallLog`
- [ ] РћС†РµРЅРёРІР°С‚СЊ input tokens
- [ ] РћС†РµРЅРёРІР°С‚СЊ output tokens
- [ ] РЎС‡РёС‚Р°С‚СЊ estimated cost

## Routing

- [ ] Planning в†’ configured strong model
- [ ] Coding в†’ configured strong model
- [ ] Debugging в†’ configured strong model
- [ ] Summary/log analysis в†’ cheap/local model if available
- [ ] Fallback РЅР° default provider

### Acceptance Criteria

- [ ] Fake provider СЂР°Р±РѕС‚Р°РµС‚ Р±РµР· API key
- [ ] OpenAI-compatible provider РІС‹Р·С‹РІР°РµС‚СЃСЏ С‡РµСЂРµР· config
- [ ] Ollama provider РІС‹Р·С‹РІР°РµС‚СЃСЏ С‡РµСЂРµР· config
- [ ] Р’СЃРµ LLM calls СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ
- [ ] Secrets РЅРµ РїРѕРїР°РґР°СЋС‚ РІ logs

---

# Milestone 10 вЂ” Agent Runtime

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ Р±Р°Р·РѕРІСѓСЋ Р°РіРµРЅС‚РЅСѓСЋ СЃРёСЃС‚РµРјСѓ.

### Checklist

## Base abstractions

- [ ] РЎРѕР·РґР°С‚СЊ `IAgent`
- [ ] РЎРѕР·РґР°С‚СЊ `AgentContext`
- [ ] РЎРѕР·РґР°С‚СЊ `AgentResult`
- [ ] РЎРѕР·РґР°С‚СЊ `AgentCapability`
- [ ] РЎРѕР·РґР°С‚СЊ `AgentRegistry`
- [ ] РЎРѕР·РґР°С‚СЊ `AgentExecutionService`

## Agents

- [ ] `OrchestratorAgent`
- [ ] `TaskPlannerAgent`
- [ ] `BackendDeveloperAgent`
- [ ] `TestWriterAgent`
- [ ] `DebuggingAgent`
- [ ] `GitManagerAgent`
- [ ] `DocumentationAgent`

## Agent constraints

- [ ] Agents РЅРµ РїРёС€СѓС‚ С„Р°Р№Р»С‹ РЅР°РїСЂСЏРјСѓСЋ
- [ ] Agents РёСЃРїРѕР»СЊР·СѓСЋС‚ tools
- [ ] Agents РїРѕР»СѓС‡Р°СЋС‚ scoped context
- [ ] Agents Р»РѕРіРёСЂСѓСЋС‚ СЂРµС€РµРЅРёСЏ
- [ ] Agents РІРѕР·РІСЂР°С‰Р°СЋС‚ structured output
- [ ] Agents РїРѕРґРґРµСЂР¶РёРІР°СЋС‚ cancellation

## Prompts

- [ ] РЎРѕР·РґР°С‚СЊ system prompt РґР»СЏ Planner
- [ ] РЎРѕР·РґР°С‚СЊ system prompt РґР»СЏ Developer
- [ ] РЎРѕР·РґР°С‚СЊ system prompt РґР»СЏ Test Writer
- [ ] РЎРѕР·РґР°С‚СЊ system prompt РґР»СЏ Debugging
- [ ] РЎРѕР·РґР°С‚СЊ system prompt РґР»СЏ Documentation
- [ ] Р”РѕР±Р°РІРёС‚СЊ response schema expectations

### Acceptance Criteria

- [ ] Agent registry СЂР°Р±РѕС‚Р°РµС‚
- [ ] Agents РІС‹Р·С‹РІР°СЋС‚СЃСЏ С‡РµСЂРµР· runtime
- [ ] Fake LLM РїРѕР·РІРѕР»СЏРµС‚ РїСЂРѕРіРЅР°С‚СЊ demo workflow
- [ ] Agent outputs СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ РєР°Рє artifacts/logs

---

# Milestone 11 вЂ” Workflow Orchestrator

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ deterministic MVP pipeline.

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

- [ ] РЎРѕР·РґР°С‚СЊ `IWorkflowOrchestrator`
- [ ] РЎРѕР·РґР°С‚СЊ `WorkflowExecutionContext`
- [ ] РЎРѕР·РґР°С‚СЊ `WorkflowStepExecutor`
- [ ] РЎРѕР·РґР°С‚СЊ checkpoint РїРѕСЃР»Рµ РєР°Р¶РґРѕРіРѕ step
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ status workflow
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ status step
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ realtime events
- [ ] РџРѕРґРґРµСЂР¶Р°С‚СЊ cancellation

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

- [ ] Workflow СЃРѕР·РґР°РµС‚СЃСЏ РёР· task
- [ ] Steps РІС‹РїРѕР»РЅСЏСЋС‚СЃСЏ РїРѕСЃР»РµРґРѕРІР°С‚РµР»СЊРЅРѕ
- [ ] State СЃРѕС…СЂР°РЅСЏРµС‚СЃСЏ РїРѕСЃР»Рµ РєР°Р¶РґРѕРіРѕ step
- [ ] Failure РЅРµ С‚РµСЂСЏРµС‚ РєРѕРЅС‚РµРєСЃС‚
- [ ] Debug loop СЂР°Р±РѕС‚Р°РµС‚
- [ ] Workflow РјРѕР¶РЅРѕ cancel

---

# Milestone 12 вЂ” Memory / Context MVP

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ Р±Р°Р·РѕРІСѓСЋ РїР°РјСЏС‚СЊ РїСЂРѕРµРєС‚Р°.

### Checklist

## Memory service

- [ ] РЎРѕР·РґР°С‚СЊ `IMemoryService`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `ProjectSummary`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `TaskSummary`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `FileSummary`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `BuildFailure`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `Decision`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `AgentNote`

## Retrieval

- [ ] РџРѕР»СѓС‡Р°С‚СЊ memory РїРѕ project id
- [ ] РџРѕР»СѓС‡Р°С‚СЊ memory РїРѕ task id
- [ ] Keyword search
- [ ] Limit top N records
- [ ] Exclude stale records by status
- [ ] Context assembly РґР»СЏ agents

## Future-ready fields

- [ ] `EmbeddingProvider`
- [ ] `EmbeddingModel`
- [ ] `EmbeddingHash`
- [ ] `VectorRef`
- [ ] `ContentHash`

### Acceptance Criteria

- [ ] Agents РїРѕР»СѓС‡Р°СЋС‚ project context
- [ ] Build/test failures СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ
- [ ] Previous task summaries РґРѕСЃС‚СѓРїРЅС‹
- [ ] Schema РіРѕС‚РѕРІР° Рє vector search

---

# Milestone 13 вЂ” Build/Test Loop

### Р¦РµР»СЊ
Р”РѕРІРµСЃС‚Рё Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРёР№ С†РёРєР» СЃР±РѕСЂРєРё, С‚РµСЃС‚РѕРІ Рё РёСЃРїСЂР°РІР»РµРЅРёР№.

### Checklist

## Build

- [ ] Р’С‹РїРѕР»РЅСЏС‚СЊ detected build command
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ `BuildRun`
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ stdout/stderr artifact
- [ ] РџР°СЂСЃРёС‚СЊ exit code
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ events

## Test

- [ ] Р’С‹РїРѕР»РЅСЏС‚СЊ detected test command
- [ ] РЎРѕС…СЂР°РЅСЏС‚СЊ test output
- [ ] РћРїСЂРµРґРµР»СЏС‚СЊ failed tests
- [ ] РџСѓР±Р»РёРєРѕРІР°С‚СЊ events

## Log analysis

- [ ] РР·РІР»РµРєР°С‚СЊ compiler errors
- [ ] РР·РІР»РµРєР°С‚СЊ stack traces
- [ ] РР·РІР»РµРєР°С‚СЊ failed assertions
- [ ] РџРµСЂРµРґР°РІР°С‚СЊ summary DebuggingAgent

## Debug cycle

- [ ] РЎС„РѕСЂРјРёСЂРѕРІР°С‚СЊ debug prompt
- [ ] РџРѕР»СѓС‡РёС‚СЊ patch
- [ ] Validate patch
- [ ] Apply patch
- [ ] РџРѕРІС‚РѕСЂРёС‚СЊ build/test
- [ ] РћСЃС‚Р°РЅРѕРІРёС‚СЊСЃСЏ РїСЂРё success/max retries

### Acceptance Criteria

- [ ] РћС€РёР±РєР° СЃР±РѕСЂРєРё РїСЂРёРІРѕРґРёС‚ Рє debug iteration
- [ ] РћС€РёР±РєР° С‚РµСЃС‚РѕРІ РїСЂРёРІРѕРґРёС‚ Рє debug iteration
- [ ] Р’СЃРµ РёС‚РµСЂР°С†РёРё РІРёРґРЅС‹ РІ logs
- [ ] РџРѕСЃР»Рµ success workflow РёРґРµС‚ Рє diff review

---

# Milestone 14 вЂ” Avalonia UI Shell

### Р¦РµР»СЊ
РЎРѕР·РґР°С‚СЊ desktop UI MAACO.

### Checklist

## App shell

- [ ] РЎРѕР·РґР°С‚СЊ Avalonia app
- [ ] РќР°СЃС‚СЂРѕРёС‚СЊ MVVM
- [ ] РќР°СЃС‚СЂРѕРёС‚СЊ DI
- [ ] РќР°СЃС‚СЂРѕРёС‚СЊ navigation service
- [ ] РќР°СЃС‚СЂРѕРёС‚СЊ API client
- [ ] РќР°СЃС‚СЂРѕРёС‚СЊ SignalR client

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

- [ ] App Р·Р°РїСѓСЃРєР°РµС‚СЃСЏ
- [ ] РќР°РІРёРіР°С†РёСЏ СЂР°Р±РѕС‚Р°РµС‚
- [ ] API client РїРѕРґРєР»СЋС‡Р°РµС‚СЃСЏ Рє backend
- [ ] SignalR РїРѕР»СѓС‡Р°РµС‚ events
- [ ] РћС€РёР±РєРё РѕС‚РѕР±СЂР°Р¶Р°СЋС‚СЃСЏ РїРѕР»СЊР·РѕРІР°С‚РµР»СЋ

---

# Milestone 15 вЂ” UI Project + Task Flow

### Р¦РµР»СЊ
Р”Р°С‚СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»СЋ РІРѕР·РјРѕР¶РЅРѕСЃС‚СЊ РґРѕР±Р°РІРёС‚СЊ repo Рё СЃРѕР·РґР°С‚СЊ Р·Р°РґР°С‡Сѓ.

### Checklist

## Projects UI

- [ ] Р’С‹Р±СЂР°С‚СЊ folder
- [ ] Р”РѕР±Р°РІРёС‚СЊ РїСЂРѕРµРєС‚
- [ ] Р—Р°РїСѓСЃС‚РёС‚СЊ scan
- [ ] РџРѕРєР°Р·Р°С‚СЊ detected stack
- [ ] РџРѕРєР°Р·Р°С‚СЊ build/test commands
- [ ] РџРѕРєР°Р·Р°С‚СЊ project files summary

## Task UI

- [ ] Р’С‹Р±СЂР°С‚СЊ project
- [ ] Р’РІРµСЃС‚Рё title
- [ ] Р’РІРµСЃС‚Рё description
- [ ] Р’РІРµСЃС‚Рё constraints
- [ ] Р’С‹Р±СЂР°С‚СЊ approval mode
- [ ] Р—Р°РїСѓСЃС‚РёС‚СЊ task
- [ ] РџРµСЂРµР№С‚Рё РІ workflow monitor

### Acceptance Criteria

- [ ] РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ РґРѕР±Р°РІР»СЏРµС‚ local repo С‡РµСЂРµР· UI
- [ ] РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ СЃРѕР·РґР°РµС‚ task С‡РµСЂРµР· UI
- [ ] Workflow СЃС‚Р°СЂС‚СѓРµС‚
- [ ] UI РїРѕРєР°Р·С‹РІР°РµС‚ РЅР°С‡Р°Р»СЊРЅС‹Р№ СЃС‚Р°С‚СѓСЃ workflow

---

# Milestone 16 вЂ” UI Workflow Monitor

### Р¦РµР»СЊ
Р’РёР·СѓР°Р»РёР·РёСЂРѕРІР°С‚СЊ РІС‹РїРѕР»РЅРµРЅРёРµ pipeline.

### Checklist

## Workflow timeline

- [ ] РџРѕРєР°Р·Р°С‚СЊ СЃРїРёСЃРѕРє steps
- [ ] РџРѕРєР°Р·Р°С‚СЊ С‚РµРєСѓС‰РёР№ step
- [ ] РџРѕРєР°Р·Р°С‚СЊ statuses
- [ ] РџРѕРєР°Р·Р°С‚СЊ duration
- [ ] РџРѕРєР°Р·Р°С‚СЊ retry count

## Live logs

- [ ] Stream logs С‡РµСЂРµР· SignalR
- [ ] Р¤РёР»СЊС‚СЂ РїРѕ severity
- [ ] Р¤РёР»СЊС‚СЂ РїРѕ agent
- [ ] Р¤РёР»СЊС‚СЂ РїРѕ tool
- [ ] Copy log line
- [ ] Save logs view

## Agent activity

- [ ] РџРѕРєР°Р·С‹РІР°С‚СЊ Р°РєС‚РёРІРЅРѕРіРѕ agent
- [ ] РџРѕРєР°Р·С‹РІР°С‚СЊ tool executions
- [ ] РџРѕРєР°Р·С‹РІР°С‚СЊ LLM call status
- [ ] РџРѕРєР°Р·С‹РІР°С‚СЊ token estimate

### Acceptance Criteria

- [ ] Workflow progress РІРёРґРµРЅ realtime
- [ ] Р›РѕРіРё РѕР±РЅРѕРІР»СЏСЋС‚СЃСЏ Р±РµР· refresh
- [ ] РћС€РёР±РєРё СЏРІРЅРѕ РїРѕРґСЃРІРµС‡РµРЅС‹
- [ ] РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ РїРѕРЅРёРјР°РµС‚ С‚РµРєСѓС‰РёР№ СЌС‚Р°Рї

---

# Milestone 17 вЂ” Diff Review + Approval UI

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ human-in-the-loop approval.

### Checklist

## Diff UI

- [ ] РџРѕР»СѓС‡РёС‚СЊ diff РїРѕ task
- [ ] РџРѕРєР°Р·Р°С‚СЊ changed files
- [ ] РџРѕРєР°Р·Р°С‚СЊ unified diff
- [ ] РџРѕРґСЃРІРµС‚РёС‚СЊ added/removed lines
- [ ] РџРѕРєР°Р·Р°С‚СЊ generated commit message
- [ ] РџРѕР·РІРѕР»РёС‚СЊ РёР·РјРµРЅРёС‚СЊ commit message

## Approval

- [ ] Approve button
- [ ] Reject button
- [ ] Reject with reason
- [ ] Approve РІС‹Р·С‹РІР°РµС‚ commit
- [ ] Reject РІС‹Р·С‹РІР°РµС‚ rollback
- [ ] РџРѕРєР°Р·Р°С‚СЊ СЂРµР·СѓР»СЊС‚Р°С‚ РѕРїРµСЂР°С†РёРё

### Acceptance Criteria

- [ ] Workflow РѕСЃС‚Р°РЅР°РІР»РёРІР°РµС‚СЃСЏ РЅР° approval
- [ ] РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ РІРёРґРёС‚ diff
- [ ] Approve СЃРѕР·РґР°РµС‚ commit
- [ ] Reject РѕС‚РєР°С‚С‹РІР°РµС‚ РёР·РјРµРЅРµРЅРёСЏ
- [ ] Р РµР·СѓР»СЊС‚Р°С‚ РІРёРґРµРЅ РІ UI

---

# Milestone 18 вЂ” Settings & Configuration

### Р¦РµР»СЊ
Р РµР°Р»РёР·РѕРІР°С‚СЊ РЅР°СЃС‚СЂРѕР№РєРё MAACO.

### Checklist

## Backend settings

- [ ] РҐСЂР°РЅРёС‚СЊ РЅР°СЃС‚СЂРѕР№РєРё РІ appsettings + DB override
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

- [ ] API key РЅРµ РїРѕРєР°Р·С‹РІР°С‚СЊ РїРѕР»РЅРѕСЃС‚СЊСЋ
- [ ] API key redaction
- [ ] Test connection button
- [ ] РќРµ Р»РѕРіРёСЂРѕРІР°С‚СЊ secrets

### Acceptance Criteria

- [ ] РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ РЅР°СЃС‚СЂР°РёРІР°РµС‚ provider
- [ ] РќР°СЃС‚СЂРѕР№РєРё СЃРѕС…СЂР°РЅСЏСЋС‚СЃСЏ
- [ ] Fake provider РґРѕСЃС‚СѓРїРµРЅ РІСЃРµРіРґР°
- [ ] Secrets РЅРµ РІРёРґРЅС‹ РІ logs

---

# Milestone 19 вЂ” Security Hardening

### Р¦РµР»СЊ
Р—Р°РєСЂС‹С‚СЊ РѕСЃРЅРѕРІРЅС‹Рµ СЂРёСЃРєРё MVP.

### Checklist

## Filesystem security

- [ ] Path traversal protection
- [ ] Workspace allowlist
- [ ] Р—Р°РїСЂРµС‚ РґРѕСЃС‚СѓРїР° РІРЅРµ repo
- [ ] Р—Р°РїСЂРµС‚ СЃРёСЃС‚РµРјРЅС‹С… РґРёСЂРµРєС‚РѕСЂРёР№
- [ ] Р—Р°РїСЂРµС‚ С‡С‚РµРЅРёСЏ `.ssh`
- [ ] Р—Р°РїСЂРµС‚ С‡С‚РµРЅРёСЏ `.aws`
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

# Milestone 20 вЂ” Reliability & Recovery

### Р¦РµР»СЊ
РЎРґРµР»Р°С‚СЊ MVP СѓСЃС‚РѕР№С‡РёРІС‹Рј Рє СЃР±РѕСЏРј.

### Checklist

## Checkpoints

- [ ] Checkpoint РїРѕСЃР»Рµ РєР°Р¶РґРѕРіРѕ workflow step
- [ ] Save step inputs
- [ ] Save step outputs
- [ ] Save artifacts
- [ ] Save errors

## Recovery

- [ ] App restart СЃРѕС…СЂР°РЅСЏРµС‚ workflows
- [ ] Failed workflow РјРѕР¶РЅРѕ РѕС‚РєСЂС‹С‚СЊ
- [ ] Failed workflow РїРѕРєР°Р·С‹РІР°РµС‚ РїСЂРёС‡РёРЅСѓ
- [ ] Cancelled workflow СЃРѕС…СЂР°РЅСЏРµС‚ logs
- [ ] Pending approval СЃРѕС…СЂР°РЅСЏРµС‚СЃСЏ РїРѕСЃР»Рµ restart

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

- [ ] Restart РЅРµ С‚РµСЂСЏРµС‚ СЃРѕСЃС‚РѕСЏРЅРёРµ
- [ ] Failed workflow РґРёР°РіРЅРѕСЃС‚РёСЂСѓРµРј
- [ ] Rollback СЂР°Р±РѕС‚Р°РµС‚
- [ ] Retry РЅРµ Р·Р°С†РёРєР»РёРІР°РµС‚СЃСЏ

---

# Milestone 21 вЂ” Testing

### Р¦РµР»СЊ
РџРѕРєСЂС‹С‚СЊ РєСЂРёС‚РёС‡РЅСѓСЋ Р»РѕРіРёРєСѓ С‚РµСЃС‚Р°РјРё.

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

- [ ] `dotnet test` РїСЂРѕС…РѕРґРёС‚
- [ ] Critical path РїРѕРєСЂС‹С‚ С‚РµСЃС‚Р°РјРё
- [ ] Fake LLM РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ РґР»СЏ deterministic tests
- [ ] Tests РЅРµ С‚СЂРµР±СѓСЋС‚ СЂРµР°Р»СЊРЅРѕРіРѕ API key

---

# Milestone 22 вЂ” Documentation & Demo

### Р¦РµР»СЊ
РџРѕРґРіРѕС‚РѕРІРёС‚СЊ РїСЂРѕРµРєС‚ Рє РёСЃРїРѕР»СЊР·РѕРІР°РЅРёСЋ Рё РґРµРјРѕРЅСЃС‚СЂР°С†РёРё.

### Checklist

## README

- [ ] Р§С‚Рѕ С‚Р°РєРѕРµ MAACO
- [ ] РђСЂС…РёС‚РµРєС‚СѓСЂР°
- [ ] РўСЂРµР±РѕРІР°РЅРёСЏ
- [ ] РљР°Рє Р·Р°РїСѓСЃС‚РёС‚СЊ backend
- [ ] РљР°Рє Р·Р°РїСѓСЃС‚РёС‚СЊ UI
- [ ] РљР°Рє РЅР°СЃС‚СЂРѕРёС‚СЊ OpenAI-compatible provider
- [ ] РљР°Рє РЅР°СЃС‚СЂРѕРёС‚СЊ Ollama
- [ ] РљР°Рє РёСЃРїРѕР»СЊР·РѕРІР°С‚СЊ Fake provider
- [ ] РљР°Рє РґРѕР±Р°РІРёС‚СЊ repo
- [ ] РљР°Рє СЃРѕР·РґР°С‚СЊ task
- [ ] РљР°Рє approve/reject changes

## Developer docs

- [ ] Solution structure
- [ ] Domain model overview
- [ ] Agent runtime overview
- [ ] Tooling architecture
- [ ] Workflow pipeline
- [ ] Security model
- [ ] Testing guide

## Demo

- [ ] Р”РѕР±Р°РІРёС‚СЊ sample project
- [ ] Р”РѕР±Р°РІРёС‚СЊ demo task
- [ ] Р”РѕР±Р°РІРёС‚СЊ screenshots РёР»Рё РѕРїРёСЃР°РЅРёРµ flow
- [ ] Р”РѕР±Р°РІРёС‚СЊ troubleshooting section

### Acceptance Criteria

- [ ] РќРѕРІС‹Р№ СЂР°Р·СЂР°Р±РѕС‚С‡РёРє РјРѕР¶РµС‚ Р·Р°РїСѓСЃС‚РёС‚СЊ MAACO РїРѕ README
- [ ] Р•СЃС‚СЊ РїРѕРЅСЏС‚РЅС‹Р№ demo flow
- [ ] Р”РѕРєСѓРјРµРЅС‚Р°С†РёСЏ СЃРѕРѕС‚РІРµС‚СЃС‚РІСѓРµС‚ СЂРµР°Р»РёР·Р°С†РёРё

---

# Milestone 23 вЂ” MVP Release Hardening

### Р¦РµР»СЊ
РџРѕРґРіРѕС‚РѕРІРёС‚СЊ РїРµСЂРІС‹Р№ СЃС‚Р°Р±РёР»СЊРЅС‹Р№ MVP build.

### Checklist

## Build

- [ ] РџСЂРѕРІРµСЂРёС‚СЊ clean build
- [ ] РџСЂРѕРІРµСЂРёС‚СЊ tests
- [ ] РџСЂРѕРІРµСЂРёС‚СЊ migrations
- [ ] РџСЂРѕРІРµСЂРёС‚СЊ app startup
- [ ] РџСЂРѕРІРµСЂРёС‚СЊ logs directory
- [ ] РџСЂРѕРІРµСЂРёС‚СЊ config templates

## Packaging

- [ ] Publish backend
- [ ] Publish Avalonia app
- [ ] РЎРѕР·РґР°С‚СЊ release notes
- [ ] РЎРѕР·РґР°С‚СЊ known limitations
- [ ] РЎРѕР·РґР°С‚СЊ `.env.example` РёР»Рё `appsettings.example.json`

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

- [ ] MVP РјРѕР¶РЅРѕ СЃРѕР±СЂР°С‚СЊ Рё Р·Р°РїСѓСЃС‚РёС‚СЊ Р»РѕРєР°Р»СЊРЅРѕ
- [ ] РћСЃРЅРѕРІРЅРѕР№ SDLC pipeline СЂР°Р±РѕС‚Р°РµС‚
- [ ] РќРµС‚ Р±Р»РѕРєРёСЂСѓСЋС‰РёС… РѕС€РёР±РѕРє
- [ ] Р’СЃРµ non-goals СЏРІРЅРѕ Р·Р°РґРѕРєСѓРјРµРЅС‚РёСЂРѕРІР°РЅС‹

---

# Definition of Done РґР»СЏ РєР°Р¶РґРѕРіРѕ milestone

- [ ] РљРѕРґ СЂРµР°Р»РёР·РѕРІР°РЅ
- [ ] РљРѕРґ РѕС‚С„РѕСЂРјР°С‚РёСЂРѕРІР°РЅ
- [ ] РќРµС‚ compiler errors
- [ ] РќРµС‚ critical warnings
- [ ] Р”РѕР±Р°РІР»РµРЅС‹ С‚РµСЃС‚С‹, РµСЃР»Рё РїСЂРёРјРµРЅРёРјРѕ
- [ ] РћР±РЅРѕРІР»РµРЅР° РґРѕРєСѓРјРµРЅС‚Р°С†РёСЏ, РµСЃР»Рё РїСЂРёРјРµРЅРёРјРѕ
- [ ] Р›РѕРіРё РЅРµ СЃРѕРґРµСЂР¶Р°С‚ secrets
- [ ] `dotnet build` РїСЂРѕС…РѕРґРёС‚
- [ ] `dotnet test` РїСЂРѕС…РѕРґРёС‚
- [ ] РР·РјРµРЅРµРЅРёСЏ РјРѕР¶РЅРѕ РѕР±СЉСЏСЃРЅРёС‚СЊ С‡РµСЂРµР· README РёР»Рё РєРѕРјРјРµРЅС‚Р°СЂРёРё

---

# Р РµРєРѕРјРµРЅРґСѓРµРјС‹Р№ РїРѕСЂСЏРґРѕРє СЂРµР°Р»РёР·Р°С†РёРё

1. Milestone 0 вЂ” Bootstrap
2. Milestone 1 вЂ” Core Domain
3. Milestone 2 вЂ” Persistence
4. Milestone 3 вЂ” Backend API
5. Milestone 4 вЂ” Realtime
6. Milestone 5 вЂ” Project Scanner
7. Milestone 6 вЂ” Tools
8. Milestone 7 вЂ” Sandbox
9. Milestone 8 вЂ” Git
10. Milestone 9 вЂ” LLM Gateway
11. Milestone 10 вЂ” Agents
12. Milestone 11 вЂ” Orchestrator
13. Milestone 12 вЂ” Memory
14. Milestone 13 вЂ” Build/Test Loop
15. Milestone 14 вЂ” UI Shell
16. Milestone 15 вЂ” UI Project/Task Flow
17. Milestone 16 вЂ” UI Workflow Monitor
18. Milestone 17 вЂ” Diff Review
19. Milestone 18 вЂ” Settings
20. Milestone 19 вЂ” Security
21. Milestone 20 вЂ” Reliability
22. Milestone 21 вЂ” Testing
23. Milestone 22 вЂ” Documentation
24. Milestone 23 вЂ” MVP Release

