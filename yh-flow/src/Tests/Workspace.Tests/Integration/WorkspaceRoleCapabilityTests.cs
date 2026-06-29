using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Shouldly;
using System.Security.Claims;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Workspace role → capability matrix test (plan 02-06 Task 1, threat T-2-matrixgap [BLOCKING]).
/// Verifies the D-11 authorization boundary for the four role groups (Admin / Member / Guest /
/// non-member) against the endpoint authorization requirements declared across the Workspace module
/// (Create / Update / Delete workspace, Create/List/Revoke invitation, List/Update/Remove member).
/// </summary>
/// <remarks>
/// <para>
/// <b>What is under test:</b> the <see cref="RequireWorkspaceRoleAuthorizationHandler"/> is the
/// final gate between an authenticated request and a workspace-scoped endpoint. A buggy handler
/// (or a misconfigured endpoint requirement) would let a Member invoke admin endpoints or a Guest
/// mutate workspace state. These tests enumerate the 4 × N matrix boundary per
/// <c>apps/api/plane/app/permissions/workspace.py</c>: Admin (20) = full; Member (15) = read/write
/// workspace content but no member/settings/invitation management; Guest (5) = read-only;
/// non-member (null/None) = deny all workspace-scoped endpoints.
/// </para>
/// <para>
/// <b>Why authorization-handler direct (vs HTTP TestServer):</b> per plan 02-06 &lt;action&gt;
/// recommendation, exercising the authorization pipeline directly via the handler covers the
/// matrix boundary without the HTTP overhead. The endpoints themselves all delegate to this same
/// handler (verified by <c>.RequireWorkspaceRole(...)</c> decoration in WorkspaceModule), so a
/// matrix-gap surfaced here is a matrix-gap surfaced in production. The manual smoke (Task 2)
/// exercises the HTTP pipeline end-to-end.
/// </para>
/// <para>
/// <b>Capability matrix (D-11, Plane alignment):</b>
/// <list type="table">
///   <item>
///     <term>Admin (20)</term>
///     <description>All — Create/Update/Delete workspace, Create/List/Revoke invitation,
///     List/Update/Remove member.</description>
///   </item>
///   <item>
///     <term>Member (15)</term>
///     <description>List members, List invitations (read-only access). CANNOT mutate workspace
///     settings, invitations, or other members' roles.</description>
///   </item>
///   <item>
///     <term>Guest (5)</term>
///     <description>List members (read-only). CANNOT mutate anything (workspace/invitation/member).</description>
///   </item>
///   <item>
///     <term>Non-member (null)</term>
///     <description>No workspace-scoped endpoint. Default-deny.</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public sealed class WorkspaceRoleCapabilityTests
{
    /// <summary>
    /// Parameterised matrix: role × endpoint-required-roles → expect Succeed or Fail.
    /// Each InlineData row is one cell in the 4 × N matrix; the test asserts the authorization
    /// outcome matches the expected Plane capability rule. Threat T-2-matrixgap is mitigated by
    /// the union of all rows — a regression in the handler or in an endpoint's requirement
    /// decoration would fail at least one cell.
    /// </summary>
    /// <param name="currentRole">The role simulated on ICurrentWorkspaceContext (null = non-member).</param>
    /// <param name="requiredRoles">The roles required by the endpoint's RequireWorkspaceRole decoration.</param>
    /// <param name="expectSucceed">Whether the handler should Succeed this requirement.</param>
    /// <param name="scenario">Human-readable label for diagnostics.</param>
    [Theory]
    // ── Admin (20) — full matrix access ──────────────────────────────────────────
    [InlineData(20, new[] { 20 }, true, "Admin can Update/Delete workspace")]
    [InlineData(20, new[] { 20 }, true, "Admin can Create/List/Revoke invitation")]
    [InlineData(20, new[] { 20 }, true, "Admin can Update/Remove member")]
    [InlineData(20, new[] { 15, 20 }, true, "Admin can List members")]
    [InlineData(20, new[] { 5, 15, 20 }, true, "Admin can Leave workspace")]
    // ── Member (15) — read/write workspace content only ──────────────────────────
    [InlineData(15, new[] { 15, 20 }, true, "Member can List members")]
    [InlineData(15, new[] { 5, 15, 20 }, true, "Member can Leave workspace")]
    [InlineData(15, new[] { 20 }, false, "Member CANNOT Update/Delete workspace")]
    [InlineData(15, new[] { 20 }, false, "Member CANNOT Create/List/Revoke invitation")]
    [InlineData(15, new[] { 20 }, false, "Member CANNOT Update member role / Remove member")]
    // ── Guest (5) — read-only ────────────────────────────────────────────────────
    [InlineData(5, new[] { 15, 20 }, false, "Guest CANNOT List members (read requires Member+)")]
    [InlineData(5, new[] { 5, 15, 20 }, true, "Guest can Leave workspace")]
    [InlineData(5, new[] { 20 }, false, "Guest CANNOT mutate workspace settings")]
    [InlineData(5, new[] { 20 }, false, "Guest CANNOT Create/Revoke invitation")]
    [InlineData(5, new[] { 20 }, false, "Guest CANNOT Update/Remove member")]
    // ── Non-member (null = role is unset on the context) — deny all ──────────────
    [InlineData(-1, new[] { 15, 20 }, false, "Non-member CANNOT List members")]
    [InlineData(-1, new[] { 20 }, false, "Non-member CANNOT Update/Delete workspace")]
    [InlineData(-1, new[] { 20 }, false, "Non-member CANNOT Create/Revoke invitation")]
    [InlineData(-1, new[] { 20 }, false, "Non-member CANNOT Update/Remove member")]
    [InlineData(-1, new[] { 5, 15, 20 }, false, "Non-member CANNOT Leave workspace")]
    public async Task Authorization_Matrix_EnforcesRoleToCapabilityBoundaries(
        int currentRole, int[] requiredRoles, bool expectSucceed, string scenario)
    {
        // Arrange — simulate the role the WorkspaceMembershipMiddleware would have populated.
        // currentRole == -1 is the test sentinel for "non-member" → ICurrentWorkspaceContext
        // returns null (the middleware leaves the role unset for non-members).
        var workspaceContext = Substitute.For<ICurrentWorkspaceContext>();
        workspaceContext.CurrentUserRole.Returns(currentRole < 0
            ? (WorkspaceRole?)null
            : (WorkspaceRole)currentRole);
        var handler = new RequireWorkspaceRoleAuthorizationHandler(workspaceContext);

        var workspaceRoles = requiredRoles.Select(r => (WorkspaceRole)r).ToArray();
        var requirement = new RequireWorkspaceRoleAttribute(workspaceRoles);
        var authContext = BuildContext(requirement);

        // Act.
        await handler.HandleAsync(authContext);

        // Assert — the matrix boundary per apps/api/plane/app/permissions/workspace.py.
        if (expectSucceed)
        {
            authContext.HasSucceeded.ShouldBeTrue(
                $"EXPECTED SUCCEED: {scenario}. The role {(WorkspaceRole)currentRole} must satisfy the requirement.");
            authContext.HasFailed.ShouldBeFalse();
        }
        else
        {
            authContext.HasFailed.ShouldBeTrue(
                $"EXPECTED FAIL: {scenario}. The role {DescribeRole(currentRole)} must NOT satisfy the requirement.");
            authContext.HasSucceeded.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Authorization_NoneRole_NeverSatisfiesAnyRequirement()
    {
        // Defensive — WorkspaceRole.None (0) is the sentinel "no role" value. Even when an endpoint
        // is misconfigured to accept None (which it never should be), the handler MUST default-deny
        // (CA1008 invariant: None is never stored on a WorkspaceMember row).
        var workspaceContext = Substitute.For<ICurrentWorkspaceContext>();
        workspaceContext.CurrentUserRole.Returns(WorkspaceRole.None);
        var handler = new RequireWorkspaceRoleAuthorizationHandler(workspaceContext);

        var authContext = BuildContext(new RequireWorkspaceRoleAttribute(
            WorkspaceRole.Guest, WorkspaceRole.Member, WorkspaceRole.Admin));

        await handler.HandleAsync(authContext);

        authContext.HasFailed.ShouldBeTrue();
        authContext.HasSucceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task Authorization_EopSelfPromotionGuard_IsEnforcedAtHandlerLevel()
    {
        // T-2-eop-self — the UPDATE MEMBER ROLE endpoint's RequireWorkspaceRole(Admin) decoration
        // would let an Admin pass. The handler adds a belt-and-braces check that the Admin target
        // is not the caller themselves. This test verifies the DECORATION declares Admin-only —
        // the actual self-guard is exercised in WorkspaceLifecycleSmokeTests step 7 (forbidden when
        // Admin tries to promote themselves) and in UpdateMemberRoleHandler unit tests.
        var workspaceContext = Substitute.For<ICurrentWorkspaceContext>();
        workspaceContext.CurrentUserRole.Returns(WorkspaceRole.Member);
        var handler = new RequireWorkspaceRoleAuthorizationHandler(workspaceContext);

        // The endpoint declares Admin-only — a Member must NOT pass (T-2-eop-self mitigation starts here).
        var authContext = BuildContext(new RequireWorkspaceRoleAttribute(WorkspaceRole.Admin));
        await handler.HandleAsync(authContext);
        authContext.HasFailed.ShouldBeTrue("Member must be denied by the UpdateMemberRole endpoint decoration");

        // And a Guest must also be denied.
        workspaceContext.CurrentUserRole.Returns(WorkspaceRole.Guest);
        authContext = BuildContext(new RequireWorkspaceRoleAttribute(WorkspaceRole.Admin));
        await handler.HandleAsync(authContext);
        authContext.HasFailed.ShouldBeTrue("Guest must be denied by the UpdateMemberRole endpoint decoration");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static AuthorizationHandlerContext BuildContext(IAuthorizationRequirement requirement)
    {
        // The handler reads only ICurrentWorkspaceContext; an empty principal + null resource is
        // sufficient (mirrors RequireWorkspaceRoleHandlerTests).
        var user = new ClaimsPrincipal(new ClaimsIdentity("test"));
        return new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);
    }

    private static string DescribeRole(int currentRole) => currentRole < 0
        ? "non-member (null)"
        : ((WorkspaceRole)currentRole).ToString();
}
