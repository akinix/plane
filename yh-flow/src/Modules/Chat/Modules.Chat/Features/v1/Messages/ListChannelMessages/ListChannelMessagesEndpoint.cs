using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Chat.Contracts.Authorization;
using YH.Modules.Chat.Contracts.v1.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Chat.Features.v1.Messages.ListChannelMessages;

public static class ListChannelMessagesEndpoint
{
    internal static RouteHandlerBuilder MapListChannelMessagesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/channels/{id:guid}/messages",
                async (Guid id, Guid? before, int? pageSize, IMediator mediator, CancellationToken cancellationToken) =>
                    Results.Ok(await mediator.Send(
                        new ListChannelMessagesQuery(id, before, pageSize ?? 50),
                        cancellationToken)))
            .WithName("ListChannelMessages")
            .WithSummary("List top-level messages in a channel (cursor-paged, reverse chronological)")
            .RequirePermission(ChatPermissions.Channels.View);
}
