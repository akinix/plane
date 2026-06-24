using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// IntakeIssue entity — a draft issue awaiting review in the Intake inbox
/// (Plane <c>models/intake.py</c> IntakeIssue).
/// </summary>
/// <remarks>
/// <b>Two-step creation (RESEARCH §Pattern 9):</b> The Create handler first creates an <see cref="Issue"/>
/// with <c>IsDraft=true</c>, saves it, then creates this <see cref="IntakeIssue"/> referencing the saved Issue.
/// <para>
/// <b>Status transitions (T-4-intake-02):</b> Only Pending → Accepted/Rejected/Snoozed/Duplicate.
/// Already-accepted/duplicate issues cannot transition again.
/// </para>
/// <para>
/// <b>NOT soft-deletable:</b> IntakeIssue records are audit records. They are not deleted.
/// </para>
/// </remarks>
public sealed class IntakeIssue : IHasTenant, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>FK to the associated Issue (created first as draft, then linked).</summary>
    public Guid IssueId { get; private set; }

    /// <summary>Project that owns this intake record.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Current review status (default Pending).</summary>
    public IntakeIssueStatus Status { get; private set; } = IntakeIssueStatus.Pending;

    /// <summary>If Snoozed, the time when the snooze expires.</summary>
    public DateTime? SnoozedTill { get; private set; }

    /// <summary>If Duplicate, the id of the original Issue.</summary>
    public Guid? DuplicateToIssueId { get; private set; }

    /// <summary>Source of the intake submission (e.g. "IN_APP", email, etc.).</summary>
    public string Source { get; private set; } = "IN_APP";

    // EF Navigation
    public Issue Issue { get; private set; } = default!;

    // IHasTenant
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private IntakeIssue() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="IntakeIssue"/> linked to an already-saved draft Issue.
    /// </summary>
    public static IntakeIssue Create(Guid issueId, Guid projectId, string source = "IN_APP")
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        return new IntakeIssue
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            ProjectId = projectId,
            Status = IntakeIssueStatus.Pending,
            Source = source ?? "IN_APP",
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates the intake status with transition validation (T-4-intake-02).
    /// Only Pending → {Accepted, Rejected, Snoozed, Duplicate} transitions are allowed.
    /// </summary>
    public void UpdateStatus(IntakeIssueStatus newStatus, DateTime? snoozedTill = null, Guid? duplicateToIssueId = null)
    {
        // Validate transition: only Pending can transition
        if (Status != IntakeIssueStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot transition from status '{Status}' to '{newStatus}'. Only Pending intake issues can change status.");
        }

        // Cannot transition Pending → Pending
        if (newStatus == IntakeIssueStatus.Pending)
        {
            throw new InvalidOperationException("Cannot transition to Pending. Intake issue is already Pending.");
        }

        Status = newStatus;

        if (newStatus == IntakeIssueStatus.Snoozed)
        {
            SnoozedTill = snoozedTill;
        }

        if (newStatus == IntakeIssueStatus.Duplicate)
        {
            DuplicateToIssueId = duplicateToIssueId;
        }

        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}
