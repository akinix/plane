---
phase: 17-calendar-gantt-sheet
plan: 06
type: execute
wave: 4
subsystem: "yh-flow/web/issues"
tags: ["view-switcher", "filter-bar", "kanban-subgroup", "integration"]
requires:
  - 17-02 (Calendar view)
  - 17-03 (Gantt view)
  - 17-04 (Spreadsheet view)
  - 17-05 (Filter engine)
provides:
  - "5-view toggle on Issues page"
  - "FilterBar integration into list and kanban views"
  - "Kanban subgroup/swimlane (KANB-04)"
affects:
  - page.tsx (5-view toggle)
  - list-view.tsx (FilterBar replacement)
  - kanban-view.tsx (FilterBar + subgroup)
  - use-view-switcher.ts (new hook)
metrics:
  duration: "2026-06-29T08:06:54Z - ~15 min"
  files_created: 1
  files_modified: 3
  commits: 2
completed_date: 2026-06-29
---

# Phase 17 Plan 06: 多视图集成与筛选引擎接入

## 概要

将所有 5 种视图（列表/看板/日历/甘特/电子表格）集成到 Issue 页面视图切换系统中，为列表视图接入 FilterBar 替换原有 QuickFilterBar，为看板视图添加子分组（Swimlane）支持，完成 Phase 17 的整体集成。

## 任务执行

### Task 1: 5 种视图切换

**文件修改:**

- `yh-flow/clients/web/app/issues/page.tsx` — 视图切换从 2 按钮扩展至 5 按钮，添加 CalendarDays/GitBranch/Table 图标，添加 4 个 React.lazy 懒加载视图（kanban/calendar/gantt/spreadsheet）
- `yh-flow/clients/web/app/hooks/use-view-switcher.ts` — 新建 hook，封装视图选项和切换逻辑

**关键变更:**

- 所有非默认视图使用 `React.lazy` + `Suspense` 懒加载，fallback 统一为 "加载中..."
- CalendarView 使用 `CalendarViewWrapper` 桥接其不同 props 接口（需要 `issues`/`isLoading`/`handleDragAndDrop`）
- 视图切换通过 `store.issue.setActiveView()` MobX action 管理，确保状态一致性
- 保持现有列表/看板视图功能和布局不变

### Task 2: FilterBar 接入 + 看板子分组

**文件修改:**

- `yh-flow/clients/web/app/components/issues/list-view.tsx` — 替换 QuickFilterBar 为 FilterBar（showSorting=true），添加"自定义列"(ColumnSelector)和"保存视图"(FilterSaveModal)按钮，移除独立排序按钮组
- `yh-flow/clients/web/app/components/issues/kanban-view.tsx` — 添加 FilterBar（showGroupBy=true），添加子分组 dropdown 选择器（None/State/Priority），实现 swimlane 渲染

**关键变更:**

- list-view 使用 `FilterBar` + `useFilters`/`useSorting` 替换原有 `QuickFilterBar`
- kanban-view 使用 `useSubGroupBy` hook 管理子分组状态，选择子分组后每个列内按 swimlane 分区显示
- 看板子分组使用独立 Droppable 容器保持拖拽兼容性
- 子分组激活时使用自定义列渲染（含 swimlane 标签行），关闭时使用标准 KanbanColumn

## 验证

| 检查项                          | 结果                |
| ------------------------------- | ------------------- |
| 5 种视图在 page.tsx 中存在      | PASS (19 matches)   |
| React.lazy 导入                 | PASS (8 matches)    |
| FilterBar 在 list-view 中       | PASS (4 matches)    |
| FilterSaveModal 在 list-view 中 | PASS (2 matches)    |
| ColumnSelector 在 list-view 中  | PASS (5 matches)    |
| subGroup 在 kanban-view 中      | PASS (6 matches)    |
| `npx tsc --noEmit` 新增错误     | PASS (0 new errors) |

## 提交记录

| 哈希        | 消息                                                                                   |
| ----------- | -------------------------------------------------------------------------------------- |
| `bde0aaa92` | feat(17-06): 5-view toggle with lazy loading for calendar/gantt/spreadsheet            |
| `495af0b9b` | feat(17-06): integrate FilterBar into list-view, add subgroup (KANB-04) to kanban-view |

## 关键决策

1. **CalendarView 包装器**: 由于 CalendarView 接受 `issues`/`isLoading`/`handleDragAndDrop` props 而非标准 `workspaceId`/`projectId` 模式，创建了内联 `CalendarViewWrapper` 组件使用 useStore + useIssues 提供数据
2. **Kanban 子分组实现**: 选择不使用 KanbanColumn 子分组 props 扩展（避免修改 17-02 的组件），而是在 kanban-view.tsx 中实现自定义列渲染，保持 DnD 兼容性
3. **列表视图排序**: 原有独立排序按钮组被移除，由 FilterBar 的 `showSorting` prop 统一处理，减少 UI 重复

## 已知 Stub

- **CalendarViewWrapper**: 拖拽回调（handleDragAndDrop）为 no-op 实现，因为 Phase 17 日历视图仅为只读显示，拖拽编辑待后续版本实现
- **visibleColumnIds**: IssueStore 中初始化为空数组 `[]`，用户需通过 ColumnSelector 首次选择显示列

## Self-Check: PASSED

所有文件和提交已验证通过。
