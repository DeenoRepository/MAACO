using MAACO.Agents.Abstractions;
using MAACO.Core.Abstractions.Llm;

namespace MAACO.Agents.Services;

public sealed class AgentDemoWorkflowService(
    ILlmGateway llmGateway,
    IAgentExecutionService agentExecutionService) : IAgentDemoWorkflowService
{
    public async Task<AgentResult> RunAsync(AgentContext context, CancellationToken cancellationToken)
    {
        var llmResponse = await llmGateway.GenerateAsync(
            new LlmRequest(
                Messages:
                [
                    new LlmMessage(LlmMessageRole.System, "You are MAACO demo planner."),
                    new LlmMessage(LlmMessageRole.User, context.Instruction)
                ],
                TaskType: LlmTaskType.Planning,
                WorkflowId: context.WorkflowId,
                CorrelationId: context.CorrelationId),
            cancellationToken);

        var plannerContext = context with
        {
            Inputs = new Dictionary<string, string>
            {
                ["demoPlan"] = llmResponse.Content
            }
        };

        var plannerResult = await agentExecutionService.ExecuteAsync(
            "TaskPlannerAgent",
            plannerContext,
            cancellationToken);

        var metadata = new Dictionary<string, string>(plannerResult.Metadata ?? new Dictionary<string, string>())
        {
            ["demoLlmProvider"] = llmResponse.Provider,
            ["demoLlmModel"] = llmResponse.Model
        };

        return plannerResult with { Metadata = metadata };
    }
}
