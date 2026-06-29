using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Estimates.GetEstimate;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Estimates.GetEstimate;

/// <summary>
/// Handles <see cref="GetEstimateQuery"/> — fetches a single estimate with its points.
/// </summary>
public sealed class GetEstimateQueryHandler : IQueryHandler<GetEstimateQuery, EstimateDto>
{
    private readonly WorkItemsDbContext _db;

    public GetEstimateQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<EstimateDto> Handle(GetEstimateQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.EstimateId == Guid.Empty)
        {
            throw new CustomException("Estimate id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var estimate = await _db.Estimates
            .AsNoTracking()
            .Include(e => e.EstimatePoints.OrderBy(ep => ep.Key))
            .FirstOrDefaultAsync(e => e.Id == query.EstimateId && e.ProjectId == query.ProjectId && !e.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (estimate is null)
        {
            throw new NotFoundException($"Estimate '{query.EstimateId}' was not found.");
        }

        return EstimateDtoMapper.ToDto(estimate);
    }
}
