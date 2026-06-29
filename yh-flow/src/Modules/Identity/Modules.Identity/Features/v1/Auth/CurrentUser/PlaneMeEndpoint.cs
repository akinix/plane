using YH.Framework.Core.Context;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.Auth.CurrentUser;

public static class PlaneMeEndpoint
{
    internal static RouteHandlerBuilder MapPlaneMeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/me", Handler);
    }

    private static async Task<Results<Ok<PlaneUserProfile>, UnauthorizedHttpResult>> Handler(
        ICurrentUser currentUser,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated())
        {
            return TypedResults.Unauthorized();
        }

        var userId = currentUser.GetUserId().ToString();
        var user = await userService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        var profile = PlaneAuthHelpers.CreateUserProfile(user);
        return TypedResults.Ok(profile);
    }
}
