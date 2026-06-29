using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.DeleteIssueComment;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.DeleteIssueComment;

/// <summary>
/// Handles <see cref="DeleteIssueCommentCommand"/> — soft-deletes a comment.
/// </summary>
public sealed class DeleteIssueCommentCommandHandler : ICommandHandler<DeleteIssueCommentCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteIssueCommentCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteIssueCommentCommand command, CancellationToken cancellationToken)
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

        comment.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
