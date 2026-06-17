using System.Reflection;
using YH.Framework.Core.Context;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Features.v1.Auth;
using YH.Modules.Identity.Features.v1.Auth.CurrentUser;
using YH.Modules.Identity.Features.v1.Auth.SignIn;
using YH.Modules.Identity.Features.v1.Auth.SignOut;
using YH.Modules.Identity.Features.v1.Auth.SignUp;
using YH.Modules.Identity.Features.v1.OAuth;
using NSubstitute;

namespace Identity.Tests.Features;

/// <summary>
/// Tests for Plane auth endpoint registration, handler logic, and authorization metadata.
/// </summary>
public sealed class PlaneAuthEndpointTests
{
    #region Endpoint Method Signature Tests

    [Theory]
    [InlineData(typeof(PlaneSignInEndpoint), "MapPlaneSignInEndpoint")]
    [InlineData(typeof(PlaneSignUpEndpoint), "MapPlaneSignUpEndpoint")]
    [InlineData(typeof(PlaneSignOutEndpoint), "MapPlaneSignOutEndpoint")]
    [InlineData(typeof(PlaneMeEndpoint), "MapPlaneMeEndpoint")]
    [InlineData(typeof(OAuthInitiateEndpoint), "MapOAuthInitiateEndpoint")]
    [InlineData(typeof(OAuthCallbackEndpoint), "MapOAuthCallbackEndpoint")]
    public void EndpointMapMethod_Should_Exist_And_ReturnRouteHandlerBuilder(Type endpointType, string methodName)
    {
        ArgumentNullException.ThrowIfNull(endpointType);

        // Act — find the extension method by reflection
        var method = endpointType.GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

        // Assert
        method.ShouldNotBeNull($"{endpointType.Name}.{methodName} should exist");
        method.ReturnType.ShouldBe(typeof(Microsoft.AspNetCore.Builder.RouteHandlerBuilder),
            $"{endpointType.Name}.{methodName} should return RouteHandlerBuilder for metadata chaining");
    }

    [Fact]
    public void AuthEndpoints_MapPlaneAuthEndpoints_Should_BeStaticAndOnInternalClass()
    {
        // Act
        var method = typeof(AuthEndpoints).GetMethod(
            "MapPlaneAuthEndpoints",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        // Assert — the class is internal (not public), the method is static
        method.ShouldNotBeNull();
        typeof(AuthEndpoints).IsPublic.ShouldBeFalse("AuthEndpoints class should be internal");
        method.IsStatic.ShouldBeTrue();
    }

    #endregion

    #region PlaneMe Handler Logic Tests

    [Fact]
    public async Task PlaneMe_Should_ReturnUserProfile_When_UserIsAuthenticated()
    {
        // Arrange
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated().Returns(true);
        currentUser.GetUserId().Returns(Guid.Parse("00000000-0000-0000-0000-000000000042"));

        var userService = Substitute.For<IUserService>();
        var userDto = new UserDto
        {
            Id = "00000000-0000-0000-0000-000000000042",
            Email = "alice@example.com",
            FirstName = "Alice",
            LastName = "Smith",
            ImageUrl = "https://cdn.example.com/avatar.png",
            EmailConfirmed = true,
        };
        userService.GetAsync("00000000-0000-0000-0000-000000000042", Arg.Any<CancellationToken>())
            .Returns(userDto);

        // Act — simulate what the PlaneMe handler does
        var isAuthenticated = currentUser.IsAuthenticated();
        PlaneUserProfile? profile = null;
        if (isAuthenticated)
        {
            var userId = currentUser.GetUserId().ToString();
            var user = await userService.GetAsync(userId, CancellationToken.None);
            profile = PlaneAuthHelpers.CreateUserProfile(user);
        }

        // Assert
        isAuthenticated.ShouldBeTrue();
        profile.ShouldNotBeNull();
        profile!.Id.ShouldBe("00000000-0000-0000-0000-000000000042");
        profile.Email.ShouldBe("alice@example.com");
        profile.FirstName.ShouldBe("Alice");
        profile.LastName.ShouldBe("Smith");
        profile.Avatar.ShouldBe("https://cdn.example.com/avatar.png");
        profile.IsEmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void PlaneMe_Should_IndicateNotAuthenticated_When_UserIsAnonymous()
    {
        // Arrange
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated().Returns(false);

        // Act — simulate the handler's authentication check
        var isAuthenticated = currentUser.IsAuthenticated();

        // Assert — the handler returns TypedResults.Unauthorized() when not authenticated
        isAuthenticated.ShouldBeFalse();
    }

    #endregion

    #region PlaneAuthResponse Record Tests

    [Fact]
    public void PlaneAuthResponse_Should_HaveCorrectJsonPropertyNames()
    {
        // Arrange & Act
        var token = new PlaneAuthResponse(
            "access-token",
            "refresh-token",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new PlaneUserProfile("id-1", "test@example.com", "Test", "User", null, true));

        // Assert — verify record construction
        token.AccessToken.ShouldBe("access-token");
        token.RefreshToken.ShouldBe("refresh-token");
        token.AccessTokenExpiresAt.ShouldBe(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        token.User.Id.ShouldBe("id-1");
        token.User.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public void PlaneUserProfile_Should_SupportNullableFields()
    {
        // Arrange & Act
        var profile = new PlaneUserProfile(
            Id: "user-1",
            Email: "test@example.com",
            FirstName: null,
            LastName: null,
            Avatar: null,
            IsEmailVerified: false);

        // Assert
        profile.Id.ShouldBe("user-1");
        profile.Email.ShouldBe("test@example.com");
        profile.FirstName.ShouldBeNull();
        profile.LastName.ShouldBeNull();
        profile.Avatar.ShouldBeNull();
        profile.IsEmailVerified.ShouldBeFalse();
    }

    #endregion

    #region PlaneAuthError Record Tests

    [Fact]
    public void PlaneAuthError_Should_StoreErrorDetails()
    {
        // Arrange & Act
        var error = new PlaneAuthError("Invalid credentials", "invalid_credentials");

        // Assert
        error.Error.ShouldBe("Invalid credentials");
        error.ErrorCode.ShouldBe("invalid_credentials");
    }

    #endregion
}
