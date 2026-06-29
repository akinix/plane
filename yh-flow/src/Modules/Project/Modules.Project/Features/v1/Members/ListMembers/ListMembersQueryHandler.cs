using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Shared.Persistence;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Members.ListMembers;
using YH.Modules.Project.Data;

namespace YH.Modules.Project.Features.v1.Members.ListMembers;

/// <summary>
/// Handles <see cref="ListMembersQuery"/> — two-phase batch lookup that avoids the N+1
/// user-resolution path (RESEARCH Pitfall 3, NFR-1).
/// </summary>
/// <remarks>
/// <para>
/// <b>Phase 1:</b> page the membership rows for the resolved project (single SQL —
/// <c>WHERE ProjectId == @id</c>). No Identity join — <c>ProjectMember.UserId</c> is a
/// scalar string with no FK (D-04 cross-module boundary).
/// </para>
/// <para>
/// <b>Phase 2:</b> collect the page's distinct user ids and call
/// <see cref="IUserIdentityService.GetUsersByIdsAsync"/> ONCE (single SQL batch in Identity —
/// D-05). The handler never enters a per-member resolution loop.
/// </para>
/// <para>
/// <b>Phase 3:</b> zip the two result sets into <see cref="ProjectMemberDto"/> items. Members whose
/// <see cref="UserSummary"/> is absent (deleted user) get a null <see cref="ProjectMemberDto.User"/>.
/// </para>
/// </remarks>
public sealed class ListMembersQueryHandler : IQueryHandler<ListMembersQuery, PlanePagedResult<ProjectMemberDto>>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ProjectDbContext _db;
    private readonly IUserIdentityService _userIdentity;

    public ListMembersQueryHandler(ProjectDbContext db, IUserIdentityService userIdentity)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _userIdentity = userIdentity ?? throw new ArgumentNullException(nameof(userIdentity));
    }

    public async ValueTask<PlanePagedResult<ProjectMemberDto>> Handle(ListMembersQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return new PlanePagedResult<ProjectMemberDto>();
        }

        var pageNumber = query.PageNumber is null or < 1 ? 1 : query.PageNumber.Value;
        var pageSize = query.PageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize.Value,
        };

        // Phase 1: page membership rows. AsNoTracking — pure read.
        var totalCount = await _db.Members
            .AsNoTracking()
            .CountAsync(m => m.ProjectId == query.ProjectId && !m.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (totalCount == 0)
        {
            return new PlanePagedResult<ProjectMemberDto>();
        }

        var members = await _db.Members
            .AsNoTracking()
            .Where(m => m.ProjectId == query.ProjectId && !m.IsDeleted)
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.CreatedOnUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (members.Count == 0)
        {
            return PlanePagedResultFactory.FromPagedResponse(
                new PagedResponse<ProjectMemberDto>
                {
                    Items = Array.Empty<ProjectMemberDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                },
                query.BaseUrl);
        }

        // Phase 2: batch-resolve users — SINGLE call (D-05). Distinct dedupes before crossing the
        // module boundary; Guid.Parse filters out any malformed rows.
        var userIds = members
            .Select(m => m.UserId)
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .Distinct()
            .ToList();

        var users = userIds.Count > 0
            ? await _userIdentity.GetUsersByIdsAsync(userIds, cancellationToken).ConfigureAwait(false)
            : new Dictionary<Guid, UserSummary>();

        // Phase 3: zip — null User when the id could not be resolved (user deleted in Identity).
        var items = members.Select(m =>
        {
            UserSummary? summary = null;
            if (Guid.TryParse(m.UserId, out var userId))
            {
                users.TryGetValue(userId, out summary);
            }

            return new ProjectMemberDto
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                UserId = Guid.TryParse(m.UserId, out var parsedId) ? parsedId : Guid.Empty,
                Role = m.Role,
                IsActive = m.IsActive,
                User = summary,
                CreatedAt = m.CreatedOnUtc,
                UpdatedAt = m.LastModifiedOnUtc,
            };
        }).ToList();

        var paged = new PagedResponse<ProjectMemberDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
        };

        return PlanePagedResultFactory.FromPagedResponse(paged, query.BaseUrl);
    }
}
