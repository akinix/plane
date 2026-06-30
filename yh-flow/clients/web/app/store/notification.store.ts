// FLOW: NotificationStore — MobX UI state for workspace notifications (per D-P20-06)
import { action, makeObservable, observable } from "mobx";
import type { TNotification } from "@plane/types";

export interface INotificationStore {
  // State
  notifications: TNotification[];
  unreadCount: number;
  loading: boolean;
  error: string | null;

  // Actions
  setNotifications: (notifications: TNotification[]) => void;
  setUnreadCount: (count: number) => void;
  markAsRead: (notificationId: string) => void;
  markAllAsRead: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  reset: () => void;
}

export class NotificationStore implements INotificationStore {
  notifications: TNotification[] = [];
  unreadCount: number = 0;
  loading: boolean = false;
  error: string | null = null;

  constructor() {
    makeObservable(this, {
      notifications: observable.ref,
      unreadCount: observable.ref,
      loading: observable.ref,
      error: observable.ref,
      setNotifications: action,
      setUnreadCount: action,
      markAsRead: action,
      markAllAsRead: action,
      setLoading: action,
      setError: action,
      reset: action,
    });
  }

  setNotifications = (notifications: TNotification[]): void => {
    this.notifications = notifications;
  };

  setUnreadCount = (count: number): void => {
    this.unreadCount = count;
  };

  markAsRead = (notificationId: string): void => {
    const idx = this.notifications.findIndex((n) => n.id === notificationId);
    if (idx >= 0) {
      this.notifications[idx] = {
        ...this.notifications[idx],
        read_at: new Date().toISOString(),
      };
    }
    // Decrement unread count (but not below 0)
    const wasUnread = this.notifications[idx]?.read_at === undefined || !this.notifications[idx]?.read_at;
    if (wasUnread && this.unreadCount > 0) {
      this.unreadCount -= 1;
    }
  };

  markAllAsRead = (): void => {
    this.notifications = this.notifications.map((n) => ({
      ...n,
      read_at: n.read_at ?? new Date().toISOString(),
    }));
    this.unreadCount = 0;
  };

  setLoading = (loading: boolean): void => {
    this.loading = loading;
  };

  setError = (error: string | null): void => {
    this.error = error;
  };

  reset = (): void => {
    this.notifications = [];
    this.unreadCount = 0;
    this.loading = false;
    this.error = null;
  };
}
