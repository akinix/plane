using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.ArchiveModule;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Features.v1.Modules.GetModule;

namespace YH.Modules.WorkItems.Features.v1.Modules.ListArchivedModules;

/// <summary>
/// Handles <see cref="ListArchivedModulesQuery"/> — lists archived modules in a project.
/// </summary>
public sealed class ListArchivedModulesQueryHandler : IQueryHandler<ListArchivedModulesQuery, List<ModuleDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListArchivedModulesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<ModuleDto>> Handle(ListArchivedModulesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return [];
        }

        var modules = await _db.Modules
            .AsNoTracking()
            .Where(m => m.ProjectId == query.ProjectId && !m.IsDeleted && m.ArchivedAt != null)
            .OrderByDescending(m => m.ArchivedAt)
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
