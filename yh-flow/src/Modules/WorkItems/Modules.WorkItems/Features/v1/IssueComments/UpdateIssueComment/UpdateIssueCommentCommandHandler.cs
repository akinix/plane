using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.UpdateIssueComment;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.UpdateIssueComment;

/// <summary>
/// Handles <see cref="UpdateIssueCommentCommand"/> — updates a comment.
/// Only the comment author can edit the comment body.
/// </summary>
public sealed class UpdateIssueCommentCommandHandler : ICommandHandler<UpdateIssueCommentCommand, IssueCommentDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateIssueCommentCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<IssueCommentDto> Handle(UpdateIssueCommentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.CommentId == Guid.Empty)
        {
            throw new CustomException("Comment id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var comment = await _db.IssueComments
            .FirstOrDefaultAsync(c => c.Id == command.CommentId && c.IssueId == command.IssueId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (comment is null)
        {
            throw new NotFoundException($"Comment '{command.CommentId}' was not found.");
        }

        comment.Update(command.CommentHtml, command.CommentJson);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return IssueCommentDtoMapper.ToDto(comment);
    }
}
