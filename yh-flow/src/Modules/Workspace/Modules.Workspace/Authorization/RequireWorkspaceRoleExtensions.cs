using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using YH.Modules.Workspace.Contracts;

namespace YH.Modules.Workspace.Authorization;

/// <summary>
/// Route-builder extension for applying a <see cref="RequireWorkspaceRoleAttribute"/> requirement
/// to an endpoint (D-11, plan 02-04 / 02-05 endpoint decoration).
/// </summary>
/// <remarks>
/// <para>
/// Use on workspace-scoped routes (under <c>MapGroup("api/v{version:apiVersion}/workspaces/{slug}")</c>):
/// <code>
/// scoped.MapGet("/", ...).RequireWorkspaceRole(WorkspaceRole.Guest, WorkspaceRole.Member, WorkspaceRole.Admin);
/// scoped.MapDelete("/members/{memberId}", ...).RequireWorkspaceRole(WorkspaceRole.Admin);
/// </code>
/// </para>
/// <para>
/// The extension wraps <c>RouteHandlerBuilder.RequireAuthorization(Action&lt;AuthorizationPolicyBuilder&gt;)</c>;
/// the policy builder's <c>AddRequirements</c> registers the
/// <see cref="RequireWorkspaceRoleAttribute"/>, which
/// <see cref="RequireWorkspaceRoleAuthorizationHandler"/> evaluates. Top-level routes
/// (no <c>{slug}</c>) use plain <c>.RequireAuthorization()</c> instead — no workspace role applies.
/// </para>
/// </remarks>
public static class RequireWorkspaceRoleExtensions
{
    /// <summary>
    /// Adds a <see cref="RequireWorkspaceRoleAttribute"/> requirement to the route. Only users
    /// whose current workspace role is in <paramref name="roles"/> will be authorized.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="roles">
    /// Permitted workspace roles. Empty / null = default-deny (no role satisfies).
    /// </param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static RouteHandlerBuilder RequireWorkspaceRole(
        this RouteHandlerBuilder builder,
        params WorkspaceRole[] roles)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // RequireAuthorization(Action<AuthorizationPolicyBuilder>) builds an ad-hoc policy that
        // the ASP.NET Core authorization pipeline evaluates against the resolved handlers.
        // The RequireWorkspaceRoleAttribute is added as the requirement; the workspace handler
        // picks it up by generic-param dispatch on AuthorizationHandler<TRequirement>.
        return builder.RequireAuthorization(policy => policy.AddRequirements(new RequireWorkspaceRoleAttribute(roles)));
    }
}
