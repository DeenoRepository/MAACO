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
            return NotFound();
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
            return ValidationProblem(ModelState);
        }

        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return NotFound($"Project {request.ProjectId} not found.");
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
            return NotFound();
        }

        task.Status = DomainTaskStatus.Cancelled;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await taskRepository.SaveChangesAsync(cancellationToken);

        return Ok(Map(task));
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
