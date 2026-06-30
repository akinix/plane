# Phase 8: View — 视图管理 - Context

**Gathered:** 2026-06-25
**Status:** Ready for planning
**Mode:** Smart discuss (autonomous)

<domain>
## Phase Boundary

实现 View CRUD、筛选条件持久化、项目级和工作区级视图管理。View 是独立模块，拥有自己的 `ViewDbContext`（`yhschema.View` schema），与 Project/WorkItems 模块解耦，但筛选条件关联 WorkItems Issue 字段。

**范围内（REQ-8.1, REQ-8.2）：**

- View 实体 + 领域模型（继承 Plane `IssueView` 模型设计）
- JSON-based 筛选条件存储（`Filters`, `DisplayFilters`, `DisplayProperties`, `RichFilters` 四个 JSON 字段）
- 项目级视图 CRUD（`/workspaces/{slug}/projects/{projectId}/views/`）
- 工作区级视图 CRUD（`/workspaces/{slug}/views/`）
- View Favorite 管理
- View 访问控制（Public/Private）
- 视图的筛选条件支持：状态、优先级、标签、指派人、创建者、开始日期、目标日期、Cycle、Module、估算点、父 Issue、子 Issue、提及、项目筛选、订阅者
- 视图的显示属性：分组、排序、布局、显示列
- EF 配置 + 迁移（`yhschema.View`）

**范围内（Phase 8 后续 wave 或推迟）：**

- WorkspaceViewIssuesViewSet（按视图筛选条件查询 Issues）— 依赖 WorkItems 模块的 Issue 查询和筛选
- 默认视图（Default View）设为目标项目设置字段或 view 标记
- View 排序（拖拽排序）
- 视图之间的 Issue 迁移
- 前后端集成（Phase 13）
- View 统计/分析端点

</domain>

<decisions>
## Implementation Decisions

### 模块结构与组织

- **新建 `Modules.View` 模块** — 独立于 WorkItems，贴近 Page 项目模式
- **独立 `ViewDbContext`**，schema `yhschema.View`
- API 路由:
  - 项目级: `/api/v1/workspaces/{slug}/projects/{projectId}/views/`（Plane 兼容）
  - 工作区级: `/api/v1/workspaces/{slug}/views/`（Plane 兼容）
- 模块注册: `AddViewModule()` 扩展方法（贴近 PageModule / ProjectModule 模式）
- 模块 Order: 290（在 Page 模块 260 之后）

### 实体模型设计

- **实体名: `View`**（贴 Plane `IssueView` 命名，但不加 Issue 前缀）
- **标准接口: `IGlobalEntity + ISoftDeletable + IAuditableEntity`**（参考 Page，无 IHasTenant，使用 IGlobalEntity opt-out tenant isolation）
- **ProjectId 可空** — null 表示工作区级视图（具有 workspace scope），非 null 表示项目级视图
- **筛选条件以 JSON 列存储** — 四个独立 JSON 字段匹配 Plane 设计：
  - `Filters`（字典 { key: value }，存储筛选条件）
  - `DisplayFilters`（字典，存储显示/分组设置）
  - `DisplayProperties`（字典，存储列显示开关）
  - `RichFilters`（字典，存储进阶筛选，可选）
- **Query 字段自动生成** — 基于 `Filters` 和实际筛选逻辑自动计算
- **Access 字段** — Private (0) / Public (1)，匹配 Plane 的 `(0, "Private"), (1, "Public")`

### 筛选条件序列化策略

- **所有筛选条件存储在 JSON 列 `Filters` 中** — 以字典形式存储，key 为筛选类型，value 为筛选值
- 支持的筛选键（匹配 Plane `issue_filters.py`）：
  - `priority` — 优先级筛选（数组: urgent, high, medium, low, none）
  - `state` — 状态 ID 筛选（UUID 数组）
  - `state_group` — 状态组分群（backlog, unstarted, started, completed, cancelled）
  - `assignees` — 指派人员 ID（UUID 数组）
  - `created_by` — 创建者 ID（UUID 数组）
  - `labels` — 标签 ID（UUID 数组）
  - `start_date` / `target_date` / `completed_at` — 日期范围
  - `type` — Issue 类型（backlog, active）
  - `cycle` — Cycle ID（UUID 数组）
  - `module` — Module ID（UUID 数组）
  - `parent` — 父 Issue ID（UUID 数组）
  - `estimate_point` — 估算点 ID（UUID 数组）
  - `sub_issue` — 是否包含子 Issue（bool）
  - `subscriber` — 订阅者 ID（UUID 数组）
  - `name` — 名称搜索（字符串）
- **显示属性存储在 `DisplayProperties`** — 键为属性名，值为 bool，Include/Exclude 模式
- **排序存储在 `DisplayFilters.order_by`** — 字符串，如 `"-created_at"`
- **优缺点**：JSON 列方式最具灵活性，无需为每个筛选字段创建独立数据库列，匹配 Plane 行为

### ViewType 范围

- **仅实现 Issue 视图** — 当前阶段聚焦 Issue 列表的筛选/排序/显示配置
- **Page 视图推迟** — Phase 7 Page 模块稳定后再考虑
- **视图命名**：实体名为 `View`，DTO 为 `ViewDto`

### Favorite 管理

- **新建 `ViewFavorite` 实体** — 类似 PageFavorite 模式，实现 `IHasTenant + ISoftDeletable`
- 路由: `POST/DELETE /workspaces/{slug}/projects/{projectId}/views/{viewId}/favorite/`
- 幂等操作（重复添加/删除不报错）

### Claude's Discretion

- View 实体使用 `IGlobalEntity`（无 TenantId），通过路由 slug 确定租户上下文
- 项目级视图筛选：通过 ProjectId 字段 + 路由 projectId 参数限制作用域
- 工作区级视图筛选：ProjectId 为 null，通过 route slug 限制工作区
- ViewDto 结构参考 Plane IssueViewSerializer 字段设计
- 排序默认按 `name` 升序 + `-created_at` 降序（Plane 行为）
- 软删除模式贴 SoftDelete（Plane 硬删除 — 我们的实现使用软删除保持一致性）
- View 列表默认返回所有非删除视图（project-scope 或 workspace-scope）
- DTO JSON 使用 snake_case 属性名（JsonPropertyName），与 Plane 兼容
- 不实现 IHasDomainEvents（View 不需要活动日志审计）

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **Page 模块模式**: `Modules.Page` — Contracts 分离 + Domain 实体 + EF Configuration + 独立 DbContext + 模块注册扩展方法（最接近的参考）
- **WorkspaceModule.cs 注册模式**: `AddWorkspaceModule()` 扩展方法 — 可复写为 `AddViewModule()`
- **RequireWorkspaceRoleAttribute**: View 端点复用（授权依赖 `ICurrentWorkspaceContext`）
- **WorkspaceSlugStrategy / Finbuckle**: View 路由复用已有 slug→tenant 解析
- **Mediator 模式**: Create/Get/Update/Delete 端点作为垂直切片模板
- **IGlobalEntity + ISoftDeletable + IAuditableEntity**: View 实体接口组合（参考 Page 模式）
- **Page.Tests / Project.Tests**: 测试项目结构模式

### Established Patterns

- **模块组织**: 每个独立领域一个模块（Workspace, Project, WorkItems, Page, View）
- **DbContext**: 独立 DbContext + 独立 schema（View → yhschema.View）
- **垂直切片**: 每个 Feature 一个目录，含 Command/Query + Handler + Endpoint + Validator
- **CQRS**: Mediator library，Commands 通过 IMediator.Send()
- **Contracts 分离**: 请求/响应 DTO 在 `Modules.View.Contracts`
- **DTO JSON**: snake_case via JsonPropertyName，与 Plane 兼容
- **软删除**: ISoftDeletable — `SoftDelete()` 方法 + EF HasQueryFilter
- **IGlobalEntity**: 无 TenantId，通过路由上下文确定租户（参考 Page 和 Workspace 模式）

### Integration Points

- API 路由通过 ViewModule 的 route builder 注册到 Program.cs / 主路由
- 契约（Commands/DTOs/Constants）在 `Modules.View.Contracts` 中
- 迁移在 `YH.Flow.Migrations.PostgreSQL/Views/` 中
- `ICurrentWorkspaceContext` + `[RequireWorkspaceRole]` 复用 Phase 2 的授权管道
- Host 项目 Program.cs 需调用 `AddViewModule()` 扩展方法
- View 筛选条件在将来需要 Issue 查询集成时关联 WorkItems 的 Issue 实体

</code_context>

<specifics>
## Specific Ideas

- View 实现应紧密跟随 Plane `IssueView` API 契约，确保未来前端兼容
- 筛选条件以原始 JSON 存储，不解析/验证值格式（Plane 行为 — JSON 字段仅作为容器，筛选逻辑在查询时解析）
- Query 字段自动从 Filters 生成（与 Plane `save()` 方法中的 `issue_filters()` 调用等效）
- 无日期配对规则（StartDate/TargetDate 独立）
- 无 Completed 限制（与 Cycle 不同，Update 不受状态限制）
- 访问控制: Access 字段（Private=0, Public=1），Private 仅 owned_by 可查看
- View 实体使用 IGlobalEntity + ISoftDeletable + IAuditableEntity（不使用 IHasTenant）
- ViewFavorite 实体使用 IHasTenant + ISoftDeletable

**字段对应（Plane IssueView -- YH.Flow View）:**

| Plane IssueView    | YH.Flow View      | 类型            | 说明                      |
| ------------------ | ----------------- | --------------- | ------------------------- |
| name               | Name              | string(255)     | 视图名称                  |
| description        | Description       | string?         | 视图描述                  |
| query              | Query             | JSON (string?)  | 自动生成的查询参数        |
| filters            | Filters           | JSON (string?)  | 筛选条件字典              |
| display_filters    | DisplayFilters    | JSON (string?)  | 显示/分组设置             |
| display_properties | DisplayProperties | JSON (string?)  | 显示列开关                |
| rich_filters       | RichFilters       | JSON (string?)  | 进阶筛选                  |
| access             | Access            | int             | Private=0, Public=1       |
| sort_order         | SortOrder         | float           | 排序值，默认 65535        |
| logo_props         | LogoProps         | JSON (string?)  | Logo 属性                 |
| owned_by           | OwnedBy           | Guid            | 所有者用户 ID             |
| is_locked          | IsLocked          | bool            | 锁定标记                  |
| archived_at        | ArchivedAt        | DateTimeOffset? | 归档时间                  |
| project            | ProjectId         | Guid?           | 关联项目（null=工作区级） |

</specifics>

<deferred>
## Deferred Ideas

- WorkspaceViewIssuesViewSet（按视图筛选条件查询 Issues）-- 需要 WorkItems Issue 查询基础设施稳定后再实现
- 默认视图标记（`is_default` 字段或 Project 设置）-- 后续迭代
- View 拖拽排序 -- 后续迭代
- View 统计/分析端点 -- 后续迭代
- View Duplicate（复制视图）-- 后续迭代
- 前后端 View 组件 -- Phase 13
- 跨项目视图（Plane 的 GlobalView）-- 后续迭代

</deferred>
