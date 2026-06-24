# Phase 3: Project — 项目 - Context

**Gathered:** 2026-06-24
**Status:** Ready for planning
**Mode:** Smart discuss (autonomous)

<domain>
## Phase Boundary

交付 `YH.Modules.Project` 模块 —— 与 Plane API 契约兼容的**多租户项目管理**能力：项目 CRUD（含 identifier/prefix、软删除）、成员管理（角色复用 workspace Admin=20/Member=15/Guest=5 枚举）、项目设置（Unsplash 封面、标识符、描述、可见性）。

**In scope（REQ-3.1 ~ REQ-3.3）：**

- Project 实体 + 领域模型（identifier、slug、cover、description、visibility）
- ProjectMember 实体（角色、加入时间）
- Project CRUD 端点（create/get/update/delete/list）
- Project Member 端点（add/list/update-role/remove）
- Project 设置端点（通用设置 + Unsplash 封面）
- ProjectDbContext + 迁移（`yhschema.Project`）
- Project 权限 + workspace role 校验（复用 `[RequireWorkspaceRole]` 和 `ICurrentWorkspaceContext`）

**Out of scope：**

- Issue/WorkItem 管理 → Phase 4 WorkItems
- Cycle 管理 → Phase 5 Cycle
- Module 管理 → Phase 6 Module
- Page 文档 → Phase 7 Page
- View 视图 → Phase 8 View
- 前端 UI → Phase 13 Flow Web

</domain>

<decisions>
## Implementation Decisions

### 灰色区域 1：项目实体模型

- **项目标识符** — 实体同时包含 `Identifier`（用户设置短前缀，如 "PROJ"，唯一约束）和 `Slug`（自动从名称生成，用于 URL）。Identifier 用于 Issue 引用编号格式（如 PROJ-123），与 Plane Issue 引用机制兼容。
- **封面图片** — 在 Project 实体上存储为可空字符串(`CoverImageUrl`)。Unsplash 直接返回 URL，无需独立 CoverImage 实体。
- **软删除** — 遵循 D-08 Workspace 模式：软删除时追加 `__{epoch}` 到 slug，释放原 slug 供新项目复用。同时标记 `DeletedAt`。

### 灰色区域 2：成员与权限模型

- **项目成员关系** — 一等公民 `ProjectMember` 实体，独立于 `WorkspaceMember`。允许工作区 Guest 角色用户成为项目 Member，支持 Plane 的精细项目级成员管控。
- **Owner 表达** — 遵循 D-06：Project 存 `OwnerId`（Guid 标量，无跨模块 FK）。创建时创建者自动获得 ProjectMember Admin 角色。
- **角色粒度** — 直接复用 Workspace 角色枚举（Admin=20, Member=15, Guest=5）。保持一致性，Plane 使用相同枚举值。复用 `WorkspaceRole` 类型。

### 灰色区域 3：模块结构与模式

- **模块组织** — 遵循 Workspace 模块模式：独立 `Modules.Project.Contracts`（DTO/Commands/Queries/Contracts） + `Modules.Project`（实现/Domain/Features/Authorization/Data）。Features 按 `v1/{Entity}/{Action}/` 组织。
- **DbContext** — 独立 `ProjectDbContext`，`yhschema.Project` 数据库 schema。模块化单体干净分离，不耦合 WorkspaceDbContext。
- **API 路由** — 嵌套路由 `/api/v1/workspaces/{slug}/projects/`，与 Plane API 契约兼容。复用 workspace slug 解析流程。

### Claude's Discretion

- Project 实体不实现 `IHasTenant`（通过 workspace slug 解析确定租户上下文），ProjectMember 等其他实体通过路由上下文获得 tenant 隔离
- `Identifier` 唯一约束范围：workspace 级别（同一工作区内 identifier 唯一）+ DB 唯一约束保障
- 封面图片搜索端点（Unsplash）的集成方式：Phase 3 仅做封面 URL 存储；Unsplash API 搜索由前端直接调用或通过项目设置端点代理，规划阶段确定
- 项目列表支持筛选（状态/成员/时间）和排序，贴 Plane API 参数格式
- EF Core 实体配置、Contracts DTO 结构、Mediator feature 切分均贴 Phase 2 Workspace 模块模式
- 项目访问控制：只有 workspace Admin/Member 可创建项目；project Member 可编辑项目内容；project Guest/非成员需要 workspace-level 至少 Member 才能看到项目
- `ProjectMember` 配置 `UserId`（Guid 标量）+ `ProjectId`（FK）+ `Role`（复用 WorkspaceRole 枚举）
- `ProjectDbContext` 边界 —— 只含 Project/ProjectMember 等本模块实体，不包含 WorkItems/State/Label 等
- `Modules.Project` 注册方式：参照 WorkspaceModule.cs 的 `AddWorkspaceModule()` 扩展方法模式
- 测试项目：`Tests/Project.Tests`，遵循 Workspace.Tests 的 InMemory + 可选 Testcontainers 模式

</decisions>

<code_context>

## Existing Code Insights

### 可复用资产

- **WorkspaceModule.cs** 注册模式 — `AddWorkspaceModule()` 扩展方法，后续 Phase 3 复用为 `AddProjectModule()`
- **WorkspaceDbContext** 配置模式 — `IHasTenant` 自动过滤、Configuration 类组织方式
- **RequireWorkspaceRoleAttribute** + handler — Project 端点直接复用（授权依赖 `ICurrentWorkspaceContext`）
- **WorkspaceSlugStrategy** / Finbuckle 租户解析器 — Project 路由复用已有 slug→tenant 解析
- **Mediator 模式** — CreateWorkspaceEndpoint / CreateWorkspaceCommandHandler 作为 endpoint+handler 模板
- **Workspace.Tests** 测试项目结构 — FluentAssertions + NSubstitute + InMemory DbContext 模式

### 已建立模式

- Vertical Slice: 每操作一个 Feature 文件夹（Endpoint + Command/Query + Handler + Validator）
- Minimal API: `RouteHandlerBuilder` 扩展方法 `Map*Endpoint`
- FSH Soft Delete: 基类 `ISoftDelete` + 自动全局查询过滤器
- Auditing: `ICurrentUser` + `IAuditService`（Phase 1）

### 集成点

- 路由：`/api/v1/workspaces/{slug}/projects/` — 嵌套在 workspace 路由组内
- Permissions：`[RequireWorkspaceRole]` 检查 workspace 级别角色
- Tenant isolation：复用 Finbuckle slug resolver（WorkspaceSlugStrategy）

</code_context>

<specifics>
## Specific Ideas

- 项目列表端点支持 Plane 兼容的分页（`count/next/previous/results` 格式）
- Project response 包含 `cover_image_url` 字段（可空）
- Project 创建后自动创建对应 ProjectMember（Owner=Admin）
- Identifier 自动生成方案：用户未提供时取 name 的前 N 个大写字母

</specifics>

<deferred>
## Deferred Ideas

- Unsplash API 搜索端点的实现位置（Project 端点代理 vs 前端直调 Unsplash API）—— 规划阶段确定
- Issue 引用编号格式（PROJ-123）的编号生成逻辑 —— Phase 4 WorkItems 范围
- 项目归档功能 —— 如果 Plane 有归档（非删除）功能，需后续研究

</deferred>
