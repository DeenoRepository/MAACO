using MAACO.App.Services.Models;
using System.Net.Http.Json;

namespace MAACO.App.Services;

public sealed class TasksClient(HttpClient httpClient) : ITasksClient
{
    public async Task<TaskCreateResult> CreateTaskAsync(Guid projectId, string title, string? description, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/tasks",
                new
                {
                    ProjectId = projectId,
                    Title = title,
                    Description = description
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = string.IsNullOrWhiteSpace(details)
                    ? $"HTTP {(int)response.StatusCode}"
                    : $"HTTP {(int)response.StatusCode}: {Trim(details)}";
                return new TaskCreateResult(null, message);
            }

            var task = await response.Content.ReadFromJsonAsync<TaskDto>(cancellationToken: cancellationToken);
            return task is null
                ? new TaskCreateResult(null, "API returned empty response body.")
                : new TaskCreateResult(task, null);
        }
        catch (Exception ex)
        {
            return new TaskCreateResult(null, ex.Message);
        }
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/tasks/{taskId:D}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TaskDto>(cancellationToken: cancellationToken);
    }

    public async Task<TaskDiffResponse?> GetTaskDiffAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/tasks/{taskId:D}/diff", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TaskDiffResponse>(cancellationToken: cancellationToken);
    }

    public async Task<TaskActionResponse?> CommitTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync($"api/tasks/{taskId:D}/commit", content: null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TaskActionResponse>(cancellationToken: cancellationToken);
    }

    public async Task<TaskActionResponse?> RollbackTaskAsync(Guid taskId, string? reason, CancellationToken cancellationToken)
    {
        var payload = new { Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim() };
        var response = await httpClient.PostAsJsonAsync($"api/tasks/{taskId:D}/rollback", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TaskActionResponse>(cancellationToken: cancellationToken);
    }

    private static string Trim(string value) =>
        value.Length <= 240 ? value : value[..240];
}
