using MAACO.Api.Contracts.Settings;
using Microsoft.Data.Sqlite;

namespace MAACO.Api.Services;

public sealed class AppSettingsDbOverrideSettingsService(IConfiguration configuration) : ISettingsService
{
    private const string SettingsSection = "Maaco:Settings";
    private readonly string connectionString = configuration.GetConnectionString("Maaco") ?? "Data Source=maaco.db";

    public async Task<SettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        await EnsureTableAsync(cancellationToken);
        var defaults = GetDefaults();
        var overrides = await ReadOverridesAsync(cancellationToken);

        return new SettingsDto(
            LlmProvider: overrides.TryGetValue("LlmProvider", out var llmProvider) ? llmProvider : defaults.LlmProvider,
            LlmModel: overrides.TryGetValue("LlmModel", out var llmModel) ? llmModel : defaults.LlmModel,
            RequireApproval: overrides.TryGetValue("RequireApproval", out var requireApproval) && bool.TryParse(requireApproval, out var parsedRequireApproval)
                ? parsedRequireApproval
                : defaults.RequireApproval,
            MaxParallelAgents: overrides.TryGetValue("MaxParallelAgents", out var maxParallelAgents) && int.TryParse(maxParallelAgents, out var parsedMaxParallelAgents)
                ? parsedMaxParallelAgents
                : defaults.MaxParallelAgents,
            BuildCommandOverride: overrides.TryGetValue("BuildCommandOverride", out var buildCommandOverride) ? NullIfEmpty(buildCommandOverride) : defaults.BuildCommandOverride,
            TestCommandOverride: overrides.TryGetValue("TestCommandOverride", out var testCommandOverride) ? NullIfEmpty(testCommandOverride) : defaults.TestCommandOverride);
    }

    public async Task<SettingsDto> UpdateAsync(UpdateSettingsRequest request, CancellationToken cancellationToken)
    {
        await EnsureTableAsync(cancellationToken);
        var normalized = new Dictionary<string, string?>
        {
            ["LlmProvider"] = request.LlmProvider.Trim(),
            ["LlmModel"] = request.LlmModel.Trim(),
            ["RequireApproval"] = request.RequireApproval.ToString(),
            ["MaxParallelAgents"] = request.MaxParallelAgents.ToString(),
            ["BuildCommandOverride"] = NormalizeOptional(request.BuildCommandOverride),
            ["TestCommandOverride"] = NormalizeOptional(request.TestCommandOverride)
        };

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var kvp in normalized)
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                INSERT INTO maaco_settings(Key, Value)
                VALUES ($key, $value)
                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value;
                """;
            command.Parameters.AddWithValue("$key", kvp.Key);
            command.Parameters.AddWithValue("$value", kvp.Value ?? string.Empty);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return await GetAsync(cancellationToken);
    }

    private SettingsDto GetDefaults()
    {
        var section = configuration.GetSection(SettingsSection);
        return new SettingsDto(
            LlmProvider: section["LlmProvider"] ?? "OpenAI-Compatible",
            LlmModel: section["LlmModel"] ?? "gpt-5",
            RequireApproval: bool.TryParse(section["RequireApproval"], out var requireApproval) ? requireApproval : true,
            MaxParallelAgents: int.TryParse(section["MaxParallelAgents"], out var maxParallelAgents) ? maxParallelAgents : 4,
            BuildCommandOverride: NullIfEmpty(section["BuildCommandOverride"]),
            TestCommandOverride: NullIfEmpty(section["TestCommandOverride"]));
    }

    private async Task EnsureTableAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS maaco_settings (
                Key TEXT NOT NULL PRIMARY KEY,
                Value TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<Dictionary<string, string>> ReadOverridesAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Key, Value FROM maaco_settings;";

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[reader.GetString(0)] = reader.GetString(1);
        }

        return result;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
