using MAACO.Core.Domain.Common;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.ValueObjects;

namespace MAACO.Core.Domain.Entities;

public sealed class Project : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public RepositoryPath RepositoryPath { get; set; } = new(".");
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}

public sealed class TaskItem : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MAACO.Core.Domain.Enums.TaskStatus Status { get; set; } = MAACO.Core.Domain.Enums.TaskStatus.Created;
}

public sealed class Workflow : AuditableEntity
{
    public Guid TaskId { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Created;
    public int RetryCount { get; set; }
}

public sealed class WorkflowStep : AuditableEntity
{
    public Guid WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;
    public int Order { get; set; }
}

public sealed class AgentDefinition : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public AgentStatus Status { get; set; } = AgentStatus.Idle;
}

public sealed class ToolExecution : AuditableEntity
{
    public Guid WorkflowId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public ToolExecutionStatus Status { get; set; } = ToolExecutionStatus.Pending;
    public ExecutionResult? Result { get; set; }
}

public sealed class LogEvent : AuditableEntity
{
    public Guid? WorkflowId { get; set; }
    public Guid? TaskId { get; set; }
    public LogSeverity Severity { get; set; } = LogSeverity.Information;
    public string Message { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
}

public sealed class Artifact : AuditableEntity
{
    public Guid TaskId { get; set; }
    public ArtifactType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Hash { get; set; }
}

public sealed class GitOperation : AuditableEntity
{
    public Guid TaskId { get; set; }
    public GitOperationType Type { get; set; }
    public bool Succeeded { get; set; }
    public string? Details { get; set; }
}

public sealed class BuildRun : AuditableEntity
{
    public Guid WorkflowId { get; set; }
    public BuildRunStatus Status { get; set; } = BuildRunStatus.Started;
    public TimeSpan Duration { get; set; }
}

public sealed class MemoryRecord : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public MemoryRecordType Type { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? EmbeddingProvider { get; set; }
    public string? EmbeddingModel { get; set; }
    public string? EmbeddingHash { get; set; }
    public string? VectorRef { get; set; }
    public string? ContentHash { get; set; }
}

public sealed class ApprovalRequest : AuditableEntity
{
    public Guid WorkflowId { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public ApprovalMode Mode { get; set; } = ApprovalMode.Conditional;
    public string? RequestedBy { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public sealed class LlmCallLog : AuditableEntity
{
    public Guid? WorkflowId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public LlmUsage Usage { get; set; } = new(0, 0, 0);
    public TimeSpan Duration { get; set; }
}

public sealed class ProjectContextSnapshot : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CommitHash { get; set; } = string.Empty;
    public DetectedProjectStack Stack { get; set; } = new("unknown", "unknown");
}
