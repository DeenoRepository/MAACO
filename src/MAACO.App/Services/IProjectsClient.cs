using MAACO.App.Services.Models;

namespace MAACO.App.Services;

public interface IProjectsClient
{
    Task<ProjectDto?> CreateProjectAsync(string name, string repositoryPath, CancellationToken cancellationToken);
    Task<ProjectScanResponse?> ScanProjectAsync(Guid projectId, CancellationToken cancellationToken);
}
