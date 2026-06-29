using System.Text.Json.Serialization;
using Mediator;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts.DTOs;

namespace YH.Modules.Workspace.Contracts.v1.Members.ListMembers;

/// <summary>
/// List members of the resolved workspace (REQ-2.2). Plane-compatible paginated response.
/// </summary>
/// <remarks>
/// <see cref="WorkspaceId"/> is populated by the endpoint from <c>ICurrentWorkspaceContext</c>
/// (set by <c>WorkspaceMembershipMiddleware</c> after slug resolution). Pagination params follow
/// <see cref="IPagedQuery"/>. <see cref="BaseUrl"/> drives the next/previous URL links.
/// </remarks>
public sealed class ListMembersQuery : IQuery<PlanePagedResult<WorkspaceMemberDto>>, IPagedQuery
{
    /// <summary>Resolved workspace id — set by the endpoint from ICurrentWorkspaceContext.</summary>
    [JsonIgnore]
    public Guid WorkspaceId { get; set; }

    /// <inheritdoc />
    public int? PageNumber { get; set; }

    /// <inheritdoc />
    public int? PageSize { get; set; }

    /// <inheritdoc />
    public string? Sort { get; set; }

    /// <summary>Base URL for next/previous pagination links.</summary>
    [JsonIgnore]
    public string BaseUrl { get; set; } = "/api/v1/workspaces/{slug}/members/";
}
