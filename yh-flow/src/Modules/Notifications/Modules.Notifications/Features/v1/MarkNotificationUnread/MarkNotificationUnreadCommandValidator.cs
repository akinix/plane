using FluentValidation;
using YH.Modules.Notifications.Contracts.v1.Commands;

namespace YH.Modules.Notifications.Features.v1.MarkNotificationUnread;

public sealed class MarkNotificationUnreadCommandValidator : AbstractValidator<MarkNotificationUnreadCommand>
{
    public MarkNotificationUnreadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
