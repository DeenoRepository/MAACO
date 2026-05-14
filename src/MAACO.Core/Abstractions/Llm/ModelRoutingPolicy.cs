namespace MAACO.Core.Abstractions.Llm;

public sealed record ModelRoutingPolicy(
    string PlanningModel,
    string CodingModel,
    string DebuggingModel,
    string SummaryModel,
    string FallbackModel);
