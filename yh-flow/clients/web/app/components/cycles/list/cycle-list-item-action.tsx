// FLOW: Forked from Plane cycles/list/cycle-list-item-action.tsx
// FLOW: CycleListItemAction — 列表项操作按钮
import type React from "react";
import { observer } from "mobx-react";
import { Eye, CalendarDays, ArrowRight } from "lucide-react";

type Props = {
  workspaceSlug: string;
  projectId: string;
  cycleId: string;
  cycleDetails: any;
  parentRef: React.RefObject<HTMLDivElement>;
  isActive?: boolean;
};

export const CycleListItemAction = observer(function CycleListItemAction(props: Props) {
  const { workspaceSlug, projectId, cycleId, cycleDetails, parentRef, isActive = false } = props;

  const formatDate = (date: string | null) => {
    if (!date) return "--";
    const d = new Date(date);
    return `${d.getMonth() + 1}/${d.getDate()}`;
  };

  return (
    <>
      <button
        className="flex items-center gap-1 text-11 text-accent-secondary opacity-0 group-hover:opacity-100"
        onClick={(e) => {
          e.preventDefault();
          e.stopPropagation();
        }}
      >
        <Eye className="h-4 w-4" />
        <span>详情</span>
      </button>
      {isActive && cycleDetails.start_date && (
        <div className="flex items-center gap-1 text-11 font-medium text-tertiary">
          <CalendarDays className="h-3 w-3" />
          <span>
            {formatDate(cycleDetails.start_date)}
            <ArrowRight className="mx-1 inline h-3 w-3" />
            {formatDate(cycleDetails.end_date)}
          </span>
        </div>
      )}
    </>
  );
});
