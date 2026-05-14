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
            var result = await tool.ExecuteAsync(request, linkedCts.Token);
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
}
