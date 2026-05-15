using MAACO.App.Services.Models;
using System.Net.Http.Json;

namespace MAACO.App.Services;

public sealed class WorkflowsClient(HttpClient httpClient) : IWorkflowsClient
{
    public async Task<WorkflowStartResult> StartWorkflowAsync(Guid taskId, string trigger, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/workflows/start",
                new { TaskId = taskId, Trigger = trigger },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = string.IsNullOrWhiteSpace(details)
                    ? $"HTTP {(int)response.StatusCode}"
                    : $"HTTP {(int)response.StatusCode}: {Trim(details)}";
                return new WorkflowStartResult(null, message);
            }

            var result = await response.Content.ReadFromJsonAsync<WorkflowStartResponse>(cancellationToken: cancellationToken);
            return result is null
                ? new WorkflowStartResult(null, "API returned empty response body.")
                : new WorkflowStartResult(result, null);
        }
        catch (Exception ex)
        {
            return new WorkflowStartResult(null, ex.Message);
        }
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

    private static string Trim(string value) =>
        value.Length <= 240 ? value : value[..240];
}
