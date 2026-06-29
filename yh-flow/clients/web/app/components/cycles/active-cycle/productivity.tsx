// FLOW: Forked from Plane cycles/active-cycle/productivity.tsx
// FLOW: ActiveCycleProductivity — Cycle 生产力指标
import { observer } from "mobx-react";
import type { ICycle } from "@plane/types";

export type ActiveCycleProductivityProps = {
  cycle: ICycle | null;
};

export const ActiveCycleProductivity = observer(function ActiveCycleProductivity(props: ActiveCycleProductivityProps) {
  const { cycle } = props;

  if (!cycle) {
    return (
      <div className="flex min-h-[10rem] flex-col gap-4 rounded-lg border border-subtle bg-surface-1 p-4">
        <div className="h-4 w-24 animate-pulse rounded bg-surface-2" />
        <div className="h-4 w-20 animate-pulse rounded bg-surface-2" />
        <div className="h-4 w-20 animate-pulse rounded bg-surface-2" />
      </div>
    );
  }

  const totalIssues = cycle.total_issues ?? 0;
  const completedIssues = cycle.completed_issues ?? 0;

  // Calculate duration in days
  const startDate = cycle.start_date ? new Date(cycle.start_date) : null;
  const endDate = cycle.end_date ? new Date(cycle.end_date) : null;
  const totalDays = startDate && endDate ? Math.max(1, Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24))) : 1;

  // Days elapsed
  const today = new Date();
  const daysElapsed = startDate ? Math.max(1, Math.ceil((today.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24))) : 1;

  const avgPerDay = totalDays > 0 ? (completedIssues / totalDays).toFixed(1) : "0";
  const dailyRate = daysElapsed > 0 ? (completedIssues / daysElapsed).toFixed(1) : "0";

  return (
    <div className="flex flex-col gap-3 rounded-lg border border-subtle bg-surface-1 p-4">
      <h4 className="text-14 font-semibold text-tertiary">生产力</h4>
      <div className="grid grid-cols-2 gap-4">
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">总计 Issues</span>
          <span className="text-14 font-semibold text-primary">{totalIssues}</span>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">已关闭</span>
          <span className="text-14 font-semibold text-primary">{completedIssues}</span>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">周期长度</span>
          <span className="text-14 text-primary">{totalDays} 天</span>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-12 text-tertiary">日均完成</span>
          <span className="text-14 text-primary">{dailyRate}/天</span>
        </div>
      </div>
    </div>
  );
});
