// FLOW: Forked from Plane modules/analytics-sidebar/progress-stats.tsx
// FLOW: ModuleProgressStats — Module 进度统计（进度百分比、Issue 统计分布）
import React, { useMemo, useState } from "react";
import type { IModule } from "@plane/types";
import { useModuleDetail } from "@/../src/lib/hooks/use-modules";

type Props = {
  moduleId: string;
  projectId: string;
};

type TabKey = "assignees" | "labels" | "states";

const TABS: { key: TabKey; label: string }[] = [
  { key: "assignees", label: "负责人" },
  { key: "labels", label: "标签" },
  { key: "states", label: "状态组" },
];

const STATE_GROUPS_DEF = [
  { key: "backlog_issues", label: "待处理", color: "#a3a3a2" },
  { key: "unstarted_issues", label: "未开始", color: "#a3a3a2" },
  { key: "started_issues", label: "进行中", color: "oklch(0.4799 0.1158 242.91)" },
  { key: "completed_issues", label: "已完成", color: "rgb(21 128 61)" },
  { key: "cancelled_issues", label: "已取消", color: "rgb(185 28 28)" },
];

export const ModuleProgressStats = React.memo(function ModuleProgressStats(props: Props) {
  const { moduleId, projectId } = props;
  const { data: moduleDetails } = useModuleDetail(projectId, moduleId);
  const [activeTab, setActiveTab] = useState<TabKey>("assignees");

  if (!moduleDetails) return null;

  const totalIssues = moduleDetails.total_issues ?? 0;
  const completedIssues = moduleDetails.completed_issues ?? 0;
  const progress = totalIssues > 0 ? Math.round((completedIssues / totalIssues) * 100) : 0;

  const groupedIssues = useMemo(() => ({
    backlog: moduleDetails.backlog_issues ?? 0,
    unstarted: moduleDetails.unstarted_issues ?? 0,
    started: moduleDetails.started_issues ?? 0,
    completed: moduleDetails.completed_issues ?? 0,
    cancelled: moduleDetails.cancelled_issues ?? 0,
  }), [moduleDetails]);

  return (
    <div className="space-y-4 px-3 py-4">
      {/* 进度概要 */}
      <div className="flex items-center justify-between">
        <span className="text-13 font-medium text-secondary">模块统计</span>
        <span className="text-13 font-medium">{progress}%</span>
      </div>

      {/* 进度条 */}
      <div className="h-2 w-full rounded-full bg-surface-2">
        <div
          className="h-full rounded-full bg-accent-primary transition-all"
          style={{ width: `${progress}%` }}
        />
      </div>

      {/* 完成概览 */}
      <div className="text-12 text-tertiary">
        {completedIssues}/{totalIssues} 个工作项完成
      </div>

      {/* Tab 切换 */}
      <div className="flex gap-1 rounded-md bg-layer-2 p-1">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            className={`flex-1 rounded-sm px-2 py-1 text-11 transition-all ${
              activeTab === tab.key
                ? "bg-surface-1 text-primary shadow-sm"
                : "text-tertiary hover:text-secondary"
            }`}
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Tab 内容 */}
      <div className="space-y-2 text-12">
        {activeTab === "assignees" && (
          <div className="text-tertiary">暂无分配人数据</div>
        )}
        {activeTab === "labels" && (
          <div className="text-tertiary">暂无标签数据</div>
        )}
        {activeTab === "states" && (
          <div className="space-y-1.5">
            {STATE_GROUPS_DEF.map((group) => {
              const count = (groupedIssues as any)[group.key.split("_")[0] as keyof typeof groupedIssues] ?? 0;
              const pct = totalIssues > 0 ? Math.round((count / totalIssues) * 100) : 0;
              return (
                <div key={group.key} className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full" style={{ backgroundColor: group.color }} />
                  <span className="flex-1 text-tertiary">{group.label}</span>
                  <span className="text-primary font-medium">{count}</span>
                  <span className="text-tertiary w-8 text-right">{pct}%</span>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
});