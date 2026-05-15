# MAACO Developer Guide

## Solution Structure

- `src/MAACO.Core`
  - Domain entities/enums/value objects
  - Abstractions for tools, workflows, llm, repositories, sandbox
- `src/MAACO.Persistence`
  - `MaacoDbContext`, migrations, EF repositories
- `src/MAACO.Infrastructure`
  - Workflow orchestrator and step handlers
  - LLM providers/gateway/retry
  - Event handlers and memory service
- `src/MAACO.Api`
  - REST controllers and DTO contracts
  - SignalR workflow hub
- `src/MAACO.Tools`
  - Tool registry and tool implementations
- `src/MAACO.Sandbox`
  - Command execution isolation/safety
- `src/MAACO.Agents`
  - Agent abstractions/runtime/stub agents/prompts
- `src/MAACO.App`
  - Avalonia MVVM desktop app

## Domain Model Overview

Core entities:

- `Project`, `TaskItem`, `Workflow`, `WorkflowStep`
- `ApprovalRequest`, `Artifact`, `LogEvent`
- `ToolExecution`, `GitOperation`, `BuildRun`, `MemoryRecord`, `LlmCallLog`

State transitions are constrained via domain service:

- `WorkflowStatusTransitions`

## Agent Runtime Overview

- `IAgent` + `AgentRegistry` + `AgentExecutionService`
- Role-specific agents (planner/dev/debug/doc/git/test writer)
- Structured outputs (`AgentResult`) and cancellation support

## Tooling Architecture

- `IAgentTool` contract with `ToolRequest/ToolResult`
- `ToolRegistry` enforces:
  - registration checks
  - permission checks
  - timeout/cancellation
  - execution logging/persistence
  - transient retry policy for tool failures
- Tools include scanner, filesystem, patch, build, test, git, diff, log analysis

## Workflow Pipeline

Primary deterministic chain:

1. Project Scan
2. Git Branch
3. Planning
4. Code Generation
5. Patch Application
6. Test Generation
7. Build
8. Test
9. Debug Loop
10. Diff
11. Approval
12. Commit or Rollback
13. Documentation
14. Final Report

Execution is persisted step-by-step with checkpoints/logs/artifacts.

## Security Model

- Workspace/path boundary enforcement
- Sensitive path blocks (`.ssh`, `.aws`, system dirs)
- Controlled `.env` redaction
- Prompt safety sanitization
- Command blocklist + allowlist + timeout + output cap
- Approval-gated commit/rollback flow
- Audit logs for commands/file writes/LLM calls/approval decisions

## Testing Guide

Run full suite:

```bash
dotnet test MAACO.sln --no-restore -m:1 -v minimal
```

Key test areas:

- Core: state transitions, workflow integration, recovery/retry logic
- Tools: boundaries, patch apply/reject, git safety, redaction
- Agents: abstractions/runtime with fake llm paths
- Recovery: restart scope persistence for failed and approval workflows

Deterministic mode:

- Fake LLM provider is supported and used in deterministic test paths.
