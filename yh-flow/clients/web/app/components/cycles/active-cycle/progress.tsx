// FLOW: Forked from Plane cycles/active-cycle/progress.tsx
// FLOW: ActiveCycleProgress — Cycle 进度条组件
import { observer } from "mobx-react";
import type { ICycle } from "@plane/types";

export type ActiveCycleProgressProps = {
  cycle: ICycle | null;
};

export const ActiveCycleProgress = observer(function ActiveCycleProgress(props: ActiveCycleProgressProps) {
  const { cycle } = props;

  if (!cycle) {
    return (
      <div className="flex min-h-[17rem] flex-col gap-5 rounded-lg border border-subtle bg-surface-1 px-3.5 py-4">
        <div className="h-4 w-32 animate-pulse rounded bg-surface-2" />
        <div className="h-2 w-full animate-pulse rounded bg-surface-2" />
      </div>
    );
  }

  const totalIssues = cycle.total_issues ?? 0;
  const completedIssues = cycle.completed_issues ?? 0;
  const cancelledIssues = cycle.cancelled_issues ?? 0;
  const startedIssues = cycle.started_issues ?? 0;
  const unstartedIssues = cycle.unstarted_issues ?? 0;
  const backlogIssues = cycle.backlog_issues ?? 0;

  const closedCount = completedIssues + cancelledIssues;
  const totalForProgress = totalIssues - cancelledIssues;
  const progressPercent = totalForProgress > 0 ? Math.round((completedIssues / totalForProgress) * 100) : 0;

  return (
    <div className="flex min-h-[17rem] flex-col gap-5 rounded-lg border border-subtle bg-surface-1 px-3.5 py-4">
      <div className="flex flex-col gap-3">
        <div className="flex items-center justify-between gap-4">
          <h3 className="text-14 font-semibold text-tertiary">周期进度</h3>
          {totalIssues > 0 && (
            <span className="flex gap-1 rounded-xs px-3 py-1 text-13 font-medium whitespace-nowrap text-placeholder">
              {closedCount}/{totalForProgress} 已完成
            </span>
          )}
        </div>
        {totalIssues > 0 && (
          <div className="h-2 w-full overflow-hidden rounded-full bg-surface-2">
            <div
              className="h-full rounded-full bg-accent-primary transition-all"
              style={{ width: `${Math.min(progressPercent, 100)}%` }}
            />
          </div>
        )}
      </div>

      {totalIssues > 0 ? (
        <div className="flex flex-col gap-3">
          {[
            { label: "已完成", count: completedIssues, color: "#10B981" },
            { label: "进行中", count: startedIssues, color: "#F59E0B" },
            { label: "未开始", count: unstartedIssues, color: "#6B7280" },
            { label: "待办", count: backlogIssues, color: "#9CA3AF" },
          ].map(
            (item, index) =>
              item.count > 0 && (
                <div key={index} className="flex items-center justify-between gap-2 text-13">
                  <div className="flex items-center gap-1.5">
                    <span className="block h-3 w-3 rounded-full" style={{ backgroundColor: item.color }} />
                    <span className="w-16 font-medium text-tertiary capitalize">{item.label}</span>
                  </div>
                  <span className="text-tertiary">{item.count} 个事项</span>
                </div>
              )
          )}
          {cancelledIssues > 0 && (
            <span className="flex items-center gap-2 text-13 text-tertiary">
              <span>{cancelledIssues} 个已取消的事项不包含在此报告中。</span>
            </span>
          )}
        </div>
      ) : (
        <div className="flex h-full w-full items-center justify-center">
          <p className="text-13 text-tertiary">暂无事项</p>
        </div>
      )}
    </div>
  );
});
