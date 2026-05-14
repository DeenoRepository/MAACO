using MAACO.Core.Abstractions.Tools;
using MAACO.Tools.Tools;
using System.Text.Json;

namespace MAACO.Tools.Tests;

public sealed class FileSystemToolTests
{
    [Fact]
    public async Task ExecuteAsync_RejectsPathOutsideWorkspace()
    {
        var workspace = CreateWorkspace();
        var outsideFile = Path.Combine(Path.GetTempPath(), "maaco-outside-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(outsideFile, "outside");

        var tool = new FileSystemTool();
        var request = new ToolRequest(
            tool.Name,
            JsonSerializer.Serialize(new
            {
                operation = "read",
                path = outsideFile
            }),
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-fs-boundary");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("outside workspace boundary", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateWorkspace()
    {
        var path = Path.Combine(Path.GetTempPath(), "maaco-tools-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
