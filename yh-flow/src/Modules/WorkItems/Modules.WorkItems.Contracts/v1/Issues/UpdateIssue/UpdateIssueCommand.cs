using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Issues.UpdateIssue;

/// <summary>
/// Update an issue (REQ-4.3).
/// <see cref="ProjectId"/> and <see cref="IssueId"/> are populated from the route by the endpoint.
/// Enforces closed-state semantics: Completed/Cancelled state groups reject updates.
/// M2M assignees/labels use full replacement pattern.
/// </summary>
public sealed class UpdateIssueCommand : ICommand<IssueDto>
{
    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment {issueId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IssueId { get; set; }

    /// <summary>Updated title (optional).</summary>
    public string? Name { get; set; }

    /// <summary>Updated priority (optional).</summary>
    public string? Priority { get; set; }

    /// <summary>Updated HTML description (optional, sanitized).</summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>Updated JSON description (optional).</summary>
    public string? DescriptionJson { get; set; }

    /// <summary>Updated state id (triggers CompletedAt sync).</summary>
    public Guid? StateId { get; set; }

    /// <summary>Updated estimate point id (optional).</summary>
    public Guid? EstimatePointId { get; set; }

    /// <summary>Updated start date (optional).</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>Updated target date (optional).</summary>
    public DateOnly? TargetDate { get; set; }

    /// <summary>Updated draft flag (optional).</summary>
    public bool? IsDraft { get; set; }

    /// <summary>Replacement assignee ids (optional, full replacement).</summary>
    public List<string>? AssigneeIds { get; set; }

    /// <summary>Replacement label ids (optional, full replacement).</summary>
    public List<Guid>? LabelIds { get; set; }
}
