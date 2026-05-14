using FluentValidation;
using FluentValidation.AspNetCore;
using MAACO.Api.Contracts.Projects;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(
    IProjectRepository projectRepository,
    IValidator<CreateProjectRequest> createProjectRequestValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetProjects(CancellationToken cancellationToken)
    {
        var projects = await projectRepository.ListAsync(cancellationToken);
        return Ok(projects.Select(Map).ToList());
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
            return ValidationProblem(ModelState);
        }

        var project = new Project
        {
            Name = request.Name.Trim(),
            RepositoryPath = new RepositoryPath(request.RepositoryPath.Trim())
        };

        await projectRepository.AddAsync(project, cancellationToken);
        await projectRepository.SaveChangesAsync(cancellationToken);

        return Created($"/api/projects/{project.Id}", Map(project));
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
