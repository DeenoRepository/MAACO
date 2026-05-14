using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public sealed class OrchestratorAgent : StubAgentBase
{
    public override string Name => "OrchestratorAgent";
    public override string Role => "Orchestrator";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Orchestration, AgentCapability.Planning];
}
