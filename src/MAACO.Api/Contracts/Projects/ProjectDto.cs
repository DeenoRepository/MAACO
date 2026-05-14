namespace MAACO.Api.Contracts.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string RepositoryPath,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);
