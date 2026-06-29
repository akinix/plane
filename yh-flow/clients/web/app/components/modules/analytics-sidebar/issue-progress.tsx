// FLOW: Forked from Plane modules/analytics-sidebar/issue-progress.tsx
// FLOW: ModuleAnalyticsProgress — Module 各状态 Issue 数量分组显示
import React from "react";
import { useModuleDetail } from "@/../src/lib/hooks/use-modules";

type Props = {
  moduleId: string;
  projectId: string;
};

const STATE_GROUPS = [
  { key: "backlog_issues", label: "待处理", color: "#a3a3a2" },
  { key: "unstarted_issues", label: "未开始", color: "#a3a3a2" },
  { key: "started_issues", label: "进行中", color: "oklch(0.4799 0.1158 242.91)" },
  { key: "completed_issues", label: "已完成", color: "rgb(21 128 61)" },
  { key: "cancelled_issues", label: "已取消", color: "rgb(185 28 28)" },
];

export const ModuleAnalyticsProgress = React.memo(function ModuleAnalyticsProgress(props: Props) {
  const { moduleId, projectId } = props;
  const { data: moduleDetails } = useModuleDetail(projectId, moduleId);

  if (!moduleDetails) return null;

  const totalIssues = moduleDetails.total_issues ?? 0;
  const completedIssues = moduleDetails.completed_issues ?? 0;
  const progress = totalIssues > 0 ? Math.round((completedIssues / totalIssues) * 100) : 0;

  return (
    <div className="space-y-4 border-t border-subtle px-3 py-4">
      {/* 进度标题 */}
      <div className="flex items-center justify-between">
        <span className="text-13 font-medium text-secondary">进度</span>
        <span className="text-12 text-tertiary">总计 {totalIssues} 个 Issue</span>
      </div>

      {/* 进度条 */}
      <div className="h-2 w-full rounded-full bg-surface-2">
        <div
          className="h-full rounded-full bg-accent-primary transition-all"
          style={{ width: `${progress}%` }}
        />
      </div>

      {/* 进度百分比 */}
      <div className="text-right text-13 font-medium">{progress}%</div>

      {/* 各状态 Issue 分组 */}
      <div className="space-y-2">
        {STATE_GROUPS.map((group) => {
          const count = (moduleDetails as any)[group.key] ?? 0;
          return (
            <div key={group.key} className="flex items-center justify-between text-12">
              <div className="flex items-center gap-2">
                <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: group.color }} />
                <span className="text-tertiary">{group.label}</span>
              </div>
              <span className="text-primary font-medium">{count}</span>
            </div>
          );
        })}
      </div>
    </div>
  );
});
