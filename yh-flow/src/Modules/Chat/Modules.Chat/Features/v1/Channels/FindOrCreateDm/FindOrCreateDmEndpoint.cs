using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Chat.Contracts.Authorization;
using YH.Modules.Chat.Contracts.v1.Commands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Chat.Features.v1.Channels.FindOrCreateDm;

public static class FindOrCreateDmEndpoint
{
    internal static RouteHandlerBuilder MapFindOrCreateDmEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/dms",
                async (FindOrCreateDmCommand command, IMediator mediator, CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(command, cancellationToken)))
            .WithName("FindOrCreateDm")
            .WithSummary("Find existing DM or create a new DM / group DM")
            .RequirePermission(ChatPermissions.Channels.Create);
}
