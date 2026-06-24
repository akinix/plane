# Phase 6: Module — 模块管理 - Context

**Gathered:** 2026-06-24
**Status:** Ready for planning

<domain>
## Phase Boundary

实现 Module CRUD、Module-Issue 关联、ModuleLink 管理。Module 实体位于 WorkItemsDbContext（`yhschema.WorkItems`），遵循与 Cycle 相同的垂直切片模式。

**范围内：**

- Module 实体 + EF 配置
- ModuleIssue M2M through 实体
- ModuleMember M2M through 实体（M2M through ModuleMember）
- ModuleLink 实体（title/url/metadata）
- Module CRUD 端点（Create/Get/Update/Delete/List）
- Module-Issue 关联端点（AddIssuesToModule/RemoveIssueFromModule/ListModuleIssues）
- Module 进度跟踪（实时聚合计算）
- Module Archive/Unarchive + ListArchivedModules
- ModuleDtoMapper + 状态计算
- WorkItemsDbContext 更新 + 迁移

**范围外（推迟）：**

- ModuleUserProperties（前端阶段 Phase 13）
- Module 收藏功能
- 富文本 description_text/description_html JSON 字段

</domain>

<decisions>
## Implementation Decisions

### Data Model Design

- Module 实体放在 WorkItemsDbContext（与 Cycle 一致）
- Status 字段: `string` enum values `"backlog"` / `"planned"` / `"in-progress"` / `"paused"` / `"completed"` / `"cancelled"`，默认 `"planned"`
- 日期字段: `DateTimeOffset?` StartDate / TargetDate（与 Cycle 模式保持一致）
- 描述字段: 仅 `Description` (string, max 10000)，暂不包含富文本 JSON 字段

### Relationship Design

- 包含 ModuleMember M2M through 实体（与 Plane 一致）
- 包含 ModuleLink 实体（title/url/metadata）
- 唯一约束: (TenantId, ProjectId, Name) WHERE deleted_at IS NULL; (TenantId, ModuleId, IssueId) WHERE deleted_at IS NULL
- ModuleUserProperties 推迟

### API Endpoints

- 路由: `/projects/{projectId}/modules/`（与 Cycle 一致）
- 标准 5 端点: Create / Get / Update / Delete / List
- Module-Issue 端点: AddIssuesToModule / RemoveIssueFromModule / ListModuleIssues
- Archive/Unarchive: ArchiveModule / UnarchiveModule / ListArchivedModules
- 进度跟踪: GetModuleProgress（实时聚合）

### Permissions & Authorization

- Module CRUD: Admin/Member 可写，Guest 只读
- Module-Issue 管理: Admin/Member 可添加/移除
- Status 变更: Admin/Member（与更新相同）
- Lead（负责人）：语义角色，不做特殊权限控制

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **Cycle 实体模式**: `Cycle.cs` 实现了 IHasTenant + ISoftDeletable + IAuditableEntity 接口，有 Factory 方法 + Update/Archive/Unarchive/SoftDelete 行为方法
- **CycleIssue 模式**: `CycleIssue.cs` 实现了 IHasTenant + ISoftDeletable，Factory + SoftDelete
- **CycleConfiguration**: `CycleConfiguration.cs` 定义了表名 `WorkItemsModuleConstants.SchemaName`，索引设计（Project_SortOrder + Tenant_Deleted_Project）
- **CycleIssueConfiguration**: 唯一索引 `(TenantId, CycleId, IssueId)` + HasFilter
- **Cycle CRUD 端点**: Create/Get/Update/Delete/List + Archive/Unarchive + DateCheck
- **CycleDtoMapper**: `CycleDtoMapper.cs` 含 ToDto() + ComputeStatus()
- **CycleConstants**: NameMaxLength=255, DescriptionMaxLength=10000, DefaultSortOrder=65535.0

### Established Patterns

- **垂直切片**: 每个 Feature 一个目录，含 Command/Query + Handler + Endpoint + Validator
- **CQRS**: Mediator library (BUnit's Mediator)，Commands 通过 IMediator.Send()
- **Contracts 分离**: 请求/响应 DTO 在 `Modules.WorkItems.Contracts`，v1/{Feature}/{Action}Command.cs
- **路由注册**: `Map{Group}Endpoints()` 扩展方法组合 `MapGroup("/api/v1/workspaces/{{slug}}/projects/{projectId}/modules")`
- **工作区角色权限**: `[RequireWorkspaceRole(Admin, Member)]` 属性
- **DTO JSON**: snake_case via JsonPropertyName，与 Plane 兼容

### Integration Points

- WorkItemsDbContext 需注册 Module/ModuleIssue/ModuleMember/ModuleLink DbSet
- Routes 通过 WorkItemsModule 的 route builder 注册
- 契约（Commands/DTOs/Constants）在 `Modules.WorkItems.Contracts` 中
- 迁移在 `YH.Flow.Migrations.PostgreSQL/WorkItems/` 中

</code_context>

<specifics>
## Specific Ideas

Module 实现应紧密跟随 Cycle 的模式，因为两者在 WorkItems 模块中的结构高度相似。关键差异：

1. Module 有 status 字段（Cycle 无，由日期计算）
2. Module 有 lead + M2M members（Cycle 无）
3. Module 有 ModuleLink（Cycle 无）
4. Module 有 (name, project) 唯一约束（Cycle 无）
5. Module 日期字段无"两者同 null 或同非 null"的配对规则

</specifics>

<deferred>
## Deferred Ideas

- ModuleUserProperties（前端需要的过滤/显示配置）→ Phase 13
- Module 收藏（UserFavorite）→ 集中收藏功能实现时
- 富文本 description_text/description_html → 按需扩展
- TransferIssuesBetweenModules（跨 Module 批量转移）→ 后续迭代

</deferred>
