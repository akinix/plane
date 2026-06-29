// FLOW: Forked from Plane cycles/analytics-sidebar/sidebar-details.tsx
import React from "react";
import { SquareUser, CalendarDays, Workflow } from "lucide-react";

type ProgressData = {
  total_issues?: number;
  completed_issues?: number;
  start_date?: string;
  end_date?: string;
} | null;

type Props = {
  projectId: string;
  progress: ProgressData;
};

export const CycleSidebarDetails: React.FC<Props> = ({ progress }) => {
  if (!progress) return null;

  const total = progress.total_issues ?? 0;
  const completed = progress.completed_issues ?? 0;
  const pct = total > 0 ? Math.round((completed / total) * 100) : 0;

  return (
    <div className="flex flex-col gap-4 px-5 pb-4">
      <div className="flex items-center gap-2 text-xs text-tertiary">
        <CalendarDays className="h-3.5 w-3.5" />
        <span>
          {progress.start_date ?? "—"} → {progress.end_date ?? "—"}
        </span>
      </div>

      <div className="flex items-center gap-2 text-xs text-tertiary">
        <Workflow className="h-3.5 w-3.5" />
        <span>
          {completed}/{total} Issue ({pct}%)
        </span>
      </div>

      <div className="flex items-center gap-2 text-xs text-tertiary">
        <SquareUser className="h-3.5 w-3.5" />
        <span>负责人: —</span>
      </div>
    </div>
  );
};
