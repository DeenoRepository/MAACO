using MAACO.Api.Contracts.Workflows;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/workflows")]
public sealed class WorkflowsController(
    IWorkflowRepository workflowRepository,
    ILogRepository logRepository,
    IArtifactRepository artifactRepository,
    ITaskRepository taskRepository,
    IServiceScopeFactory scopeFactory) : ControllerBase
{
    private static readonly string[] DefaultWorkflowSteps =
    [
        "ProjectScanStep",
        "PlanningStep",
        "CodeGenerationStep",
        "PatchApplicationStep",
        "TestGenerationStep",
        "BuildStep",
        "TestStep",
        "DebugStep",
        "DiffStep",
        "ApprovalStep",
        "CommitStep",
        "RollbackStep",
        "DocumentationStep",
        "FinalReportStep"
    ];

    [HttpPost("start")]
    [ProducesResponseType(typeof(StartWorkflowResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StartWorkflowResponse>> StartWorkflow(
        [FromBody] StartWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return this.NotFoundError($"Task {request.TaskId} not found.");
        }

        var workflow = new Workflow
        {
            TaskId = task.Id,
            Status = MAACO.Core.Domain.Enums.WorkflowStatus.Created
        };
        await workflowRepository.AddWorkflowAsync(workflow, cancellationToken);
        await workflowRepository.SaveChangesAsync(cancellationToken);

        var workflowId = workflow.Id;
        var correlationId = $"wf-start-{workflowId:N}";

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
                await orchestrator.ExecuteAsync(
                    new WorkflowExecutionContext(
                        task.ProjectId,
                        task.Id,
                        workflowId,
                        request.Trigger,
                        correlationId),
                    DefaultWorkflowSteps,
                    CancellationToken.None);
            }
            catch
            {
                // Failures are persisted by orchestrator/log pipeline.
            }
        });

        return Accepted(new StartWorkflowResponse(
            workflowId,
            "Queued",
            "Workflow start accepted."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return this.NotFoundError("Workflow not found.");
        }

        var failureReason = await ResolveFailureReasonAsync(workflow, cancellationToken);
        return Ok(Map(workflow, failureReason));
    }

    [HttpGet("{id:guid}/steps")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowStepDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkflowStepDto>>> GetWorkflowSteps(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return this.NotFoundError("Workflow not found.");
        }

        var steps = await workflowRepository.ListStepsAsync(id, cancellationToken);
        return Ok(steps.Select(Map).ToList());
    }

    [HttpGet("{id:guid}/logs")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkflowLogDto>>> GetWorkflowLogs(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return this.NotFoundError("Workflow not found.");
        }

        var logs = await logRepository.ListByWorkflowIdAsync(id, cancellationToken);
        return Ok(logs.Select(Map).ToList());
    }

    [HttpGet("{id:guid}/artifacts")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowArtifactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkflowArtifactDto>>> GetWorkflowArtifacts(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return this.NotFoundError("Workflow not found.");
        }

        var artifacts = await artifactRepository.ListByTaskIdAsync(workflow.TaskId, cancellationToken);
        return Ok(artifacts.Select(Map).ToList());
    }

    private static WorkflowDto Map(Workflow workflow, string? failureReason) =>
        new(
            workflow.Id,
            workflow.TaskId,
            workflow.Status.ToString(),
            failureReason,
            workflow.RetryCount,
            workflow.CreatedAt,
            workflow.UpdatedAt,
            workflow.Version);

    private async Task<string?> ResolveFailureReasonAsync(Workflow workflow, CancellationToken cancellationToken)
    {
        if (workflow.Status != MAACO.Core.Domain.Enums.WorkflowStatus.Failed)
        {
            return null;
        }

        var logs = await logRepository.ListByWorkflowIdAsync(workflow.Id, cancellationToken);
        var lastError = logs
            .Where(x => x.Severity == MAACO.Core.Domain.Enums.LogSeverity.Error)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        return lastError?.Message;
    }

    private static WorkflowStepDto Map(WorkflowStep step) =>
        new(
            step.Id,
            step.WorkflowId,
            step.Name,
            step.Status.ToString(),
            step.Order,
            step.CreatedAt,
            step.UpdatedAt,
            step.Version);

    private static WorkflowLogDto Map(LogEvent log) =>
        new(
            log.Id,
            log.WorkflowId,
            log.TaskId,
            log.Severity.ToString(),
            log.Message,
            log.CorrelationId,
            log.CreatedAt,
            log.UpdatedAt,
            log.Version);

    private static WorkflowArtifactDto Map(Artifact artifact) =>
        new(
            artifact.Id,
            artifact.TaskId,
            artifact.Type.ToString(),
            artifact.Path,
            artifact.Hash,
            artifact.CreatedAt,
            artifact.UpdatedAt,
            artifact.Version);
}
