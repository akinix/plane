using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.EstimatePoints.CreateEstimatePoint;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Estimates.CreateEstimatePoint;

/// <summary>
/// Handles <see cref="CreateEstimatePointCommand"/> — creates a new estimate point.
/// Validates the parent estimate exists and key uniqueness (DB index enforces).
/// </summary>
public sealed class CreateEstimatePointCommandHandler : ICommandHandler<CreateEstimatePointCommand, EstimatePointDto>
{
    private readonly WorkItemsDbContext _db;

    public CreateEstimatePointCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<EstimatePointDto> Handle(CreateEstimatePointCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.EstimateId == Guid.Empty)
        {
            throw new CustomException("Estimate id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        // Validate parent estimate exists
        var estimateExists = await _db.Estimates
            .AnyAsync(e => e.Id == command.EstimateId && !e.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!estimateExists)
        {
            throw new NotFoundException($"Estimate '{command.EstimateId}' was not found.");
        }

        var point = EstimatePoint.Create(
            estimateId: command.EstimateId,
            key: command.Key,
            value: command.Value);

        _db.EstimatePoints.Add(point);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return EstimateDtoMapper.ToPointDto(point);
    }
}
