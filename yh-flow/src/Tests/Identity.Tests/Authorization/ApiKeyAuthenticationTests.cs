using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using YH.Framework.Shared.Constants;
using YH.Modules.Identity.Authorization.ApiKey;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;

namespace Identity.Tests.Authorization;

public sealed class ApiKeyAuthenticationTests
{
    [Fact]
    public void ApiKeyAuthenticationDefaults_Scheme_Should_Be_ApiKey()
    {
        ApiKeyAuthenticationDefaults.AuthenticationScheme.ShouldBe("ApiKey");
    }

    [Fact]
    public void ApiKeyAuthenticationOptions_DefaultHeaderName_Should_Be_XApiKey()
    {
        var options = new ApiKeyAuthenticationOptions();
        options.HeaderName.ShouldBe("X-Api-Key");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_NoApiKeyHeader_ReturnsNoResult()
    {
        var context = CreateHttpContext();
        var handler = await CreateHandlerAsync(context);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.ShouldBeFalse();
        result.None.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_EmptyApiKeyHeader_ReturnsNoResult()
    {
        var context = CreateHttpContext();
        context.Request.Headers["X-Api-Key"] = string.Empty;
        var handler = await CreateHandlerAsync(context);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.ShouldBeFalse();
        result.None.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhitespaceOnlyApiKey_ReturnsNoResult()
    {
        var context = CreateHttpContext();
        context.Request.Headers["X-Api-Key"] = "   ";
        var handler = await CreateHandlerAsync(context);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.ShouldBeFalse();
        result.None.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_InvalidKey_ReturnsFail()
    {
        var apiTokenService = Substitute.For<IApiTokenService>();
        apiTokenService.ValidateAndGetOwnerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ApiKeyValidationResult?)null);

        var context = CreateHttpContext(apiTokenService);
        context.Request.Headers["X-Api-Key"] = "pk_invalid_key_12345";
        var handler = await CreateHandlerAsync(context);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.ShouldBeFalse();
        result.Failure.ShouldNotBeNull();
        result.Failure!.Message.ShouldContain("Invalid");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ValidKey_ReturnsSuccessWithCorrectClaims()
    {
        var userId = "user-123";
        var email = "test@example.com";
        var tenantId = "tenant-456";
        var permissions = new List<string> { "read", "write", "admin" };
        var tokenId = Guid.NewGuid();

        var validationResult = new ApiKeyValidationResult(
            userId, email, tenantId, permissions, tokenId);

        var apiTokenService = Substitute.For<IApiTokenService>();
        apiTokenService.ValidateAndGetOwnerAsync("pk_valid_key_12345", Arg.Any<CancellationToken>())
            .Returns(validationResult);

        var context = CreateHttpContext(apiTokenService);
        context.Request.Headers["X-Api-Key"] = "pk_valid_key_12345";
        var handler = await CreateHandlerAsync(context);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.ShouldBeTrue();
        result.Principal.ShouldNotBeNull();

        var principal = result.Principal!;
        principal.FindFirstValue(ClaimTypes.NameIdentifier).ShouldBe(userId);
        principal.FindFirstValue(ClaimTypes.Email).ShouldBe(email);
        principal.FindFirstValue(ClaimConstants.Tenant).ShouldBe(tenantId);

        var permissionClaims = principal.FindAll(ClaimConstants.Permission).ToList();
        permissionClaims.Count.ShouldBe(3);
        permissionClaims.ShouldContain(c => c.Value == "read");
        permissionClaims.ShouldContain(c => c.Value == "write");
        permissionClaims.ShouldContain(c => c.Value == "admin");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ValidKey_SetsSchemeNameAsAuthenticationType()
    {
        var validationResult = new ApiKeyValidationResult(
            "user-1", "user@test.com", "tenant-1",
            new List<string> { "read" }, Guid.NewGuid());

        var apiTokenService = Substitute.For<IApiTokenService>();
        apiTokenService.ValidateAndGetOwnerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        var context = CreateHttpContext(apiTokenService);
        context.Request.Headers["X-Api-Key"] = "pk_test_key";
        var handler = await CreateHandlerAsync(context);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.ShouldBeTrue();
        var identity = result.Principal?.Identity as ClaimsIdentity;
        identity.ShouldNotBeNull();
        identity!.AuthenticationType.ShouldBe(ApiKeyAuthenticationDefaults.AuthenticationScheme);
    }

    [Fact]
    public void ConfigureApiKeyAuth_RegistersSchemeInAuthenticationOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();

        services.ConfigureApiKeyAuth();

        var sp = services.BuildServiceProvider();
        var authOptions = sp.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        var schemeRegistration = authOptions.SchemeMap
            .FirstOrDefault(kvp => kvp.Key == ApiKeyAuthenticationDefaults.AuthenticationScheme);
        schemeRegistration.Key.ShouldBe(ApiKeyAuthenticationDefaults.AuthenticationScheme);
    }

    private static DefaultHttpContext CreateHttpContext(IApiTokenService? apiTokenService = null)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(apiTokenService ?? Substitute.For<IApiTokenService>());

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };
    }

    private static async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(HttpContext context)
    {
        var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.CurrentValue.Returns(new ApiKeyAuthenticationOptions());
        optionsMonitor.Get(Arg.Any<string?>()).Returns(new ApiKeyAuthenticationOptions());

        var loggerFactory = Substitute.For<Microsoft.Extensions.Logging.ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<Microsoft.Extensions.Logging.ILogger>());
        var encoder = System.Text.Encodings.Web.UrlEncoder.Default;

        var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder);

        await handler.InitializeAsync(
            new AuthenticationScheme(
                ApiKeyAuthenticationDefaults.AuthenticationScheme,
                ApiKeyAuthenticationDefaults.AuthenticationScheme,
                typeof(ApiKeyAuthenticationHandler)),
            context);

        return handler;
    }
}
