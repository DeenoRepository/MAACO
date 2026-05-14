namespace MAACO.Api.Services;

public interface IProjectPathValidator
{
    Task<ProjectPathValidationResult> ValidateAsync(string path, CancellationToken cancellationToken);
}
