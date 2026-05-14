using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class DebuggingAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog) : StubAgentBase(tools, promptCatalog)
{
    public override string Name => "DebuggingAgent";
    public override string Role => "Debugger";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Debugging];
}
