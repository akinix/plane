// FLOW: Forked from Plane modules/module-list-item-action.tsx
// FLOW: ModuleListItemAction — Module 列表项操作按钮组
import React from "react";
import { SquareUser } from "lucide-react";
import { MODULE_STATUS } from "@plane/constants";
import type { IModule } from "@plane/types";
import { useModules } from "@/../src/lib/hooks/use-modules";
import { ModuleStatusDropdown } from "./module-status-dropdown";
import { ModuleQuickActions } from "./quick-actions";

type Props = {
  moduleId: string;
  projectId: string;
  workspaceId: string;
  parentRef: React.RefObject<HTMLDivElement>;
};

export const ModuleListItemAction = React.memo(function ModuleListItemAction(props: Props) {
  const { moduleId, projectId, workspaceId, parentRef } = props;
  const { data: modules } = useModules(projectId);
  const moduleDetails = modules?.find((m) => m.id === moduleId);

  if (!moduleDetails) return null;

  const moduleStatus = MODULE_STATUS.find((status) => status.value === moduleDetails.status);

  return (
    <>
      <div className="flex items-center gap-2">
        {/* 日期范围 */}
        {moduleDetails.start_date || moduleDetails.target_date ? (
          <span className="text-11 text-tertiary whitespace-nowrap">
            {moduleDetails.start_date && new Date(moduleDetails.start_date).toLocaleDateString("zh-CN")}
            {moduleDetails.start_date && moduleDetails.target_date && " - "}
            {moduleDetails.target_date && new Date(moduleDetails.target_date).toLocaleDateString("zh-CN")}
          </span>
        ) : (
          <span className="text-11 text-tertiary">无日期</span>
        )}

        {/* 状态下拉 */}
        {moduleStatus && (
          <ModuleStatusDropdown
            value={moduleDetails.status}
            isDisabled={false}
          />
        )}

        {/* 负责人 */}
        <SquareUser className="h-4 w-4 text-tertiary" />
      </div>

      {/* 快速操作 */}
      <ModuleQuickActions
        parentRef={parentRef}
        moduleId={moduleId}
        projectId={projectId}
        workspaceSlug={workspaceId}
      />
    </>
  );
});
