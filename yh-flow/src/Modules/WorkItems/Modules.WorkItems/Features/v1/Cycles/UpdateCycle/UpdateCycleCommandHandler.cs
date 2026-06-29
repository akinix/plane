using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Cycles.UpdateCycle;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Features.v1.Cycles.GetCycle;

namespace YH.Modules.WorkItems.Features.v1.Cycles.UpdateCycle;

/// <summary>
/// Handles <see cref="UpdateCycleCommand"/> — applies PATCH updates to mutable cycle fields.
/// COMPLETED cycles (EndDate in the past) use UpdateRestricted (external fields only);
/// active cycles use the full Update method.
/// </summary>
public sealed class UpdateCycleCommandHandler : ICommandHandler<UpdateCycleCommand, CycleDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CycleDto> Handle(UpdateCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.CycleId == Guid.Empty)
        {
            throw new CustomException("Cycle id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var cycle = await _db.Cycles
            .FirstOrDefaultAsync(c => c.Id == command.CycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
        {
            throw new NotFoundException($"Cycle '{command.CycleId}' was not found.");
        }

        // COMPLETED cycles use UpdateRestricted (external fields only); active cycles use full Update
        if (cycle.EndDate.HasValue && cycle.EndDate.Value < DateTimeOffset.UtcNow)
        {
            cycle.UpdateRestricted();
        }
        else
        {
            cycle.Update(
                name: command.Name,
                description: command.Description,
                startDate: command.StartDate,
                endDate: command.EndDate,
                timezone: command.Timezone,
                sortOrder: command.SortOrder);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var status = Features.v1.Cycles.CycleDtoMapper.ComputeStatus(cycle);

        // Count issues by state group
        var issueCounts = await GetCycleQueryHandler.GetIssueCountsByGroup(_db, command.CycleId, cancellationToken);

        return Features.v1.Cycles.CycleDtoMapper.ToDto(cycle, status,
            issueCounts.totalIssues, issueCounts.completedIssues,
            issueCounts.cancelledIssues, issueCounts.startedIssues,
            issueCounts.unstartedIssues, issueCounts.backlogIssues);
    }
}
