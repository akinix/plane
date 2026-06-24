using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.States.CreateState;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.States.CreateState;

/// <summary>
/// Handles <see cref="CreateStateCommand"/> — creates a new project state.
/// </summary>
public sealed class CreateStateCommandHandler : ICommandHandler<CreateStateCommand, CreateStateResponse>
{
    private readonly WorkItemsDbContext _db;

    public CreateStateCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateStateResponse> Handle(CreateStateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Validate StateGroup is a known value (belt-and-braces after FluentValidation)
        var group = command.Group switch
        {
            0 => StateGroup.Backlog,
            1 => StateGroup.Unstarted,
            2 => StateGroup.Started,
            3 => StateGroup.Completed,
            4 => StateGroup.Cancelled,
            _ => throw new CustomException(
                $"Invalid state group: {command.Group}. Must be 0 (Backlog), 1 (Unstarted), 2 (Started), 3 (Completed), or 4 (Cancelled).",
                Array.Empty<string>(),
                HttpStatusCode.BadRequest)
        };

        var state = State.Create(
            name: command.Name,
            color: command.Color,
            group: group,
            projectId: command.ProjectId,
            isDefault: false,
            sortOrder: 65535.0);

        _db.States.Add(state);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateStateResponse(state.Id);
    }
}
