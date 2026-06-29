// FLOW: ProjectDashboard — project detail framework with placeholder tabs (PROJ-03, D-P15-11)
// Issues tab now navigates to Issue list page per 16-02
import { useState } from "react";
import { useNavigate } from "react-router";
import { useProject } from "../../../src/lib/hooks/use-projects";
import { cn } from "@plane/utils";

const TABS = [
  { key: "issues", label: "Issues" },
  { key: "cycles", label: "Cycles" },
  { key: "modules", label: "Modules" },
  { key: "pages", label: "Pages" },
  { key: "views", label: "Views" },
] as const;

type TProps = { workspaceId: string; projectId: string };

export const ProjectDashboard = ({ workspaceId, projectId }: TProps) => {
  const { data: project, isLoading } = useProject(workspaceId, projectId);
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<string>("issues");

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="size-8 animate-spin rounded-full border-2 border-custom-border-strong border-t-custom-primary" />
      </div>
    );
  }

  if (!project) {
    return (
      <p className="py-8 text-center text-sm text-custom-text-300">
        项目不存在
      </p>
    );
  }

  const emojiCode = project.logo_props?.emoji?.value;
  const emoji = emojiCode
    ? String.fromCodePoint(parseInt(emojiCode, 10))
    : null;

  const placeholderText = (tab: string) => {
    const map: Record<string, string> = {
      cycles: "Cycle 管理将在 Phase 18 中实现",
      modules: "Module 管理将在 Phase 18 中实现",
      pages: "Page 管理将在 Phase 19 中实现",
      views: "视图管理将在 Phase 19 中实现",
    };
    return map[tab] ?? "即将实现";
  };

  const handleTabClick = (tabKey: string) => {
    if (tabKey === "issues") {
      navigate(`/workspaces/${workspaceId}/projects/${projectId}/issues`);
      return;
    }
    setActiveTab(tabKey);
  };

  return (
    <div className="flex flex-col">
      {/* Project header */}
      <div className="border-b border-custom-border-200 px-6 py-4">
        <div className="flex items-center gap-3">
          <span className="flex size-10 items-center justify-center rounded-lg bg-custom-background-80 text-xl">
            {emoji ?? project.name.charAt(0)}
          </span>
          <div>
            <h1 className="text-lg font-semibold text-custom-text-100">
              {project.name}
            </h1>
            <p className="text-xs text-custom-text-300">
              {project.identifier} — {project.description}
            </p>
          </div>
        </div>
      </div>

      {/* Tab bar */}
      <div className="flex gap-0 border-b border-custom-border-200 px-6">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            onClick={() => handleTabClick(tab.key)}
            className={cn(
              "px-4 py-2.5 text-sm font-medium transition-colors",
              activeTab === tab.key && tab.key !== "issues"
                ? "border-b-2 border-custom-primary text-custom-primary"
                : "text-custom-text-300 hover:text-custom-text-100",
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Tab content — only shown for non-issues tabs */}
      {activeTab !== "issues" && (
        <div className="flex items-center justify-center py-16">
          <p className="text-sm text-custom-text-400">
            {placeholderText(activeTab)}
          </p>
        </div>
      )}
    </div>
  );
};
