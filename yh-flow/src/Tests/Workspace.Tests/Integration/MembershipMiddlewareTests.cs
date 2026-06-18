using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using YH.Framework.Core.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Middleware;

// CurrentWorkspaceContext lives in the root YH.Modules.Workspace namespace (same identifier as
// the namespace itself — the 02-02 alias pattern was unnecessary here because the test file does
// not use the entity type Workspace, only the context type).
using WorkspaceContextImpl = YH.Modules.Workspace.CurrentWorkspaceContext;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Integration tests for <see cref="WorkspaceMembershipMiddleware"/> (D-02, plan 02-03 Task 2).
/// </summary>
/// <remarks>
/// <para>
/// <b>What is under test:</b> the middleware populates <see cref="ICurrentWorkspaceContext"/>
/// based on the Finbuckle-resolved tenant + the current user's active membership. Three scenarios
/// drive the assertions:
/// </para>
/// <list type="bullet">
///   <item><b>Member (Admin role):</b> seeded member row exists with <c>IsActive=true</c> and
///   <c>Role=Admin(20)</c> → <see cref="ICurrentWorkspaceContext.CurrentUserRole"/> == Admin,
///   <see cref="ICurrentWorkspaceContext.CurrentWorkspaceId"/> == seeded workspace Guid.</item>
///   <item><b>Non-member:</b> no member row exists for the user in the resolved workspace →
///   <see cref="ICurrentWorkspaceContext.CurrentUserRole"/> == null (the Plane semantics for
///   non-member; downstream <c>[RequireWorkspaceRole]</c> must fail).</item>
///   <item><b>Inactive member:</b> member row exists with <c>IsActive=false</c> → role == null
///   (deactivated members lose access, threat T-2-eop mitigation).</item>
/// </list>
/// <para>
/// <b>Test isolation note:</b> InMemory provider does not run the Finbuckle tenant filter
/// (<c>ApplyTenantIsolationByDefault</c> requires a relational provider for shadow-property
/// <c>TenantId</c> semantics), so these tests rely on the explicit <c>UserId + IsActive</c>
/// predicate to exercise the middleware's data query. The Finbuckle-filtered double-protection
/// path is covered by <c>TenantIsolationTests</c> (relational semantics verified via a hand-rolled
/// workspace A / workspace B separation using distinct <c>WorkspaceId</c> values and the same
/// InMemory context).
/// </para>
/// </remarks>
public sealed class MembershipMiddlewareTests
{
    private static readonly Guid WorkspaceId = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private const string WorkspaceSlug = "acme-test";
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-0000000000b1");
    private static readonly Guid NonMemberUserId = Guid.Parse("00000000-0000-0000-0000-0000000000b2");
    private static readonly Guid InactiveMemberUserId = Guid.Parse("00000000-0000-0000-0000-0000000000b3");

    [Fact]
    public async Task InvokeAsync_ActiveAdminMember_PopulatesContextWithAdminRole()
    {
        await using var db = CreateInMemoryContext();
        await SeedAdminMemberAsync(db);

        var (middleware, workspaceContext) = BuildMiddleware(db, AdminUserId);
        var ctx = new DefaultHttpContext();
        var nextCalled = false;

        await middleware.InvokeAsync(ctx, _ => { nextCalled = true; return Task.CompletedTask; });

        nextCalled.ShouldBeTrue();
        workspaceContext.CurrentWorkspaceId.ShouldBe(WorkspaceId);
        workspaceContext.Slug.ShouldBe(WorkspaceSlug);
        workspaceContext.CurrentUserRole.ShouldBe(WorkspaceRole.Admin);
    }

    [Fact]
    public async Task InvokeAsync_NonMemberUser_PopulatesNullRole()
    {
        await using var db = CreateInMemoryContext();
        // No member row seeded for NonMemberUserId.

        var (middleware, workspaceContext) = BuildMiddleware(db, NonMemberUserId);
        var ctx = new DefaultHttpContext();

        await middleware.InvokeAsync(ctx, _ => Task.CompletedTask);

        workspaceContext.CurrentWorkspaceId.ShouldBe(WorkspaceId);
        workspaceContext.Slug.ShouldBe(WorkspaceSlug);
        // Non-member: role must be null so downstream [RequireWorkspaceRole] fails (T-2-eop).
        workspaceContext.CurrentUserRole.ShouldBeNull();
    }

    [Fact]
    public async Task InvokeAsync_InactiveMember_PopulatesNullRole()
    {
        await using var db = CreateInMemoryContext();
        await SeedInactiveMemberAsync(db);

        var (middleware, workspaceContext) = BuildMiddleware(db, InactiveMemberUserId);
        var ctx = new DefaultHttpContext();

        await middleware.InvokeAsync(ctx, _ => Task.CompletedTask);

        // Inactive members lose access (deactivated but kept for audit) — role must be null.
        workspaceContext.CurrentWorkspaceId.ShouldBe(WorkspaceId);
        workspaceContext.CurrentUserRole.ShouldBeNull();
    }

    [Fact]
    public async Task InvokeAsync_NoTenantResolved_DoesNotTouchContext()
    {
        await using var db = CreateInMemoryContext();
        await SeedAdminMemberAsync(db);

        // tenantAccessor returns null MultiTenantContext → top-level endpoint path.
        var tenantAccessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        tenantAccessor.MultiTenantContext.Returns((MultiTenantContext<AppTenantInfo>?)null);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated().Returns(true);
        currentUser.GetUserId().Returns(AdminUserId);

        var workspaceContext = new WorkspaceContextImpl();
        var middleware = new WorkspaceMembershipMiddleware(tenantAccessor, currentUser, db, workspaceContext);
        var ctx = new DefaultHttpContext();

        await middleware.InvokeAsync(ctx, _ => Task.CompletedTask);

        // Top-level endpoint: context stays at default (null) values — request is user-scoped.
        workspaceContext.CurrentWorkspaceId.ShouldBeNull();
        workspaceContext.Slug.ShouldBeNull();
        workspaceContext.CurrentUserRole.ShouldBeNull();
    }

    [Fact]
    public async Task InvokeAsync_AnonymousUser_DoesNotTouchContext()
    {
        await using var db = CreateInMemoryContext();
        await SeedAdminMemberAsync(db);

        // Authenticated=false → guard prevents GetUserId() / SetContext (anonymous cannot be a member).
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated().Returns(false);

        var (tenantAccessor, _) = BuildTenantAccessor();
        var workspaceContext = new WorkspaceContextImpl();
        var middleware = new WorkspaceMembershipMiddleware(tenantAccessor, currentUser, db, workspaceContext);
        var ctx = new DefaultHttpContext();

        await middleware.InvokeAsync(ctx, _ => Task.CompletedTask);

        // Anonymous: no SetContext call — threat T-2-memberskip mitigation (no spurious member stamp).
        workspaceContext.CurrentWorkspaceId.ShouldBeNull();
        workspaceContext.CurrentUserRole.ShouldBeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static WorkspaceDbContext CreateInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(WorkspaceId.ToString(), WorkspaceSlug, "Acme Test");
        var context = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(context);

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"ws-mw-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }

    private static async Task SeedAdminMemberAsync(WorkspaceDbContext db)
    {
        var member = WorkspaceMember.Create(WorkspaceId, AdminUserId.ToString(), role: (int)WorkspaceRole.Admin);
        db.Members.Add(member);
        await db.SaveChangesAsync();
    }

    private static async Task SeedInactiveMemberAsync(WorkspaceDbContext db)
    {
        var member = WorkspaceMember.Create(
            WorkspaceId,
            InactiveMemberUserId.ToString(),
            role: (int)WorkspaceRole.Member,
            isActive: false);
        db.Members.Add(member);
        await db.SaveChangesAsync();
    }

    private static (IMultiTenantContextAccessor<AppTenantInfo> accessor, AppTenantInfo tenant) BuildTenantAccessor()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(WorkspaceId.ToString(), WorkspaceSlug, "Acme Test");
        var context = new MultiTenantContext<AppTenantInfo>(tenant);
        accessor.MultiTenantContext.Returns(context);
        return (accessor, tenant);
    }

    private static (WorkspaceMiddlewareInstance middleware, ICurrentWorkspaceContext workspaceContext)
        BuildMiddleware(WorkspaceDbContext db, Guid userId)
    {
        var (tenantAccessor, _) = BuildTenantAccessor();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated().Returns(true);
        currentUser.GetUserId().Returns(userId);

        var workspaceContext = new WorkspaceContextImpl();
        var middleware = new WorkspaceMembershipMiddleware(tenantAccessor, currentUser, db, workspaceContext);
        return (new WorkspaceMiddlewareInstance(middleware), workspaceContext);
    }

    /// <summary>Tag wrapper so the helper tuple signature stays readable.</summary>
    private sealed class WorkspaceMiddlewareInstance(WorkspaceMembershipMiddleware inner)
    {
        public Task InvokeAsync(HttpContext ctx, RequestDelegate next) => inner.InvokeAsync(ctx, next);
    }
}
