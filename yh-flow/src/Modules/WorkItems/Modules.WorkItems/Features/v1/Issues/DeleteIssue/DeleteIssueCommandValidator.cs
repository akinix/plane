using FluentValidation;
using YH.Modules.WorkItems.Contracts.v1.Issues.DeleteIssue;

namespace YH.Modules.WorkItems.Features.v1.Issues.DeleteIssue;

/// <summary>
/// FluentValidation validator for <see cref="DeleteIssueCommand"/>.
/// </summary>
public sealed class DeleteIssueCommandValidator : AbstractValidator<DeleteIssueCommand>
{
    public DeleteIssueCommandValidator()
    {
        RuleFor(x => x.IssueId)
            .NotEmpty().WithMessage("Issue id is required.");
    }
}
