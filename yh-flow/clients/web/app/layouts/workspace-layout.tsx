// FLOW: Workspace layout — sidebar + topbar + Outlet (15-02 per D-P15-01, D-P15-06)
// UI-06: Responsive sidebar with fixed positioning, margin-left based on sidebarCollapsed
import { observer } from "mobx-react";
import { Outlet } from "react-router";
import { useStore } from "@/lib/store-context";
import { AuthenticationWrapper, EPageTypes } from "@/lib/wrappers/authentication-wrapper";
// Components (created in Task 2 and Task 3)
import { WorkspaceSidebar } from "@/components/sidebar/workspace-sidebar";
import { TopBar } from "@/components/navigation/top-bar";
import { CommandPalette } from "@/components/command-palette";

const WorkspaceLayout = observer(function WorkspaceLayout() {
  const { workspace: workspaceStore } = useStore();
  const isCollapsed = workspaceStore.sidebarCollapsed;

  return (
    <AuthenticationWrapper pageType={EPageTypes.AUTHENTICATED}>
      <CommandPalette workspaceId={workspaceStore.currentWorkspaceId} />
      <div className="flex h-full w-full">
        {/* Sidebar — fixed positioned, taken out of flow */}
        <WorkspaceSidebar />

        {/* Main content area — margin-left offsets fixed sidebar */}
        <div
          className={`flex flex-1 flex-col overflow-hidden transition-all duration-300 ${
            isCollapsed ? "ml-14" : "ml-64"
          }`}
        >
          <TopBar />
          <main className="bg-custom-background-90 min-w-0 flex-1 overflow-auto">
            <Outlet />
          </main>
        </div>
      </div>
    </AuthenticationWrapper>
  );
});

export default WorkspaceLayout;
