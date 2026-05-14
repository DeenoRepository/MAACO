using FluentValidation;

namespace MAACO.Api.Contracts.Settings;

public sealed class UpdateSettingsRequestValidator : AbstractValidator<UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.LlmProvider).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LlmModel).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaxParallelAgents).InclusiveBetween(1, 64);
    }
}
