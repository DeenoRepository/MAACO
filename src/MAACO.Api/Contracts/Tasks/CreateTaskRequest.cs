namespace MAACO.Api.Contracts.Tasks;

public sealed record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string? Description);
