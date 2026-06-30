# Phase 12: Analytics — 分析 - Context

**Gathered:** 2026-06-26
**Status:** Ready for planning

<domain>
## Phase Boundary

实现工作区/项目分析数据功能，提供 Issue 统计、Cycle 进度、成员工作量等分析 API。Phase 12 不创建独立分析数据库，所有统计基于对 WorkItemsDbContext (Issues/Cycle/Module) 的实时聚合查询。

**范围内（REQ-12.1 ~ REQ-12.2）：**

- Issue 统计（按状态、优先级、负责人）
- 完成率趋势（月度图表数据）
- Cycle 进度概览
- 成员工作量分布
- 分析数据导出（CSV）
- 图表数据 API

**范围外（推迟）：**

- 自定义图表引擎（x_axis/segment/group_by 复杂查询）→ 后续版本
- Dashboard Widget CRUD → 后续版本
- AnalyticView 保存查询条件 → 后续版本
- 预计算/缓存聚合 → v1.0 不采用
- 前端 UI → Phase 13 Flow Web

</domain>

<decisions>
## Implementation Decisions

### 模块结构

- 创建独立 `Modules.Analytics` + `Modules.Analytics.Contracts` 项目（与所有后端模块一致）
- **不创建**独立 AnalyticsDbContext — Analytics 是纯查询模块，直接跨 WorkItemsDbContext 做读取查询
- **不建独立表** — 所有统计/图表数据直接从 Issue/Cycle/Module 表实时聚合
- AnalyticView 实体推迟（Phase 12 不实现）

### API 端点设计

- 路由层级：`/api/v1/workspaces/{slug}/analytics/` + `/api/v1/workspaces/{slug}/projects/{projectId}/analytics/`
- 4 个端点组：**Overview**（工作区概览）/ **Stats**（项目维度统计）/ **Chart**（图表数据）/ **Export**（导出）
- 筛选方式：Query 参数（?date_filter=this_month&project_ids=xxx）
- 响应格式：Plane 兼容，分组计数 + 按状态/优先级/负责人维度

### 数据聚合策略

- **实时查询** — GET 请求时动态 COUNT/GROUP BY Issue 表（与 Cycle Burndown D-02 一致）
- 为分析查询添加覆盖索引 — (TenantId, ProjectId, StateGroup, CreatedAt) 复合索引
- 日期范围支持预定义 + 自定义
- v1.0 不做缓存

### 范围与优先级

- 优先级排序：① Issue 状态分布统计 → ② Cycle 进度 → ③ 成员工作量 → ④ 自定义图表
- 导出格式：仅 CSV（Hangfire 后台异步导出）
- 自定义图表引擎推迟
- Dashboard Widget 推迟

### Claude's Discretion

- 具体端点路径命名（/analytics/overview, /analytics/stats, /analytics/charts, /analytics/export）
- Plane 兼容的响应 DTO 字段名映射
- EF Core 聚合查询优化策略
- 测试策略和覆盖范围

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **Cycle Burndown 模式**：Phase 5 已实现实时聚合计算（D-02），可直接复用模式
- **Module progress 模式**：Phase 6 实时聚合进度（GetModuleProgress）
- **Hangfire 后台任务**：Phase 10 Webhook 已建立 Hangfire 基础设施，可复用导出任务
- **WorkItemsDbContext**：包含 Issue/Cycle/Module 实体，Analytics 直接依赖读取
- **Route group 模式**：`/workspaces/{slug}/projects/{projectId}/` 嵌套路由在 Phase 3-8 中已建立
- **平面 API 兼容模式**：所有端点复用 Plane 的 JSON 响应格式惯例

### Established Patterns

- 模块化单体结构（Module + Contracts 分离）
- 垂直切片（Feature 目录下 Command/Query/Handler）
- FastEndpoints + Marten-less EF Core 实时查询
- FSH 适配的 Mapster DTO 映射
- [RequireWorkspaceRole] 权限装饰器

### Integration Points

- 路由组挂在 WorkspaceModule 或独立 AnalyticsModule
- 跨 DbContext 读取（AnalyticsHandler → IReadRepository<Issue>）
- Export 端点对接 Hangfire 后台作业

</code_context>

<specifics>
## Specific Ideas

- 严格按 Plane API 兼容路线：端点路径、响应格式、查询参数命名尽量一致
- 统计数据按 `state__group` 分组（backlog/unstarted/started/completed/cancelled）
- 优先周期进度：Phase 5 已有 burndown_completion_chart，Phase 12 在workspace层级聚合

</specifics>

<deferred>
## Deferred Ideas

- 自定义图表引擎 — 后续版本实现
- 仪表板 Widget — 后续版本
- AnalyticView 保存查询 — 后续版本
- 预计算/缓存优化 — 需要时再考虑

</deferred>
