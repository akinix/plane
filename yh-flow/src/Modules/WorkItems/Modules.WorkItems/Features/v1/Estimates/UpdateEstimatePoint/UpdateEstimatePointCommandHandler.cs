using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.EstimatePoints.UpdateEstimatePoint;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Estimates.UpdateEstimatePoint;

/// <summary>
/// Handles <see cref="UpdateEstimatePointCommand"/> — updates an estimate point's key/value.
/// </summary>
public sealed class UpdateEstimatePointCommandHandler : ICommandHandler<UpdateEstimatePointCommand, EstimatePointDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateEstimatePointCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<EstimatePointDto> Handle(UpdateEstimatePointCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.EstimatePointId == Guid.Empty)
        {
            throw new CustomException("Estimate point id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var point = await _db.EstimatePoints
            .FirstOrDefaultAsync(ep => ep.Id == command.EstimatePointId, cancellationToken)
            .ConfigureAwait(false);

        if (point is null)
        {
            throw new NotFoundException($"Estimate point '{command.EstimatePointId}' was not found.");
        }

        point.UpdateValue(
            value: command.Value ?? point.Value,
            key: command.Key);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return EstimateDtoMapper.ToPointDto(point);
    }
}
