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

        return Accepted(new TaskActionResponse(
            id,
            "Queued",
            "Commit request accepted. Git execution will be implemented in Milestone 8."));
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

        return Accepted(new TaskActionResponse(
            id,
            "Queued",
            $"Rollback request accepted. Git execution will be implemented in Milestone 8.{reasonSuffix}"));
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

