using MAACO.Api.Realtime;
using MAACO.Core.Domain.Events;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace MAACO.Core.Tests;

public sealed class SignalrWorkflowHubEventHandlersTests
{
    [Fact]
    public async Task WorkflowStartedHandler_PublishesToWorkflowGroup()
    {
        var workflowId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var clientProxy = new Mock<IClientProxy>();
        clientProxy
            .Setup(x => x.SendCoreAsync(
                "WorkflowStarted",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clients = new Mock<IHubClients>();
        clients
            .Setup(x => x.Group(WorkflowHub.WorkflowGroup(workflowId)))
            .Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<WorkflowHub>>();
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var handler = new WorkflowStartedSignalrHandler(hubContext.Object);

        await handler.HandleAsync(
            new WorkflowStartedEvent(workflowId, taskId, DateTimeOffset.UtcNow, "corr-1"),
            CancellationToken.None);

        clientProxy.Verify(
            x => x.SendCoreAsync(
                "WorkflowStarted",
                It.Is<object?[]>(args => args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
