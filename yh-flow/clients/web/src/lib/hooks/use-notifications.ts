// FLOW: TanStack Query hooks for workspace notifications (mock data layer per D-P20-07)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TNotification, TUnreadNotificationsCount } from "@plane/types";
import notificationService from "../services/notification.service";

export const useNotifications = (workspaceId: string) => {
  return useQuery<TNotification[]>({
    queryKey: ["workspace-notifications", workspaceId],
    queryFn: async () => {
      return notificationService.getNotifications(workspaceId);
    },
    enabled: !!workspaceId,
  });
};

export const useUnreadCount = (workspaceId: string) => {
  return useQuery<TUnreadNotificationsCount>({
    queryKey: ["workspace-unread-count", workspaceId],
    queryFn: async () => {
      return notificationService.getUnreadCount(workspaceId);
    },
    enabled: !!workspaceId,
    refetchInterval: 30000, // Poll every 30s per D-P20-01
  });
};

export const useNotificationMutations = () => {
  const queryClient = useQueryClient();

  const markAsRead = useMutation({
    mutationFn: async ({
      notificationId,
      workspaceId: _workspaceId,
    }: {
      notificationId: string;
      workspaceId: string;
    }) => {
      await notificationService.markAsRead(notificationId);
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["workspace-notifications", variables.workspaceId] });
      queryClient.invalidateQueries({ queryKey: ["workspace-unread-count", variables.workspaceId] });
    },
  });

  const markAllAsRead = useMutation({
    mutationFn: async (workspaceId: string) => {
      await notificationService.markAllAsRead(workspaceId);
    },
    onSuccess: (_data, workspaceId) => {
      queryClient.invalidateQueries({ queryKey: ["workspace-notifications", workspaceId] });
      queryClient.invalidateQueries({ queryKey: ["workspace-unread-count", workspaceId] });
    },
  });

  return { markAsRead, markAllAsRead };
};
