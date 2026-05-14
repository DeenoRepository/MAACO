using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class OrchestratorAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog) : StubAgentBase(tools, promptCatalog)
{
    public override string Name => "OrchestratorAgent";
    public override string Role => "Orchestrator";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Orchestration, AgentCapability.Planning];
}
