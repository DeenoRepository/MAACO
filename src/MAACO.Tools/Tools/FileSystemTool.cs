using MAACO.Core.Abstractions.Tools;
using System.Text.Json;

namespace MAACO.Tools.Tools;

public sealed class FileSystemTool : IAgentTool
{
    private const int MaxEntries = 500;

    public string Name => "FileSystemTool";

    public IReadOnlyCollection<ToolPermission> RequiredPermissions =>
    [
        ToolPermission.ReadOnly
    ];

    public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            var input = JsonSerializer.Deserialize<FileSystemToolInput>(request.Input);
            if (input is null || string.IsNullOrWhiteSpace(input.Path))
            {
                return Task.FromResult(Fail("Invalid input for FileSystemTool.", request.CorrelationId, startedAt));
            }

            var target = Path.GetFullPath(Path.IsPathRooted(input.Path)
                ? input.Path
                : Path.Combine(request.WorkspacePath, input.Path));

            if (!ToolPathSafety.IsWithinWorkspace(request.WorkspacePath, target))
            {
                return Task.FromResult(Fail("Path is outside workspace boundary.", request.CorrelationId, startedAt));
            }

            if (input.Operation.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(target))
                {
                    return Task.FromResult(Fail("Directory does not exist.", request.CorrelationId, startedAt));
                }

                var entries = Directory.EnumerateFileSystemEntries(target)
                    .Take(MaxEntries)
                    .Select(Path.GetFileName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                var output = JsonSerializer.Serialize(new { entries });
                return Task.FromResult(Success(output, request.CorrelationId, startedAt));
            }

            if (input.Operation.Equals("read", StringComparison.OrdinalIgnoreCase))
            {
                if (!File.Exists(target))
                {
                    return Task.FromResult(Fail("File does not exist.", request.CorrelationId, startedAt));
                }

                var text = File.ReadAllText(target);
                var originalLength = text.Length;
                if (text.Length > 10000)
                {
                    text = text[..10000];
                }

                var output = JsonSerializer.Serialize(new { content = text, truncated = originalLength > text.Length });
                return Task.FromResult(Success(output, request.CorrelationId, startedAt));
            }

            return Task.FromResult(Fail("Unsupported operation.", request.CorrelationId, startedAt));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Fail($"FileSystemTool failed: {ex.Message}", request.CorrelationId, startedAt));
        }
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

    private sealed record FileSystemToolInput(string Operation, string Path);
}
