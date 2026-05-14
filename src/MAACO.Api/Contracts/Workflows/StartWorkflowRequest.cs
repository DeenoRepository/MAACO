namespace MAACO.Api.Contracts.Workflows;

public sealed record StartWorkflowRequest(
    Guid TaskId,
    string Trigger = "ui-task-start");
