using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YH.Modules.Identity.Authorization.ApiKey;
using YH.Modules.Identity.Authorization.Jwt;
using YH.Modules.Identity.Authorization.SessionCookie;

namespace Identity.Tests.Authorization;

public sealed class MultiSchemeSelectorTests
{
    [Fact]
    public void ConfigureJwtAuth_SetsSmartSelectorAsDefaultScheme()
    {
        using var serviceProvider = CreateServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        options.DefaultAuthenticateScheme.ShouldBe("SmartSelector");
        options.DefaultChallengeScheme.ShouldBe("SmartSelector");
        options.SchemeMap.ShouldContainKey("SmartSelector");
    }

    [Fact]
    public void ForwardDefaultSelector_WithApiKeyHeader_ReturnsApiKeyScheme()
    {
        using var serviceProvider = CreateServiceProvider();
        var selector = GetForwardDefaultSelector(serviceProvider);
        var context = new DefaultHttpContext();
        context.Request.Headers[ApiKeyAuthenticationDefaults.HeaderName] = "pk_test_key";

        var scheme = selector(context);

        scheme.ShouldBe(ApiKeyAuthenticationDefaults.AuthenticationScheme);
    }

    [Fact]
    public void ForwardDefaultSelector_WithSessionCookie_ReturnsSessionCookieScheme()
    {
        using var serviceProvider = CreateServiceProvider();
        var selector = GetForwardDefaultSelector(serviceProvider);
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{SessionCookieAuthenticationDefaults.CookieName}=session-value";

        var scheme = selector(context);

        scheme.ShouldBe(SessionCookieAuthenticationDefaults.AuthenticationScheme);
    }

    [Fact]
    public void ForwardDefaultSelector_WithBearerRequest_ReturnsJwtBearerScheme()
    {
        using var serviceProvider = CreateServiceProvider();
        var selector = GetForwardDefaultSelector(serviceProvider);
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token-value";

        var scheme = selector(context);

        scheme.ShouldBe(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void ForwardDefaultSelector_WithApiKeyAndSessionCookie_PrioritizesApiKeyScheme()
    {
        using var serviceProvider = CreateServiceProvider();
        var selector = GetForwardDefaultSelector(serviceProvider);
        var context = new DefaultHttpContext();
        context.Request.Headers[ApiKeyAuthenticationDefaults.HeaderName] = "pk_test_key";
        context.Request.Headers.Cookie = $"{SessionCookieAuthenticationDefaults.CookieName}=session-value";

        var scheme = selector(context);

        scheme.ShouldBe(ApiKeyAuthenticationDefaults.AuthenticationScheme);
    }

    private static Func<HttpContext, string?> GetForwardDefaultSelector(ServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<PolicySchemeOptions>>()
            .Get("SmartSelector");

        options.ForwardDefaultSelector.ShouldNotBeNull();
        return options.ForwardDefaultSelector!;
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureJwtAuth();
        services.ConfigureApiKeyAuth();
        services.ConfigureSessionCookieAuth();
        return services.BuildServiceProvider();
    }
}
