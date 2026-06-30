// FLOW: Forked from Plane modules/module-card-item.tsx
// FLOW: ModuleCardItem — Module 卡片组件（4 色状态徽章、进度条、Issue 计数）
import React from "react";
import { useNavigate } from "react-router";
import { SquareUser } from "lucide-react";
import { MODULE_STATUS, PROGRESS_STATE_GROUPS_DETAILS } from "@plane/constants";
import type { IModule } from "@plane/types";
import { Card, LinearProgressIndicator } from "@plane/ui";
import { useModules } from "@/../src/lib/hooks/use-modules";
import { ModuleStatusDropdown } from "./module-status-dropdown";

const STATUS_STYLES: Record<string, { bg: string; text: string }> = {
  backlog: { bg: "rgba(163,163,163,0.15)", text: "#a3a3a2" },
  planned: { bg: "rgba(163,163,163,0.15)", text: "#a3a3a2" },
  in_progress: { bg: "oklch(0.4799 0.1158 242.91 / 0.15)", text: "oklch(0.4799 0.1158 242.91)" },
  completed: { bg: "rgba(34,197,94,0.15)", text: "rgb(21 128 61)" },
  cancelled: { bg: "var(--bg-danger-subtle)", text: "rgb(185 28 28)" },
};

const STATUS_LABELS: Record<string, string> = {
  backlog: "待开始",
  planned: "待开始",
  in_progress: "进行中",
  completed: "已完成",
  cancelled: "已取消",
};

type Props = {
  moduleId: string;
  projectId: string;
  workspaceId: string;
};

export const ModuleCardItem = React.memo(function ModuleCardItem(props: Props) {
  const { moduleId, projectId, workspaceId } = props;
  const navigate = useNavigate();
  const { data: modules } = useModules(projectId);
  const moduleDetails = modules?.find((m) => m.id === moduleId);

  if (!moduleDetails) return null;

  const moduleTotalIssues =
    (moduleDetails.backlog_issues ?? 0) +
    (moduleDetails.unstarted_issues ?? 0) +
    (moduleDetails.started_issues ?? 0) +
    (moduleDetails.completed_issues ?? 0) +
    (moduleDetails.cancelled_issues ?? 0);

  const moduleCompletedIssues = moduleDetails.completed_issues ?? 0;
  const moduleStatus = MODULE_STATUS.find((status) => status.value === moduleDetails.status);
  const statusStyle = STATUS_STYLES[moduleDetails.status ?? "backlog"] ?? STATUS_STYLES.backlog;

  const issueCount =
    moduleTotalIssues === 0
      ? "0 个工作项"
      : moduleTotalIssues === moduleCompletedIssues
        ? `${moduleTotalIssues} 个工作项`
        : `${moduleCompletedIssues}/${moduleTotalIssues} 个工作项`;

  const progressIndicatorData = PROGRESS_STATE_GROUPS_DETAILS.map((group) => ({
    id: group.key,
    name: group.title,
    value: moduleTotalIssues > 0 ? (moduleDetails[group.key as keyof IModule] as number) ?? 0 : 0,
    color: group.color,
  }));

  const handleClick = () => {
    navigate(`/workspaces/${workspaceId}/projects/${projectId}/modules/${moduleId}`);
  };

  return (
    <div className="relative">
      <button className="w-full text-left" onClick={handleClick}>
        <Card>
          <div>
            <div className="flex items-center justify-between gap-2">
              <span className="truncate text-14 font-medium">{moduleDetails.name}</span>
              <div className="flex items-center gap-2" onClick={(e) => e.stopPropagation()}>
                {moduleStatus && (
                  <span
                    className="flex h-5 items-center justify-center rounded-sm px-2 text-11"
                    style={{ color: statusStyle.text, backgroundColor: statusStyle.bg }}
                  >
                    {STATUS_LABELS[moduleDetails.status ?? "backlog"] ?? "待开始"}
                  </span>
                )}
              </div>
            </div>
          </div>
          <div className="flex flex-col gap-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-1.5 text-secondary">
                <span className="text-11 text-tertiary">{issueCount}</span>
              </div>
              {moduleDetails.lead_id ? (
                <SquareUser className="h-4 w-4 text-tertiary" />
              ) : (
                <SquareUser className="h-4 w-4 text-tertiary" />
              )}
            </div>
            <LinearProgressIndicator size="lg" data={progressIndicatorData} />
            <div className="flex items-center justify-between py-0.5">
              {moduleDetails.start_date || moduleDetails.target_date ? (
                <span className="text-11 text-tertiary">
                  {moduleDetails.start_date && new Date(moduleDetails.start_date).toLocaleDateString("zh-CN")}
                  {moduleDetails.start_date && moduleDetails.target_date && " — "}
                  {moduleDetails.target_date && new Date(moduleDetails.target_date).toLocaleDateString("zh-CN")}
                </span>
              ) : (
                <span className="text-11 text-tertiary">未设置日期</span>
              )}
            </div>
          </div>
        </Card>
      </button>
    </div>
  );
});
