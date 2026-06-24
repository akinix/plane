using FluentValidation;
using YH.Modules.WorkItems.Contracts;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.CreateIssueComment;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.CreateIssueComment;

/// <summary>
/// Validator for <see cref="CreateIssueCommentCommand"/>.
/// </summary>
public sealed class CreateIssueCommentCommandValidator : AbstractValidator<CreateIssueCommentCommand>
{
    public CreateIssueCommentCommandValidator()
    {
        RuleFor(x => x.CommentHtml)
            .NotEmpty().WithMessage("Comment HTML is required.")
            .MaximumLength(WorkItemsConstants.DescriptionMaxLength).WithMessage($"Comment HTML must not exceed {WorkItemsConstants.DescriptionMaxLength} characters.");
    }
}
