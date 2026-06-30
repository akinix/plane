// FLOW: TopBar — top navigation bar (D-P15-07)
import { observer } from "mobx-react";
import { Search } from "lucide-react";
import { useLocation } from "react-router";
import { useStore } from "@/lib/store-context";
import { NotificationBell } from "@/components/notifications/notification-bell";
import { UserDropdown } from "./user-dropdown";

export const TopBar = observer(function TopBar() {
  const location = useLocation();
  const { workspace: workspaceStore } = useStore();
  const wsId = workspaceStore.currentWorkspaceId ?? "";

  // Derive page title from current path
  const getTitle = () => {
    const path = location.pathname;
    if (path === "/" || path === `/workspaces/${workspaceStore.currentWorkspaceId}`) {
      return "仪表板";
    }
    if (path.includes("/projects/")) {
      return "项目详情";
    }
    if (path.includes("/settings")) return "设置";
    if (path.includes("/members")) return "成员";
    if (path.includes("/notifications")) return "通知";
    return "Flow";
  };

  return (
    <header className="flex h-12 items-center justify-between border-b border-custom-border-200 bg-custom-sidebar-background-100 px-4">
      {/* Left: page title */}
      <div className="flex items-center gap-2">
        <h1 className="text-sm font-medium text-custom-text-100">
          {getTitle()}
        </h1>
      </div>

      {/* Right: action icons */}
      <div className="flex items-center gap-1.5">
        {/* Search icon (placeholder — implemented in later phases) */}
        <button
          className="flex size-8 items-center justify-center rounded-md text-custom-sidebar-text-200 transition-colors hover:bg-custom-sidebar-background-80"
          title="搜索"
        >
          <Search className="size-4" />
        </button>

        {/* Notification bell with unread count */}
        <NotificationBell workspaceId={wsId} />

        {/* User avatar dropdown */}
        <UserDropdown />
      </div>
    </header>
  );
});
