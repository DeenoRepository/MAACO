namespace MAACO.Api.Services;

public sealed class ProjectPathValidator : IProjectPathValidator
{
    private static readonly string[] ForbiddenPrefixes =
    [
        @"C:\Windows",
        @"C:\Program Files",
        @"C:\Program Files (x86)",
        @"C:\Users\Default",
        @"C:\Users\All Users",
        "/bin",
        "/sbin",
        "/etc",
        "/usr",
        "/var",
        "/proc",
        "/sys",
        "/dev"
    ];

    public Task<ProjectPathValidationResult> ValidateAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult(new ProjectPathValidationResult(false, null, "Repository path is empty."));
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path.Trim());
        }
        catch (Exception)
        {
            return Task.FromResult(new ProjectPathValidationResult(false, null, "Repository path is invalid."));
        }

        if (!Directory.Exists(fullPath))
        {
            return Task.FromResult(new ProjectPathValidationResult(false, null, "Repository directory does not exist."));
        }

        if (IsForbiddenPath(fullPath))
        {
            return Task.FromResult(new ProjectPathValidationResult(false, null, "Repository path points to a restricted system directory."));
        }

        try
        {
            _ = Directory.EnumerateFileSystemEntries(fullPath).FirstOrDefault();
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(new ProjectPathValidationResult(false, null, "Repository directory is not readable."));
        }
        catch (IOException)
        {
            return Task.FromResult(new ProjectPathValidationResult(false, null, "Repository directory is not accessible."));
        }

        var gitDirectory = Path.Combine(fullPath, ".git");
        var hasGit = Directory.Exists(gitDirectory) || File.Exists(gitDirectory);
        if (!hasGit)
        {
            return Task.FromResult(new ProjectPathValidationResult(false, null, "Repository must contain a .git directory."));
        }

        return Task.FromResult(new ProjectPathValidationResult(true, fullPath, null));
    }

    private static bool IsForbiddenPath(string fullPath)
    {
        foreach (var prefix in ForbiddenPrefixes)
        {
            if (fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
