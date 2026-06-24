using System.Text.Json.Serialization;
using Mediator;
using YH.Framework.Shared.Persistence;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Issues.ListIssues;

/// <summary>
/// List issues with multidimensional filtering and Plane-compatible pagination (REQ-4.3).
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// Filters: StateId, Priority, AssigneeId, LabelId, ParentId, IsDraft.
/// Pagination: PageNumber, PageSize (default 30, max 100).
/// Ordering: OrderBy (default "-created_at").
/// </summary>
public sealed class ListIssuesQuery : IQuery<PlanePagedResult<IssueDto>>
{
    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Page number (1-based, default 1).</summary>
    public int? PageNumber { get; set; }

    /// <summary>Page size (default 30, max 100).</summary>
    public int? PageSize { get; set; }

    /// <summary>Order by field (default "-created_at"). Supports - prefix for descending.</summary>
    public string? OrderBy { get; set; }

    /// <summary>Base URL for pagination links.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Filter by state id.</summary>
    public Guid? StateId { get; set; }

    /// <summary>Filter by priority (exact match).</summary>
    public string? Priority { get; set; }

    /// <summary>Filter by assignee id (checks IssueAssignees table).</summary>
    public string? AssigneeId { get; set; }

    /// <summary>Filter by label id (checks IssueLabels table).</summary>
    public Guid? LabelId { get; set; }

    /// <summary>Filter by parent id (null = root issues only).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Filter by draft flag.</summary>
    public bool? IsDraft { get; set; }
}
