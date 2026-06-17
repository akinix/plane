using YH.Modules.Identity.Contracts.v1.ApiTokens;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.ApiTokens.Create;

public static class CreateApiTokenEndpoint
{
    internal static RouteHandlerBuilder MapCreateApiTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapPost("/api-tokens", Handler)
            .WithName("CreateApiToken")
            .WithSummary("Create a new API token")
            .WithDescription("Create a new API token for the current user. The plaintext token is returned only once — store it securely.")
            .Produces<ApiTokenCreateResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handler(
        CreateApiTokenCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Results.BadRequest(new { error = "Token name is required." });
        }

        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Results.Created($"/auth/api-tokens/{result.Id}", result);
    }
}
