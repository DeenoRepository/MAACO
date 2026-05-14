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
    ILogRepository logRepository,
    IEnumerable<IWorkflowStepHandler> stepHandlers,
    IEventBus eventBus)
{
    private const int DefaultMaxDebugRetries = 3;
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

            try
            {
                if (handlers.TryGetValue(step.Name, out var handler))
                {
                    await handler.ExecuteAsync(context, step, cancellationToken);
                }
            }
            catch (Exception ex) when (IsDebuggableStep(step.Name))
            {
                var maxRetries = ResolveMaxDebugRetries(context);
                await logRepository.AddAsync(
                    new LogEvent
                    {
                        WorkflowId = context.WorkflowId,
                        TaskId = context.TaskId,
                        Severity = LogSeverity.Warning,
                        CorrelationId = context.CorrelationId,
                        Message = $"{step.Name} failed. Starting debug loop with max retries {maxRetries}. Reason: {ex.Message}"
                    },
                    cancellationToken);
                await logRepository.SaveChangesAsync(cancellationToken);

                var succeeded = await ExecuteDebugLoopAsync(context, step, maxRetries, ex.Message, cancellationToken);
                if (!succeeded)
                {
                    step.Status = WorkflowStepStatus.Failed;
                    await workflowRepository.SaveChangesAsync(cancellationToken);

                    await eventBus.PublishAsync(
                        new WorkflowStepFailedEvent(
                            step.Id,
                            context.WorkflowId,
                            $"Step {step.Name} failed after {maxRetries} retries.",
                            DateTimeOffset.UtcNow,
                            context.CorrelationId),
                        cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                step.Status = WorkflowStepStatus.Failed;
                await workflowRepository.SaveChangesAsync(cancellationToken);

                await eventBus.PublishAsync(
                    new WorkflowStepFailedEvent(
                        step.Id,
                        context.WorkflowId,
                        ex.Message,
                        DateTimeOffset.UtcNow,
                        context.CorrelationId),
                    cancellationToken);
                throw;
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

    private async Task<bool> ExecuteDebugLoopAsync(
        WorkflowExecutionContext context,
        WorkflowStep failedStep,
        int maxRetries,
        string reason,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await IncrementWorkflowRetryCountAsync(context.WorkflowId, cancellationToken);
            await logRepository.AddAsync(
                new LogEvent
                {
                    WorkflowId = context.WorkflowId,
                    TaskId = context.TaskId,
                    Severity = LogSeverity.Warning,
                    CorrelationId = context.CorrelationId,
                    Message = $"Debug attempt {attempt}/{maxRetries} for {failedStep.Name}. Reason: {reason}"
                },
                cancellationToken);
            await logRepository.SaveChangesAsync(cancellationToken);

            if (handlers.TryGetValue("DebugStep", out var debugHandler))
            {
                await debugHandler.ExecuteAsync(context, failedStep, cancellationToken);
            }

            if (handlers.TryGetValue("PatchApplicationStep", out var patchHandler))
            {
                await patchHandler.ExecuteAsync(context, failedStep, cancellationToken);
            }

            try
            {
                if (handlers.TryGetValue(failedStep.Name, out var failedStepHandler))
                {
                    await failedStepHandler.ExecuteAsync(context, failedStep, cancellationToken);
                }

                await logRepository.AddAsync(
                    new LogEvent
                    {
                        WorkflowId = context.WorkflowId,
                        TaskId = context.TaskId,
                        Severity = LogSeverity.Information,
                        CorrelationId = context.CorrelationId,
                        Message = $"{failedStep.Name} recovered on debug attempt {attempt}."
                    },
                    cancellationToken);
                await logRepository.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception retryEx)
            {
                reason = retryEx.Message;
                await logRepository.AddAsync(
                    new LogEvent
                    {
                        WorkflowId = context.WorkflowId,
                        TaskId = context.TaskId,
                        Severity = LogSeverity.Warning,
                        CorrelationId = context.CorrelationId,
                        Message = $"{failedStep.Name} still failing after debug attempt {attempt}: {retryEx.Message}"
                    },
                    cancellationToken);
                await logRepository.SaveChangesAsync(cancellationToken);
            }
        }

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Error,
                CorrelationId = context.CorrelationId,
                Message = $"{failedStep.Name} reached max debug retries ({maxRetries})."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
        return false;
    }

    private async Task IncrementWorkflowRetryCountAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(workflowId, cancellationToken);
        if (workflow is null)
        {
            return;
        }

        workflow.RetryCount++;
        await workflowRepository.SaveChangesAsync(cancellationToken);
    }

    private static bool IsDebuggableStep(string stepName) =>
        stepName.Equals("BuildStep", StringComparison.OrdinalIgnoreCase) ||
        stepName.Equals("TestStep", StringComparison.OrdinalIgnoreCase);

    private static int ResolveMaxDebugRetries(WorkflowExecutionContext context)
    {
        if (context.Inputs is not null &&
            context.Inputs.TryGetValue("MaxDebugRetries", out var configured) &&
            int.TryParse(configured, out var parsed) &&
            parsed > 0)
        {
            return parsed;
        }

        return DefaultMaxDebugRetries;
    }
}
