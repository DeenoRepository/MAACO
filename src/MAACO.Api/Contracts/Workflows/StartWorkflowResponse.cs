namespace MAACO.Api.Contracts.Workflows;

public sealed record StartWorkflowResponse(
    Guid WorkflowId,
    string Status,
    string Message);
