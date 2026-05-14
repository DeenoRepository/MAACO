using MAACO.Core.Abstractions.Tools;
using System.Text.Json;

namespace MAACO.Tools.Tools;

public sealed class CodePatchTool : IAgentTool
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string Name => "CodePatchTool";

    public IReadOnlyCollection<ToolPermission> RequiredPermissions =>
    [
        ToolPermission.WorkspaceWrite
    ];

    public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            var input = JsonSerializer.Deserialize<CodePatchToolInput>(request.Input, JsonOptions);
            if (input is null ||
                string.IsNullOrWhiteSpace(input.TargetPath) ||
                string.IsNullOrEmpty(input.OldText))
            {
                return Task.FromResult(Fail("Invalid input for CodePatchTool.", request.CorrelationId, startedAt));
            }

            var targetPath = Path.GetFullPath(Path.IsPathRooted(input.TargetPath)
                ? input.TargetPath
                : Path.Combine(request.WorkspacePath, input.TargetPath));

            if (!ToolPathSafety.IsWithinWorkspace(request.WorkspacePath, targetPath))
            {
                return Task.FromResult(Fail("Path is outside workspace boundary.", request.CorrelationId, startedAt));
            }

            if (!File.Exists(targetPath))
            {
                return Task.FromResult(Fail("Target file does not exist.", request.CorrelationId, startedAt));
            }

            cancellationToken.ThrowIfCancellationRequested();
            var currentContent = File.ReadAllText(targetPath);
            var matchCount = CountMatches(currentContent, input.OldText);

            if (matchCount == 0)
            {
                return Task.FromResult(Fail("Patch cannot be applied: old text not found.", request.CorrelationId, startedAt));
            }

            if (input.RequireSingleMatch && matchCount != 1)
            {
                return Task.FromResult(Fail("Patch cannot be applied: expected a single match.", request.CorrelationId, startedAt));
            }

            var updatedContent = currentContent.Replace(input.OldText, input.NewText ?? string.Empty, StringComparison.Ordinal);
            File.WriteAllText(targetPath, updatedContent);

            var output = JsonSerializer.Serialize(new
            {
                applied = true,
                targetPath,
                replacements = matchCount
            });

            return Task.FromResult(Success(output, request.CorrelationId, startedAt));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Fail($"CodePatchTool failed: {ex.Message}", request.CorrelationId, startedAt));
        }
    }

    private static int CountMatches(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while (true)
        {
            index = text.IndexOf(pattern, index, StringComparison.Ordinal);
            if (index < 0)
            {
                return count;
            }

            count++;
            index += pattern.Length;
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

    private sealed record CodePatchToolInput(
        string TargetPath,
        string OldText,
        string? NewText,
        bool RequireSingleMatch = true);
}
