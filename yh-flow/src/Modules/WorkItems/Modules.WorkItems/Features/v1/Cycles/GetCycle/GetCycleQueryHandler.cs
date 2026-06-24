using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.GetCycle;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Cycles.GetCycle;

/// <summary>
/// Handles <see cref="GetCycleQuery"/> — fetches a single cycle by id.
/// </summary>
public sealed class GetCycleQueryHandler : IQueryHandler<GetCycleQuery, CycleDto>
{
    private readonly WorkItemsDbContext _db;

    public GetCycleQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CycleDto> Handle(GetCycleQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.CycleId == Guid.Empty)
        {
            throw new CustomException("Cycle id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var cycle = await _db.Cycles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CycleId && c.ProjectId == query.ProjectId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
        {
            throw new NotFoundException($"Cycle '{query.CycleId}' was not found.");
        }

        var status = CycleDtoMapper.ComputeStatus(cycle);

        // Count issues by state group
        var issueCounts = await GetIssueCountsByGroup(_db, query.CycleId, cancellationToken);

        return CycleDtoMapper.ToDto(cycle, status,
            issueCounts.totalIssues, issueCounts.completedIssues,
            issueCounts.cancelledIssues, issueCounts.startedIssues,
            issueCounts.unstartedIssues, issueCounts.backlogIssues);
    }

    internal static async Task<(int totalIssues, int completedIssues, int cancelledIssues,
        int startedIssues, int unstartedIssues, int backlogIssues)>
        GetIssueCountsByGroup(WorkItemsDbContext db, Guid cycleId, CancellationToken cancellationToken)
    {
        var cycleIssues = await db.CycleIssues
            .AsNoTracking()
            .Where(ci => ci.CycleId == cycleId && !ci.IsDeleted)
            .Join(db.Issues.AsNoTracking().Where(i => !i.IsDeleted),
                ci => ci.IssueId,
                i => i.Id,
                (ci, i) => new { i.StateId })
            .Join(db.States.AsNoTracking().Where(s => !s.IsDeleted),
                ci => ci.StateId,
                s => s.Id,
                (ci, s) => s.Group)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalIssues = cycleIssues.Count;
        var backlogIssues = cycleIssues.Count(g => g == StateGroup.Backlog);
        var unstartedIssues = cycleIssues.Count(g => g == StateGroup.Unstarted);
        var startedIssues = cycleIssues.Count(g => g == StateGroup.Started);
        var completedIssues = cycleIssues.Count(g => g == StateGroup.Completed);
        var cancelledIssues = cycleIssues.Count(g => g == StateGroup.Cancelled);

        return (totalIssues, completedIssues, cancelledIssues, startedIssues, unstartedIssues, backlogIssues);
    }
}
