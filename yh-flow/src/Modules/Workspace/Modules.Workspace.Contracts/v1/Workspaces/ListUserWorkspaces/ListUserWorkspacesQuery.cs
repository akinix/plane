using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using Mediator;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts.DTOs;

namespace YH.Modules.Workspace.Contracts.v1.Workspaces.ListUserWorkspaces;

/// <summary>
/// List workspaces the current user is a member of (REQ-2.1). Plane-compatible paginated response.
/// </summary>
/// <remarks>
/// <see cref="UserId"/> is populated by the endpoint from the authenticated user. Pagination params
/// follow <see cref="IPagedQuery"/>.
/// </remarks>
public sealed class ListUserWorkspacesQuery : IQuery<PlanePagedResult<WorkspaceDto>>, IPagedQuery
{
    /// <summary>Authenticated user id — set by the endpoint.</summary>
    [JsonIgnore]
    public string UserId { get; set; } = default!;

    /// <inheritdoc />
    public int? PageNumber { get; set; }

    /// <inheritdoc />
    public int? PageSize { get; set; }

    /// <inheritdoc />
    public string? Sort { get; set; }

    /// <summary>Base URL used to build next/previous pagination links in the Plane format.</summary>
    [JsonIgnore]
    public string BaseUrl { get; set; } = "/api/v1/users/me/workspaces/";
}
