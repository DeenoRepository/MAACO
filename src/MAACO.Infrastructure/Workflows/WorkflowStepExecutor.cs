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
    IEnumerable<IWorkflowStepHandler> stepHandlers,
    IEventBus eventBus)
{
    private readonly IReadOnlyDictionary<string, IWorkflowStepHandler> handlers =
        stepHandlers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

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

            if (handlers.TryGetValue(step.Name, out var handler))
            {
                await handler.ExecuteAsync(context, step, cancellationToken);
            }

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
