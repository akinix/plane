using System.Collections.ObjectModel;
using YH.Framework.Core.Context;
using YH.Modules.Notifications.Contracts.v1.Commands;
using YH.Modules.Notifications.Contracts.v1.DTOs;
using YH.Modules.Notifications.Contracts.v1.Queries;
using YH.Modules.Notifications.Domain;
using YH.Modules.Notifications.Features.v1.ListNotifications;
using YH.Modules.Notifications.Features.v1.GetUnreadCount;
using YH.Modules.Notifications.Features.v1.MarkNotificationRead;
using YH.Modules.Notifications.Features.v1.MarkNotificationUnread;
using YH.Modules.Notifications.Features.v1.MarkAllNotificationsRead;
using YH.Modules.Notifications.Features.v1.ArchiveNotification;
using YH.Modules.Notifications.Features.v1.UnarchiveNotification;
using NSubstitute;

namespace Notifications.Tests.Integration;

public sealed class NotificationTests
{
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string TestUserIdString = "00000000-0000-0000-0000-000000000001";

    private static ICurrentUser CreateCurrentUser()
    {
        var user = Substitute.For<ICurrentUser>();
        user.GetUserId().Returns(TestUserId);
        return user;
    }

    private static Notification CreateTestNotification(string userId, string type = "test.type", string title = "Test Notification")
    {
        return Notification.Create(userId, type, title, null, null, "test", null);
    }

    [Fact]
    public async Task List_Should_Return_Notifications()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        db.Notifications.Add(CreateTestNotification(TestUserIdString, "type.a", "Notification A"));
        db.Notifications.Add(CreateTestNotification(TestUserIdString, "type.b", "Notification B"));
        await db.SaveChangesAsync();

        var handler = new ListNotificationsQueryHandler(db, user);
        var query = new ListNotificationsQuery();

        ReadOnlyCollection<NotificationDto> result = await handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task List_Should_Be_Empty_When_No_Notifications()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        var handler = new ListNotificationsQueryHandler(db, user);
        var query = new ListNotificationsQuery();

        ReadOnlyCollection<NotificationDto> result = await handler.Handle(query, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task MarkRead_Should_Set_ReadAt()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        var notification = CreateTestNotification(TestUserIdString);
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        notification.ReadAtUtc.ShouldBeNull();

        var handler = new MarkNotificationReadCommandHandler(db, user);
        await handler.Handle(new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        var saved = await db.Notifications.FirstAsync(n => n.Id == notification.Id);
        saved.ReadAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task MarkUnread_Should_Clear_ReadAt()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        var notification = CreateTestNotification(TestUserIdString);
        notification.MarkRead();
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
        notification.ReadAtUtc.ShouldNotBeNull();

        var handler = new MarkNotificationUnreadCommandHandler(db, user);
        await handler.Handle(new MarkNotificationUnreadCommand(notification.Id), CancellationToken.None);

        var saved = await db.Notifications.FirstAsync(n => n.Id == notification.Id);
        saved.ReadAtUtc.ShouldBeNull();
    }

    [Fact(Skip = "ExecuteUpdateAsync is not supported by EF Core InMemory provider")]
    public async Task MarkAllRead_Should_Mark_All_Read()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        db.Notifications.Add(CreateTestNotification(TestUserIdString));
        db.Notifications.Add(CreateTestNotification(TestUserIdString));
        await db.SaveChangesAsync();

        var handler = new MarkAllNotificationsReadCommandHandler(db, user);
        int updated = await handler.Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        updated.ShouldBe(2);

        var unreadCount = await db.Notifications.CountAsync(n => n.ReadAtUtc == null);
        unreadCount.ShouldBe(0);
    }

    [Fact]
    public async Task Archive_Should_Set_ArchivedAt()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        var notification = CreateTestNotification(TestUserIdString);
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        notification.ArchivedAt.ShouldBeNull();

        var handler = new ArchiveNotificationCommandHandler(db, user);
        await handler.Handle(new ArchiveNotificationCommand(notification.Id), CancellationToken.None);

        var saved = await db.Notifications.FirstAsync(n => n.Id == notification.Id);
        saved.ArchivedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Unarchive_Should_Clear_ArchivedAt()
    {
        await using var db = NotificationTestFixture.CreateDbContext();
        var user = CreateCurrentUser();

        var notification = CreateTestNotification(TestUserIdString);
        notification.Archive();
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
        notification.ArchivedAt.ShouldNotBeNull();

        var handler = new UnarchiveNotificationCommandHandler(db, user);
        await handler.Handle(new UnarchiveNotificationCommand(notification.Id), CancellationToken.None);

        var saved = await db.Notifications.FirstAsync(n => n.Id == notification.Id);
        saved.ArchivedAt.ShouldBeNull();
    }
}
