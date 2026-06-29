using System.Security.Claims;
using YH.Framework.Shared.Constants;
using YH.Modules.Identity.Authorization.SessionCookie;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Features.v1.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Identity.Tests.Features;

/// <summary>
/// Tests for PlaneAuthHelpers static methods — profile creation, request origin, and session cookie sign-in.
/// </summary>
public sealed class PlaneAuthHelpersTests
{
    #region CreateUserProfile(UserDto) Tests

    [Fact]
    public void CreateUserProfile_FromUserDto_Should_MapAllFields()
    {
        // Arrange
        var user = new UserDto
        {
            Id = "user-123",
            Email = "alice@example.com",
            FirstName = "Alice",
            LastName = "Smith",
            ImageUrl = "https://cdn.example.com/avatar.png",
            EmailConfirmed = true,
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(user);

        // Assert
        profile.Id.ShouldBe("user-123");
        profile.Email.ShouldBe("alice@example.com");
        profile.FirstName.ShouldBe("Alice");
        profile.LastName.ShouldBe("Smith");
        profile.Avatar.ShouldBe("https://cdn.example.com/avatar.png");
        profile.IsEmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void CreateUserProfile_FromUserDto_Should_NullOutEmptyOptionalFields()
    {
        // Arrange
        var user = new UserDto
        {
            Id = "user-456",
            Email = "bob@example.com",
            FirstName = null,
            LastName = "",
            ImageUrl = "  ",
            EmailConfirmed = false,
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(user);

        // Assert
        profile.Id.ShouldBe("user-456");
        profile.Email.ShouldBe("bob@example.com");
        profile.FirstName.ShouldBeNull();
        profile.LastName.ShouldBeNull();
        profile.Avatar.ShouldBeNull();
        profile.IsEmailVerified.ShouldBeFalse();
    }

    [Fact]
    public void CreateUserProfile_FromUserDto_Should_HandleNullId()
    {
        // Arrange
        var user = new UserDto { Id = null, Email = null };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(user);

        // Assert
        profile.Id.ShouldBe(string.Empty);
        profile.Email.ShouldBe(string.Empty);
    }

    [Fact]
    public void CreateUserProfile_FromUserDto_Should_ThrowOnNull()
    {
        Should.Throw<ArgumentNullException>(() => PlaneAuthHelpers.CreateUserProfile((UserDto)null!));
    }

    #endregion

    #region CreateUserProfile(IEnumerable<Claim>, fallbackEmail) Tests

    [Fact]
    public void CreateUserProfile_FromClaims_Should_MapStandardJwtClaims()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "sub-123"),
            new Claim(ClaimTypes.Email, "carol@example.com"),
            new Claim(ClaimTypes.Name, "Carol"),
            new Claim(ClaimTypes.Surname, "Jones"),
            new Claim(ClaimConstants.ImageUrl, "https://cdn.example.com/carol.png"),
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(claims, "fallback@example.com");

        // Assert
        profile.Id.ShouldBe("sub-123");
        profile.Email.ShouldBe("carol@example.com");
        profile.FirstName.ShouldBe("Carol");
        profile.LastName.ShouldBe("Jones");
        profile.Avatar.ShouldBe("https://cdn.example.com/carol.png");
        profile.IsEmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void CreateUserProfile_FromClaims_Should_UseJwtSubClaim_When_NameIdentifierMissing()
    {
        // Arrange — uses "sub" (JwtRegisteredClaimNames.Sub) instead of ClaimTypes.NameIdentifier
        var claims = new[]
        {
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "jwt-sub-456"),
            new Claim(ClaimTypes.Email, "dave@example.com"),
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(claims, "fallback@example.com");

        // Assert
        profile.Id.ShouldBe("jwt-sub-456");
    }

    [Fact]
    public void CreateUserProfile_FromClaims_Should_UseFallbackEmail_When_NoEmailClaim()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-789"),
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(claims, "fallback@example.com");

        // Assert
        profile.Email.ShouldBe("fallback@example.com");
    }

    [Fact]
    public void CreateUserProfile_FromClaims_Should_PreferJwtEmailOverClaimTypesEmail()
    {
        // Arrange — both "email" and ClaimTypes.Email are present; JWT "email" takes priority
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-001"),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, "jwt-email@example.com"),
            new Claim(ClaimTypes.Email, "claim-email@example.com"),
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(claims, "fallback@example.com");

        // Assert
        profile.Email.ShouldBe("jwt-email@example.com");
    }

    [Fact]
    public void CreateUserProfile_FromClaims_Should_ReturnEmptyId_When_NoIdClaims()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "noname@example.com"),
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(claims, "fallback@example.com");

        // Assert
        profile.Id.ShouldBe(string.Empty);
        profile.Email.ShouldBe("noname@example.com");
    }

    [Fact]
    public void CreateUserProfile_FromClaims_Should_NullOutMissingOptionalFields()
    {
        // Arrange — no name, surname, or image claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-min"),
            new Claim(ClaimTypes.Email, "min@example.com"),
        };

        // Act
        var profile = PlaneAuthHelpers.CreateUserProfile(claims, "fallback@example.com");

        // Assert
        profile.FirstName.ShouldBeNull();
        profile.LastName.ShouldBeNull();
        profile.Avatar.ShouldBeNull();
    }

    [Fact]
    public void CreateUserProfile_FromClaims_Should_ThrowOnNullClaims()
    {
        Should.Throw<ArgumentNullException>(() =>
            PlaneAuthHelpers.CreateUserProfile((IEnumerable<Claim>)null!, "test@example.com"));
    }

    #endregion

    #region CreateRequestOrigin Tests

    [Fact]
    public void CreateRequestOrigin_Should_CombineSchemeHostAndPathBase()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("app.example.com");
        httpContext.Request.PathBase = "/api";

        // Act
        var origin = PlaneAuthHelpers.CreateRequestOrigin(httpContext);

        // Assert
        origin.ShouldBe("https://app.example.com/api");
    }

    [Fact]
    public void CreateRequestOrigin_Should_HandleEmptyPathBase()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost", 5030);
        httpContext.Request.PathBase = PathString.Empty;

        // Act
        var origin = PlaneAuthHelpers.CreateRequestOrigin(httpContext);

        // Assert
        origin.ShouldBe("http://localhost:5030");
    }

    [Fact]
    public void CreateRequestOrigin_Should_ThrowOnNullHttpContext()
    {
        Should.Throw<ArgumentNullException>(() => PlaneAuthHelpers.CreateRequestOrigin(null!));
    }

    #endregion

    #region SignInSessionCookieAsync Tests

    [Fact]
    public async Task SignInSessionCookieAsync_Should_BuildCorrectClaims()
    {
        // Arrange — use a mock IAuthenticationService to capture what SignInAsync passes
        ClaimsPrincipal? capturedPrincipal = null;
        string? capturedScheme = null;
        var authService = Substitute.For<IAuthenticationService>();
        await authService.SignInAsync(
            Arg.Any<HttpContext>(),
            Arg.Do<string>(s => capturedScheme = s),
            Arg.Do<ClaimsPrincipal>(p => capturedPrincipal = p),
            Arg.Any<AuthenticationProperties>());

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };

        var user = new PlaneUserProfile(
            Id: "user-abc",
            Email: "eve@example.com",
            FirstName: "Eve",
            LastName: "Adams",
            Avatar: null,
            IsEmailVerified: true);

        // Act
        await PlaneAuthHelpers.SignInSessionCookieAsync(httpContext, user);

        // Assert
        capturedScheme.ShouldBe(SessionCookieAuthenticationDefaults.AuthenticationScheme);
        capturedPrincipal.ShouldNotBeNull();
        capturedPrincipal!.FindFirstValue(ClaimTypes.NameIdentifier).ShouldBe("user-abc");
        capturedPrincipal.FindFirstValue(ClaimTypes.Email).ShouldBe("eve@example.com");
        capturedPrincipal.FindFirstValue(ClaimTypes.Name).ShouldBe("Eve");
        capturedPrincipal.FindFirstValue(ClaimTypes.Surname).ShouldBe("Adams");
    }

    [Fact]
    public async Task SignInSessionCookieAsync_Should_OmitEmptyOptionalClaims()
    {
        // Arrange
        ClaimsPrincipal? capturedPrincipal = null;
        var authService = Substitute.For<IAuthenticationService>();
        await authService.SignInAsync(
            Arg.Any<HttpContext>(),
            Arg.Any<string>(),
            Arg.Do<ClaimsPrincipal>(p => capturedPrincipal = p),
            Arg.Any<AuthenticationProperties>());

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };

        var user = new PlaneUserProfile(
            Id: "user-minimal",
            Email: "minimal@example.com",
            FirstName: null,
            LastName: null,
            Avatar: null,
            IsEmailVerified: false);

        // Act
        await PlaneAuthHelpers.SignInSessionCookieAsync(httpContext, user);

        // Assert — only NameIdentifier and Email claims; Name and Surname should NOT be present
        capturedPrincipal.ShouldNotBeNull();
        capturedPrincipal!.FindFirstValue(ClaimTypes.NameIdentifier).ShouldBe("user-minimal");
        capturedPrincipal.FindFirstValue(ClaimTypes.Email).ShouldBe("minimal@example.com");
        capturedPrincipal.FindFirst(ClaimTypes.Name).ShouldBeNull();
        capturedPrincipal.FindFirst(ClaimTypes.Surname).ShouldBeNull();
    }

    [Fact]
    public async Task SignInSessionCookieAsync_Should_UseSessionCookieScheme()
    {
        // Arrange
        string? capturedScheme = null;
        var authService = Substitute.For<IAuthenticationService>();
        await authService.SignInAsync(
            Arg.Any<HttpContext>(),
            Arg.Do<string>(s => capturedScheme = s),
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<AuthenticationProperties>());

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };

        var user = new PlaneUserProfile(
            Id: "user-scheme",
            Email: "scheme@example.com",
            FirstName: null,
            LastName: null,
            Avatar: null,
            IsEmailVerified: true);

        // Act
        await PlaneAuthHelpers.SignInSessionCookieAsync(httpContext, user);

        // Assert — verify the SessionCookie scheme is used
        capturedScheme.ShouldBe(SessionCookieAuthenticationDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task SignInSessionCookieAsync_Should_CreateIdentityWithCorrectAuthType()
    {
        // Arrange
        ClaimsPrincipal? capturedPrincipal = null;
        var authService = Substitute.For<IAuthenticationService>();
        await authService.SignInAsync(
            Arg.Any<HttpContext>(),
            Arg.Any<string>(),
            Arg.Do<ClaimsPrincipal>(p => capturedPrincipal = p),
            Arg.Any<AuthenticationProperties>());

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };

        var user = new PlaneUserProfile(
            Id: "user-identity",
            Email: "identity@example.com",
            FirstName: "Test",
            LastName: null,
            Avatar: null,
            IsEmailVerified: true);

        // Act
        await PlaneAuthHelpers.SignInSessionCookieAsync(httpContext, user);

        // Assert — verify the identity uses the SessionCookie authentication type
        capturedPrincipal.ShouldNotBeNull();
        var identity = capturedPrincipal!.Identity as ClaimsIdentity;
        identity.ShouldNotBeNull();
        identity!.AuthenticationType.ShouldBe(SessionCookieAuthenticationDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task SignInSessionCookieAsync_Should_ThrowOnNullArgs()
    {
        var user = new PlaneUserProfile("id", "email@test.com", null, null, null, true);

        await Should.ThrowAsync<ArgumentNullException>(
            () => PlaneAuthHelpers.SignInSessionCookieAsync(null!, user));
        await Should.ThrowAsync<ArgumentNullException>(
            () => PlaneAuthHelpers.SignInSessionCookieAsync(new DefaultHttpContext(), null!));
    }

    #endregion
}
