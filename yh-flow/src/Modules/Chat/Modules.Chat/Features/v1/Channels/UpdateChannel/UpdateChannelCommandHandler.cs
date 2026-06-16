using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Chat.Contracts.v1.Commands;
using YH.Modules.Chat.Data;
using YH.Modules.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace YH.Modules.Chat.Features.v1.Channels.UpdateChannel;

public sealed class UpdateChannelCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateChannelCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateChannelCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == cmd.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");

        channel.RequireAdmin(userId.ToString());
        channel.Rename(cmd.Name, cmd.Description);
        channel.SetPrivate(cmd.IsPrivate);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
