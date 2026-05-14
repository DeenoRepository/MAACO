namespace MAACO.Api.Services;

public sealed record ProjectPathValidationResult(
    bool IsValid,
    string? NormalizedPath,
    string? ErrorMessage);
