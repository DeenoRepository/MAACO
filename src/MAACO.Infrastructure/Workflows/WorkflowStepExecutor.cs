using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;
using System.Text.Json;

namespace MAACO.Infrastructure.Workflows;

public sealed class WorkflowStepExecutor(
    IWorkflowRepository workflowRepository,
    IArtifactRepository artifactRepository,
    ILogRepository logRepository,
    IEnumerable<IWorkflowStepHandler> stepHandlers,
    IEventBus eventBus)
{
    private const int DefaultMaxDebugRetries = 3;
    private const int MaxCheckpointPayloadLength = 2000;
    private readonly IReadOnlyDictionary<string, IWorkflowStepHandler> handlers =
        stepHandlers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public async Task<WorkflowExecutionOutcome> ExecuteAsync(
        WorkflowExecutionContext context,
        IReadOnlyList<WorkflowStep> steps,
        CancellationToken cancellationToken)
    {
        var waitingForApproval = false;
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
                await PersistStepInputCheckpointAsync(context, step, cancellationToken);

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
                    await PersistStepErrorCheckpointAsync(context, step, $"Failed after {maxRetries} debug retries. Root reason: {ex}", cancellationToken);
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
                await PersistStepErrorCheckpointAsync(context, step, ex.ToString(), cancellationToken);
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
            await PersistStepOutputCheckpointAsync(context, step, cancellationToken);

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

            if (string.Equals(step.Name, "ApprovalStep", StringComparison.OrdinalIgnoreCase))
            {
                waitingForApproval = true;
                await eventBus.PublishAsync(
                    new ApprovalRequestedEvent(
                        Guid.NewGuid(),
                        context.WorkflowId,
                        DateTimeOffset.UtcNow,
                        context.CorrelationId),
                    cancellationToken);
                break;
            }
        }

        return waitingForApproval
            ? WorkflowExecutionOutcome.WaitingForApproval
            : WorkflowExecutionOutcome.Completed;
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
                await ExecutePostPatchValidationAsync(context, failedStep, cancellationToken);

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

    private async Task ExecutePostPatchValidationAsync(
        WorkflowExecutionContext context,
        WorkflowStep failedStep,
        CancellationToken cancellationToken)
    {
        var replayedHandlers = new List<string>();

        if (handlers.TryGetValue("BuildStep", out var buildHandler))
        {
            await buildHandler.ExecuteAsync(context, failedStep, cancellationToken);
            replayedHandlers.Add("BuildStep");
        }

        if (handlers.TryGetValue("TestStep", out var testHandler))
        {
            await testHandler.ExecuteAsync(context, failedStep, cancellationToken);
            replayedHandlers.Add("TestStep");
        }

        // Fallback for minimal pipelines where only a custom failing step exists.
        if (replayedHandlers.Count == 0 &&
            handlers.TryGetValue(failedStep.Name, out var failedStepHandler))
        {
            await failedStepHandler.ExecuteAsync(context, failedStep, cancellationToken);
            replayedHandlers.Add(failedStep.Name);
        }

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Post-debug validation replayed: {string.Join(", ", replayedHandlers)}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
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

    private async Task PersistStepInputCheckpointAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        var payload = context.Inputs is null
            ? "{}"
            : JsonSerializer.Serialize(context.Inputs);

        await PersistStepCheckpointLogAsync(
            context,
            step,
            "input",
            payload,
            cancellationToken);
    }

    private async Task PersistStepOutputCheckpointAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            step.Name,
            StepOrder = step.Order,
            Status = step.Status.ToString(),
            CompletedAtUtc = DateTimeOffset.UtcNow
        });

        await PersistStepCheckpointLogAsync(
            context,
            step,
            "output",
            payload,
            cancellationToken);
    }

    private async Task PersistStepErrorCheckpointAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        string errorPayload,
        CancellationToken cancellationToken) =>
        await PersistStepCheckpointLogAsync(
            context,
            step,
            "error",
            errorPayload,
            cancellationToken);

    private async Task PersistStepCheckpointLogAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        string checkpointType,
        string payload,
        CancellationToken cancellationToken)
    {
        var normalizedPayload = payload.Length <= MaxCheckpointPayloadLength
            ? payload
            : $"{payload[..MaxCheckpointPayloadLength]}...(truncated)";

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"StepCheckpoint {checkpointType} {step.Name}#{step.Order}: {normalizedPayload}"
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }
}

public enum WorkflowExecutionOutcome
{
    Completed = 0,
    WaitingForApproval = 1
}
