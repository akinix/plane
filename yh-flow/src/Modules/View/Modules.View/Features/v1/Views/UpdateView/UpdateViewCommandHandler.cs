using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.UpdateView;
using YH.Modules.View.Data;
using YH.Modules.View.Domain;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views.UpdateView;

/// <summary>
/// Handles <see cref="UpdateViewCommand"/> — applies PATCH updates to mutable view fields.
/// </summary>
public sealed class UpdateViewCommandHandler : ICommandHandler<UpdateViewCommand, ViewDetailDto>
{
    private readonly ViewDbContext _db;

    public UpdateViewCommandHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<ViewDetailDto> Handle(UpdateViewCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ViewId == Guid.Empty)
        {
            throw new CustomException("View id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var view = await _db.Views
            .FirstOrDefaultAsync(v => v.Id == command.ViewId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (view is null)
        {
            throw new NotFoundException($"View '{command.ViewId}' was not found.");
        }

        view.Update(
            name: command.Name,
            description: command.Description,
            filters: command.Filters,
            displayFilters: command.DisplayFilters,
            displayProperties: command.DisplayProperties,
            richFilters: command.RichFilters,
            access: command.Access.HasValue ? (ViewAccess)command.Access.Value : null,
            sortOrder: command.SortOrder,
            logoProps: command.LogoProps);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ViewDtoMapper.ToDetailDto(view);
    }
}
