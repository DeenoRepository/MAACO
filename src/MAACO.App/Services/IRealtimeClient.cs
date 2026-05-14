namespace MAACO.App.Services;

public interface IRealtimeClient : IAsyncDisposable
{
    bool IsConnected { get; }
    event EventHandler<string>? EventReceived;
    event EventHandler<string>? ErrorOccurred;
    event EventHandler<RealtimeEvent>? WorkflowEventReceived;
    Task StartAsync(CancellationToken cancellationToken);
    Task JoinWorkflowGroupAsync(Guid workflowId, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public sealed record RealtimeEvent(
    string EventType,
    string Severity,
    string Message,
    string Agent,
    string Tool,
    DateTimeOffset OccurredAt);
