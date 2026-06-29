using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Contracts.v1.Members.ListMembers;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Features.v1.Members.ListMembers;

namespace YH.Tests.Workspace.Integration;

/// <summary>
/// Integration tests for <see cref="ListMembersQueryHandler"/> — verify the D-05 two-phase
/// batch lookup avoids the N+1 (RESEARCH Pitfall 3, threat T-2-n1 [BLOCKING], NFR-1).
/// </summary>
/// <remarks>
/// <para>
/// <b>What is under test:</b> the handler MUST call
/// <see cref="IUserIdentityService.GetUsersByIdsAsync"/> EXACTLY ONCE per list call, regardless
/// of how many members are on the page. The naive alternative (resolve each member's user in a
/// loop) would issue N round trips to Identity — exactly the N+1 these tests guard against.
/// </para>
/// <para>
/// The handler is exercised against an InMemory <see cref="WorkspaceDbContext"/> seeded with
/// 5 members; <see cref="IUserIdentityService"/> is stubbed via NSubstitute so we can capture
/// the call count + assert the batched id list contains every member's user id.
/// </para>
/// </remarks>
public sealed class ListMembersBatchTests
{
    private static readonly Guid WorkspaceId = Guid.Parse("00000000-0000-0000-0000-0000000000a1");

    private static readonly Guid[] MemberUserIds =
    {
        Guid.Parse("00000000-0000-0000-0000-0000000000b1"),
        Guid.Parse("00000000-0000-0000-0000-0000000000b2"),
        Guid.Parse("00000000-0000-0000-0000-0000000000b3"),
        Guid.Parse("00000000-0000-0000-0000-0000000000b4"),
        Guid.Parse("00000000-0000-0000-0000-0000000000b5"),
    };

    [Fact]
    public async Task Handle_BatchesUserResolutionIntoSingleCall_AvoidsNPlusOne()
    {
        // Arrange — seed 5 members.
        await using var db = NewInMemoryContext();
        await SeedMembersAsync(db);

        var identity = Substitute.For<IUserIdentityService>();
        identity
            .GetUsersByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult<IReadOnlyDictionary<Guid, UserSummary>>(
                BuildSummaries(ci.Arg<IEnumerable<Guid>>())));

        var handler = new ListMembersQueryHandler(db, identity);

        // Act.
        var result = await handler.Handle(new ListMembersQuery
        {
            WorkspaceId = WorkspaceId,
            PageSize = 50, // entire roster fits on one page
        }, CancellationToken.None);

        // Assert — D-05 N+1 avoidance: the identity service MUST be called exactly once.
        // A naive per-member loop would have invoked it 5 times.
        await identity.Received(1).GetUsersByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.ContainsAll(MemberUserIds)),
            Arg.Any<CancellationToken>());

        // All 5 members surfaced with their resolved user summaries.
        result.Count.ShouldBe(5);
        foreach (var dto in result.Results)
        {
            dto.User.ShouldNotBeNull();
            dto.WorkspaceId.ShouldBe(WorkspaceId);
        }
    }

    [Fact]
    public async Task Handle_Paginates_BatchResolvesOnlyThePage()
    {
        // Arrange — seed 5 members; request page size 2.
        await using var db = NewInMemoryContext();
        await SeedMembersAsync(db);

        var identity = Substitute.For<IUserIdentityService>();
        identity
            .GetUsersByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult<IReadOnlyDictionary<Guid, UserSummary>>(
                BuildSummaries(ci.Arg<IEnumerable<Guid>>())));

        var handler = new ListMembersQueryHandler(db, identity);

        // Act — first page (2 members).
        var firstPage = await handler.Handle(new ListMembersQuery
        {
            WorkspaceId = WorkspaceId,
            PageNumber = 1,
            PageSize = 2,
        }, CancellationToken.None);

        // Assert — only 2 ids batched through the identity service.
        await identity.Received(1).GetUsersByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.ToList().Count == 2),
            Arg.Any<CancellationToken>());

        firstPage.Count.ShouldBe(5); // total membership
        firstPage.Results.Count.ShouldBe(2); // page slice
    }

    [Fact]
    public async Task Handle_EmptyWorkspace_ReturnsEmptyWithoutCallingIdentity()
    {
        await using var db = NewInMemoryContext();
        var identity = Substitute.For<IUserIdentityService>();

        var handler = new ListMembersQueryHandler(db, identity);

        var result = await handler.Handle(new ListMembersQuery
        {
            WorkspaceId = WorkspaceId,
        }, CancellationToken.None);

        result.Count.ShouldBe(0);
        result.Results.ShouldBeEmpty();

        // No members → no batch lookup. Defense against accidental empty-batch round trips.
        await identity.DidNotReceive().GetUsersByIdsAsync(
            Arg.Any<IEnumerable<Guid>>(),
            Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private static async Task SeedMembersAsync(WorkspaceDbContext db)
    {
        for (var i = 0; i < MemberUserIds.Length; i++)
        {
            var role = i == 0 ? WorkspaceRole.Admin : WorkspaceRole.Member;
            db.Members.Add(WorkspaceMember.Create(
                workspaceId: WorkspaceId,
                userId: MemberUserIds[i].ToString(),
                role: (int)role,
                isActive: true));
        }

        await db.SaveChangesAsync();
    }

    private static Dictionary<Guid, UserSummary> BuildSummaries(IEnumerable<Guid> ids)
    {
        return ids.ToDictionary(
            id => id,
            id => new UserSummary(id, $"User {id.ToString().Substring(34, 2)}", $"{id:N}@test", null));
    }

    private static WorkspaceDbContext NewInMemoryContext()
    {
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(WorkspaceId.ToString(), "ws-members", "Members Test");
        accessor.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(tenant));

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"ws-members-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }
}

internal static class ListMembersBatchTestExtensions
{
    internal static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> expected)
    {
        var set = new HashSet<T>(source);
        return expected.All(set.Contains);
    }
}
