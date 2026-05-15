using MAACO.App.Services.Models;

namespace MAACO.App.Services;

public interface IProjectsClient
{
    Task<IReadOnlyList<ProjectDto>> ListProjectsAsync(CancellationToken cancellationToken);
    Task<ProjectCreateResult> CreateProjectAsync(string name, string repositoryPath, CancellationToken cancellationToken);
    Task<ProjectScanResponse?> ScanProjectAsync(Guid projectId, CancellationToken cancellationToken);
}
