// FLOW: Forked from Plane modules/module-peek-overview.tsx
// FLOW: ModulePeekOverview — Module 详情弹窗概览
import React, { useEffect } from "react";
import { X } from "lucide-react";
import { useModuleDetail } from "@/../src/lib/hooks/use-modules";
import { ModuleStatusDropdown } from "./module-status-dropdown";

type Props = {
  moduleId: string;
  projectId: string;
  workspaceSlug: string;
  isOpen: boolean;
  onClose: () => void;
};

export const ModulePeekOverview = React.memo(function ModulePeekOverview(props: Props) {
  const { moduleId, projectId, workspaceSlug, isOpen, onClose } = props;
  const { data: moduleDetails } = useModuleDetail(projectId, moduleId);

  if (!isOpen || !moduleDetails) return null;

  const completionPercentage = moduleDetails.total_issues > 0
    ? Math.round(((moduleDetails.completed_issues + moduleDetails.cancelled_issues) / moduleDetails.total_issues) * 100)
    : 0;

  return (
    <div
      className="fixed right-0 top-0 z-[9] flex h-full w-full max-w-[24rem] flex-shrink-0 flex-col gap-3.5 overflow-y-auto border-l border-subtle bg-surface-1 px-6 py-4 shadow-lg"
    >
      {/* 头部 */}
      <div className="flex items-center justify-between">
        <h3 className="text-16 font-medium text-primary">{moduleDetails.name}</h3>
        <button onClick={onClose} className="text-tertiary hover:text-primary">
          <X className="h-4 w-4" />
        </button>
      </div>

      {/* 状态 */}
      <div className="flex items-center gap-2">
        <ModuleStatusDropdown value={moduleDetails.status} />
      </div>

      {/* 进度 */}
      <div className="flex flex-col gap-1">
        <div className="flex items-center justify-between">
          <span className="text-12 text-tertiary">进度</span>
          <span className="text-12 font-medium">{completionPercentage}%</span>
        </div>
        <div className="h-2 w-full rounded-full bg-surface-2">
          <div
            className="h-full rounded-full bg-accent-primary"
            style={{ width: `${completionPercentage}%` }}
          />
        </div>
      </div>

      {/* 描述 */}
      {moduleDetails.description && (
        <div className="text-13 text-secondary">
          {moduleDetails.description}
        </div>
      )}

      {/* 日期 */}
      <div className="flex flex-col gap-1 text-12 text-tertiary">
        {moduleDetails.start_date && (
          <div className="flex justify-between">
            <span>开始日期</span>
            <span>{new Date(moduleDetails.start_date).toLocaleDateString("zh-CN")}</span>
          </div>
        )}
        {moduleDetails.target_date && (
          <div className="flex justify-between">
            <span>目标日期</span>
            <span>{new Date(moduleDetails.target_date).toLocaleDateString("zh-CN")}</span>
          </div>
        )}
      </div>

      {/* Issue 统计 */}
      <div className="space-y-1 text-12">
        <div className="flex justify-between">
          <span className="text-tertiary">总计 Issue</span>
          <span>{moduleDetails.total_issues ?? 0}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-tertiary">已完成</span>
          <span className="text-green-700">{moduleDetails.completed_issues ?? 0}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-tertiary">进行中</span>
          <span className="text-accent-primary">{moduleDetails.started_issues ?? 0}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-tertiary">未开始</span>
          <span>{moduleDetails.unstarted_issues ?? 0}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-tertiary">已取消</span>
          <span className="text-red-500">{moduleDetails.cancelled_issues ?? 0}</span>
        </div>
      </div>
    </div>
  );
});
