using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Intake.ListIntakeIssues;

/// <summary>
/// List IntakeIssues for a project, optionally filtered by status.
/// Ordered by CreatedOnUtc descending.
/// </summary>
public sealed class ListIntakeIssuesQuery : IQuery<List<IntakeIssueDto>>
{
    /// <summary>Filter by status (-2 Pending, -1 Rejected, 0 Snoozed, 1 Accepted, 2 Duplicate). Optional.</summary>
    public int? Status { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
