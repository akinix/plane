using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Configuration;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Services;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Verifies the D-12 state-machine invalidation (threat T-2-replay [BLOCKING] +
/// T-2-acceptdouble).
/// </summary>
/// <remarks>
/// <para>
/// After any terminal transition (Accept / Revoke / Reject), the SAME raw token MUST no longer
/// validate — <see cref="IInvitationTokenService.ValidateAsync"/> returns null. This is the
/// load-bearing replay guard: a leaked token that has been consumed cannot be replayed.
/// </para>
/// <para>
/// Three cases mirror the three terminal transitions on <see cref="WorkspaceInvitation"/>:
/// <see cref="WorkspaceInvitation.Accept"/>, <see cref="WorkspaceInvitation.Revoke"/>,
/// <see cref="WorkspaceInvitation.Reject"/>.
/// </para>
/// </remarks>
public sealed class InvitationInvalidateTests
{
    private static readonly Guid WorkspaceId = Guid.Parse("00000000-0000-0000-0000-0000000000f1");

    [Fact]
    public async Task Invalidate_AfterAccept_TokenBecomesInvalid()
    {
        await using var db = NewInMemoryContext();
        var tokenService = NewTokenService(db);

        var (invitation, rawToken) = await tokenService.CreateAsync(
            workspaceId: WorkspaceId,
            email: "alice@example.com",
            role: WorkspaceRole.Member,
            ttlDays: 7,
            message: null,
            cancellationToken: CancellationToken.None);

        // Sanity — fresh token validates.
        (await tokenService.ValidateAsync(rawToken, CancellationToken.None)).ShouldNotBeNull();

        // Apply Accept transition via the tracked context (same path AcceptInvitationCommandHandler takes).
        var tracked = await db.Invitations.FirstAsync(i => i.Id == invitation.Id);
        tracked.Accept().ShouldBeTrue();
        await db.SaveChangesAsync();

        // CRITICAL — replay rejected (T-2-acceptdouble).
        (await tokenService.ValidateAsync(rawToken, CancellationToken.None)).ShouldBeNull();
    }

    [Fact]
    public async Task Invalidate_AfterRevoke_TokenBecomesInvalid()
    {
        await using var db = NewInMemoryContext();
        var tokenService = NewTokenService(db);

        var (invitation, rawToken) = await tokenService.CreateAsync(
            workspaceId: WorkspaceId,
            email: "alice@example.com",
            role: WorkspaceRole.Member,
            ttlDays: 7,
            message: null,
            cancellationToken: CancellationToken.None);

        (await tokenService.ValidateAsync(rawToken, CancellationToken.None)).ShouldNotBeNull();

        var tracked = await db.Invitations.FirstAsync(i => i.Id == invitation.Id);
        tracked.Revoke(reason: "admin revoked").ShouldBeTrue();
        await db.SaveChangesAsync();

        // CRITICAL — revoked token rejected (T-2-replay).
        (await tokenService.ValidateAsync(rawToken, CancellationToken.None)).ShouldBeNull();
    }

    [Fact]
    public async Task Invalidate_AfterReject_TokenBecomesInvalid()
    {
        await using var db = NewInMemoryContext();
        var tokenService = NewTokenService(db);

        var (invitation, rawToken) = await tokenService.CreateAsync(
            workspaceId: WorkspaceId,
            email: "alice@example.com",
            role: WorkspaceRole.Member,
            ttlDays: 7,
            message: null,
            cancellationToken: CancellationToken.None);

        (await tokenService.ValidateAsync(rawToken, CancellationToken.None)).ShouldNotBeNull();

        var tracked = await db.Invitations.FirstAsync(i => i.Id == invitation.Id);
        tracked.Reject().ShouldBeTrue();
        await db.SaveChangesAsync();

        // CRITICAL — rejected token rejected (T-2-replay).
        (await tokenService.ValidateAsync(rawToken, CancellationToken.None)).ShouldBeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static WorkspaceDbContext NewInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(WorkspaceId.ToString(), "ws-invalidate", "Invalidate Test");
        accessor.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(tenant));

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"ws-invalidate-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }

    private static InvitationTokenService NewTokenService(WorkspaceDbContext db)
    {
        var options = Options.Create(new WorkspaceTokenOptions());
        return new InvitationTokenService(db, options);
    }
}
