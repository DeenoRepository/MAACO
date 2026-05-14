using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;

namespace MAACO.Infrastructure.Workflows.Steps;

public sealed class TestGenerationStepHandler(
    ILlmGateway llmGateway,
    ILogRepository logRepository) : IWorkflowStepHandler
{
    public string Name => "TestGenerationStep";

    public async Task ExecuteAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        var llmResponse = await llmGateway.GenerateAsync(
            new LlmRequest(
                Messages:
                [
                    new LlmMessage(LlmMessageRole.System, "You are MAACO test generation step."),
                    new LlmMessage(LlmMessageRole.User, $"Generate tests for workflow {context.WorkflowId:D}")
                ],
                TaskType: LlmTaskType.Coding,
                WorkflowId: context.WorkflowId,
                CorrelationId: context.CorrelationId),
            cancellationToken);

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name}. Provider={llmResponse.Provider}; Model={llmResponse.Model}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }
}
