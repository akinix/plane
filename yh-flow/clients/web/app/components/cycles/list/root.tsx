// FLOW: Forked from Plane cycles/list/root.tsx
// FLOW: CyclesList — Cycle 列表根容器，按状态分组渲染
import { observer } from "mobx-react";
import { useStore } from "@/lib/store-context";
import { useCycles } from "@/../src/lib/hooks/use-cycles";
import { CycleListGroupHeader } from "./cycle-list-group-header";
import { CyclesListMap } from "./cycles-list-map";

export interface ICyclesList {
  completedCycleIds: string[];
  upcomingCycleIds?: string[];
  cycleIds: string[];
  workspaceSlug: string;
  projectId: string;
  isArchived?: boolean;
}

export const CyclesList = observer(function CyclesList(props: ICyclesList) {
  const { completedCycleIds, upcomingCycleIds, cycleIds, workspaceSlug, projectId, isArchived = false } = props;
  const { data: cycles } = useCycles(projectId);

  const activeCycleIds = cycles?.filter((c) => c.status === "current").map((c) => c.id) ?? [];
  const draftCycleIds = cycles?.filter((c) => c.status === "draft").map((c) => c.id) ?? [];

  return (
    <div className="flex flex-col gap-0">
      {/* Active cycles section */}
      {activeCycleIds.length > 0 && (
        <div className="flex flex-col pb-2">
          <CycleListGroupHeader title="当前活跃周期" type="current" count={activeCycleIds.length} showCount isExpanded />
          <CyclesListMap cycleIds={activeCycleIds} projectId={projectId} workspaceSlug={workspaceSlug} />
        </div>
      )}

      {/* Upcoming cycles section */}
      {upcomingCycleIds && upcomingCycleIds.length > 0 && (
        <div className="flex flex-col pb-2">
          <CycleListGroupHeader title="即将开始" type="upcoming" count={upcomingCycleIds.length} showCount isExpanded />
          <CyclesListMap cycleIds={upcomingCycleIds} projectId={projectId} workspaceSlug={workspaceSlug} />
        </div>
      )}

      {/* Draft cycles section */}
      {draftCycleIds.length > 0 && (
        <div className="flex flex-col pb-2">
          <CycleListGroupHeader title="草稿" type="draft" count={draftCycleIds.length} showCount isExpanded />
          <CyclesListMap cycleIds={draftCycleIds} projectId={projectId} workspaceSlug={workspaceSlug} />
        </div>
      )}

      {/* Completed cycles section */}
      {completedCycleIds.length > 0 && (
        <div className="flex flex-col pb-7">
          <CycleListGroupHeader title="已完成周期" type="completed" count={completedCycleIds.length} showCount isExpanded />
          <CyclesListMap cycleIds={completedCycleIds} projectId={projectId} workspaceSlug={workspaceSlug} />
        </div>
      )}
    </div>
  );
});
