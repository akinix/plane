using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.ArchiveCycle;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Features.v1.Cycles.GetCycle;

namespace YH.Modules.WorkItems.Features.v1.Cycles.ListArchivedCycles;

/// <summary>
/// Handles <see cref="ListArchivedCyclesQuery"/> — lists archived cycles in a project.
/// </summary>
public sealed class ListArchivedCyclesQueryHandler : IQueryHandler<ListArchivedCyclesQuery, List<CycleDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListArchivedCyclesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<CycleDto>> Handle(ListArchivedCyclesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return [];
        }

        var cycles = await _db.Cycles
            .AsNoTracking()
            .Where(c => c.ProjectId == query.ProjectId && !c.IsDeleted && c.ArchivedAt != null)
            .OrderByDescending(c => c.ArchivedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = new List<CycleDto>(cycles.Count);
        foreach (var cycle in cycles)
        {
            var status = CycleDtoMapper.ComputeStatus(cycle);
            var issueCounts = await GetCycleQueryHandler.GetIssueCountsByGroup(_db, cycle.Id, cancellationToken);

            result.Add(CycleDtoMapper.ToDto(cycle, status,
                issueCounts.totalIssues, issueCounts.completedIssues,
                issueCounts.cancelledIssues, issueCounts.startedIssues,
                issueCounts.unstartedIssues, issueCounts.backlogIssues));
        }

        return result;
    }
}
