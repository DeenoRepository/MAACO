using MAACO.Agents.Abstractions;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Tools;

namespace MAACO.Agents.Agents;

public sealed class TestWriterAgent(
    IEnumerable<IAgentTool> tools,
    IAgentPromptCatalog promptCatalog,
    ILlmGateway llmGateway) : LlmAgentBase(tools, promptCatalog, llmGateway)
{
    public override string Name => "TestWriterAgent";
    public override string Role => "TestWriter";
    public override IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Testing];
    protected override LlmTaskType TaskType => LlmTaskType.Coding;
}
