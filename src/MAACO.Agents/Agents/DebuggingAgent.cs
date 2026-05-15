using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class DebuggingAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog,
    ILlmGateway llmGateway) : LlmAgentBase(tools, promptCatalog, llmGateway)
{
    public override string Name => "DebuggingAgent";
    public override string Role => "Debugger";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Debugging];
    protected override LlmTaskType TaskType => LlmTaskType.Debugging;
}
