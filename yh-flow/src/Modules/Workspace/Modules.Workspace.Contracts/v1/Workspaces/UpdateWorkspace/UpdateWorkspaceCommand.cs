using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Workspace.Contracts.DTOs;

namespace YH.Modules.Workspace.Contracts.v1.Workspaces.UpdateWorkspace;

/// <summary>
/// Update mutable workspace display fields (REQ-2.3 settings). Slug is NOT editable here.
/// All fields are optional; only provided fields are applied (PATCH semantics).
/// </summary>
public sealed class UpdateWorkspaceCommand : IQuery<WorkspaceDto>
{
    public string? Name { get; set; }
    public string? Logo { get; set; }
    public string? Description { get; set; }
    public string? TimeZone { get; set; }
    public string? OrganizationSize { get; set; }
    public string? BackgroundColor { get; set; }

    /// <summary>Route segment <c>{slug}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public string Slug { get; set; } = default!;
}
