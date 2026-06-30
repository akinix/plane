// FLOW: Forked from Plane cycles/cycle-peek-overview.tsx
// FLOW: CyclePeekOverview — Cycle 详情弹窗/概览
import { observer } from "mobx-react";
import { useCycleDetail } from "@/../src/lib/hooks/use-cycles";
import { ActiveCycleProgress } from "./active-cycle/progress";
import { ActiveCycleStats } from "./active-cycle/cycle-stats";
import { ActiveCycleProductivity } from "./active-cycle/productivity";

type Props = {
  cycleId: string;
  projectId: string;
  isOpen: boolean;
  onClose: () => void;
};

export const CyclePeekOverview = observer(function CyclePeekOverview(props: Props) {
  const { cycleId, projectId, isOpen, onClose } = props;
  const { data: cycle, isLoading } = useCycleDetail(projectId, cycleId);

  if (!isOpen) return null;

  return (
    <div className="fixed right-0 top-0 z-[9] flex h-full w-full max-w-[21.5rem] flex-col gap-3.5 overflow-y-auto border-l border-subtle bg-surface-1 px-4 py-4 duration-300">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-18 font-semibold text-primary">{cycle?.name ?? "周期详情"}</h3>
        <button
          className="flex h-6 w-6 items-center justify-center rounded-full bg-layer-3 hover:bg-layer-3-hover"
          onClick={onClose}
        >
          <svg className="h-4 w-4 text-secondary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      {isLoading ? (
        <div className="space-y-2">
          <div className="h-4 w-1/2 animate-pulse rounded bg-surface-2" />
          <div className="h-4 w-1/3 animate-pulse rounded bg-surface-2" />
          <div className="mt-8 space-y-3">
            <div className="h-8 animate-pulse rounded bg-surface-2" />
            <div className="h-8 animate-pulse rounded bg-surface-2" />
          </div>
        </div>
      ) : cycle ? (
        <div className="flex flex-col gap-4">
          {/* Date range */}
          {cycle.start_date && cycle.end_date && (
            <div className="flex items-center gap-2 text-13 text-tertiary">
              <span>{new Date(cycle.start_date).toLocaleDateString("zh-CN")}</span>
              <span>→</span>
              <span>{new Date(cycle.end_date).toLocaleDateString("zh-CN")}</span>
            </div>
          )}

          {/* Description */}
          {cycle.description && (
            <p className="text-13 text-secondary">{cycle.description}</p>
          )}

          {/* Issue count badge */}
          <div className="flex items-center gap-2">
            <span className="rounded-md bg-layer-1 px-2 py-0.5 text-12 font-medium text-tertiary">
              {cycle.total_issues ?? 0} 个事项
            </span>
            {cycle.status && (
              <span className="rounded-md bg-layer-1 px-2 py-0.5 text-12 capitalize text-tertiary">
                {cycle.status === "current" ? "活跃" : cycle.status === "completed" ? "已完成" : cycle.status}
              </span>
            )}
          </div>

          {/* Progress bar */}
          <ActiveCycleProgress cycle={cycle} />

          {/* Stats */}
          <ActiveCycleStats cycle={cycle} />

          {/* Productivity */}
          <ActiveCycleProductivity cycle={cycle} />
        </div>
      ) : (
        <div className="flex flex-1 items-center justify-center">
          <p className="text-13 text-tertiary">未找到周期数据</p>
        </div>
      )}
    </div>
  );
});
