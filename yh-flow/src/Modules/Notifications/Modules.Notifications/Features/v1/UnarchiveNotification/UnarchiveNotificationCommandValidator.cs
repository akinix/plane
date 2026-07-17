using FluentValidation;
using YH.Modules.Notifications.Contracts.v1.Commands;

namespace YH.Modules.Notifications.Features.v1.UnarchiveNotification;

public sealed class UnarchiveNotificationCommandValidator : AbstractValidator<UnarchiveNotificationCommand>
{
    public UnarchiveNotificationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
