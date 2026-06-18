using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using System.Reflection;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace.Configuration;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Services;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Verifies the D-12 TTL guard (threat T-2-ttl).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IInvitationTokenService.ValidateAsync"/> MUST reject tokens whose invitation is
/// past <see cref="WorkspaceInvitation.ExpiresAt"/>. The TTL default is 7 days
/// (<see cref="WorkspaceTokenOptions.InvitationTokenTtlDays"/>); this test constructs an
/// invitation whose <c>ExpiresAt</c> is already in the past and asserts the validation gate
/// returns null.
/// </para>
/// <para>
/// Because <see cref="WorkspaceInvitation.Create"/> stamps <c>ExpiresAt = UtcNow + ttlDays</c>
/// (forward-looking), the test forces the property to a back-dated value via reflection. The
/// setter is private by design (entity immutability from outside); the test reflects to simulate
/// the passage of time without depending on the system clock. The production code path never
/// back-dates <c>ExpiresAt</c>.
/// </para>
/// </remarks>
public sealed class InvitationTtlTests
{
    private static readonly Guid WorkspaceId = Guid.Parse("00000000-0000-0000-0000-0000000000ea");

    [Fact]
    public async Task ValidateAsync_RejectsExpiredToken()
    {
        await using var db = NewInMemoryContext();
        var tokenService = NewTokenService(db);

        // Create a fresh invitation via the production path, then back-date its ExpiresAt to
        // simulate a token that has aged past its TTL.
        var (invitation, rawToken) = await tokenService.CreateAsync(
            workspaceId: WorkspaceId,
            email: "alice@example.com",
            role: WorkspaceRole.Member,
            ttlDays: 7,
            message: null,
            cancellationToken: CancellationToken.None);

        // Force the property — entity keeps the setter private to prevent external mutation; the
        // test reflects to simulate the clock advancing past expiry.
        var attached = await db.Invitations.FirstAsync(i => i.Id == invitation.Id);
        SetPrivateProperty(attached, nameof(WorkspaceInvitation.ExpiresAt),
            DateTimeOffset.UtcNow.AddHours(-1));
        await db.SaveChangesAsync();

        // CRITICAL — expired token rejected (T-2-ttl).
        var validated = await tokenService.ValidateAsync(rawToken, CancellationToken.None);
        validated.ShouldBeNull();
    }

    [Fact]
    public async Task ValidateAsync_AcceptsNonExpiredToken()
    {
        // Sanity check — a fresh (non-expired) token validates normally.
        await using var db = NewInMemoryContext();
        var tokenService = NewTokenService(db);

        var (_, rawToken) = await tokenService.CreateAsync(
            workspaceId: WorkspaceId,
            email: "bob@example.com",
            role: WorkspaceRole.Member,
            ttlDays: 7,
            message: null,
            cancellationToken: CancellationToken.None);

        var validated = await tokenService.ValidateAsync(rawToken, CancellationToken.None);
        validated.ShouldNotBeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static WorkspaceDbContext NewInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(WorkspaceId.ToString(), "ws-ttl", "TTL Test");
        accessor.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(tenant));

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"ws-ttl-{Guid.NewGuid()}")
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

    /// <summary>
    /// Test-only helper for back-dating entity properties whose setters are private by design.
    /// Used exclusively by this class to simulate clock advancement.
    /// </summary>
    private static void SetPrivateProperty<T>(T target, string propertyName, object value)
    {
        var prop = typeof(T).GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new InvalidOperationException(
                $"Property '{propertyName}' not found on {typeof(T).Name}.");

        prop.SetValue(target, value);
    }
}
