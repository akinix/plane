// FLOW: Forked from Plane modules/analytics-sidebar/root.tsx
// FLOW: ModuleAnalyticsSidebar — Module 分析侧栏（进度统计 + 链接管理）
import React, { useState } from "react";
import { X, Info } from "lucide-react";
import { useModuleDetail, useModuleLinkMutations } from "@/../src/lib/hooks/use-modules";
import { ModuleStatusDropdown } from "../module-status-dropdown";
import { ModuleLinksList } from "../links/list";
import { ModuleAnalyticsProgress } from "./issue-progress";
import { ModuleProgressStats } from "./progress-stats";

type Props = {
  moduleId: string;
  projectId: string;
  workspaceSlug: string;
  handleClose: () => void;
};

export const ModuleAnalyticsSidebar = React.memo(function ModuleAnalyticsSidebar(props: Props) {
  const { moduleId, projectId, workspaceSlug, handleClose } = props;
  const { data: moduleDetails } = useModuleDetail(projectId, moduleId);

  if (!moduleDetails) {
    return (
      <div className="flex flex-col gap-2 py-4">
        <div className="h-4 w-24 animate-pulse rounded bg-surface-2" />
        <div className="mt-4 space-y-3">
          <div className="h-8 animate-pulse rounded bg-surface-2" />
          <div className="h-8 animate-pulse rounded bg-surface-2" />
          <div className="h-8 animate-pulse rounded bg-surface-2" />
        </div>
      </div>
    );
  }

  const totalIssues = moduleDetails.total_issues ?? 0;
  const completedIssues = moduleDetails.completed_issues ?? 0;
  const issueCount = totalIssues === 0 ? "0 个工作项" : `${completedIssues}/${totalIssues}`;

  return (
    <div className="relative">
      {/* 关闭按钮 */}
      <div className="sticky top-0 z-10 flex items-center justify-between bg-surface-1 pt-5 pb-5">
        <button
          className="flex h-5 w-5 items-center justify-center rounded-full bg-layer-3 text-tertiary hover:text-primary"
          onClick={handleClose}
        >
          <X className="h-3 w-3" />
        </button>
      </div>

      {/* Module 名称 + 状态 */}
      <div className="flex flex-col gap-3">
        <div className="flex items-center gap-5 pt-2">
          <ModuleStatusDropdown value={moduleDetails.status} />
        </div>
        <h4 className="w-full text-18 font-semibold break-words text-primary">{moduleDetails.name}</h4>
      </div>

      {/* 描述 */}
      {moduleDetails.description && (
        <div className="mt-2 text-13 leading-5 text-secondary">{moduleDetails.description}</div>
      )}

      {/* 属性信息 */}
      <div className="mt-4 flex flex-col gap-3">
        {/* 日期 */}
        <div className="flex items-center justify-start gap-1">
          <div className="flex w-2/5 items-center justify-start gap-2 text-tertiary">
            <span className="text-14">日期范围</span>
          </div>
          <div className="w-3/5 text-13 text-secondary">
            {moduleDetails.start_date ? new Date(moduleDetails.start_date).toLocaleDateString("zh-CN") : "未设置"}
            {moduleDetails.start_date && moduleDetails.target_date && " — "}
            {moduleDetails.target_date ? new Date(moduleDetails.target_date).toLocaleDateString("zh-CN") : ""}
          </div>
        </div>

        {/* Issue 计数 */}
        <div className="flex items-center justify-start gap-1">
          <div className="flex w-2/5 items-center justify-start gap-2 text-tertiary">
            <span className="text-14">Issue</span>
          </div>
          <div className="flex h-7 w-3/5 items-center">
            <span className="px-1.5 text-13 text-tertiary">{issueCount}</span>
          </div>
        </div>
      </div>

      {/* 进度（Issue 分组） */}
      <ModuleAnalyticsProgress moduleId={moduleId} projectId={projectId} />

      {/* 进度统计 */}
      <ModuleProgressStats moduleId={moduleId} projectId={projectId} />

      {/* 链接管理 */}
      <div className="flex flex-col border-t border-subtle px-1.5 py-5">
        <div className="flex items-center justify-between mb-3">
          <span className="text-13 font-medium text-secondary">链接</span>
        </div>
        <ModuleLinksList moduleId={moduleId} projectId={projectId} />
      </div>
    </div>
  );
});
