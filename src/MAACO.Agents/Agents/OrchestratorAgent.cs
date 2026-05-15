using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class OrchestratorAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog,
    ILlmGateway llmGateway) : LlmAgentBase(tools, promptCatalog, llmGateway)
{
    public override string Name => "OrchestratorAgent";
    public override string Role => "Orchestrator";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Orchestration, AgentCapability.Planning];
    protected override LlmTaskType TaskType => LlmTaskType.Planning;
}
