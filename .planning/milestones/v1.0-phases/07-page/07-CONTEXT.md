# Phase 7: Page — 文档管理 - Context

**Gathered:** 2026-06-25
**Status:** Ready for planning
**Mode:** Smart discuss (autonomous)

<domain>
## Phase Boundary

实现 Page CRUD、层级结构、富文本内容存储。Page 是独立的模块，拥有自己的 `PageDbContext`（`yhschema.Page` schema），与 Workspace/Project/WorkItems 模块解耦。

**范围内（REQ-7.1, REQ-7.2）：**

- Page 实体 + 领域模型（name, description_html, description_stripped, description_json, access, color, parent, sort_order, is_locked, archived_at）
- `ProjectPage` 桥接实体（Page ↔ Project M2M through）
- 自引用 FK `ParentId → Page.Id` 实现层级结构（树形嵌套）
- `PageDbContext` + EF 迁移（`yhschema.Page`）
- 核心 Page CRUD（Create/Get/Update/Delete/List + pages-summary）
- Page 归档/恢复
- Page Description 独立 CRUD 端点
- Page Favorite 管理
- Page 权限（Private/Public access + workspace role authz）

**范围内（Phase 7 后续 wave 或推迟）：**

- PageVersion 管理
- Page Duplicate
- Page Lock/Unlock
- Page 移动（moved_to_page, moved_to_project）
- description_binary 支持
- PageLabel M2M through 实体

</domain>

<decisions>
## Implementation Decisions

### 模块结构与组织

- **新建 `Modules.Page` 模块** — 独立于 WorkItems，贴 Project 模块模式
- **独立 `PageDbContext`**，schema `yhschema.Page`
- API 路由: `/api/v1/workspaces/{slug}/projects/{projectId}/pages/`（Plane 兼容）
- 模块注册: `AddPageModule()` 扩展方法（贴 WorkspaceModule / ProjectModule 模式）

### 实体模型设计

- **实体名: `Page`**，贴 Plane 命名
- **标准接口: `IHasTenant + ISoftDeletable + IAuditableEntity`**
- **层级结构: 自引用 FK `ParentId`**，非闭包表
- **Project 关联: `ProjectPage` 桥接实体**（M2M through），Plane API 兼容

### 富文本内容策略

- **核心字段: `DescriptionHtml` + `DescriptionStripped`（自动生成纯文本）**
- **`DescriptionJson`（可空 JSON）** — 可选编辑器状态同步
- **`DescriptionStripped` 自动生成** — 在 SaveChanges 或实体 Save 时自动 strip HTML
- **description_binary 推迟**
- **外部引用字段** — `ExternalId` + `ExternalSource` 可空字段（Plane 兼容）

### API Endpoints 范围

- **核心 CRUD**: Create/Get/Update/Delete/List + Summary（Plane 完全兼容）
- **Archive/Unarchive**: 包含归档与恢复端点，无日期限制
- **Description CRUD**: 独立端点 `/{pageId}/description/`（Plane 兼容）
- **Favorite**: POST/DELETE `/{pageId}/favorite/`
- **推迟**: PageVersion、Duplicate、Lock/Unlock、Move

### Claude's Discretion

- Page 实体不实现 `IHasTenant`（通过 workspace slug 路由上下文确定租户），`ProjectPage` 等其他实体通过路由上下文获得 tenant 隔离
- PageDTO 结构参考 Plane PageSerializer + PageDetailSerializer 的字段设计
- 分页、权限授权贴 Existing Code Insights 中已建立的模式
- Summary 端点返回项目级别的 page 概览（总数、最近更新等）
- 软删除模式贴 SoftDelete（不追加 slug 后缀 — Page 无 slug 字段）
- Page 列表默认仅返回顶层（`parent == null`），Plane 兼容
- 排序: `SortOrder` float 字段（默认 65535）+ `-CreatedAt` 后备排序
- 模块 Contracts 独立为 `Modules.Page.Contracts`
- EntityTypeConfiguration 使用 `PageConfiguration` / `ProjectPageConfiguration` / `PageFavoriteConfiguration`

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **Project 模块模式**: `Modules.Project` — Contracts 分离 + Domain 实体 + EF Configuration + 独立 DbContext + 模块注册扩展方法
- **WorkspaceModule.cs 注册模式**: `AddWorkspaceModule()` 扩展方法 — Phase 7 复用为 `AddPageModule()`
- **RequireWorkspaceRoleAttribute**: Page 端点直接复用（授权依赖 `ICurrentWorkspaceContext`）
- **WorkspaceSlugStrategy / Finbuckle**: Page 路由复用已有 slug→tenant 解析
- **Mediator 模式**: CreateWorkspaceEndpoint/Handler 作为垂直切片模板
- **IHasTenant + ISoftDeletable + IAuditableEntity**: 所有领域实体标准接口
- **Workspace.Tests / Project.Tests**: 测试项目结构模式

### Established Patterns

- **模块组织**: 每个独立领域一个模块（Workspace, Project, WorkItems, Page）
- **DbContext**: 独立 DbContext + 独立 schema（Project → yhschema.Project, Page → yhschema.Page）
- **垂直切片**: 每个 Feature 一个目录，含 Command/Query + Handler + Endpoint + Validator
- **CQRS**: Mediator library (BUnit's Mediator)，Commands 通过 IMediator.Send()
- **Contracts 分离**: 请求/响应 DTO 在 `Modules.Page.Contracts`
- **DTO JSON**: snake_case via JsonPropertyName，与 Plane 兼容
- **软删除**: ISoftDeletable — `SoftDelete()` 方法 + EF HasQueryFilter

### Integration Points

- API 路由通过 PageModule 的 route builder 注册到 `Program.cs` / 主路由
- 契约（Commands/DTOs/Constants）在 `Modules.Page.Contracts` 中
- 迁移在 `YH.Flow.Migrations.PostgreSQL/Pages/` 中
- `ICurrentWorkspaceContext` + `[RequireWorkspaceRole]` 复用 Phase 2 的授权管道
- Host 项目 Program.cs 需调用 `AddPageModule()` 扩展方法

</code_context>

<specifics>
## Specific Ideas

- Page 实现应紧密跟随 Plane API 契约，确保前端兼容
- 内容存储以 `DescriptionHtml` 为主，`DescriptionStripped` 自动从 HTML strip 生成（Plane 的 `save()` 等效）
- `description_json` 可选存储，用于编辑器状态同步
- 无日期配对规则（StartDate/TargetDate 独立）
- 无 COMPLETED 限制（与 Cycle 不同，Update 不受状态限制）
- 无日期限制的归档（任何状态的 Page 皆可归档）
- 访问控制: `Access` 字段（Private=1, Public=0），Private 仅 owned_by 可查看

</specifics>

<deferred>
## Deferred Ideas

- PageVersion 管理 → Phase 7 后续 wave 或独立子阶段
- Page Duplicate 端点 → 后续迭代
- Page Lock/Unlock → 后续迭代
- Page Move（moved_to_page, moved_to_project）→ 后续迭代
- PageLabel M2M through 实体（Page ↔ Label）→ 后续迭代
- 富文本编辑器实时同步 → 前端阶段 Phase 13
- 附件/图片嵌入（T7.2 部分）→ 按需实现

</deferred>
