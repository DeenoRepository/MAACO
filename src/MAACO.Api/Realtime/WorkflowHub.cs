using Microsoft.AspNetCore.SignalR;

namespace MAACO.Api.Realtime;

public sealed class WorkflowHub : Hub
{
    public Task JoinWorkflowGroup(Guid workflowId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, WorkflowGroup(workflowId));

    public Task LeaveWorkflowGroup(Guid workflowId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, WorkflowGroup(workflowId));

    public Task JoinProjectGroup(Guid projectId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public Task LeaveProjectGroup(Guid projectId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public static string WorkflowGroup(Guid workflowId) => $"workflow:{workflowId:D}";
    public static string ProjectGroup(Guid projectId) => $"project:{projectId:D}";
}
