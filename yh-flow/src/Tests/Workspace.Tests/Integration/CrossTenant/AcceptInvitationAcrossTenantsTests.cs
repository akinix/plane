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
/// Relational test for the CR-01 fix (plan 02-08 / Wave 7). Seeds an invitation whose
/// <c>WorkspaceId</c> belongs to workspace tenant A, then invokes
/// <see cref="AcceptInvitationCommandHandler"/> while the DbContext is scoped to the platform
/// RootTenant (the production top-level endpoint semantic) and asserts the handler returns
/// <c>200</c> with the new membership row's <c>TenantId</c> shadow property stamped to
/// <c>workspaceA.Id</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this test must be relational (CR-01 single source of truth — <c>02-REVIEW.md §CR-01</c>):</b>
/// the InMemory EF Core provider skips both Finbuckle's <c>AdjustUniqueIndexes</c> pipeline and the
/// auto-applied <c>TenantId</c> query filter, so a pre-fix AcceptInvitation handler would happily
/// "find" the invitation on InMemory even without <c>IgnoreQueryFilters</c>. Only a real Postgres
/// schema (where the invitation row lives in tenant A and the DbContext is scoped to RootTenant)
/// fails the test pre-fix (NotFoundException → 404) and passes post-fix.
/// </para>
/// <para>
/// <b>BLOCKER 2 regression guard:</b> the handler now DI-injects <see cref="IMultiTenantContextSetter"/>
/// (NOT a cast on the accessor). If the cast had been used and the accessor implementation did not
/// implement the setter interface, the setter would be null and every Accept would throw
/// InvalidOperationException → 500. This test would catch that regression: the assert on
/// <c>TenantId == workspaceA.Id</c> only passes if the SaveChanges scope switch succeeded, which
/// requires a non-null setter.
/// </para>
/// <para>
/// <b>Test isolation:</b> each test method <c>TRUNCATE</c>s the Workspace tables (with
/// <c>RESTART IDENTITY CASCADE</c>) at the start so test classes sharing the fixture do not leak
/// rows. The fixture itself keeps the container + schema alive across tests in the same class.
/// </para>
/// </remarks>
public sealed class AcceptInvitationAcrossTenantsTests : IClassFixture<WorkspacePostgresFixture>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000081");
    private static readonly Guid InviteeUserId = Guid.Parse("00000000-0000-0000-0000-000000000082");

    private readonly WorkspacePostgresFixture _fixture;

    public AcceptInvitationAcrossTenantsTests(WorkspacePostgresFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    /// <summary>
    /// LOAD-BEARING CR-01 assertion: an invitee whose current DbContext scope = RootTenant (the
    /// top-level accept endpoint semantic — the invitation's workspace tenant is NOT resolved as
    /// the caller's current tenant) can still accept an invitation whose <c>WorkspaceId</c> belongs
    /// to a DIFFERENT workspace tenant. The handler must:
    /// <list type="bullet">
    ///   <item><description>Find the invitation via <c>IgnoreQueryFilters</c> (hash + id lookups are
    ///   tenant-agnostic).</description></item>
    ///   <item><description>Stamp the new <c>WorkspaceMember</c> row's <c>TenantId</c> shadow
    ///   property with the invitation's workspace id (via the try/finally scope switch on
    ///   <c>IMultiTenantContextSetter</c>).</description></item>
    /// </list>
    /// Pre-fix the handler would throw <see cref="YH.Framework.Core.Exceptions.NotFoundException"/>
    /// (404) because the tenant filter silently dropped the invitation row.
    /// </summary>
    [Fact]
    [Trait("Category", "Postgres")]
    [Trait("Category", "RequiresDocker")]
    public async Task Accept_Succeeds_When_Invitee_Current_Tenant_Differs_From_Invitation_Workspace()
    {
        await TruncateAsync();

        var accessor = _fixture.Services.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
        var setter = _fixture.Services.GetRequiredService<IMultiTenantContextSetter>();

        // ── Seed workspace A (IGlobalEntity — RootTenant scope suffices to insert) ─────────
        var workspaceA = WorkspaceEntity.Create(
            name: "Acme CR01",
            slug: "acme-cr01",
            ownerUserId: AdminUserId);

        await using (var seedCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant))
        {
            await seedCtx.Workspaces.AddAsync(workspaceA);
            await seedCtx.SaveChangesAsync();
        }

        // ── Create the invitation while scoped to workspaceA tenant (workspace-scoped endpoint) ─
        var tenantA = new AppTenantInfo(
            id: workspaceA.Id.ToString(),
            identifier: workspaceA.Id.ToString(),
            name: workspaceA.Name);
        string rawToken;
        using (new FinbuckleTestTenantScope(accessor, setter, tenantA))
        {
            await using var inviteCtx = _fixture.CreateContextForTenant(tenantA);
            var tokenOptions = Options.Create(new WorkspaceTokenOptions());
            var tokenService = new InvitationTokenService(inviteCtx, tokenOptions);
            var (_, raw) = await tokenService.CreateAsync(
                workspaceId: workspaceA.Id,
                email: "invitee-cr01@example.com",
                role: WorkspaceRole.Member,
                ttlDays: 7,
                message: null,
                cancellationToken: CancellationToken.None);
            rawToken = raw;
        }

        // ── Accept the invitation while scoped to RootTenant (top-level endpoint semantic) ────
        // This is the CRITICAL divergence: RootTenant.Id != workspaceA.Id, so without
        // IgnoreQueryFilters the ValidateAsync + re-attach queries would return null → 404.
        // BLOCKER 2 guard: we pass the SAME setter/accessor DI pair the production handler resolves.
        WorkspaceMember? readBack;
        AcceptInvitationResponse response;
        using (new FinbuckleTestTenantScope(accessor, setter, WorkspacePostgresFixture.RootTenant))
        {
            await using var acceptCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant);
            var tokenOptions = Options.Create(new WorkspaceTokenOptions());
            var tokenService = new InvitationTokenService(acceptCtx, tokenOptions);
            var acceptHandler = new AcceptInvitationCommandHandler(
                tokenService,
                acceptCtx,
                setter,
                accessor);

            response = await acceptHandler.Handle(
                new AcceptInvitationCommand
                {
                    Token = rawToken,
                    CurrentUserId = InviteeUserId,
                },
                CancellationToken.None);

            // Read back through the same RootTenant-scoped context but WITHOUT the tenant filter —
            // we need to see the row regardless of which tenant it landed in.
            readBack = await acceptCtx.Members
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceA.Id
                    && m.UserId == InviteeUserId.ToString());
        }

        // ── Assertions ───────────────────────────────────────────────────────────────────
        response.ShouldNotBeNull();
        response.WorkspaceId.ShouldBe(workspaceA.Id);
        response.MemberId.ShouldNotBe(Guid.Empty);

        readBack.ShouldNotBeNull("CR-01: Accept must insert a WorkspaceMember row even when the DbContext is scoped to a different tenant.");
        // CR-01 + BLOCKER 2 load-bearing assertion: the new row's TenantId shadow property MUST be
        // the invitation's workspace id, NOT RootTenant.Id. This only happens if the
        // IMultiTenantContextSetter SaveChanges scope switch in the handler executed successfully
        // (non-null setter + correct workspaceTenant AppTenantInfo).
        readBack.TenantId.ShouldBe(workspaceA.Id.ToString(), "CR-01: TenantId shadow property must be stamped with the invitation's workspace id, not the caller's current tenant.");
        readBack.WorkspaceId.ShouldBe(workspaceA.Id);
        readBack.UserId.ShouldBe(InviteeUserId.ToString());
        readBack.Role.ShouldBe((int)WorkspaceRole.Member);
        readBack.IsActive.ShouldBeTrue();
        readBack.Id.ShouldBe(response.MemberId);
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
