using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Chat.Contracts.Authorization;
using YH.Modules.Chat.Contracts.v1.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Chat.Features.v1.Channels.GetChannelById;

public static class GetChannelByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetChannelByIdEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/channels/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(new GetChannelByIdQuery(id), cancellationToken)))
            .WithName("GetChannelById")
            .WithSummary("Get a single channel with members and unread count")
            .RequirePermission(ChatPermissions.Channels.View);
}
