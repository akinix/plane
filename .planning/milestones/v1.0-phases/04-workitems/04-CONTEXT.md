# Phase 4: WorkItems — 工作项 - Context

**Gathered:** 2026-06-24
**Status:** Ready for planning
**Mode:** Discuss

<domain>
## Phase Boundary

交付完整的**多租户工作项管理系统**——涵盖 Issue 生命周期（状态流转、优先级、父子关联、链接）、Label 标签、Estimate 评估、Comment 评论、Activity 审计日志、Intake 收件箱、Import/Export 导入导出。

工作项属于项目（Project），通过 `ProjectId` 关联。所有实体多租户隔离（TenantId）。

**In scope（REQ-4.1 ~ REQ-4.9）：**

- State 实体 + 状态生命周期管理
- Label 实体 + 标签 CRUD
- Issue 实体（含优先级、日期、指派人、父子关联、Issue 链接）
- Issue CRUD 端点（含批量操作）
- IssueComment 实体 + CRUD 端点
- IssueActivity 实体 + 审计日志端点
- Estimate 实体 + CRUD 端点
- Intake 收件箱端点（Issue 草稿/审核流程）
- Import/Export 导入导出端点
- WorkItemsDbContext + EF 迁移（`yhschema.WorkItems`）
- WorkItems 权限（复用 WorkspaceRole 和 ProjectMember 角色）

**Out of scope：**

- Cycle 管理 → Phase 5
- Module 管理 → Phase 6
- Page 文档 → Phase 7
- View 视图 → Phase 8
- 前端 UI → Phase 13 Flow Web
  </domain>

<decisions>
## Implementation Decisions

### 灰色区域 1：Issue 实体模型

- **优先级字段** — 字符串类型 `Priority`，取值范围：`"urgent"` / `"high"` / `"medium"` / `"low"` / `"none"`。与 Plane 兼容，不使用独立 Priority 实体或枚举。
- **父子关联** — `ParentId`（Guid? 自引用 FK）。深度限制为 1 层（Plane 行为，不支持多层嵌套树）。
- **Issue 链接** — 独立 `IssueLink` 实体，`LinkType` 枚举区分关系类型：`RelatesTo` / `Duplicate` / `Blocks` / `BlockedBy`。与 Plane issue_link 表兼容。
- **编号** — `SequenceId`（项目内自增 int）+ `SortOrder`（double，默认 65535.0）。`SequenceId` 格式如 `PROJ-1`（Phase 3 的 Identifier + SequenceId 组合在 API 响应中拼接）。

### 灰色区域 2：State 生命周期

- **State 实体结构** — `Id`（Guid）、`Name`（可自定义 string）、`Group`（枚举）、`ProjectId`（Guid）、`Color`（string?）、`IsDefault`（bool，每个 group 一个默认）、`SortOrder`（double）。
- **Group 枚举** — 固定 5 个值，与 Plane 兼容：
  - `Backlog`（0）— 待办
  - `Unstarted`（1）— 未开始
  - `Started`（2）— 进行中
  - `Completed`（3）— 已完成
  - `Cancelled`（4）— 已取消
- **默认模板** — 项目创建时自动创建 5 个默认 State，name 可自定义：
  - Backlog / Todo（Unstarted）/ In Progress（Started）/ Done（Completed）/ Cancelled（Cancelled）
- **关闭状态语义** — `Completed` 和 `Cancelled` group 的 State 视为"关闭"状态。Issue 在这些状态下不可直接修改（需要先 reopen）。
- **迁移注意** - Phase 4 需要为已有项目插入默认 State 种子数据。DbMigrator 启动时检测并填充。

### 灰色区域 3：DbContext 边界

- **独立 DbContext** — 使用独立 `WorkItemsDbContext`，EF Core schema `yhschema.WorkItems`。跟随 Phase 3 的 ProjectDbContext 模式。
- **包含实体** — State、Label、Issue (含链接)、IssueComment、IssueActivity、Estimate、Intake（及其子实体）。不包含 Cycle/Module 实体。
- **跨模块关联** — Issue 通过 `ProjectId`（Guid 标量）关联 Project，不跨 DbContext 做 JOIN。IHasTenant 自动过滤。
- **模块注册** — `AddWorkItemsModule()` 扩展方法，与 Phase 3 的 `AddProjectModule()` 模式一致。

### Claude's Discretion

- Label 实体：简单键值对（`Name` + `Color` + `ParentId?`），支持层级标签
- Estimate 实体：Plane 使用 points（数值），支持 `EstimatePoint` 值对象
- IssueActivity：使用 MediatR 领域事件捕获变更 + 异步持久化
- Intake：Issue 的"收件箱"模式——Issue 有 `is_intake` 标记，不经 Intake 流程直接创建的 Issue 没有该标记
- Import/Export：通过 API 端点直接处理 CSV/JSON 格式（非后台 Job）
- 权限模型：复用 `[RequireWorkspaceRole]` + ProjectMember 角色检查
- WorkItemsModule 注册 Order：260（在 Project 250 之后）

## Specific Ideas

- Issue 列表端点支持 Plane 兼容分页（`count/next/previous/results`）+ 多维筛选（assignee/priority/state/label）
- Issue 批量操作端点支持批量更新状态/指派人/优先级
- Import 端点支持 CSV/JSON 文件上传，自动验证字段映射
- 活动日志记录格式：`"{actor} updated {field} from {old} to {new}"`
  </decisions>

<code_context>

## Existing Code Insights

### 可复用资产

- **ProjectDbContext** 配置模式 — WorkItemsDbContext 跟随相同模式（BaseDbContext + ApplyConfigurationsFromAssembly 优先顺序）
- **ProjectModule.cs** 注册模式 — `AddProjectModule()` → WorkItems 使用 `AddWorkItemsModule()`，Order 260
- **IHasTenant 多租户** — 所有 WorkItems 实体复用已有 BaseDbContext 的 ApplyTenantIsolationByDefault
- **RequireWorkspaceRoleAttribute** + handler — 端点直接复用
- **ICurrentWorkspaceContext** — 路由上下文获得 workspace slug→tenant 解析
- **Mediator 模式** — Phase 3 的 CRUD 端点模式直接套用

### 已建立模式

- Vertical Slice: 每操作一个 Feature 文件夹（Endpoint + Command/Query + Handler + Validator）
- Minimal API: `RouteHandlerBuilder` 扩展方法 `Map*Endpoint`
- FSH Soft Delete: `ISoftDelete` + 自动全局查询过滤器
- Auditing: `ICurrentUser` + `IAuditService`
- PlanePagedResult 分页格式
- 独立 schema per 模块（`yhschema.Project` → `yhschema.WorkItems`）

### 集成点

- 路由：`/api/v1/workspaces/{slug}/projects/{projectId}/issues/` — 嵌套在 project 路由组下
- Permissions：`[RequireWorkspaceRole]` 检查 workspace 角色 + ProjectMember 角色检查
- Tenant isolation：复用 Finbuckle slug resolver（所有实体 IHasTenant）
- Label 与 Issue：多对多关系（中间表 IssueLabels）
- State 与 Issue：一对多（StateId on Issue）

</code_context>

<canonical_refs>

- `.planning/ROADMAP.md` — Phase 4 task breakdown (15 tasks)
- `.planning/REQUIREMENTS.md` — REQ-4.1 ~ REQ-4.9
- `.planning/phases/03-project/03-CONTEXT.md` — Phase 3 decisions (Project entity, ProjectMember pattern, schema conventions)
- `.planning/phases/03-project/03-RESEARCH.md` — Technical research (entity patterns, DbContext conventions)
  </canonical_refs>

<deferred>
## Deferred Ideas

- Issue 时间线视图（甘特图）的 API 支持 — 超出 Phase 4 scope
- 工作项模板/预设 — 超出 Phase 4 scope
- 全文搜索 — 超出 Phase 4 scope
  </deferred>
