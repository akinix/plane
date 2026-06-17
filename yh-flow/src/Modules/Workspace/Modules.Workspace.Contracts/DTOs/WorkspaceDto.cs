namespace YH.Modules.Workspace.Contracts.DTOs;

/// <summary>
/// Workspace response DTO (REQ-2.1 / REQ-2.3).
/// Field set mirrors Plane <c>apps/api/plane/db/models/workspace.py:122-139</c> Workspace model:
/// name (max 80), logo (text URL), slug (max 48, unique), owner_id (scalar Guid, D-06 — no FK),
/// organization_size (max 20), timezone (default "UTC"), background_color (default "#000000").
/// Audit timestamps (created_at / updated_at / deleted_at) follow Plane conventions.
/// </summary>
public class WorkspaceDto
{
    public Guid Id { get; set; }

    /// <summary>Display name (Plane: max 80).</summary>
    public string Name { get; set; } = default!;

    /// <summary>URL-safe unique slug (Plane: max 48). Format per D-09: <c>[a-z0-9-]</c>.</summary>
    public string Slug { get; set; } = default!;

    /// <summary>Owner user id (scalar; no cross-module FK per D-06).</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Optional logo URL or asset reference (Plane: <c>logo</c> text field).</summary>
    public string? Logo { get; set; }

    /// <summary>Organization size bucket (Plane: max 20, e.g. "1-10", "11-50").</summary>
    public string? OrganizationSize { get; set; }

    /// <summary>IANA timezone (Plane: default "UTC").</summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>Background color hex (Plane: default random; we default to "#000000" per RESEARCH §Example 5).</summary>
    public string BackgroundColor { get; set; } = "#000000";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Soft-delete timestamp (Plane convention; null when active). Slug is suffixed with <c>__{epoch}</c> on delete (D-08).</summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
