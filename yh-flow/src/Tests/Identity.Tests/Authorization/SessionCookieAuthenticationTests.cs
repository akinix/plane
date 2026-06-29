using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YH.Modules.Identity.Authorization.SessionCookie;

namespace Identity.Tests.Authorization;

public sealed class SessionCookieAuthenticationTests
{
    [Fact]
    public void SessionCookieAuthenticationDefaults_Scheme_Should_Be_SessionCookie()
    {
        SessionCookieAuthenticationDefaults.AuthenticationScheme.ShouldBe("SessionCookie");
    }

    [Fact]
    public void SessionCookieAuthenticationDefaults_CookieName_Should_Be_YHFlowSession()
    {
        SessionCookieAuthenticationDefaults.CookieName.ShouldBe(".YHFlow.Session");
    }

    [Fact]
    public void ConfigureSessionCookieAuth_RegistersSessionCookieScheme()
    {
        using var serviceProvider = CreateServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        options.SchemeMap.ShouldContainKey(SessionCookieAuthenticationDefaults.AuthenticationScheme);
        options.SchemeMap[SessionCookieAuthenticationDefaults.AuthenticationScheme]
            .HandlerType.ShouldBe(typeof(CookieAuthenticationHandler));
    }

    [Fact]
    public void ConfigureSessionCookieAuth_ConfiguresCookieSecurityOptions()
    {
        using var serviceProvider = CreateServiceProvider();
        var cookieOptions = GetCookieOptions(serviceProvider);

        cookieOptions.Cookie.Name.ShouldBe(SessionCookieAuthenticationDefaults.CookieName);
        cookieOptions.Cookie.HttpOnly.ShouldBeTrue();
        cookieOptions.Cookie.SameSite.ShouldBe(SameSiteMode.Lax);
        cookieOptions.Cookie.SecurePolicy.ShouldBe(CookieSecurePolicy.Always);
    }

    [Fact]
    public void ConfigureSessionCookieAuth_ConfiguresExpirationOptions()
    {
        using var serviceProvider = CreateServiceProvider();
        var cookieOptions = GetCookieOptions(serviceProvider);

        cookieOptions.ExpireTimeSpan.ShouldBe(TimeSpan.FromDays(7));
        cookieOptions.SlidingExpiration.ShouldBeTrue();
    }

    [Fact]
    public async Task ConfigureSessionCookieAuth_OnRedirectToLogin_ReturnsUnauthorizedInsteadOfRedirect()
    {
        using var serviceProvider = CreateServiceProvider();
        var cookieOptions = GetCookieOptions(serviceProvider);
        var context = new RedirectContext<CookieAuthenticationOptions>(
            new DefaultHttpContext(),
            new AuthenticationScheme(
                SessionCookieAuthenticationDefaults.AuthenticationScheme,
                SessionCookieAuthenticationDefaults.AuthenticationScheme,
                typeof(CookieAuthenticationHandler)),
            cookieOptions,
            new AuthenticationProperties(),
            "/login");

        await cookieOptions.Events.OnRedirectToLogin(context);

        context.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    private static CookieAuthenticationOptions GetCookieOptions(ServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(SessionCookieAuthenticationDefaults.AuthenticationScheme);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureSessionCookieAuth();
        return services.BuildServiceProvider();
    }
}
