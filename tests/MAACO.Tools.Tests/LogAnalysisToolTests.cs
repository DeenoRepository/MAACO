using MAACO.Core.Abstractions.Tools;
using MAACO.Tools.Tools;
using System.Text.Json;

namespace MAACO.Tools.Tests;

public sealed class LogAnalysisToolTests
{
    [Fact]
    public async Task ExecuteAsync_ExtractsCompilerErrorsStackTracesAndAssertions()
    {
        var tool = new LogAnalysisTool();
        var log = """
                  Program.cs(10,5): error CS1002: ; expected
                  Some other line
                  Assert.Equal() Failure
                  Expected: 1
                  Actual:   2
                     at MAACO.Tests.SampleTests.Failing() in C:\repo\SampleTests.cs:line 42
                  --- End of stack trace from previous location ---
                  """;

        var request = new ToolRequest(
            tool.Name,
            JsonSerializer.Serialize(new { logContent = log }),
            WorkspacePath: "D:\\Projects\\MAACO",
            Permissions: [ToolPermission.ReadOnly],
            CorrelationId: "corr-log");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.True(result.Succeeded);
        using var doc = JsonDocument.Parse(result.Output);
        var root = doc.RootElement;

        var compilerErrors = root.GetProperty("compilerErrors");
        var stackTraces = root.GetProperty("stackTraces");
        var failedAssertions = root.GetProperty("failedAssertions");

        Assert.True(compilerErrors.GetArrayLength() > 0);
        Assert.True(stackTraces.GetArrayLength() > 0);
        Assert.True(failedAssertions.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailure_ForInvalidInput()
    {
        var tool = new LogAnalysisTool();
        var request = new ToolRequest(
            tool.Name,
            JsonSerializer.Serialize(new { notLogContent = "oops" }),
            WorkspacePath: "D:\\Projects\\MAACO",
            Permissions: [ToolPermission.ReadOnly],
            CorrelationId: "corr-log-invalid");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }
}
