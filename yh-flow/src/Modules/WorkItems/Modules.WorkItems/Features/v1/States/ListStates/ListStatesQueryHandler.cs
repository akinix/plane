using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.States.ListStates;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.States.ListStates;

/// <summary>
/// Handles <see cref="ListStatesQuery"/> — lists states in a project, ordered by SortOrder ascending.
/// </summary>
public sealed class ListStatesQueryHandler : IQueryHandler<ListStatesQuery, List<StateDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListStatesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<StateDto>> Handle(ListStatesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return [];
        }

        var filtered = _db.States
            .AsNoTracking()
            .Where(s => s.ProjectId == query.ProjectId && !s.IsDeleted);

        if (query.Group.HasValue)
        {
            filtered = filtered.Where(s => (int)s.Group == query.Group.Value);
        }

        var states = await filtered
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return StateDtoMapper.ToDtoList(states);
    }
}
