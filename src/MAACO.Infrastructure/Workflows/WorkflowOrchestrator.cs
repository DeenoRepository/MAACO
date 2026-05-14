using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;

namespace MAACO.Infrastructure.Workflows;

public sealed class WorkflowOrchestrator(
    IWorkflowRepository workflowRepository,
    ILogRepository logRepository,
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

        var executionContext = context with { WorkflowId = workflow.Id };

        workflow.Status = WorkflowStatus.Running;
        await workflowRepository.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(
            new WorkflowStartedEvent(
                workflow.Id,
                workflow.TaskId,
                DateTimeOffset.UtcNow,
                executionContext.CorrelationId),
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
            var outcome = await stepExecutor.ExecuteAsync(executionContext, steps, cancellationToken);
            if (outcome == WorkflowExecutionOutcome.WaitingForApproval)
            {
                workflow.Status = WorkflowStatus.WaitingForApproval;
                await workflowRepository.SaveChangesAsync(cancellationToken);
                return workflow;
            }
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
        catch (Exception ex)
        {
            workflow.Status = WorkflowStatus.Failed;
            await workflowRepository.SaveChangesAsync(CancellationToken.None);

            await logRepository.AddAsync(
                new LogEvent
                {
                    WorkflowId = workflow.Id,
                    TaskId = workflow.TaskId,
                    Severity = LogSeverity.Error,
                    CorrelationId = context.CorrelationId,
                    Message = $"Workflow {workflow.Id:D} failed: {ex.Message}"
                },
                CancellationToken.None);
            await logRepository.SaveChangesAsync(CancellationToken.None);

            await eventBus.PublishAsync(
                new WorkflowFailedEvent(
                    workflow.Id,
                    ex.Message,
                    DateTimeOffset.UtcNow,
                    executionContext.CorrelationId),
                CancellationToken.None);
            throw;
        }

        workflow.Status = WorkflowStatus.Completed;
        await workflowRepository.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(
            new WorkflowCompletedEvent(
                workflow.Id,
                DateTimeOffset.UtcNow,
                executionContext.CorrelationId),
            cancellationToken);

        return workflow;
    }
}
