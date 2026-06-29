using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Issues.BulkUpdateIssues;

/// <summary>
/// Bulk-update multiple issues (state, assignees, priority) atomically.
/// Only non-null fields are applied — null fields are left unchanged.
/// </summary>
public sealed class BulkUpdateIssuesCommand : ICommand<BulkUpdateResultDto>
{
    /// <summary>Issue ids to update (required, at least 1).</summary>
    public List<Guid> IssueIds { get; set; } = default!;

    /// <summary>Target state id (null = unchanged).</summary>
    public Guid? StateId { get; set; }

    /// <summary>Replacement assignee ids (null = unchanged, empty list = clear).</summary>
    public List<string>? AssigneeIds { get; set; }

    /// <summary>Target priority (null = unchanged).</summary>
    public string? Priority { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}

/// <summary>
/// Bulk update result summary.
/// </summary>
public sealed class BulkUpdateResultDto
{
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];
}
