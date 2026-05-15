using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class GitManagerAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog,
    ILlmGateway llmGateway) : LlmAgentBase(tools, promptCatalog, llmGateway)
{
    public override string Name => "GitManagerAgent";
    public override string Role => "GitManager";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Git];
    protected override LlmTaskType TaskType => LlmTaskType.Coding;
}
