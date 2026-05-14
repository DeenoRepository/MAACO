namespace MAACO.Api.Contracts.Common;

public sealed record ApiError(
    string Code,
    string Message,
    IDictionary<string, string[]>? Details,
    string? TraceId);
