// FLOW: Forked from Plane cycles/analytics-sidebar/issue-progress.tsx
import React from "react";
import { observer } from "mobx-react";
import { Loader } from "@plane/ui";
import { useCycleProgress } from "@/../src/lib/hooks/use-cycle-issues";
import { BurndownChart } from "./burndown-chart";
import { AnalyticsProgressStats } from "./progress-stats";

type Props = {
  projectId: string;
  cycleId: string;
};

export const CycleAnalyticsProgress = observer(function CycleAnalyticsProgress(props: Props) {
  const { projectId, cycleId } = props;
  const { data: progress, isLoading } = useCycleProgress(projectId, cycleId);

  if (isLoading)
    return (
      <Loader className="mt-4 px-5">
        <Loader.Item height="200px" />
      </Loader>
    );

  if (!progress) return null;

  const burndownData = progress.burndown ?? [];
  const totalIssues = progress.total_issues ?? 0;

  return (
    <div className="mt-4 flex flex-col gap-4 px-5">
      <BurndownChart
        data={burndownData}
        startDate={progress.start_date ?? ""}
        endDate={progress.end_date ?? ""}
        totalIssues={totalIssues}
      />

      <div className="border-t border-subtle pt-4">
        <h4 className="text-xs font-medium text-secondary mb-2">Issue 进度</h4>
        <div className="space-y-2">
          {[
            { label: "待处理", count: progress.backlog_issues ?? 0, color: "bg-gray-400" },
            { label: "未开始", count: progress.unstarted_issues ?? 0, color: "bg-gray-500" },
            { label: "进行中", count: progress.started_issues ?? 0, color: "bg-blue-500" },
            { label: "已完成", count: progress.completed_issues ?? 0, color: "bg-green-500" },
            { label: "已取消", count: progress.cancelled_issues ?? 0, color: "bg-red-400" },
          ].map((item) => (
            <div key={item.label} className="flex items-center justify-between text-xs">
              <div className="flex items-center gap-2">
                <span className={`inline-block h-2 w-2 rounded-full ${item.color}`} />
                <span className="text-secondary">{item.label}</span>
              </div>
              <span className="text-primary">{item.count}</span>
            </div>
          ))}
        </div>
      </div>

      <div className="border-t border-subtle pt-4">
        <AnalyticsProgressStats
          completed={progress.completed_issues ?? 0}
          pending={(totalIssues - (progress.completed_issues ?? 0))}
          total={totalIssues}
        />
      </div>
    </div>
  );
});
