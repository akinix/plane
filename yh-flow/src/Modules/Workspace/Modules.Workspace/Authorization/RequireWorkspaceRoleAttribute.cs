using Microsoft.AspNetCore.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Workspace.Authorization;

/// <summary>
/// Marker attribute + authorization requirement carrying the set of workspace roles permitted
/// to invoke the decorated endpoint (CONTEXT D-11 / threat T-2-eop).
/// </summary>
/// <remarks>
/// <para>
/// Apply to workspace-scoped endpoints via the <see cref="RequireWorkspaceRoleExtensions.RequireWorkspaceRole"/>
/// route-builder extension (e.g. <c>.RequireWorkspaceRole(WorkspaceRole.Admin)</c>). The ASP.NET
/// Core authorization pipeline routes the requirement to
/// <see cref="RequireWorkspaceRoleAuthorizationHandler"/>, which reads
/// <see cref="ICurrentWorkspaceContext.CurrentUserRole"/> (populated by
/// <c>WorkspaceMembershipMiddleware</c>, plan 02-03 Task 2) and SUCCEEDs only when the current
/// role is in <see cref="Roles"/>.
/// </para>
/// <para>
/// <b>Default-deny (T-2-eop mitigation):</b> a non-member (role == <see cref="WorkspaceRole.None"/>
/// or context-stored role == null) MUST NOT satisfy the requirement — the handler fails it
/// explicitly. <see cref="WorkspaceRole.None"/> (=0) is never persisted on a
/// <c>WorkspaceMember</c> row; it is the canonical "no role" sentinel that the middleware and
/// handler agree on.
/// </para>
/// <para>
/// <b>Coexistence with identity permission authorization:</b> this attribute / handler pair is
/// independent of the Phase 1 <c>[RequirePermission]</c> system. Both register as
/// <c>IAuthorizationHandler</c> via <c>TryAddEnumerable</c>; an endpoint with a
/// <c>[RequireWorkspaceRole]</c> requirement is evaluated by THIS handler, an endpoint with a
/// <c>[RequirePermission]</c> requirement by Identity's handler. No conflict.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireWorkspaceRoleAttribute : Attribute, IAuthorizationRequirement
{
    /// <summary>
    /// The roles permitted to satisfy this requirement. Empty array = no role satisfies
    /// (default-deny). The handler treats null current-role (non-member) as never satisfying.
    /// </summary>
    public WorkspaceRole[] Roles { get; }

    /// <summary>
    /// Constructs a requirement allowing any of the specified <paramref name="roles"/>.
    /// </summary>
    /// <param name="roles">
    /// Permitted roles. If omitted / empty, no role satisfies (used to mark endpoints that no
    /// workspace role can reach — typically a configuration error).
    /// </param>
    public RequireWorkspaceRoleAttribute(params WorkspaceRole[] roles)
    {
        // Defensive copy: callers may mutate their input array after the attribute is applied.
        Roles = roles ?? [];
    }
}
