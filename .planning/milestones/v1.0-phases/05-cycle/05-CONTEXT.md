# Phase 5: Cycle — 周期管理 - Context

**Gathered:** 2026-06-24
**Status:** Ready for planning

<domain>
## Phase Boundary

交付完整的**周期管理能力**——Cycle（迭代/冲刺）CRUD、Cycle-Issue 关联管理、Burndown 进度数据。周期属于项目（Project），通过 `ProjectId` 关联。所有实体多租户隔离（TenantId）。

Cycle 实体纳入 **WorkItemsDbContext**（`yhschema.WorkItems` schema），与 Issue/State/Label 等共享同一个 DbContext。

**In scope（REQ-5.1 ~ REQ-5.2）：**

- Cycle 实体（名称、日期范围、描述、外部来源/ID、sort_order、progress_snapshot、timezone、logo_props）
- CycleIssue 桥接实体（多对多关联 Cycle ↔ Issue）
- Cycle CRUD 端点（create/get/update/delete/list）
- Cycle 状态（DRAFT/UPCOMING/CURRENT/COMPLETED — 基于日期范围动态计算）
- Cycle-Issue 关联端点（add/remove/list issues for cycle）
- Issue 在 Cycle 间迁移（transfer incomplete issues）
- Burndown 进度数据（按日期显示剩余 Issue 数量）
- Cycle 归档/取消归档端点
- Cycle 权限（复用 WorkspaceRole + ProjectMember 角色）

**Out of scope：**

- CycleUserProperties（每用户显示设置）→ Phase 13 Flow Web 前端
- Cycle 收藏（Favorites）→ Phase 13 Flow Web 前端
- Cycle 进度邮件/通知 → Phase 11 Notification
- Module 管理 → Phase 6 Module
- 前端 UI → Phase 13 Flow Web

</domain>

<decisions>
## Implementation Decisions

### 灰色区域 1：模块归属

- **D-01:** Cycle 纳入 **WorkItemsDbContext**（`yhschema.WorkItems` schema），与 Issue/State/Label 共享 DbContext。CycleIssue 桥接表使用真正 FK 约束关联 Issue 和 Cycle，Burndown 查询可在一个 DbContext 内完成聚合操作。
- **理由：** Cycle 与 Issue 紧密耦合（多对多关联、Burndown 实时聚合查询），独立 DbContext 需要跨 DB 操作和 Guid 标量引用，复杂度大于收益。

### 灰色区域 2：Burndown 计算

- **D-02:** API **实时计算**模式。GET 请求时动态查询 Issue 表，按 `completed_at` 日期统计完成数量，从总量中累减得到每日剩余量。
- **Plane 实现参考：** 使用 `TruncDate(completed_at)` 按日期分组累加完成数，对日期范围内每一天计算 `total_scope - cumulative_completed`。未来日期设为 `None`。
- **progress_snapshot：** 仅在 **transfer-issues**（从已结束 Cycle 迁移 Issue）时冻结快照存为 JSON。活跃 Cycle 不做快照。
- **不采用** 预计算快照模式（维护复杂、一致性问题）。

### 灰色区域 3：已结束 Cycle 编辑限制

- **D-03:** **严格匹配 Plane 行为**：
  - COMPLETED 周期只允许修改 `sort_order`、`name`、`description`
  - 禁止添加新 Issue 到已结束周期
  - 允许将未完成 Issue（backlog/unstarted/started）通过 transfer-issues 迁移到新周期
  - 迁移时自动冻结 `progress_snapshot` 保存当前统计

### Claude's Discretion

- **Cycle 状态**：与 Plane 一致，**动态计算**（不存储 status 字段）。使用 `Case/When` 注解基于日期范围计算状态：
  - `DRAFT` — start_date 和 end_date 均为 null
  - `UPCOMING` — start_date > now
  - `CURRENT` — start_date ≤ now ≤ end_date
  - `COMPLETED` — end_date < now
- **CycleIssue 桥接表**：与 Plane 一致，`(Issue, Cycle)` 唯一约束 + 软删除唯一索引
- **sort_order**：与 Plane 一致，新 Cycle 自动设为当前项目最小 sort_order - 10000
- **归档**：实现 archive/unarchive 端点，只有 end_date < now 的 Cycle 可以归档
- **日期校验**：实现 Cycle 日期重叠检查（POST `cycles/date-check` 端点）
- **Cycle 列表参数**：`cycle_view` 查询参数筛选 `current`/`upcoming`/`completed`/`draft`/`incomplete`/`all`
- **路由格式**：`/api/v1/workspaces/{slug}/projects/{projectId}/cycles/` — 嵌套在 project 路由组下
- **权限**：复用 `[RequireWorkspaceRole]` + ProjectMember 角色检查
- **Burndown 响应格式**：返回 `{ "completion_chart": { "2026-06-01": 15, "2026-06-02": 12, ... } }`，支持按 `?type=issues|points` 切换计算基础
- **Cycle CRUD 序列化器**：响应包含注释字段（total_issues/completed_issues/cancelled_issues/started_issues/unstarted_issues/backlog_issues/status/is_favorite/assignee_ids）
- **归档端点**：独立路由 `/archived-cycles/` + POST archive/DELETE unarchive
- **CycleUserProperties** 和 **Favorites**：不在 Phase 5 实现，留待 Phase 13 前端支持时补充

</decisions>

<canonical_refs>

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Plane Reference Implementation

- `apps/api/plane/db/models/cycle.py` — Cycle/CycleIssue/CycleUserProperties 模型定义（完整字段、约束、save 方法）
- `apps/api/plane/api/views/cycle.py` — REST API 端点（CRUD、CycleIssue、Transfer、Archive）
- `apps/api/plane/app/views/cycle/base.py` — 新一代端点（CycleViewSet、CycleProgressEndpoint、CycleAnalyticsEndpoint、CycleDateCheckEndpoint、CycleFavoriteViewSet）
- `apps/api/plane/app/views/cycle/archive.py` — 归档相关视图
- `apps/api/plane/api/serializers/cycle.py` — 序列化器（CycleSerializer、CycleIssueSerializer、CycleWriteSerializer）
- `apps/api/plane/app/serializers/cycle.py` — 新一代序列化器（CycleProgressSerializer、CycleAnalyticsSerializer）
- `apps/api/plane/api/urls/cycle.py` — URL 路由定义
- `apps/api/plane/utils/analytics_plot.py` — burndown_plot 函数（核心 Burndown 算法）
- `apps/api/plane/utils/cycle_transfer_issues.py` — Issue 迁移逻辑（冻结 snapshot、分配新 Cycle）

### Frontend Contract (API 兼容参考)

- `packages/types/src/cycle/cycle.ts` — TypeScript 类型定义（ICycle、TCycleProgress、TCycleDistribution、TCycleEstimateType）
- `packages/types/src/cycle/cycle_filters.ts` — 筛选/显示类型
- `packages/services/src/cycle/cycle.service.ts` — CycleService（前端 API 调用规范）
- `packages/services/src/cycle/cycle-operations.service.ts` — 收藏/迁移操作
- `packages/services/src/cycle/cycle-archive.service.ts` — 归档操作
- `packages/services/src/cycle/cycle-analytics.service.ts` — 分析 API 调用

### Project Documents

- `.planning/ROADMAP.md` — Phase 5 task breakdown (7 tasks, REQ-5.1 ~ REQ-5.2)
- `.planning/REQUIREMENTS.md` — REQ-5.1 Cycle CRUD, REQ-5.2 Cycle-Issue 关联
- `.planning/phases/04-workitems/04-CONTEXT.md` — Phase 4 决策（WorkItemsDbContext 模式、Issue 实体模型）
- `.planning/phases/03-project/03-CONTEXT.md` — Phase 3 决策（Project 实体、路由嵌套模式）

</canonical_refs>

<code_context>

## Existing Code Insights

### 可复用资产

- **WorkItemsDbContext** — Cycle 实体直接纳入现有 WorkItemsDbContext，复用 `yhschema.WorkItems` schema
- **Issue 实体** — Cycle 通过 CycleIssue 桥接表关联 Issue，复用 Issue 的 `completed_at`、`estimate_point_value` 等字段做 Burndown 计算
- **WorkItemsModule.cs** 注册模式 — Cycle 功能在 `AddWorkItemsModule()` 中注册，Order 维持 260
- **Project 路由组** — Cycle 路由嵌套在 project 下：`/api/v1/workspaces/{slug}/projects/{projectId}/cycles/`
- **RequireWorkspaceRoleAttribute** + handler — 端点直接复用
- **ICurrentWorkspaceContext** — 路由上下文获得 workspace slug → tenant 解析
- **Mediator 模式** — Phase 3/4 的 CRUD 端点模式直接套用
- **State Group 枚举** — Burndown 按 state group 分类 Issue 状态（Backlog/Unstarted/Started/Completed/Cancelled）
- **PlanePagedResult** 分页格式 — Cycle 列表复用
- **FSH Soft Delete** — Cycle 和 CycleIssue 都实现 `ISoftDelete`

### 已建立模式

- Vertical Slice: 每操作一个 Feature 文件夹（Endpoint + Command/Query + Handler + Validator）
- Minimal API: `RouteHandlerBuilder` 扩展方法 `Map*Endpoint`
- FSH Soft Delete: `ISoftDelete` + 自动全局查询过滤器
- 多租户: `IHasTenant` + `BaseDbContext.ApplyTenantIsolationByDefault`
- 独立 schema per 模块（Cycle 纳入 `yhschema.WorkItems`）

### 集成点

- 路由：`/api/v1/workspaces/{slug}/projects/{projectId}/cycles/` — 嵌套在 project 路由组下
- Permissions：`[RequireWorkspaceRole]` 检查 workspace 角色 + ProjectMember 角色检查
- Tenant isolation：复用 Finbuckle slug resolver（所有实体 IHasTenant）
- Issue 与 Cycle：多对多关系通过 CycleIssue 桥接表（WorkItemsDbContext 内）
- Burndown：实时聚合查询 Issue 的 `completed_at` 和 `estimate_point_value`

</code_context>

<specifics>
## Specific Ideas

- Cycle 状态动态计算的 Case/When SQL 注解，直接复用 Plane 的四状态逻辑（DRAFT/UPCOMING/CURRENT/COMPLETED）
- Cycle 列表端点支持 `cycle_view` 查询参数（current/upcoming/completed/draft/incomplete/all）
- Burndown 端点支持 `?type=issues|points` 切换计算基础（Issue 计数 vs 估算点求和）
- 日期校验端点 `cycles/{cycle_id}/date-check` 防止 Cycle 时间范围重叠
- 归档端点：GET `/archived-cycles/`（列表）+ POST archive + DELETE unarchive
- Cycle 创建时自动计算 sort_order（最小 sort_order - 10000）
- Transfer endpoint 在迁移 Issue 时自动冻结 progress_snapshot
- Cycle 响应包含注释字段：`total_issues`、`completed_issues`、`cancelled_issues`、`started_issues`、`unstarted_issues`、`backlog_issues`、`status`

</specifics>

<deferred>
## Deferred Ideas

- CycleUserProperties（每用户筛选/显示偏好）— 超出 Phase 5 scope，Phase 13 前端补充
- Cycle 收藏（Favorites）— 超出 Phase 5 scope，Phase 13 前端补充
- Cycle 进度通知/邮件 — Phase 11 Notification
- Cycle 分析图表 API（按指派人/标签分布）— 超出 Phase 5 scope，Phase 12 Analytics
- 甘特图/Gantt 视图 — 前端能力，Phase 13

</deferred>

---

_Phase: 5-Cycle_
_Context gathered: 2026-06-24_
