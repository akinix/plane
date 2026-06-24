using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.DateCheckCycle;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Cycles.DateCheckCycle;

/// <summary>
/// Handles <see cref="DateCheckCycleCommand"/> — checks if date range overlaps existing cycles.
/// Plane-style triple overlap detection.
/// </summary>
public sealed class DateCheckCycleCommandHandler : ICommandHandler<DateCheckCycleCommand, DateCheckCycleResponse>
{
    private readonly WorkItemsDbContext _db;

    public DateCheckCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<DateCheckCycleResponse> Handle(DateCheckCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Plane triple overlap detection:
        // 1) existing starts within our range (existing.StartDate <= our.EndDate && existing.StartDate >= our.StartDate) → overlap A
        // 2) existing ends within our range → overlap B
        // 3) existing fully contains our range → overlap C
        var overlapping = await _db.Cycles
            .AsNoTracking()
            .Where(c => c.ProjectId == command.ProjectId && !c.IsDeleted)
            .Where(c => c.StartDate != null && c.EndDate != null)
            .Where(c => (c.StartDate <= command.StartDate && c.EndDate >= command.StartDate) ||
                        (c.StartDate <= command.EndDate && c.EndDate >= command.EndDate) ||
                        (c.StartDate >= command.StartDate && c.EndDate <= command.EndDate))
            .Where(c => command.ExcludeCycleId == null || c.Id != command.ExcludeCycleId.Value)
            .Select(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new DateCheckCycleResponse(
            overlapping.Count == 0,
            overlapping.Count > 0 ? overlapping : null);
    }
}
