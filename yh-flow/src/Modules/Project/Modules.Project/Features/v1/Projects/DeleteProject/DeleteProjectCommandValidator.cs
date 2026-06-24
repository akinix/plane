using FluentValidation;
using YH.Modules.Project.Contracts.v1.Projects.DeleteProject;

namespace YH.Modules.Project.Features.v1.Projects.DeleteProject;

/// <summary>
/// FluentValidation validator for <see cref="DeleteProjectCommand"/>.
/// </summary>
public sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user id is required.");
    }
}
