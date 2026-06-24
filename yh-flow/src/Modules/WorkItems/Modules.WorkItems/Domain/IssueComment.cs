using System.Text.RegularExpressions;
using YH.Framework.Core.Domain;

namespace YH.Modules.WorkItems.Domain;

/// <summary>
/// IssueComment entity — a comment attached to an issue (Plane <c>models/issue.py</c> IssueComment).
/// Supports nested replies via <see cref="ParentId"/> (depth 1 per CONTEXT), HTML body with
/// sanitization, and edit tracking via <see cref="EditedAt"/>.
/// </summary>
/// <remarks>
/// <b>Tenant isolation:</b> <see cref="IHasTenant"/> provides multi-tenant filtering.
/// <b>Soft delete:</b> <see cref="ISoftDeletable"/> — comments are soft-deleted, not hard-removed.
/// </remarks>
public sealed class IssueComment : IHasTenant, ISoftDeletable, IAuditableEntity
{
    public Guid Id { get; private set; }

    /// <summary>FK to the parent Issue.</summary>
    public Guid IssueId { get; private set; }

    /// <summary>HTML body of the comment (max 10000 chars, required).</summary>
    public string CommentHtml { get; private set; } = default!;

    /// <summary>JSON format body (optional).</summary>
    public string? CommentJson { get; private set; }

    /// <summary>Plain-text stripped from HTML (optional).</summary>
    public string? CommentStripped { get; private set; }

    /// <summary>Actor who created the comment (scalar user id, no EF FK).</summary>
    public string ActorId { get; private set; } = default!;

    /// <summary>Parent comment id for nested replies (depth 1 per CONTEXT).</summary>
    public Guid? ParentId { get; private set; }

    /// <summary>Set when the comment is edited (null = never edited).</summary>
    public DateTimeOffset? EditedAt { get; private set; }

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

    private IssueComment() { } // EF Core

    /// <summary>
    /// Factory — creates a new <see cref="IssueComment"/>.
    /// Sanitizes comment HTML and strips tags for the plain-text version.
    /// </summary>
    public static IssueComment Create(
        Guid issueId,
        string commentHtml,
        string actorId,
        Guid? parentId = null,
        string? commentJson = null)
    {
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue id is required.", nameof(issueId));
        if (string.IsNullOrWhiteSpace(commentHtml))
            throw new ArgumentException("Comment HTML is required.", nameof(commentHtml));
        if (string.IsNullOrWhiteSpace(actorId))
            throw new ArgumentException("Actor id is required.", nameof(actorId));

        var sanitizedHtml = SanitizeHtml(commentHtml);

        return new IssueComment
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            CommentHtml = sanitizedHtml,
            CommentJson = commentJson,
            CommentStripped = StripHtml(sanitizedHtml),
            ActorId = actorId,
            ParentId = parentId,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates the comment body. Sets <see cref="EditedAt"/> to current UTC time.
    /// </summary>
    public void Update(string commentHtml, string? commentJson = null)
    {
        if (string.IsNullOrWhiteSpace(commentHtml))
            throw new ArgumentException("Comment HTML is required.", nameof(commentHtml));

        var sanitizedHtml = SanitizeHtml(commentHtml);
        CommentHtml = sanitizedHtml;
        CommentJson = commentJson;
        CommentStripped = StripHtml(sanitizedHtml);
        EditedAt = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft-deletes this comment (idempotent).</summary>
    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }

    /// <summary>
    /// Sanitizes HTML content by removing script tags and other dangerous elements.
    /// </summary>
    private static string SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var sanitized = Regex.Replace(html, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"\bon\w+\s*=\s*""[^""]*""", string.Empty, RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"\bon\w+\s*=\s*'[^']*'", string.Empty, RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"href\s*=\s*""javascript:[^""]*""", "href=\"\"", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"href\s*=\s*'javascript:[^']*'", "href=''", RegexOptions.IgnoreCase);

        return sanitized;
    }

    /// <summary>
    /// Strips HTML tags for the CommentStripped field.
    /// </summary>
    private static string? StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var stripped = Regex.Replace(html, @"<[^>]*>", string.Empty);
        stripped = Regex.Replace(stripped, @"\s+", " ").Trim();
        return string.IsNullOrWhiteSpace(stripped) ? null : stripped;
    }
}
