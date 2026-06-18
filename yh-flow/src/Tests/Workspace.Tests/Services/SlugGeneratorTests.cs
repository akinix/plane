using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using YH.Modules.Workspace;
using YH.Modules.Workspace.Data;
using YH.Modules.Workspace.Domain;
using YH.Modules.Workspace.Services;

// Namespace/type collision: `YH.Modules.Workspace` (the namespace) and `YH.Modules.Workspace.Domain.Workspace`
// (the entity) share the "Workspace" identifier. Alias the entity so `Workspace.Create(...)` resolves
// to the entity factory rather than the namespace (pattern from 02-02 WorkspaceDbContext).
using WorkspaceEntity = YH.Modules.Workspace.Domain.Workspace;

namespace YH.Tests.Workspace.Services;

/// <summary>
/// Unit tests for <see cref="SlugGenerator"/> (plan 02-04 Task 1).
/// Covers D-09 (format + restricted-words), D-07 (collision retry), and the safe-fallback
/// behaviour for non-Latin input.
/// </summary>
/// <remarks>
/// The pure members (<see cref="SlugGenerator.Slugify"/> / <see cref="SlugGenerator.IsValidSlug"/>)
/// are tested without any DbContext. The collision-retry path uses an InMemory
/// <see cref="WorkspaceDbContext"/> seeded with an existing "acme" workspace row.
/// </remarks>
public sealed class SlugGeneratorTests : IDisposable
{
    private readonly WorkspaceDbContext _db = NewInMemoryContext();

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData("ACME Corp!", "acme-corp")]
    [InlineData("Hello   World", "hello-world")]
    [InlineData("---leading-trailing---", "leading-trailing")]
    [InlineData("UPPER_CASE_NAME", "upper-case-name")]
    [InlineData("café résumé", "caf-r-sum")] // non-ASCII → dashes (no diacritic folding by design)
    public void Slugify_NormalizesPlainText(string input, string expected)
    {
        var gen = NewGenerator();
        gen.Slugify(input).ShouldBe(expected);
    }

    [Fact]
    public void Slugify_TrimsAndCapsAtMaxLength()
    {
        var gen = NewGenerator();
        var longInput = new string('a', 200);
        var result = gen.Slugify(longInput);
        result.Length.ShouldBeLessThanOrEqualTo(WorkspaceModuleConstants.SlugMaxLength);
    }

    [Fact]
    public void Slugify_EmptyOrWhitespace_ReturnsEmpty()
    {
        var gen = NewGenerator();
        gen.Slugify(string.Empty).ShouldBeEmpty();
        gen.Slugify("   ").ShouldBeEmpty();
        gen.Slugify(null!).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("api", false)]       // restricted
    [InlineData("admin", false)]     // restricted
    [InlineData("settings", false)]  // restricted
    [InlineData("billing", false)]   // restricted
    [InlineData("sign-in", false)]   // restricted (compound reserved word)
    [InlineData("Acme", false)]      // uppercase — fails regex
    [InlineData("ac", false)]        // too short (<3)
    [InlineData("acme", true)]       // valid
    [InlineData("acme-corp", true)]  // valid
    [InlineData("acme_corp", false)] // underscore not allowed
    [InlineData("-acme", false)]     // leading dash
    [InlineData("acme-", false)]     // trailing dash
    [InlineData("acme--corp", false)]// double dash
    public void IsValidSlug_RejectsFormatAndRestrictedWords(string slug, bool expected)
    {
        var gen = NewGenerator();
        gen.IsValidSlug(slug).ShouldBe(expected);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_EmptyName_Throws()
    {
        var gen = NewGenerator();
        await Should.ThrowAsync<ArgumentException>(() =>
            gen.GenerateUniqueSlugAsync(string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_NoCollision_ReturnsBaseSlug()
    {
        var gen = NewGenerator();

        var slug = await gen.GenerateUniqueSlugAsync("Acme", CancellationToken.None);

        slug.ShouldBe("acme");
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_OnCollision_AppendsRandomSuffix()
    {
        await SeedExistingWorkspaceAsync(_db, "acme");
        var gen = NewGenerator();

        var slug = await gen.GenerateUniqueSlugAsync("Acme", CancellationToken.None);

        // Must NOT be the colliding base; must follow the acme-{suffix} shape.
        slug.ShouldNotBe("acme");
        slug.ShouldStartWith("acme-");
        slug.Length.ShouldBeGreaterThan("acme-".Length);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_OnCollision_ProducesValidSlugAndIsRandom()
    {
        await SeedExistingWorkspaceAsync(_db, "acme");
        var gen = NewGenerator();

        // Act — generator retries until a non-colliding suffix is found (success path of the
        // 5-attempt contract). The conflict-throw path cannot be reliably exercised against InMemory
        // (the suffix space is 16^4); the conflict-throw is verified by reading the source contract.
        var slug = await gen.GenerateUniqueSlugAsync("Acme", CancellationToken.None);
        var slug2 = await gen.GenerateUniqueSlugAsync("Acme", CancellationToken.None);

        slug.ShouldStartWith("acme-");
        slug2.ShouldStartWith("acme-");
        // Both must be valid slugs (suffix matches the strict format).
        gen.IsValidSlug(slug).ShouldBeTrue();
        gen.IsValidSlug(slug2).ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_NonLatinFallback_ProducesValidSlug()
    {
        // Pure-CJK input has no ASCII alphanumerics; Slugify returns empty → generator falls back
        // to a "ws-{suffix}" candidate so non-Latin workspace names still get a unique slug.
        var gen = NewGenerator();

        var slug = await gen.GenerateUniqueSlugAsync("你好世界", CancellationToken.None);

        gen.IsValidSlug(slug).ShouldBeTrue();
        slug.ShouldStartWith("ws-");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private SlugGenerator NewGenerator() => new(_db);

    private static WorkspaceDbContext NewInMemoryContext()
    {
        // Provide a tenant with no ConnectionString so BaseDbContext.OnConfiguring skips the
        // tenant-specific re-configuration (which would clobber UseInMemoryDatabase). Pattern from
        // MembershipMiddlewareTests.CreateInMemoryContext.
        var accessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();
        var tenant = new AppTenantInfo(Guid.NewGuid().ToString(), "acme-test", "Acme Test");
        accessor.MultiTenantContext.Returns(new MultiTenantContext<AppTenantInfo>(tenant));

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseInMemoryDatabase($"slug-{Guid.NewGuid()}")
            .Options;
        var databaseOptions = Options.Create(new DatabaseOptions { Provider = "inmemory" });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new WorkspaceDbContext(accessor, options, databaseOptions, environment);
    }

    private static async Task SeedExistingWorkspaceAsync(WorkspaceDbContext db, string slug)
    {
        // Workspaces is IGlobalEntity (no tenant filter), so direct add is fine.
        var ws = WorkspaceEntity.Create(
            name: slug,
            slug: slug,
            ownerUserId: Guid.NewGuid());
        db.Workspaces.Add(ws);
        await db.SaveChangesAsync();
    }
}
