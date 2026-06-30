// FLOW: NotificationService — Mock data service for workspace notifications (per D-P20-07)
import type { TNotification, TUnreadNotificationsCount } from "@plane/types";
import { MOCK_NOTIFICATIONS } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

class NotificationService {
  async getNotifications(workspaceId: string): Promise<TNotification[]> {
    await delay(250);
    return MOCK_NOTIFICATIONS.filter((n) => n.workspace === workspaceId);
  }

  async getUnreadCount(workspaceId: string): Promise<TUnreadNotificationsCount> {
    await delay(150);
    const unread = MOCK_NOTIFICATIONS.filter((n) => n.workspace === workspaceId && !n.read_at).length;
    return {
      total_unread_notifications_count: unread,
      mention_unread_notifications_count: 0,
    };
  }

  async markAsRead(notificationId: string): Promise<void> {
    await delay(100);
    const idx = MOCK_NOTIFICATIONS.findIndex((n) => n.id === notificationId);
    if (idx >= 0 && !MOCK_NOTIFICATIONS[idx].read_at) {
      (MOCK_NOTIFICATIONS[idx] as TNotification).read_at = new Date().toISOString();
    }
  }

  async markAllAsRead(workspaceId: string): Promise<void> {
    await delay(200);
    for (const n of MOCK_NOTIFICATIONS) {
      if (n.workspace === workspaceId && !n.read_at) {
        (n as TNotification).read_at = new Date().toISOString();
      }
    }
  }
}

const notificationService = new NotificationService();
export default notificationService;
