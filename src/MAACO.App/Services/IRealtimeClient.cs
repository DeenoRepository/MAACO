namespace MAACO.App.Services;

public interface IRealtimeClient : IAsyncDisposable
{
    bool IsConnected { get; }
    event EventHandler<string>? EventReceived;
    event EventHandler<string>? ErrorOccurred;
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
