namespace MAACO.Api.Services;

public sealed class ProjectBuildTestCommandDetector(ISettingsService settingsService) : IProjectBuildTestCommandDetector
{
    public async Task<BuildTestCommandDetectionResult> DetectAsync(
        ProjectStackDetectionResult stack,
        IReadOnlyList<string> packageManifests,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.BuildCommandOverride) || !string.IsNullOrWhiteSpace(settings.TestCommandOverride))
        {
            return new BuildTestCommandDetectionResult(
                settings.BuildCommandOverride ?? InferBuildCommand(stack, packageManifests),
                settings.TestCommandOverride ?? InferTestCommand(stack, packageManifests),
                IsOverrideApplied: true);
        }

        return new BuildTestCommandDetectionResult(
            InferBuildCommand(stack, packageManifests),
            InferTestCommand(stack, packageManifests),
            IsOverrideApplied: false);
    }

    private static string InferBuildCommand(ProjectStackDetectionResult stack, IReadOnlyList<string> manifests)
    {
        if (stack.HasDotNet)
        {
            return "dotnet build";
        }

        if (stack.HasNodeJs)
        {
            if (HasPackageJson(manifests))
            {
                return "npm run build";
            }

            return "npm run build";
        }

        if (stack.HasPython)
        {
            return "python -m compileall .";
        }

        return "echo \"No build command detected\"";
    }

    private static string InferTestCommand(ProjectStackDetectionResult stack, IReadOnlyList<string> manifests)
    {
        if (stack.HasDotNet)
        {
            return "dotnet test";
        }

        if (stack.HasNodeJs)
        {
            if (HasPackageJson(manifests))
            {
                return "npm test";
            }

            return "npm test";
        }

        if (stack.HasPython)
        {
            return "pytest";
        }

        return "echo \"No test command detected\"";
    }

    private static bool HasPackageJson(IReadOnlyList<string> manifests) =>
        manifests.Any(x => x.EndsWith("package.json", StringComparison.OrdinalIgnoreCase));
}
