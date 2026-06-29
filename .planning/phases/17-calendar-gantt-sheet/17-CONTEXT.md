# Phase 17: 日历/甘特/电子表格 & 筛选引擎 - Context

**Gathered:** 2026-06-29
**Status:** Ready for planning
**Mode:** Smart Discuss (autonomous — batch proposals accepted)

<domain>
## Phase Boundary

实现多种 Issue 视图（日历/甘特/电子表格）和可复用的筛选/排序/自定义列引擎。用户可在 Issue 列表中切换日历、甘特、电子表格视图，按多种条件筛选和排序 Issues，自定义显示的列，以及将筛选配置保存为自定义视图。看板子分组 (Sub-Group / Swimlane) 的通用 hook 也在本阶段提供。

**Requirements covered:** CALN-01, CALN-02, GANT-01, GANT-02, SHEE-01, SHEE-02, FILT-01, FILT-02, FILT-03, FILT-04, KANB-04

</domain>

<decisions>
## Implementation Decisions

### Grey Area 1: 包与依赖策略

- **D-P17-01:** 日历视图 — Fork Plane 现有 `apps/web/core/components/issues/issue-layouts/calendar/`，直接复制到本地 `src/lib/views/calendar/`，保持 Plane UI 一致性
- **D-P17-02:** 甘特图 — Fork Plane 现有实现 `apps/web/core/components/gantt-chart/`，直接复制到 `src/lib/views/gantt/`
- **D-P17-03:** 电子表格 — Fork Plane 现有 `apps/web/core/components/issues/issue-layouts/spreadsheet/`，直接复制到 `src/lib/views/spreadsheet/`
- **D-P17-04:** 不引入额外的第三方视图库 — 全部使用 Fork Plane 组件，确保 UI 一致性，降低依赖复杂度

### Grey Area 2: 视图架构与状态管理

- **D-P17-05:** 延用 Phase 16 的 Issue Layout HOC 模式 — 5 个视图（列表/看板/日历/甘特/电子表格）共享同一个布局 HOC 管理当前激活视图切换
- **D-P17-06:** 日历视图按当前可见月份范围懒加载 Issue — 使用 date-fns 计算起止日期，仅请求该时间范围内的 Issues
- **D-P17-07:** 甘特图显示 Issue 依赖关系连线（基于 Plane 现有 gantt-chart 实现）
- **D-P17-08:** 电子表格采用单点击内联编辑模式 — 点击单元格直接切换为编辑控件

### Grey Area 3: 筛选引擎架构

- **D-P17-09:** 构建可复用筛选引擎 — 供给列表/看板/日历/甘特/电子表格 5 个视图共用，包括 `useFilters` hook + `FilterBar` 组件
- **D-P17-10:** 筛选条件 UI 模式 — 快速筛选按钮（状态/优先级/负责人）+ 展开筛选面板（更多条件），FilterBar 组件化
- **D-P17-11:** 筛选状态通过 TanStack Query 管理并同步到 URL query params — 筛选结果可分享
- **D-P17-12:** 在筛选引擎中提供通用 `useGroupBy` + `useSubGroupBy` hooks，各视图按需使用

### Grey Area 4: 自定义列 & 视图保存

- **D-P17-13:** 构建 `ColumnSelector` 组件，列配置通过 TanStack Query 持久化到 API
- **D-P17-14:** Phase 17 实现筛选/排序/列配置的保存（FILT-04 保存到 API），Phase 19 实现视图管理和应用 UI
- **D-P17-15:** 筛选器配置序列化为 JSON 存储到后端 `IssueView` 实体
- **D-P17-16:** 甘特图支持 4 级缩放（天/周/月/季度）

### Claude's Discretion

- 日历视图的配色方案和状态颜色映射
- 电子表格自动保存 debounce 时间
- 筛选引擎的加载/空/错误状态具体呈现
- 拖拽调整日历/甘特日期时的乐观更新策略
- 电子表格虚拟滚动的具体实现（如果 Issues 数量大）

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **Issue 类型**: `src/lib/types/issues/` — TIssue 包含 `start_date`、`target_date`、`assignees`、`priority` 等所有字段
- **State 类型**: `src/lib/types/state/` — TStateGroups（backlog/unstarted/started/completed/cancelled）用于状态颜色映射
- **Issue 常量**: `src/lib/constants/issue/common.ts` — 布局选项、Issue 属性 key
- **UI 组件库**: `src/lib/ui/` — Avatar, Badge, Button, Dropdown, Modal, Tooltip, Loader, DragHandle
- **@hello-pangea/dnd**: 已安装，可用于拖拽交互
- **date-fns**: 已安装，适合日历日期计算
- **TanStack Query**: 已配置，用于筛选/列配置的状态管理
- **MobX 模式**: `app/store/` — IssueStore 已有 UI 状态管理模式参考
- **FlowApiService**: `src/lib/services/` — API 基类，JWT Bearer + snake_case 转换

### Plane 参考 (`apps/web/`)

- `apps/web/core/components/issues/issue-layouts/calendar/` — 日历视图完整实现
- `apps/web/core/components/gantt-chart/` — 甘特图完整实现（blocks, chart, sidebar, views, contexts, data, helpers）
- `apps/web/core/components/issues/issue-layouts/spreadsheet/` — 电子表格视图完整实现
- `apps/web/core/components/core/filters/` — 筛选过滤器组件
- `apps/web/core/components/common/filters/` — 通用筛选组件
- `apps/web/core/components/issues/issue-layouts/filters/` — Issue 布局筛选条
- `apps/web/core/components/views/` — 视图管理相关（将在 Phase 19 深入，本阶段仅保存接口）
- `apps/web/core/components/views/form.tsx` — 视图表单（保存筛选配置）
- `apps/web/core/components/views/helper.tsx` — 视图辅助函数

### Established Patterns

- **Issue Layout HOC**: Phase 16 已建立的 `issue-layout-HOC.tsx` 模式—列表/看板切换方式
- **TanStack Query hooks**: `useIssues.ts`, `useIssue.ts` 等 hooks 模式
- **MobX Store**: 仅有 UI 状态管理（选中、展开等）
- **Mock 数据**: `src/lib/mock-data.ts` 扩展中
- **Fork 组件**: 4 个 Plane 包 Fork 到 `src/lib/` 的流程

### Integration Points

- `app/components/issues/issue-layouts/` — 新增日历/甘特/电子表格视图目录
- `app/components/issues/issue-layouts/filters/` — 新增筛选引擎组件
- `app/store/` — 可能需要新增 filter/column store（纯 UI 状态）
- `src/lib/hooks/` — 新增 useFilters, useGroupBy, useSubGroupBy hooks
- `src/lib/services/` — 新增 issue-view.service.ts（保存视图配置）

</code_context>

<specifics>
## Specific Ideas

- 筛选引擎应设计为独立于具体领域（不绑定 Issue），以便未来复用于 Cycle/Module/Page 等筛选
- 甘特图默认显示完成后 7 天到开始前 7 天，确保上下文可见
- 甘特图中 Issue 进度条基于已关闭的子 Issue 比例计算（无子 Issue 则 0/100）
- 电子表格支持多选单元格批量编辑

</specifics>

<deferred>
## Deferred Ideas

- 国际化 (i18n) 推迟到未来里程碑（一期仅中文）
- 视图列表管理 UI（增删改/排序/分享）在 Phase 19 实现
- 甘特图中的依赖关系连线交互（创建/编辑依赖关系）在后续迭代优化
- 电子表格的列冻结和排序在当前列实现之后考虑

</deferred>
