using System.Text.Json.Serialization;
using Mediator;

namespace YH.Modules.Project.Contracts.v1.Members.UpdateMemberRole;

/// <summary>
/// Update a project member's role (REQ-3.2). Only project Admins or workspace Admins may change roles.
/// </summary>
/// <remarks>
/// <para><see cref="ProjectId"/> and <see cref="MemberId"/> are populated by the endpoint
/// from route parameters (<c>[JsonIgnore]</c> so the client cannot override them).</para>
/// </remarks>
public sealed class UpdateMemberRoleCommand : ICommand
{
    /// <summary>Route segment <c>{projectId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>Route segment <c>{memberId}</c> — set by the endpoint.</summary>
    [JsonIgnore]
    public Guid MemberId { get; set; }

    /// <summary>
    /// New role (WorkspaceRole values: 5=Guest, 15=Member, 20=Admin).
    /// Must be a valid WorkspaceRole value (not None).
    /// </summary>
    public int Role { get; set; }
}
