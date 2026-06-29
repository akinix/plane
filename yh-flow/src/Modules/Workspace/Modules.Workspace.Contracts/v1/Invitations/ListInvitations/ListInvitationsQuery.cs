using System.Text.Json.Serialization;
using Mediator;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts.DTOs;

namespace YH.Modules.Workspace.Contracts.v1.Invitations.ListInvitations;

/// <summary>
/// List invitations for the resolved workspace (REQ-2.4). Plane-compatible paginated response.
/// </summary>
public sealed class ListInvitationsQuery : IQuery<PlanePagedResult<WorkspaceInvitationDto>>, IPagedQuery
{
    /// <summary>Resolved workspace id — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid WorkspaceId { get; set; }

    /// <summary>Optional filter: when true, only pending (non-accepted) invitations are returned.</summary>
    public bool PendingOnly { get; set; }

    /// <inheritdoc />
    public int? PageNumber { get; set; }

    /// <inheritdoc />
    public int? PageSize { get; set; }

    /// <inheritdoc />
    public string? Sort { get; set; }

    /// <summary>Base URL for next/previous pagination links.</summary>
    [JsonIgnore]
    public string BaseUrl { get; set; } = "/api/v1/workspaces/{slug}/invitations/";
}
