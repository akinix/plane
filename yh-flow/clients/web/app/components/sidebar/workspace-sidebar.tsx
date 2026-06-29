// FLOW: WorkspaceSidebar — sidebar container with switcher, tree, and collapse button (D-P15-01, D-P15-06)
import { observer } from "mobx-react";
import { PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { useStore } from "@/lib/store-context";
import { WorkspaceSwitcher } from "./workspace-switcher";
import { SidebarTree } from "./sidebar-tree";
import { cn } from "@plane/utils";

export const WorkspaceSidebar = observer(function WorkspaceSidebar() {
  const { workspace: workspaceStore } = useStore();
  const collapsed = workspaceStore.sidebarCollapsed;

  return (
    <div
      className={cn(
        "flex h-full flex-col border-r border-custom-border-200 bg-custom-sidebar-background-100 transition-all duration-300",
        collapsed ? "w-14" : "w-64",
      )}
    >
      {/* Top: Workspace Switcher / collapsed icon */}
      <div
        className={cn(
          "flex-shrink-0 px-2 py-3",
          collapsed && "flex justify-center",
        )}
      >
        {collapsed ? (
          <div className="flex size-8 items-center justify-center rounded-md bg-custom-sidebar-background-80 text-sm font-medium text-custom-sidebar-text-100">
            {workspaceStore.currentWorkspaceId?.charAt(0).toUpperCase() ?? "?"}
          </div>
        ) : (
          <WorkspaceSwitcher />
        )}
      </div>

      {/* Middle: Navigation Tree */}
      <div className="flex-1 overflow-y-auto">{!collapsed && <SidebarTree />}</div>

      {/* Bottom: Collapse/Expand Button */}
      <div className="flex-shrink-0 border-t border-custom-border-200 p-2">
        <button
          onClick={() => workspaceStore.setSidebarCollapsed(!collapsed)}
          className={cn(
            "flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-sm text-custom-sidebar-text-200 transition-colors hover:bg-custom-sidebar-background-80",
            collapsed && "justify-center",
          )}
          title={collapsed ? "展开侧边栏" : "折叠侧边栏"}
        >
          {collapsed ? (
            <PanelLeftOpen className="size-4" />
          ) : (
            <>
              <PanelLeftClose className="size-4" />
              <span>折叠</span>
            </>
          )}
        </button>
      </div>
    </div>
  );
});
