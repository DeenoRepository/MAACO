using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class DocumentationAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog) : StubAgentBase(tools, promptCatalog)
{
    public override string Name => "DocumentationAgent";
    public override string Role => "Documenter";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Documentation];
}
