using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public sealed class DocumentationAgent : StubAgentBase
{
    public override string Name => "DocumentationAgent";
    public override string Role => "Documenter";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Documentation];
}
