using MAACO.Core.Abstractions.Tools;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MAACO.Tools;

public sealed class ToolRegistry(
    IEnumerable<IAgentTool> tools,
    ILogger<ToolRegistry> logger,
    IToolExecutionRepository? toolExecutionRepository = null) : IToolRegistry
{
    private const int MaxLoggedTextLength = 4000;
    private const int ToolTransientRetryCount = 2;

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
        var workflowId = TryExtractWorkflowId(request.Input);
        ToolExecution? execution = null;
        if (workflowId is Guid persistedWorkflowId && toolExecutionRepository is not null)
        {
            execution = await CreateExecutionRecordAsync(
                persistedWorkflowId,
                request.ToolName,
                startedAt,
                linkedCts.Token);
        }

        logger.LogInformation(
            "Tool execution started: {ToolName}, CorrelationId={CorrelationId}, Input={Input}",
            request.ToolName,
            request.CorrelationId,
            safeInput);

        try
        {
            var result = await ExecuteWithTransientRetryAsync(
                tool,
                request,
                linkedCts.Token);
            var safeOutput = PrepareForLog(result.Output);
            var safeError = PrepareForLog(result.Error);
            await UpdateExecutionRecordAsync(
                execution,
                MapStatus(result),
                result,
                linkedCts.Token);
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
            await UpdateExecutionRecordAsync(
                execution,
                ToolExecutionStatus.TimedOut,
                new ToolResult(false, string.Empty, "Tool execution timed out.", DateTimeOffset.UtcNow - startedAt, TimedOut: true, CorrelationId: request.CorrelationId),
                CancellationToken.None);
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
            await UpdateExecutionRecordAsync(
                execution,
                ToolExecutionStatus.Cancelled,
                new ToolResult(false, string.Empty, "Tool execution cancelled.", DateTimeOffset.UtcNow - startedAt, CorrelationId: request.CorrelationId),
                CancellationToken.None);
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
            await UpdateExecutionRecordAsync(
                execution,
                ToolExecutionStatus.Failed,
                new ToolResult(false, string.Empty, $"Tool execution failed: {ex.Message}", DateTimeOffset.UtcNow - startedAt, CorrelationId: request.CorrelationId),
                CancellationToken.None);
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

    private async Task<ToolResult> ExecuteWithTransientRetryAsync(
        IAgentTool tool,
        ToolRequest request,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        var attempts = ToolTransientRetryCount + 1;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                var result = await tool.ExecuteAsync(request, cancellationToken);
                if (result.Succeeded || !IsTransientToolFailure(result))
                {
                    return result;
                }

                if (attempt < attempts)
                {
                    await DelayWithBackoffAsync(attempt, cancellationToken);
                    continue;
                }

                return result;
            }
            catch (Exception ex) when (attempt < attempts && IsTransientException(ex))
            {
                lastException = ex;
                await DelayWithBackoffAsync(attempt, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Tool execution failed after retries.");
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

    private async Task<ToolExecution?> CreateExecutionRecordAsync(
        Guid workflowId,
        string toolName,
        DateTimeOffset startedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            var execution = new ToolExecution
            {
                WorkflowId = workflowId,
                ToolName = toolName,
                Status = ToolExecutionStatus.Running,
                CreatedAt = startedAt,
                UpdatedAt = startedAt
            };

            await toolExecutionRepository!.AddAsync(execution, cancellationToken);
            await toolExecutionRepository.SaveChangesAsync(cancellationToken);
            return execution;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist tool execution start for {ToolName}", toolName);
            return null;
        }
    }

    private async Task UpdateExecutionRecordAsync(
        ToolExecution? execution,
        ToolExecutionStatus status,
        ToolResult result,
        CancellationToken cancellationToken)
    {
        if (execution is null || toolExecutionRepository is null)
        {
            return;
        }

        try
        {
            execution.Status = status;
            execution.Result = new ExecutionResult(
                ExitCode: result.Succeeded ? 0 : 1,
                StdOut: result.Output ?? string.Empty,
                StdErr: result.Error ?? string.Empty,
                Duration: result.Duration,
                TimedOut: result.TimedOut);
            execution.UpdatedAt = DateTimeOffset.UtcNow;

            await toolExecutionRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist tool execution completion for {ToolName}", execution.ToolName);
        }
    }

    private static ToolExecutionStatus MapStatus(ToolResult result)
    {
        if (result.TimedOut)
        {
            return ToolExecutionStatus.TimedOut;
        }

        return result.Succeeded
            ? ToolExecutionStatus.Completed
            : ToolExecutionStatus.Failed;
    }

    private static Guid? TryExtractWorkflowId(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(input);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("workflowId", out var workflowIdProperty))
            {
                return null;
            }

            if (workflowIdProperty.ValueKind == JsonValueKind.String &&
                Guid.TryParse(workflowIdProperty.GetString(), out var workflowId))
            {
                return workflowId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsTransientToolFailure(ToolResult result)
    {
        if (result.TimedOut)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(result.Error))
        {
            return false;
        }

        if (IsValidationError(result.Error))
        {
            return false;
        }

        return ContainsAny(
            result.Error,
            "transient",
            "temporar",
            "timeout",
            "timed out",
            "throttle",
            "rate limit",
            "too many requests",
            "unavailable",
            "connection reset",
            "network");
    }

    private static bool IsTransientException(Exception ex)
    {
        if (ex is TimeoutException or HttpRequestException or IOException)
        {
            return true;
        }

        if (ex is OperationCanceledException)
        {
            return true;
        }

        return ContainsAny(
            ex.Message,
            "transient",
            "temporar",
            "timeout",
            "throttle",
            "rate limit",
            "too many requests",
            "unavailable",
            "connection reset",
            "network");
    }

    private static bool IsValidationError(string error) =>
        ContainsAny(
            error,
            "validation",
            "invalid",
            "unprocessable",
            "bad request");

    private static bool ContainsAny(string value, params string[] markers)
    {
        foreach (var marker in markers)
        {
            if (value.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static Task DelayWithBackoffAsync(int attempt, CancellationToken cancellationToken)
    {
        var exponent = Math.Max(0, attempt - 1);
        var delayMs = Math.Min(100 * Math.Pow(2, exponent), 1000);
        return Task.Delay(TimeSpan.FromMilliseconds(delayMs), cancellationToken);
    }
}
