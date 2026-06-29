using System.Text.Json.Serialization;
using Mediator;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Project.Contracts.v1.Members.AddMember;

/// <summary>
/// Add a user as a member to a project (REQ-3.2).
/// The caller must be a workspace Admin or project Admin.
/// </summary>
/// <remarks>
/// <para><see cref="ProjectId"/> is populated by the endpoint from the route parameter
/// (<c>[JsonIgnore]</c> so the client cannot override it).</para>
/// <para><see cref="CurrentUserId"/> is set by the endpoint from the authenticated principal
/// for authorization checks (Admin verification).</para>
/// <para><see cref="Role"/> defaults to <c>Member (15)</c> and must be one of
/// <see cref="WorkspaceRole"/> values (Guest=5, Member=15, Admin=20).</para>
/// </remarks>
public sealed class AddMemberCommand : ICommand<AddMemberResponse>
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>The user id to add to the project.</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Membership role (WorkspaceRole values: 5=Guest, 15=Member, 20=Admin).
    /// Defaults to <see cref="WorkspaceRole.Member"/> (15).
    /// </summary>
    public int Role { get; set; } = (int)WorkspaceRole.Member;

    /// <summary>Authenticated user id — set by the endpoint for the Admin authorization check.</summary>
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }
}
