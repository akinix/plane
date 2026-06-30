// FLOW: Forked from Plane cycles/cycles-view.tsx
// FLOW: CyclesView — 主视图容器，3 Tab 切换（活跃/已完成/全部）
import { observer } from "mobx-react";
import { useStore } from "@/lib/store-context";
import { useCycles } from "@/../src/lib/hooks/use-cycles";
import { CyclesViewHeader } from "./cycles-view-header";
import { CyclesList } from "./list/root";
import { CycleModal } from "./modal";

export interface ICyclesView {
  workspaceSlug: string;
  projectId: string;
}

export const CyclesView = observer(function CyclesView(props: ICyclesView) {
  const { workspaceSlug, projectId } = props;
  const { cycle: cycleStore } = useStore();
  const { data: cycles, isLoading, error } = useCycles(projectId);

  // Derived filtered lists based on activeTab
  const activeCycles = cycles?.filter((c) => c.status === "current") ?? [];
  const completedCycles = cycles?.filter((c) => c.status === "completed") ?? [];
  const upcomingCycles = cycles?.filter((c) => c.status === "upcoming") ?? [];
  const getFilteredCycleIds = () => {
    switch (cycleStore.activeTab) {
      case "active":
        return activeCycles;
      case "completed":
        return completedCycles;
      case "all":
      default:
        return cycles ?? [];
    }
  };

  const filteredCycles = getFilteredCycleIds();

  // Loading state
  if (isLoading) {
    return (
      <div className="flex h-full w-full flex-col gap-2 p-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={`skeleton-${i}`} className="h-16 animate-pulse rounded-md bg-surface-2" />
        ))}
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="grid h-full w-full place-items-center">
        <div className="text-center">
          <p className="text-14 text-tertiary">加载周期数据失败，请重试</p>
          <button
            className="mt-3 rounded-md bg-accent-primary px-4 py-2 text-13 text-white"
            onClick={() => window.location.reload()}
          >
            重试
          </button>
        </div>
      </div>
    );
  }

  // Empty state
  if (!cycles || cycles.length === 0) {
    return (
      <div className="flex h-full w-full flex-col">
        <CyclesViewHeader projectId={projectId} />
        <div className="grid h-full w-full place-items-center">
          <div className="text-center">
            <h5 className="mt-7 mb-1 text-18 font-medium">暂无周期</h5>
            <p className="text-14 text-placeholder">创建第一个周期来开始迭代管理</p>
          </div>
        </div>
      </div>
    );
  }

  // Empty state per tab
  const emptyStateText = () => {
    switch (cycleStore.activeTab) {
      case "active":
        return "暂无活跃周期";
      case "completed":
        return "暂无已完成周期";
      case "all":
      default:
        return "暂无周期";
    }
  };

  if (filteredCycles.length === 0) {
    return (
      <div className="flex h-full w-full flex-col">
        <CyclesViewHeader projectId={projectId} />
        <div className="grid h-full w-full place-items-center">
          <div className="text-center">
            <h5 className="mt-7 mb-1 text-18 font-medium">{emptyStateText()}</h5>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-full w-full flex-col">
      <CyclesViewHeader projectId={projectId} />
      <div className="flex-1 overflow-y-auto">
        <CyclesList
          completedCycleIds={completedCycles.map((c) => c.id)}
          upcomingCycleIds={upcomingCycles.map((c) => c.id)}
          cycleIds={filteredCycles.map((c) => c.id)}
          workspaceSlug={workspaceSlug}
          projectId={projectId}
        />
      </div>
      <CycleModal />
    </div>
  );
});
