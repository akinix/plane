using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.IssueActivities.ListIssueActivities;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.IssueActivities.ListIssueActivities;

/// <summary>
/// Handles <see cref="ListIssueActivitiesQuery"/> — lists all activities for an issue.
/// Ordered by Epoch descending (most recent first).
/// </summary>
public sealed class ListIssueActivitiesQueryHandler : IQueryHandler<ListIssueActivitiesQuery, List<IssueActivityDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListIssueActivitiesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<IssueActivityDto>> Handle(ListIssueActivitiesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var activities = await _db.IssueActivities
            .AsNoTracking()
            .Where(a => a.IssueId == query.IssueId)
            .OrderByDescending(a => a.Epoch)
            .Select(a => new IssueActivityDto
            {
                Id = a.Id,
                IssueId = a.IssueId,
                Verb = a.Verb,
                Field = a.Field,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                Comment = a.Comment,
                ActorId = a.ActorId,
                IssueCommentId = a.IssueCommentId,
                Epoch = a.Epoch,
                CreatedAt = a.CreatedOnUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return activities;
    }
}
