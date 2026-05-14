using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public sealed class BackendDeveloperAgent : StubAgentBase
{
    public override string Name => "BackendDeveloperAgent";
    public override string Role => "Developer";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Coding];
}
