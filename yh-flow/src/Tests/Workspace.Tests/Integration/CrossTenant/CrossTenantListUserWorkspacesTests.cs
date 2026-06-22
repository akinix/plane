using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using YH.Framework.Shared.Multitenancy;
using YH.Modules.Workspace.Contracts.v1.Workspaces.ListUserWorkspaces;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Features.v1.Workspaces.ListUserWorkspaces;
using YH.Tests.Workspace.Integration.PostgresFixtures;
// Namespace/type collision: the root namespace `YH.Tests.Workspace` and the entity
// `YH.Modules.Workspace.Domain.Workspace` share the "Workspace" identifier when the compiler
// tries to resolve `Workspace.Create`. Alias the entity so the factory call resolves cleanly
// (mirrors the pattern in WorkspaceDbContext.cs:13).
using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;

namespace YH.Tests.Workspace.Integration.CrossTenant;

/// <summary>
/// Relational test for the CR-02 fix (plan 02-07 / Wave 6). Seeds membership rows for one user in
/// TWO distinct workspace tenants and asserts <see cref="ListUserWorkspacesQueryHandler"/> returns
/// both workspace ids — something the InMemory suite cannot verify because InMemory does not apply
/// Finbuckle's auto <c>TenantId</c> query filter (see <c>02-REVIEW.md §Notes on the 02-06 closeout
/// tests</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this test must be relational (CR-02 single source of truth — <c>02-REVIEW.md §CR-02</c>):</b>
/// the InMemory provider skips Finbuckle's <c>AdjustUniqueIndexes</c> pipeline AND the auto-applied
/// tenant filter. A user who is a member of workspaces A, B and C will see at most one of them on
/// the top-level endpoint before the CR-02 fix, but InMemory will report all three regardless of
/// whether <c>IgnoreQueryFilters()</c> is present. Only a real Postgres schema (with the
/// <c>TenantId</c> shadow column populated by Finbuckle on save) can fail the test pre-fix and pass
/// post-fix.
/// </para>
/// <para>
/// <b>Test isolation:</b> each test method <c>TRUNCATE</c>s the Workspace tables (with
/// <c>RESTART IDENTITY CASCADE</c>) at the start so test classes sharing the fixture do not leak
/// rows. The fixture itself keeps the container + schema alive across tests in the same class.
/// </para>
/// </remarks>
public sealed class CrossTenantListUserWorkspacesTests : IClassFixture<WorkspacePostgresFixture>
{
    private static readonly Guid OwnerUserId = Guid.Parse("00000000-0000-0000-0000-000000000071");

    private readonly WorkspacePostgresFixture _fixture;

    public CrossTenantListUserWorkspacesTests(WorkspacePostgresFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    /// <summary>
    /// LOAD-BEARING CR-02 assertion: a user with active memberships in two DISTINCT workspace tenants
    /// sees BOTH workspace ids in <see cref="ListUserWorkspacesQueryHandler"/>, even when the DbContext
    /// is scoped to a third "root" tenant (simulating a top-level endpoint where the user has no
    /// current workspace resolved).
    /// </summary>
    [Fact]
    [Trait("Category", "Postgres")]
    [Trait("Category", "RequiresDocker")]
    public async Task Returns_Workspaces_Across_Distinct_Tenants_On_Real_Postgres()
    {
        // Reset state so multiple invocations of this class do not accumulate rows.
        await TruncateAsync();

        var accessor = _fixture.Services.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
        var setter = _fixture.Services.GetRequiredService<IMultiTenantContextSetter>();
        var userId = OwnerUserId.ToString();

        // Seed workspace A (an IGlobalEntity — NOT filtered) and workspace B.
        var workspaceA = WorkspaceEntity.Create(name: "Acme", slug: "acme", ownerUserId: OwnerUserId);
        var workspaceB = WorkspaceEntity.Create(name: "Globex", slug: "globex", ownerUserId: OwnerUserId);

        await using (var seedCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant))
        {
            // Workspace is IGlobalEntity; RootTenant scope suffices to insert without tenant filter issues.
            await seedCtx.Workspaces.AddRangeAsync(workspaceA, workspaceB);
            await seedCtx.SaveChangesAsync();
        }

        // Seed an ACTIVE membership in workspace A (tenant = workspaceA.Id).
        var tenantA = new AppTenantInfo(id: workspaceA.Id.ToString(), identifier: workspaceA.Id.ToString(), name: workspaceA.Name);
        using (new FinbuckleTestTenantScope(accessor, setter, tenantA))
        {
            await using var ctxA = _fixture.CreateContextForTenant(tenantA);
            await ctxA.Members.AddAsync(WorkspaceMember.Create(workspaceId: workspaceA.Id, userId: userId, role: 20));
            await ctxA.SaveChangesAsync();
        }

        // Seed an ACTIVE membership in workspace B (tenant = workspaceB.Id).
        var tenantB = new AppTenantInfo(id: workspaceB.Id.ToString(), identifier: workspaceB.Id.ToString(), name: workspaceB.Name);
        using (new FinbuckleTestTenantScope(accessor, setter, tenantB))
        {
            await using var ctxB = _fixture.CreateContextForTenant(tenantB);
            await ctxB.Members.AddAsync(WorkspaceMember.Create(workspaceId: workspaceB.Id, userId: userId, role: 15));
            await ctxB.SaveChangesAsync();
        }

        // CRITICAL: invoke the handler scoped to the RootTenant (NEITHER A nor B), so Finbuckle's
        // tenant filter on db.Members would silently drop BOTH rows without IgnoreQueryFilters().
        // This mirrors the top-level endpoint where the user has no current workspace resolved.
        await using var handlerCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant);
        var handler = new ListUserWorkspacesQueryHandler(handlerCtx);

        var result = await handler.Handle(new ListUserWorkspacesQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 50,
        }, CancellationToken.None);

        var returnedIds = result.Results.Select(w => w.Id).OrderBy(id => id).ToList();
        var expected = new[] { workspaceA.Id, workspaceB.Id }.OrderBy(id => id).ToList();
        returnedIds.ShouldBe(expected, "CR-02: handler must surface memberships across distinct tenants via IgnoreQueryFilters.");
        result.Count.ShouldBe(2);
    }

    /// <summary>
    /// Boundary case: an inactive membership row (IsActive=false) must NOT surface in the result
    /// even when IgnoreQueryFilters is applied. Confirms the business-rule Where clause on
    /// IsActive and IsDeleted is preserved.
    /// </summary>
    [Fact]
    [Trait("Category", "Postgres")]
    [Trait("Category", "RequiresDocker")]
    public async Task Inactive_Membership_Row_Is_Excluded_From_Results()
    {
        await TruncateAsync();

        var accessor = _fixture.Services.GetRequiredService<AsyncLocalMultiTenantContextAccessor<AppTenantInfo>>();
        var setter = _fixture.Services.GetRequiredService<IMultiTenantContextSetter>();
        var userId = OwnerUserId.ToString();

        var workspaceA = WorkspaceEntity.Create(name: "Acme2", slug: "acme2", ownerUserId: OwnerUserId);
        var workspaceB = WorkspaceEntity.Create(name: "Globex2", slug: "globex2", ownerUserId: OwnerUserId);

        await using (var seedCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant))
        {
            await seedCtx.Workspaces.AddRangeAsync(workspaceA, workspaceB);
            await seedCtx.SaveChangesAsync();
        }

        // ACTIVE membership in A.
        var tenantA = new AppTenantInfo(id: workspaceA.Id.ToString(), identifier: workspaceA.Id.ToString(), name: workspaceA.Name);
        using (new FinbuckleTestTenantScope(accessor, setter, tenantA))
        {
            await using var ctxA = _fixture.CreateContextForTenant(tenantA);
            await ctxA.Members.AddAsync(WorkspaceMember.Create(workspaceId: workspaceA.Id, userId: userId, role: 20, isActive: true));
            await ctxA.SaveChangesAsync();
        }

        // INACTIVE membership in B (Deactivate via factory's isActive=false arg).
        var tenantB = new AppTenantInfo(id: workspaceB.Id.ToString(), identifier: workspaceB.Id.ToString(), name: workspaceB.Name);
        using (new FinbuckleTestTenantScope(accessor, setter, tenantB))
        {
            await using var ctxB = _fixture.CreateContextForTenant(tenantB);
            await ctxB.Members.AddAsync(WorkspaceMember.Create(workspaceId: workspaceB.Id, userId: userId, role: 15, isActive: false));
            await ctxB.SaveChangesAsync();
        }

        await using var handlerCtx = _fixture.CreateContextForTenant(WorkspacePostgresFixture.RootTenant);
        var handler = new ListUserWorkspacesQueryHandler(handlerCtx);

        var result = await handler.Handle(new ListUserWorkspacesQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 50,
        }, CancellationToken.None);

        // Only workspaceA's id surfaces; the inactive row in workspaceB must NOT appear even with
        // IgnoreQueryFilters (the active business rule is independent of tenant scoping).
        var returnedIds = result.Results.Select(w => w.Id).ToList();
        returnedIds.ShouldBe(new[] { workspaceA.Id });
        result.Count.ShouldBe(1);
    }

    /// <summary>
    /// Truncates the Workspace module tables between tests so the shared fixture does not accumulate
    /// cross-test state. Uses CASCADE so dependent rows go too; RESTART IDENTITY resets the slug
    /// unique index too.
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
