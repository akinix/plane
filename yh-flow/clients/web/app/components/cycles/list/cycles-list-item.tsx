// FLOW: Forked from Plane cycles/list/cycles-list-item.tsx
// FLOW: CyclesListItem — Cycle 列表项卡片
import type { MouseEvent } from "react";
import React, { useRef } from "react";
import { observer } from "mobx-react";
import { CalendarDays, Circle } from "lucide-react";
import { useCycles } from "@/../src/lib/hooks/use-cycles";
import { CycleListItemAction } from "./cycle-list-item-action";
import { CycleQuickActions } from "../quick-actions";

type TCyclesListItem = {
  cycleId: string;
  workspaceSlug: string;
  projectId: string;
};

export const CyclesListItem = observer(function CyclesListItem(props: TCyclesListItem) {
  const { cycleId, workspaceSlug, projectId } = props;
  const parentRef = useRef<HTMLDivElement>(null);
  const { data: cycles } = useCycles(projectId);
  const cycleDetails = cycles?.find((c) => c.id === cycleId);

  if (!cycleDetails) return null;

  const cycleStatus = cycleDetails.status?.toLocaleLowerCase() ?? "draft";
  const isActive = cycleStatus === "current";

  // Calculate progress
  const completedIssues = cycleDetails.completed_issues ?? 0;
  const totalIssues = cycleDetails.total_issues ?? 0;
  const progress = totalIssues > 0 ? Math.round((completedIssues / totalIssues) * 100) : 0;

  // Format date range
  const formatDate = (date: string | null) => {
    if (!date) return "";
    const d = new Date(date);
    return `${d.getMonth() + 1}/${d.getDate()}`;
  };

  // Status color
  const statusColor = isActive ? "bg-green-500" : cycleStatus === "completed" ? "bg-blue-500" : "bg-gray-400";

  return (
    <div
      ref={parentRef}
      className="group flex items-center gap-3 border-b border-subtle px-4 py-3 hover:bg-surface-1 cursor-pointer"
      onClick={() => {
        window.location.href = `/${workspaceSlug}/projects/${projectId}/cycles/${cycleDetails.id}`;
      }}
    >
      {/* Status dot */}
      <Circle className={`h-2.5 w-2.5 ${isActive ? "fill-green-500 text-green-500" : "fill-gray-400 text-gray-400"}`} />

      {/* Cycle name */}
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <div className="flex items-center gap-2">
          <span className="truncate text-14 font-semibold text-primary">{cycleDetails.name}</span>
        </div>
        <div className="flex items-center gap-3 text-12 text-tertiary">
          <span className="flex items-center gap-1">
            <CalendarDays className="h-3 w-3" />
            {formatDate(cycleDetails.start_date)} - {formatDate(cycleDetails.end_date)}
          </span>
          <span>{totalIssues} 个事项</span>
        </div>
      </div>

      {/* Progress bar */}
      {totalIssues > 0 && (
        <div className="flex w-32 items-center gap-2">
          <div className="h-2 flex-1 overflow-hidden rounded-full bg-surface-2">
            <div
              className="h-full rounded-full bg-accent-primary transition-all"
              style={{ width: `${Math.min(progress, 100)}%` }}
            />
          </div>
          <span className="text-12 text-tertiary">{progress}%</span>
        </div>
      )}

      {/* Completed count */}
      <div className="hidden text-12 text-tertiary md:block">
        {completedIssues}/{totalIssues}
      </div>

      {/* Actions (desktop) */}
      <div className="hidden items-center gap-1 md:flex">
        <CycleListItemAction
          workspaceSlug={workspaceSlug}
          projectId={projectId}
          cycleId={cycleId}
          cycleDetails={cycleDetails}
          parentRef={parentRef as unknown as React.RefObject<HTMLDivElement>}
          isActive={isActive}
        />
      </div>

      {/* Quick actions (mobile) */}
      <div className="block md:hidden">
        <CycleQuickActions
          parentRef={parentRef as unknown as React.RefObject<HTMLElement>}
          cycleId={cycleId}
          projectId={projectId}
          workspaceSlug={workspaceSlug}
        />
      </div>
    </div>
  );
});
