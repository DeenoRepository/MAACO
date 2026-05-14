using MAACO.Core.Abstractions.Tools;
using System.Text.Json;

namespace MAACO.Tools.Tools;

public sealed class ProjectScannerTool : IAgentTool
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "bin", "obj", "node_modules", ".vs", ".idea", "dist", "build", "out", "coverage"
    };

    public string Name => "ProjectScannerTool";

    public IReadOnlyCollection<ToolPermission> RequiredPermissions =>
    [
        ToolPermission.ReadOnly
    ];

    public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            var root = Path.GetFullPath(request.WorkspacePath);
            if (!Directory.Exists(root))
            {
                return Task.FromResult(Fail("Workspace path does not exist.", request.CorrelationId, startedAt));
            }

            var stack = new Stack<string>();
            stack.Push(root);

            var files = new List<string>();
            while (stack.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dir = stack.Pop();

                foreach (var childDir in SafeDirs(dir))
                {
                    var name = Path.GetFileName(childDir);
                    if (!IgnoredDirectories.Contains(name))
                    {
                        stack.Push(childDir);
                    }
                }

                foreach (var file in SafeFiles(dir))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    files.Add(Path.GetRelativePath(root, file));
                    if (files.Count >= 5000)
                    {
                        break;
                    }
                }

                if (files.Count >= 5000)
                {
                    break;
                }
            }

            var output = JsonSerializer.Serialize(new
            {
                scanned = files.Count,
                files = files.Take(200).ToArray(),
                truncated = files.Count > 200
            });
            return Task.FromResult(Success(output, request.CorrelationId, startedAt));
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(new ToolResult(
                Succeeded: false,
                Output: string.Empty,
                Error: "Project scan cancelled.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Fail($"ProjectScannerTool failed: {ex.Message}", request.CorrelationId, startedAt));
        }
    }

    private static IEnumerable<string> SafeDirs(string dir)
    {
        try { return Directory.EnumerateDirectories(dir); }
        catch { return []; }
    }

    private static IEnumerable<string> SafeFiles(string dir)
    {
        try { return Directory.EnumerateFiles(dir); }
        catch { return []; }
    }

    private static ToolResult Success(string output, string? correlationId, DateTimeOffset startedAt) =>
        new(
            Succeeded: true,
            Output: output,
            Error: null,
            Duration: DateTimeOffset.UtcNow - startedAt,
            CorrelationId: correlationId);

    private static ToolResult Fail(string error, string? correlationId, DateTimeOffset startedAt) =>
        new(
            Succeeded: false,
            Output: string.Empty,
            Error: error,
            Duration: DateTimeOffset.UtcNow - startedAt,
            CorrelationId: correlationId);
}
