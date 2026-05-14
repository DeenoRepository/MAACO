using MAACO.App.Services.Models;

namespace MAACO.App.Services;

public interface IWorkflowsClient
{
    Task<WorkflowStartResponse?> StartWorkflowAsync(Guid taskId, string trigger, CancellationToken cancellationToken);
    Task<WorkflowDto?> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken);
}
