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

    public async Task<ProjectPathValidationResult> ValidateAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(path))
        {
            return new ProjectPathValidationResult(false, null, "Repository path is empty.");
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path.Trim());
        }
        catch (Exception)
        {
            return new ProjectPathValidationResult(false, null, "Repository path is invalid.");
        }

        if (!Directory.Exists(fullPath))
        {
            try
            {
                Directory.CreateDirectory(fullPath);
            }
            catch (Exception ex)
            {
                return new ProjectPathValidationResult(false, null, $"Repository directory does not exist and cannot be created: {ex.Message}");
            }
        }

        if (IsForbiddenPath(fullPath))
        {
            return new ProjectPathValidationResult(false, null, "Repository path points to a restricted system directory.");
        }

        try
        {
            _ = Directory.EnumerateFileSystemEntries(fullPath).FirstOrDefault();
        }
        catch (UnauthorizedAccessException)
        {
            return new ProjectPathValidationResult(false, null, "Repository directory is not readable.");
        }
        catch (IOException)
        {
            return new ProjectPathValidationResult(false, null, "Repository directory is not accessible.");
        }

        var gitRoot = FindGitRoot(fullPath);
        if (gitRoot is null)
        {
            var initResult = await TryInitializeGitRepositoryAsync(fullPath, cancellationToken);
            if (!initResult.Succeeded)
            {
                return new ProjectPathValidationResult(false, null, initResult.ErrorMessage);
            }

            gitRoot = FindGitRoot(fullPath);
        }

        if (gitRoot is null)
        {
            return new ProjectPathValidationResult(false, null, "Repository must contain a .git directory (current or parent folder).");
        }

        return new ProjectPathValidationResult(true, gitRoot, null);
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

    private static string? FindGitRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        while (current is not null)
        {
            var gitPath = Path.Combine(current.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    private static async Task<(bool Succeeded, string ErrorMessage)> TryInitializeGitRepositoryAsync(
        string fullPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = fullPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process is null)
            {
                return (false, "Repository is missing .git and git init could not be started.");
            }

            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode == 0)
            {
                return (true, string.Empty);
            }

            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            return (false, $"Repository is missing .git and auto-init failed: {Trim(stderr)}");
        }
        catch (Exception ex)
        {
            return (false, $"Repository is missing .git and auto-init failed: {ex.Message}");
        }
    }

    private static string Trim(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown error";
        }

        var normalized = value.Trim();
        return normalized.Length <= 240 ? normalized : normalized[..240];
    }
}
