# Phase 18: 周期 & 模块 - Context

**Gathered:** 2026-06-29
**Status:** Ready for planning
**Mode:** Smart Discuss (autonomous — batch proposals accepted)

<domain>
## Phase Boundary

实现 Cycle（迭代/冲刺）和 Module（功能分组/史诗）的完整管理功能。用户可在项目下看到 Cycle 列表（活跃/已完成/全部），创建/编辑 Cycle，查看 Cycle 详情（进度/Burndown/关联 Issues），在 Cycle 看板中管理 Issue 分配。同时，用户可看到 Module 列表，创建/编辑 Module，查看 Module 详情（进度/关联 Issues）。

**Requirements covered:** CYCLE-01, CYCLE-02, CYCLE-03, CYCLE-04, MODU-01, MODU-02, MODU-03

</domain>

<decisions>
## Implementation Decisions

### Grey Area 1: 页面架构与路由

| # | 问题 | 选项 | 推荐 |
|---|------|------|------|
| D-P18-01 | Cycle 和 Module 的路由位置 | A) `app/cycles/` + `app/modules/` 独立路由（贴近 Plane）<br>B) `app/issues/cycles/` + `app/issues/modules/` 作为 Issue 子路径<br>C) `app/projects/:id/cycles/` + `app/projects/:id/modules/` | **A** — 保持与 Phase 16-17 一致的独立路由模式：`app/routes.ts` 添加 `/workspaces/:workspaceId/projects/:projectId/cycles/` 和 `/workspaces/:workspaceId/projects/:projectId/modules/` 路由 |
| D-P18-02 | Cycle/Module 页面布局是否复用 Issue Layout HOC | A) 复用 Issue Layout HOC<br>B) 各自独立布局 | **B** — Cycle/Module 有各自的列表/详情布局结构，Issue Layout HOC 专为 5 视图切换设计。Cycle/Module 使用更简单的 `app/layouts/` 布局组件 |
| D-P18-03 | Cycle 的列表页 Tab 结构 | A) 3 个独立页面（路由参数）<br>B) 单页面 Tab 切换 | **B** — 一个 Cycle 列表页包含「活跃/已完成/全部」3 个 Tab，通过内部状态切换，避免路由层级过深 |

### Grey Area 2: 状态管理与数据获取

| # | 问题 | 选项 | 推荐 |
|---|------|------|------|
| D-P18-04 | Cycle/Module 数据存储在哪 | A) MobX Store（CycleStore + ModuleStore）<br>B) TanStack Query 仅缓存<br>C) 混合（Query 存数据，Store 存 UI 状态） | **C** — 延用 Phase 16 的 D-P16-02 模式：TanStack Query hooks（useCycles, useCycleDetail, useModules 等）管理服务端数据；MobX stores(`cycle.store.ts`, `module.store.ts`)仅管理 UI 状态 |
| D-P18-05 | Mock 数据策略 | A) 在 `mock-data.ts` 中扩展 cycles/modules<br>B) 新建 `mock-cycle-data.ts` + `mock-module-data.ts` | **A** — 延用 Phase 16 D-P16-03 模式：在现有 `mock-data.ts` 中扩展，保持一致性 |
| D-P18-06 | Cycle/Module 是否复用筛选引擎 | A) 复用 FilterBar 组件（showGroupBy=false，showSorting=true）<br>B) 不复用 — 每个视图独立筛选 | **A** — FilterBar 已设计为独立于领域的筛选引擎，Cycle 列表/详情和 Module 列表均可复用，传入对应 viewType 即可 |

### Grey Area 3: Cycle 功能范围

| # | 问题 | 选项 | 推荐 |
|---|------|------|------|
| D-P18-07 | Cycle 创建表单深度 | A) 创建弹窗 + 编辑页（轻量弹窗创建，详情页完整编辑）<br>B) 全屏创建页面<br>C) 仅弹窗 | **A** — 弹窗（Modal）创建 Cycle（名称+起止日期），详情页提供完整编辑（名称/描述/日期）。Plane 模式 |
| D-P18-08 | Burndown 图表实现 | A) 使用 Plane fork 组件（有 SVG 渲染）<br>B) 使用 recharts 库<br>C) 手写 SVG | **A** — Plane 已有 Burndown 图表实现（cycle-peek-overview + analytics-sidebar）。若 Plane 组件过于耦合 Django API 响应格式，则降级为 **C**（手写简单 SVG）。不引入额外图表库 |
| D-P18-09 | Cycle 看板 Issue 分配 | A) 复用 KanbanView + FilterBar（filter by cycle）<br>B) 独立的 CycleBoard 组件 | **A** — 延用 Phase 16 的 KanbanView 组件，通过 `cycleId` prop 过滤 Issue。用户可在 Cycle 看板中拖拽 Issue 分配/移出 Cycle |

### Grey Area 4: Module 功能范围

| # | 问题 | 选项 | 推荐 |
|---|------|------|------|
| D-P18-10 | Module 详情页布局 | A) 双栏（左侧列表 + 右侧详情/属性）<br>B) 单栏全宽列表 | **A** — 类似 Issue 详情页的双栏模式：左栏 Module Issue 列表，右栏 Module 属性/进度/描述 |
| D-P18-11 | Module Gantt 图表 | A) 复用 Phase 17 的 GanttView<br>B) Module 页独立甘特<br>C) 不进甘特 | **A** — Plane 的 Module 页面有甘特图（`modules/gantt-chart/`），复用 Phase 17 已 Fork 的 GanttView 组件，通过 `moduleId` 参数过滤 |
| D-P18-12 | Module Issue 关联 | A) Module 详情页显示关联 Issue 列表（复用 IssueListView + filterByModule）<br>B) 单独 IssueList 组件 | **A** — 复用 IssueListView/IssueRow，通过 moduleId 过滤，与 Cycle 详情页策略一致 |

### Claude's Discretion

- Cycle/Module 列表页的分页大小和排序默认值
- Module 的状态选项（待开始/进行中/已完成/取消）
- 进度条颜色方案（基于完成度百分比渐变色）
- 空状态/加载状态/错误状态的具体呈现
- Cycle Tab 切换动画
- 默认的视图布局（列表 vs 看板）

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **FilterBar 组件**: `app/components/issues/filters/filter-bar.tsx` — 领域无关的筛选引擎，可复用于 Cycle/Module 列表
- **KanbanView**: `app/components/issues/kanban-view.tsx` — 看板视图，可通过 `cycleId` prop 过滤用于 Cycle Issue 分配
- **IssueListView**: `app/components/issues/list-view.tsx` — 列表视图，可复用为 Cycle/Module 详情页的 Issue 列表
- **GanttView**: `app/components/issues/gantt-view/` — Phase 17 已 Fork，可用于 Module 甘特图
- **TanStack Query hooks**: `useIssues`, `useIssueMutations` 等 hooks 模式 — 可参照创建 `useCycles`, `useCycleDetail`, `useModules` 等 hooks
- **MobX Store 模式**: 参照 `issue.store.ts` 创建 `cycle.store.ts` + `module.store.ts`
- **Mock 数据**: `src/lib/mock-data.ts` 中扩展 cycle/module 数据
- **FlowApiService**: `src/lib/services/flow-api.service.ts` — API 基类
- **Store 上下文**: `app/lib/store-context.tsx` — 已注册 IssueStore/ProjectStore/WorkspaceStore

### Plane 参考组件 (`apps/web/core/components/`)

**Cycle (周期):**
- `cycles/cycles-view.tsx` — 主视图容器（Tab: 活跃/已完成/全部）
- `cycles/cycles-view-header.tsx` — 页头（Cycle 创建按钮 + 视图切换）
- `cycles/list/` — Cycle 列表项（group-header, list-item, list-map, root）
- `cycles/active-cycle/` — 活跃 Cycle 概览（progress, stats, productivity）
- `cycles/cycle-peek-overview.tsx` — Cycle 详情弹窗
- `cycles/analytics-sidebar/` — Burndown/分析侧栏
- `cycles/form.tsx` — Cycle 创建/编辑表单
- `cycles/modal.tsx` — Cycle 弹窗
- `cycles/dropdowns/` — Cycle 选择下拉
- `cycles/delete-modal.tsx` — 删除确认弹窗
- `cycles/transfer-issues-modal.tsx` — Issue 转移弹窗

**Module (模块):**
- `modules/modules-list-view.tsx` — Module 列表页
- `modules/module-card-item.tsx` — Module 卡片
- `modules/module-list-item.tsx` — Module 列表项
- `modules/module-peek-overview.tsx` — Module 详情弹窗
- `modules/module-view-header.tsx` — 页头
- `modules/form.tsx` — 创建/编辑表单
- `modules/modal.tsx` — 弹窗
- `modules/gantt-chart/` — Module 甘特图
- `modules/select/` — Module 选择器
- `modules/links/` — Module 链接管理
- `modules/sidebar-select/` — 侧栏选择器
- `modules/dropdowns/` — 下拉选择
- `modules/analytics-sidebar/` — 分析侧栏

### Established Patterns

- **TanStack Query hooks**: `useIssues.ts` 模式 — queryFn returns mock data, queryKey 含 projectId
- **MobX Store**: UI 状态管理（选中、展开页、activeTab）
- **Mock 数据**: `mock-data.ts` 集中管理
- **Fork Plane 组件**: 直接复制 Plane 组件，替换 import 路径，移除 plane-web 依赖
- **路由注册**: `app/routes.ts` 中注册路由，页面组件在 `app/` 目录下

### Integration Points

- `app/routes.ts` — 添加 `/workspaces/:workspaceId/projects/:projectId/cycles/` 和 `.../modules/` 路由
- `app/store/` — 新增 `cycle.store.ts` + `module.store.ts`
- `app/lib/store-context.tsx` — 注册新 store
- `src/lib/mock-data.ts` — 扩展 mock cycles + modules
- `src/lib/hooks/` — 新增 useCycles, useCycleDetail, useModules hooks
- `app/components/cycles/` — 新建 Cycle 组件目录
- `app/components/modules/` — 新建 Module 组件目录

</code_context>

<specifics>
## Specific Ideas

- Cycle 列表页顶栏使用 FilterBar 支持周期筛选（按状态 Tab + 日期范围），但不复用 GroupBy（无需分组）
- Cycle 详情页左侧 Issue 列表复用 IssueListView，通过 cycleId 过滤
- Module 详情页左侧 Issue 列表同样复用 IssueListView，通过 moduleId 过滤
- Module 详情页右侧属性面板展示进度、描述、链接、成员
- Cycle 的统计分析（进度、完成率、Burndown）放在详情页右栏
- Cycle 创建时自动计算起止日期（默认：开始日期=今天，结束日期=30天后）

</specifics>

<deferred>
## Deferred Ideas

- Cycle/Module 的筛选条件保存为自定义视图（Phase 19 VIEW 要求）
- Module 的甘特图依赖关系连线交互（Phase 17 同样递延）
- Cycle 自动完成/归档（CYCLE 完成日期过后自动标记为已完成）
- Module 的权限管理（谁可以创建/编辑 Module）
- i18n 国际化和响应式适配（Phase 20）

</deferred>
