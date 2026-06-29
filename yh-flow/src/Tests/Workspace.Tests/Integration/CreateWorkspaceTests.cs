using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using YH.Framework.Core.Exceptions;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;
using YH.Modules.Workspace.Services;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Integration tests for <see cref="CreateWorkspaceCommandHandler"/> (plan 02-04 Task 2 — D-06).
/// Verifies that creating a workspace auto-enrols the owner as the first Admin member.
/// </summary>
/// <remarks>
/// InMemory DbContext is used (consistent with MembershipMiddlewareTests pattern). InMemory does
/// not enforce the unique Slug index — that constraint is verified at the relational level by the
/// InitialWorkspace migration (02-03). These tests cover the BUSINESS semantics of D-06.
/// </remarks>
public sealed class CreateWorkspaceTests
{
    private static readonly Guid OwnerUserId = Guid.Parse("00000000-0000-0000-0000-0000000000d1");

    [Fact]
    public async Task Handle_GeneratesSlugFromName_AndAutoEnrolsOwnerAsAdmin()
    {
        await using var db = NewInMemoryContext();
        var gen = new SlugGenerator(db);
        var handler = new CreateWorkspaceCommandHandler(db, gen);

        var result = await handler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme Corp",
            OwnerUserId = OwnerUserId,
        }, CancellationToken.None);

        // Slug derived from name.
        result.Slug.ShouldBe("acme-corp");

        // Workspace persisted.
        var ws = await db.Workspaces.SingleAsync();
        ws.Name.ShouldBe("Acme Corp");
        ws.Slug.ShouldBe("acme-corp");
        ws.OwnerId.ShouldBe(OwnerUserId);

        // D-06 — owner auto-enrolled as Admin member.
        var member = await db.Members.SingleAsync();
        member.WorkspaceId.ShouldBe(ws.Id);
        member.UserId.ShouldBe(OwnerUserId.ToString());
        member.Role.ShouldBe((int)WorkspaceRole.Admin);
        member.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ExplicitSlug_WhenValidAndFree_UsesItAsIs()
    {
        await using var db = NewInMemoryContext();
        var gen = new SlugGenerator(db);
        var handler = new CreateWorkspaceCommandHandler(db, gen);

        var result = await handler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            Slug = "custom-slug",
            OwnerUserId = OwnerUserId,
        }, CancellationToken.None);

        result.Slug.ShouldBe("custom-slug");
    }

    [Fact]
    public async Task Handle_ExplicitSlug_WhenTaken_Returns409Conflict()
    {
        // Seed an existing workspace owning "acme".
        await using var seedDb = NewInMemoryContext();
        var seedGen = new SlugGenerator(seedDb);
        var seedHandler = new CreateWorkspaceCommandHandler(seedDb, seedGen);
        await seedHandler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            OwnerUserId = Guid.NewGuid(),
        }, CancellationToken.None);
        await seedDb.SaveChangesAsync();

        // Re-use the same context (already has "acme").
        var gen = new SlugGenerator(seedDb);
        var handler = new CreateWorkspaceCommandHandler(seedDb, gen);

        var act = async () => await handler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            Slug = "acme", // explicit + already taken
            OwnerUserId = Guid.NewGuid(),
        }, CancellationToken.None);

        var ex = await Should.ThrowAsync<CustomException>(act);
        ex.StatusCode.ShouldBe(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Handle_ExplicitSlug_WhenInvalidFormat_Returns400BadRequest()
    {
        await using var db = NewInMemoryContext();
        var gen = new SlugGenerator(db);
        var handler = new CreateWorkspaceCommandHandler(db, gen);

        var act = async () => await handler.Handle(new CreateWorkspaceCommand
        {
            Name = "Acme",
            Slug = "UPPER-CASE", // invalid format
            OwnerUserId = OwnerUserId,
        }, CancellationToken.None);

        var ex = await Should.ThrowAsync<CustomException>(act);
        ex.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
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
            .UseInMemoryDatabase($"ws-create-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }
}
