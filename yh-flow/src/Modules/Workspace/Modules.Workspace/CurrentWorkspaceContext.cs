using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Workspace;

/// <summary>
/// Scoped implementation of <see cref="ICurrentWorkspaceContext"/> (CONTEXT D-03 / D-02).
/// </summary>
/// <remarks>
/// One instance is created per HTTP request scope. <c>WorkspaceMembershipMiddleware</c>
/// (plan 02-03) calls <see cref="SetContext"/> exactly once per request — after Finbuckle resolves
/// the tenant (slug → AppTenantInfo) and after authentication, by querying
/// <c>WorkspaceMember</c> for the current user in the resolved workspace. Downstream consumers
/// (handlers in plans 02-04 / 02-05, <c>RequireWorkspaceRoleAuthorizationHandler</c> in 02-03)
/// only READ the populated state; they never write it.
/// <para>
/// <b>Top-level endpoints (D-03):</b> when no <c>{slug}</c> route value is present (e.g.
/// <c>POST /api/v1/workspaces/</c>, <c>/api/v1/users/me/workspaces/invitations/</c>),
/// <c>WorkspaceMembershipMiddleware</c> leaves <see cref="CurrentWorkspaceId"/> null and the
/// handler treats the request as user-scoped rather than workspace-scoped. Downstream
/// <c>[RequireWorkspaceRole]</c> handlers must fail authorisation with a null role
/// (Plane semantics: non-member → 403).
/// </para>
/// <para>
/// <b>Thread safety:</b> scoped — one instance per request, so single-threaded by construction.
/// Concurrent reads from the same request are safe (the fields are set once before the first
/// handler reads them).
/// </para>
/// </remarks>
public sealed class CurrentWorkspaceContext : ICurrentWorkspaceContext
{
    private Guid? _currentWorkspaceId;
    private string? _slug;
    private WorkspaceRole? _currentUserRole;

    public Guid? CurrentWorkspaceId => _currentWorkspaceId;

    public string? Slug => _slug;

    public WorkspaceRole? CurrentUserRole => _currentUserRole;

    /// <summary>
    /// Populate the context. Called once per request by <c>WorkspaceMembershipMiddleware</c>
    /// (plan 02-03). <see cref="ICurrentWorkspaceContext.SetContext"/> docs the CA1716 rename
    /// rationale (<c>Set</c> collides with reserved-language keyword under TreatWarningsAsErrors).
    /// </summary>
    /// <param name="workspaceId">Resolved workspace id (from Finbuckle <c>TenantInfo.Id</c>).</param>
    /// <param name="slug">Resolved workspace slug (from Finbuckle <c>TenantInfo.Identifier</c>).</param>
    /// <param name="role">Membership role of the current user, or null if not an active member.</param>
    public void SetContext(Guid workspaceId, string slug, WorkspaceRole? role)
    {
        // Set unconditionally — the middleware is the single writer per scope; calling twice in the
        // same scope would indicate a pipeline bug (logged by the middleware, not asserted here).
        _currentWorkspaceId = workspaceId;
        _slug = slug;
        _currentUserRole = role;
    }
}
