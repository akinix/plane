using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.v1.Auth;
using YH.Modules.Identity.Contracts.v1.Users.RegisterUser;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.Auth.SignUp;

public static class PlaneSignUpEndpoint
{
    internal static RouteHandlerBuilder MapPlaneSignUpEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/sign-up", Handler);
    }

    private static async Task<Results<Ok<PlaneAuthResponse>, BadRequest<PlaneAuthError>>> Handler(
        [FromBody] PlaneSignUpCommand command,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var origin = PlaneAuthHelpers.CreateRequestOrigin(httpContext);

            var registerCommand = new RegisterUserCommand
            {
                FirstName = command.FirstName ?? string.Empty,
                LastName = command.LastName ?? string.Empty,
                Email = command.Email,
                UserName = command.Email,
                Password = command.Password,
                ConfirmPassword = command.Password,
                PhoneNumber = string.Empty,
                Origin = origin,
            };

            await mediator.Send(registerCommand, cancellationToken).ConfigureAwait(false);

            var token = await mediator.Send(
                new Contracts.v1.Tokens.TokenGeneration.GenerateTokenCommand(command.Email, command.Password),
                cancellationToken).ConfigureAwait(false);

            var user = PlaneAuthHelpers.CreateUserProfile(token, command.Email);
            await PlaneAuthHelpers.SignInSessionCookieAsync(httpContext, user).ConfigureAwait(false);

            return TypedResults.Ok(new PlaneAuthResponse(
                token.AccessToken,
                token.RefreshToken,
                token.AccessTokenExpiresAt,
                user));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new PlaneAuthError(
                ex.Message,
                "registration_failed"));
        }
    }
}
