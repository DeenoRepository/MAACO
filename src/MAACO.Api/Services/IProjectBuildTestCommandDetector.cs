namespace MAACO.Api.Services;

public interface IProjectBuildTestCommandDetector
{
    Task<BuildTestCommandDetectionResult> DetectAsync(
        ProjectStackDetectionResult stack,
        IReadOnlyList<string> packageManifests,
        CancellationToken cancellationToken);
}
