using YH.Framework.Shared.Identity.Authorization;
using YH.Modules.Chat.Contracts.Authorization;
using YH.Modules.Chat.Contracts.v1.Commands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Chat.Features.v1.Messages.UnpinMessage;

public static class UnpinMessageEndpoint
{
    internal static RouteHandlerBuilder MapUnpinMessageEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/messages/{id:guid}/pin",
                async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new UnpinMessageCommand(id), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("UnpinMessage")
            .WithSummary("Remove a pin from a message")
            .RequirePermission(ChatPermissions.Messages.Send);
}
