using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.Issues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Features.v1.Issues;

namespace YH.Modules.WorkItems.Features.v1.Modules.Issues.ListModuleIssues;

/// <summary>
/// Handles <see cref="ListModuleIssuesQuery"/> — lists all issues in a module.
/// </summary>
public sealed class ListModuleIssuesQueryHandler : IQueryHandler<ListModuleIssuesQuery, List<IssueDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListModuleIssuesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<IssueDto>> Handle(ListModuleIssuesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get issue IDs for this module
        var issueIds = await _db.Set<Domain.ModuleIssue>()
            .AsNoTracking()
            .Where(mi => mi.ModuleId == query.ModuleId && !mi.IsDeleted)
            .Select(mi => mi.IssueId)
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
