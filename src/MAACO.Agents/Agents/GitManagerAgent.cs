using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public sealed class GitManagerAgent : StubAgentBase
{
    public override string Name => "GitManagerAgent";
    public override string Role => "GitManager";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Git];
}
