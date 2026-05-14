using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;
using MAACO.Core.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Infrastructure.Events.Handlers;

public sealed class WorkflowStartedStatusHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowStartedEvent>
{
    public Task HandleAsync(WorkflowStartedEvent @event, CancellationToken cancellationToken) =>
        WorkflowStatusProjectionHelpers.UpdateStatusAsync(scopeFactory, @event.WorkflowId, WorkflowStatus.Running, cancellationToken);
}

public sealed class ApprovalRequestedStatusHandler(IServiceScopeFactory scopeFactory) : IEventHandler<ApprovalRequestedEvent>
{
    public Task HandleAsync(ApprovalRequestedEvent @event, CancellationToken cancellationToken) =>
        WorkflowStatusProjectionHelpers.UpdateStatusAsync(scopeFactory, @event.WorkflowId, WorkflowStatus.WaitingForApproval, cancellationToken);
}

public sealed class WorkflowCompletedStatusHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowCompletedEvent>
{
    public Task HandleAsync(WorkflowCompletedEvent @event, CancellationToken cancellationToken) =>
        WorkflowStatusProjectionHelpers.UpdateStatusAsync(scopeFactory, @event.WorkflowId, WorkflowStatus.Completed, cancellationToken);
}

public sealed class WorkflowFailedStatusHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowFailedEvent>
{
    public Task HandleAsync(WorkflowFailedEvent @event, CancellationToken cancellationToken) =>
        WorkflowStatusProjectionHelpers.UpdateStatusAsync(scopeFactory, @event.WorkflowId, WorkflowStatus.Failed, cancellationToken);
}

internal static class WorkflowStatusProjectionHelpers
{
    public static async Task UpdateStatusAsync(
        IServiceScopeFactory scopeFactory,
        Guid workflowId,
        WorkflowStatus status,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var workflowRepository = scope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
        var workflow = await workflowRepository.GetByIdAsync(workflowId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.Status = WorkflowStatusTransitions.EnsureCanTransition(workflow.Status, status);
        workflow.UpdatedAt = DateTimeOffset.UtcNow;
        await workflowRepository.SaveChangesAsync(cancellationToken);
    }
}
