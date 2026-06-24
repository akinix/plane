using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Project.Contracts.DTOs;

namespace YH.Modules.Project.Contracts.v1.Projects.UpdateProject;

/// <summary>
/// Update mutable project display fields (REQ-3.1 / REQ-3.3). Slug and Identifier are NOT editable here.
/// All fields are optional; only provided fields are applied (PATCH semantics).
/// </summary>
public sealed class UpdateProjectCommand : ICommand<ProjectDto>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    // Mutable display fields

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? DescriptionText { get; set; }

    public string? DescriptionHtml { get; set; }

    /// <summary>Project visibility: 0=Secret, 2=Public.</summary>
    public int? Network { get; set; }

    public string? Emoji { get; set; }

    public string? IconProp { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? LogoProps { get; set; }

    public string? TimeZone { get; set; }

    public Guid? ProjectLeadId { get; set; }

    public Guid? DefaultAssigneeId { get; set; }

    // Feature toggles

    public bool? ModuleViewEnabled { get; set; }

    public bool? CycleViewEnabled { get; set; }

    public bool? IssueViewsViewEnabled { get; set; }

    public bool? PageViewEnabled { get; set; }

    public bool? IntakeViewEnabled { get; set; }

    public bool? GuestViewAllFeatures { get; set; }

    public bool? IsTimeTrackingEnabled { get; set; }

    public bool? IsIssueTypeEnabled { get; set; }

    // Archive / close settings

    public int? ArchiveIn { get; set; }

    public int? CloseIn { get; set; }
}
