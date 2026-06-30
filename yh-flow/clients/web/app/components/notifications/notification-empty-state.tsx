// FLOW: NotificationEmptyState — Empty state for notifications list (per UI-SPEC)
import { Bell } from "lucide-react";

export const NotificationEmptyState = () => {
  return (
    <div className="flex h-full w-full flex-col items-center justify-center">
      <Bell className="size-12 text-custom-text-300" />
      <h3 className="mt-4 text-sm font-medium text-custom-text-300">暂无通知</h3>
    </div>
  );
};
