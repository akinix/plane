---
phase: 16-issue
plan: 04
type: execute
status: complete
---

# Plan 16-04 — Kanban Board View

**Objective:** 实现 Issue 看板视图（Kanban），含拖拽和分组功能

**Duration:** ~30 min (agent) + ~5 min (orchestrator completing Task 2)

## Tasks

| # | Status | Commits |
|---|--------|---------|
| 1 | ✅ | `b964c17df` — Create Kanban core components (KanbanView, KanbanColumn, KanbanCard) |
| 2 | ✅ | (orchestrator) — Update issues/page.tsx to integrate Kanban view with lazy loading |

## Files Created

- `yh-flow/clients/web/app/components/issues/kanban-view.tsx` — 看板视图容器 (DragDropContext + 列布局 + GroupBySelector)
- `yh-flow/clients/web/app/components/issues/kanban-column.tsx` — 单列 (Droppable + 标题 + 卡片列表)
- `yh-flow/clients/web/app/components/issues/kanban-card.tsx` — 单卡片 (Draggable + Issue 信息)

## Files Modified

- `yh-flow/clients/web/app/issues/page.tsx` — 集成看板视图切换 (React.lazy + Suspense)

## Key Features

- 三种分组方式：按状态(默认)/按优先级/按负责人
- 分组切换不影响筛选（与列表视图共享筛选状态）
- 拖拽变更 Issue 状态（乐观更新 + error 回滚）
- 列宽度固定 280px，水平滚动
- 空状态："拖动 Issue 到此列"
- StrictMode 防抖 guard
- 看板按钮在已实现的 plan 16-02 页面中激活

## Verification

- `npx tsc --noEmit` — 通过
- Issue 页面视图切换按钮正常工作并切换 List/Kanban 视图
- GroupBySelector 切换分组方式
- 与列表视图共享筛选状态
