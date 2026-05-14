namespace MAACO.App.Services;

public interface IRealtimeClient : IAsyncDisposable
{
    bool IsConnected { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
