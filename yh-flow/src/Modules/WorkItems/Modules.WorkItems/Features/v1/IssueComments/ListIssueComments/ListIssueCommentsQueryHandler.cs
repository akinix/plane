using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueComments.ListIssueComments;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.IssueComments.ListIssueComments;

/// <summary>
/// Handles <see cref="ListIssueCommentsQuery"/> — lists all comments for an issue.
/// Ordered by CreatedOnUtc ascending (oldest first).
/// </summary>
public sealed class ListIssueCommentsQueryHandler : IQueryHandler<ListIssueCommentsQuery, List<IssueCommentDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListIssueCommentsQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<IssueCommentDto>> Handle(ListIssueCommentsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var comments = await _db.IssueComments
            .AsNoTracking()
            .Where(c => c.IssueId == query.IssueId && !c.IsDeleted)
            .OrderBy(c => c.CreatedOnUtc)
            .Select(c => new IssueCommentDto
            {
                Id = c.Id,
                IssueId = c.IssueId,
                CommentHtml = c.CommentHtml,
                CommentJson = c.CommentJson,
                CommentStripped = c.CommentStripped,
                ActorId = c.ActorId,
                ParentId = c.ParentId,
                EditedAt = c.EditedAt,
                CreatedAt = c.CreatedOnUtc,
                UpdatedAt = c.LastModifiedOnUtc,
                DeletedAt = c.DeletedOnUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return comments;
    }
}
