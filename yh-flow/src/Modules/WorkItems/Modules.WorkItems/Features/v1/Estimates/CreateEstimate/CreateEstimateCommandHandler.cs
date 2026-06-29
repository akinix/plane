using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Estimates.CreateEstimate;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Estimates.CreateEstimate;

/// <summary>
/// Handles <see cref="CreateEstimateCommand"/> — creates an estimate system with optional initial points.
/// </summary>
public sealed class CreateEstimateCommandHandler : ICommandHandler<CreateEstimateCommand, CreateEstimateResponse>
{
    private readonly WorkItemsDbContext _db;

    public CreateEstimateCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateEstimateResponse> Handle(CreateEstimateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var estimate = Estimate.Create(
            name: command.Name,
            type: command.Type,
            projectId: command.ProjectId);

        // Seed initial estimate points if provided
        if (command.InitialPoints is { Count: > 0 })
        {
            foreach (var point in command.InitialPoints)
            {
                var ep = EstimatePoint.Create(
                    estimateId: estimate.Id,
                    key: point.Key,
                    value: point.Value);
                estimate.AddPoint(ep);
            }
        }

        _db.Estimates.Add(estimate);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateEstimateResponse(estimate.Id);
    }
}
