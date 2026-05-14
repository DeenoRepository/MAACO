using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public sealed class TestWriterAgent : StubAgentBase
{
    public override string Name => "TestWriterAgent";
    public override string Role => "TestWriter";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Testing];
}
