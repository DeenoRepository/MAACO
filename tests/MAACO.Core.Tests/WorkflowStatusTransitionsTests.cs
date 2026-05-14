using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Services;

namespace MAACO.Core.Tests;

public sealed class WorkflowStatusTransitionsTests
{
    [Theory]
    [InlineData(WorkflowStatus.Created, WorkflowStatus.Planned)]
    [InlineData(WorkflowStatus.Planned, WorkflowStatus.Queued)]
    [InlineData(WorkflowStatus.Queued, WorkflowStatus.Running)]
    [InlineData(WorkflowStatus.Running, WorkflowStatus.WaitingForApproval)]
    [InlineData(WorkflowStatus.Running, WorkflowStatus.Completed)]
    [InlineData(WorkflowStatus.WaitingForApproval, WorkflowStatus.Running)]
    [InlineData(WorkflowStatus.Failed, WorkflowStatus.Retrying)]
    [InlineData(WorkflowStatus.Cancelled, WorkflowStatus.RolledBack)]
    public void IsTransitionAllowed_ReturnsTrue_ForValidTransitions(WorkflowStatus from, WorkflowStatus to)
    {
        var result = WorkflowStatusTransitions.IsTransitionAllowed(from, to);

        Assert.True(result);
    }

    [Theory]
    [InlineData(WorkflowStatus.Created, WorkflowStatus.Completed)]
    [InlineData(WorkflowStatus.Planned, WorkflowStatus.Completed)]
    [InlineData(WorkflowStatus.Completed, WorkflowStatus.Running)]
    [InlineData(WorkflowStatus.RolledBack, WorkflowStatus.Running)]
    [InlineData(WorkflowStatus.Running, WorkflowStatus.Planned)]
    [InlineData(WorkflowStatus.WaitingForApproval, WorkflowStatus.Queued)]
    [InlineData(WorkflowStatus.Running, WorkflowStatus.Running)]
    public void IsTransitionAllowed_ReturnsFalse_ForInvalidTransitions(WorkflowStatus from, WorkflowStatus to)
    {
        var result = WorkflowStatusTransitions.IsTransitionAllowed(from, to);

        Assert.False(result);
    }

    [Theory]
    [InlineData(WorkflowStatus.Created, WorkflowStatus.Completed)]
    [InlineData(WorkflowStatus.Completed, WorkflowStatus.Cancelled)]
    [InlineData(WorkflowStatus.RolledBack, WorkflowStatus.Failed)]
    public void EnsureCanTransition_Throws_ForInvalidTransitions(WorkflowStatus from, WorkflowStatus to)
    {
        Assert.Throws<InvalidOperationException>(() => WorkflowStatusTransitions.EnsureCanTransition(from, to));
    }
}
