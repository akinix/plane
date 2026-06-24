using System.Text.Json.Serialization;
using Mediator;
using YH.Framework.Shared.Persistence;
using YH.Modules.Project.Contracts.DTOs;

namespace YH.Modules.Project.Contracts.v1.Members.ListMembers;

/// <summary>
/// List members of a project (REQ-3.2). Plane-compatible paginated response with batch-resolved user details.
/// </summary>
/// <remarks>
/// <para><see cref="ProjectId"/> is populated by the endpoint from the route parameter
/// (<c>[JsonIgnore]</c> so the client cannot override it).</para>
/// <para><see cref="BaseUrl"/> is set by the endpoint from <c>HttpRequest</c> to drive
/// the <c>next</c>/<c>previous</c> pagination links.</para>
/// </remarks>
public sealed class ListMembersQuery : IQuery<PlanePagedResult<ProjectMemberDto>>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Page number (1-based).</summary>
    public int? PageNumber { get; set; }

    /// <summary>Page size (default 20, max 100).</summary>
    public int? PageSize { get; set; }

    /// <summary>Base URL for next/previous pagination links.</summary>
    [JsonIgnore]
    public string BaseUrl { get; set; } = "/api/v1/workspaces/{slug}/projects/{projectId}/members/";
}
