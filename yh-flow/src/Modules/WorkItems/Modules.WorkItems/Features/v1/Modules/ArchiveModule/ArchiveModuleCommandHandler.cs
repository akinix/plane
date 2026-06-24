using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.ArchiveModule;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Modules.ArchiveModule;

/// <summary>
/// Handles <see cref="ArchiveModuleCommand"/> — archives a module.
/// Module has no date restrictions (unlike Cycle — any status can be archived).
/// </summary>
public sealed class ArchiveModuleCommandHandler : ICommandHandler<ArchiveModuleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public ArchiveModuleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(ArchiveModuleCommand command, CancellationToken cancellationToken)
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
                                   && m.ArchivedAt == null, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Module '{command.ModuleId}' was not found or is already archived.");
        }

        // No date restrictions — Module any status can be archived (unlike Cycle).
        module.Archive();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
