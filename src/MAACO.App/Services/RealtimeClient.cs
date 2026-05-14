using Microsoft.AspNetCore.SignalR.Client;

namespace MAACO.App.Services;

public sealed class RealtimeClient : IRealtimeClient
{
    private readonly HubConnection connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5168/workflowHub")
        .WithAutomaticReconnect()
        .Build();

    public bool IsConnected => connection.State == HubConnectionState.Connected;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (connection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
        {
            return;
        }

        try
        {
            await connection.StartAsync(cancellationToken);
        }
        catch
        {
            // UI shell should remain functional even if backend/realtime is unavailable.
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (connection.State == HubConnectionState.Disconnected)
        {
            return;
        }

        await connection.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
    }
}
