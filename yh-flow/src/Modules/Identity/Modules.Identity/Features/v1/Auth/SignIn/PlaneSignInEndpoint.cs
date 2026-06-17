using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using YH.Modules.Identity.Contracts.v1.Auth;
using YH.Modules.Identity.Contracts.v1.Tokens.TokenGeneration;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.Auth.SignIn;

public static class PlaneSignInEndpoint
{
    internal static RouteHandlerBuilder MapPlaneSignInEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/sign-in", Handler);
    }

    private static async Task<Results<Ok<PlaneAuthResponse>, UnauthorizedHttpResult>> Handler(
        [FromBody] PlaneSignInCommand command,
        HttpContext httpContext,
        IMediator mediator,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        var validation = await identityService.ValidateCredentialsAsync(
            command.Email,
            command.Password,
            twoFactorCode: null,
            ct: cancellationToken).ConfigureAwait(false);

        if (validation is null)
        {
            return TypedResults.Unauthorized();
        }

        var (subject, _) = validation.Value;

        var token = await mediator.Send(
            new GenerateTokenCommand(command.Email, command.Password),
            cancellationToken).ConfigureAwait(false);

        var user = PlaneAuthHelpers.CreateUserProfile(token, command.Email);
        await PlaneAuthHelpers.SignInSessionCookieAsync(httpContext, user).ConfigureAwait(false);

        await identityService.StoreRefreshTokenAsync(
            subject,
            token.RefreshToken,
            token.RefreshTokenExpiresAt,
            cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(new PlaneAuthResponse(
            token.AccessToken,
            token.RefreshToken,
            token.AccessTokenExpiresAt,
            user));
    }
}
