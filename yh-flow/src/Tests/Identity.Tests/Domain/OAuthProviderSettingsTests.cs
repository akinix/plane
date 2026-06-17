using YH.Framework.Core.Domain;
using YH.Modules.Identity;
using YH.Modules.Identity.Data;
using YH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Tests.Domain;

/// <summary>
/// Tests for OAuthProviderSettings domain entity — factory method, behavior methods, and EF configuration.
/// </summary>
public sealed class OAuthProviderSettingsTests
{
    private const string TestProviderName = "github";
    private const string TestClientId = "client-123";
    private const string TestClientSecret = "super-secret-456";
    private const string TestCallbackUrl = "https://example.com/auth/oauth/github/callback";
    private const bool TestAutoCreateAccount = true;
    private const string? TestScope = "read:user,user:email";

    #region Factory Method Tests

    [Fact]
    public void Create_Should_InitializeAllProperties_And_DefaultEnabledFalse()
    {
        // Act
        var settings = OAuthProviderSettings.Create(
            TestProviderName, TestClientId, TestClientSecret,
            TestCallbackUrl, TestAutoCreateAccount, TestScope);

        // Assert
        settings.Id.ShouldNotBe(Guid.Empty);
        settings.ProviderName.ShouldBe(TestProviderName);
        settings.ClientId.ShouldBe(TestClientId);
        settings.ClientSecret.ShouldBe(TestClientSecret);
        settings.CallbackUrl.ShouldBe(TestCallbackUrl);
        settings.AutoCreateAccount.ShouldBe(TestAutoCreateAccount);
        settings.Scope.ShouldBe(TestScope);
        settings.Enabled.ShouldBeFalse(); // default is disabled
    }

    [Fact]
    public void Create_Should_HandleNullScope()
    {
        // Act
        var settings = OAuthProviderSettings.Create(
            TestProviderName, TestClientId, TestClientSecret,
            TestCallbackUrl, false, null);

        // Assert
        settings.Scope.ShouldBeNull();
    }

    #endregion

    #region ToggleEnabled Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToggleEnabled_Should_UpdateEnabledStatus(bool enabled)
    {
        // Arrange
        var settings = OAuthProviderSettings.Create(
            TestProviderName, TestClientId, TestClientSecret,
            TestCallbackUrl, false, null);

        // Act
        settings.ToggleEnabled(enabled);

        // Assert
        settings.Enabled.ShouldBe(enabled);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_Should_UpdateAllMutableProperties()
    {
        // Arrange
        var settings = OAuthProviderSettings.Create(
            TestProviderName, TestClientId, TestClientSecret,
            TestCallbackUrl, false, null);

        var newClientId = "new-client-789";
        var newSecret = "new-secret-abc";
        var newCallback = "https://new.example.com/callback";
        var newAutoCreate = false;
        var newScope = "repo";

        // Act
        settings.Update(newClientId, newSecret, newCallback, newAutoCreate, newScope);

        // Assert
        settings.ClientId.ShouldBe(newClientId);
        settings.ClientSecret.ShouldBe(newSecret);
        settings.CallbackUrl.ShouldBe(newCallback);
        settings.AutoCreateAccount.ShouldBe(newAutoCreate);
        settings.Scope.ShouldBe(newScope);
        // ProviderName should NOT change
        settings.ProviderName.ShouldBe(TestProviderName);
    }

    #endregion

    #region Interface Tests

    [Fact]
    public void OAuthProviderSettings_Should_Implement_IGlobalEntity()
    {
        // Arrange & Act
        var settings = OAuthProviderSettings.Create(
            TestProviderName, TestClientId, TestClientSecret,
            TestCallbackUrl, false, null);

        // Assert — IGlobalEntity means NOT tenant-isolated
        settings.ShouldBeAssignableTo<IGlobalEntity>();
    }

    #endregion

    #region EF Configuration Tests

    [Fact]
    public void OAuthProviderSettingsConfiguration_Should_MapToIdentitySchema()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OAuthTestDbContext>()
            .UseInMemoryDatabase(databaseName: $"OAuthConfigTest_{Guid.NewGuid()}")
            .Options;

        // Act
        using var context = new OAuthTestDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OAuthProviderSettings));

        // Assert
        entityType.ShouldNotBeNull();
        entityType.GetSchema().ShouldBe(IdentityModuleConstants.SchemaName);
        entityType.GetTableName().ShouldBe("OAuthProviderSettings");
    }

    #endregion
}

/// <summary>
/// Minimal DbContext for testing OAuthProviderSettings EF configuration.
/// </summary>
public sealed class OAuthTestDbContext : DbContext
{
    public DbSet<OAuthProviderSettings> OAuthProviderSettings => Set<OAuthProviderSettings>();

    public OAuthTestDbContext(DbContextOptions<OAuthTestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OAuthProviderSettingsConfiguration());
    }
}
