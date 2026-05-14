using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;

namespace MAACO.Infrastructure.Workflows;

public sealed class WorkflowStepExecutor(
    IWorkflowRepository workflowRepository,
    IArtifactRepository artifactRepository,
    IEventBus eventBus)
{
    public async Task ExecuteAsync(
        WorkflowExecutionContext context,
        IReadOnlyList<WorkflowStep> steps,
        CancellationToken cancellationToken)
    {
        foreach (var step in steps.OrderBy(x => x.Order))
        {
            cancellationToken.ThrowIfCancellationRequested();

            step.Status = WorkflowStepStatus.Running;
            await workflowRepository.SaveChangesAsync(cancellationToken);

            await eventBus.PublishAsync(
                new WorkflowStepStartedEvent(
                    step.Id,
                    context.WorkflowId,
                    DateTimeOffset.UtcNow,
                    context.CorrelationId),
                cancellationToken);

            step.Status = WorkflowStepStatus.Completed;
            await workflowRepository.SaveChangesAsync(cancellationToken);

            await artifactRepository.AddAsync(
                new Artifact
                {
                    TaskId = context.TaskId,
                    Type = ArtifactType.Snapshot,
                    Path = $"checkpoint://workflow/{context.WorkflowId:D}/step/{step.Order}",
                    Hash = $"{step.Name}:{step.Status}:{DateTimeOffset.UtcNow:O}"
                },
                cancellationToken);
            await artifactRepository.SaveChangesAsync(cancellationToken);

            await eventBus.PublishAsync(
                new WorkflowStepCompletedEvent(
                    step.Id,
                    context.WorkflowId,
                    DateTimeOffset.UtcNow,
                    context.CorrelationId),
                cancellationToken);
        }
    }
}
