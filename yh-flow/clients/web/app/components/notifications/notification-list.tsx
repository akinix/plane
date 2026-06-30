// FLOW: NotificationList — Flat list container for notification items (per D-P20-08 simplified UI)
import { type TNotification } from "@plane/types";
import { NotificationItem } from "./notification-item";

type Props = {
  notifications: TNotification[];
  workspaceId: string;
};

export const NotificationList = ({ notifications, workspaceId }: Props) => {
  return (
    <div className="flex flex-col overflow-y-auto">
      {notifications.map((notification) => (
        <NotificationItem
          key={notification.id}
          notification={notification}
          workspaceId={workspaceId}
        />
      ))}
    </div>
  );
};
