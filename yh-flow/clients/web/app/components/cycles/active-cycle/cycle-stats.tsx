// FLOW: Forked from Plane cycles/active-cycle/cycle-stats.tsx
// FLOW: ActiveCycleStats — Cycle 统计概览
import { observer } from "mobx-react";
import type { ICycle } from "@plane/types";

export type ActiveCycleStatsProps = {
  cycle: ICycle | null;
};

export const ActiveCycleStats = observer(function ActiveCycleStats(props: ActiveCycleStatsProps) {
  const { cycle } = props;

  if (!cycle) {
    return (
      <div className="flex min-h-[10rem] flex-col gap-4 rounded-lg border border-subtle bg-surface-1 p-4">
        <div className="h-4 w-24 animate-pulse rounded bg-surface-2" />
        <div className="h-4 w-32 animate-pulse rounded bg-surface-2" />
        <div className="h-4 w-28 animate-pulse rounded bg-surface-2" />
      </div>
    );
  }

  const totalIssues = cycle.total_issues ?? 0;
  const completedIssues = cycle.completed_issues ?? 0;

  // Calculate remaining days
  const endDate = cycle.end_date ? new Date(cycle.end_date) : null;
  const today = new Date();
  const remainingDays = endDate ? Math.max(0, Math.ceil((endDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24))) : 0;

  // Format date
  const formatDate = (date: string | null) => {
    if (!date) return "--";
    const d = new Date(date);
    return `${d.getFullYear()}/${d.getMonth() + 1}/${d.getDate()}`;
  };

  return (
    <div className="flex flex-col gap-3 rounded-lg border border-subtle bg-surface-1 p-4">
      <h4 className="text-14 font-semibold text-tertiary">统计概览</h4>
      <div className="grid grid-cols-2 gap-3">
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">已完成</span>
          <span className="text-14 font-semibold text-primary">
            {completedIssues}/{totalIssues}
          </span>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">开始日期</span>
          <span className="text-14 text-primary">{formatDate(cycle.start_date)}</span>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">结束日期</span>
          <span className="text-14 text-primary">{formatDate(cycle.end_date)}</span>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">剩余天数</span>
          <span className="text-14 font-semibold text-primary">{remainingDays} 天</span>
        </div>
      </div>
    </div>
  );
});
