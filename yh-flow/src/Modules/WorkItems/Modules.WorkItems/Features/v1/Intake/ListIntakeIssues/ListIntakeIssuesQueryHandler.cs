using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Intake.ListIntakeIssues;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Intake.ListIntakeIssues;

/// <summary>
/// Handles <see cref="ListIntakeIssuesQuery"/> — lists intake issues for a project.
/// Ordered by CreatedOnUtc descending. Optional status filter.
/// Includes the nested Issue for display.
/// </summary>
public sealed class ListIntakeIssuesQueryHandler : IQueryHandler<ListIntakeIssuesQuery, List<IntakeIssueDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListIntakeIssuesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<IntakeIssueDto>> Handle(ListIntakeIssuesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Domain.IntakeIssue> filtered = _db.Set<Domain.IntakeIssue>()
            .AsNoTracking()
            .Include(i => i.Issue)
            .Where(i => i.ProjectId == query.ProjectId);

        if (query.Status.HasValue)
        {
            var status = (Domain.IntakeIssueStatus)query.Status.Value;
            filtered = filtered.Where(i => i.Status == status);
        }

        var items = await filtered
            .OrderByDescending(i => i.CreatedOnUtc)
            .Select(i => new IntakeIssueDto
            {
                Id = i.Id,
                IssueId = i.IssueId,
                ProjectId = i.ProjectId,
                Status = (int)i.Status,
                SnoozedTill = i.SnoozedTill,
                DuplicateToIssueId = i.DuplicateToIssueId,
                Source = i.Source,
                Issue = new IssueDto
                {
                    Id = i.Issue.Id,
                    Name = i.Issue.Name,
                    Priority = i.Issue.Priority,
                    SequenceId = i.Issue.SequenceId,
                    ProjectId = i.Issue.ProjectId,
                    StateId = i.Issue.StateId,
                    IsDraft = i.Issue.IsDraft,
                    CreatedAt = i.Issue.CreatedOnUtc,
                    UpdatedAt = i.Issue.LastModifiedOnUtc,
                },
                CreatedAt = i.CreatedOnUtc,
                UpdatedAt = i.LastModifiedOnUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return items;
    }
}
