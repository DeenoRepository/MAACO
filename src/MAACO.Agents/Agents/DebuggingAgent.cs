using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public sealed class DebuggingAgent : StubAgentBase
{
    public override string Name => "DebuggingAgent";
    public override string Role => "Debugger";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Debugging];
}
