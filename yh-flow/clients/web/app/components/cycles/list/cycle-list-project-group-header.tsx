// FLOW: Forked from Plane cycles/list/cycle-list-project-group-header.tsx
// FLOW: CycleListProjectGroupHeader — 项目分组标题（多项目视图）
import React from "react";
import { ChevronRight } from "lucide-react";

type Props = {
  projectId: string;
  projectName?: string;
  count?: number;
  showCount?: boolean;
  isExpanded?: boolean;
};

export function CycleListProjectGroupHeader(props: Props) {
  const { projectId, projectName, count, showCount = false, isExpanded = false } = props;

  return (
    <div className="flex items-center gap-2 px-4 py-2.5">
      <ChevronRight
        className={`h-4 w-4 text-tertiary transition-transform duration-300 ${isExpanded ? "rotate-90" : ""}`}
        strokeWidth={2}
      />
      <div className="flex h-4 w-4 items-center justify-center overflow-hidden">
        <span className="text-14">{projectName?.charAt(0) ?? "?"}</span>
      </div>
      <div className="flex items-center gap-1 overflow-hidden">
        <span className="truncate font-medium text-primary">{projectName ?? "未知项目"}</span>
        {showCount && <span className="text-13 font-medium text-tertiary">{count ?? "0"}</span>}
      </div>
    </div>
  );
}
