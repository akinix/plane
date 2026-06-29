// FLOW: Workspace layout — sidebar + topbar + Outlet (15-02 per D-P15-01, D-P15-06)
import { observer } from "mobx-react";
import { Outlet } from "react-router";
import { useStore } from "@/lib/store-context";
import { AuthenticationWrapper, EPageTypes } from "@/lib/wrappers/authentication-wrapper";
// Components (created in Task 2 and Task 3)
import { WorkspaceSidebar } from "@/components/sidebar/workspace-sidebar";
import { TopBar } from "@/components/navigation/top-bar";

const WorkspaceLayout = observer(function WorkspaceLayout() {
  const { workspace: workspaceStore } = useStore();
  const sidebarWidth = workspaceStore.sidebarCollapsed ? "w-14" : "w-64";

  return (
    <AuthenticationWrapper pageType={EPageTypes.AUTHENTICATED}>
      <div className="flex h-full w-full">
        {/* Sidebar */}
        <div className={`${sidebarWidth} flex-shrink-0 transition-all duration-300`}>
          <WorkspaceSidebar />
        </div>

        {/* Main content area */}
        <div className="flex flex-1 flex-col overflow-hidden">
          <TopBar />
          <main className="bg-custom-background-90 flex-1 overflow-auto">
            <Outlet />
          </main>
        </div>
      </div>
    </AuthenticationWrapper>
  );
});

export default WorkspaceLayout;
