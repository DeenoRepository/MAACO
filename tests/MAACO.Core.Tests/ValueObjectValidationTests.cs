using MAACO.Core.Domain.ValueObjects;

namespace MAACO.Core.Tests;

public sealed class ValueObjectValidationTests
{
    [Fact]
    public void RepositoryPath_Throws_WhenEmpty()
    {
        Assert.Throws<ArgumentException>(() => new RepositoryPath(string.Empty));
        Assert.Throws<ArgumentException>(() => new RepositoryPath("   "));
    }

    [Fact]
    public void RepositoryPath_KeepsValue_WhenValid()
    {
        var path = new RepositoryPath("C:\\repo\\maaco");
        Assert.Equal("C:\\repo\\maaco", path.Value);
    }

    [Fact]
    public void ValueObjects_RoundtripProperties()
    {
        var command = new CommandSpec("dotnet", "test", "C:\\repo", TimeSpan.FromMinutes(5));
        var usage = new LlmUsage(12, 8, 20, "gpt-5");
        var patch = new PatchSummary(3, 25, 5, "fix");
        var result = new ExecutionResult(0, "ok", string.Empty, TimeSpan.FromSeconds(2));
        var stack = new DetectedProjectStack("C#", ".NET", "SQLite", "net10.0");
        var change = new FileChangeSummary("src/a.cs", 10, 2, false);

        Assert.Equal("dotnet", command.FileName);
        Assert.Equal(20, usage.TotalTokens);
        Assert.Equal(3, patch.FilesChanged);
        Assert.False(result.TimedOut);
        Assert.Equal(".NET", stack.Framework);
        Assert.Equal("src/a.cs", change.Path);
    }
}
