using MAACO.App.Services.Models;
using System.Net.Http.Json;

namespace MAACO.App.Services;

public sealed class ProjectsClient(HttpClient httpClient) : IProjectsClient
{
    public async Task<IReadOnlyList<ProjectDto>> ListProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await httpClient.GetFromJsonAsync<IReadOnlyList<ProjectDto>>("api/projects", cancellationToken);
        return projects ?? [];
    }

    public async Task<ProjectCreateResult> CreateProjectAsync(string name, string repositoryPath, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/projects",
                new { Name = name, RepositoryPath = repositoryPath },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = string.IsNullOrWhiteSpace(details)
                    ? $"HTTP {(int)response.StatusCode}"
                    : $"HTTP {(int)response.StatusCode}: {Trim(details)}";
                return new ProjectCreateResult(null, message);
            }

            var project = await response.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: cancellationToken);
            return project is null
                ? new ProjectCreateResult(null, "API returned empty response body.")
                : new ProjectCreateResult(project, null);
        }
        catch (Exception ex)
        {
            return new ProjectCreateResult(null, ex.Message);
        }
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

    private static string Trim(string value) =>
        value.Length <= 240 ? value : value[..240];
}
