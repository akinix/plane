// FLOW: Module detail page — dual-column layout per MODU-02, D-P18-10, D-P18-11, D-P18-12
"use client";

import { useState } from "react";
import { observer } from "mobx-react";
import { useParams, useNavigate } from "react-router";
import {
  ArrowLeft,
  List,
  GitBranch as GanttIcon,
  Pencil,
  Trash2,
  Plus,
  User,
  Calendar,
  ChevronDown,
} from "lucide-react";
import { cn } from "@plane/utils";
import { format } from "date-fns";
import { zhCN } from "date-fns/locale";
import { useStore } from "@/lib/store-context";
import {
  useModuleDetail,
  useModuleMutations,
} from "@/../src/lib/hooks/use-modules";
import { useModuleIssues, useAddIssueToModule } from "@/../src/lib/hooks/use-module-issues";
import { ModuleStatusDropdown } from "@/components/modules/module-status-dropdown";
import { ModuleLinksList } from "@/components/modules/links/list";
import { ModuleAnalyticsSidebar } from "@/components/modules/analytics-sidebar/root";
import { Loader, ProgressBar } from "@plane/ui";
import { MOCK_MEMBERS } from "@/../src/lib/mock-data";

const ModuleDetailPage = observer(function ModuleDetailPage() {
  const { workspaceId, projectId, moduleId } = useParams<{
    workspaceId: string;
    projectId: string;
    moduleId: string;
  }>();
  const navigate = useNavigate();
  const store = useStore();
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [addIssueOpen, setAddIssueOpen] = useState(false);

  const { data: module, isLoading: moduleLoading, error: moduleError } = useModuleDetail(
    projectId ?? "",
    moduleId ?? ""
  );
  const { data: moduleIssues, isLoading: issuesLoading } = useModuleIssues(
    projectId ?? "",
    moduleId ?? ""
  );
  const { updateModule } = useModuleMutations();
  const addIssueToModule = useAddIssueToModule();

  const activeView = store.module.activeView;

  // Loading state
  if (moduleLoading) {
    return (
      <div className="flex h-full flex-col gap-6 p-6">
        <Loader className="flex gap-6">
          <div className="flex-[2] space-y-4">
            <Loader.Item height="40px" width="60%" />
            <Loader.Item height="200px" />
          </div>
          <div className="w-80 space-y-4">
            <Loader.Item height="30px" />
            <Loader.Item height="30px" />
            <Loader.Item height="30px" />
            <Loader.Item height="200px" />
          </div>
        </Loader>
      </div>
    );
  }

  // Error state
  if (moduleError || !module) {
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

  // Compute progress percentage
  const totalIssues = module.total_issues ?? 0;
  const completedIssues = module.completed_issues ?? 0;
  const cancelledIssues = module.cancelled_issues ?? 0;
  const progressPct =
    totalIssues > 0
      ? Math.round(((completedIssues + cancelledIssues) / totalIssues) * 100)
      : 0;

  // Get lead member info
  const members = MOCK_MEMBERS[workspaceId ?? ""] ?? [];
  const leadMember = module.lead_id
    ? members.find((m) => m.member.id === module.lead_id)
    : undefined;
  const moduleMembers = module.member_ids
    ? members.filter((m) => module.member_ids?.includes(m.member.id))
    : [];

  const handleStatusChange = async (status: string) => {
    if (!moduleId) return;
    await updateModule.mutateAsync({ moduleId, data: { status } as any });
  };

  return (
    <div className="flex h-full flex-col">
      {/* Top navigation bar */}
      <div className="flex items-center gap-2 border-b border-custom-border-200 px-6 py-3">
        <button
          onClick={() =>
            navigate(`/workspaces/${workspaceId}/projects/${projectId}/modules`)
          }
          className="flex items-center gap-1.5 text-sm text-custom-text-300 hover:text-custom-text-100 transition-colors"
          aria-label="返回模块列表"
        >
          <ArrowLeft className="size-4" />
          <span>返回模块列表</span>
        </button>
      </div>

      {/* Dual-column layout */}
      <div className="flex flex-1 gap-6 overflow-y-auto p-6">
        {/* Left column */}
        <div className="flex flex-1 flex-col gap-4">
          {/* Header: Module name + status badge + actions */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h1 className="text-lg font-semibold text-custom-text-100">
                {module.name}
              </h1>
              <span className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-custom-background-80 text-custom-text-200">
                {module.status ?? "待开始"}
              </span>
            </div>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => store.module.openModuleModal("edit")}
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

          {/* View toggle: List / Gantt */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-0.5 rounded-md border border-custom-border-200 bg-custom-background-90 p-0.5">
              <button
                type="button"
                onClick={() => store.module.setActiveView("list")}
                className={cn(
                  "flex items-center gap-1 rounded-sm px-2 py-1 text-xs transition-colors",
                  activeView === "list"
                    ? "bg-custom-background-100 text-custom-text-100 shadow-sm"
                    : "text-custom-text-400 hover:text-custom-text-200"
                )}
              >
                <List className="size-3.5" />
                列表
              </button>
              <button
                type="button"
                onClick={() => store.module.setActiveView("gantt")}
                className={cn(
                  "flex items-center gap-1 rounded-sm px-2 py-1 text-xs transition-colors",
                  activeView === "gantt"
                    ? "bg-custom-background-100 text-custom-text-100 shadow-sm"
                    : "text-custom-text-400 hover:text-custom-text-200"
                )}
              >
                <GanttIcon className="size-3.5" />
                甘特图
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

          {/* Issue list / Gantt view */}
          <div className="flex-1 overflow-auto">
            {issuesLoading ? (
              <div className="flex items-center justify-center h-full text-custom-text-400">
                加载中...
              </div>
            ) : !moduleIssues || moduleIssues.length === 0 ? (
              <div className="flex h-full items-center justify-center">
                <div className="text-center">
                  <h3 className="text-sm font-medium text-custom-text-200">
                    模块中暂无 Issue
                  </h3>
                  <p className="mt-1 text-xs text-custom-text-400">
                    点击"添加 Issue"将 Issue 关联到此模块。
                  </p>
                </div>
              </div>
            ) : activeView === "list" ? (
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
                {moduleIssues.map((issue) => (
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
                      {issue.state_id ?? "-"}
                    </span>
                  </div>
                ))}
              </div>
            ) : (
              /* Gantt view — simplified timeline per D-P18-11 */
              <div className="flex flex-col gap-2 p-4">
                <div className="text-sm text-custom-text-400 mb-2">
                  甘特图视图 (共 {moduleIssues.length} 个 Issue)
                </div>
                {moduleIssues.map((issue) => (
                  <div
                    key={issue.id}
                    onClick={() =>
                      navigate(
                        `/workspaces/${workspaceId}/projects/${projectId}/issues/${issue.id}`
                      )
                    }
                    className="flex cursor-pointer items-center gap-3 rounded-md border border-custom-border-200 bg-custom-background-100 px-3 py-2 text-sm hover:bg-custom-background-80 transition-colors"
                  >
                    <div className="min-w-0 flex-1">
                      <p className="truncate font-medium text-custom-text-100">
                        {issue.name}
                      </p>
                      {issue.target_date && (
                        <p className="mt-0.5 text-xs text-custom-text-400">
                          截止: {format(new Date(issue.target_date), "MM/dd", { locale: zhCN })}
                        </p>
                      )}
                    </div>
                    <span className="text-xs text-custom-text-400">
                      {issue.priority ?? "-"}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Right sidebar — w-80 */}
        <div className="flex w-80 flex-shrink-0 flex-col gap-4">
          {/* Properties panel */}
          <div className="rounded-md border border-custom-border-200 bg-custom-background-90 p-4">
            <h3 className="mb-3 text-sm font-semibold text-custom-text-200">属性</h3>
            <div className="flex flex-col gap-3">
              {/* Status */}
              <div className="flex flex-col gap-1">
                <span className="text-xs text-custom-text-400">状态</span>
                <ModuleStatusDropdown
                  value={module.status}
                  onChange={handleStatusChange}
                />
              </div>

              {/* Lead */}
              <div className="flex flex-col gap-1">
                <span className="text-xs text-custom-text-400">负责人</span>
                <div className="flex items-center gap-2 text-sm text-custom-text-100">
                  <User className="size-3.5 text-custom-text-400" />
                  {leadMember ? (
                    <span>{leadMember.member.display_name}</span>
                  ) : (
                    <span className="text-custom-text-400">未指定</span>
                  )}
                </div>
              </div>

              {/* Members */}
              <div className="flex flex-col gap-1">
                <span className="text-xs text-custom-text-400">成员</span>
                <div className="flex flex-wrap gap-1">
                  {moduleMembers.length > 0 ? (
                    moduleMembers.map((m) => (
                      <div
                        key={m.member.id}
                        className="flex items-center gap-1 rounded-full bg-custom-background-80 px-2 py-0.5 text-xs text-custom-text-200"
                      >
                        {m.member.display_name}
                      </div>
                    ))
                  ) : (
                    <span className="text-xs text-custom-text-400">无成员</span>
                  )}
                </div>
              </div>

              {/* Start / Target dates */}
              <div className="flex flex-col gap-1">
                <span className="text-xs text-custom-text-400">日期</span>
                <div className="flex items-center gap-2 text-sm text-custom-text-100">
                  <Calendar className="size-3.5 text-custom-text-400" />
                  {module.start_date || module.target_date ? (
                    <span>
                      {module.start_date
                        ? format(new Date(module.start_date), "MM/dd", { locale: zhCN })
                        : "..."}
                      {" - "}
                      {module.target_date
                        ? format(new Date(module.target_date), "MM/dd", { locale: zhCN })
                        : "..."}
                    </span>
                  ) : (
                    <span className="text-custom-text-400">未设置</span>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Progress panel */}
          <div className="rounded-md border border-custom-border-200 bg-custom-background-90 p-4">
            <h3 className="mb-3 text-sm font-semibold text-custom-text-200">进度</h3>
            <div className="flex flex-col gap-2">
              <div className="flex items-center gap-2">
                <div className="flex-1">
                  <ProgressBar value={progressPct} />
                </div>
                <span className="text-xs text-custom-text-400">{progressPct}%</span>
              </div>
              <div className="flex justify-between text-xs text-custom-text-400">
                <span>已完成: {completedIssues}</span>
                <span>总数: {totalIssues}</span>
              </div>
            </div>
          </div>

          {/* Description panel */}
          {module.description && (
            <div className="rounded-md border border-custom-border-200 bg-custom-background-90 p-4">
              <h3 className="mb-2 text-sm font-semibold text-custom-text-200">描述</h3>
              <p className="text-sm text-custom-text-300 whitespace-pre-wrap">
                {module.description}
              </p>
            </div>
          )}

          {/* Links panel */}
          <div className="rounded-md border border-custom-border-200 bg-custom-background-90 p-4">
            <h3 className="mb-2 text-sm font-semibold text-custom-text-200">链接</h3>
            <ModuleLinksList moduleId={moduleId ?? ""} projectId={projectId ?? ""} />
          </div>

          {/* Module Analytics */}
          <ModuleAnalyticsSidebar
            moduleId={moduleId ?? ""}
            projectId={projectId ?? ""}
            workspaceSlug={workspaceId ?? ""}
            handleClose={() => {}}
          />
        </div>
      </div>

      {/* Delete confirmation modal */}
      {deleteModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="w-full max-w-md rounded-lg border border-custom-border-200 bg-custom-background-100 p-6 shadow-xl">
            <h3 className="text-lg font-semibold text-custom-text-100">删除模块</h3>
            <p className="mt-2 text-sm text-custom-text-400">
              确定删除此模块？模块内的 Issue 不会被删除。
            </p>
            <div className="mt-6 flex justify-end gap-3">
              <button
                onClick={() => setDeleteModalOpen(false)}
                className="rounded-md border border-custom-border-200 px-4 py-2 text-sm text-custom-text-200 hover:bg-custom-background-80 transition-colors"
              >
                取消
              </button>
              <button
                onClick={async () => {
                  if (!moduleId) return;
                  await updateModule.mutateAsync({
                    moduleId,
                    data: { archived_at: new Date().toISOString() } as any,
                  });
                  setDeleteModalOpen(false);
                  navigate(
                    `/workspaces/${workspaceId}/projects/${projectId}/modules`
                  );
                }}
                className="rounded-md bg-danger-primary px-4 py-2 text-sm text-white hover:bg-danger-primary/90 transition-colors"
              >
                删除
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Add Issue to Module — simplified inline picker */}
      {addIssueOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="w-full max-w-lg rounded-lg border border-custom-border-200 bg-custom-background-100 p-6 shadow-xl">
            <h3 className="text-lg font-semibold text-custom-text-100">添加 Issue 到模块</h3>
            <p className="mt-1 text-sm text-custom-text-400">
              选择已有 Issue 关联到此模块（功能将在 Phase 19 完成）
            </p>
            <div className="mt-6 flex justify-end gap-3">
              <button
                onClick={() => setAddIssueOpen(false)}
                className="rounded-md border border-custom-border-200 px-4 py-2 text-sm text-custom-text-200 hover:bg-custom-background-80 transition-colors"
              >
                关闭
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
});

export default ModuleDetailPage;
