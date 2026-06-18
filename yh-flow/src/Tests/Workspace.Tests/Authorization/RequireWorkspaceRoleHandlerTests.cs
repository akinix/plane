using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using System.Security.Claims;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Tests.Workspace.Authorization;

/// <summary>
/// Unit tests for <see cref="RequireWorkspaceRoleAuthorizationHandler"/> (D-11, plan 02-03 Task 3,
/// threat T-2-eop [BLOCKING]).
/// </summary>
/// <remarks>
/// <para>
/// <b>What is under test:</b> the handler's default-deny behaviour given each combination of
/// the current user's workspace role and the required roles. Four cases drive the assertions:
/// </para>
/// <list type="bullet">
///   <item>Current=Admin + required=[Admin] → Succeed.</item>
///   <item>Current=Member + required=[Admin] → Fail (insufficient role).</item>
///   <item>Current=null (non-member) + required=[Member] → Fail (default-deny for non-members).</item>
///   <item>Current=Guest + required=[Member, Guest] → Succeed (Guest is in the allowed set).</item>
/// </list>
/// <para>
/// <b>Why this matters (T-2-eop):</b> the handler is the LAST line of defense against an
/// elevation-of-privilege attack — a buggy handler that succeeds a non-member or an
/// insufficient role would let any authenticated user invoke admin endpoints. Each test
/// exercises one code path in <see cref="RequireWorkspaceRoleAuthorizationHandler.HandleRequirementAsync"/>
/// so a regression surfaces immediately.
/// </para>
/// <para>
/// <b>NSubstitute stubs:</b> <see cref="ICurrentWorkspaceContext"/> is stubbed to return a fixed
/// role. No DB / no HTTP context — the handler is a pure in-memory check on the context.
/// </para>
/// </remarks>
public sealed class RequireWorkspaceRoleHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_AdminRoleWithAdminRequirement_Succeeds()
    {
        var handler = BuildHandler(WorkspaceRole.Admin);
        var context = BuildContext(new RequireWorkspaceRoleAttribute(WorkspaceRole.Admin));

        await handler.HandleAsync(context);

        context.HasSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_MemberRoleWithAdminRequirement_Fails()
    {
        var handler = BuildHandler(WorkspaceRole.Member);
        var context = BuildContext(new RequireWorkspaceRoleAttribute(WorkspaceRole.Admin));

        await handler.HandleAsync(context);

        context.HasFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_NullRoleWithMemberRequirement_Fails()
    {
        // Non-member (role=null) — the default-deny path. T-2-eop mitigation.
        var handler = BuildHandler(role: null);
        var context = BuildContext(new RequireWorkspaceRoleAttribute(WorkspaceRole.Member));

        await handler.HandleAsync(context);

        context.HasFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_GuestRoleWithMemberOrGuestRequirement_Succeeds()
    {
        var handler = BuildHandler(WorkspaceRole.Guest);
        var context = BuildContext(new RequireWorkspaceRoleAttribute(WorkspaceRole.Member, WorkspaceRole.Guest));

        await handler.HandleAsync(context);

        context.HasSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoneRoleWithAdminRequirement_Fails()
    {
        // None is the sentinel "no role" — even though it is non-null, it must be treated as
        // default-deny (CA1008 invariant: never stored on a WorkspaceMember row).
        var handler = BuildHandler(WorkspaceRole.None);
        var context = BuildContext(new RequireWorkspaceRoleAttribute(WorkspaceRole.Admin));

        await handler.HandleAsync(context);

        context.HasFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_EmptyRolesRequirement_Fails()
    {
        // Defensive: a requirement with NO permitted roles is a configuration error.
        // Default-deny so a misconfigured endpoint is safe rather than open.
        var handler = BuildHandler(WorkspaceRole.Admin);
        var context = BuildContext(new RequireWorkspaceRoleAttribute());

        await handler.HandleAsync(context);

        context.HasFailed.ShouldBeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static RequireWorkspaceRoleAuthorizationHandler BuildHandler(WorkspaceRole? role)
    {
        var workspaceContext = Substitute.For<ICurrentWorkspaceContext>();
        workspaceContext.CurrentUserRole.Returns(role);
        return new RequireWorkspaceRoleAuthorizationHandler(workspaceContext);
    }

    private static AuthorizationHandlerContext BuildContext(IAuthorizationRequirement requirement)
    {
        // The handler does not read the user / resource — it reads only ICurrentWorkspaceContext.
        // An empty ClaimsPrincipal + null resource is sufficient.
        var user = new ClaimsPrincipal(new ClaimsIdentity("test"));
        return new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);
    }
}
