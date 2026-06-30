// FLOW: Notifications page — workspace-level notification list (per D-P20-04, D-P20-08)
"use client";

import { useParams } from "react-router";
import { observer } from "mobx-react";
import { RefreshCw, CheckCheck } from "lucide-react";
import { useNotifications, useNotificationMutations } from "@/lib/hooks/use-notifications";
import { useStore } from "@/lib/store-context";
import { NotificationList } from "@/components/notifications/notification-list";
import { NotificationEmptyState } from "@/components/notifications/notification-empty-state";
import { NotificationSkeleton } from "@/components/notifications/notification-skeleton";
import { Button } from "@/lib/ui/button";

const NotificationsPage = observer(function NotificationsPage() {
  const { workspaceId } = useParams<{ workspaceId: string }>();
  const wsId = workspaceId ?? "";
  const { workspace: workspaceStore } = useStore();

  const {
    data: notifications,
    isLoading,
    isError,
    refetch,
  } = useNotifications(wsId);
  const { markAllAsRead } = useNotificationMutations();

  return (
    <div className="mx-auto flex h-full w-full max-w-3xl flex-col">
      {/* Header — responsive padding */}
      <div className="flex items-center justify-between border-b border-custom-border-200 px-4 py-3 md:px-6 md:py-4">
        <h1 className="text-2xl font-semibold text-custom-text-100">通知</h1>
        <div className="flex items-center gap-2">
          {/* Refresh button */}
          <Button
            variant="neutral-primary"
            size="sm"
            onClick={() => refetch()}
            title="刷新"
          >
            <RefreshCw className="size-3.5" />
          </Button>
          {/* Mark all as read button */}
          <Button
            variant="primary"
            size="sm"
            onClick={() => markAllAsRead.mutate(wsId)}
          >
            <CheckCheck className="size-3.5" />
            <span>全部标记已读</span>
          </Button>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto">
        {isLoading ? (
          <NotificationSkeleton />
        ) : isError ? (
          <div className="flex flex-col items-center justify-center gap-3 px-6 py-12">
            <p className="text-sm text-custom-text-300">加载通知失败，请重试</p>
            <Button variant="neutral-primary" size="sm" onClick={() => refetch()}>
              重试
            </Button>
          </div>
        ) : !notifications || notifications.length === 0 ? (
          <NotificationEmptyState />
        ) : (
          <NotificationList notifications={notifications} workspaceId={wsId} />
        )}
      </div>
    </div>
  );
});

export default NotificationsPage;
