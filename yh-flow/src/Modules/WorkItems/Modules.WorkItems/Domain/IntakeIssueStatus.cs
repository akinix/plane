namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// IntakeIssue status values (Plane <c>models/intake.py</c> IntakeIssueStatus).
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><b>-2 Pending:</b> Awaiting review (default).</item>
///   <item><b>-1 Rejected:</b> Declined by reviewer.</item>
///   <item><b>0 Snoozed:</b> Postponed until <see cref="IntakeIssue.SnoozedTill"/>.</item>
///   <item><b>1 Accepted:</b> Converted to active Issue.</item>
///   <item><b>2 Duplicate:</b> Marked as duplicate of another Issue.</item>
/// </list>
/// </remarks>
public enum IntakeIssueStatus
{
    Pending = -2,
    Rejected = -1,
    Snoozed = 0,
    Accepted = 1,
    Duplicate = 2,
}
