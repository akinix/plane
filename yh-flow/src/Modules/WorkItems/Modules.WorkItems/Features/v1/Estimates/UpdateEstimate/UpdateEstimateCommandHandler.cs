using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Estimates.UpdateEstimate;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Estimates.UpdateEstimate;

/// <summary>
/// Handles <see cref="UpdateEstimateCommand"/> — updates mutable fields on an estimate system.
/// </summary>
public sealed class UpdateEstimateCommandHandler : ICommandHandler<UpdateEstimateCommand, EstimateDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateEstimateCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<EstimateDto> Handle(UpdateEstimateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.EstimateId == Guid.Empty)
        {
            throw new CustomException("Estimate id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var estimate = await _db.Estimates
            .Include(e => e.EstimatePoints.OrderBy(ep => ep.Key))
            .FirstOrDefaultAsync(e => e.Id == command.EstimateId && e.ProjectId == command.ProjectId && !e.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (estimate is null)
        {
            throw new NotFoundException($"Estimate '{command.EstimateId}' was not found.");
        }

        estimate.Update(
            name: command.Name,
            type: command.Type);

        if (command.IsLastUsed is true)
        {
            estimate.MarkAsLastUsed();
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return EstimateDtoMapper.ToDto(estimate);
    }
}
