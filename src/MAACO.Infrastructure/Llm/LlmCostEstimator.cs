namespace MAACO.Infrastructure.Llm;

internal static class LlmCostEstimator
{
    // Simplified default estimate for MVP: $0.002 per 1K tokens.
    private const decimal DefaultUsdPerThousandTokens = 0.002m;

    public static decimal EstimateUsd(int totalTokens) =>
        Math.Round((totalTokens / 1000m) * DefaultUsdPerThousandTokens, 6, MidpointRounding.AwayFromZero);
}
