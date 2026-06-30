// FLOW: Forked from Plane modules/module-list-item.tsx
// FLOW: ModuleListItem — Module 紧凑列表项
import React from "react";
import { useNavigate } from "react-router";
import { Info, Check } from "lucide-react";
import { useModules } from "@/../src/lib/hooks/use-modules";
import { ModuleStatusDropdown } from "./module-status-dropdown";
import { ModuleQuickActions } from "./quick-actions";

type Props = {
  moduleId: string;
  projectId: string;
  workspaceId: string;
};

export const ModuleListItem = React.memo(function ModuleListItem(props: Props) {
  const { moduleId, projectId, workspaceId } = props;
  const navigate = useNavigate();
  const { data: modules } = useModules(projectId);
  const moduleDetails = modules?.find((m) => m.id === moduleId);

  if (!moduleDetails) return null;

  const completionPercentage = moduleDetails.total_issues > 0
    ? Math.round(((moduleDetails.completed_issues + moduleDetails.cancelled_issues) / moduleDetails.total_issues) * 100)
    : 0;

  const progress = isNaN(completionPercentage) ? 0 : completionPercentage;

  const handleClick = () => {
    navigate(`/workspaces/${workspaceId}/projects/${projectId}/modules/${moduleId}`);
  };

  return (
    <button
      type="button"
      className="group flex w-full items-center gap-3 border-b border-subtle px-4 py-2.5 hover:bg-surface-1 cursor-pointer text-left"
      onClick={handleClick}
    >
      {/* 进度圆环 */}
      <div className="flex-shrink-0 relative h-[30px] w-[30px]">
        <svg className="h-full w-full" viewBox="0 0 30 30">
          <circle cx="15" cy="15" r="12" fill="none" stroke="var(--border-subtle)" strokeWidth="3" />
          <circle
            cx="15"
            cy="15"
            r="12"
            fill="none"
            stroke="var(--accent-primary)"
            strokeWidth="3"
            strokeDasharray={`${2 * Math.PI * 12}`}
            strokeDashoffset={`${2 * Math.PI * 12 * (1 - progress / 100)}`}
            transform="rotate(-90 15 15)"
          />
        </svg>
        <div className="absolute inset-0 flex items-center justify-center">
          {progress === 100 ? (
            <Check className="h-3 w-3 stroke-[2] text-accent-primary" />
          ) : (
            <span className="text-9 text-tertiary">{progress}%</span>
          )}
        </div>
      </div>

      {/* Module 名称 */}
      <div className="flex-1 min-w-0">
        <span className="truncate text-14 font-medium">{moduleDetails.name}</span>
      </div>

      {/* 状态 */}
      <ModuleStatusDropdown
        value={moduleDetails.status}
        isDisabled={false}
      />

      {/* Issue 计数 */}
      <span className="text-11 text-tertiary flex-shrink-0">
        {moduleDetails.completed_issues ?? 0}/{(moduleDetails.total_issues ?? 0)} 个工作项
      </span>

      {/* 操作按钮 */}
      <button
        className="flex-shrink-0 hidden group-hover:flex"
        onClick={(e) => {
          e.stopPropagation();
          // open peek
        }}
      >
        <Info className="h-4 w-4 text-placeholder" />
      </button>

      <ModuleQuickActions
        parentRef={React.createRef<HTMLDivElement>() as unknown as React.RefObject<HTMLDivElement>}
        moduleId={moduleId}
        projectId={projectId}
        workspaceSlug={workspaceId}
      />
    </button>
  );
});
