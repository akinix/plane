---
phase: 17-calendar-gantt-sheet
plan: 05
type: execute
subsystem: "Filter UI Components"
tags: ["filter", "ui", "components", "column-selector", "view-save"]
requires: [17-01]
provides: [FILT-03, FILT-04]
affects: ["IssueList", "KanbanView", "CalendarView", "GanttView", "SpreadsheetView"]
tech-stack:
  added: []
  patterns: ["TanStack Query hooks for view CRUD", "ModalCore-based modal pattern"]
key-files:
  created:
    - "yh-flow/clients/web/app/components/issues/filters/filter-bar.tsx"
    - "yh-flow/clients/web/app/components/issues/filters/column-selector.tsx"
    - "yh-flow/clients/web/app/components/issues/filters/filter-save-modal.tsx"
    - "yh-flow/clients/web/src/lib/services/issue-view.service.ts"
    - "yh-flow/clients/web/src/lib/hooks/use-issue-views.ts"
  modified:
    - "yh-flow/clients/web/app/components/issues/quick-filter-bar.tsx"
  removed: []
decisions:
  - "FilterBar uses useFilters/useSorting hooks (URL params) instead of MobX store"
  - "Group-by state managed locally via useState, not via useGroupBy hook (no issues needed)"
  - "Toast integration deferred — no toast system exists in project yet"
  - "IssueViewService extends FlowApiService but mock layer uses in-memory storage"
metrics:
  duration: ~35min
  completed_date: "2026-06-29"
  tasks_completed: 2 / 2
  files_created: 5
  files_modified: 1
---

# Phase 17 Plan 05: Filter UI Components Summary

**Objective:** 构建筛选引擎 UI 组件：FilterBar 筛选条、ColumnSelector 列选择器、FilterSaveModal 保存视图模态框，以及 IssueView API 服务层。

**Result:** 5 files created, 1 file modified. All components compile with zero new TypeScript errors.

## Tasks Completed

### Task 1: FilterBar 组件 (commit 5c14059db)

Created standalone FilterBar component in `filters/filter-bar.tsx`:

- **Quick filter row**: Search input with clear button, state/priority/assignee native select dropdowns, "更多" toggle for extended panel, "清除" link (shown only when filters active)
- **Active filter chips**: Removable chips for state (color dot + name), priority (Chinese label), assignee (name), labels (color dot + name)
- **Sort row** (optional via `showSorting` prop): Sort field buttons with active state highlight and direction arrow (created_at/updated_at/priority/sequence_id)
- **GroupBy row** (optional via `showGroupBy` prop): Group option buttons (state/priority/assignees/created_by/none)
- **Extended panel**: Label multi-select toggles with color indicators + date range (start/end date inputs)
- Uses `useFilters()` for URL-param-synced filter state and `useSorting()` for sort state
- Internal `useState` for group-by selection (no issues query required)
- Clears extended panel on "清除 all"
- **Deprecation**: Added `// DEPRECATED` comment to `quick-filter-bar.tsx` with migration instructions
- File: 405 lines (requirement: min 100)

### Task 2: ColumnSelector + FilterSaveModal + IssueView 服务 + Hooks (commit 1232e5250)

Created 4 files:

**ColumnSelector** (`column-selector.tsx`, 117 lines):

- Overlay/panel with checkbox list of 14 columns (state/priority/assignee/start_date/target_date/labels/cycle/module/estimate/created_at/updated_at/attachments/links/sub_issues)
- Chinese labels for each column
- "重置" button restores default columns (state/priority/assignee)
- "确定" button closes panel
- Backdrop click to dismiss

**FilterSaveModal** (`filter-save-modal.tsx`, 129 lines):

- ModalCore-based dialog with "保存为视图" header
- Name input (with htmlFor a11y) and read-only layout type display
- Save button calls `useCreateIssueView()` mutation with full filter/sort/column/layout config
- Disables save when name is empty or mutation is pending
- Resets form on close

**IssueView Service** (`issue-view.service.ts`, 84 lines):

- Extends `FlowApiService` for future API migration
- `createIssueView()`: mock in-memory implementation with auto-generated IDs
- `getIssueViews()`: filters mock array by projectId
- `getIssueView()` and `deleteIssueView()` for completeness
- Singleton export for convenience

**use-issue-views Hooks** (`use-issue-views.ts`, 19 lines):

- `useIssueViews(projectId)`: TanStack Query `useQuery` fetching views
- `useCreateIssueView()`: TanStack Query `useMutation` creating views with `onSuccess` cache invalidation

## Key Decisions

1. **URL params drive filters**: FilterBar uses `useFilters()` hook (URL search params) rather than MobX store, consistent with the Plan 17 filter architecture.
2. **No useGroupBy hook dependency**: The group-by row uses local `useState` since the `useGroupBy` hook requires issues array. Group selection state is sufficient for FilterBar.
3. **Toast deferred**: Plan mentions "显示 toast" but the project has no toast system. Marked as `// FUTURE` comment. Toast system will be added globally when needed.
4. **FlowApiService for future migration**: IssueViewService extends `FlowApiService` so switching from mock to real API requires only uncommented API calls.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing] Pre-commit hook lint warnings in quick-filter-bar.tsx**

- **Found during:** Task 1 commit
- **Issue:** Pre-existing unused imports (`Dropdown as SingleSelectDropdown`, `TDropdownOption`) and unused variables (`stateOptions`, `assigneeOptions`, `priorityOptions`) blocked the pre-commit hook (oxlint --deny-warnings)
- **Fix:** Removed unused imports and variables
- **Files modified:** `yh-flow/clients/web/app/components/issues/quick-filter-bar.tsx`
- **Commit:** 5c14059db

**2. [Rule 2 - Lint] Unused `viewType` prop in FilterBar**

- **Found during:** Task 1 commit (oxlint)
- **Issue:** `viewType` prop is required by design but not yet consumed internally
- **Fix:** Renamed to `_viewType` convention to suppress unused-var warning
- **Files modified:** `yh-flow/clients/web/app/components/issues/filters/filter-bar.tsx`
- **Commit:** 5c14059db

**3. [Rule 2 - A11y] Unassociated labels in FilterSaveModal**

- **Found during:** Task 2 commit (oxlint jsx-a11y)
- **Issue:** Labels lacked `htmlFor` attribute association
- **Fix:** Added `htmlFor="view-name"` + `id="view-name"`, changed layout label to `<span>`
- **Files modified:** `yh-flow/clients/web/app/components/issues/filters/filter-save-modal.tsx`
- **Commit:** 1232e5250

**4. [Rule 2 - Lint] Unused `X` import in ColumnSelector**

- **Found during:** Task 2 commit (oxlint)
- **Issue:** Imported `X` from lucide-react but never used
- **Fix:** Removed unused import
- **Files modified:** `yh-flow/clients/web/app/components/issues/filters/column-selector.tsx`
- **Commit:** 1232e5250

## Known Stubs

None. All components are fully implemented with mock data (intentional per plan design — API layer will replace mock in later phases).

## Threat Flags

None. No new security-relevant surface introduced — all components are frontend-only UI with mock backend.

## TypeScript Compilation

- Baseline: 89 pre-existing errors
- After Plan 17-05: 88 errors (error count decreased due to quick-filter-bar cleanup)
- **Zero new errors** in created/modified files

## Verification

```bash
# TSC check
cd yh-flow/clients/web && npx tsc --noEmit  # 88 errors (pre-existing)

# File existence
ls -la app/components/issues/filters/filter-bar.tsx        # 405 lines
ls -la app/components/issues/filters/column-selector.tsx   # 117 lines
ls -la app/components/issues/filters/filter-save-modal.tsx # 129 lines
ls -la src/lib/services/issue-view.service.ts              # 84 lines
ls -la src/lib/hooks/use-issue-views.ts                    # 19 lines
```

## Success Criteria

- [x] FilterBar component complete (search/quick filters/chips/extended panel/sort/group-by)
- [x] ColumnSelector component complete (column checkboxes/reset/confirm)
- [x] FilterSaveModal component complete (save filter/sort/column/layout as view)
- [x] IssueView mock service layer complete
- [x] useIssueViews + useCreateIssueView hooks complete
- [x] `npx tsc --noEmit` zero new errors

## Self-Check

All created files exist and compile without new errors.

## Deferred Items

- Toast notification system — needed when FilterSaveModal save completes (marked FUTURE)
- API migration from mock to real endpoints (FlowApiService base ready, implementation pending)
