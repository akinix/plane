using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.CreateIssueComment;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.CreateIssueComment;

/// <summary>
/// Handles <see cref="CreateIssueCommentCommand"/> — creates a new comment on an issue.
/// Sanitizes comment HTML before persisting. Validates ParentId references an existing comment
/// in the same issue.
/// </summary>
public sealed class CreateIssueCommentCommandHandler : ICommandHandler<CreateIssueCommentCommand, IssueCommentDto>
{
    private readonly WorkItemsDbContext _db;

    public CreateIssueCommentCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<IssueCommentDto> Handle(CreateIssueCommentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.IssueId == Guid.Empty)
        {
            throw new CustomException("Issue id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Verify the issue exists in this project
        var issueExists = await _db.Issues
            .AnyAsync(i => i.Id == command.IssueId && i.ProjectId == command.ProjectId && !i.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!issueExists)
        {
            throw new NotFoundException($"Issue '{command.IssueId}' was not found.");
        }

        // Validate ParentId references an existing comment in the same issue
        if (command.ParentId.HasValue)
        {
            var parentExists = await _db.IssueComments
                .AnyAsync(c => c.Id == command.ParentId.Value && c.IssueId == command.IssueId && !c.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (!parentExists)
            {
                throw new CustomException(
                    "Parent comment not found in this issue.",
                    Array.Empty<string>(),
                    System.Net.HttpStatusCode.BadRequest);
            }
        }

        var comment = IssueComment.Create(
            issueId: command.IssueId,
            commentHtml: command.CommentHtml,
            actorId: command.ActorId,
            parentId: command.ParentId,
            commentJson: command.CommentJson);

        _db.IssueComments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return IssueCommentDtoMapper.ToDto(comment);
    }
}
