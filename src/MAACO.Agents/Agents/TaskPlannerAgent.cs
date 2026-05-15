using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class TaskPlannerAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog,
    ILlmGateway llmGateway) : LlmAgentBase(tools, promptCatalog, llmGateway)
{
    public override string Name => "TaskPlannerAgent";
    public override string Role => "Planner";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Planning];
    protected override LlmTaskType TaskType => LlmTaskType.Planning;
}
