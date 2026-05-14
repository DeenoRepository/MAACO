using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public abstract class StubAgentBase : IAgent
{
    private readonly IReadOnlyDictionary<string, IAgentTool> _tools;
    private readonly IAgentPromptCatalog _promptCatalog;

    protected StubAgentBase(
        IEnumerable<IAgentTool> tools,
        IAgentPromptCatalog promptCatalog)
    {
        _tools = tools.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _promptCatalog = promptCatalog;
    }

    public abstract string Name { get; }
    public abstract string Role { get; }
    public abstract IReadOnlyCollection<AgentCapability> Capabilities { get; }

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

        return new AgentResult(
            Succeeded: true,
            Output: $"{Name} stub completed.",
            Error: null,
            Metadata: new Dictionary<string, string>
            {
                ["agent"] = Name,
                ["role"] = Role,
                ["capabilities"] = string.Join(",", Capabilities),
                ["workflowId"] = context.WorkflowId.ToString("D"),
                ["decision"] = "Used deterministic prompt + schema and optional delegated tool execution.",
                ["systemPrompt"] = _promptCatalog.GetSystemPrompt(Name),
                ["responseSchema"] = _promptCatalog.GetResponseSchema(Name),
                ["delegatedTool"] = delegatedToolResult is null ? "none" : delegatedToolResult.CorrelationId ?? "executed"
            });
    }
}
