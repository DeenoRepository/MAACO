using Microsoft.AspNetCore.SignalR.Client;

namespace MAACO.App.Services;

public sealed class RealtimeClient : IRealtimeClient
{
    private readonly HubConnection connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5168/workflowHub")
        .WithAutomaticReconnect()
        .Build();

    public event EventHandler<string>? EventReceived;
    public event EventHandler<string>? ErrorOccurred;

    public RealtimeClient()
    {
        connection.On<string>("WorkflowEvent", payload => EventReceived?.Invoke(this, payload));
        connection.On<string>("WorkflowLog", payload => EventReceived?.Invoke(this, payload));
        connection.Reconnecting += error =>
        {
            ErrorOccurred?.Invoke(this, $"SignalR reconnecting: {error?.Message ?? "transient"}");
            return Task.CompletedTask;
        };
        connection.Closed += error =>
        {
            if (error is not null)
            {
                ErrorOccurred?.Invoke(this, $"SignalR closed: {error.Message}");
            }

            return Task.CompletedTask;
        };
    }

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
            EventReceived?.Invoke(this, "SignalR connection established.");
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"SignalR unavailable: {ex.Message}");
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
