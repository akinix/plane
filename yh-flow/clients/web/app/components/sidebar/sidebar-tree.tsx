// FLOW: SidebarTree — tree navigation showing workspace > projects > views (D-P15-05)
import { observer } from "mobx-react";
import { useLocation, useNavigate } from "react-router";
import {
  ChevronRight,
  ChevronDown,
  LayoutGrid,
  ListTodo,
  Repeat,
  Blocks,
  FileText,
  Eye,
} from "lucide-react";
import { useStore } from "@/lib/store-context";
import { useProjects } from "../../../src/lib/hooks/use-projects";
import { cn } from "@plane/utils";

const PROJECT_VIEWS = [
  { key: "issues", label: "Issues", icon: ListTodo },
  { key: "cycles", label: "周期", icon: Repeat },
  { key: "modules", label: "模块", icon: Blocks },
  { key: "pages", label: "Pages", icon: FileText },
  { key: "views", label: "Views", icon: Eye },
] as const;

export const SidebarTree = observer(function SidebarTree() {
  const location = useLocation();
  const navigate = useNavigate();
  const { workspace: workspaceStore, project: projectStore } = useStore();
  const { data: projects } = useProjects(workspaceStore.currentWorkspaceId ?? "");

  // Determine current project from URL: /workspaces/:wsId/projects/:projId/...
  const match = location.pathname.match(
    /\/workspaces\/([^/]+)\/projects\/([^/]+)/,
  );
  const currentProjectId = match?.[2] ?? null;

  const wsId = workspaceStore.currentWorkspaceId ?? "";
  const isExpanded = workspaceStore.expandedWorkspaceIds.includes(wsId);

  return (
    <div className="flex flex-col gap-0.5 px-2">
      {/* Current workspace header */}
      <button
        onClick={() => workspaceStore.toggleWorkspaceExpand(wsId)}
        className={cn(
          "flex w-full items-center gap-1.5 rounded-md px-2 py-1.5 text-sm transition-colors",
          {
            "text-custom-sidebar-text-100": !currentProjectId,
            "text-custom-sidebar-text-200 hover:bg-custom-sidebar-background-80":
              currentProjectId,
          },
        )}
      >
        {isExpanded ? (
          <ChevronDown className="size-3.5 flex-shrink-0" />
        ) : (
          <ChevronRight className="size-3.5 flex-shrink-0" />
        )}
        <LayoutGrid className="size-4 flex-shrink-0" />
        <span className="truncate">项目</span>
      </button>

      {/* Project list */}
      {isExpanded && (
        <div className="ml-2 flex flex-col gap-0.5">
          {projects?.map((project) => {
            const projExpanded = projectStore.expandedProjectIds.includes(
              project.id,
            );
            const isActive = project.id === currentProjectId;

            return (
              <div key={project.id}>
                <button
                  onClick={() => projectStore.setExpandedProject(project.id)}
                  className={cn(
                    "flex w-full items-center gap-1.5 rounded-md px-2 py-1 text-sm transition-colors",
                    {
                      "bg-custom-sidebar-background-80 text-custom-sidebar-text-100":
                        isActive,
                      "text-custom-sidebar-text-200 hover:bg-custom-sidebar-background-80":
                        !isActive,
                    },
                  )}
                >
                  {/* Emoji or first char */}
                  <span className="flex size-4 items-center justify-center text-xs">
                    {project.logo_props?.emoji?.value
                      ? String.fromCodePoint(
                          parseInt(project.logo_props.emoji.value, 10),
                        )
                      : project.name.charAt(0)}
                  </span>
                  <span className="truncate">{project.name}</span>
                  <ChevronRight
                    className={cn(
                      "ml-auto size-3 flex-shrink-0 transition-transform",
                      { "rotate-90": projExpanded },
                    )}
                  />
                </button>

                {/* Project views (placeholder for future phases per D-P15-11) */}
                {projExpanded && (
                  <div className="ml-4 flex flex-col gap-0.5 py-0.5">
                    {PROJECT_VIEWS.map((view) => {
                      const ViewIcon = view.icon;
                      const isViewActive =
                        isActive &&
                        location.pathname.includes(`/${view.key}`);
                      return (
                        <button
                          key={view.key}
                          onClick={() =>
                            navigate(
                              `/workspaces/${workspaceStore.currentWorkspaceId}/projects/${project.id}/${view.key}`,
                            )
                          }
                          className={cn(
                            "flex w-full items-center gap-2 rounded-md px-2 py-1 text-xs transition-colors",
                            {
                              "text-custom-sidebar-text-100": isViewActive,
                              "text-custom-sidebar-text-300 hover:bg-custom-sidebar-background-80":
                                !isViewActive,
                            },
                          )}
                        >
                          <ViewIcon className="size-3.5" />
                          <span>{view.label}</span>
                        </button>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
});
