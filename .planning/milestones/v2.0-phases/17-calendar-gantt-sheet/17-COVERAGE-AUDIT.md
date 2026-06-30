---
phase: 17-calendar-gantt-sheet
type: coverage-audit
created: 2026-06-29
---

# Phase 17 — Multi-Source Coverage Audit

## Source Type Audit

### GOAL (ROADMAP phase goal)

| #   | Goal Item                                               | Covered By                                                               | Status  |
| --- | ------------------------------------------------------- | ------------------------------------------------------------------------ | ------- |
| G1  | Issue 日历视图（按截止日期/开始日期展示，拖拽调整日期） | Plan 02: Task 1+2 (CalendarView fork + calendar-view.tsx)                | COVERED |
| G2  | Issue 甘特图视图（横向时间轴，Issue 依赖关系连线）      | Plan 03: Task 1+2 (GanttChart fork + gantt-view.tsx)                     | COVERED |
| G3  | Issue 电子表格视图（表格批量编辑，内联编辑属性）        | Plan 04: Task 1+2 (Spreadsheet fork + spreadsheet-view.tsx)              | COVERED |
| G4  | 按状态/优先级/负责人/标签筛选 + 排序                    | Plan 01: useFilters/useSorting, Plan 05: FilterBar, Plan 06: integration | COVERED |
| G5  | 自定义列 + 保存视图配置                                 | Plan 05: ColumnSelector + FilterSaveModal + IssueView service            | COVERED |

### REQ (phase_req_ids from REQUIREMENTS.md)

| Req ID  | Description     | Covered By                                                    | Status  |
| ------- | --------------- | ------------------------------------------------------------- | ------- |
| CALN-01 | 日历视图展示    | Plan 02: Task 1+2                                             | COVERED |
| CALN-02 | 拖拽调整日期    | Plan 02: Task 2 (DragDrop onDragEnd)                          | COVERED |
| GANT-01 | 甘特图视图      | Plan 03: Task 1+2                                             | COVERED |
| GANT-02 | Issue 依赖关系  | Plan 03: Task 2 (blocked_by SVG lines)                        | COVERED |
| SHEE-01 | 电子表格视图    | Plan 04: Task 1+2                                             | COVERED |
| SHEE-02 | 内联编辑        | Plan 04: Task 2 (column editors + auto-save)                  | COVERED |
| FILT-01 | 筛选 Issues     | Plan 01: useFilters, Plan 05: FilterBar, Plan 06: integration | COVERED |
| FILT-02 | 排序 Issues     | Plan 01: useSorting, Plan 05: FilterBar, Plan 06: integration | COVERED |
| FILT-03 | 自定义列        | Plan 05: ColumnSelector                                       | COVERED |
| FILT-04 | 保存视图配置    | Plan 05: FilterSaveModal + issue-view.service                 | COVERED |
| KANB-04 | 子分组/Swimlane | Plan 01: useSubGroupBy, Plan 06: kanban subgroup integration  | COVERED |

### RESEARCH (implicit features/constraints from CONTEXT.md decisions)

| D-ID     | Description                      | Covered By                                             | Status  |
| -------- | -------------------------------- | ------------------------------------------------------ | ------- |
| D-P17-01 | 日历视图 Fork Plane              | Plan 02                                                | COVERED |
| D-P17-02 | 甘特图 Fork Plane                | Plan 03                                                | COVERED |
| D-P17-03 | 电子表格 Fork Plane              | Plan 04                                                | COVERED |
| D-P17-04 | 不引入第三方视图库               | All plans use Plane forks only                         | COVERED |
| D-P17-05 | 延用 Phase 16 Issue Layout HOC   | Plan 06 (extend page.tsx to 5 views)                   | COVERED |
| D-P17-06 | 日历按月范围懒加载               | Plan 02: Task 2 (date-fns range in CalendarView)       | COVERED |
| D-P17-07 | 甘特图依赖连线                   | Plan 03: Task 2                                        | COVERED |
| D-P17-08 | 电子表格单击内联编辑             | Plan 04: Task 2 (column editors)                       | COVERED |
| D-P17-09 | 可复用筛选引擎                   | Plan 01: types + hooks, Plan 05: FilterBar             | COVERED |
| D-P17-10 | 快速筛选 + 展开面板              | Plan 05: FilterBar (quick chips + "更多"button)        | COVERED |
| D-P17-11 | TanStack Query + URL 同步        | Plan 01: useFilters, useSorting URL sync pattern       | COVERED |
| D-P17-12 | useGroupBy + useSubGroupBy hooks | Plan 01: Task 3                                        | COVERED |
| D-P17-13 | ColumnSelector + API 持久化      | Plan 05: Task 2 (column-selector + issue-view.service) | COVERED |
| D-P17-14 | Phase 17 仅保存到 API            | Plan 05: FilterSaveModal saves, Phase 19 manages UI    | COVERED |
| D-P17-15 | 筛选配置序列化 JSON              | Plan 05: FilterSaveModal serializes                    | COVERED |
| D-P17-16 | 甘特图 4 级缩放                  | Plan 03: Task 2 (day/week/month/quarter)               | COVERED |

### CONTEXT (decisions from CONTEXT.md `## Decisions`)

All D-P17-01 through D-P17-16 are covered — see RESEARCH section above.

---

## Gaps and Unplanned Items

**None** — all requirements, decisions, and goals are covered across the 6 plans.

---

## Deferred Ideas Confirmation

Items EXCLUDED per CONTEXT.md `## Deferred Ideas`:

- [x] i18n (future milestone)
- [x] 视图列表管理 UI (Phase 19)
- [x] 依赖关系创建/编辑交互 (后续迭代)
- [x] 列冻结和排序 (列实现之后)

These are intentionally NOT covered — confirmed deferred.

---

## Coverage Summary

| Source                   | Total      | Covered | Missed | Rate |
| ------------------------ | ---------- | ------- | ------ | ---- |
| GOAL items               | 5          | 5       | 0      | 100% |
| REQ (ROADMAP)            | 11         | 11      | 0      | 100% |
| RESEARCH decisions       | 16         | 16      | 0      | 100% |
| CONTEXT locked decisions | 0 deferred | -       | -      | -    |

**All items COVERED. No gaps.**
