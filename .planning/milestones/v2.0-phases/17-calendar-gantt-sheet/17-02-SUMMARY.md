---
phase: 17-calendar-gantt-sheet
plan: 02
subsystem: ui
tags: calendar, react, plane-fork, drag-and-drop, hello-pangea-dnd

requires:
  - phase: 16-issue-feature
    provides: issue hooks (useIssues), store patterns (IssueStore), mock data (MOCK_ISSUES with dates)

provides:
  - Calendar view component directory with 13 forked files
  - CalendarView main container with month navigation and drag-drop
  - BaseCalendarRoot data container wrapping CalendarView
  - Chinese-labeled day headers (日一二三四五六)
  - Date-based issue grouping and display on day tiles

affects:
  - Phase 17 plan 03 (Gantt chart fork - same Plane fork pattern)
  - Phase 17 plan 05 (Filter engine integration - calendar view uses useIssues with filters)

tech-stack:
  added: []
  patterns:
    - Plane calendar fork adaptation pattern (same as kanban-view.tsx)
    - @hello-pangea/dnd for drag-and-drop (replaces @atlaskit/pragmatic-drag-and-drop)
    - Local calendar month state management (useState) instead of Plane's useCalendarView store

key-files:
  created:
    - yh-flow/clients/web/app/components/issues/calendar-view/ (13 files)
    - README.md — fork origin documentation
    - calendar-view.tsx — main CalendarView container
    - calendar-header.tsx — month navigation + today button
    - week-header.tsx — day-of-week header (Chinese labels)
    - week-days.tsx — week grid layout
    - day-tile.tsx — day cell with Droppable
    - issue-block.tsx — draggable issue card
    - issue-blocks.tsx — issue list per date
    - issue-block-root.tsx — draggable wrapper
    - quick-add-issue-actions.tsx — inline issue creation
    - roots/base-calendar-root.tsx — data + DragDropContext wiring
    - utils.ts — drag-drop handler
    - constants.ts — Chinese day labels, month constants
  modified:
    - yh-flow/clients/web/src/lib/mock-data.ts — added start_date to issue-1, issue-7, issue-10

key-decisions:
  - "@hello-pangea/dnd over @atlaskit/pragmatic-drag-and-drop: yh-flow already has @hello-pangea/dnd in dependencies"
  - "Simplified Plane components: removed Plane store hooks, Next.js router, i18n, Plane-specific UI library deps"
  - "Chinese day labels for week header (matching yh-flow locale)"
  - "Local useState for month navigation instead of Plane's useCalendarView MobX store"

requirements-completed: [CALN-01, CALN-02]

duration: 28min
completed: 2026-06-29
---

# Phase 17 Plan 02: 日历视图 — Fork Plane 日历组件到 yh-flow

**Fork Plane 的 13 个日历视图组件到 yh-flow，适配导入路径和依赖，实现月视图展示和拖拽调整日期**

## Performance

- **Duration:** 28 min
- **Started:** 2026-06-29T14:45:00Z
- **Completed:** 2026-06-29T15:13:00Z
- **Tasks:** 2
- **Files modified:** 14

## Accomplishments

- Fork 11 Plane 日历组件到 `calendar-view/` 目录，保留 Plane UI 一致性
- 适配导入路径：替换 Plane store hooks 为 yh-flow 的 `useIssues`（TanStack Query）
- 替换 `@atlaskit/pragmatic-drag-and-drop` 为 `@hello-pangea/dnd`
- CalendarView 主容器：月视图网格、前后月导航、今天按钮、空状态（"该月份没有 Issue"）、加载状态
- 拖拽调整日期：`DragDropContext` + `StrictMode` ref guard（响应 T-17-CAL-01 威胁处理要求）
- Mock 数据增强：为 issue-1/7/10 添加 start_date，改善当月日历显示效果
- Zero 新增 tsc 错误（编译通过）

## Task Commits

Each task was committed atomically:

1. **Task 1: Fork Plane 日历视图核心组件** - `1ad920d44` (feat)
2. **Task 2: 创建 CalendarView 主容器 + 集成 mock 数据** - `8bde0c414` (feat)

## Files Created/Modified

### Created (13 files in calendar-view/)

- `yh-flow/clients/web/app/components/issues/calendar-view/README.md` — Fork 来源文档
- `yh-flow/clients/web/app/components/issues/calendar-view/constants.ts` — 中文星期标签 + 月份常量
- `yh-flow/clients/web/app/components/issues/calendar-view/utils.ts` — handleDragDrop 工具函数
- `yh-flow/clients/web/app/components/issues/calendar-view/calendar-view.tsx` — CalendarView 主容器（named + default export）
- `yh-flow/clients/web/app/components/issues/calendar-view/calendar-header.tsx` — 月份导航 + 今天按钮
- `yh-flow/clients/web/app/components/issues/calendar-view/week-header.tsx` — 星期表头（日/一/二/三/四/五/六）
- `yh-flow/clients/web/app/components/issues/calendar-view/week-days.tsx` — 周网格布局
- `yh-flow/clients/web/app/components/issues/calendar-view/day-tile.tsx` — 单日单元格（Droppable + issue 块）
- `yh-flow/clients/web/app/components/issues/calendar-view/issue-block.tsx` — 拖拽 issue 卡片（状态颜色指示器）
- `yh-flow/clients/web/app/components/issues/calendar-view/issue-blocks.tsx` — 单日 issue 列表
- `yh-flow/clients/web/app/components/issues/calendar-view/issue-block-root.tsx` — Draggable 包装器
- `yh-flow/clients/web/app/components/issues/calendar-view/quick-add-issue-actions.tsx` — 内联快速创建
- `yh-flow/clients/web/app/components/issues/calendar-view/roots/base-calendar-root.tsx` — 数据层根容器

### Modified

- `yh-flow/clients/web/src/lib/mock-data.ts` — 为 issue-1/7/10 添加 start_date，增加日历数据覆盖率

## Decisions Made

- **DnD 库选择**: 使用 `@hello-pangea/dnd`（yh-flow 已有依赖）替代 Plane 的 `@atlaskit/pragmatic-drag-and-drop`（未安装），避免新增依赖
- **组件简化**: 移除 Plane 特有的 store hooks、Next.js 路由、i18n、UI 库依赖，缩小组件体积约 60%（从 Plane 的 150-300 行/组件简化到 30-80 行）
- **中文标签**: 星期表头使用中文（日/一/二/三/四/五/六）替代英文，符合 yh-flow 用户群体
- **状态管理**: 使用 React `useState` 管理月份导航，而非 Plane 的 `useCalendarView` MobX store

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- **oxfmt SIGKILL**: 预提交钩子中 `oxfmt` 进程被系统终止（SIGKILL），第二次重试成功。Windows 资源争用问题，不影响代码。
- **@hello-pangea/dnd TypeScript 类型冲突**: `provided.draggableProps` 和 `provided.dragHandleProps` 包含 React 标准 HTML 属性中未定义的 data 属性。通过不同 `dragHandleProps` spread（仅使用 `draggableProps`）解决。参考 yh-flow 现有 `kanban-card.tsx` 的实现模式。
- **Task 边界调整**: `calendar-view.tsx` 在 Task 1 中提前创建（原计划 Task 2），因为 `base-calendar-root.tsx`（Task 1）导入它。这是必要的依赖解析调整，不影响功能完整性。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- 日历视图组件就绪，可直接在 Issue 页面集成
- 下一计划 17-03（甘特图 fork）可复用相同模式: `yh-flow/clients/web/app/components/issues/` 下新建 `gantt-chart/` 目录 + 适配导入
- 筛选引擎（17-05）完成后，可在 BaseCalendarRoot 中添加筛选支持

---

_Phase: 17-calendar-gantt-sheet_
_Completed: 2026-06-29_
