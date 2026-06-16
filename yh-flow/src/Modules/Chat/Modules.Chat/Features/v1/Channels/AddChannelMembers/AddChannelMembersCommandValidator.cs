using FluentValidation;
using YH.Modules.Chat.Contracts.v1.Commands;

namespace YH.Modules.Chat.Features.v1.Channels.AddChannelMembers;

public sealed class AddChannelMembersCommandValidator : AbstractValidator<AddChannelMembersCommand>
{
    public AddChannelMembersCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.UserIds).NotNull().NotEmpty();
        RuleForEach(x => x.UserIds).NotEmpty().MaximumLength(64);
    }
}
