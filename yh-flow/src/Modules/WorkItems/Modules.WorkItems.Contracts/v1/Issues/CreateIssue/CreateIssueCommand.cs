using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.WorkItems.Contracts.v1.Issues.CreateIssue;

/// <summary>
/// Create an issue (REQ-4.3).
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// SequenceId is auto-assigned by the handler via IssueSequenceService.
/// StateId is auto-assigned to project default if not provided.
/// Requires workspace Admin, Member, or Guest role per CONTEXT.
/// </summary>
public sealed class CreateIssueCommand : ICommand<CreateIssueResponse>
{
    /// <summary>Issue title (max 255, required).</summary>
    public string Name { get; set; } = default!;

    /// <summary>HTML description (optional, sanitized by handler).</summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>JSON format description (optional).</summary>
    public string? DescriptionJson { get; set; }

    /// <summary>Priority: "urgent" / "high" / "medium" / "low" / "none" (default "none").</summary>
    public string Priority { get; set; } = "none";

    /// <summary>Parent issue id (self-referencing FK, depth 1 per CONTEXT).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Target state id. If null, auto-assigned to project default.</summary>
    public Guid? StateId { get; set; }

    /// <summary>Estimate point id (FK to EstimatePoints).</summary>
    public Guid? EstimatePointId { get; set; }

    /// <summary>Start date.</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>Target/due date.</summary>
    public DateOnly? TargetDate { get; set; }

    /// <summary>Assignee user ids (M2M full replacement).</summary>
    public List<string>? AssigneeIds { get; set; }

    /// <summary>Label ids (M2M full replacement).</summary>
    public List<Guid>? LabelIds { get; set; }

    /// <summary>Draft flag for Intake workflow (default false).</summary>
    public bool IsDraft { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
