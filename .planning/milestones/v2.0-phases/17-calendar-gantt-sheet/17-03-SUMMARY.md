---
phase: 17-calendar-gantt-sheet
plan: 03
type: execute
subsystem: gantt-chart
tags: [gantt, fork, issue-view, zoom]
requires: [17-01 (types)]
provides: [gantt-chart module, GanttView container]
affects: [mock-data (issue_relation), gantt types]
tech-stack:
  added:
    - "@plane/types (extension): TGanttViews now includes 'day'"
    - "lucide-react: GitBranch icon for empty state"
  patterns:
    - "Forked Plane gantt-chart with MobX→React context migration"
    - "Zoom level management via VIEWS_LIST + CHART_VIEW_COMPONENTS"
key-files:
  created:
    - yh-flow/clients/web/app/components/issues/gantt-view/ (39 files — full Plane fork)
    - yh-flow/clients/web/app/components/issues/gantt-view/gantt-view.tsx
    - yh-flow/clients/web/app/components/issues/gantt-view/chart/views/day.tsx
    - yh-flow/clients/web/app/hooks/use-multiple-select.ts (type stub)
  modified:
    - yh-flow/clients/web/src/lib/types/layout/gantt.ts (add 'day' to TGanttViews)
    - yh-flow/clients/web/app/components/issues/gantt-view/data/index.ts (add day to VIEWS_LIST)
    - yh-flow/clients/web/app/components/issues/gantt-view/chart/main-content.tsx (add DayChartView)
    - yh-flow/clients/web/app/components/issues/gantt-view/chart/root.tsx (add day view helper)
    - yh-flow/clients/web/app/components/issues/gantt-view/chart/header.tsx (add "天" label)
    - yh-flow/clients/web/src/lib/mock-data.ts (add dates, issue_relation)
decisions:
  - "Day view: Uses weekView helper + DayChartView wrapper; dayWidth=120 with 14-day approxFilterRange"
  - "Progress bar: Based on sub-issue completion ratio; falls back to completed_at 0/100"
  - "Dependency lines: Phase 17 read-only — issue_relation data added to mock-data but GANT-02 SVG wiring deferred"
  - "use-multiple-select: Type stub only — no implementation since Phase 17 has no multi-select interaction"
metrics:
  duration: "~45 minutes"
  completed_date: "2026-06-29"
---

# Phase 17 Plan 03: Fork Plane 甘特图组件到 yh-flow

## 目标

Fork Plane gantt-chart 组件目录到 yh-flow `app/components/issues/gantt-view/`，适配导入路径。创建 GanttView 主容器，支持 4 级缩放（天/周/月/季度 per D-P17-16）。

## 任务完成情况

| Task | 名称                                               | Commit    | 文件                                                                                          |
| ---- | -------------------------------------------------- | --------- | --------------------------------------------------------------------------------------------- |
| 1    | Fork Plane 甘特图组件（39 个文件全量复刻）         | 2546c7534 | gantt-view/ 目录：blocks, chart, sidebar, views, contexts, data, helpers, hooks, constants 等 |
| 2    | 创建 GanttView 主容器 + zoom 控制 + mock-data 更新 | 2206b1c86 | gantt-view.tsx, day.tsx, mock-data.ts, gantt.ts, main-content.tsx, root.tsx, header.tsx       |

## 关键实现细节

### Task 1: Fork Plane 甘特图组件

- 完全复刻 Plane `apps/web/core/components/gantt-chart/` 目录结构到 `app/components/issues/gantt-view/`
- 所有导入路径适配到 yh-flow 项目结构
- `useTimeLineChartStore` 替换为简化版 React context hook（非 MobX 版本），包含 TimeLineChartProvider
- 移除 Plane web-only 组件引用（依赖路径 GanttAdditionalLayers、批量操作 IssueBulkOperationsRoot 等）
- 移除 `@plane/i18n` 国际化，使用硬编码中文文本
- 移除 `@atlaskit/pragmatic-drag-and-drop` 依赖（自动滚动、重排序）
- 创建 `@/hooks/use-multiple-select` 类型存根（Phase 17 只读，无多选交互）
- 所有修改处使用 `// FLOW:` 标记

### Task 2: GanttView 主容器

- **组件架构**: `GanttView` (observer) → `TimeLineChartProvider` → `GanttChartRoot` → `ChartViewRoot` + `GanttChartMainContent`
- **数据获取**: 使用 `useIssues(projectId, filters)` hook，筛选有 start_date/target_date 的 issue
- **4 级缩放**: 天(dayWidth=120)/周(60)/月(20)/季度(5)，默认"月"
- **进度条**: 有子 issue 则按子 issue 关闭比例计算；无子 issue 则 0%（未完成）/ 100%（已完成）
- **状态处理**: Loading（skeleton 骨架屏）、空状态（"暂无 Issue"）、无日期状态

### 依赖关系 (GANT-02)

`issue_relation` 已添加到 mock data：

- issue-1 (登录UI) blocked_by issue-5 (后端认证API)
- issue-4 (首页优化) blocked_by issue-5 (后端认证API)
- issue-9 (CRUD API) blocked_by issue-8 (数据模型) + issue-10 (API版本控制)
- issue-14 (单元测试) blocked_by issue-12 (缓存层)

GANT-02 SVG 连线逻辑保留在 Plane 原始实现中，Phase 17 为只读显示。

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 — Bug] .ts 文件包含 JSX 导致编译错误**

- **发现于**: Task 1
- **问题**: `hooks/use-timeline-chart.ts` 包含 JSX（`<TimeLineChartContext.Provider>`），但文件扩展名为 `.ts`
- **修复**: 重命名为 `.tsx`
- **文件**: `yh-flow/clients/web/app/components/issues/gantt-view/hooks/use-timeline-chart.tsx`

**2. [Rule 2 — Missing] chart/main-content.tsx 缺少 enableSelection 解构**

- **发现于**: Task 1
- **问题**: `enableSelection` 在 props 中被使用（传递给 GanttChartSidebar）但未从 props 解构
- **修复**: 在解构列表中添加 `enableSelection`
- **文件**: `yh-flow/clients/web/app/components/issues/gantt-view/chart/main-content.tsx`

**3. [Rule 2 — Missing] 缺少 `@/hooks/use-multiple-select` 模块**

- **发现于**: Task 1
- **问题**: gantt-view 的 sidebar 组件引用了 `@/hooks/use-multiple-select` 的 `TSelectionHelper` 类型，但该模块不存在于 yh-flow
- **修复**: 创建类型存根文件，导出 `TSelectionHelper` 接口定义
- **文件**: `yh-flow/clients/web/app/hooks/use-multiple-select.ts`

### 其他偏差

**Task 1 commit 包含所有 yh-flow 文件**

由于 `git checkout preview -- yh-flow/` 命令会将文件自动暂存到索引中，Task 1 的 commit (2546c7534) 包含了从 preview 分支恢复的所有 yh-flow 文件（共 3184 个文件，239337 行新增），而不仅仅是 gantt-view 的 39 个文件。这些文件是 yh-flow 的正确源文件，但 commit 边界过大。后续 commit 仅包含实际有变更的文件。

## 已知问题 (Deferred to Future Plans)

- **StrictMode ref guard (T-17-GANT-02)**: `use-gantt-resizable.ts` 中缺少 StrictMode 防重复触发保护，标记为 `// FLOW: ... omitted per T-17-GANT-02`
- **Day view 实现**: DayChartView 使用 WeekChartView 的数据生成器（weekView helper），日列渲染基于 IWeekBlock 结构。真正的"天"视图（每小时/半天粒度）超出 Phase 17 范围。
- **依赖关系连线交互**: GANT-02 的 SVG 箭头连线逻辑保留在 Plane 原始实现中，但未激活。需要后续 phase 激活。
- **拖拽编辑**: enableBlockMove/LeftResize/RightResize 设置为 false，Phase 17 为只读。

## 威胁扫描

| Flag                   | File         | Description                                                         |
| ---------------------- | ------------ | ------------------------------------------------------------------- |
| threat_flag: new_type  | gantt.ts     | TGanttViews 扩展添加 'day' — 新枚举值影响所有 switch/match 穷举检查 |
| threat_flag: mock_data | mock-data.ts | issue_relation 字段添加 mock 数据 — 仅用于测试，不影响生产          |

## Self-Check

- [x] gantt-view/ 目录存在，包含 39 个文件
- [x] 4 种 zoom level view 文件存在：day.tsx, week.tsx, month.tsx, quarter.tsx
- [x] GanttView named export: `grep -q "export { GanttView }" gantt-view.tsx`
- [x] gantt-view.tsx 包含 `TimeLineChartProvider` 包装
- [x] mock-data.ts 添加了 date 和 issue_relation 数据
- [x] 所有 mock-data 变更无新增 tsc error
- [x] gantt.ts 类型扩展正确（添加 'day' 到 TGanttViews）
- [x] Task 1 commit: `2546c7534`
- [x] Task 2 commit: `2206b1c86`

## Self-Check: PASSED
