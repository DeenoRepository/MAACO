using MAACO.Api.Contracts.Settings;

namespace MAACO.Api.Services;

public sealed class InMemorySettingsService : ISettingsService
{
    private SettingsDto settings = new(
        LlmProvider: "OpenAI-Compatible",
        LlmModel: "gpt-5",
        RequireApproval: true,
        MaxParallelAgents: 4,
        BuildCommandOverride: null,
        TestCommandOverride: null);

    public Task<SettingsDto> GetAsync(CancellationToken cancellationToken) =>
        Task.FromResult(settings);

    public Task<SettingsDto> UpdateAsync(UpdateSettingsRequest request, CancellationToken cancellationToken)
    {
        settings = new SettingsDto(
            request.LlmProvider.Trim(),
            request.LlmModel.Trim(),
            request.RequireApproval,
            request.MaxParallelAgents,
            string.IsNullOrWhiteSpace(request.BuildCommandOverride) ? null : request.BuildCommandOverride.Trim(),
            string.IsNullOrWhiteSpace(request.TestCommandOverride) ? null : request.TestCommandOverride.Trim());

        return Task.FromResult(settings);
    }
}
