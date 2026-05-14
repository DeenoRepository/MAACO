using MAACO.Core.Abstractions.Tools;
using Microsoft.Extensions.Logging;

namespace MAACO.Tools;

public sealed class ToolRegistry(
    IEnumerable<IAgentTool> tools,
    ILogger<ToolRegistry> logger) : IToolRegistry
{
    private const int MaxLoggedTextLength = 4000;

    private readonly Dictionary<string, IAgentTool> toolMap = tools.ToDictionary(
        x => x.Name,
        x => x,
        StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> ListToolNames() => toolMap.Keys.ToArray();

    public async Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        if (!toolMap.TryGetValue(request.ToolName, out var tool))
        {
            return new ToolResult(
                Succeeded: false,
                Output: string.Empty,
                Error: $"Tool '{request.ToolName}' is not registered.",
                Duration: TimeSpan.Zero,
                CorrelationId: request.CorrelationId);
        }

        if (!HasPermissions(request.Permissions, tool.RequiredPermissions))
        {
            return new ToolResult(
                Succeeded: false,
                Output: string.Empty,
                Error: $"Insufficient permissions for tool '{request.ToolName}'.",
                Duration: TimeSpan.Zero,
                CorrelationId: request.CorrelationId);
        }

        using var timeoutCts = request.Timeout is { } timeout
            ? new CancellationTokenSource(timeout)
            : null;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts?.Token ?? CancellationToken.None);

        var startedAt = DateTimeOffset.UtcNow;
        var safeInput = PrepareForLog(request.Input);
        logger.LogInformation(
            "Tool execution started: {ToolName}, CorrelationId={CorrelationId}, Input={Input}",
            request.ToolName,
            request.CorrelationId,
            safeInput);

        try
        {
            var result = await tool.ExecuteAsync(request, linkedCts.Token);
            var safeOutput = PrepareForLog(result.Output);
            var safeError = PrepareForLog(result.Error);
            logger.LogInformation(
                "Tool execution completed: {ToolName}, Success={Succeeded}, CorrelationId={CorrelationId}, Output={Output}, Error={Error}",
                request.ToolName,
                result.Succeeded,
                request.CorrelationId,
                safeOutput,
                safeError);
            return result with
            {
                Duration = result.Duration == TimeSpan.Zero ? DateTimeOffset.UtcNow - startedAt : result.Duration
            };
        }
        catch (OperationCanceledException) when (timeoutCts is not null && timeoutCts.IsCancellationRequested)
        {
            logger.LogWarning(
                "Tool execution timed out: {ToolName}, CorrelationId={CorrelationId}",
                request.ToolName,
                request.CorrelationId);
            return new ToolResult(
                Succeeded: false,
                Output: string.Empty,
                Error: "Tool execution timed out.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                TimedOut: true,
                CorrelationId: request.CorrelationId);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "Tool execution cancelled: {ToolName}, CorrelationId={CorrelationId}",
                request.ToolName,
                request.CorrelationId);
            return new ToolResult(
                Succeeded: false,
                Output: string.Empty,
                Error: "Tool execution cancelled.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Tool execution failed: {ToolName}, CorrelationId={CorrelationId}",
                request.ToolName,
                request.CorrelationId);
            return new ToolResult(
                Succeeded: false,
                Output: string.Empty,
                Error: $"Tool execution failed: {ex.Message}",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
        }
    }

    private static bool HasPermissions(
        IReadOnlyCollection<ToolPermission> granted,
        IReadOnlyCollection<ToolPermission> required)
    {
        if (required.Count == 0)
        {
            return true;
        }

        var grantedSet = granted.ToHashSet();
        return required.All(grantedSet.Contains);
    }

    private static string PrepareForLog(string? value)
    {
        var redacted = ToolLogRedactor.Redact(value);
        if (redacted.Length <= MaxLoggedTextLength)
        {
            return redacted;
        }

        return redacted[..MaxLoggedTextLength];
    }
}
