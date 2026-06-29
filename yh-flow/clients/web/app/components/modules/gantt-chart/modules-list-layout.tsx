// FLOW: Forked from Plane modules/gantt-chart/modules-list-layout.tsx
// FLOW: ModulesListGanttChartView — Module 甘特图列表布局（左侧 Module 列表 + 甘特图）
import React from "react";
import { useModules } from "@/../src/lib/hooks/use-modules";
import { ModuleGanttBlock, ModuleGanttSidebarBlock } from "./blocks";

type Props = {
  projectId: string;
  workspaceId: string;
};

export const ModulesListGanttChartView = React.memo(function ModulesListGanttChartView({ projectId, workspaceId }: Props) {
  const { data: modules, isLoading } = useModules(projectId);

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-13 text-tertiary">加载甘特图中...</div>
      </div>
    );
  }

  if (!modules || modules.length === 0) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-13 text-tertiary">暂无模块数据</div>
      </div>
    );
  }

  return (
    <div className="flex size-full overflow-hidden">
      {/* 左侧边栏：Module 列表 */}
      <div className="flex-shrink-0 w-[280px] border-r border-subtle overflow-y-auto">
        {modules.map((mod) => (
          <div key={mod.id} className="h-10 border-b border-subtle px-3 flex items-center">
            <ModuleGanttSidebarBlock
              moduleId={mod.id}
              projectId={projectId}
              workspaceId={workspaceId}
            />
          </div>
        ))}
      </div>
      {/* 右侧：甘特图区块 */}
      <div className="flex-1 overflow-x-auto">
        <div className="flex h-full flex-col">
          {modules.map((mod) => (
            <div key={mod.id} className="h-10 border-b border-subtle">
              <ModuleGanttBlock
                moduleId={mod.id}
                projectId={projectId}
                workspaceId={workspaceId}
              />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
});
