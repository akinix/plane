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
/// Verifies the D-12 token storage invariant (threat T-2-token [BLOCKING] + T-2-tokenleak).
/// </summary>
/// <remarks>
/// <para>
/// <b>What is under test:</b> after <see cref="IInvitationTokenService.CreateAsync"/>, the
/// database row for the new invitation MUST contain a SHA-256 <c>TokenHash</c> (64 lowercase hex
/// chars) and MUST NOT carry a raw-token field. The entity itself does not expose a RawToken
/// property — this test asserts that fact via reflection so a future regression that adds a
/// <c>RawToken</c> column to <see cref="WorkspaceInvitation"/> fails fast here.
/// </para>
/// <para>
/// The raw token MUST appear ONLY in the create-response tuple; the persisted hash MUST differ
/// from the raw token (so <c>TokenHash == rawToken</c> would imply plaintext storage).
/// </para>
/// </remarks>
public sealed class InvitationHashTests
{
    private static readonly Guid WorkspaceId = Guid.Parse("00000000-0000-0000-0000-0000000000e1");

    [Fact]
    public async Task Create_PersistsTokenHashNotRawToken()
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

        // Persisted row MUST exist and MUST carry a 64-char lowercase hex hash.
        var persisted = await db.Invitations.SingleAsync(i => i.Id == invitation.Id);
        persisted.TokenHash.Length.ShouldBe(64);
        foreach (var c in persisted.TokenHash)
        {
            (char.IsDigit(c) || (c >= 'a' && c <= 'f'))
                .ShouldBeTrue($"expected lowercase hex char, got '{c}'");
        }

        // CRITICAL — no RawToken field on the entity (D-12). If a future regression adds one,
        // this assertion catches it.
        typeof(WorkspaceInvitation)
            .GetProperties()
            .Any(p => p.Name.Contains("RawToken", StringComparison.OrdinalIgnoreCase))
            .ShouldBeFalse("WorkspaceInvitation MUST NOT carry a RawToken property (D-12 plaintext-leakage guard)");

        // The hash MUST differ from the raw token (otherwise storing the hash == storing plaintext).
        persisted.TokenHash.ShouldNotBe(rawToken);
        rawToken.ShouldNotContain(persisted.TokenHash);
    }

    [Fact]
    public async Task ValidateAsync_HashesInboundToken_AndMatchesByHash()
    {
        // Round-trip: create → validate-by-raw-token returns the persisted invitation. Same
        // InMemory DbContext for both ops so the row persists across calls.
        await using var db = NewInMemoryContext();
        var tokenService = NewTokenService(db);

        var (created, rawToken) = await tokenService.CreateAsync(
            workspaceId: WorkspaceId,
            email: "bob@example.com",
            role: WorkspaceRole.Member,
            ttlDays: 7,
            message: null,
            cancellationToken: CancellationToken.None);

        var validated = await tokenService.ValidateAsync(rawToken, CancellationToken.None);

        validated.ShouldNotBeNull();
        validated.Id.ShouldBe(created.Id);
        validated.TokenHash.ShouldBe(created.TokenHash);
    }

    [Fact]
    public async Task ValidateAsync_RejectsUnmatchedToken()
    {
        // A token whose hash matches no row returns null — the gate rejects unknown tokens.
        await using var db = NewInMemoryContext();
        var tokenService = NewTokenService(db);

        var validated = await tokenService.ValidateAsync(
            "definitely-not-a-real-token-0123456789abcdef",
            CancellationToken.None);

        validated.ShouldBeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static WorkspaceDbContext NewInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(WorkspaceId.ToString(), "ws-invite", "Invitations Test");
        accessor.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(tenant));

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"ws-invite-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }

    private static InvitationTokenService NewTokenService(WorkspaceDbContext db)
    {
        // The token service shares the test's InMemory DbContext so created invitations are
        // visible to subsequent ValidateAsync calls within the same test.
        var options = Options.Create(new WorkspaceTokenOptions());
        return new InvitationTokenService(db, options);
    }
}
