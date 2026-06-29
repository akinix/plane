// FLOW: FilterBar — shared filter component for all 5 views (per D-P17-09, D-P17-10)
import { useState, useMemo, useCallback } from "react";
import { Search, X, Filter, ArrowUpDown, ChevronDown } from "lucide-react";
import { cn } from "@plane/utils";
import { useFilters } from "./use-filters";
import { useSorting } from "./use-sorting";
import type { TViewLayout, TGroupByOptions } from "./types";
import { MOCK_MEMBERS, MOCK_STATES, MOCK_LABELS } from "@/../src/lib/mock-data";

const PRIORITY_LABELS: Record<string, string> = {
  urgent: "紧急",
  high: "高",
  medium: "中",
  low: "低",
  none: "无",
};

const PRIORITY_COLORS: Record<string, string> = {
  urgent: "#EF4444",
  high: "#F59E0B",
  medium: "#3B82F6",
  low: "#6B7280",
  none: "#A3A3A3",
};

const SORT_FIELDS = [
  { key: "created_at", label: "创建时间" },
  { key: "updated_at", label: "更新时间" },
  { key: "priority", label: "优先级" },
  { key: "sequence_id", label: "Issue ID" },
] as const;

const GROUP_OPTIONS: { key: TGroupByOptions; label: string }[] = [
  { key: "state", label: "按状态" },
  { key: "priority", label: "按优先级" },
  { key: "assignees", label: "按负责人" },
  { key: "created_by", label: "按创建人" },
  { key: "none", label: "无" },
];

type TFilterBarProps = {
  workspaceId: string;
  projectId: string;
  viewType: TViewLayout;
  showSorting?: boolean;
  showGroupBy?: boolean;
};

export function FilterBar({
  workspaceId,
  projectId,
  viewType: _viewType,
  showSorting = false,
  showGroupBy = false,
}: TFilterBarProps) {
  // Filter state from URL params
  const { filters, setFilters, clearFilters, hasActiveFilters } = useFilters(projectId);
  // Sort state from URL params
  const { sortConfig, setSortBy } = useSorting();
  // Group-by local state
  const [groupBy, setGroupBy] = useState<TGroupByOptions>("state");

  const [showExtended, setShowExtended] = useState(false);

  // Get project data
  const projectStates = useMemo(() => MOCK_STATES.filter((s) => s.project_id === projectId), [projectId]);
  const workspaceMembers = useMemo(() => MOCK_MEMBERS[workspaceId] ?? [], [workspaceId]);
  const projectLabels = useMemo(() => MOCK_LABELS.filter((l) => l.project_id === projectId), [projectId]);

  const handleClear = useCallback(() => {
    clearFilters();
    setShowExtended(false);
  }, [clearFilters]);

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
            value={filters.searchQuery}
            onChange={(e) => setFilters({ searchQuery: e.target.value })}
            className="border-custom-border-200 bg-custom-background-90 text-sm text-custom-text-100 placeholder:text-custom-text-400 focus:border-custom-primary w-full rounded-md border py-2 pr-3 pl-9 outline-none"
          />
          {filters.searchQuery && (
            <button
              type="button"
              onClick={() => setFilters({ searchQuery: "" })}
              className="text-custom-text-400 hover:text-custom-text-200 absolute top-1/2 right-2 -translate-y-1/2"
            >
              <X className="size-3.5" />
            </button>
          )}
        </div>

        {/* Filter chips */}
        <div className="flex items-center gap-2">
          {/* State filter */}
          <div className="relative">
            <select
              value={filters.stateIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                setFilters({ stateIds: val ? [val] : [] });
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

          {/* Priority filter */}
          <div className="relative">
            <select
              value={filters.priorityIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                setFilters({ priorityIds: val ? [val] : [] });
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

          {/* Assignee filter */}
          <div className="relative">
            <select
              value={filters.assigneeIds[0] ?? ""}
              onChange={(e) => {
                const val = e.target.value;
                setFilters({ assigneeIds: val ? [val] : [] });
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
              onClick={handleClear}
              className="text-xs text-custom-text-400 hover:text-custom-text-200 flex items-center gap-1 rounded-md px-3 py-2"
            >
              <X className="size-3" />
              清除
            </button>
          )}
        </div>
      </div>

      {/* Active filter chips display */}
      {(filters.stateIds.length > 0 ||
        filters.priorityIds.length > 0 ||
        filters.assigneeIds.length > 0 ||
        filters.labelIds.length > 0) && (
        <div className="flex flex-wrap items-center gap-1.5">
          {/* State chips */}
          {filters.stateIds.map((id) => {
            const state = projectStates.find((s) => s.id === id);
            if (!state) return null;
            return (
              <span
                key={`state-${id}`}
                className="border-custom-border-200 bg-custom-background-80 text-xs text-custom-text-200 flex items-center gap-1 rounded-full border px-2 py-0.5"
              >
                <span className="size-1.5 rounded-full" style={{ backgroundColor: state.color }} />
                {state.name}
                <button
                  type="button"
                  onClick={() => setFilters({ stateIds: filters.stateIds.filter((sid) => sid !== id) })}
                >
                  <X className="text-custom-text-400 hover:text-custom-text-200 size-3" />
                </button>
              </span>
            );
          })}
          {/* Priority chips */}
          {filters.priorityIds.map((id) => (
            <span
              key={`priority-${id}`}
              className="border-custom-border-200 bg-custom-background-80 text-xs text-custom-text-200 flex items-center gap-1 rounded-full border px-2 py-0.5"
            >
              <span className="size-1.5 rounded-full" style={{ backgroundColor: PRIORITY_COLORS[id] ?? "#A3A3A3" }} />
              {PRIORITY_LABELS[id] ?? id}
              <button
                type="button"
                onClick={() => setFilters({ priorityIds: filters.priorityIds.filter((pid) => pid !== id) })}
              >
                <X className="text-custom-text-400 hover:text-custom-text-200 size-3" />
              </button>
            </span>
          ))}
          {/* Assignee chips */}
          {filters.assigneeIds.map((id) => {
            const member = workspaceMembers.find((m) => m.member.id === id);
            if (!member) return null;
            return (
              <span
                key={`assignee-${id}`}
                className="border-custom-border-200 bg-custom-background-80 text-xs text-custom-text-200 flex items-center gap-1 rounded-full border px-2 py-0.5"
              >
                {member.member.display_name}
                <button
                  type="button"
                  onClick={() => setFilters({ assigneeIds: filters.assigneeIds.filter((aid) => aid !== id) })}
                >
                  <X className="text-custom-text-400 hover:text-custom-text-200 size-3" />
                </button>
              </span>
            );
          })}
          {/* Label chips */}
          {filters.labelIds.map((id) => {
            const label = projectLabels.find((l) => l.id === id);
            if (!label) return null;
            return (
              <span
                key={`label-${id}`}
                className="border-custom-border-200 bg-custom-background-80 text-xs text-custom-text-200 flex items-center gap-1 rounded-full border px-2 py-0.5"
              >
                <span className="size-1.5 rounded-full" style={{ backgroundColor: label.color ?? "#A3A3A3" }} />
                {label.name}
                <button
                  type="button"
                  onClick={() => setFilters({ labelIds: filters.labelIds.filter((lid) => lid !== id) })}
                >
                  <X className="text-custom-text-400 hover:text-custom-text-200 size-3" />
                </button>
              </span>
            );
          })}
        </div>
      )}

      {/* Sort row (optional via showSorting prop) */}
      {showSorting && (
        <div className="border-custom-border-200 flex items-center gap-2 border-t pt-2">
          <ArrowUpDown className="text-custom-text-400 size-3.5" />
          <span className="text-xs text-custom-text-400">排序：</span>
          <div className="flex items-center gap-1">
            {SORT_FIELDS.map((field) => (
              <button
                key={field.key}
                type="button"
                onClick={() => setSortBy(field.key)}
                className={cn(
                  "text-xs rounded-md px-2 py-1 transition-colors",
                  sortConfig.sortBy === field.key
                    ? "bg-custom-primary/10 text-custom-primary"
                    : "text-custom-text-300 hover:text-custom-text-100"
                )}
              >
                {field.label}
                {sortConfig.sortBy === field.key && (
                  <span className="ml-1">{sortConfig.sortDirection === "asc" ? "↑" : "↓"}</span>
                )}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* GroupBy row (optional via showGroupBy prop) */}
      {showGroupBy && (
        <div className="border-custom-border-200 flex items-center gap-2 border-t pt-2">
          <ChevronDown className="text-custom-text-400 size-3.5" />
          <span className="text-xs text-custom-text-400">分组：</span>
          <div className="flex items-center gap-1">
            {GROUP_OPTIONS.map((opt) => (
              <button
                key={opt.key}
                type="button"
                onClick={() => setGroupBy(opt.key)}
                className={cn(
                  "text-xs rounded-md px-2 py-1 transition-colors",
                  groupBy === opt.key
                    ? "bg-custom-primary/10 text-custom-primary"
                    : "text-custom-text-300 hover:text-custom-text-100"
                )}
              >
                {opt.label}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Extended filter panel: labels + date range */}
      {showExtended && (
        <div className="border-custom-border-200 bg-custom-background-90 flex flex-col gap-3 rounded-md border p-3">
          {/* Label multi-select */}
          <div>
            <span className="text-xs text-custom-text-300 mb-1.5 block font-medium">标签</span>
            <div className="flex flex-wrap gap-2">
              {projectLabels.map((label) => {
                const isSelected = filters.labelIds.includes(label.id);
                return (
                  <button
                    key={label.id}
                    type="button"
                    onClick={() => {
                      const next = isSelected
                        ? filters.labelIds.filter((lid) => lid !== label.id)
                        : [...filters.labelIds, label.id];
                      setFilters({ labelIds: next });
                    }}
                    className={cn(
                      "text-xs flex items-center gap-1.5 rounded-md border px-2.5 py-1 transition-colors",
                      isSelected
                        ? "border-custom-primary bg-custom-primary/10 text-custom-primary"
                        : "border-custom-border-200 text-custom-text-300 hover:border-custom-border-300"
                    )}
                  >
                    <span className="size-2 rounded-full" style={{ backgroundColor: label.color ?? "#A3A3A3" }} />
                    {label.name}
                  </button>
                );
              })}
              {projectLabels.length === 0 && <span className="text-xs text-custom-text-400">暂无标签</span>}
            </div>
          </div>

          {/* Date range */}
          <div>
            <span className="text-xs text-custom-text-300 mb-1.5 block font-medium">创建日期范围</span>
            <div className="flex items-center gap-2">
              <input
                type="date"
                value={filters.dateRange?.start ?? ""}
                onChange={(e) =>
                  setFilters({
                    dateRange: {
                      start: e.target.value || undefined,
                      end: filters.dateRange?.end,
                    },
                  })
                }
                className="border-custom-border-200 bg-custom-background-100 text-xs text-custom-text-200 focus:border-custom-primary rounded-md border px-2.5 py-1.5 outline-none"
              />
              <span className="text-xs text-custom-text-400">至</span>
              <input
                type="date"
                value={filters.dateRange?.end ?? ""}
                onChange={(e) =>
                  setFilters({
                    dateRange: {
                      start: filters.dateRange?.start,
                      end: e.target.value || undefined,
                    },
                  })
                }
                className="border-custom-border-200 bg-custom-background-100 text-xs text-custom-text-200 focus:border-custom-primary rounded-md border px-2.5 py-1.5 outline-none"
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
