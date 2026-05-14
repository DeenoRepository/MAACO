using MAACO.Core.Domain.Enums;

namespace MAACO.Core.Domain.Services;

public static class WorkflowStatusTransitions
{
    private static readonly IReadOnlyDictionary<WorkflowStatus, HashSet<WorkflowStatus>> AllowedTransitions =
        new Dictionary<WorkflowStatus, HashSet<WorkflowStatus>>
        {
            [WorkflowStatus.Created] = new() { WorkflowStatus.Planned, WorkflowStatus.Running, WorkflowStatus.Cancelled },
            [WorkflowStatus.Planned] = new() { WorkflowStatus.Queued, WorkflowStatus.Cancelled },
            [WorkflowStatus.Queued] = new() { WorkflowStatus.Running, WorkflowStatus.Cancelled },
            [WorkflowStatus.Running] = new()
            {
                WorkflowStatus.WaitingForApproval,
                WorkflowStatus.Retrying,
                WorkflowStatus.Failed,
                WorkflowStatus.Completed,
                WorkflowStatus.Cancelled
            },
            [WorkflowStatus.WaitingForApproval] = new()
            {
                WorkflowStatus.Running,
                WorkflowStatus.Cancelled,
                WorkflowStatus.Failed
            },
            [WorkflowStatus.Retrying] = new() { WorkflowStatus.Running, WorkflowStatus.Failed, WorkflowStatus.Cancelled },
            [WorkflowStatus.Failed] = new() { WorkflowStatus.Retrying, WorkflowStatus.RolledBack, WorkflowStatus.Cancelled },
            [WorkflowStatus.Completed] = new(),
            [WorkflowStatus.Cancelled] = new() { WorkflowStatus.RolledBack },
            [WorkflowStatus.RolledBack] = new()
        };

    public static bool IsTransitionAllowed(WorkflowStatus from, WorkflowStatus to)
    {
        if (from == to)
        {
            return false;
        }

        return AllowedTransitions.TryGetValue(from, out var next) && next.Contains(to);
    }

    public static WorkflowStatus EnsureCanTransition(WorkflowStatus from, WorkflowStatus to)
    {
        if (!IsTransitionAllowed(from, to))
        {
            throw new InvalidOperationException($"Invalid workflow status transition: {from} -> {to}.");
        }

        return to;
    }
}
