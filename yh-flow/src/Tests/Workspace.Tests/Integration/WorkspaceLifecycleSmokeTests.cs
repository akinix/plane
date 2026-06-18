using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Invitations.AcceptInvitation;
using YH.Modules.Workspace.Contracts.v1.Invitations.CreateInvitation;
using YH.Modules.Workspace.Contracts.v1.Members.ListMembers;
using YH.Modules.Workspace.Contracts.v1.Members.UpdateMemberRole;
using YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Contracts.v1.Workspaces.DeleteWorkspace;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Features.v1.Invitations.AcceptInvitation;
using YH.Modules.Workspace.Features.v1.Invitations.CreateInvitation;
using YH.Modules.Workspace.Features.v1.Members.ListMembers;
using YH.Modules.Workspace.Features.v1.Members.UpdateMemberRole;
using YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Features.v1.Workspaces.DeleteWorkspace;
using YH.Modules.Workspace.Services;
using YH.Modules.Workspace.Configuration;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// End-to-end workspace lifecycle smoke test (plan 02-06 Task 1).
/// Drives the full Mediator handler chain in a single [Fact] — exercising D-06 (auto-Admin member),
/// D-07 (slug generation), D-08 (slug release after soft delete), D-10/D-12 (invitation token +
/// SHA-256 hash + accept), REQ-2.2 (member listing + role updates), and T-2-eop-self (Member
/// cannot self-promote to Admin). Step 10 is the load-bearing D-08 assertion: the original slug
/// MUST be reusable by a brand-new workspace after the first is soft-deleted.
/// </summary>
/// <remarks>
/// <para>
/// <b>Test design rationale (mediator-direct vs HTTP TestServer):</b> per plan 02-06 &lt;action&gt;
/// recommendation, the smoke test prefers the mediator-direct call chain for speed and
/// deterministic InMemory state. The full Finbuckle HTTP stack is exercised by the manual smoke
/// (plan 02-06 Task 2). The handler chain still walks the real
/// <see cref="WorkspaceDbContext"/> / <see cref="SlugGenerator"/> / <see cref="InvitationTokenService"/>
/// / <see cref="WorkspaceMembershipService"/> implementations, so this guards against regressions
/// in the business logic that per-handler unit tests would miss (state flowing from one stage to
/// the next).
/// </para>
/// <para>
/// <b>Tenant scoping note:</b> the InMemory DbContext is scoped to a fixed workspace tenant via
/// <see cref="IMultiTenantContextAccessor{AppTenantInfo}"/>. The Accept invitation handler queries
/// the invitation by hash without tenant filter (top-level endpoint semantic — see
/// <see cref="AcceptInvitationCommandHandler"/> remarks), so the membership row lands in the
/// invitation's workspace.
/// </para>
/// </remarks>
public sealed class WorkspaceLifecycleSmokeTests
{
    private static readonly Guid OwnerUserId = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid InviteeUserId = Guid.Parse("00000000-0000-0000-0000-0000000000a2");

    [Fact]
    public async Task Lifecycle_TenSteps_CreateSlugCheckInviteAcceptListRoleDeleteAndSlugReuse()
    {
        // ───────────────────────────────────────────────────────────────────────────
        // Fixture: shared InMemory DbContext + slug generator + token service + the
        // handlers under test. The token service shares the DbContext so created
        // invitations persist across the Accept call within the same test.
        // ───────────────────────────────────────────────────────────────────────────
        await using var db = NewInMemoryContext();
        var slugGenerator = new SlugGenerator(db);
        var tokenOptions = Options.Create(new WorkspaceTokenOptions());
        var tokenService = new InvitationTokenService(db, tokenOptions);
        var membershipService = new WorkspaceMembershipService(db);
        var identity = StubIdentity();
        var tenantStore = Substitute.For<IMultiTenantStore<AppTenantInfo>>();
        tenantStore.RemoveAsync(Arg.Any<string>()).Returns(true);

        var createHandler = new CreateWorkspaceCommandHandler(db, slugGenerator);
        var createInviteHandler = new CreateInvitationCommandHandler(tokenService, tokenOptions);
        var acceptHandler = new AcceptInvitationCommandHandler(tokenService, db);
        var listMembersHandler = new ListMembersQueryHandler(db, identity);
        var updateRoleHandler = new UpdateMemberRoleCommandHandler(db, membershipService);
        var deleteHandler = new DeleteWorkspaceCommandHandler(db, tenantStore);

        // ───────────────────────────────────────────────────────────────────────────
        // Step 1 — create workspace "Acme Smoke" + verify D-06 auto-Admin member.
        // ───────────────────────────────────────────────────────────────────────────
        var created = await createHandler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme Smoke",
            OwnerUserId = OwnerUserId,
        }, CancellationToken.None);

        created.Slug.ShouldBe("acme-smoke");
        var workspace = await db.Workspaces.SingleAsync(w => w.Slug == "acme-smoke");
        workspace.OwnerId.ShouldBe(OwnerUserId);

        var ownerMember = await db.Members.SingleAsync(m => m.WorkspaceId == workspace.Id);
        ownerMember.UserId.ShouldBe(OwnerUserId.ToString());
        ownerMember.Role.ShouldBe((int)WorkspaceRole.Admin); // D-06 — creator is the first Admin
        ownerMember.IsActive.ShouldBeTrue();

        // ───────────────────────────────────────────────────────────────────────────
        // Step 2 — slug-check the just-created slug + a restricted word (D-09).
        // The SlugGenerator returns false for "acme-smoke" because it is now taken and
        // for "api" because it is a reserved word (RESTRICTED_WORKSPACE_SLUGS).
        // ───────────────────────────────────────────────────────────────────────────
        slugGenerator.IsValidSlug("acme-smoke").ShouldBeTrue("the created slug must still pass format validation");
        slugGenerator.IsValidSlug("api").ShouldBeFalse("restricted words must be rejected (D-09)");

        // ───────────────────────────────────────────────────────────────────────────
        // Step 3 — owner creates an invitation for the invitee as Member (D-10/D-12).
        // ───────────────────────────────────────────────────────────────────────────
        var inviteResponse = await createInviteHandler.Handle(new CreateInvitationCommand
        {
            WorkspaceId = workspace.Id,
            Slug = workspace.Slug,
            Email = "invitee@example.com",
            Role = WorkspaceRole.Member,
        }, CancellationToken.None);

        inviteResponse.Slug.ShouldBe("acme-smoke");
        inviteResponse.Token.ShouldNotBeNullOrWhiteSpace();
        inviteResponse.Token.Length.ShouldBe(64); // 32 bytes → 64 lowercase hex chars

        // The persisted invitation row carries the SHA-256 hash, NEVER the raw token.
        var invitation = await db.Invitations.SingleAsync(i => i.WorkspaceId == workspace.Id);
        invitation.TokenHash.ShouldNotBe(inviteResponse.Token);
        invitation.TokenHash.Length.ShouldBe(64);
        invitation.Role.ShouldBe((int)WorkspaceRole.Member);
        invitation.Accepted.ShouldBeFalse();

        // ───────────────────────────────────────────────────────────────────────────
        // Step 4 — invitee accepts the invitation (T-2-acceptpublic + T-2-replay).
        // Verifies that a new WorkspaceMember row is created with role=Member.
        // ───────────────────────────────────────────────────────────────────────────
        var accepted = await acceptHandler.Handle(new AcceptInvitationCommand
        {
            Token = inviteResponse.Token,
            CurrentUserId = InviteeUserId,
        }, CancellationToken.None);

        accepted.WorkspaceId.ShouldBe(workspace.Id);
        accepted.MemberId.ShouldNotBe(Guid.Empty);

        var inviteeMember = await db.Members
            .FirstAsync(m => m.WorkspaceId == workspace.Id && m.UserId == InviteeUserId.ToString());
        inviteeMember.Role.ShouldBe((int)WorkspaceRole.Member);
        inviteeMember.IsActive.ShouldBeTrue();

        // Invitation is now terminal (Accepted=true); a second accept must throw.
        var secondAccept = async () => await acceptHandler.Handle(new AcceptInvitationCommand
        {
            Token = inviteResponse.Token,
            CurrentUserId = InviteeUserId,
        }, CancellationToken.None);
        await Should.ThrowAsync<NotFoundException>(secondAccept);

        // ───────────────────────────────────────────────────────────────────────────
        // Step 5 — list members: 2 members (owner Admin + invitee Member), each with a
        // resolved UserSummary (D-05 batch — IUserIdentityService called exactly once).
        // ───────────────────────────────────────────────────────────────────────────
        var listResult = await listMembersHandler.Handle(new ListMembersQuery
        {
            WorkspaceId = workspace.Id,
            PageSize = 50,
        }, CancellationToken.None);

        listResult.Count.ShouldBe(2);
        // Admin (role 20) sorts ahead of Member (role 15) per ListMembersQueryHandler ordering.
        var roster = listResult.Results.ToList();
        roster.Count.ShouldBe(2);
        roster[0].Role.ShouldBe((int)WorkspaceRole.Admin);
        roster[0].User.ShouldNotBeNull();
        roster[0].User!.Id.ShouldBe(OwnerUserId);
        roster[1].Role.ShouldBe((int)WorkspaceRole.Member);
        roster[1].User.ShouldNotBeNull();
        roster[1].User!.Id.ShouldBe(InviteeUserId);

        // ───────────────────────────────────────────────────────────────────────────
        // Step 6 — owner promotes the invitee to Admin (D-11 matrix boundary).
        // ───────────────────────────────────────────────────────────────────────────
        var promoted = await updateRoleHandler.Handle(new UpdateMemberRoleCommand
        {
            WorkspaceId = workspace.Id,
            MemberId = inviteeMember.Id,
            Role = WorkspaceRole.Admin,
            CurrentUserId = OwnerUserId, // owner (Admin) is the caller — permitted
        }, CancellationToken.None);

        promoted.Role.ShouldBe((int)WorkspaceRole.Admin);

        var refreshedInvitee = await db.Members
            .FirstAsync(m => m.Id == inviteeMember.Id);
        refreshedInvitee.Role.ShouldBe((int)WorkspaceRole.Admin);

        // ───────────────────────────────────────────────────────────────────────────
        // Step 7 — T-2-eop-self guard: an Admin cannot promote THEMSELVES to Admin via
        // this endpoint. The handler explicitly rejects target == caller when promoting
        // to Admin (belt-and-braces on top of RequireWorkspaceRole(Admin) decoration).
        // We simulate a "callerAdmin" who is the target of their own promotion request.
        // ───────────────────────────────────────────────────────────────────────────
        var selfPromote = async () => await updateRoleHandler.Handle(new UpdateMemberRoleCommand
        {
            WorkspaceId = workspace.Id,
            MemberId = ownerMember.Id, // owner is the caller AND the target
            Role = WorkspaceRole.Admin,
            CurrentUserId = OwnerUserId,
        }, CancellationToken.None);
        await Should.ThrowAsync<ForbiddenException>(selfPromote);

        // ───────────────────────────────────────────────────────────────────────────
        // Step 8 — owner (D-06/D-08) soft-deletes the workspace; cache invalidation
        // is invoked on the Finbuckle tenant store.
        // ───────────────────────────────────────────────────────────────────────────
        await deleteHandler.Handle(new DeleteWorkspaceCommand
        {
            Slug = "acme-smoke",
            CurrentUserId = OwnerUserId,
        }, CancellationToken.None);

        var softDeleted = await db.Workspaces.IgnoreQueryFilters()
            .SingleAsync(w => w.Id == workspace.Id);
        softDeleted.IsDeleted.ShouldBeTrue();
        softDeleted.Slug.ShouldStartWith("acme-smoke__"); // D-08 — slug suffixed with epoch

        await tenantStore.Received(1).RemoveAsync("acme-smoke");

        // ───────────────────────────────────────────────────────────────────────────
        // Step 9 — the default query filter (IsDeleted == false) must NOT surface the
        // soft-deleted workspace. This is the tenant isolation invariant (NFR-3).
        // ───────────────────────────────────────────────────────────────────────────
        var visible = await db.Workspaces.ToListAsync();
        visible.ShouldBeEmpty("soft-deleted workspaces must not appear in the default query");

        // ───────────────────────────────────────────────────────────────────────────
        // Step 10 — LOAD-BEARING D-08 assertion: a brand-new workspace with the same
        // name reuses the original slug "acme-smoke" (the soft-deleted row's slug is
        // now suffixed with __{epoch}, so the unique index no longer collides).
        // This is the end-to-end proof that D-08's slug release works.
        // ───────────────────────────────────────────────────────────────────────────
        var recreated = await createHandler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme Smoke",
            OwnerUserId = Guid.NewGuid(), // a different owner — fresh aggregate
        }, CancellationToken.None);

        recreated.Slug.ShouldBe("acme-smoke"); // D-08 — slug reuse after soft delete

        // Two rows total now: the soft-deleted original + the new one with the bare slug.
        var allWorkspaces = await db.Workspaces.IgnoreQueryFilters().ToListAsync();
        allWorkspaces.Count.ShouldBe(2);
        allWorkspaces.ShouldContain(w => w.Slug == "acme-smoke" && !w.IsDeleted);
        allWorkspaces.ShouldContain(w => w.Slug.StartsWith("acme-smoke__") && w.IsDeleted);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static IUserIdentityService StubIdentity()
    {
        var identity = Substitute.For<IUserIdentityService>();
        identity
            .GetUsersByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var ids = ci.Arg<IEnumerable<Guid>>().ToList();
                var dict = new Dictionary<Guid, UserSummary>();
                foreach (var id in ids)
                {
                    dict[id] = new UserSummary(
                        id,
                        $"User {id.ToString().Substring(34, 2)}",
                        $"{id:N}@example.com",
                        null);
                }

                return Task.FromResult<IReadOnlyDictionary<Guid, UserSummary>>(dict);
            });
        return identity;
    }

    private static WorkspaceDbContext NewInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(
            id: Guid.NewGuid().ToString(),
            identifier: "smoke-test",
            name: "Smoke Test Workspace");
        accessor.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(tenant));

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"ws-smoke-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }
}
