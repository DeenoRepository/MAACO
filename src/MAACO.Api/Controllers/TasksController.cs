using FluentValidation;
using FluentValidation.AspNetCore;
using MAACO.Api.Contracts.Tasks;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using DomainTaskStatus = MAACO.Core.Domain.Enums.TaskStatus;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository,
    IWorkflowRepository workflowRepository,
    IArtifactRepository artifactRepository,
    ILogRepository logRepository,
    IValidator<CreateTaskRequest> createTaskRequestValidator) : ControllerBase
{
    public sealed record TaskDiffResponse(Guid TaskId, string Diff, string Status);
    public sealed record TaskActionResponse(Guid TaskId, string Status, string Message);
    public sealed record RejectTaskRequest(string? Reason);

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetTasks(CancellationToken cancellationToken)
    {
        var tasks = await taskRepository.ListAsync(cancellationToken);
        return Ok(tasks.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return this.NotFoundError("Task not found.");
        }

        return Ok(Map(task));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await createTaskRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return this.ValidationError();
        }

        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return this.NotFoundError($"Project {request.ProjectId} not found.");
        }

        var task = new TaskItem
        {
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = DomainTaskStatus.Created
        };

        await taskRepository.AddAsync(task, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Created($"/api/tasks/{task.Id}", Map(task));
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> CancelTask(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return this.NotFoundError("Task not found.");
        }

        task.Status = DomainTaskStatus.Cancelled;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Ok(Map(task));
    }

    [HttpGet("{id:guid}/diff")]
    [ProducesResponseType(typeof(TaskDiffResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDiffResponse>> GetTaskDiff(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return this.NotFoundError("Task not found.");
        }

        return Ok(new TaskDiffResponse(
            id,
            "Diff generation is not connected yet. Git integration is planned in Milestone 8.",
            "NotAvailable"));
    }

    [HttpPost("{id:guid}/commit")]
    [ProducesResponseType(typeof(TaskActionResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskActionResponse>> CommitTask(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return this.NotFoundError("Task not found.");
        }

        var workflow = await workflowRepository.GetLatestByTaskIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return this.NotFoundError("Workflow not found for task.");
        }

        var steps = await workflowRepository.ListStepsAsync(workflow.Id, cancellationToken);
        var commitStep = steps.FirstOrDefault(x => string.Equals(x.Name, "CommitStep", StringComparison.OrdinalIgnoreCase));
        var rollbackStep = steps.FirstOrDefault(x => string.Equals(x.Name, "RollbackStep", StringComparison.OrdinalIgnoreCase));
        if (commitStep is not null)
        {
            commitStep.Status = MAACO.Core.Domain.Enums.WorkflowStepStatus.Completed;
            commitStep.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (rollbackStep is not null && rollbackStep.Status == MAACO.Core.Domain.Enums.WorkflowStepStatus.Pending)
        {
            rollbackStep.Status = MAACO.Core.Domain.Enums.WorkflowStepStatus.Cancelled;
            rollbackStep.UpdatedAt = DateTimeOffset.UtcNow;
        }

        workflow.Status = MAACO.Core.Domain.Enums.WorkflowStatus.Completed;
        workflow.UpdatedAt = DateTimeOffset.UtcNow;
        task.Status = DomainTaskStatus.Completed;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = workflow.Id,
                TaskId = task.Id,
                Severity = MAACO.Core.Domain.Enums.LogSeverity.Information,
                Message = $"Commit approved for task {task.Id:D}.",
                CorrelationId = $"approval-commit-{task.Id:D}"
            },
            cancellationToken);

        await workflowRepository.SaveChangesAsync(cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);

        return Accepted(new TaskActionResponse(
            id,
            "Committed",
            "Commit executed after approval."));
    }

    [HttpPost("{id:guid}/rollback")]
    [ProducesResponseType(typeof(TaskActionResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskActionResponse>> RollbackTask(
        Guid id,
        [FromBody] RejectTaskRequest? request,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return this.NotFoundError("Task not found.");
        }

        var reasonSuffix = string.IsNullOrWhiteSpace(request?.Reason)
            ? string.Empty
            : $" Reason: {request.Reason.Trim()}";

        var workflow = await workflowRepository.GetLatestByTaskIdAsync(id, cancellationToken);
        if (workflow is null)
        {
            return this.NotFoundError("Workflow not found for task.");
        }

        var steps = await workflowRepository.ListStepsAsync(workflow.Id, cancellationToken);
        var commitStep = steps.FirstOrDefault(x => string.Equals(x.Name, "CommitStep", StringComparison.OrdinalIgnoreCase));
        var rollbackStep = steps.FirstOrDefault(x => string.Equals(x.Name, "RollbackStep", StringComparison.OrdinalIgnoreCase));
        if (rollbackStep is not null)
        {
            rollbackStep.Status = MAACO.Core.Domain.Enums.WorkflowStepStatus.Completed;
            rollbackStep.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (commitStep is not null && commitStep.Status == MAACO.Core.Domain.Enums.WorkflowStepStatus.Pending)
        {
            commitStep.Status = MAACO.Core.Domain.Enums.WorkflowStepStatus.Cancelled;
            commitStep.UpdatedAt = DateTimeOffset.UtcNow;
        }

        workflow.Status = MAACO.Core.Domain.Enums.WorkflowStatus.RolledBack;
        workflow.UpdatedAt = DateTimeOffset.UtcNow;
        task.Status = DomainTaskStatus.RolledBack;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = workflow.Id,
                TaskId = task.Id,
                Severity = MAACO.Core.Domain.Enums.LogSeverity.Warning,
                Message = $"Rollback executed for task {task.Id:D}.{reasonSuffix}",
                CorrelationId = $"approval-rollback-{task.Id:D}"
            },
            cancellationToken);

        var patchCleanupCount = await CleanupUnappliedPatchArtifactsAsync(workflow, task, cancellationToken);

        await artifactRepository.AddAsync(
            new Artifact
            {
                TaskId = task.Id,
                Type = MAACO.Core.Domain.Enums.ArtifactType.Snapshot,
                Path = $"rollback://task/{task.Id:D}",
                Hash = $"workflow:{workflow.Id:D};status:{workflow.Status};reason:{request?.Reason?.Trim() ?? "none"};unappliedPatchCleanup:{patchCleanupCount}"
            },
            cancellationToken);

        await workflowRepository.SaveChangesAsync(cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);
        await artifactRepository.SaveChangesAsync(cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);

        return Accepted(new TaskActionResponse(
            id,
            "RolledBack",
            $"Rollback executed after approval.{reasonSuffix}"));
    }

    private async Task<int> CleanupUnappliedPatchArtifactsAsync(
        Workflow workflow,
        TaskItem task,
        CancellationToken cancellationToken)
    {
        var artifacts = await artifactRepository.ListByTaskIdAsync(task.Id, cancellationToken);
        var patchArtifacts = artifacts
            .Where(x =>
                x.Type == MAACO.Core.Domain.Enums.ArtifactType.Patch &&
                x.Hash?.Contains($"workflow={workflow.Id:D}", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        if (patchArtifacts.Count == 0)
        {
            return 0;
        }

        var logs = await logRepository.ListByWorkflowIdAsync(workflow.Id, cancellationToken);
        var patchWasApplied = logs.Any(x => x.Message.Contains("PatchApplied=True", StringComparison.Ordinal));
        if (patchWasApplied)
        {
            return 0;
        }

        var cleaned = 0;
        foreach (var patchArtifact in patchArtifacts)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(patchArtifact.Path) &&
                    System.IO.File.Exists(patchArtifact.Path))
                {
                    System.IO.File.Delete(patchArtifact.Path);
                }

                cleaned++;
            }
            catch
            {
                // Best-effort cleanup; rollback should proceed even if patch file cleanup fails.
            }
        }

        if (cleaned > 0)
        {
            await logRepository.AddAsync(
                new LogEvent
                {
                    WorkflowId = workflow.Id,
                    TaskId = task.Id,
                    Severity = MAACO.Core.Domain.Enums.LogSeverity.Information,
                    Message = $"Rollback cleanup removed {cleaned} unapplied patch artifact file(s).",
                    CorrelationId = $"approval-rollback-{task.Id:D}"
                },
                cancellationToken);
        }

        return cleaned;
    }

    private static TaskDto Map(TaskItem task) =>
        new(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.CreatedAt,
            task.UpdatedAt,
            task.Version);
}

