using MAACO.App.Services.Models;

namespace MAACO.App.Services;

public interface ITasksClient
{
    Task<TaskDto?> CreateTaskAsync(Guid projectId, string title, string? description, CancellationToken cancellationToken);
    Task<TaskDiffResponse?> GetTaskDiffAsync(Guid taskId, CancellationToken cancellationToken);
}
