using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.ArchiveModule;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Modules.UnarchiveModule;

/// <summary>
/// Handles <see cref="UnarchiveModuleCommand"/> — unarchives a module, restoring it to active status.
/// </summary>
public sealed class UnarchiveModuleCommandHandler : ICommandHandler<UnarchiveModuleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public UnarchiveModuleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(UnarchiveModuleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }
        if (command.ModuleId == Guid.Empty)
        {
            throw new CustomException("Module id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var module = await _db.Modules
            .FirstOrDefaultAsync(m => m.Id == command.ModuleId
                                   && m.ProjectId == command.ProjectId
                                   && !m.IsDeleted
                                   && m.ArchivedAt != null, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Archived module '{command.ModuleId}' was not found.");
        }

        module.Unarchive();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
