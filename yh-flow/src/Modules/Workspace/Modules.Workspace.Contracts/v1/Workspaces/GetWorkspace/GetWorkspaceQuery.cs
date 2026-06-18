using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Workspace.Contracts.DTOs;

namespace YH.Modules.Workspace.Contracts.v1.Workspaces.GetWorkspace;

/// <summary>
/// Fetch a workspace by slug (REQ-2.1). <see cref="Slug"/> is populated from the route by the endpoint.
/// </summary>
public sealed class GetWorkspaceQuery : IQuery<WorkspaceDto>
{
    /// <summary>Route segment <c>{slug}</c> — set by the endpoint, not client-writable.</summary>
    [JsonIgnore]
    public string Slug { get; set; } = default!;
}
