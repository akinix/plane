using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Intake.CreateIntakeIssue;

/// <summary>
/// Create an IntakeIssue (draft Issue + IntakeIssue in two steps).
/// <see cref="ProjectId"/> is populated from the route by the endpoint.
/// </summary>
public sealed class CreateIntakeIssueCommand : ICommand<IntakeIssueDto>
{
    /// <summary>Issue name (required, max 255).</summary>
    public string Name { get; set; } = default!;

    /// <summary>HTML description (optional, sanitized).</summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>Priority: "urgent" / "high" / "medium" / "low" / "none" (default "none").</summary>
    public string Priority { get; set; } = "none";

    /// <summary>Source of the intake submission (default "IN_APP").</summary>
    public string Source { get; set; } = "IN_APP";

    /// <summary>Route segment {projectId} — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
