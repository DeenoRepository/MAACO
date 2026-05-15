using MAACO.App.Services.Models;

namespace MAACO.App.Services;

public interface ITasksClient
{
    Task<TaskCreateResult> CreateTaskAsync(Guid projectId, string title, string? description, CancellationToken cancellationToken);
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<TaskDiffResponse?> GetTaskDiffAsync(Guid taskId, CancellationToken cancellationToken);
    Task<TaskActionResponse?> CommitTaskAsync(Guid taskId, CancellationToken cancellationToken);
    Task<TaskActionResponse?> RollbackTaskAsync(Guid taskId, string? reason, CancellationToken cancellationToken);
}
