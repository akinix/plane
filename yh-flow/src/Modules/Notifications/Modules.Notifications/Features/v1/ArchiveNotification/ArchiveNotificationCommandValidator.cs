using FluentValidation;
using YH.Modules.Notifications.Contracts.v1.Commands;

namespace YH.Modules.Notifications.Features.v1.ArchiveNotification;

public sealed class ArchiveNotificationCommandValidator : AbstractValidator<ArchiveNotificationCommand>
{
    public ArchiveNotificationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
