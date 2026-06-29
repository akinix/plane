// FLOW: Forked from Plane modules/gantt-chart/blocks.tsx
// FLOW: ModuleGanttBlock + ModuleGanttSidebarBlock — Module 甘特图区块（复用 Phase 17 GanttView per D-P18-11）
import React from "react";
import { useRouter } from "next/navigation";
import { MODULE_STATUS } from "@plane/constants";
import { useModules } from "@/../src/lib/hooks/use-modules";

const STATUS_STYLES: Record<string, { bg: string; text: string }> = {
  backlog: { bg: "#a3a3a220", text: "#a3a3a2" },
  planned: { bg: "#a3a3a220", text: "#a3a3a2" },
  in_progress: { bg: "oklch(0.4799 0.1158 242.91 / 0.15)", text: "oklch(0.4799 0.1158 242.91)" },
  completed: { bg: "rgba(34,197,94,0.15)", text: "rgb(21 128 61)" },
  cancelled: { bg: "var(--bg-danger-subtle)", text: "rgb(185 28 28)" },
};

const SIDEBAR_WIDTH = 280;

type Props = {
  moduleId: string;
  projectId: string;
  workspaceId: string;
};

export const ModuleGanttBlock = React.memo(function ModuleGanttBlock({ moduleId, projectId, workspaceId }: Props) {
  const router = useRouter();
  const { data: modules } = useModules(projectId);
  const moduleDetails = modules?.find((m) => m.id === moduleId);

  if (!moduleDetails) return null;

  const moduleStatus = MODULE_STATUS.find((s) => s.value === moduleDetails?.status);
  const color = moduleStatus?.color ?? "";
  const statusStyle = STATUS_STYLES[moduleDetails.status ?? "backlog"] ?? STATUS_STYLES.backlog;

  const blockStyle = {
    backgroundColor: statusStyle.bg,
    borderLeft: `3px solid ${color || statusStyle.text}`,
  };

  return (
    <div
      className="relative flex h-full w-full cursor-pointer items-center rounded-sm text-13"
      style={blockStyle}
      onClick={() =>
        router.push(`/workspaces/${workspaceId}/projects/${projectId}/modules/${moduleId}`)
      }
    >
      <div className="absolute top-0 left-0 h-full w-full bg-surface-1/50" />
      <div
        className="sticky w-auto truncate overflow-hidden px-2.5 py-1 text-primary"
        style={{ left: `${SIDEBAR_WIDTH}px` }}
      >
        {moduleDetails.name}
      </div>
    </div>
  );
});

export const ModuleGanttSidebarBlock = React.memo(function ModuleGanttSidebarBlock({ moduleId, projectId, workspaceId }: Props) {
  const router = useRouter();
  const { data: modules } = useModules(projectId);
  const moduleDetails = modules?.find((m) => m.id === moduleId);

  return (
    <div
      className="relative flex h-full w-full cursor-pointer items-center gap-2"
      onClick={() => router.push(`/workspaces/${workspaceId}/projects/${projectId}/modules/${moduleId}`)}
    >
      <span
        className="h-3 w-3 rounded-full flex-shrink-0"
        style={{
          backgroundColor: MODULE_STATUS.find((s) => s.value === moduleDetails?.status)?.color ?? "#a3a3a2",
        }}
      />
      <h6 className="flex-grow truncate text-13 font-medium">{moduleDetails?.name ?? "未知模块"}</h6>
    </div>
  );
});
