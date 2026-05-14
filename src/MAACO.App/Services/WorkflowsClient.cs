using MAACO.App.Services.Models;
using System.Net.Http.Json;

namespace MAACO.App.Services;

public sealed class WorkflowsClient(HttpClient httpClient) : IWorkflowsClient
{
    public async Task<WorkflowStartResponse?> StartWorkflowAsync(Guid taskId, string trigger, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/workflows/start",
            new { TaskId = taskId, Trigger = trigger },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<WorkflowStartResponse>(cancellationToken: cancellationToken);
    }

    public async Task<WorkflowDto?> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<WorkflowDto>($"api/workflows/{workflowId:D}", cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowStepDto>> GetWorkflowStepsAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var steps = await httpClient.GetFromJsonAsync<IReadOnlyList<WorkflowStepDto>>(
            $"api/workflows/{workflowId:D}/steps",
            cancellationToken);
        return steps ?? [];
    }
}
