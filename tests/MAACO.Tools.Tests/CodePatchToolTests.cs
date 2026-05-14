using MAACO.Core.Abstractions.Tools;
using MAACO.Tools.Tools;
using System.Text.Json;

namespace MAACO.Tools.Tests;

public sealed class CodePatchToolTests
{
    [Fact]
    public async Task ExecuteAsync_AppliesValidPatch()
    {
        var workspace = CreateWorkspace();
        var filePath = Path.Combine(workspace, "sample.txt");
        await File.WriteAllTextAsync(filePath, "hello old world");

        var tool = new CodePatchTool();
        var request = new ToolRequest(
            tool.Name,
            JsonSerializer.Serialize(new
            {
                targetPath = "sample.txt",
                oldText = "old",
                newText = "new",
                requireSingleMatch = true
            }),
            workspace,
            [ToolPermission.WorkspaceWrite],
            CorrelationId: "corr-test");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Null(result.Error);
        var content = await File.ReadAllTextAsync(filePath);
        Assert.Equal("hello new world", content);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidPatch_WhenOldTextMissing()
    {
        var workspace = CreateWorkspace();
        var filePath = Path.Combine(workspace, "sample.txt");
        await File.WriteAllTextAsync(filePath, "hello world");

        var tool = new CodePatchTool();
        var request = new ToolRequest(
            tool.Name,
            JsonSerializer.Serialize(new
            {
                targetPath = "sample.txt",
                oldText = "missing",
                newText = "new",
                requireSingleMatch = true
            }),
            workspace,
            [ToolPermission.WorkspaceWrite],
            CorrelationId: "corr-test");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        var content = await File.ReadAllTextAsync(filePath);
        Assert.Equal("hello world", content);
    }

    private static string CreateWorkspace()
    {
        var path = Path.Combine(Path.GetTempPath(), "maaco-tools-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
