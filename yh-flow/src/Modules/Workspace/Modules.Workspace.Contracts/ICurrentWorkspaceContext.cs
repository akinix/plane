namespace YH.Modules.Workspace.Contracts;

/// <summary>
/// Per-request workspace context (CONTEXT D-03 / RESEARCH §ICurrentWorkspaceContext).
/// Implementation lives in <c>YH.Modules.Workspace</c> (not Contracts), registered Scoped.
/// <c>WorkspaceMembershipMiddleware</c> populates this after Finbuckle resolves the tenant and
/// after authentication, by querying <c>WorkspaceMember</c> for the current user in the resolved
/// workspace. Downstream modules (Phase 3 Project, etc.) depend on this contract — not on Finbuckle —
/// keeping the tenant-resolution library encapsulated inside the Workspace module.
/// </summary>
public interface ICurrentWorkspaceContext
{
    /// <summary>
    /// The currently resolved workspace id, or null when the request targets a top-level endpoint
    /// without a <c>{slug}</c> route segment (e.g. <c>POST /api/v1/workspaces/</c>).
    /// </summary>
    Guid? CurrentWorkspaceId { get; }

    /// <summary>The resolved workspace slug, or null on top-level endpoints.</summary>
    string? Slug { get; }

    /// <summary>
    /// The current user's role within the resolved workspace, or null when the user is not an
    /// active member (or no workspace was resolved). Null means downstream <c>[RequireWorkspaceRole]</c>
    /// handlers must fail the authorization requirement (Plane semantics: non-member → 403).
    /// </summary>
    WorkspaceRole? CurrentUserRole { get; }

    /// <summary>
    /// Populate the context. Called once per request by <c>WorkspaceMembershipMiddleware</c>.
    /// Renamed from <c>Set</c> to <c>SetContext</c> to avoid the reserved-language-keyword collision
    /// flagged by CA1716 (the FSH template runs with <c>TreatWarningsAsErrors</c>).
    /// </summary>
    /// <param name="workspaceId">Resolved workspace id (from Finbuckle <c>TenantInfo.Id</c>).</param>
    /// <param name="slug">Resolved workspace slug (from Finbuckle <c>TenantInfo.Identifier</c>).</param>
    /// <param name="role">Membership role of the current user, or null if not an active member.</param>
    void SetContext(Guid workspaceId, string slug, WorkspaceRole? role);
}
