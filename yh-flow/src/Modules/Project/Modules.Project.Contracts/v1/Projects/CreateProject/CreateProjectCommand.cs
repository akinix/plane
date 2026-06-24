using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Project.Contracts.v1.Projects.CreateProject;

/// <summary>
/// Create a project (REQ-3.1). The authenticated user becomes the owner and first Admin member.
/// </summary>
/// <remarks>
/// <see cref="Identifier"/> is required and must be uppercase alphanumeric (max 12).
/// <see cref="Slug"/> is optional — when omitted the handler generates a unique slug from <see cref="Name"/>.
/// <para><see cref="OwnerUserId"/> is populated by the endpoint from the authenticated user
/// (<c>[JsonIgnore]</c> so a caller cannot create a project on someone else's behalf).</para>
/// </remarks>
public sealed class CreateProjectCommand : ICommand<CreateProjectResponse>
{
    /// <summary>Display name (Plane: max 255, non-empty).</summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Short uppercase identifier prefix used in issue keys (e.g. "PROJ", "ENG").
    /// Max length 12, alphanumeric, uppercase only.
    /// </summary>
    public string Identifier { get; set; } = default!;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Optional plain-text description.</summary>
    public string? DescriptionText { get; set; }

    /// <summary>Optional HTML description.</summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>Project visibility: 0=Secret, 2=Public (default).</summary>
    public int Network { get; set; } = 2;

    /// <summary>Optional explicit slug; if null/whitespace, derived from <see cref="Name"/>.</summary>
    public string? Slug { get; set; }

    /// <summary>Optional emoji icon.</summary>
    public string? Emoji { get; set; }

    /// <summary>Optional JSON icon configuration.</summary>
    public string? IconProp { get; set; }

    /// <summary>Optional cover image URL.</summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>Optional JSON logo configuration.</summary>
    public string? LogoProps { get; set; }

    /// <summary>Optional IANA timezone (defaults to "UTC" when null).</summary>
    public string? TimeZone { get; set; }

    /// <summary>Optional project lead user id.</summary>
    public Guid? ProjectLeadId { get; set; }

    /// <summary>Optional default assignee user id.</summary>
    public Guid? DefaultAssigneeId { get; set; }

    // Feature toggles

    public bool ModuleViewEnabled { get; set; }

    public bool CycleViewEnabled { get; set; }

    public bool IssueViewsViewEnabled { get; set; }

    public bool PageViewEnabled { get; set; } = true;

    public bool IntakeViewEnabled { get; set; }

    public bool GuestViewAllFeatures { get; set; }

    public bool IsTimeTrackingEnabled { get; set; }

    public bool IsIssueTypeEnabled { get; set; }

    // Archive / close settings

    /// <summary>Days of inactivity before auto-archive (0 = disabled).</summary>
    public int ArchiveIn { get; set; }

    /// <summary>Days of inactivity before auto-close (0 = disabled).</summary>
    public int CloseIn { get; set; }

    /// <summary>Authenticated creator user id — set by the endpoint, NOT client-writable.</summary>
    [JsonIgnore]
    public Guid OwnerUserId { get; set; }
}
