using Mediator;
using YH.Modules.Notifications.Contracts.v1.DTOs;

namespace YH.Modules.Notifications.Contracts.v1.Queries;

public sealed record GetNotificationPreferencesQuery : IQuery<UserNotificationPreferenceDto>;
