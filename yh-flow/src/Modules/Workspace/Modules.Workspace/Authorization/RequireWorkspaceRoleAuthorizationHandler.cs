using Microsoft.AspNetCore.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Workspace.Authorization;

/// <summary>
/// Authorization handler for <see cref="RequireWorkspaceRoleAttribute"/> (D-11, threat T-2-eop).
/// </summary>
/// <remarks>
/// <para>
/// <b>Key design difference vs Identity's <c>RequiredPermissionAuthorizationHandler</c>:</b>
/// Identity's handler calls <c>IUserService.HasPermissionAsync</c> (DB hit) because Phase 1
/// permissions are computed at request time. The Workspace handler reads pre-populated
/// <see cref="ICurrentWorkspaceContext.CurrentUserRole"/> — <c>WorkspaceMembershipMiddleware</c>
/// (plan 02-03 Task 2) already populated it by querying <c>WorkspaceDbContext.Members</c>. So
/// this handler is a pure in-memory check: zero DB hit per authz evaluation.
/// </para>
/// <para>
/// <b>Default-deny logic (T-2-eop mitigation):</b>
/// <list type="bullet">
///   <item><c>CurrentUserRole == null</c> (non-member, top-level endpoint, or anonymous)
///   → <c>context.Fail()</c>.</item>
///   <item><c>CurrentUserRole</c> is set but not in <see cref="RequireWorkspaceRoleAttribute.Roles"/>
///   → <c>context.Fail()</c>.</item>
///   <item><c>CurrentUserRole</c> is set and in <c>Roles</c> → <c>context.Succeed()</c>.</item>
/// </list>
/// Explicit <c>Fail()</c> (vs relying on no-<c>Succeed()</c>) is important: it prevents other
/// handlers in the pipeline from accidentally succeeding the requirement.
/// </para>
/// </remarks>
public sealed class RequireWorkspaceRoleAuthorizationHandler(ICurrentWorkspaceContext workspaceContext)
    : AuthorizationHandler<RequireWorkspaceRoleAttribute>
{
    private readonly ICurrentWorkspaceContext _workspaceContext = workspaceContext
        ?? throw new ArgumentNullException(nameof(workspaceContext));

    /// <summary>
    /// Evaluates the requirement against the current user's workspace role. Reads
    /// <see cref="ICurrentWorkspaceContext"/> — no DB hit.
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireWorkspaceRoleAttribute requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        // Default-deny (T-2-eop): a non-member has role=null. No match possible.
        var role = _workspaceContext.CurrentUserRole;
        if (role is null || role == WorkspaceRole.None)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (requirement.Roles.Length == 0)
        {
            // No permitted roles declared → default-deny (configuration guard).
            context.Fail();
            return Task.CompletedTask;
        }

        if (requirement.Roles.Contains(role.Value))
        {
            context.Succeed(requirement);
        }
        else
        {
            // Explicit Fail so no other handler can succeed the requirement by accident.
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
