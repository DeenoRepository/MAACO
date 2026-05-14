using MAACO.Api.Contracts.Workflows;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/workflows")]
public sealed class WorkflowsController(
    IWorkflowRepository workflowRepository,
    ILogRepository logRepository,
    IArtifactRepository artifactRepository) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return NotFound();
        }

        return Ok(Map(workflow));
    }

    [HttpGet("{id:guid}/steps")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowStepDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkflowStepDto>>> GetWorkflowSteps(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await workflowRepository.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return NotFound();
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
            return NotFound();
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
            return NotFound();
        }

        var artifacts = await artifactRepository.ListByTaskIdAsync(workflow.TaskId, cancellationToken);
        return Ok(artifacts.Select(Map).ToList());
    }

    private static WorkflowDto Map(Workflow workflow) =>
        new(
            workflow.Id,
            workflow.TaskId,
            workflow.Status.ToString(),
            workflow.RetryCount,
            workflow.CreatedAt,
            workflow.UpdatedAt,
            workflow.Version);

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
