// FLOW: Issue list view — container with FilterBar, table header, rows, pagination (per D-P17-06)
import { useMemo, useState } from "react";
import { observer } from "mobx-react";
import { Columns3, BookmarkPlus } from "lucide-react";
import { cn } from "@plane/utils";
import { useStore } from "@/lib/store-context";
import { useIssues } from "@/../src/lib/hooks/use-issues";
import { Loader } from "@plane/ui";
import { FilterBar } from "./filters/filter-bar";
import { ColumnSelector } from "./filters/column-selector";
import { FilterSaveModal } from "./filters/filter-save-modal";
import { IssueRow } from "./issue-row";
import { Pagination } from "./pagination";
import { BulkActionBar } from "./bulk-action-bar";

type TProps = {
  workspaceId: string;
  projectId: string;
};

export const IssueListView = observer(function IssueListView({ workspaceId, projectId }: TProps) {
  const store = useStore();
  const [columnSelectorOpen, setColumnSelectorOpen] = useState(false);
  const [saveModalOpen, setSaveModalOpen] = useState(false);

  const { data: issues, isLoading } = useIssues(projectId, {
    state: store.issue.filters.stateIds,
    priority: store.issue.filters.priorityIds,
    assignee: store.issue.filters.assigneeIds,
    searchQuery: store.issue.filters.searchQuery,
  });

  // Client-side sorting
  const sorted = useMemo(() => {
    if (!issues) return [];
    const sortedIssues = [...issues].sort((a, b) => {
      const field = store.issue.sortBy as keyof typeof a;
      const dir = store.issue.sortDirection === "asc" ? 1 : -1;

      const aVal = a[field];
      const bVal = b[field];

      if (aVal == null && bVal == null) return 0;
      if (aVal == null) return 1;
      if (bVal == null) return -1;

      if (typeof aVal === "string" && typeof bVal === "string") {
        return aVal.localeCompare(bVal) * dir;
      }
      if (typeof aVal === "number" && typeof bVal === "number") {
        return (aVal - bVal) * dir;
      }
      return 0;
    });
    return sortedIssues;
  }, [issues, store.issue.sortBy, store.issue.sortDirection]);

  // Pagination
  const totalPages = Math.ceil(sorted.length / store.issue.pageSize);
  const currentPageIssues = sorted.slice(
    (store.issue.currentPage - 1) * store.issue.pageSize,
    store.issue.currentPage * store.issue.pageSize
  );

  const handlePageChange = (page: number) => {
    store.issue.setCurrentPage(page);
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="flex flex-col gap-3 p-4">
        <Loader className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="flex items-center gap-3 px-4">
              <Loader.Item width="16px" height="16px" className="rounded" />
              <Loader.Item width="60px" height="14px" className="rounded" />
              <Loader.Item width="100%" height="14px" className="rounded" />
              <Loader.Item width="40px" height="14px" className="rounded" />
              <Loader.Item width="24px" height="24px" className="rounded-full" />
              <Loader.Item width="80px" height="20px" className="rounded-full" />
              <Loader.Item width="60px" height="14px" className="rounded" />
            </div>
          ))}
        </Loader>
      </div>
    );
  }

  // Empty state — no issues at all
  if (!issues || issues.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <div className="mb-3 rounded-full bg-custom-background-80 p-4">
          <svg className="size-8 text-custom-text-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h3 className="text-sm font-medium text-custom-text-200">暂无 Issue</h3>
        <p className="mt-1 text-xs text-custom-text-400">这个项目还没有创建 Issue，点击上方按钮创建第一个。</p>
      </div>
    );
  }

  // Empty state — filters returned no results
  if (sorted.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <div className="mb-3 rounded-full bg-custom-background-80 p-4">
          <svg className="size-8 text-custom-text-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <h3 className="text-sm font-medium text-custom-text-200">没有匹配的 Issue</h3>
        <p className="mt-1 text-xs text-custom-text-400">没有符合当前筛选条件的 Issue。</p>
        {(store.issue.filters.stateIds.length > 0 ||
          store.issue.filters.priorityIds.length > 0 ||
          store.issue.filters.assigneeIds.length > 0 ||
          store.issue.filters.searchQuery) && (
          <button
            type="button"
            onClick={() => store.issue.clearFilters()}
            className="mt-2 text-xs text-custom-primary hover:underline"
          >
            清除所有筛选条件
          </button>
        )}
      </div>
    );
  }

  return (
    <div className="flex flex-col">
      {/* FilterBar + column selector + save view */}
      <div className="border-b border-custom-border-200 px-4 py-3">
        <div className="flex items-start gap-2">
          <div className="flex-1">
            <FilterBar
              workspaceId={workspaceId}
              projectId={projectId}
              viewType="list"
              showSorting={true}
              showGroupBy={false}
            />
          </div>
          <div className="flex items-center gap-1 shrink-0 pt-2">
            {/* Column selector button */}
            <div className="relative">
              <button
                type="button"
                onClick={() => setColumnSelectorOpen(!columnSelectorOpen)}
                className={cn(
                  "flex items-center gap-1 rounded-md border px-2.5 py-1.5 text-xs transition-colors",
                  columnSelectorOpen
                    ? "border-custom-primary text-custom-primary"
                    : "border-custom-border-200 text-custom-text-300 hover:text-custom-text-200"
                )}
              >
                <Columns3 className="size-3.5" />
                自定义列
              </button>
              <ColumnSelector
                isOpen={columnSelectorOpen}
                onClose={() => setColumnSelectorOpen(false)}
                selectedColumns={store.issue.visibleColumnIds}
                onChange={(ids) => store.issue.setVisibleColumnIds(ids)}
              />
            </div>

            {/* Save view button */}
            <button
              type="button"
              onClick={() => setSaveModalOpen(true)}
              className="flex items-center gap-1 rounded-md border border-custom-border-200 px-2.5 py-1.5 text-xs text-custom-text-300 hover:text-custom-text-200 transition-colors"
            >
              <BookmarkPlus className="size-3.5" />
              保存视图
            </button>
          </div>
        </div>
      </div>

      {/* Table header */}
      <div className="flex h-9 items-center gap-3 border-b border-custom-border-200 px-4 text-xs text-custom-text-400">
        <div className="flex w-4 shrink-0 items-center" />
        <span className="w-20 shrink-0">ID</span>
        <span className="min-w-0 flex-1">标题</span>
        <span className="w-12 shrink-0">优先级</span>
        <span className="w-8 shrink-0">负责人</span>
        <span className="w-24 shrink-0">状态</span>
        <span className="w-20 shrink-0 text-right">更新时间</span>
      </div>

      {/* Issue rows */}
      <div className="flex-1">
        {currentPageIssues.map((issue) => (
          <IssueRow
            key={issue.id}
            issue={issue}
            workspaceId={workspaceId}
            projectId={projectId}
            isSelected={store.issue.selectedIssueIds.includes(issue.id)}
            onToggleSelect={() => store.issue.toggleIssueSelection(issue.id)}
          />
        ))}
      </div>

      {/* Pagination + Bulk action area */}
      <div className="flex items-center justify-between border-t border-custom-border-200 px-4 py-3">
        <span className="text-xs text-custom-text-400">
          共 {sorted.length} 条
        </span>
        <Pagination
          currentPage={store.issue.currentPage}
          totalPages={totalPages}
          onPageChange={handlePageChange}
        />
      </div>

      {/* Bulk action bar */}
      <BulkActionBar
        workspaceId={workspaceId}
        projectId={projectId}
        selectedIds={store.issue.selectedIssueIds}
        onClearSelection={() => store.issue.clearSelection()}
      />

      {/* Filter save modal */}
      <FilterSaveModal
        isOpen={saveModalOpen}
        onClose={() => setSaveModalOpen(false)}
        projectId={projectId}
        workspaceId={workspaceId}
        currentFilters={store.issue.filters}
        currentSort={{ sortBy: store.issue.sortBy, sortDirection: store.issue.sortDirection }}
        currentColumns={store.issue.visibleColumnIds}
        currentLayout="list"
      />
    </div>
  );
});
