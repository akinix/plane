using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.WorkItems.Contracts.DTOs;

namespace YH.Modules.WorkItems.Contracts.v1.Intake.UpdateIntakeIssue;

/// <summary>
/// Update an IntakeIssue status (Accept/Reject/Snooze/Duplicate).
/// <see cref="IntakeIssueId"/> and <see cref="ProjectId"/> are populated from the route by the endpoint.
/// </summary>
public sealed class UpdateIntakeIssueCommand : ICommand<IntakeIssueDto>
{
    /// <summary>Target status: -2 Pending, -1 Rejected, 0 Snoozed, 1 Accepted, 2 Duplicate (required).</summary>
    public int Status { get; set; }

    /// <summary>If Snoozed, the snooze expiration time (required for Snoozed).</summary>
    public DateTime? SnoozedTill { get; set; }

    /// <summary>If Duplicate, the original Issue id (required for Duplicate).</summary>
    public Guid? DuplicateToIssueId { get; set; }

    /// <summary>Route segment {intakeIssueId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid IntakeIssueId { get; set; }

    /// <summary>Route segment {projectId} — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
