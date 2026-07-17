using FluentValidation;
using YH.Modules.Notifications.Contracts.v1.Commands;

namespace YH.Modules.Notifications.Features.v1.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesCommandValidator()
    {
        RuleFor(x => x).Must(x =>
            x.PropertyChanged.HasValue ||
            x.StateChanged.HasValue ||
            x.Comment.HasValue ||
            x.Mention.HasValue ||
            x.IssueCompleted.HasValue)
            .WithMessage("At least one preference toggle must be provided.");
    }
}
