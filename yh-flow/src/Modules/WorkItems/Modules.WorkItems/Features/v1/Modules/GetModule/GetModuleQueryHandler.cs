using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.GetModule;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Modules.GetModule;

/// <summary>
/// Handles <see cref="GetModuleQuery"/> — fetches a single module by id.
/// </summary>
public sealed class GetModuleQueryHandler : IQueryHandler<GetModuleQuery, ModuleDto>
{
    private readonly WorkItemsDbContext _db;

    public GetModuleQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<ModuleDto> Handle(GetModuleQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ModuleId == Guid.Empty)
        {
            throw new CustomException("Module id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var module = await _db.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.ModuleId && m.ProjectId == query.ProjectId && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Module '{query.ModuleId}' was not found.");
        }

        // Count issues by state group
        var issueCounts = await GetIssueCountsByGroup(_db, query.ModuleId, cancellationToken);

        return Features.v1.Modules.ModuleDtoMapper.ToDto(module,
            issueCounts.totalIssues, issueCounts.completedIssues,
            issueCounts.cancelledIssues, issueCounts.startedIssues,
            issueCounts.unstartedIssues, issueCounts.backlogIssues);
    }

    internal static async Task<(int totalIssues, int completedIssues, int cancelledIssues,
        int startedIssues, int unstartedIssues, int backlogIssues)>
        GetIssueCountsByGroup(WorkItemsDbContext db, Guid moduleId, CancellationToken cancellationToken)
    {
        var moduleIssues = await db.ModuleIssues
            .AsNoTracking()
            .Where(mi => mi.ModuleId == moduleId && !mi.IsDeleted)
            .Join(db.Issues.AsNoTracking().Where(i => !i.IsDeleted),
                mi => mi.IssueId,
                i => i.Id,
                (mi, i) => new { i.StateId })
            .Join(db.States.AsNoTracking().Where(s => !s.IsDeleted),
                mi => mi.StateId,
                s => s.Id,
                (mi, s) => s.Group)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalIssues = moduleIssues.Count;
        var backlogIssues = moduleIssues.Count(g => g == StateGroup.Backlog);
        var unstartedIssues = moduleIssues.Count(g => g == StateGroup.Unstarted);
        var startedIssues = moduleIssues.Count(g => g == StateGroup.Started);
        var completedIssues = moduleIssues.Count(g => g == StateGroup.Completed);
        var cancelledIssues = moduleIssues.Count(g => g == StateGroup.Cancelled);

        return (totalIssues, completedIssues, cancelledIssues, startedIssues, unstartedIssues, backlogIssues);
    }
}
