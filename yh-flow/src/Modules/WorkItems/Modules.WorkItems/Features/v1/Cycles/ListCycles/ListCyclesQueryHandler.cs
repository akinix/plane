using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.ListCycles;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.WorkItems.Features.v1.Cycles.GetCycle;

namespace YH.Modules.WorkItems.Features.v1.Cycles.ListCycles;

/// <summary>
/// Handles <see cref="ListCyclesQuery"/> — lists cycles in a project, ordered by SortOrder ascending.
/// Supports optional cycle_view filter.
/// </summary>
public sealed class ListCyclesQueryHandler : IQueryHandler<ListCyclesQuery, List<CycleDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListCyclesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<CycleDto>> Handle(ListCyclesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return [];
        }

        var cycles = await _db.Cycles
            .AsNoTracking()
            .Where(c => c.ProjectId == query.ProjectId && !c.IsDeleted && c.ArchivedAt == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // In-memory status computation and filtering
        var statuses = cycles.ToDictionary(c => c.Id, CycleDtoMapper.ComputeStatus);

        var filtered = FilterByCycleView(cycles, statuses, query.CycleView);

        var result = new List<CycleDto>(filtered.Count);
        foreach (var cycle in filtered)
        {
            var status = statuses[cycle.Id];

            // Count issues by state group for each cycle
            var issueCounts = await GetCycleQueryHandler.GetIssueCountsByGroup(_db, cycle.Id, cancellationToken);

            result.Add(CycleDtoMapper.ToDto(cycle, status,
                issueCounts.totalIssues, issueCounts.completedIssues,
                issueCounts.cancelledIssues, issueCounts.startedIssues,
                issueCounts.unstartedIssues, issueCounts.backlogIssues));
        }

        return result;
    }

    private static List<Cycle> FilterByCycleView(List<Cycle> cycles, Dictionary<Guid, string> statuses, string? cycleView)
    {
        if (string.IsNullOrWhiteSpace(cycleView) || cycleView == "all")
        {
            return cycles;
        }

        return cycleView switch
        {
            "current" => cycles.Where(c => statuses[c.Id] == "CURRENT").ToList(),
            "upcoming" => cycles.Where(c => statuses[c.Id] == "UPCOMING").ToList(),
            "completed" => cycles.Where(c => statuses[c.Id] == "COMPLETED").ToList(),
            "draft" => cycles.Where(c => statuses[c.Id] == "DRAFT").ToList(),
            "incomplete" => cycles.Where(c => statuses[c.Id] != "COMPLETED").ToList(),
            _ => cycles,
        };
    }
}
