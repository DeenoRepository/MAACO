using MAACO.App.Services.Models;
using System.Net.Http.Json;

namespace MAACO.App.Services;

public sealed class TasksClient(HttpClient httpClient) : ITasksClient
{
    public async Task<TaskDto?> CreateTaskAsync(Guid projectId, string title, string? description, CancellationToken cancellationToken)
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
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TaskDto>(cancellationToken: cancellationToken);
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
}
