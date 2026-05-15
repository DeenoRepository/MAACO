using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public abstract class LlmAgentBase : IAgent
{
    private readonly IReadOnlyDictionary<string, IAgentTool> _tools;
    private readonly IAgentPromptCatalog _promptCatalog;
    private readonly ILlmGateway _llmGateway;

    protected LlmAgentBase(
        IEnumerable<IAgentTool> tools,
        IAgentPromptCatalog promptCatalog,
        ILlmGateway llmGateway)
    {
        _tools = tools.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _promptCatalog = promptCatalog;
        _llmGateway = llmGateway;
    }

    public abstract string Name { get; }
    public abstract string Role { get; }
    public abstract IReadOnlyCollection<AgentCapability> Capabilities { get; }
    protected abstract LlmTaskType TaskType { get; }

    public virtual async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ToolResult? delegatedToolResult = null;
        if (context.Inputs is not null
            && context.Inputs.TryGetValue("toolName", out var toolName)
            && context.Inputs.TryGetValue("toolInput", out var toolInput)
            && _tools.TryGetValue(toolName, out var tool))
        {
            delegatedToolResult = await tool.ExecuteAsync(
                new ToolRequest(
                    ToolName: toolName,
                    Input: toolInput,
                    WorkspacePath: context.Inputs.TryGetValue("workspacePath", out var workspacePath)
                        ? workspacePath
                        : ".",
                    Permissions: tool.RequiredPermissions,
                    CorrelationId: context.CorrelationId),
                cancellationToken);
        }

        var userInstruction = BuildUserInstruction(context, delegatedToolResult);
        var llmResponse = await _llmGateway.GenerateAsync(
            new LlmRequest(
                Messages:
                [
                    new LlmMessage(LlmMessageRole.System, _promptCatalog.GetSystemPrompt(Name)),
                    new LlmMessage(LlmMessageRole.User, userInstruction)
                ],
                TaskType: TaskType,
                CorrelationId: context.CorrelationId,
                WorkflowId: context.WorkflowId),
            cancellationToken);

        var succeeded = llmResponse.Succeeded;
        var output = succeeded ? llmResponse.Content : string.Empty;
        var error = succeeded ? null : (llmResponse.Error ?? "LLM request failed.");

        return new AgentResult(
            Succeeded: succeeded,
            Output: output,
            Error: error,
            Metadata: new Dictionary<string, string>
            {
                ["agent"] = Name,
                ["role"] = Role,
                ["capabilities"] = string.Join(",", Capabilities),
                ["workflowId"] = context.WorkflowId.ToString("D"),
                ["decision"] = "Used prompt catalog + llm gateway and optional delegated tool execution.",
                ["systemPrompt"] = _promptCatalog.GetSystemPrompt(Name),
                ["responseSchema"] = _promptCatalog.GetResponseSchema(Name),
                ["delegatedTool"] = delegatedToolResult is null ? "none" : delegatedToolResult.CorrelationId ?? "executed",
                ["llmProvider"] = llmResponse.Provider,
                ["llmModel"] = llmResponse.Model,
                ["llmSucceeded"] = llmResponse.Succeeded.ToString(),
                ["llmError"] = llmResponse.Error ?? string.Empty
            });
    }

    private string BuildUserInstruction(AgentContext context, ToolResult? delegatedToolResult)
    {
        var delegatedSummary = delegatedToolResult is null
            ? "No delegated tool result."
            : $"Delegated tool result: Succeeded={delegatedToolResult.Succeeded}; Output={delegatedToolResult.Output}; Error={delegatedToolResult.Error ?? "none"}";

        return
            $"WorkflowId: {context.WorkflowId:D}\n" +
            $"TaskId: {context.TaskId:D}\n" +
            $"ProjectId: {context.ProjectId:D}\n" +
            $"Instruction: {context.Instruction}\n" +
            $"{delegatedSummary}\n" +
            $"Respond according to schema: {_promptCatalog.GetResponseSchema(Name)}";
    }
}
