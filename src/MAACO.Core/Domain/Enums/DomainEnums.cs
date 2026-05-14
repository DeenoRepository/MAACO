namespace MAACO.Core.Domain.Enums;

public enum TaskStatus
{
    Created = 0,
    Planned = 1,
    Queued = 2,
    Running = 3,
    WaitingForApproval = 4,
    Retrying = 5,
    Failed = 6,
    Completed = 7,
    Cancelled = 8,
    RolledBack = 9
}

public enum WorkflowStatus
{
    Created = 0,
    Planned = 1,
    Queued = 2,
    Running = 3,
    WaitingForApproval = 4,
    Retrying = 5,
    Failed = 6,
    Completed = 7,
    Cancelled = 8,
    RolledBack = 9
}

public enum WorkflowStepStatus
{
    Pending = 0,
    Running = 1,
    WaitingForApproval = 2,
    Retrying = 3,
    Failed = 4,
    Completed = 5,
    Cancelled = 6
}

public enum AgentStatus
{
    Idle = 0,
    Running = 1,
    Waiting = 2,
    Failed = 3,
    Completed = 4,
    Cancelled = 5
}

public enum ToolExecutionStatus
{
    Pending = 0,
    Running = 1,
    Failed = 2,
    Completed = 3,
    TimedOut = 4,
    Cancelled = 5
}

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Expired = 3,
    Cancelled = 4
}

public enum ApprovalMode
{
    Always = 0,
    Conditional = 1,
    Never = 2
}

public enum ArtifactType
{
    Patch = 0,
    BuildLog = 1,
    TestLog = 2,
    RuntimeLog = 3,
    Trace = 4,
    Diff = 5,
    Snapshot = 6
}

public enum MemoryRecordType
{
    Fact = 0,
    Decision = 1,
    Constraint = 2,
    Observation = 3,
    Summary = 4
}

public enum GitOperationType
{
    Status = 0,
    Diff = 1,
    Branch = 2,
    Commit = 3,
    Rollback = 4
}

public enum BuildRunStatus
{
    Started = 0,
    Failed = 1,
    Succeeded = 2,
    Cancelled = 3,
    TimedOut = 4
}

public enum LogSeverity
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
