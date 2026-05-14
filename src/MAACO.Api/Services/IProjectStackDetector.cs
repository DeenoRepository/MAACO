namespace MAACO.Api.Services;

public interface IProjectStackDetector
{
    Task<ProjectStackDetectionResult> DetectAsync(
        string repositoryPath,
        IReadOnlyList<string> scannedFiles,
        CancellationToken cancellationToken);
}
