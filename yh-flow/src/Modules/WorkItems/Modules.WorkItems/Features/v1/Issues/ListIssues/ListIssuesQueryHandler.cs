using Mediator;
using Microsoft.EntityFrameworkCore;
using YH.Framework.Shared.Persistence;
using YH.Modules.WorkItems.Contracts.DTOs;
using YH.Modules.WorkItems.Contracts.v1.Issues.ListIssues;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;

namespace YH.Modules.WorkItems.Features.v1.Issues.ListIssues;

/// <summary>
/// Handles <see cref="ListIssuesQuery"/> — paginated issue list with multidimensional filters.
/// Implements Plane-compatible pagination format (count/next/previous/results).
/// </summary>
/// <remarks>
/// <b>Base query:</b> <c>AsNoTracking</c> Issues where <c>!IsDeleted &amp;&amp; ProjectId</c>.
/// Drafts are hidden by default (<c>!IsDraft</c>) — shown via Intake.
/// <para>
/// <b>Filters:</b> StateId, Priority (exact), AssigneeId (.Any on IssueAssignees), LabelId (.Any on IssueLabels),
/// ParentId (null = root only), IsDraft.
/// </para>
/// <para>
/// <b>Ordering:</b> Supports created_at, updated_at, sort_order, priority, sequence_id with - prefix for desc.
/// </para>
/// <para>
/// <b>Pagination:</b> Default page size 30, max 100. Returns PlanePagedResult with count/next/previous/results.
/// </para>
/// </remarks>
public sealed class ListIssuesQueryHandler : IQueryHandler<ListIssuesQuery, PlanePagedResult<IssueDto>>
{
    private const int DefaultPageSize = 30;
    private const int MaxPageSize = 100;

    private readonly WorkItemsDbContext _db;

    public ListIssuesQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<PlanePagedResult<IssueDto>> Handle(ListIssuesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ProjectId == Guid.Empty)
        {
            return new PlanePagedResult<IssueDto>
            {
                Count = 0,
                Next = null,
                Previous = null,
                Results = [],
            };
        }

        var pageNumber = query.PageNumber is null or < 1 ? 1 : query.PageNumber.Value;
        var pageSize = query.PageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize.Value,
        };

        // Base query: non-deleted issues for this project
        IQueryable<Issue> filtered = _db.Issues
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.ProjectId == query.ProjectId);

        // Hide drafts by default (drafts shown via Intake)
        if (query.IsDraft is null)
        {
            filtered = filtered.Where(i => !i.IsDraft);
        }
        else
        {
            filtered = filtered.Where(i => i.IsDraft == query.IsDraft.Value);
        }

        // Multidimensional filters
        if (query.StateId.HasValue)
        {
            filtered = filtered.Where(i => i.StateId == query.StateId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            filtered = filtered.Where(i => i.Priority == query.Priority);
        }

        if (!string.IsNullOrWhiteSpace(query.AssigneeId))
        {
            filtered = filtered.Where(i => _db.IssueAssignees
                .Any(a => a.IssueId == i.Id && a.AssigneeId == query.AssigneeId));
        }

        if (query.LabelId.HasValue)
        {
            filtered = filtered.Where(i => _db.IssueLabels
                .Any(l => l.IssueId == i.Id && l.LabelId == query.LabelId.Value));
        }

        if (query.ParentId.HasValue)
        {
            filtered = filtered.Where(i => i.ParentId == query.ParentId.Value);
        }

        // Count for pagination
        var totalCount = await filtered.CountAsync(cancellationToken).ConfigureAwait(false);

        // Apply ordering
        var ordered = ApplyOrdering(filtered, query.OrderBy);

        // Fetch paged results
        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new IssueDto
            {
                Id = i.Id,
                Name = i.Name,
                DescriptionHtml = i.DescriptionHtml,
                DescriptionJson = i.DescriptionJson,
                DescriptionStripped = i.DescriptionStripped,
                Priority = i.Priority,
                SequenceId = i.SequenceId,
                SortOrder = i.SortOrder,
                ProjectId = i.ProjectId,
                ParentId = i.ParentId,
                StateId = i.StateId,
                EstimatePointId = i.EstimatePointId,
                StartDate = i.StartDate,
                TargetDate = i.TargetDate,
                CompletedAt = i.CompletedAt,
                ArchivedAt = i.ArchivedAt,
                IsDraft = i.IsDraft,
                CreatedAt = i.CreatedOnUtc,
                UpdatedAt = i.LastModifiedOnUtc,
                DeletedAt = i.DeletedOnUtc,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var paged = new PagedResponse<IssueDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
        };

        return PlanePagedResultFactory.FromPagedResponse(paged, query.BaseUrl ?? $"/api/v1/workspaces/{{slug}}/projects/{query.ProjectId}/work-items/");
    }

    private static IQueryable<Issue> ApplyOrdering(IQueryable<Issue> query, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return query.OrderByDescending(i => i.CreatedOnUtc);
        }

        var parts = orderBy.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].TrimStart('-');
        var descending = parts[0].StartsWith('-') || (parts.Length > 1 && parts[^1].Equals("desc", StringComparison.OrdinalIgnoreCase));

#pragma warning disable CA1308
        return (field.ToLowerInvariant(), descending) switch
#pragma warning restore CA1308
        {
            ("created_at", false) => query.OrderBy(i => i.CreatedOnUtc),
            ("created_at", true) => query.OrderByDescending(i => i.CreatedOnUtc),
            ("updated_at", false) => query.OrderBy(i => i.LastModifiedOnUtc ?? i.CreatedOnUtc),
            ("updated_at", true) => query.OrderByDescending(i => i.LastModifiedOnUtc ?? i.CreatedOnUtc),
            ("sort_order", false) => query.OrderBy(i => i.SortOrder),
            ("sort_order", true) => query.OrderByDescending(i => i.SortOrder),
            ("priority", false) => query.OrderBy(i => i.Priority),
            ("priority", true) => query.OrderByDescending(i => i.Priority),
            ("sequence_id", false) => query.OrderBy(i => i.SequenceId),
            ("sequence_id", true) => query.OrderByDescending(i => i.SequenceId),
            _ => query.OrderByDescending(i => i.CreatedOnUtc),
        };
    }
}
