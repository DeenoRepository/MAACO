using MAACO.Api.Controllers;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MAACO.Core.Tests;

public sealed class ApprovalsControllerTests
{
    [Fact]
    public async Task Approve_PersistsApprovalAndAuditLog()
    {
        var approval = CreatePendingApproval();
        var approvalRepository = new Mock<IApprovalRepository>();
        var logRepository = new Mock<ILogRepository>();

        approvalRepository
            .Setup(x => x.GetByIdAsync(approval.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approval);

        var controller = CreateController(approvalRepository.Object, logRepository.Object, "trace-approve");

        var response = await controller.Approve(approval.Id, CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(ApprovalStatus.Approved, approval.Status);
        approvalRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        logRepository.Verify(
            x => x.AddAsync(
                It.Is<LogEvent>(log =>
                    log.WorkflowId == approval.WorkflowId &&
                    log.Severity == LogSeverity.Information &&
                    log.Message.Contains("Approval decision: Approved", StringComparison.Ordinal) &&
                    log.CorrelationId == "trace-approve"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        logRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_PersistsApprovalAndAuditLog()
    {
        var approval = CreatePendingApproval();
        var approvalRepository = new Mock<IApprovalRepository>();
        var logRepository = new Mock<ILogRepository>();

        approvalRepository
            .Setup(x => x.GetByIdAsync(approval.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approval);

        var controller = CreateController(approvalRepository.Object, logRepository.Object, "trace-reject");

        var response = await controller.Reject(approval.Id, CancellationToken.None);

        Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(ApprovalStatus.Rejected, approval.Status);
        approvalRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        logRepository.Verify(
            x => x.AddAsync(
                It.Is<LogEvent>(log =>
                    log.WorkflowId == approval.WorkflowId &&
                    log.Severity == LogSeverity.Information &&
                    log.Message.Contains("Approval decision: Rejected", StringComparison.Ordinal) &&
                    log.CorrelationId == "trace-reject"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        logRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_ReturnsBadRequest_WhenApprovalIsNotPending()
    {
        var approval = CreatePendingApproval();
        approval.Status = ApprovalStatus.Rejected;

        var approvalRepository = new Mock<IApprovalRepository>();
        var logRepository = new Mock<ILogRepository>();

        approvalRepository
            .Setup(x => x.GetByIdAsync(approval.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approval);

        var controller = CreateController(approvalRepository.Object, logRepository.Object, "trace-approve-invalid");
        var response = await controller.Approve(approval.Id, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        approvalRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        logRepository.Verify(x => x.AddAsync(It.IsAny<LogEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Reject_ReturnsBadRequest_WhenApprovalIsNotPending()
    {
        var approval = CreatePendingApproval();
        approval.Status = ApprovalStatus.Approved;

        var approvalRepository = new Mock<IApprovalRepository>();
        var logRepository = new Mock<ILogRepository>();

        approvalRepository
            .Setup(x => x.GetByIdAsync(approval.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approval);

        var controller = CreateController(approvalRepository.Object, logRepository.Object, "trace-reject-invalid");
        var response = await controller.Reject(approval.Id, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        approvalRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        logRepository.Verify(x => x.AddAsync(It.IsAny<LogEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ApprovalsController CreateController(
        IApprovalRepository approvalRepository,
        ILogRepository logRepository,
        string traceIdentifier)
    {
        var controller = new ApprovalsController(approvalRepository, logRepository)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    TraceIdentifier = traceIdentifier
                }
            }
        };

        return controller;
    }

    private static ApprovalRequest CreatePendingApproval() =>
        new()
        {
            WorkflowId = Guid.NewGuid(),
            Status = ApprovalStatus.Pending,
            Mode = ApprovalMode.Conditional,
            RequestedBy = "tester",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
