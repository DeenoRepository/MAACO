namespace MAACO.Api.Services;

public sealed class ProjectScanner : IProjectScanner
{
    private const int MaxFileCount = 5000;
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        "bin",
        "obj",
        "node_modules",
        ".vs",
        ".idea",
        "dist",
        "build",
        "out",
        "coverage"
    };

    public Task<ProjectScanResult> ScanAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var files = new List<string>(capacity: Math.Min(MaxFileCount, 1024));
        var stack = new Stack<string>();
        stack.Push(repositoryPath);

        var skippedBySize = 0;
        var skippedByLimit = 0;
        var scannedFiles = 0;

        while (stack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentDirectory = stack.Pop();

            foreach (var directory in SafeEnumerateDirectories(currentDirectory))
            {
                var dirName = Path.GetFileName(directory);
                if (IgnoredDirectoryNames.Contains(dirName))
                {
                    continue;
                }

                stack.Push(directory);
            }

            foreach (var file in SafeEnumerateFiles(currentDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                scannedFiles++;

                if (files.Count >= MaxFileCount)
                {
                    skippedByLimit++;
                    continue;
                }

                long fileSize;
                try
                {
                    fileSize = new FileInfo(file).Length;
                }
                catch
                {
                    continue;
                }

                if (fileSize > MaxFileSizeBytes)
                {
                    skippedBySize++;
                    continue;
                }

                files.Add(file);
            }
        }

        return Task.FromResult(new ProjectScanResult(files, scannedFiles, skippedBySize, skippedByLimit));
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string directory)
    {
        try
        {
            return Directory.EnumerateDirectories(directory);
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory);
        }
        catch
        {
            return [];
        }
    }
}
