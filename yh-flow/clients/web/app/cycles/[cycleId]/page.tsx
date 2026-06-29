// FLOW: Cycle detail page — dual-column layout per CYCLE-02, D-P18-09, D-P18-10
"use client";

import { useState, useMemo } from "react";
import { observer } from "mobx-react";
import { useParams, useNavigate } from "react-router";
import { ArrowLeft, List, Columns3, Pencil, Trash2, Plus, Kanban } from "lucide-react";
import { cn } from "@plane/utils";
import { useStore } from "@/lib/store-context";
import { useCycleDetail, useCycleMutations, useCycleProgress } from "@/../src/lib/hooks/use-cycles";
import { useCycleIssues, useAddIssueToCycle } from "@/../src/lib/hooks/use-cycle-issues";
import { ActiveCycleProgress } from "@/components/cycles/active-cycle/progress";
import { BurndownChart } from "@/components/cycles/analytics-sidebar/burndown-chart";
import { CycleDeleteModal } from "@/components/cycles/delete-modal";
import { Loader } from "@plane/ui";
import { format, differenceInDays } from "date-fns";
import { zhCN } from "date-fns/locale";

const CycleDetailPage = observer(function CycleDetailPage() {
  const { workspaceId, projectId, cycleId } = useParams<{
    workspaceId: string;
    projectId: string;
    cycleId: string;
  }>();
  const navigate = useNavigate();
  const store = useStore();
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [addIssueOpen, setAddIssueOpen] = useState(false);

  const { data: cycle, isLoading: cycleLoading, error: cycleError } = useCycleDetail(
    projectId ?? "",
    cycleId ?? ""
  );
  const { data: progress } = useCycleProgress(projectId ?? "", cycleId ?? "");
  const { data: cycleIssues, isLoading: issuesLoading } = useCycleIssues(
    projectId ?? "",
    cycleId ?? ""
  );
  const { updateCycle, deleteCycle } = useCycleMutations();
  const addIssueToCycle = useAddIssueToCycle();

  const viewLayout = store.cycle.viewLayout;

  // Loading state
  if (cycleLoading) {
    return (
      <div className="flex h-full flex-col gap-6 p-6">
        <Loader className="flex gap-6">
          <div className="flex-[2] space-y-4">
            <Loader.Item height="40px" width="60%" />
            <Loader.Item height="200px" />
          </div>
          <div className="w-80 space-y-4">
            <Loader.Item height="30px" />
            <Loader.Item height="200px" />
          </div>
        </Loader>
      </div>
    );
  }

  // Error state
  if (cycleError || !cycle) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-center">
          <p className="text-sm text-custom-text-400">加载失败，请稍后重试。</p>
          <button
            onClick={() => window.location.reload()}
            className="mt-2 text-xs text-custom-primary hover:underline"
          >
            重试
          </button>
        </div>
      </div>
    );
  }

  // Prepare burndown data
  const totalDays = cycle.start_date && cycle.end_date
    ? Math.max(1, differenceInDays(new Date(cycle.end_date), new Date(cycle.start_date)))
    : 14;
  const burndownData = Array.from({ length: totalDays }, (_, i) => ({
    date: cycle.start_date
      ? format(new Date(new Date(cycle.start_date).getTime() + i * 86400000), "MM/dd")
      : "",
    ideal: cycle.total_issues - (cycle.total_issues / totalDays) * i,
    actual: Math.max(0, cycle.total_issues - (cycle.completed_issues ?? 0) * (i / totalDays)),
  }));

  // Format date range display
  const dateRangeText =
    cycle.start_date && cycle.end_date
      ? `${format(new Date(cycle.start_date), "MM/dd", { locale: zhCN })} - ${format(new Date(cycle.end_date), "MM/dd", { locale: zhCN })}`
      : "";

  return (
    <div className="flex h-full flex-col">
      {/* Top navigation bar */}
      <div className="flex items-center gap-2 border-b border-custom-border-200 px-6 py-3">
        <button
          onClick={() => navigate(`/workspaces/${workspaceId}/projects/${projectId}/cycles`)}
          className="flex items-center gap-1.5 text-sm text-custom-text-300 hover:text-custom-text-100 transition-colors"
          aria-label="返回周期列表"
        >
          <ArrowLeft className="size-4" />
          <span>返回周期列表</span>
        </button>
      </div>

      {/* Dual-column layout */}
      <div className="flex flex-1 gap-6 overflow-y-auto p-6">
        {/* Left column - flex-1 */}
        <div className="flex flex-1 flex-col gap-4">
          {/* Header: Cycle name + meta + actions */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h1 className="text-lg font-semibold text-custom-text-100">
                {cycle.name}
              </h1>
              {dateRangeText && (
                <span className="text-sm text-custom-text-400">{dateRangeText}</span>
              )}
            </div>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => store.cycle.openCycleModal("edit")}
                className="flex items-center gap-1 rounded-md px-2 py-1.5 text-sm text-custom-text-300 hover:text-custom-text-100 transition-colors"
              >
                <Pencil className="size-4" />
              </button>
              <button
                type="button"
                onClick={() => setDeleteModalOpen(true)}
                className="flex items-center gap-1 rounded-md px-2 py-1.5 text-sm text-danger-primary hover:bg-danger-subtle transition-colors"
              >
                <Trash2 className="size-4" />
              </button>
            </div>
          </div>

          {/* View toggle: List / Board */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-0.5 rounded-md border border-custom-border-200 bg-custom-background-90 p-0.5">
              <button
                type="button"
                onClick={() => store.cycle.setViewLayout("list")}
                className={cn(
                  "flex items-center gap-1 rounded-sm px-2 py-1 text-xs transition-colors",
                  viewLayout === "list"
                    ? "bg-custom-background-100 text-custom-text-100 shadow-sm"
                    : "text-custom-text-400 hover:text-custom-text-200"
                )}
              >
                <List className="size-3.5" />
                列表
              </button>
              <button
                type="button"
                onClick={() => store.cycle.setViewLayout("board")}
                className={cn(
                  "flex items-center gap-1 rounded-sm px-2 py-1 text-xs transition-colors",
                  viewLayout === "board"
                    ? "bg-custom-background-100 text-custom-text-100 shadow-sm"
                    : "text-custom-text-400 hover:text-custom-text-200"
                )}
              >
                <Kanban className="size-3.5" />
                看板
              </button>
            </div>

            <button
              type="button"
              onClick={() => setAddIssueOpen(true)}
              className="flex items-center gap-1.5 rounded-md bg-custom-primary px-3 py-2 text-sm font-medium text-white hover:bg-custom-primary/90 transition-colors"
            >
              <Plus className="size-4" />
              添加 Issue
            </button>
          </div>

          {/* Issue list / Board */}
          <div className="flex-1 overflow-auto">
            {issuesLoading ? (
              <div className="flex items-center justify-center h-full text-custom-text-400">
                加载中...
              </div>
            ) : !cycleIssues || cycleIssues.length === 0 ? (
              <div className="flex h-full items-center justify-center">
                <div className="text-center">
                  <h3 className="text-sm font-medium text-custom-text-200">
                    Cycle 中暂无 Issue
                  </h3>
                  <p className="mt-1 text-xs text-custom-text-400">
                    点击"添加 Issue"将 Issue 关联到此 Cycle。
                  </p>
                </div>
              </div>
            ) : viewLayout === "list" ? (
              <div className="flex flex-col">
                {/* List header */}
                <div className="flex h-10 items-center gap-3 border-b border-custom-border-200 px-4 text-xs text-custom-text-400">
                  <span className="w-8 shrink-0" />
                  <span className="w-20 shrink-0">编号</span>
                  <span className="min-w-0 flex-1">名称</span>
                  <span className="w-12 shrink-0">优先级</span>
                  <span className="w-8 shrink-0">负责人</span>
                  <span className="w-24 shrink-0">状态</span>
                </div>
                {/* Issue rows */}
                {cycleIssues.map((issue) => {
                  const state = issue.state_id;
                  return (
                    <div
                      key={issue.id}
                      onClick={() =>
                        navigate(
                          `/workspaces/${workspaceId}/projects/${projectId}/issues/${issue.id}`
                        )
                      }
                      className="flex h-11 cursor-pointer items-center gap-3 border-b border-custom-border-200 px-4 text-sm transition-colors hover:bg-custom-background-80"
                    >
                      <span className="w-8 shrink-0 text-xs text-custom-text-400">
                        {issue.sequence_id}
                      </span>
                      <span className="w-20 shrink-0 text-xs text-custom-text-400 font-mono">
                        {issue.project_id}-{issue.sequence_id}
                      </span>
                      <span className="min-w-0 flex-1 truncate text-custom-text-100">
                        {issue.name}
                      </span>
                      <span className="w-12 shrink-0 text-xs text-custom-text-400">
                        {issue.priority ?? "-"}
                      </span>
                      <span className="w-8 shrink-0">
                        <div className="size-6 rounded-full bg-custom-background-80 flex items-center justify-center text-[10px] text-custom-text-400">
                          {issue.assignee_ids?.[0]?.slice(-2) ?? "-"}
                        </div>
                      </span>
                      <span className="w-24 shrink-0 text-xs text-custom-text-400">
                        {state ?? "-"}
                      </span>
                    </div>
                  );
                })}
              </div>
            ) : (
              /* Kanban view — grouped by state per D-P18-09 */
              <div className="flex h-full gap-4 overflow-x-auto p-4">
                {["待处理", "进行中", "已完成"].map((status) => {
                  const statusIssues = cycleIssues.filter(
                    (i) => (i.state_id === status || false)
                  );
                  return (
                    <div
                      key={status}
                      className="flex min-w-[280px] flex-col gap-2 rounded-md border border-custom-border-200 bg-custom-background-90 p-3"
                    >
                      <h4 className="text-sm font-medium text-custom-text-200">
                        {status}
                        <span className="ml-2 text-xs text-custom-text-400">
                          {statusIssues.length}
                        </span>
                      </h4>
                      <div className="flex flex-col gap-2">
                        {statusIssues.map((issue) => (
                          <div
                            key={issue.id}
                            onClick={() =>
                              navigate(
                                `/workspaces/${workspaceId}/projects/${projectId}/issues/${issue.id}`
                              )
                            }
                            className="cursor-pointer rounded-md border border-custom-border-200 bg-custom-background-100 p-3 text-sm hover:bg-custom-background-80 transition-colors"
                          >
                            <p className="font-medium text-custom-text-100">
                              {issue.name}
                            </p>
                            <p className="mt-1 text-xs text-custom-text-400">
                              {issue.priority ?? "-"}
                            </p>
                          </div>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>

        {/* Right sidebar column — w-80 */}
        <div className="flex w-80 flex-shrink-0 flex-col gap-4">
          {/* Progress */}
          <div className="rounded-md border border-custom-border-200 bg-custom-background-90 p-4">
            <h3 className="mb-3 text-sm font-semibold text-custom-text-200">进度</h3>
            <ActiveCycleProgress cycle={cycle} />
          </div>

          {/* Burndown chart */}
          {cycle.start_date && cycle.end_date && (
            <div className="rounded-md border border-custom-border-200 bg-custom-background-90 p-4">
              <h3 className="mb-3 text-sm font-semibold text-custom-text-200">Burndown 图</h3>
              <BurndownChart
                data={burndownData}
                startDate={cycle.start_date}
                endDate={cycle.end_date}
                totalIssues={cycle.total_issues}
              />
            </div>
          )}

          {/* Cycle stats */}
          <div className="rounded-md border border-custom-border-200 bg-custom-background-90 p-4">
            <h3 className="mb-3 text-sm font-semibold text-custom-text-200">周期统计</h3>
            <div className="flex flex-col gap-2 text-sm">
              <div className="flex justify-between">
                <span className="text-custom-text-400">已完成 Issue</span>
                <span className="text-custom-text-100">{cycle.completed_issues}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-custom-text-400">Issue 总数</span>
                <span className="text-custom-text-100">{cycle.total_issues}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-custom-text-400">进行中 Issue</span>
                <span className="text-custom-text-100">{cycle.started_issues}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-custom-text-400">待处理 Issue</span>
                <span className="text-custom-text-100">{cycle.unstarted_issues}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-custom-text-400">已取消 Issue</span>
                <span className="text-custom-text-100">{cycle.cancelled_issues}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-custom-text-400">待办 Issue</span>
                <span className="text-custom-text-100">{cycle.backlog_issues}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Delete Modal */}
      <CycleDeleteModal
        cycle={cycle}
        isOpen={deleteModalOpen}
        handleClose={() => setDeleteModalOpen(false)}
        workspaceSlug={workspaceId ?? ""}
        projectId={projectId ?? ""}
      />
    </div>
  );
});

export default CycleDetailPage;
