using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class BackendDeveloperAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog) : StubAgentBase(tools, promptCatalog)
{
    public override string Name => "BackendDeveloperAgent";
    public override string Role => "Developer";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Coding];
}
