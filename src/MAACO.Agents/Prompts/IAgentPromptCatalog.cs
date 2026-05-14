namespace MAACO.Agents.Prompts;

public interface IAgentPromptCatalog
{
    string GetSystemPrompt(string agentName);
    string GetResponseSchema(string agentName);
}
