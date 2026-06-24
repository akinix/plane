using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.States.GetState;
using YH.Modules.WorkItems.Data;

namespace YH.Modules.WorkItems.Features.v1.States.GetState;

/// <summary>
/// Handles <see cref="GetStateQuery"/> — fetches a single state by id.
/// </summary>
public sealed class GetStateQueryHandler : IQueryHandler<GetStateQuery, StateDto>
{
    private readonly WorkItemsDbContext _db;

    public GetStateQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<StateDto> Handle(GetStateQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.StateId == Guid.Empty)
        {
            throw new CustomException("State id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var state = await _db.States
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.StateId && s.ProjectId == query.ProjectId && !s.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (state is null)
        {
            throw new NotFoundException($"State '{query.StateId}' was not found.");
        }

        return StateDtoMapper.ToDto(state);
    }
}
