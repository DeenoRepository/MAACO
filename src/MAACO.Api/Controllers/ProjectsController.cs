using FluentValidation;
using FluentValidation.AspNetCore;
using MAACO.Api.Contracts.Projects;
using MAACO.Api.Services;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(
    IProjectRepository projectRepository,
    IValidator<CreateProjectRequest> createProjectRequestValidator,
    IProjectPathValidator projectPathValidator) : ControllerBase
{
    public sealed record StartProjectScanResponse(Guid ProjectId, string Status, string Message);

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetProjects(CancellationToken cancellationToken)
    {
        var projects = await projectRepository.ListAsync(cancellationToken);
        return Ok(projects.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
        {
            return this.NotFoundError("Project not found.");
        }

        return Ok(Map(project));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDto>> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await createProjectRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return this.ValidationError();
        }

        var pathValidation = await projectPathValidator.ValidateAsync(request.RepositoryPath, cancellationToken);
        if (!pathValidation.IsValid)
        {
            ModelState.AddModelError(nameof(request.RepositoryPath), pathValidation.ErrorMessage ?? "Invalid repository path.");
            return this.ValidationError();
        }

        var project = new Project
        {
            Name = request.Name.Trim(),
            RepositoryPath = new RepositoryPath(pathValidation.NormalizedPath!)
        };

        await projectRepository.AddAsync(project, cancellationToken);
        await projectRepository.SaveChangesAsync(cancellationToken);

        return Created($"/api/projects/{project.Id}", Map(project));
    }

    [HttpPost("{id:guid}/scan")]
    [ProducesResponseType(typeof(StartProjectScanResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StartProjectScanResponse>> StartScan(Guid id, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
        {
            return this.NotFoundError("Project not found.");
        }

        return Accepted(new StartProjectScanResponse(
            id,
            "Queued",
            "Project scan request accepted. Scanner workflow will be wired in Milestone 5."));
    }

    private static ProjectDto Map(Project project) =>
        new(
            project.Id,
            project.Name,
            project.RepositoryPath.Value,
            project.CreatedAt,
            project.UpdatedAt,
            project.Version);
}
