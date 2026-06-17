using YH.Framework.Core.Domain;
using YH.Modules.Identity;
using YH.Modules.Identity.Data.Configurations;
using YH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Tests.Domain;

/// <summary>
/// Tests for APIToken domain entity — factory method, behavior methods, and EF configuration.
/// </summary>
public sealed class APITokenTests
{
    private const string TestName = "My CI Key";
    private const string TestUserId = "user-123";
    private const string TestTokenHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private const string TestPrefix = "pk_abc1";
    private const string TestTenantId = "tenant-456";

    #region Factory Method Tests

    [Fact]
    public void Create_Should_SetAllProperties_And_ActiveIsTrue_LastUsedIsNull()
    {
        // Arrange
        var expiredAt = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, expiredAt);

        // Assert
        token.Id.ShouldNotBe(Guid.Empty);
        token.Name.ShouldBe(TestName);
        token.UserId.ShouldBe(TestUserId);
        token.TokenHash.ShouldBe(TestTokenHash);
        token.Prefix.ShouldBe(TestPrefix);
        token.TenantId.ShouldBe(TestTenantId);
        token.ExpiredAt.ShouldBe(expiredAt);
        token.IsActive.ShouldBeTrue();
        token.LastUsed.ShouldBeNull();
        token.CreatedAt.ShouldNotBe(default);
    }

    #endregion

    #region RecordUsage Tests

    [Fact]
    public void RecordUsage_Should_SetLastUsed_ToCurrentUtcTime()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, null);

        // Act
        token.RecordUsage();

        // Assert
        token.LastUsed.ShouldNotBeNull();
        token.LastUsed!.Value.ShouldBeGreaterThanOrEqualTo(before.AddSeconds(-1));
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_Should_SetIsActive_ToFalse()
    {
        // Arrange
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, null);
        token.IsActive.ShouldBeTrue();

        // Act
        token.Revoke();

        // Assert
        token.IsActive.ShouldBeFalse();
    }

    #endregion

    #region IsExpired Tests

    [Fact]
    public void IsExpired_Should_ReturnTrue_When_ExpiredAtIsInThePast()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, pastDate);

        // Act & Assert
        token.IsExpired().ShouldBeTrue();
    }

    [Fact]
    public void IsExpired_Should_ReturnFalse_When_ExpiredAtIsNull()
    {
        // Arrange — null means never expires
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, null);

        // Act & Assert
        token.IsExpired().ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_Should_ReturnFalse_When_ExpiredAtIsInTheFuture()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(30);
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, futureDate);

        // Act & Assert
        token.IsExpired().ShouldBeFalse();
    }

    #endregion

    #region Interface Tests

    [Fact]
    public void APIToken_Should_Implement_IHasTenant()
    {
        // Arrange & Act
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, null);

        // Assert
        token.ShouldBeAssignableTo<IHasTenant>();
        token.TenantId.ShouldBe(TestTenantId);
    }

    [Fact]
    public void APIToken_Should_Implement_IHasDomainEvents()
    {
        // Arrange & Act
        var token = APIToken.Create(TestName, TestUserId, TestTokenHash, TestPrefix, TestTenantId, null);

        // Assert
        token.ShouldBeAssignableTo<IHasDomainEvents>();
    }

    #endregion

    #region EF Configuration Tests

    [Fact]
    public void APITokenConfiguration_Should_MapToIdentitySchema_And_ApiTokensTable()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<APITokenTestDbContext>()
            .UseInMemoryDatabase(databaseName: $"APITokenConfigTest_{Guid.NewGuid()}")
            .Options;

        // Act
        using var context = new APITokenTestDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(APIToken));

        // Assert
        entityType.ShouldNotBeNull();
        entityType.GetSchema().ShouldBe(IdentityModuleConstants.SchemaName);
        entityType.GetTableName().ShouldBe("ApiTokens");
    }

    #endregion
}

/// <summary>
/// Minimal DbContext for testing APIToken EF configuration.
/// </summary>
public sealed class APITokenTestDbContext : DbContext
{
    public DbSet<APIToken> ApiTokens => Set<APIToken>();

    public APITokenTestDbContext(DbContextOptions<APITokenTestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new APITokenConfiguration());
    }
}
