using FluentValidation;
using FluentValidation.AspNetCore;
using MAACO.Api.Contracts.Projects;
using MAACO.Api.Services;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(
    IProjectRepository projectRepository,
    IProjectContextSnapshotRepository snapshotRepository,
    IValidator<CreateProjectRequest> createProjectRequestValidator,
    IProjectPathValidator projectPathValidator,
    IProjectScanner projectScanner,
    IProjectStackDetector projectStackDetector,
    IProjectBuildTestCommandDetector buildTestCommandDetector) : ControllerBase
{
    public sealed record StartProjectScanResponse(
        Guid ProjectId,
        string Status,
        string Message,
        int ScannedFiles,
        int SkippedBySize,
        int SkippedByLimit,
        string PrimaryStack,
        bool HasDotNet,
        bool HasNodeJs,
        bool HasPython,
        IReadOnlyList<string> SolutionFiles,
        IReadOnlyList<string> ProjectFiles,
        IReadOnlyList<string> PackageManifests,
        string BuildCommand,
        string TestCommand,
        bool IsCommandOverrideApplied);

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

        var existing = (await projectRepository.ListAsync(cancellationToken))
            .FirstOrDefault(x => string.Equals(
                x.RepositoryPath.Value,
                project.RepositoryPath.Value,
                StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return Ok(Map(existing));
        }

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

        var scanResult = await projectScanner.ScanAsync(project.RepositoryPath.Value, cancellationToken);
        var stackResult = await projectStackDetector.DetectAsync(
            project.RepositoryPath.Value,
            scanResult.Files,
            cancellationToken);
        var commandResult = await buildTestCommandDetector.DetectAsync(
            stackResult,
            stackResult.PackageManifests,
            cancellationToken);
        var summary = $"Stack: {stackResult.PrimaryStack}. Files scanned: {scanResult.ScannedFiles}.";
        var keyFiles = stackResult.SolutionFiles
            .Concat(stackResult.ProjectFiles)
            .Concat(stackResult.PackageManifests)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToArray();

        var snapshot = new ProjectContextSnapshot
        {
            ProjectId = project.Id,
            BranchName = "unknown",
            CommitHash = "unknown",
            Stack = new DetectedProjectStack(
                stackResult.PrimaryStack,
                stackResult.PrimaryStack == "Generic" ? "unknown" : "detected"),
            MetadataJson = JsonSerializer.Serialize(new
            {
                Summary = summary,
                KeyFiles = keyFiles,
                BuildCommand = commandResult.BuildCommand,
                TestCommand = commandResult.TestCommand
            })
        };

        await snapshotRepository.AddAsync(snapshot, cancellationToken);
        await snapshotRepository.SaveChangesAsync(cancellationToken);

        return Accepted(new StartProjectScanResponse(
            id,
            "Completed",
            "Project scan completed.",
            scanResult.ScannedFiles,
            scanResult.SkippedBySize,
            scanResult.SkippedByLimit,
            stackResult.PrimaryStack,
            stackResult.HasDotNet,
            stackResult.HasNodeJs,
            stackResult.HasPython,
            stackResult.SolutionFiles,
            stackResult.ProjectFiles,
            stackResult.PackageManifests,
            commandResult.BuildCommand,
            commandResult.TestCommand,
            commandResult.IsOverrideApplied));
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
