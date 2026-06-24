using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.Links;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Modules.Links.RemoveModuleLink;

/// <summary>
/// Handles <see cref="RemoveModuleLinkCommand"/> — removes a link from a module via soft-delete.
/// </summary>
public sealed class RemoveModuleLinkCommandHandler : ICommandHandler<RemoveModuleLinkCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public RemoveModuleLinkCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(RemoveModuleLinkCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var link = await _db.Set<ModuleLink>()
            .FirstOrDefaultAsync(l => l.Id == command.LinkId
                                   && l.ModuleId == command.ModuleId
                                   && !l.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (link is null)
        {
            throw new NotFoundException($"ModuleLink '{command.LinkId}' was not found on module '{command.ModuleId}'.");
        }

        link.SoftDelete(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
