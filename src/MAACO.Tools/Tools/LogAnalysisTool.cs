using MAACO.Core.Abstractions.Tools;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MAACO.Tools.Tools;

public sealed class LogAnalysisTool : IAgentTool
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex CompilerErrorRegex = new(
        @"\berror\b\s+([A-Za-z]+\d+)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FailedAssertionRegex = new(
        @"\b(Assert\.\w+|Expected:|Actual:|Assertion)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Name => "LogAnalysisTool";

    public IReadOnlyCollection<ToolPermission> RequiredPermissions =>
    [
        ToolPermission.ReadOnly
    ];

    public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            var input = JsonSerializer.Deserialize<LogAnalysisInput>(request.Input, JsonOptions);
            if (input is null || string.IsNullOrWhiteSpace(input.LogContent))
            {
                return Task.FromResult(Fail("Invalid input for LogAnalysisTool.", request.CorrelationId, startedAt));
            }

            var lines = input.LogContent
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var compilerErrors = ExtractCompilerErrors(lines);
            var stackTraces = ExtractStackTraces(lines);
            var failedAssertions = ExtractFailedAssertions(lines);

            var output = JsonSerializer.Serialize(new
            {
                compilerErrors,
                stackTraces,
                failedAssertions,
                summary = new
                {
                    compilerErrorCount = compilerErrors.Count,
                    stackTraceCount = stackTraces.Count,
                    failedAssertionCount = failedAssertions.Count
                }
            });

            return Task.FromResult(Success(output, request.CorrelationId, startedAt));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Fail($"LogAnalysisTool failed: {ex.Message}", request.CorrelationId, startedAt));
        }
    }

    private static List<string> ExtractCompilerErrors(IEnumerable<string> lines) =>
        lines
            .Where(line => CompilerErrorRegex.IsMatch(line))
            .Distinct(StringComparer.Ordinal)
            .Take(100)
            .ToList();

    private static List<string> ExtractFailedAssertions(IEnumerable<string> lines) =>
        lines
            .Where(line => FailedAssertionRegex.IsMatch(line))
            .Distinct(StringComparer.Ordinal)
            .Take(100)
            .ToList();

    private static List<string> ExtractStackTraces(IReadOnlyList<string> lines)
    {
        var stackTraces = new List<string>();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!line.StartsWith("at ", StringComparison.Ordinal) &&
                !line.Contains("--- End of stack trace", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            stackTraces.Add(line);
            if (stackTraces.Count >= 200)
            {
                break;
            }
        }

        return stackTraces
            .Distinct(StringComparer.Ordinal)
            .ToList();
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

    private sealed record LogAnalysisInput(string LogContent);
}
