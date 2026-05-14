namespace MAACO.Api.Contracts.Projects;

public sealed record CreateProjectRequest(
    string Name,
    string RepositoryPath);
