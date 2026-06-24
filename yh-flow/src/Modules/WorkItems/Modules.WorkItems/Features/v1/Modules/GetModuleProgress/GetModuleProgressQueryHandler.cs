using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.GetModuleProgress;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Modules.GetModuleProgress;

/// <summary>
/// Handles <see cref="GetModuleProgressQuery"/> — computes real-time progress for a module
/// by aggregating ModuleIssues -> Issues -> States -> StateGroup.
/// </summary>
public sealed class GetModuleProgressQueryHandler : IQueryHandler<GetModuleProgressQuery, ModuleProgressDto>
{
    private readonly WorkItemsDbContext _db;

    public GetModuleProgressQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<ModuleProgressDto> Handle(GetModuleProgressQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Verify module exists
        var module = await _db.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.ModuleId && m.ProjectId == query.ProjectId && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Module '{query.ModuleId}' was not found.");
        }

        // Real-time aggregate: ModuleIssues -> Issues -> States -> Group by StateGroup
        var progress = await (
            from mi in _db.Set<ModuleIssue>().AsNoTracking()
            join i in _db.Issues.AsNoTracking() on mi.IssueId equals i.Id
            join s in _db.States.AsNoTracking() on i.StateId equals s.Id
            where mi.ModuleId == query.ModuleId && !mi.IsDeleted && !i.IsDeleted
            group s by s.Group into g
            select new { Group = g.Key, Count = g.Count() }
        ).ToListAsync(cancellationToken)
        .ConfigureAwait(false);

        var total = progress.Sum(x => x.Count);
        var completed = progress.Where(x => x.Group == StateGroup.Completed).Sum(x => x.Count);
        var cancelled = progress.Where(x => x.Group == StateGroup.Cancelled).Sum(x => x.Count);
        var started = progress.Where(x => x.Group == StateGroup.Started).Sum(x => x.Count);
        var unstarted = progress.Where(x => x.Group == StateGroup.Unstarted).Sum(x => x.Count);
        var backlog = progress.Where(x => x.Group == StateGroup.Backlog).Sum(x => x.Count);

        return new ModuleProgressDto
        {
            TotalIssues = total,
            CompletedIssues = completed,
            CancelledIssues = cancelled,
            StartedIssues = started,
            UnstartedIssues = unstarted,
            BacklogIssues = backlog,
            CompletedPercentage = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0
        };
    }
}
