using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Workspace.Contracts.v1.Workspaces.DeleteWorkspace;

/// <summary>
/// Soft-delete a workspace (REQ-2.1, D-08). Only the owner may delete.
/// </summary>
/// <remarks>
/// <see cref="Slug"/> and <see cref="CurrentUserId"/> are populated by the endpoint from the route
/// and the authenticated principal respectively.
/// </remarks>
public sealed class DeleteWorkspaceCommand : ICommand
{
    /// <summary>Route segment <c>{slug}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public string Slug { get; set; } = default!;

    /// <summary>Authenticated user id — set by the endpoint for the owner check.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
