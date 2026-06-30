// FLOW: NotificationItem — Single notification row with avatar, content, timestamp, read state (per NOTI-04)
import { type TNotification } from "@plane/types";
import { formatDistanceToNow } from "date-fns";
import { ControlLink } from "@/lib/ui/control-link";
import { Avatar } from "@/lib/ui/avatar";
import { useNotificationMutations } from "@/lib/hooks/use-notifications";
import { cn } from "@plane/utils";

// Map activity field to Chinese verb description
const fieldVerbMap: Record<string, (notif: TNotification) => string> = {
  priority: () => "更新了优先级",
  state: () => "更新了状态",
  assignee: () => "分配了 Issue",
  description: () => "更新了描述",
  name: (notif: TNotification) => (notif.data?.issue_activity?.verb === "created" ? "创建了 Issue" : "修改了标题"),
};

const getActionDescription = (notification: TNotification): string => {
  const field = notification.data?.issue_activity?.field;
  const mapper = field ? fieldVerbMap[field] : undefined;
  if (mapper) return mapper(notification);

  // Fallback
  const actorName = notification.triggered_by_details?.display_name ?? "";
  const title = notification.title ?? "";
  return `${actorName} ${title}`;
};

type Props = {
  notification: TNotification;
  workspaceId: string;
};

export const NotificationItem = ({ notification, workspaceId }: Props) => {
  const { markAsRead } = useNotificationMutations();
  const isUnread = !notification.read_at;
  const actorName = notification.triggered_by_details?.display_name ?? "";
  const issueName = notification.data?.issue?.name ?? notification.entity_name ?? "";
  const actionDesc = getActionDescription(notification);

  // Navigate to associated issue per NOTI-04
  const issueUrl =
    notification.entity_identifier && notification.project
      ? `/workspaces/${workspaceId}/projects/${notification.project}/issues/${notification.entity_identifier}`
      : undefined;

  const handleClick = () => {
    if (isUnread) {
      markAsRead.mutate({ notificationId: notification.id, workspaceId });
    }
  };

  return (
    <ControlLink
      href={issueUrl ?? "#"}
      onClick={handleClick}
      className={cn(
        "flex items-start gap-3 px-4 py-3 transition-colors",
        isUnread ? "bg-custom-background-80" : "bg-transparent",
        "hover:bg-custom-background-70 cursor-pointer"
      )}
    >
      {/* Left: Avatar */}
      <div className="flex-shrink-0">
        {notification.triggered_by_details ? (
          <Avatar
            name={actorName}
            src={notification.triggered_by_details.avatar_url}
            size={32}
            className="rounded-full"
          />
        ) : (
          <div className="size-8 rounded-full bg-custom-background-80" />
        )}
      </div>

      {/* Center: Content */}
      <div className="min-w-0 flex-1">
        <p className="text-sm text-custom-text-100">
          <span className="font-medium">{actorName}</span>
          <span className="text-custom-text-200"> {actionDesc}</span>
        </p>
        {issueName && (
          <p className="truncate text-sm text-custom-text-300">{issueName}</p>
        )}
        <p className="mt-0.5 text-xs text-custom-text-300">
          {notification.created_at
            ? formatDistanceToNow(new Date(notification.created_at), { addSuffix: true })
            : ""}
        </p>
      </div>

      {/* Right: Unread indicator */}
      {isUnread && (
        <div className="mt-1.5 flex-shrink-0">
          <div className="size-1.5 rounded-full bg-custom-primary" />
        </div>
      )}
    </ControlLink>
  );
};
