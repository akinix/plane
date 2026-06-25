using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.v1.Views.CreateView;
using YH.Modules.View.Data;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views.CreateView;

/// <summary>
/// Handles <see cref="CreateViewCommand"/> — creates the view aggregate.
/// </summary>
public sealed class CreateViewCommandHandler : ICommandHandler<CreateViewCommand, CreateViewResponse>
{
    private readonly ViewDbContext _db;

    public CreateViewCommandHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateViewResponse> Handle(CreateViewCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.OwnedBy == Guid.Empty)
        {
            throw new ArgumentException("Owner user id is required.", nameof(command));
        }

        var view = ViewEntity.Create(
            name: command.Name,
            ownedBy: command.OwnedBy,
            projectId: command.ProjectId,
            description: command.Description,
            filters: command.Filters,
            displayFilters: command.DisplayFilters,
            displayProperties: command.DisplayProperties,
            richFilters: command.RichFilters,
            access: (Domain.ViewAccess)command.Access,
            sortOrder: command.SortOrder,
            logoProps: command.LogoProps);

        _db.Views.Add(view);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateViewResponse(view.Id);
    }
}
