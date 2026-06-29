using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.v1.ApiTokens;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.ApiTokens.List;

public static class ListApiTokensEndpoint
{
    internal static RouteHandlerBuilder MapListApiTokensEndpoint(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapGet("/api-tokens", Handler)
            .WithName("ListApiTokens")
            .WithSummary("List API tokens")
            .WithDescription("List all API tokens for the current user. Token hashes are never returned.")
            .Produces<IReadOnlyList<APITokenDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Ok<IReadOnlyList<APITokenDto>>> Handler(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tokens = await mediator.Send(new ListApiTokensQuery(), cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(tokens);
    }
}
