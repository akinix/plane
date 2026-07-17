using YH.Framework.Core.Context;
using YH.Modules.Notifications.Contracts.v1.Commands;
using YH.Modules.Notifications.Contracts.v1.DTOs;
using YH.Modules.Notifications.Contracts.v1.Queries;
using YH.Modules.Notifications.Features.v1.GetNotificationPreferences;
using YH.Modules.Notifications.Features.v1.UpdateNotificationPreferences;
using NSubstitute;

namespace Notifications.Tests.Integration;

public sealed class PreferenceTests
{
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static ICurrentUser CreateCurrentUser()
    {
        var user = Substitute.For<ICurrentUser>();
        user.GetUserId().Returns(TestUserId);
        return user;
    }

    [Fact]
    public async Task Get_Should_Return_Default_When_None_Exists()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        var handler = new GetNotificationPreferencesQueryHandler(db, user);
        var result = await handler.Handle(new GetNotificationPreferencesQuery(), CancellationToken.None);

        result.ShouldNotBeNull();
        result.UserId.ShouldBe(TestUserId);
        result.PropertyChanged.ShouldBeTrue();
        result.StateChanged.ShouldBeTrue();
        result.Comment.ShouldBeTrue();
        result.Mention.ShouldBeTrue();
        result.IssueCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Update_Should_Change_Preferences()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        var updateHandler = new UpdateNotificationPreferencesCommandHandler(db, user);
        var cmd = new UpdateNotificationPreferencesCommand(
            null, null,
            PropertyChanged: false,
            StateChanged: null,
            Comment: null,
            Mention: true,
            IssueCompleted: null);

        await updateHandler.Handle(cmd, CancellationToken.None);

        var getHandler = new GetNotificationPreferencesQueryHandler(db, user);
        var result = await getHandler.Handle(new GetNotificationPreferencesQuery(), CancellationToken.None);

        result.PropertyChanged.ShouldBeFalse();
        result.Mention.ShouldBeTrue();
        // Defaults for unchanged fields
        result.StateChanged.ShouldBeTrue();
        result.Comment.ShouldBeTrue();
        result.IssueCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Update_Should_Create_Preference_When_None_Exists()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        // Verify no preference exists
        var getHandler = new GetNotificationPreferencesQueryHandler(db, user);
        var defaultResult = await getHandler.Handle(new GetNotificationPreferencesQuery(), CancellationToken.None);
        defaultResult.Id.ShouldBe(Guid.Empty);

        // Update should create one
        var updateHandler = new UpdateNotificationPreferencesCommandHandler(db, user);
        var cmd = new UpdateNotificationPreferencesCommand(
            null, null,
            PropertyChanged: false,
            StateChanged: null,
            Comment: null,
            Mention: null,
            IssueCompleted: null);

        await updateHandler.Handle(cmd, CancellationToken.None);

        // Now it should exist with the updated value
        var result = await getHandler.Handle(new GetNotificationPreferencesQuery(), CancellationToken.None);
        result.Id.ShouldNotBe(Guid.Empty);
        result.PropertyChanged.ShouldBeFalse();
    }
}
