namespace YH.Modules.Project.Contracts.v1.Members.AddMember;

/// <summary>
/// Response for adding a member to a project.
/// </summary>
/// <param name="Id">The created ProjectMember id.</param>
/// <param name="ProjectId">The project id the member was added to.</param>
/// <param name="UserId">The user id of the added member.</param>
/// <param name="Role">The membership role (WorkspaceRole values: 5=Guest, 15=Member, 20=Admin).</param>
public sealed record AddMemberResponse(Guid Id, Guid ProjectId, Guid UserId, int Role);
