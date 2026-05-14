using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;

namespace MAACO.Infrastructure.Workflows;

public sealed class WorkflowOrchestrator(
    IWorkflowRepository workflowRepository,
    WorkflowStepExecutor stepExecutor,
    IEventBus eventBus) : IWorkflowOrchestrator
{
    public async Task<Workflow> ExecuteAsync(
        WorkflowExecutionContext context,
        IReadOnlyList<string> stepNames,
        CancellationToken cancellationToken)
    {
        if (stepNames.Count == 0)
        {
            throw new ArgumentException("At least one workflow step is required.", nameof(stepNames));
        }

        var workflow = await workflowRepository.GetByIdAsync(context.WorkflowId, cancellationToken);
        if (workflow is null)
        {
            workflow = new Workflow
            {
                TaskId = context.TaskId,
                Status = WorkflowStatus.Created
            };

            await workflowRepository.AddWorkflowAsync(workflow, cancellationToken);
        }

        workflow.Status = WorkflowStatus.Running;
        await workflowRepository.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(
            new WorkflowStartedEvent(
                workflow.Id,
                workflow.TaskId,
                DateTimeOffset.UtcNow,
                context.CorrelationId),
            cancellationToken);

        var steps = new List<WorkflowStep>(stepNames.Count);
        for (var i = 0; i < stepNames.Count; i++)
        {
            var step = new WorkflowStep
            {
                WorkflowId = workflow.Id,
                Name = stepNames[i],
                Order = i + 1,
                Status = WorkflowStepStatus.Pending
            };

            await workflowRepository.AddStepAsync(step, cancellationToken);
            steps.Add(step);
        }

        await workflowRepository.SaveChangesAsync(cancellationToken);
        try
        {
            await stepExecutor.ExecuteAsync(context, steps, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            workflow.Status = WorkflowStatus.Cancelled;

            var persistedSteps = await workflowRepository.ListStepsAsync(workflow.Id, CancellationToken.None);
            foreach (var step in persistedSteps.Where(x => x.Status is WorkflowStepStatus.Pending or WorkflowStepStatus.Running))
            {
                step.Status = WorkflowStepStatus.Cancelled;
            }

            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        workflow.Status = WorkflowStatus.Completed;
        await workflowRepository.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(
            new WorkflowCompletedEvent(
                workflow.Id,
                DateTimeOffset.UtcNow,
                context.CorrelationId),
            cancellationToken);

        return workflow;
    }
}
