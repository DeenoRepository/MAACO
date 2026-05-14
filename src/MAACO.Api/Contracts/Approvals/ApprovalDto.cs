namespace MAACO.Api.Contracts.Approvals;

public sealed record ApprovalDto(
    Guid Id,
    Guid WorkflowId,
    string Status,
    string Mode,
    string? RequestedBy,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);
