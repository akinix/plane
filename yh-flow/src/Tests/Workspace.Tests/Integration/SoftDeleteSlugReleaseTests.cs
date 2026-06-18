using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Contracts.v1.Workspaces.DeleteWorkspace;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Features.v1.Workspaces.DeleteWorkspace;
using YH.Modules.Workspace.Services;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Integration tests for D-08 — soft-deleting a workspace releases its slug for reuse by a new
/// workspace (plan 02-04 Task 2).
/// </summary>
/// <remarks>
/// Flow: create "acme" → soft-delete (slug becomes "acme__{epoch}") → create new "acme" → the new
/// workspace reuses the original slug (SlugGenerator collision probe ignores the suffixed
/// soft-deleted row). Also verifies the cache-invalidation call to
/// <c>IMultiTenantStore&lt;AppTenantInfo&gt;.RemoveAsync(slug)</c>.
/// </remarks>
public sealed class SoftDeleteSlugReleaseTests
{
    private static readonly Guid OwnerUserId = Guid.Parse("00000000-0000-0000-0000-0000000000e1");

    [Fact]
    public async Task SoftDelete_ReleasesSlugForReuse_NewWorkspaceGetsOriginalSlug()
    {
        await using var db = NewInMemoryContext();
        var gen = new SlugGenerator(db);
        var createHandler = new CreateWorkspaceCommandHandler(db, gen);
        var tenantStore = Substitute.For<IMultiTenantStore<AppTenantInfo>>();
        tenantStore.RemoveAsync(Arg.Any<string>()).Returns(true);
        var deleteHandler = new DeleteWorkspaceCommandHandler(db, tenantStore);

        // 1. Create "acme".
        var first = await createHandler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            OwnerUserId = OwnerUserId,
        }, CancellationToken.None);
        first.Slug.ShouldBe("acme");

        // 2. Soft-delete it (owner-initiated).
        await deleteHandler.Handle(new DeleteWorkspaceCommand
        {
            Slug = "acme",
            CurrentUserId = OwnerUserId,
        }, CancellationToken.None);

        // The soft-deleted row now carries the suffixed slug; the original "acme" is free.
        var all = await db.Workspaces.IgnoreQueryFilters().ToListAsync();
        all.Count.ShouldBe(1);
        all[0].Slug.ShouldStartWith("acme__");
        all[0].IsDeleted.ShouldBeTrue();

        // 3. Cache-invalidation WAS called with the original slug (T-2-cacheinvalid / 02-02 wiring).
        await tenantStore.Received(1).RemoveAsync("acme");

        // 4. A brand-new workspace with the same name reuses the original slug (D-08).
        var second = await createHandler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            OwnerUserId = Guid.NewGuid(),
        }, CancellationToken.None);
        second.Slug.ShouldBe("acme");
    }

    [Fact]
    public async Task SoftDelete_NonOwner_ThrowsForbidden()
    {
        // Threat T-2-eop-delete [BLOCKING] — admin-but-not-owner must NOT delete.
        await using var db = NewInMemoryContext();
        var gen = new SlugGenerator(db);
        var createHandler = new CreateWorkspaceCommandHandler(db, gen);
        var tenantStore = Substitute.For<IMultiTenantStore<AppTenantInfo>>();
        var deleteHandler = new DeleteWorkspaceCommandHandler(db, tenantStore);

        await createHandler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            OwnerUserId = OwnerUserId,
        }, CancellationToken.None);

        var notOwner = Guid.NewGuid();
        var act = async () => await deleteHandler.Handle(new DeleteWorkspaceCommand
        {
            Slug = "acme",
            CurrentUserId = notOwner,
        }, CancellationToken.None);

        await Should.ThrowAsync<ForbiddenException>(act);
        // Workspace row untouched.
        var ws = await db.Workspaces.IgnoreQueryFilters().SingleAsync();
        ws.IsDeleted.ShouldBeFalse();

        // Cache-invalidation NOT called (delete aborted before SaveChanges).
        await tenantStore.DidNotReceive().RemoveAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task SoftDelete_AlreadyDeleted_Returns404_Idempotent()
    {
        // Threat T-2-softdelete-idempotent (accept) — second DELETE surfaces as 404, no double epoch.
        await using var db = NewInMemoryContext();
        var gen = new SlugGenerator(db);
        var createHandler = new CreateWorkspaceCommandHandler(db, gen);
        var tenantStore = Substitute.For<IMultiTenantStore<AppTenantInfo>>();
        var deleteHandler = new DeleteWorkspaceCommandHandler(db, tenantStore);

        await createHandler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            OwnerUserId = OwnerUserId,
        }, CancellationToken.None);

        // First delete — succeeds.
        await deleteHandler.Handle(new DeleteWorkspaceCommand
        {
            Slug = "acme",
            CurrentUserId = OwnerUserId,
        }, CancellationToken.None);

        // Second delete of the same slug — 404 (soft-deleted row's slug is now suffixed).
        var act = async () => await deleteHandler.Handle(new DeleteWorkspaceCommand
        {
            Slug = "acme",
            CurrentUserId = OwnerUserId,
        }, CancellationToken.None);

        await Should.ThrowAsync<YH.Framework.Core.Exceptions.NotFoundException>(act);

        // Only ONE row, only ONE epoch suffix (no double epoch).
        var all = await db.Workspaces.IgnoreQueryFilters().ToListAsync();
        all.Count.ShouldBe(1);
        all[0].Slug.ShouldStartWith("acme__");
        // Ensure no second "__{epoch}" was appended.
        all[0].Slug.Count(c => c == '_').ShouldBe(2);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static WorkspaceDbContext NewInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(Guid.NewGuid().ToString(), "acme-test", "Acme Test");
        accessor.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(tenant));

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"ws-softdel-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }
}
