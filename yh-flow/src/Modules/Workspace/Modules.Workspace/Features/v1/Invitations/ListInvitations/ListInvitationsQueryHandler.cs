using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts.DTOs;
using YH.Modules.Workspace.Contracts.v1.Invitations.ListInvitations;
using YH.Modules.Workspace.Data;

namespace YH.Modules.Workspace.Features.v1.Invitations.ListInvitations;

/// <summary>
/// Handles <see cref="ListInvitationsQuery"/> — paginated list of invitations for the resolved
/// workspace (REQ-2.4). Plane-compatible paginated response. NEVER serialises the raw token —
/// the response DTO contains only persisted fields (threat T-2-tokenleak).
/// </summary>
public sealed class ListInvitationsQueryHandler : IQueryHandler<ListInvitationsQuery, PlanePagedResult<WorkspaceInvitationDto>>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly WorkspaceDbContext _db;

    public ListInvitationsQueryHandler(WorkspaceDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PlanePagedResult<WorkspaceInvitationDto>> Handle(
        ListInvitationsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.WorkspaceId == Guid.Empty)
        {
            return new PlanePagedResult<WorkspaceInvitationDto>();
        }

        var pageNumber = query.PageNumber is null or < 1 ? 1 : query.PageNumber.Value;
        var pageSize = query.PageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize.Value,
        };

        var baseQuery = _db.Invitations
            .AsNoTracking()
            .Where(i => i.WorkspaceId == query.WorkspaceId && !i.IsDeleted);

        if (query.PendingOnly)
        {
            baseQuery = baseQuery.Where(i => !i.Accepted && i.RespondedAt == null);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);
        if (totalCount == 0)
        {
            return new PlanePagedResult<WorkspaceInvitationDto>();
        }

        var invitations = await baseQuery
            .OrderByDescending(i => i.CreatedOnUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = invitations.Select(MapToDto).ToList();

        var paged = new PagedResponse<WorkspaceInvitationDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
        };

        return PlanePagedResultFactory.FromPagedResponse(paged, query.BaseUrl);
    }

    private static WorkspaceInvitationDto MapToDto(YH.Modules.Workspace.Domain.WorkspaceInvitation i)
    {
        Guid? createdBy = null;
        if (!string.IsNullOrWhiteSpace(i.CreatedBy) && Guid.TryParse(i.CreatedBy, out var c))
        {
            createdBy = c;
        }

        return new WorkspaceInvitationDto
        {
            Id = i.Id,
            Email = i.Email,
            Role = i.Role,
            Accepted = i.Accepted,
            RespondedAt = i.RespondedAt,
            Message = i.Message,
            CreatedAt = i.CreatedOnUtc,
            ExpiresAt = i.ExpiresAt,
            WorkspaceId = i.WorkspaceId,
            CreatedBy = createdBy,
        };
    }
}
