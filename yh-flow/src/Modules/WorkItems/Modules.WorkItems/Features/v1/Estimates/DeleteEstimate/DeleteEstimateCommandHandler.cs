using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Estimates.DeleteEstimate;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Estimates.DeleteEstimate;

/// <summary>
/// Handles <see cref="DeleteEstimateCommand"/> — hard-deletes an estimate and cascades its points.
/// Estimate is NOT soft-deletable per Plane pattern (hard-delete with cascade).
/// </summary>
public sealed class DeleteEstimateCommandHandler : ICommandHandler<DeleteEstimateCommand>
{
    private readonly WorkItemsDbContext _db;

    public DeleteEstimateCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteEstimateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.EstimateId == Guid.Empty)
        {
            throw new CustomException("Estimate id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var estimate = await _db.Estimates
             .Include(e => e.EstimatePoints)
            .FirstOrDefaultAsync(e => e.Id == command.EstimateId && e.ProjectId == command.ProjectId && !e.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (estimate is null)
        {
            throw new NotFoundException($"Estimate '{command.EstimateId}' was not found.");
        }

        // Hard-delete per Plane pattern — Estimate is not soft-deletable
        _db.Estimates.Remove(estimate);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
