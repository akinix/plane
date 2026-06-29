using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.States.UpdateState;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.States.UpdateState;

/// <summary>
/// Handles <see cref="UpdateStateCommand"/> — applies PATCH updates to mutable state display fields.
/// </summary>
public sealed class UpdateStateCommandHandler : ICommandHandler<UpdateStateCommand, StateDto>
{
    private readonly WorkItemsDbContext _db;

    public UpdateStateCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<StateDto> Handle(UpdateStateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.StateId == Guid.Empty)
        {
            throw new CustomException("State id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var state = await _db.States
            .FirstOrDefaultAsync(s => s.Id == command.StateId && s.ProjectId == command.ProjectId && !s.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (state is null)
        {
            throw new NotFoundException($"State '{command.StateId}' was not found.");
        }

        state.Update(
            name: command.Name,
            color: command.Color,
            sortOrder: command.SortOrder);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return StateDtoMapper.ToDto(state);
    }
}
