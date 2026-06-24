using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.ListModules;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Features.v1.Modules.GetModule;

namespace YH.Modules.WorkItems.Features.v1.Modules.ListModules;

/// <summary>
/// Handles <see cref="ListModulesQuery"/> — lists modules in a project, ordered by SortOrder ascending.
/// Supports optional status filter. Status is a stored field (DB-level filter, unlike Cycle).
/// </summary>
public sealed class ListModulesQueryHandler : IQueryHandler<ListModulesQuery, List<ModuleDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListModulesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<ModuleDto>> Handle(ListModulesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return [];
        }

        var modulesQuery = _db.Modules
            .AsNoTracking()
            .Where(m => m.ProjectId == query.ProjectId && !m.IsDeleted && m.ArchivedAt == null);

        // Apply status filter (stored field — can filter at DB level)
        if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "all")
        {
            modulesQuery = modulesQuery.Where(m => m.Status == query.Status);
        }

        var modules = await modulesQuery
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = new List<ModuleDto>(modules.Count);
        foreach (var module in modules)
        {
            var issueCounts = await GetModuleQueryHandler.GetIssueCountsByGroup(_db, module.Id, cancellationToken);

            result.Add(Features.v1.Modules.ModuleDtoMapper.ToDto(module,
                issueCounts.totalIssues, issueCounts.completedIssues,
                issueCounts.cancelledIssues, issueCounts.startedIssues,
                issueCounts.unstartedIssues, issueCounts.backlogIssues));
        }

        return result;
    }
}
