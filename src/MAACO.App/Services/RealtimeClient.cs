using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace MAACO.App.Services;

public sealed class RealtimeClient : IRealtimeClient
{
    private readonly HubConnection connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5168/workflowHub")
        .WithAutomaticReconnect()
        .Build();

    public event EventHandler<string>? EventReceived;
    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler<RealtimeEvent>? WorkflowEventReceived;

    public RealtimeClient()
    {
        connection.On<JsonElement>("LogReceived", payload => EmitStructured("LogReceived", payload));
        connection.On<JsonElement>("ToolExecutionStarted", payload => EmitStructured("ToolExecutionStarted", payload));
        connection.On<JsonElement>("ToolExecutionCompleted", payload => EmitStructured("ToolExecutionCompleted", payload));
        connection.On<JsonElement>("StepStarted", payload => EmitStructured("StepStarted", payload));
        connection.On<JsonElement>("StepCompleted", payload => EmitStructured("StepCompleted", payload));
        connection.On<JsonElement>("StepFailed", payload => EmitStructured("StepFailed", payload));
        connection.On<JsonElement>("WorkflowStarted", payload => EmitStructured("WorkflowStarted", payload));
        connection.On<JsonElement>("WorkflowCompleted", payload => EmitStructured("WorkflowCompleted", payload));
        connection.On<JsonElement>("WorkflowFailed", payload => EmitStructured("WorkflowFailed", payload));
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

    public async Task JoinWorkflowGroupAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        if (connection.State != HubConnectionState.Connected)
        {
            return;
        }

        await connection.InvokeAsync("JoinWorkflowGroup", workflowId, cancellationToken);
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

    private void EmitStructured(string eventType, JsonElement payload)
    {
        var severity = ReadString(payload, "Severity", "Information");
        var message = ReadString(payload, "Message", payload.ToString());
        var tool = ReadString(payload, "ToolName", "-");
        var occurredAt = DateTimeOffset.TryParse(ReadString(payload, "OccurredAt", string.Empty), out var parsed)
            ? parsed
            : DateTimeOffset.UtcNow;

        EventReceived?.Invoke(this, $"{eventType}: {message}");
        WorkflowEventReceived?.Invoke(this, new RealtimeEvent(
            eventType,
            severity,
            message,
            Agent: "-",
            Tool: tool,
            occurredAt));
    }

    private static string ReadString(JsonElement element, string propertyName, string fallback)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? fallback;
        }

        return fallback;
    }
}
