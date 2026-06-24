using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Modules.UpdateModule;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Features.v1.Modules.GetModule;

namespace YH.Modules.WorkItems.Features.v1.Modules.UpdateModule;

/// <summary>
/// Handles <see cref="UpdateModuleCommand"/> — applies PATCH updates to mutable module fields.
/// Module has no COMPLETED edit restriction (unlike Cycle).
/// </summary>
public sealed class UpdateModuleCommandHandler : ICommandHandler<UpdateModuleCommand, ModuleDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateModuleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<ModuleDto> Handle(UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ModuleId == Guid.Empty)
        {
            throw new CustomException("Module id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var module = await _db.Modules
            .FirstOrDefaultAsync(m => m.Id == command.ModuleId && m.ProjectId == command.ProjectId && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Module '{command.ModuleId}' was not found.");
        }

        // Module has no COMPLETED edit restriction (unlike Cycle — D-03).
        module.Update(
            name: command.Name,
            description: command.Description,
            status: command.Status,
            startDate: command.StartDate,
            targetDate: command.TargetDate,
            leadId: command.LeadId,
            sortOrder: command.SortOrder);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Count issues by state group
        var issueCounts = await GetModuleQueryHandler.GetIssueCountsByGroup(_db, command.ModuleId, cancellationToken);

        return Features.v1.Modules.ModuleDtoMapper.ToDto(module,
            issueCounts.totalIssues, issueCounts.completedIssues,
            issueCounts.cancelledIssues, issueCounts.startedIssues,
            issueCounts.unstartedIssues, issueCounts.backlogIssues);
    }
}
