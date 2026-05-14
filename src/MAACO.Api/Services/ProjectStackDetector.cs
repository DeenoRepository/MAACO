namespace MAACO.Api.Services;

public sealed class ProjectStackDetector : IProjectStackDetector
{
    public Task<ProjectStackDetectionResult> DetectAsync(
        string repositoryPath,
        IReadOnlyList<string> scannedFiles,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var solutionFiles = new List<string>();
        var projectFiles = new List<string>();
        var manifests = new List<string>();

        var hasDotNet = false;
        var hasNode = false;
        var hasPython = false;

        foreach (var fullPath in scannedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(repositoryPath, fullPath);
            var fileName = Path.GetFileName(fullPath);
            var ext = Path.GetExtension(fullPath);

            if (fileName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
            {
                hasDotNet = true;
                solutionFiles.Add(relative);
            }

            if (fileName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase))
            {
                hasDotNet = true;
                projectFiles.Add(relative);
            }

            if (fileName.Equals("package.json", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("package-lock.json", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("pnpm-lock.yaml", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("yarn.lock", StringComparison.OrdinalIgnoreCase))
            {
                hasNode = true;
                manifests.Add(relative);
            }

            if (fileName.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("poetry.lock", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".py", StringComparison.OrdinalIgnoreCase))
            {
                hasPython = true;
                if (!manifests.Contains(relative, StringComparer.OrdinalIgnoreCase))
                {
                    manifests.Add(relative);
                }
            }
        }

        var primary = ResolvePrimary(hasDotNet, hasNode, hasPython);
        return Task.FromResult(new ProjectStackDetectionResult(
            primary,
            hasDotNet,
            hasNode,
            hasPython,
            solutionFiles,
            projectFiles,
            manifests));
    }

    private static string ResolvePrimary(bool hasDotNet, bool hasNode, bool hasPython)
    {
        if (hasDotNet)
        {
            return ".NET";
        }

        if (hasNode)
        {
            return "Node.js";
        }

        if (hasPython)
        {
            return "Python";
        }

        return "Generic";
    }
}
