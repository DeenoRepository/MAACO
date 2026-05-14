using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public sealed class TaskPlannerAgent : StubAgentBase
{
    public override string Name => "TaskPlannerAgent";
    public override string Role => "Planner";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Planning];
}
