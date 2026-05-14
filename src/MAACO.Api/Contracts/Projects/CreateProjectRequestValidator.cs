using FluentValidation;

namespace MAACO.Api.Contracts.Projects;

public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
