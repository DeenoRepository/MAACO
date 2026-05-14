namespace MAACO.Api.Services;

public interface IProjectScanner
{
    Task<ProjectScanResult> ScanAsync(string repositoryPath, CancellationToken cancellationToken);
}
