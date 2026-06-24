using System.Text.RegularExpressions;
using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// Issue entity — the core work-item aggregate (Plane <c>models/issue.py</c>).
/// Implements <see cref="IHasDomainEvents"/> for activity logging, <see cref="IHasTenant"/> for
/// multi-tenant isolation, <see cref="ISoftDeletable"/> for soft deletes, and
/// <see cref="IAuditableEntity"/> for audit trails.
/// </summary>
/// <remarks>
/// <b>SequenceId:</b> Assigned by <c>IssueSequenceService</c> before save — never set by client.
/// Uses transaction-level locking (SERIALIZABLE) for atomic MAX(SequenceId)+1 per project.
/// <para>
/// <b>CompletedAt sync (Plane behavior):</b> When state transitions to Completed group,
/// CompletedAt is set to UtcNow. When transitioning away from Completed, CompletedAt is set to null.
/// </para>
/// </remarks>
public sealed class Issue : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    // Private navigation lists for M2M through entities (EF Core backing fields)
    private readonly List<IssueAssignee> _assignees = [];
    private readonly List<IssueLabel> _labels = [];

    public Guid Id { get; private set; }

    /// <summary>Issue title (max 255, required).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>HTML description (default "&lt;p&gt;&lt;/p&gt;"). Max 10000 chars.</summary>
    public string DescriptionHtml { get; private set; } = "<p></p>";

    /// <summary>JSON format description (optional).</summary>
    public string? DescriptionJson { get; private set; }

    /// <summary>Plain-text stripped from HTML (optional).</summary>
    public string? DescriptionStripped { get; private set; }

    /// <summary>
    /// Priority: "urgent" / "high" / "medium" / "low" / "none". Default "none".
    /// String type per CONTEXT decision (not enum).
    /// </summary>
    public string Priority { get; private set; } = "none";

    /// <summary>Project-scoped auto-increment integer. Assigned by IssueSequenceService.</summary>
    public int SequenceId { get; internal set; }

    /// <summary>Drag-and-drop sort order (default 65535.0 per Plane convention).</summary>
    public double SortOrder { get; private set; } = 65535.0;

    /// <summary>Project that owns this issue (scalar Guid, no cross-module FK).</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Parent issue id for 1-level hierarchy (self-referencing FK). Depth 1 per CONTEXT.</summary>
    public Guid? ParentId { get; private set; }

    /// <summary>Current state FK (references WorkItemsDbContext.States).</summary>
    public Guid? StateId { get; private set; }

    /// <summary>Estimate point FK (references EstimatePoint).</summary>
    public Guid? EstimatePointId { get; private set; }

    /// <summary>Start date (Plane "start_date").</summary>
    public DateOnly? StartDate { get; private set; }

    /// <summary>Target/due date (Plane "target_date").</summary>
    public DateOnly? TargetDate { get; private set; }

    /// <summary>Auto-set when state transitions to Completed group (Plane behavior).</summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>When the issue was archived.</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>Draft flag for Intake workflow. Default false.</summary>
    public bool IsDraft { get; private set; }

    // IReadOnlyCollection public accessors for EF navigation
    public IReadOnlyCollection<IssueAssignee> Assignees => _assignees.AsReadOnly();
    public IReadOnlyCollection<IssueLabel> Labels => _labels.AsReadOnly();

    // IHasTenant
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    // IHasDomainEvents
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Issue() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="Issue"/>.
    /// Validates name and priority. Sanitizes description HTML. Does NOT assign SequenceId
    /// (handled by <c>IssueSequenceService</c> before save).
    /// </summary>
    public static Issue Create(
        string name,
        Guid projectId,
        Guid? stateId = null,
        Guid? parentId = null,
        Guid? estimatePointId = null,
        string? descriptionHtml = null,
        string? descriptionJson = null,
        string priority = "none",
        DateOnly? startDate = null,
        DateOnly? targetDate = null,
        bool isDraft = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Issue name is required.", nameof(name));

        var sanitizedHtml = SanitizeHtml(descriptionHtml ?? "<p></p>");

        return new Issue
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectId = projectId,
            StateId = stateId,
            ParentId = parentId,
            EstimatePointId = estimatePointId,
            DescriptionHtml = sanitizedHtml,
            DescriptionJson = descriptionJson,
            DescriptionStripped = StripHtml(sanitizedHtml),
            Priority = ValidatePriority(priority),
            SortOrder = 65535.0,
            StartDate = startDate,
            TargetDate = targetDate,
            IsDraft = isDraft,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable issue fields with nullable param PATCH semantics.
    /// Captures changes into <see cref="FieldChange"/> list before applying, then emits
    /// <c>IssueUpdatedDomainEvent</c> for activity logging (Wave 3).
    /// </summary>
    public void UpdateDetails(
        string? name = null,
        string? priority = null,
        string? descriptionHtml = null,
        string? descriptionJson = null,
        DateOnly? startDate = null,
        DateOnly? targetDate = null,
        Guid? stateId = null,
        Guid? estimatePointId = null,
        bool? isDraft = null,
        double? sortOrder = null)
    {
        var changes = new List<FieldChange>();

        if (name is not null && name != Name)
        {
            changes.Add(new("name", Name, name));
            Name = name;
        }

        if (priority is not null)
        {
            var validated = ValidatePriority(priority);
            if (validated != Priority)
            {
                changes.Add(new("priority", Priority, validated));
                Priority = validated;
            }
        }

        if (descriptionHtml is not null)
        {
            var sanitized = SanitizeHtml(descriptionHtml);
            if (sanitized != DescriptionHtml)
            {
                changes.Add(new("description_html", DescriptionHtml, sanitized));
                DescriptionHtml = sanitized;
                DescriptionStripped = StripHtml(sanitized);
            }
        }

        if (descriptionJson is not null && descriptionJson != DescriptionJson)
        {
            changes.Add(new("description_json", DescriptionJson, descriptionJson));
            DescriptionJson = descriptionJson;
        }

        if (stateId.HasValue && stateId != StateId)
        {
            changes.Add(new("state_id", StateId?.ToString(), stateId.Value.ToString()));
            StateId = stateId;
            // CompletedAt sync is handled by UpdateState method (not here)
        }

        if (startDate.HasValue && startDate != StartDate)
        {
            changes.Add(new("start_date", StartDate?.ToString("O"), startDate.Value.ToString("O")));
            StartDate = startDate;
        }

        if (targetDate.HasValue && targetDate != TargetDate)
        {
            changes.Add(new("target_date", TargetDate?.ToString("O"), targetDate.Value.ToString("O")));
            TargetDate = targetDate;
        }

        if (estimatePointId.HasValue && estimatePointId != EstimatePointId)
        {
            changes.Add(new("estimate_point_id", EstimatePointId?.ToString(), estimatePointId.Value.ToString()));
            EstimatePointId = estimatePointId;
        }

        if (isDraft.HasValue && isDraft != IsDraft)
        {
            changes.Add(new("is_draft", IsDraft.ToString(), isDraft.Value.ToString()));
            IsDraft = isDraft.Value;
        }

        if (sortOrder.HasValue && Math.Abs(sortOrder.Value - SortOrder) > 0.001)
        {
            changes.Add(new("sort_order", SortOrder.ToString("F"), sortOrder.Value.ToString("F")));
            SortOrder = sortOrder.Value;
        }

        LastModifiedOnUtc = DateTimeOffset.UtcNow;

        if (changes.Count > 0)
        {
            // TODO(Wave 3): emit IssueUpdatedDomainEvent for activity logging
            // _domainEvents.Add(new IssueUpdatedDomainEvent(Id, changes, LastModifiedBy ?? "unknown"));
        }
    }

    /// <summary>
    /// Updates state with CompletedAt sync (Plane behavior).
    /// </summary>
    /// <param name="stateId">New state id.</param>
    /// <param name="isCompletedGroup">Whether the new state is in the Completed group.</param>
    /// <param name="isCancelledGroup">Whether the new state is in the Cancelled group.</param>
    public void UpdateState(Guid stateId, bool isCompletedGroup, bool isCancelledGroup)
    {
        StateId = stateId;

        // CompletedAt sync: set when entering completed group, clear when leaving
        if (isCompletedGroup || isCancelledGroup)
        {
            if (CompletedAt is null)
            {
                CompletedAt = DateTimeOffset.UtcNow;
            }
        }
        else if (CompletedAt is not null)
        {
            CompletedAt = null;
        }

        LastModifiedOnUtc = DateTimeOffset.UtcNow;

        // TODO(Wave 3): emit IssueUpdatedDomainEvent for activity logging
        // var stateChanges = new List<FieldChange> { new("state_id", oldStateId?.ToString(), stateId.ToString()) };
        // _domainEvents.Add(new IssueUpdatedDomainEvent(Id, stateChanges, LastModifiedBy ?? "unknown"));
    }

    /// <summary>
    /// Replaces assignee list (full replacement pattern per RESEARCH Pitfall 2).
    /// </summary>
    public void UpdateAssigneeList(ICollection<string> assigneeUserIds)
    {
        ArgumentNullException.ThrowIfNull(assigneeUserIds);

        _assignees.Clear();
        foreach (var userId in assigneeUserIds)
        {
            _assignees.Add(IssueAssignee.Create(Id, userId));
        }
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Replaces label list (full replacement pattern per RESEARCH Pitfall 2).
    /// </summary>
    public void UpdateLabelList(ICollection<Guid> labelIds)
    {
        ArgumentNullException.ThrowIfNull(labelIds);

        _labels.Clear();
        foreach (var labelId in labelIds)
        {
            _labels.Add(IssueLabel.Create(Id, labelId));
        }
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Accepts a draft issue (Intake flow) — sets IsDraft=false and assigns default state.
    /// </summary>
    public void MarkAsAccepted(Guid defaultStateId)
    {
        IsDraft = false;
        StateId = defaultStateId;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft-deletes this issue (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }

    /// <summary>
    /// Validates priority string against allowed values.
    /// </summary>
    private static string ValidatePriority(string p) => p switch
    {
        "urgent" or "high" or "medium" or "low" or "none" => p,
        _ => throw new ArgumentException($"Invalid priority: '{p}'. Must be one of: urgent, high, medium, low, none.")
    };

    /// <summary>
    /// Sanitizes HTML content by removing script tags and other dangerous elements.
    /// NOTE: Uses basic regex sanitization. Production should use HtmlSanitizer NuGet package.
    /// </summary>
    private static string SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return "<p></p>";

        // Basic sanitization: remove script tags, event handlers, and javascript: URLs
        var sanitized = Regex.Replace(html, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"\bon\w+\s*=\s*""[^""]*""", string.Empty, RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"\bon\w+\s*=\s*'[^']*'", string.Empty, RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"href\s*=\s*""javascript:[^""]*""", "href=\"\"", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"href\s*=\s*'javascript:[^']*'", "href=''", RegexOptions.IgnoreCase);

        return sanitized;
    }

    /// <summary>
    /// Strips HTML tags for the DescriptionStripped field.
    /// </summary>
    private static string? StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var stripped = Regex.Replace(html, @"<[^>]*>", string.Empty);
        stripped = Regex.Replace(stripped, @"\s+", " ").Trim();
        return string.IsNullOrWhiteSpace(stripped) ? null : stripped;
    }

    /// <summary>
    /// Records a single field change for domain event payload.
    /// </summary>
    /// <param name="Field">The field name that changed (Plane snake_case).</param>
    /// <param name="OldValue">The previous value (string representation).</param>
    /// <param name="NewValue">The new value (string representation).</param>
    public sealed record FieldChange(string Field, string? OldValue, string? NewValue);
}
