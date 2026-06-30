// FLOW: NotificationBell — Top-bar Bell icon with unread badge (per D-P20-08)
import { observer } from "mobx-react";
import { Bell } from "lucide-react";
import { useNavigate } from "react-router";
import { useUnreadCount } from "@/lib/hooks/use-notifications";
import { Tooltip } from "@/lib/ui/tooltip";
import { cn } from "@plane/utils";

type Props = {
  workspaceId: string;
};

export const NotificationBell = observer(function NotificationBell({ workspaceId }: Props) {
  const navigate = useNavigate();
  const { data: unreadData } = useUnreadCount(workspaceId);
  const count = unreadData?.total_unread_notifications_count ?? 0;
  const hasUnread = count >= 1;
  const displayCount = count >= 100 ? "99+" : String(count);

  const tooltipContent = hasUnread ? `${count} 条未读通知` : "通知";

  return (
    <Tooltip tooltipContent={tooltipContent} position="bottom">
      <button
        onClick={() => navigate(`/workspaces/${workspaceId}/notifications`)}
        className={cn(
          "relative flex size-8 items-center justify-center rounded-md transition-colors",
          "text-custom-sidebar-text-200 hover:bg-custom-sidebar-background-80"
        )}
      >
        <Bell className={cn("size-4", hasUnread ? "fill-current" : "")} />
        {hasUnread && (
          <span className="absolute -right-0.5 -top-0.5 flex min-w-[18px] items-center justify-center rounded-full bg-custom-primary px-1 py-0.5 text-[10px] font-medium leading-none text-white">
            {displayCount}
          </span>
        )}
      </button>
    </Tooltip>
  );
});
