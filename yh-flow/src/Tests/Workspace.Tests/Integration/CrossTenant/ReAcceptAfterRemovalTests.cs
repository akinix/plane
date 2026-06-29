using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Workspace.Configuration;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Invitations.AcceptInvitation;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Features.v1.Invitations.AcceptInvitation;
using YH.Modules.Workspace.Services;
using YH.Tests.Workspace.Integration.PostgresFixtures;
// Namespace/type collision: the root namespace `YH.Tests.Workspace` and the entity
// `YH.Modules.Workspace.Domain.Workspace` share the "Workspace" identifier when the compiler
// tries to resolve `Workspace.Create`. Alias the entity so the factory call resolves cleanly
// (mirrors the pattern in CrossTenantListUserWorkspacesTests.cs:17).
using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;

namespace YH.Tests.Workspace.Integration.CrossTenant;

/// <summary>
/// Relational test for the CR-03 fix (plan 02-08 / Wave 7). Drives the full
/// invite → accept → admin-remove → re-invite → re-accept flow on real PostgreSQL and asserts the
/// second accept reuses the existing (deactivated) membership row instead of throwing a
/// <c>UniqueConstraintException</c> → 500.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this test must be relational (CR-03 single source of truth — <c>02-REVIEW.md §CR-03</c>):</b>
/// the InMemory EF Core provider does not enforce the composite unique index
/// <c>IX_WorkspaceMembers_Tenant_User</c> on <c>(TenantId, UserId)</c>. The deactivated row that
/// <c>WorkspaceMembershipService.RemoveAsync</c> leaves behind (IsActive=false but the row remains,
/// and IsActive is NOT part of the unique key) still occupies the uniqueness slot on real PG —
/// but InMemory happily accepts a duplicate insert without complaint. Only a real Postgres schema
/// can fail the test pre-fix (500 from <c>UniqueConstraintException</c>) and pass post-fix.
/// </para>
/// <para>
/// <b>Test isolation:</b> the single [Fact] method <c>TRUNCATE</c>s the Workspace tables (with
/// <c>RESTART IDENTITY CASCADE</c>) at the start so prior classes sharing the fixture do not leak.
/// </para>
/// </remarks>
public sealed class ReAcceptAfterRemovalTests : IClassFixture<WorkspacePostgresFixture>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000091");
    private static readonly Guid InviteeUserId = Guid.Parse("00000000-0000-0000-0000-000000000092");

    private readonly WorkspacePostgresFixture _fixture;

    public ReAcceptAfterRemovalTests(WorkspacePostgresFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    /// <summary>
    /// LOAD-BEARING CR-03 assertion: a user who was previously invited, accepted, and then removed
    /// (Deactivate) by an admin can be re-invited and re-accept without the second accept throwing
    /// a 500. The handler must detect the existing (deactivated) membership row and call
    /// <see cref="WorkspaceMember.Activate"/> + <see cref="WorkspaceMember.UpdateRole"/> on it
    /// instead of <c>_db.Members.Add(...)</c> a brand-new row (which would collide with the
    /// deactivated row's unique slot on <c>(TenantId, UserId)</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Flow (5 stages, mirrors <c>02-REVIEW.md §CR-03</c> "Cover with a test" prescription):</b>
    /// <list type="number">
    ///   <item><description>Seed workspaceA + create invitation (workspace-scoped) → rawToken1.</description></item>
    ///   <item><description>First accept under RootTenant scope (CR-01 cross-tenant path) → member1Id.</description></item>
    ///   <item><description>Admin removes the member via <see cref="WorkspaceMembershipService.RemoveAsync"/>
    ///   → the row is deactivated (IsActive=false) but NOT deleted.</description></item>
    ///   <item><description>Create a second invitation (re-invite) → rawToken2.</description></item>
    ///   <item><description>Second accept under RootTenant scope — the handler MUST find the existing
    ///   deactivated row via <c>IgnoreQueryFilters</c> and reuse it. The response.MemberId must equal
    ///   member1Id; the row’s IsActive must be true again. Pre-fix this would throw
    ///   <c>UniqueConstraintException</c> → 500.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Fact]
    [Trait("Category", "Postgres")]
    [Trait("Category", "RequiresDocker")]
    public async Task ReAccept_After_Admin_Remove_Reuses_Existing_Row_No_UniqueConstraint_500()
    {
        await TruncateAsync();

        var accessor = _fixture.Services.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
        var setter = _fixture.Services.GetRequiredService<IMultiTenantContextSetter>();

        // ── Stage 0 — seed workspaceA (IGlobalEntity, RootTenant scope suffices) ─────────────
        var workspaceA = WorkspaceEntity.Create(
            name: "Acme CR03",
            slug: "acme-cr03",
            ownerUserId: AdminUserId);

        await using (var seedCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant))
        {
            await seedCtx.Workspaces.AddAsync(workspaceA);
            await seedCtx.SaveChangesAsync();
        }

        var tenantA = new AppTenantInfo(
            id: workspaceA.Id.ToString(),
            identifier: workspaceA.Id.ToString(),
            name: workspaceA.Name);
        var tokenOptions = Options.Create(new WorkspaceTokenOptions());

        // ── Stage 1 — create the first invitation scoped to workspaceA ───────────────────────
        string rawToken1;
        using (new FinbuckleTestTenantScope(accessor, setter, tenantA))
        {
            await using var inviteCtx = _fixture.CreateContextForTenant(tenantA);
            var tokenService = new InvitationTokenService(inviteCtx, tokenOptions);
            var (_, raw) = await tokenService.CreateAsync(
                workspaceId: workspaceA.Id,
                email: "invitee-cr03@example.com",
                role: WorkspaceRole.Member,
                ttlDays: 7,
                message: null,
                cancellationToken: CancellationToken.None);
            rawToken1 = raw;
        }

        // ── Stage 2 — first accept (RootTenant scope, CR-01 cross-tenant path) ───────────────
        Guid member1Id;
        using (new FinbuckleTestTenantScope(accessor, setter, WorkspacePostgresFixture.RootTenant))
        {
            await using var acceptCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant);
            var tokenService = new InvitationTokenService(acceptCtx, tokenOptions);
            var acceptHandler = new AcceptInvitationCommandHandler(
                tokenService,
                acceptCtx,
                setter,
                accessor);

            var response1 = await acceptHandler.Handle(
                new AcceptInvitationCommand
                {
                    Token = rawToken1,
                    CurrentUserId = InviteeUserId,
                },
                CancellationToken.None);

            response1.WorkspaceId.ShouldBe(workspaceA.Id);
            response1.MemberId.ShouldNotBe(Guid.Empty);
            member1Id = response1.MemberId;
        }

        // ── Stage 3 — admin removes the member (Deactivate; row stays, IsActive=false) ───────
        using (new FinbuckleTestTenantScope(accessor, setter, tenantA))
        {
            await using var adminCtx = _fixture.CreateContextForTenant(tenantA);
            var membershipService = new WorkspaceMembershipService(adminCtx);
            await membershipService.RemoveAsync(member1Id, CancellationToken.None);
        }

        // Sanity check: the removal truly deactivated the row (not deleted it).
        await using (var verifyCtx = _fixture.CreateContextForTenant(tenantA))
        {
            var deactivated = await verifyCtx.Members
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstAsync(m => m.Id == member1Id);
            deactivated.IsActive.ShouldBeFalse("CR-03 setup: RemoveAsync must leave IsActive=false (not delete).");
        }

        // ── Stage 4 — create the second invitation (re-invite the same user) ─────────────────
        string rawToken2;
        using (new FinbuckleTestTenantScope(accessor, setter, tenantA))
        {
            await using var inviteCtx2 = _fixture.CreateContextForTenant(tenantA);
            var tokenService2 = new InvitationTokenService(inviteCtx2, tokenOptions);
            var (_, raw2) = await tokenService2.CreateAsync(
                workspaceId: workspaceA.Id,
                email: "invitee-cr03@example.com",
                role: WorkspaceRole.Admin,
                ttlDays: 7,
                message: null,
                cancellationToken: CancellationToken.None);
            rawToken2 = raw2;
        }

        // ── Stage 5 — second accept (CR-03 load-bearing assertion) ───────────────────────────
        // Pre-fix: this would throw UniqueConstraintException because the handler unconditionally
        // _db.Members.Add(WorkspaceMember.Create(...)) — colliding with the deactivated row’s
        // (TenantId, UserId) slot. Post-fix: the handler detects the existing row via
        // IgnoreQueryFilters and calls Activate() + UpdateRole() on it.
        AcceptInvitationResponse response2;
        using (new FinbuckleTestTenantScope(accessor, setter, WorkspacePostgresFixture.RootTenant))
        {
            await using var acceptCtx2 = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant);
            var tokenService2 = new InvitationTokenService(acceptCtx2, tokenOptions);
            var acceptHandler2 = new AcceptInvitationCommandHandler(
                tokenService2,
                acceptCtx2,
                setter,
                accessor);

            response2 = await acceptHandler2.Handle(
                new AcceptInvitationCommand
                {
                    Token = rawToken2,
                    CurrentUserId = InviteeUserId,
                },
                CancellationToken.None);
        }

        // ── Assertions ───────────────────────────────────────────────────────────────────────
        response2.WorkspaceId.ShouldBe(workspaceA.Id);
        // CR-03 load-bearing assertion: the second accept reuses the SAME member row — it does NOT
        // create a duplicate (which would have thrown UniqueConstraintException on the
        // (TenantId, UserId) index before reaching this assert).
        response2.MemberId.ShouldBe(member1Id, "CR-03: second accept must reuse the existing (deactivated) member row, not insert a new one.");

        // Read back via IgnoreQueryFilters (the DbContext scope has been restored to RootTenant by
        // the FinbuckleTestTenantScope dispose) and verify the row is active again + role updated.
        await using (var readCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant))
        {
            var reactivated = await readCtx.Members
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstAsync(m => m.Id == member1Id);

            reactivated.IsActive.ShouldBeTrue("CR-03: Activate() must flip IsActive back to true on the reused row.");
            reactivated.WorkspaceId.ShouldBe(workspaceA.Id);
            reactivated.UserId.ShouldBe(InviteeUserId.ToString());
            // The re-invite carried role=Admin; the handler’s UpdateRole(tracked.Role) must reflect it.
            reactivated.Role.ShouldBe((int)WorkspaceRole.Admin);
            reactivated.TenantId.ShouldBe(workspaceA.Id.ToString(), "CR-01 invariant preserved: TenantId shadow property stays the workspace id across re-accept.");

            // Exactly ONE row for this (workspaceId, userId) pair — no duplicate accumulated.
            var rowsForPair = await readCtx.Members
                .IgnoreQueryFilters()
                .AsNoTracking()
                .CountAsync(m => m.WorkspaceId == workspaceA.Id && m.UserId == InviteeUserId.ToString());
            rowsForPair.ShouldBe(1, "CR-03: no duplicate membership row should exist after re-accept.");
        }
    }

    /// <summary>
    /// Truncates the Workspace module tables between tests so the shared fixture does not
    /// accumulate cross-test state. Mirrors <c>CrossTenantListUserWorkspacesTests.TruncateAsync</c>.
    /// </summary>
    private async Task TruncateAsync()
    {
        await using var ctx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant);
        await ctx.Database.ExecuteSqlRawAsync(
            @"TRUNCATE TABLE ""yhschema.Workspace"".""WorkspaceMembers"",
                                   ""yhschema.Workspace"".""WorkspaceInvitations"",
                                   ""yhschema.Workspace"".""Workspaces""
                             RESTART IDENTITY CASCADE;");
    }
}
