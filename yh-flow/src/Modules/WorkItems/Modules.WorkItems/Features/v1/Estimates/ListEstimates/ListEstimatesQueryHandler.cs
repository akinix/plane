using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Estimates.ListEstimates;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Estimates.ListEstimates;

/// <summary>
/// Handles <see cref="ListEstimatesQuery"/> — lists all estimates for a project with points.
/// </summary>
public sealed class ListEstimatesQueryHandler : IQueryHandler<ListEstimatesQuery, List<EstimateDto>>
{
    private readonly WorkItemsDbContext _db;

    public ListEstimatesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<List<EstimateDto>> Handle(ListEstimatesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return [];
        }

        var estimates = await _db.Estimates
            .AsNoTracking()
            .Include(e => e.EstimatePoints.OrderBy(ep => ep.Key))
            .Where(e => e.ProjectId == query.ProjectId && !e.IsDeleted)
            .OrderBy(e => e.CreatedOnUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return EstimateDtoMapper.ToDtoList(estimates);
    }
}
