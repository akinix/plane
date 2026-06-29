using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Core.Exceptions;
using YH.Modules.View.Contracts.DTOs;
using YH.Modules.View.Contracts.v1.Views.GetView;
using YH.Modules.View.Data;
using ViewEntity = YH.Modules.View.Domain.View;

namespace YH.Modules.View.Features.v1.Views.GetView;

/// <summary>
/// Handles <see cref="GetViewQuery"/> — fetches a single view by id.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="ViewEntity"/> is an <c>IGlobalEntity</c> (no TenantId column),
/// so tenant isolation relies on the route slug context and <c>[RequireWorkspaceRole]</c>
/// authorization.
/// </remarks>
public sealed class GetViewQueryHandler : IQueryHandler<GetViewQuery, ViewDetailDto>
{
    private readonly ViewDbContext _db;

    public GetViewQueryHandler(ViewDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<ViewDetailDto> Handle(GetViewQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ViewId == Guid.Empty)
        {
            throw new CustomException("View id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var view = await _db.Views
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == query.ViewId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (view is null)
        {
            throw new NotFoundException($"View '{query.ViewId}' was not found.");
        }

        return ViewDtoMapper.ToDetailDto(view);
    }
}
