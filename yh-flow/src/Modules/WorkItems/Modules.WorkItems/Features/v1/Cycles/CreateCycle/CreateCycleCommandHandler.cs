using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.CreateCycle;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Cycles.CreateCycle;

/// <summary>
/// Handles <see cref="CreateCycleCommand"/> — creates a new project cycle.
/// </summary>
public sealed class CreateCycleCommandHandler : ICommandHandler<CreateCycleCommand, CreateCycleResponse>
{
    private readonly WorkItemsDbContext _db;

    public CreateCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateCycleResponse> Handle(CreateCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Query the minimum sort order to place this cycle at the beginning
        var minSortOrder = await _db.Cycles
            .AsNoTracking()
            .Where(c => c.ProjectId == command.ProjectId && !c.IsDeleted)
            .MinAsync(c => (double?)c.SortOrder, cancellationToken)
            .ConfigureAwait(false);

        var cycle = Cycle.Create(
            name: command.Name,
            projectId: command.ProjectId,
            startDate: command.StartDate,
            endDate: command.EndDate,
            description: command.Description,
            timezone: command.Timezone);

        cycle.AssignSortOrder(minSortOrder.HasValue ? minSortOrder.Value - 10000.0 : 65535.0);

        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateCycleResponse(cycle.Id);
    }
}
