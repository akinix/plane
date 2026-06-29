// FLOW: Quick filter bar for Issue list (per D-P16-06)
import { observer } from "mobx-react";
import { Filter, Search, X } from "lucide-react";
import { useState } from "react";
import { cn } from "@plane/utils";
import { MOCK_MEMBERS, MOCK_STATES } from "@/../src/lib/mock-data";
import { Dropdown as SingleSelectDropdown } from "@plane/ui";
import type { TDropdownOption } from "@plane/ui";
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

  const stateOptions: TDropdownOption[] = projectStates.map((s) => ({
    value: s.id,
    data: { name: s.name, color: s.color },
    query: s.name,
  }));

  // Get members for this workspace
  const workspaceMembers = MOCK_MEMBERS[workspaceId] ?? [];
  const assigneeOptions: TDropdownOption[] = workspaceMembers.map((m) => ({
    value: m.member.id,
    data: {
      name: m.member.display_name,
      avatar: m.member.avatar_url,
    },
  }));

  const priorityOptions: TDropdownOption[] = [
    { value: "urgent", data: { name: "紧急", color: "#EF4444" } },
    { value: "high", data: { name: "高", color: "#F59E0B" } },
    { value: "medium", data: { name: "中", color: "#3B82F6" } },
    { value: "low", data: { name: "低", color: "#6B7280" } },
    { value: "none", data: { name: "无", color: "#A3A3A3" } },
  ];

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
          <Search className="pointer-events-none absolute left-3 top-1/2 size-3.5 -translate-y-1/2 text-custom-text-400" />
          <input
            type="text"
            placeholder="搜索 Issue..."
            value={store.filters.searchQuery}
            onChange={(e) => store.setFilters({ searchQuery: e.target.value })}
            className="w-full rounded-md border border-custom-border-200 bg-custom-background-90 py-2 pl-9 pr-3 text-sm text-custom-text-100 outline-none placeholder:text-custom-text-400 focus:border-custom-primary"
          />
          {store.filters.searchQuery && (
            <button
              type="button"
              onClick={() => store.setFilters({ searchQuery: "" })}
              className="absolute right-2 top-1/2 -translate-y-1/2 text-custom-text-400 hover:text-custom-text-200"
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
              className="appearance-none rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 pr-8 text-xs text-custom-text-200 outline-none focus:border-custom-primary"
            >
              <option value="">所有状态</option>
              {projectStates.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
            <Filter className="pointer-events-none absolute right-2 top-1/2 size-3 -translate-y-1/2 text-custom-text-400" />
          </div>

          {/* Priority filter chip */}
          <div className="relative">
            <select
              value={store.filters.priorityIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                store.setFilters({ priorityIds: val ? [val] : [] });
              }}
              className="appearance-none rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 pr-8 text-xs text-custom-text-200 outline-none focus:border-custom-primary"
            >
              <option value="">所有优先级</option>
              <option value="urgent">紧急</option>
              <option value="high">高</option>
              <option value="medium">中</option>
              <option value="low">低</option>
              <option value="none">无</option>
            </select>
            <Filter className="pointer-events-none absolute right-2 top-1/2 size-3 -translate-y-1/2 text-custom-text-400" />
          </div>

          {/* Assignee filter chip */}
          <div className="relative">
            <select
              value={store.filters.assigneeIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                store.setFilters({ assigneeIds: val ? [val] : [] });
              }}
              className="appearance-none rounded-md border border-custom-border-200 bg-custom-background-90 px-3 py-2 pr-8 text-xs text-custom-text-200 outline-none focus:border-custom-primary"
            >
              <option value="">所有负责人</option>
              {workspaceMembers.map((m) => (
                <option key={m.member.id} value={m.member.id}>
                  {m.member.display_name}
                </option>
              ))}
            </select>
            <Filter className="pointer-events-none absolute right-2 top-1/2 size-3 -translate-y-1/2 text-custom-text-400" />
          </div>

          {/* Extended filter toggle */}
          <button
            type="button"
            onClick={() => setShowExtended(!showExtended)}
            className={cn(
              "flex items-center gap-1 rounded-md border px-3 py-2 text-xs transition-colors",
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
              className="flex items-center gap-1 rounded-md px-3 py-2 text-xs text-custom-text-400 hover:text-custom-text-200"
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
                className="flex items-center gap-1 rounded-full border border-custom-border-200 bg-custom-background-80 px-2 py-0.5 text-xs text-custom-text-200"
              >
                <span className="size-1.5 rounded-full" style={{ backgroundColor: state.color }} />
                {state.name}
                <button type="button" onClick={() => store.setFilters({ stateIds: store.filters.stateIds.filter((sid) => sid !== id) })}>
                  <X className="size-3 text-custom-text-400 hover:text-custom-text-200" />
                </button>
              </span>
            );
          })}
          {store.filters.priorityIds.map((id) => (
            <span
              key={id}
              className="flex items-center gap-1 rounded-full border border-custom-border-200 bg-custom-background-80 px-2 py-0.5 text-xs text-custom-text-200"
            >
              {id === "urgent" && "紧急"}
              {id === "high" && "高"}
              {id === "medium" && "中"}
              {id === "low" && "低"}
              {id === "none" && "无"}
              <button type="button" onClick={() => store.setFilters({ priorityIds: store.filters.priorityIds.filter((pid) => pid !== id) })}>
                <X className="size-3 text-custom-text-400 hover:text-custom-text-200" />
              </button>
            </span>
          ))}
          {store.filters.assigneeIds.map((id) => {
            const member = workspaceMembers.find((m) => m.member.id === id);
            if (!member) return null;
            return (
              <span
                key={id}
                className="flex items-center gap-1 rounded-full border border-custom-border-200 bg-custom-background-80 px-2 py-0.5 text-xs text-custom-text-200"
              >
                {member.member.display_name}
                <button type="button" onClick={() => store.setFilters({ assigneeIds: store.filters.assigneeIds.filter((aid) => aid !== id) })}>
                  <X className="size-3 text-custom-text-400 hover:text-custom-text-200" />
                </button>
              </span>
            );
          })}
        </div>
      )}
    </div>
  );
});
