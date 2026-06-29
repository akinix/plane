using YH.Modules.Identity.Contracts.v1.ApiTokens;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.ApiTokens.Revoke;

public static class RevokeApiTokenEndpoint
{
    internal static RouteHandlerBuilder MapRevokeApiTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapDelete("/api-tokens/{id:guid}", Handler)
            .WithName("RevokeApiToken")
            .WithSummary("Revoke an API token")
            .WithDescription("Revoke an API token. Only the token owner can revoke their own tokens.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, NotFound>> Handler(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RevokeApiTokenCommand(id), cancellationToken).ConfigureAwait(false);
        return result ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
