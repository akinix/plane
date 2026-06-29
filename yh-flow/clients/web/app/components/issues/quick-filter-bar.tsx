// DEPRECATED: 此组件将被 filter-bar.tsx 替换。请在 Phase 17 完成前迁移。
// FLOW: Quick filter bar for Issue list (per D-P16-06)
import { observer } from "mobx-react";
import { Filter, Search, X } from "lucide-react";
import { useState } from "react";
import { cn } from "@plane/utils";
import { MOCK_MEMBERS, MOCK_STATES } from "@/../src/lib/mock-data";
import type { IIssueStore } from "@/store/types";

type TProps = {
  store: IIssueStore;
  workspaceId: string;
  projectId: string;
};

export const QuickFilterBar = observer(function QuickFilterBar({ store, workspaceId, projectId }: TProps) {
  const [showExtended, setShowExtended] = useState(false);

  // Get states for this project
  const projectStates = MOCK_STATES.filter((s) => s.project_id === projectId);

  // Get members for this workspace
  const workspaceMembers = MOCK_MEMBERS[workspaceId] ?? [];

  const clearFilters = () => {
    store.clearFilters();
  };

  const hasActiveFilters =
    store.filters.stateIds.length > 0 ||
    store.filters.priorityIds.length > 0 ||
    store.filters.assigneeIds.length > 0 ||
    store.filters.searchQuery.length > 0;

  return (
    <div className="flex flex-col gap-2">
      {/* Primary row: Search + Quick filter chips */}
      <div className="flex items-center gap-2">
        {/* Search */}
        <div className="relative flex-1">
          <Search className="text-custom-text-400 pointer-events-none absolute top-1/2 left-3 size-3.5 -translate-y-1/2" />
          <input
            type="text"
            placeholder="搜索 Issue..."
            value={store.filters.searchQuery}
            onChange={(e) => store.setFilters({ searchQuery: e.target.value })}
            className="border-custom-border-200 bg-custom-background-90 text-sm text-custom-text-100 placeholder:text-custom-text-400 focus:border-custom-primary w-full rounded-md border py-2 pr-3 pl-9 outline-none"
          />
          {store.filters.searchQuery && (
            <button
              type="button"
              onClick={() => store.setFilters({ searchQuery: "" })}
              className="text-custom-text-400 hover:text-custom-text-200 absolute top-1/2 right-2 -translate-y-1/2"
            >
              <X className="size-3.5" />
            </button>
          )}
        </div>

        {/* Filter chips */}
        <div className="flex items-center gap-2">
          {/* State filter chip */}
          <div className="relative">
            <select
              value={store.filters.stateIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                store.setFilters({ stateIds: val ? [val] : [] });
              }}
              className="border-custom-border-200 bg-custom-background-90 text-xs text-custom-text-200 focus:border-custom-primary appearance-none rounded-md border px-3 py-2 pr-8 outline-none"
            >
              <option value="">所有状态</option>
              {projectStates.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
            <Filter className="text-custom-text-400 pointer-events-none absolute top-1/2 right-2 size-3 -translate-y-1/2" />
          </div>

          {/* Priority filter chip */}
          <div className="relative">
            <select
              value={store.filters.priorityIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                store.setFilters({ priorityIds: val ? [val] : [] });
              }}
              className="border-custom-border-200 bg-custom-background-90 text-xs text-custom-text-200 focus:border-custom-primary appearance-none rounded-md border px-3 py-2 pr-8 outline-none"
            >
              <option value="">所有优先级</option>
              <option value="urgent">紧急</option>
              <option value="high">高</option>
              <option value="medium">中</option>
              <option value="low">低</option>
              <option value="none">无</option>
            </select>
            <Filter className="text-custom-text-400 pointer-events-none absolute top-1/2 right-2 size-3 -translate-y-1/2" />
          </div>

          {/* Assignee filter chip */}
          <div className="relative">
            <select
              value={store.filters.assigneeIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                store.setFilters({ assigneeIds: val ? [val] : [] });
              }}
              className="border-custom-border-200 bg-custom-background-90 text-xs text-custom-text-200 focus:border-custom-primary appearance-none rounded-md border px-3 py-2 pr-8 outline-none"
            >
              <option value="">所有负责人</option>
              {workspaceMembers.map((m) => (
                <option key={m.member.id} value={m.member.id}>
                  {m.member.display_name}
                </option>
              ))}
            </select>
            <Filter className="text-custom-text-400 pointer-events-none absolute top-1/2 right-2 size-3 -translate-y-1/2" />
          </div>

          {/* Extended filter toggle */}
          <button
            type="button"
            onClick={() => setShowExtended(!showExtended)}
            className={cn(
              "text-xs flex items-center gap-1 rounded-md border px-3 py-2 transition-colors",
              showExtended
                ? "border-custom-primary text-custom-primary"
                : "border-custom-border-200 text-custom-text-300 hover:text-custom-text-200"
            )}
          >
            <Filter className="size-3" />
            更多
          </button>

          {/* Clear filters */}
          {hasActiveFilters && (
            <button
              type="button"
              onClick={clearFilters}
              className="text-xs text-custom-text-400 hover:text-custom-text-200 flex items-center gap-1 rounded-md px-3 py-2"
            >
              <X className="size-3" />
              清除
            </button>
          )}
        </div>
      </div>

      {/* Active filter chips display */}
      {(store.filters.stateIds.length > 0 ||
        store.filters.priorityIds.length > 0 ||
        store.filters.assigneeIds.length > 0) && (
        <div className="flex flex-wrap items-center gap-1.5">
          {store.filters.stateIds.map((id) => {
            const state = projectStates.find((s) => s.id === id);
            if (!state) return null;
            return (
              <span
                key={id}
                className="border-custom-border-200 bg-custom-background-80 text-xs text-custom-text-200 flex items-center gap-1 rounded-full border px-2 py-0.5"
              >
                <span className="size-1.5 rounded-full" style={{ backgroundColor: state.color }} />
                {state.name}
                <button
                  type="button"
                  onClick={() => store.setFilters({ stateIds: store.filters.stateIds.filter((sid) => sid !== id) })}
                >
                  <X className="text-custom-text-400 hover:text-custom-text-200 size-3" />
                </button>
              </span>
            );
          })}
          {store.filters.priorityIds.map((id) => (
            <span
              key={id}
              className="border-custom-border-200 bg-custom-background-80 text-xs text-custom-text-200 flex items-center gap-1 rounded-full border px-2 py-0.5"
            >
              {id === "urgent" && "紧急"}
              {id === "high" && "高"}
              {id === "medium" && "中"}
              {id === "low" && "低"}
              {id === "none" && "无"}
              <button
                type="button"
                onClick={() => store.setFilters({ priorityIds: store.filters.priorityIds.filter((pid) => pid !== id) })}
              >
                <X className="text-custom-text-400 hover:text-custom-text-200 size-3" />
              </button>
            </span>
          ))}
          {store.filters.assigneeIds.map((id) => {
            const member = workspaceMembers.find((m) => m.member.id === id);
            if (!member) return null;
            return (
              <span
                key={id}
                className="border-custom-border-200 bg-custom-background-80 text-xs text-custom-text-200 flex items-center gap-1 rounded-full border px-2 py-0.5"
              >
                {member.member.display_name}
                <button
                  type="button"
                  onClick={() =>
                    store.setFilters({ assigneeIds: store.filters.assigneeIds.filter((aid) => aid !== id) })
                  }
                >
                  <X className="text-custom-text-400 hover:text-custom-text-200 size-3" />
                </button>
              </span>
            );
          })}
        </div>
      )}
    </div>
  );
});
