using System.Text.Json.Serialization;
using Mediator;
using YH.Framework.Shared.Persistence;
using YH.Modules.Project.Contracts.DTOs;

namespace YH.Modules.Project.Contracts.v1.Projects.ListProjects;

/// <summary>
/// List projects in a workspace (REQ-3.1), with Plane-compatible paginated response.
/// Supports network filter (Secret vs Public) and optional sorting.
/// </summary>
/// <remarks>
/// <see cref="WorkspaceSlug"/> and <see cref="CurrentUserId"/> are populated by the endpoint from
/// the route and the authenticated principal respectively.
/// </remarks>
public sealed class ListProjectsQuery : IQuery<PlanePagedResult<ProjectDto>>, IPagedQuery
{
    /// <summary>Route segment <c>{slug}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public string WorkspaceSlug { get; set; } = default!;

    /// <summary>Authenticated user id — set by the endpoint for membership annotation.</summary>
    [JsonIgnore]
    public string CurrentUserId { get; set; } = default!;

    /// <inheritdoc />
    public int? PageNumber { get; set; }

    /// <inheritdoc />
    public int? PageSize { get; set; }

    /// <summary>Base URL used to build next/previous pagination links in Plane format.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Optional filter: 0=Secret, 2=Public. When null, returns all visible projects.</summary>
    public int? Network { get; set; }

    /// <summary>Optional ordering expression (e.g. "name", "-created_at"). Default: created_at desc.</summary>
    public string? OrderBy { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    string? IPagedQuery.Sort { get => OrderBy; set => OrderBy = value; }
}
