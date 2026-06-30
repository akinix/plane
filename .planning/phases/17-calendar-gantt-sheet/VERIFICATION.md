---
phase: 17-calendar-gantt-sheet
verified: 2026-06-29T22:00:00Z
status: gaps_found
score: 15/17 must-haves verified
overrides_applied: 0
gaps:
  - truth: "用户可通过拖拽调整 Issue 日期（日历视图）"
    status: failed
    reason: "CalendarViewWrapper.handleDragAndDrop 为 no-op 空实现，拖拽後無任何實際效果"
    artifacts:
      - path: "yh-flow/clients/web/app/issues/page.tsx"
        issue: "CalendarViewWrapper 中的 handleDragAndDrop 回調為空函數 (lines 56-66)"
    missing:
      - "實現 handleDragAndDrop 以實際更新 Issue 日期並同步到 API/backend"
  - truth: "用戶可查看 Issue 依賴關係連線（甘特圖視圖）"
    status: failed
    reason: "GanttChartRoot 傳入 enableDependency={false}，SVG 依賴線未渲染；getDependencies 函數被定義但從未被調用"
    artifacts:
      - path: "yh-flow/clients/web/app/components/issues/gantt-view/gantt-view.tsx"
        issue: "enableDependency={false} (line 230) 禁用依賴線渲染；getDependencies (lines 67-74) 定義但未被使用"
    missing:
      - "設置 enableDependency 為 true 或依賴注入正確的依賴關係數據"
      - "將 getDependencies 的返回值接入 GanttBlock 的依賴線渲染"
deferred:
  - truth: "保存的視圖可在 Phase 19 中管理和應用"
    addressed_in: "Phase 19"
    evidence: "ROADMAP.md SC5 明確說明 '保存的視圖在 Phase 19 中管理和應用'。Phase 17 已實作 FilterSaveModal 和 IssueViewService 保存功能"
---

# Phase 17: 日曆/甘特/電子表格 & 篩選引擎 Verification Report

**Phase Goal:** 用戶可使用多種 Issue 視圖和可復用的篩選/排序/自定義列引擎
**Verified:** 2026-06-29T22:00:00Z
**Status:** gaps_found
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| #   | Truth                                        | Status     | Evidence                                                                                                                    |
| --- | -------------------------------------------- | ---------- | --------------------------------------------------------------------------------------------------------------------------- |
| 1   | 日曆視圖元件存在且可在 Issue 頁面切換        | VERIFIED   | `calendar-view/calendar-view.tsx` (210 lines) + page.tsx lines 150-154 lazy-loaded via React.lazy                           |
| 2   | Issue 按截止日期在日曆月格上展示             | VERIFIED   | calendar-view.tsx lines 96-110: 按 target_date 分組到對應日期格                                                             |
| 3   | **用戶可拖拽調整 Issue 日期（日曆）**        | **FAILED** | CalendarViewWrapper.handleDragAndDrop (page.tsx lines 56-66) is no-op; 拖拽基礎設施存在但無實際效果                         |
| 4   | 甘特圖視圖元件存在且可在 Issue 頁面切換      | VERIFIED   | `gantt-view/gantt-view.tsx` (240 lines) + page.tsx lines 155-159 lazy-loaded                                                |
| 5   | 甘特圖橫向時間軸展示 Issue 跨度，支援4級縮放 | VERIFIED   | gantt-view.tsx lines 88-97 (日期過濾排序) + chart/views/ 包含 day/week/month/quarter 四級縮放                               |
| 6   | **可查看 Issue 依賴關係連線（甘特圖）**      | **FAILED** | enableDependency={false} (gantt-view.tsx line 230); getDependencies 定義 (67-74) 但從未被調用                               |
| 7   | 電子表格視圖元件存在且可在 Issue 頁面切換    | VERIFIED   | `spreadsheet-view/spreadsheet-view.tsx` (107 lines) + base-spreadsheet-root.tsx data fetching                               |
| 8   | 電子表格支援內聯編輯 Issue 屬性              | VERIFIED   | spreadsheet-view.tsx lines 59-70 (800ms debounce onChange handler); 8 editable column types with native select/date         |
| 9   | 可按狀態/優先級/負責人/標籤組合篩選          | VERIFIED   | FilterBar (filter-bar.tsx) + useFilters (URL params sync) + extended panel with label multi-select + date range             |
| 10  | 可按任意欄位升序/降序排序                    | VERIFIED   | FilterBar showSorting prop + useSorting hook; 4 sort fields (created_at/updated_at/priority/sequence_id)                    |
| 11  | ColumnSelector 可自定義各視圖顯示列          | VERIFIED   | column-selector.tsx (110 lines) + store.issue.visibleColumnIds + "自定義列" button in list-view.tsx                         |
| 12  | FilterSaveModal 可保存篩選/排序/列配置為視圖 | VERIFIED   | filter-save-modal.tsx (128 lines) + issue-view.service.ts (mock) + useCreateIssueView mutation                              |
| 13  | 看板子分組/Swimlane (KANB-04)                | VERIFIED   | kanban-view.tsx lines 319-411: sub-group (state/priority/none) with separate Droppable containers                           |
| 14  | React.lazy + Suspense 懶加載 4 個視圖        | VERIFIED   | page.tsx lines 14-28: 4 lazy() calls; lines 146-163: Suspense fallback "加载中..."                                          |
| 15  | 5 視圖切換按鈕整合在 Issue 頁面              | VERIFIED   | page.tsx lines 31-37: VIEW_OPTIONS (列表/看板/日曆/甘特/表格) with icons; lines 104-126: toggle buttons                     |
| 16  | QuickFilterBar 已棄用/替換（無殘留導入）     | VERIFIED   | quick-filter-bar.tsx line 1: "DEPRECATED" comment; grep 確認無其他文件導入 QuickFilterBar; list-view.tsx 改用 FilterBar     |
| 17  | Filter engine types + hooks 完整實作         | VERIFIED   | types.ts (7 types), use-filters.ts, use-sorting.ts, use-group-by.ts, use-sub-group-by.ts, IssueViewService, use-issue-views |

**Score:** 15/17 truths verified

### Deferred Items

Items not yet met but explicitly addressed in later milestone phases.

| #   | Item                    | Addressed In | Evidence                                                                     |
| --- | ----------------------- | ------------ | ---------------------------------------------------------------------------- |
| 1   | 保存的視圖管理和應用 UI | Phase 19     | ROADMA.md SC5: "保存的視圖在 Phase 19 中管理和應用"。Phase 17 已實作保存功能 |

### Required Artifacts

| Artifact                                              | Expected                  | Status   | Details                                                                                                                                                                                |
| ----------------------------------------------------- | ------------------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app/components/issues/calendar-view/`                | 13 files                  | VERIFIED | 12 files + README: calendar-view, header, day-tile, issue-block, week-days, week-header, utils, issue-blocks, issue-block-root, quick-add-issue-actions, base-calendar-root, constants |
| `app/components/issues/gantt-view/`                   | 39 files                  | VERIFIED | Full fork: blocks/, chart/, sidebar/, views/ (day/week/month/quarter), contexts/, data/, helpers/, hooks/, root.tsx                                                                    |
| `app/components/issues/spreadsheet-view/`             | 24 files                  | VERIFIED | 7 core + 16 columns (15 components + index.ts registry) + README                                                                                                                       |
| `app/components/issues/filters/types.ts`              | 7 types                   | VERIFIED | TFilterCriteria, TSortConfig, TGroupByOptions, TSubGroupByOptions, TViewLayout, TIssueView, TColumnVisibility                                                                          |
| `app/components/issues/filters/use-filters.ts`        | TanStack Query + URL sync | VERIFIED | useFilters hook with parseFiltersFromParams/serializeFiltersToParams                                                                                                                   |
| `app/components/issues/filters/use-sorting.ts`        | sort config + client sort | VERIFIED | useSorting with URL sync + sortedIssues function                                                                                                                                       |
| `app/components/issues/filters/use-group-by.ts`       | 5 group modes             | VERIFIED | state/priority/assignees/created_by/none with groupIssues function                                                                                                                     |
| `app/components/issues/filters/use-sub-group-by.ts`   | 3 sub-group modes         | VERIFIED | state/priority/none with subGroupIssues function                                                                                                                                       |
| `app/components/issues/filters/filter-bar.tsx`        | 405 lines                 | VERIFIED | Search, quick chips (state/priority/assignee), extended panel (labels/date), sort row, group-by row, active chips                                                                      |
| `app/components/issues/filters/column-selector.tsx`   | 117 lines                 | VERIFIED | 14 columns checkbox list, reset, backdrop dismiss                                                                                                                                      |
| `app/components/issues/filters/filter-save-modal.tsx` | 129 lines                 | VERIFIED | ModalCore dialog, name input, layout display, save mutation, form validation                                                                                                           |
| `src/lib/services/issue-view.service.ts`              | 84 lines                  | VERIFIED | FlowApiService extension, mock CRUD (createIssueView, getIssueViews, etc.)                                                                                                             |
| `src/lib/hooks/use-issue-views.ts`                    | 19 lines                  | VERIFIED | useIssueViews query + useCreateIssueView mutation with cache invalidation                                                                                                              |
| `app/hooks/use-view-switcher.ts`                      | 46 lines                  | ORPHANED | Exists but not imported or used by any component; page.tsx has inline VIEW_OPTIONS                                                                                                     |
| `app/store/issue.store.ts`                            | Extended store            | VERIFIED | activeView: TViewLayout, visibleColumnIds, filters: TFilterCriteria, groupBy: TGroupByOptions + all actions                                                                            |

### Key Link Verification

| From                       | To                          | Via                     | Status    | Details                                                                                       |
| -------------------------- | --------------------------- | ----------------------- | --------- | --------------------------------------------------------------------------------------------- |
| page.tsx                   | CalendarView                | React.lazy + Suspense   | WIRED     | `IssuesCalendarView` imported via lazy (line 18), rendered in Suspense (lines 150-154)        |
| page.tsx                   | GanttView                   | React.lazy + Suspense   | WIRED     | `IssuesGanttView` imported via lazy (lines 20-22), rendered in Suspense (lines 155-159)       |
| page.tsx                   | BaseSpreadsheetRoot         | React.lazy + Suspense   | WIRED     | `IssuesSpreadsheetView` imported via lazy (lines 24-28), rendered in Suspense (lines 160-164) |
| page.tsx                   | IssueStore                  | MobX store.issue        | WIRED     | `store.issue.setActiveView()` and `store.issue.activeView` in page.tsx (lines 90-95, 108)     |
| page.tsx                   | CalendarViewWrapper         | useIssues + useStore    | WIRED     | CalendarViewWrapper (lines 41-77) fetches data and passes props to IssuesCalendarView         |
| list-view.tsx              | FilterBar                   | Import + render         | WIRED     | list-view.tsx lines 9, 138-144: FilterBar with showSorting=true                               |
| list-view.tsx              | ColumnSelector              | Import + render         | WIRED     | list-view.tsx lines 10, 162-168: ColumnSelector in list-view                                  |
| list-view.tsx              | FilterSaveModal             | Import + render         | WIRED     | list-view.tsx lines 11, 229-238: FilterSaveModal wired to store                               |
| kanban-view.tsx            | FilterBar                   | Import + render         | WIRED     | kanban-view.tsx lines 12, 240-246: FilterBar with showGroupBy=true                            |
| kanban-view.tsx            | useSubGroupBy               | Import + render         | WIRED     | kanban-view.tsx lines 13, 79: useSubGroupBy, lines 320-397: swimlane rendering                |
| FilterBar                  | useFilters                  | URL params sync         | WIRED     | filter-bar.tsx lines 5, 57: useFilters hook, sync to URL via setSearchParams                  |
| FilterBar                  | useSorting                  | URL params sync         | WIRED     | filter-bar.tsx lines 6, 59: useSorting hook                                                   |
| FilterSaveModal            | useCreateIssueView          | TanStack Query mutation | WIRED     | filter-save-modal.tsx lines 7, 40: createIssueView mutation call                              |
| ColumnSelector             | IssueStore.visibleColumnIds | MobX state              | WIRED     | list-view.tsx lines 165-166: selectedColumns + onChange bound to store                        |
| GanttView → GanttChartRoot | enableDependency            | Prop                    | NOT_WIRED | enableDependency={false} (line 230) — dependency lines explicitly disabled                    |

### Data-Flow Trace (Level 4)

| Artifact         | Data Variable                       | Source                                | Produces Real Data | Status              |
| ---------------- | ----------------------------------- | ------------------------------------- | ------------------ | ------------------- |
| CalendarView     | `issues` (from CalendarViewWrapper) | useIssues hook with URL filter params | MOCK_DATA          | FLOWING (mock data) |
| GanttView        | `issues`                            | useIssues hook with filter params     | MOCK_DATA          | FLOWING (mock data) |
| SpreadsheetView  | `issues` (from BaseSpreadsheetRoot) | useIssues hook with filter params     | MOCK_DATA          | FLOWING (mock data) |
| FilterBar        | `filters` (from useFilters)         | URL search params                     | URL params         | FLOWING             |
| FilterSaveModal  | `currentFilters`/`currentSort`      | IssueStore.filters                    | Store state        | FLOWING             |
| IssueViewService | `MOCK_VIEWS`                        | In-memory array                       | MOCK DATA          | FLOWING (mock)      |

### Behavioral Spot-Checks

Step 7b: SKIPPED (no runnable entry point — frontend requires Vite dev server + node_modules)

### Probe Execution

No probe scripts found for Phase 17. Skipped.

### Requirements Coverage

| Requirement | Source  | Description                        | Status   | Evidence                                                                 |
| ----------- | ------- | ---------------------------------- | -------- | ------------------------------------------------------------------------ |
| CALN-01     | ROADMAP | Calendar view shows issues by date | VERIFIED | CalendarView groups issues by target_date on month grid                  |
| CALN-02     | ROADMAP | Drag to adjust dates in calendar   | FAILED   | handleDragAndDrop is no-op in CalendarViewWrapper                        |
| GANT-01     | ROADMAP | Gantt chart view with timeline     | VERIFIED | GanttView with 4 zoom levels, issue spans by start_date/target_date      |
| GANT-02     | ROADMAP | Issue dependency lines in gantt    | FAILED   | enableDependency={false}; getDependencies defined but never invoked      |
| SHEE-01     | ROADMAP | Spreadsheet view for issues        | VERIFIED | SpreadsheetView with column registry, header, rows                       |
| SHEE-02     | ROADMAP | Inline edit in spreadsheet         | VERIFIED | 8 editable column types (native select/date) + 800ms debounce            |
| FILT-01     | ROADMAP | Filter issues by criteria          | VERIFIED | FilterBar + useFilters with state/priority/assignee/labels/date range    |
| FILT-02     | ROADMAP | Sort issues                        | VERIFIED | useSorting + FilterBar showSorting with 4 fields + asc/desc              |
| FILT-03     | ROADMAP | Custom display columns             | VERIFIED | ColumnSelector component with 14 columns, bound to IssueStore            |
| FILT-04     | ROADMAP | Save view configuration            | VERIFIED | FilterSaveModal + IssueViewService (mock) + useCreateIssueView           |
| KANB-04     | ROADMAP | Kanban subgroup/swimlane           | VERIFIED | kanban-view.tsx: subGroupBy state/priority/none with Droppable swimlanes |

### Anti-Patterns Found

| File                                    | Line        | Pattern           | Severity | Impact                                                                                               |
| --------------------------------------- | ----------- | ----------------- | -------- | ---------------------------------------------------------------------------------------------------- |
| `page.tsx`                              | 56-66       | No-op callback    | WARNING  | Calendar drag infrastructure exists but handler is empty — user drags with no visual feedback        |
| `gantt-view/gantt-view.tsx`             | 67-74       | Dead code         | INFO     | `getDependencies` function fully implemented but never called                                        |
| `gantt-view/gantt-view.tsx`             | 230         | Feature gated off | WARNING  | `enableDependency={false}` disables dependency line rendering                                        |
| `use-view-switcher.ts`                  | entire file | Orphaned artifact | INFO     | Hook exists but page.tsx uses inline VIEW_OPTIONS instead                                            |
| `spreadsheet-view/spreadsheet-view.tsx` | 37-39       | Dead config       | INFO     | DEFAULT_DISPLAY_FILTERS variable defined but only used for type assertion                            |
| `spreadsheet-view/spreadsheet-view.tsx` | 88          | No-op handler     | INFO     | `handleDisplayFilterUpdate={() => {}}` — filter updates are no-ops pending filter engine integration |

### Human Verification Required

None. All remaining uncertainties are documented as gaps or deferred items.

### Gaps Summary

2 gaps found, 0 blockers. Phase goal is largely achieved — all 5 views are visible and switchable, the filter engine works across views, and the save-view pipeline is functional. Two visual features are not wired:

1. **Calendar drag-to-adjust (CALN-02)**: The drag-and-drop infrastructure (DragDropContext, Droppable day tiles, Draggable issue blocks) is fully implemented, but the handler in CalendarViewWrapper (page.tsx lines 56-66) is a no-op. Users can initiate a drag but no date change occurs. This requires implementing the actual issue date update logic and API sync.

2. **Gantt dependency lines (GANT-02)**: The dependency data (`issue_relation`/`blocked_by`) exists in mock data, and `getDependencies` function is defined with correct logic, but `enableDependency={false}` prevents SVG line rendering in GanttChartRoot. The Plane-originated SVG rendering code exists in the forked block components but is gated off. Requires setting enableDependency to true and connecting getDependencies output to the block rendering pipeline.

Both gaps are acknowledged in the SUMMARY files as intentional deferrals but are not addressed in any later phase's success criteria in ROADMAP.md.

---

_Verified: 2026-06-29T22:00:00Z_
_Verifier: Claude (gsd-verifier)_
