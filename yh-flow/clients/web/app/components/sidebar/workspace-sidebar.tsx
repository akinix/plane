// FLOW: WorkspaceSidebar — sidebar container with switcher, tree, and collapse button (D-P15-01, D-P15-06)
// UI-06: Responsive auto-collapse at <1024px, fixed positioning, debounced resize handler
import { useEffect } from "react";
import { observer } from "mobx-react";
import { PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { useStore } from "@/lib/store-context";
import { WorkspaceSwitcher } from "./workspace-switcher";
import { SidebarTree } from "./sidebar-tree";
import { cn } from "@plane/utils";

export const WorkspaceSidebar = observer(function WorkspaceSidebar() {
  const { workspace: workspaceStore } = useStore();
  const collapsed = workspaceStore.sidebarCollapsed;

  // Keyboard shortcut: Cmd/Ctrl + B to toggle sidebar
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === "b") {
        e.preventDefault();
        workspaceStore.toggleSidebarCollapsed();
      }
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [workspaceStore]);

  // Responsive auto-collapse/expand at 1024px breakpoint
  useEffect(() => {
    const handleResize = () => {
      const width = window.innerWidth;
      const shouldCollapse = width < 1024;
      if (shouldCollapse !== workspaceStore.sidebarCollapsed) {
        workspaceStore.setSidebarCollapsed(shouldCollapse);
      }
    };

    // Set initial state on mount
    handleResize();

    // Debounced resize handler
    let timeoutId: ReturnType<typeof setTimeout>;
    const debouncedResize = () => {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(handleResize, 100);
    };

    window.addEventListener("resize", debouncedResize);
    return () => {
      window.removeEventListener("resize", debouncedResize);
      clearTimeout(timeoutId);
    };
  }, [workspaceStore]);

  return (
    <div
      className={cn(
        "border-custom-border-200 bg-custom-sidebar-background-100 fixed top-0 left-0 z-30 flex h-screen flex-col border-r transition-all duration-300",
        collapsed ? "w-14" : "w-64"
      )}
    >
      {/* Top: Workspace Switcher / collapsed icon */}
      <div className={cn("flex-shrink-0 px-2 py-3", collapsed && "flex justify-center")}>
        {collapsed ? (
          <div className="bg-custom-sidebar-background-80 text-sm text-custom-sidebar-text-100 flex size-8 items-center justify-center rounded-md font-medium">
            {workspaceStore.currentWorkspaceId?.charAt(0).toUpperCase() ?? "?"}
          </div>
        ) : (
          <WorkspaceSwitcher />
        )}
      </div>

      {/* Middle: Navigation Tree */}
      <div className="flex-1 overflow-y-auto">{!collapsed && <SidebarTree />}</div>

      {/* Bottom: Collapse/Expand Button */}
      <div className="border-custom-border-200 flex-shrink-0 border-t p-2">
        <button
          onClick={() => workspaceStore.toggleSidebarCollapsed()}
          className={cn(
            "text-sm text-custom-sidebar-text-200 hover:bg-custom-sidebar-background-80 flex w-full items-center gap-2 rounded-md px-2 py-1.5 transition-colors",
            collapsed && "justify-center"
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
