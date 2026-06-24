using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.DeleteModule;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.Modules.DeleteModule;

/// <summary>
/// Handles <see cref="DeleteModuleCommand"/> — soft-deletes a module.
/// </summary>
public sealed class DeleteModuleCommandHandler : ICommandHandler<DeleteModuleCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public DeleteModuleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(DeleteModuleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ModuleId == Guid.Empty)
        {
            throw new CustomException("Module id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var module = await _db.Modules
            .FirstOrDefaultAsync(m => m.Id == command.ModuleId && m.ProjectId == command.ProjectId && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Module '{command.ModuleId}' was not found.");
        }

        module.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
