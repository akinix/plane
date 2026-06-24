using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Modules.Links;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Modules.Links.AddModuleLink;

/// <summary>
/// Handles <see cref="AddModuleLinkCommand"/> — adds an external resource link to a module.
/// </summary>
public sealed class AddModuleLinkCommandHandler : ICommandHandler<AddModuleLinkCommand, Unit>
{
    private readonly WorkItemsDbContext _db;

    public AddModuleLinkCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<Unit> Handle(AddModuleLinkCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Verify module exists
        var module = await _db.Modules
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == command.ModuleId && m.ProjectId == command.ProjectId && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            throw new NotFoundException($"Module '{command.ModuleId}' was not found.");
        }

        var link = ModuleLink.Create(command.Title, command.Url, command.Metadata, command.ModuleId);
        _db.Set<ModuleLink>().Add(link);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
