// FLOW: Forked from Plane cycles/list/cycle-list-group-header.tsx
// FLOW: CycleListGroupHeader — 分组标题组件
import React from "react";
import { ChevronDown } from "lucide-react";

type Props = {
  type: string;
  title: string;
  count?: number;
  showCount?: boolean;
  isExpanded?: boolean;
};

export function CycleListGroupHeader(props: Props) {
  const { type, title, count, showCount = false, isExpanded = false } = props;
  return (
    <div className="flex items-center justify-between px-4 py-2.5">
      <div className="flex items-center gap-2">
        <div className="flex h-5 w-5 items-center justify-center overflow-hidden rounded-xs">
          {type === "current" && <span className="h-5 w-5 rounded-full bg-green-500" />}
          {type === "upcoming" && <span className="h-5 w-5 rounded-full bg-blue-400" />}
          {type === "draft" && <span className="h-5 w-5 rounded-full bg-gray-400" />}
          {type === "completed" && <span className="h-5 w-5 rounded-full bg-blue-500" />}
        </div>
        <div className="flex items-center gap-1 overflow-hidden">
          <span className="truncate font-medium text-primary">{title}</span>
          {showCount && <span className="text-13 font-medium text-tertiary">{count ?? "0"}</span>}
        </div>
      </div>
      <ChevronDown
        className={`h-4 w-4 shrink-0 text-tertiary transition-transform ${isExpanded ? "rotate-180" : ""}`}
      />
    </div>
  );
}
