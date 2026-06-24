using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.Issues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Features.v1.Issues;

namespace YH.Modules.WorkItems.Features.v1.Cycles.Issues.ListCycleIssues;

/// <summary>
/// Handles <see cref="ListCycleIssuesQuery"/> — lists all issues in a cycle.
/// </summary>
public sealed class ListCycleIssuesQueryHandler : IQueryHandler<ListCycleIssuesQuery, List<IssueDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListCycleIssuesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<IssueDto>> Handle(ListCycleIssuesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get issue IDs for this cycle
        var issueIds = await _db.Set<Domain.CycleIssue>()
            .AsNoTracking()
            .Where(ci => ci.CycleId == query.CycleId && !ci.IsDeleted)
            .Select(ci => ci.IssueId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (issueIds.Count == 0)
        {
            return [];
        }

        // Load the actual issues, filtering out deleted ones
        var issues = await _db.Issues
            .AsNoTracking()
            .Where(i => issueIds.Contains(i.Id) && !i.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return IssueDtoMapper.ToDtoList(issues);
    }
}
