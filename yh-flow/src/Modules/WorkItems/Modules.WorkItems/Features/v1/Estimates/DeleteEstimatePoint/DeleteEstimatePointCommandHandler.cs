using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.EstimatePoints.DeleteEstimatePoint;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Estimates.DeleteEstimatePoint;

/// <summary>
/// Handles <see cref="DeleteEstimatePointCommand"/> — deletes an estimate point.
/// Checks if any Issue references this EstimatePointId — returns 409 if found.
/// </summary>
public sealed class DeleteEstimatePointCommandHandler : ICommandHandler<DeleteEstimatePointCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteEstimatePointCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteEstimatePointCommand command, CancellationToken cancellationToken)
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

        // Check if any Issue references this EstimatePoint
        var isReferenced = await _db.Issues
            .AnyAsync(i => i.EstimatePointId == command.EstimatePointId && !i.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (isReferenced)
        {
            throw new CustomException(
                "Cannot delete estimate point: it is referenced by one or more issues.",
                Array.Empty<string>(),
                System.Net.HttpStatusCode.Conflict);
        }

        _db.EstimatePoints.Remove(point);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
