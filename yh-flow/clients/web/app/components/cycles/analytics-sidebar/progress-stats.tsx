// FLOW: Forked from Plane cycles/analytics-sidebar/progress-stats.tsx
import React from "react";

type Props = {
  completed: number;
  pending: number;
  total: number;
};

export const AnalyticsProgressStats: React.FC<Props> = ({ completed, pending, total }) => {
  const pct = total > 0 ? Math.round((completed / total) * 100) : 0;

  return (
    <div className="flex flex-col gap-2">
      <h4 className="text-xs font-medium text-secondary">进度统计</h4>
      <div className="grid grid-cols-3 gap-2 text-center">
        <div className="rounded border border-subtle bg-surface-1 p-2">
          <div className="text-xs text-tertiary">已完成</div>
          <div className="text-sm font-semibold text-green-600">{completed}</div>
        </div>
        <div className="rounded border border-subtle bg-surface-1 p-2">
          <div className="text-xs text-tertiary">剩余</div>
          <div className="text-sm font-semibold text-secondary">{pending}</div>
        </div>
        <div className="rounded border border-subtle bg-surface-1 p-2">
          <div className="text-xs text-tertiary">完成率</div>
          <div className="text-sm font-semibold text-primary">{pct}%</div>
        </div>
      </div>
    </div>
  );
};
