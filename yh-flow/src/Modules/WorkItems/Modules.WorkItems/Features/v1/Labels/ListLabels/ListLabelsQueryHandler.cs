using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Labels.ListLabels;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Labels.ListLabels;

/// <summary>
/// Handles <see cref="ListLabelsQuery"/> — lists labels in a project, ordered by SortOrder ascending.
/// </summary>
public sealed class ListLabelsQueryHandler : IQueryHandler<ListLabelsQuery, List<LabelDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListLabelsQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<LabelDto>> Handle(ListLabelsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return [];
        }

        var filtered = _db.Labels
            .AsNoTracking()
            .Where(l => l.ProjectId == query.ProjectId && !l.IsDeleted);

        // Optional ParentId filter
        if (query.ParentId.HasValue)
        {
            filtered = query.ParentId.Value == Guid.Empty
                ? filtered.Where(l => l.ParentId == null)
                : filtered.Where(l => l.ParentId == query.ParentId.Value);
        }

        var labels = await filtered
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return LabelDtoMapper.ToDtoList(labels);
    }
}
