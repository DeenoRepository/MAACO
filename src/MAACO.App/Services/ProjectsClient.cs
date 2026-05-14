using MAACO.App.Services.Models;
using System.Net.Http.Json;

namespace MAACO.App.Services;

public sealed class ProjectsClient(HttpClient httpClient) : IProjectsClient
{
    public async Task<ProjectDto?> CreateProjectAsync(string name, string repositoryPath, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/projects",
            new { Name = name, RepositoryPath = repositoryPath },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: cancellationToken);
    }

    public async Task<ProjectScanResponse?> ScanProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync($"api/projects/{projectId:D}/scan", content: null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProjectScanResponse>(cancellationToken: cancellationToken);
    }
}
