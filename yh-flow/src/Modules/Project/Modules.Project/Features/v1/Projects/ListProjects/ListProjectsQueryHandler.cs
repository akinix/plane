using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Shared.Persistence;
using YH.Modules.Project.Contracts.DTOs;
using YH.Modules.Project.Contracts.v1.Projects.ListProjects;
using YH.Modules.Project.Data;
using YH.Modules.Project.Domain;
using ProjectEntity = YH.Modules.Project.Domain.Project;

namespace YH.Modules.Project.Features.v1.Projects.ListProjects;

/// <summary>
/// Handles <see cref="ListProjectsQuery"/> — lists projects in a workspace with network visibility
/// filtering and Plane-compatible paginated response.
/// </summary>
/// <remarks>
/// <para><b>Network filter (CRITICAL — threat T-3-crud-01):</b> applies Plane's <c>Q(network=2) | Q(is_member=True)</c>
/// pattern. Non-members see only <see cref="ProjectNetwork.Public"/> projects; members see all.
/// Projects are annotated with <c>isMember</c> and <c>memberRole</c> for the list response.</para>
/// <para><b>Tenant isolation:</b> <see cref="ProjectEntity"/> implements <c>IHasTenant</c>, so
/// <see cref="ProjectDbContext"/> automatically scopes queries to the current workspace. No
/// explicit <c>IgnoreQueryFilters()</c> needed here (unlike the workspace list-mine endpoint,
/// which is cross-tenant).</para>
/// </remarks>
public sealed class ListProjectsQueryHandler : IQueryHandler<ListProjectsQuery, PlanePagedResult<ProjectDto>>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ProjectDbContext _db;

    public ListProjectsQueryHandler(ProjectDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PlanePagedResult<ProjectDto>> Handle(ListProjectsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pageNumber = query.PageNumber is null or < 1 ? 1 : query.PageNumber.Value;
        var pageSize = query.PageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize.Value,
        };

        // Base query: projects scoped to current workspace by tenant filter.
        // Network filter: members see all; non-members see only Public(2).
        var currentUserId = query.CurrentUserId;

        var filteredQuery = _db.Projects
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Where(p => p.Network == ProjectNetwork.Public
                || _db.Members.Any(m => m.ProjectId == p.Id && m.UserId == currentUserId && m.IsActive));

        // Optional network filter override.
        if (query.Network.HasValue)
        {
            var network = (ProjectNetwork)query.Network.Value;
            filteredQuery = filteredQuery.Where(p => p.Network == network);
        }

        // Count for pagination.
        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        // Apply ordering (default: created_at descending).
        var orderedQuery = ApplyOrdering(filteredQuery, query.OrderBy);

        // Fetch paged results with membership annotation.
        var items = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Network = (int)p.Network,
                Identifier = p.Identifier,
                Slug = p.Slug,
                OwnerId = p.OwnerId,
                ProjectLeadId = p.ProjectLeadId,
                DefaultAssigneeId = p.DefaultAssigneeId,
                Emoji = p.Emoji,
                IconProp = p.IconProp,
                CoverImageUrl = p.CoverImageUrl,
                LogoProps = p.LogoProps,
                TimeZone = p.TimeZone,
                ModuleViewEnabled = p.ModuleViewEnabled,
                CycleViewEnabled = p.CycleViewEnabled,
                IssueViewsViewEnabled = p.IssueViewsViewEnabled,
                PageViewEnabled = p.PageViewEnabled,
                IntakeViewEnabled = p.IntakeViewEnabled,
                GuestViewAllFeatures = p.GuestViewAllFeatures,
                IsTimeTrackingEnabled = p.IsTimeTrackingEnabled,
                IsIssueTypeEnabled = p.IsIssueTypeEnabled,
                ArchiveIn = p.ArchiveIn,
                CloseIn = p.CloseIn,
                ArchivedAt = p.ArchivedAt,
                SortOrder = p.SortOrder,
                IsMember = _db.Members.Any(m => m.ProjectId == p.Id && m.UserId == currentUserId && m.IsActive),
                MemberRole = _db.Members
                    .Where(m => m.ProjectId == p.Id && m.UserId == currentUserId && m.IsActive)
                    .Select(m => (int?)m.Role)
                    .FirstOrDefault(),
                CreatedAt = p.CreatedOnUtc,
                UpdatedAt = p.LastModifiedOnUtc,
                DeletedAt = p.DeletedOnUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var paged = new PagedResponse<ProjectDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
        };

        return PlanePagedResultFactory.FromPagedResponse(paged, query.BaseUrl ?? $"/api/v1/workspaces/{query.WorkspaceSlug}/projects/");
    }

    private static IQueryable<ProjectEntity> ApplyOrdering(IQueryable<ProjectEntity> query, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return query.OrderByDescending(p => p.CreatedOnUtc);
        }

        // Simple single-field ordering: "name" or "-name" for descending.
        var parts = orderBy.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].TrimStart('-');
        var descending = parts[0].StartsWith('-') || (parts.Length > 1 && parts[^1].Equals("desc", StringComparison.OrdinalIgnoreCase));

        #pragma warning disable CA1308 // OrderBy field comparison, not security-sensitive
        return (field.ToLowerInvariant(), descending) switch
        #pragma warning restore CA1308
        {
            ("name", false) => query.OrderBy(p => p.Name),
            ("name", true) => query.OrderByDescending(p => p.Name),
            ("created_at", false) => query.OrderBy(p => p.CreatedOnUtc),
            ("created_at", true) => query.OrderByDescending(p => p.CreatedOnUtc),
            ("updated_at", false) => query.OrderBy(p => p.LastModifiedOnUtc ?? p.CreatedOnUtc),
            ("updated_at", true) => query.OrderByDescending(p => p.LastModifiedOnUtc ?? p.CreatedOnUtc),
            ("sort_order", false) => query.OrderBy(p => p.SortOrder),
            ("sort_order", true) => query.OrderByDescending(p => p.SortOrder),
            _ => query.OrderByDescending(p => p.CreatedOnUtc),
        };
    }
}
