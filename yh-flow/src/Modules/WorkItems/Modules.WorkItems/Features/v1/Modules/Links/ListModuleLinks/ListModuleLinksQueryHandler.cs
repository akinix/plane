using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.Links;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Modules.Links.ListModuleLinks;

/// <summary>
/// Handles <see cref="ListModuleLinksQuery"/> — lists all links in a module.
/// </summary>
public sealed class ListModuleLinksQueryHandler : IQueryHandler<ListModuleLinksQuery, List<ModuleLinkDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListModuleLinksQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<ModuleLinkDto>> Handle(ListModuleLinksQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var links = await _db.Set<Domain.ModuleLink>()
            .AsNoTracking()
            .Where(l => l.ModuleId == query.ModuleId && !l.IsDeleted)
            .OrderBy(l => l.CreatedOnUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return links.Select(l => new ModuleLinkDto
        {
            Id = l.Id,
            Title = l.Title,
            Url = l.Url,
            Metadata = l.Metadata,
            ModuleId = l.ModuleId,
            CreatedAt = l.CreatedOnUtc,
        }).ToList();
    }
}
