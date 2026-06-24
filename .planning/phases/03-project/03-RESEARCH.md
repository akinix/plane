# Phase 3: Project — 项目 - Research

**Researched:** 2026-06-24
**Domain:** 多租户项目管理（Project CRUD + ProjectMember + 嵌套 Workspace 路由 + Plane API 契约迁移）
**Confidence:** HIGH（核心栈已通过现有 Workspace 模块 + Plane 源码双重验证；仅 Unsplash 集成方式为 ASSUMED）

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions

#### 灰色区域 1：项目实体模型

- **项目标识符** — 实体同时包含 `Identifier`（用户设置短前缀，如 "PROJ"，唯一约束）和 `Slug`（自动从名称生成，用于 URL）
- **封面图片** — 在 Project 实体上存储为可空字符串(`CoverImageUrl`)。Unsplash 直接返回 URL，无需独立 CoverImage 实体。
- **软删除** — 遵循 D-08 Workspace 模式：软删除时追加 `__{epoch}` 到 slug，释放原 slug 供新项目复用。同时标记 `DeletedAt`。

#### 灰色区域 2：成员与权限模型

- **项目成员关系** — 一等公民 `ProjectMember` 实体，独立于 `WorkspaceMember`。
- **Owner 表达** — 遵循 D-06：Project 存 `OwnerId`（Guid 标量，无跨模块 FK）。创建时创建者自动获得 ProjectMember Admin 角色。
- **角色粒度** — 直接复用 Workspace 角色枚举（Admin=20, Member=15, Guest=5）。复用 `WorkspaceRole` 类型。

#### 灰色区域 3：模块结构与模式

- **模块组织** — 遵循 Workspace 模块模式：独立 `Modules.Project.Contracts` + `Modules.Project`。Features 按 `v1/{Entity}/{Action}/` 组织。
- **DbContext** — 独立 `ProjectDbContext`，`yhschema.Project` 数据库 schema。
- **API 路由** — 嵌套路由 `/api/v1/workspaces/{slug}/projects/`，与 Plane API 契约兼容。

### Claude's Discretion

- Project 实体不实现 `IHasTenant`（通过 workspace slug 解析确定租户上下文）
- `Identifier` 唯一约束范围：workspace 级别 + DB 唯一约束保障
- 封面图片搜索端点（Unsplash）的集成方式：Phase 3 仅做封面 URL 存储
- 项目列表支持筛选（状态/成员/时间）和排序，贴 Plane API 参数格式
- EF Core 实体配置、Contracts DTO 结构、Mediator feature 切分均贴 Phase 2 Workspace 模块模式
- 项目访问控制：只有 workspace Admin/Member 可创建项目；project Member 可编辑项目内容；project Guest/非成员需要 workspace-level 至少 Member 才能看到项目
- `ProjectMember` 配置 `UserId`（Guid 标量）+ `ProjectId`（FK）+ `Role`（复用 WorkspaceRole 枚举）
- `ProjectDbContext` 边界 —— 只含 Project/ProjectMember 等本模块实体
- `Modules.Project` 注册方式：参照 WorkspaceModule.cs 的 `AddWorkspaceModule()` 扩展方法模式
- 测试项目：`Tests/Project.Tests`，遵循 Workspace.Tests 的 InMemory + 可选 Testcontainers 模式

### Deferred Ideas（OUT OF SCOPE）

- Unsplash API 搜索端点的实现位置（Project 端点代理 vs 前端直调 Unsplash API）—— 规划阶段确定
- Issue 引用编号格式（PROJ-123）的编号生成逻辑 —— Phase 4 WorkItems 范围
- 项目归档功能 —— 如果 Plane 有归档（非删除）功能，需后续研究

</user_constraints>

<phase_requirements>

## Phase Requirements

| ID      | Description                                                   | Research Support                                                                                                                                            |
| ------- | ------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| REQ-3.1 | 项目 CRUD（在工作区内创建/获取/更新/删除/列表，支持筛选排序） | §Standard Stack, §Plane API Contract (Project Serializer), §Architecture Patterns (CRUD slice), §Code Examples (Project entity + DbContext + CreateCommand) |
| REQ-3.2 | 项目成员管理（添加/列出/更新角色/移除）                       | §Plane API Contract (ProjectMember), §Permission Model (ProjectBasePermission), §Code Examples (ProjectMember entity + Configuration)                       |
| REQ-3.3 | 项目设置（封面图片、标识符/前缀、描述、可见性）               | §Plane API Contract (ProjectSerializer fields: identifier, cover_image, network, description), §Don't Hand-Roll (Unsplash)                                  |

</phase_requirements>

## Project Constraints (from CLAUDE.md)

- **Namespace 约定**：`YH.Framework.{Name}`（BuildingBlocks）、`YH.Modules.{Name}`（Modules）、`YH.Flow.{Name}`（Host）。新建 `YH.Modules.Project` + `YH.Modules.Project.Contracts`。
- **`TreatWarningsAsErrors`** — 警告即编译失败，所有新代码必须 0 warning。
- **`AGENTS.md` 是规范准则**（10 条黄金法则）—— 规划阶段须遵守。
- **Quick Commands**：`dotnet build src/YH.Flow.slnx`、`dotnet test src/YH.Flow.slnx`、API 端口 7030/5030。
- **Minimal API + Vertical Slice** —— 所有端点用 Minimal API，feature 切分贴 Workspace `Features/v1/{Feature}/{Endpoint.cs, Handler.cs, Validator.cs}` 模式。

## Summary

Phase 3 在 Phase 2 Workspace 模块建立的"workspace 即租户"基座之上，落地 Plane 的项目管理层。项目是 Plane 的核心聚合根，所有后续实体（WorkItems、Cycle、Module、Page、View）都从属于项目。

**核心差异于 Workspace：**

1. **项目是对租户隔离的普通实体**（不像 Workspace 是 `IGlobalEntity`）—— Project 实现 `IHasTenant`，`BaseDbContext` 的自动租户过滤天然生效。项目数据按每个 workspace 自动隔离。
2. **路由嵌套**—— 项目是 `/api/v1/workspaces/{slug}/projects/`，Finbuckle 已在 Phase 2 建好 slug→tenant 解析和 middleware，Phase 3 直接复用 `ICurrentWorkspaceContext` 和 `[RequireWorkspaceRole]`。
3. **Identifier 系统**—— Plane 项目有独立于 slug 的短前缀标识符（如 "PROJ"），用于 Issue 引用编号（如 PROJ-123）。需 workspace 级别唯一约束。
4. **ProjectMember 是独立的实体**（非 WorkspaceMember 的子集）—— 虽然角色值复用 `WorkspaceRole` (20/15/5)，但 ProjectMember 是单独的表，允许 workspace Guest 用户成为项目 Member。

**技术栈：** 100% 复用现有基础设施。0 新增 NuGet 包。所有模式均从 Workspace 模块直接复制适配。

**Primary recommendation：** 严格遵循 Workspace 模块的 Vertical Slice 结构：每个操作一个 Feature 文件夹（Command/Query + Handler + Validator + Endpoint）。`Project` 实体实现 `IHasTenant`（自动租户过滤）、`ISoftDeletable`（软删除 + slug epoch 释放）。`ProjectMember` 实体实现 `IHasTenant`（自动租户过滤）。复用 Workspace 的 `[RequireWorkspaceRole]` 和 `ICurrentWorkspaceContext` 进行项目级授权。

## Architectural Responsibility Map

| Capability                   | Primary Tier                                              | Secondary Tier               | Rationale                                                                         |
| ---------------------------- | --------------------------------------------------------- | ---------------------------- | --------------------------------------------------------------------------------- |
| Project CRUD 业务逻辑        | API / Backend（Mediator handler）                         | Database（ProjectDbContext） | Vertical slice，handler 在 tenant-scoped ProjectDbContext 上操作                  |
| URL slug → TenantId 解析     | API / Middleware（Phase 2 WorkspaceSlugStrategy）         | —                            | Work for Phase 3, zero new code                                                   |
| 成员资格 + 角色填充          | API / Middleware（Phase 2 WorkspaceMembershipMiddleware） | —                            | Work for Phase 3, zero new code — ICurrentWorkspaceContext 已有                   |
| Workspace-role 授权          | API / Authorization（Phase 2 [RequireWorkspaceRole]）     | —                            | Work for Phase 3, zero new code                                                   |
| Project-role 授权            | API / Backend（自定义 ProjectMemberPermission 检查）      | Authorization handler        | 项目级角色需查 ProjectMember 表（不同于 workspace 级的 ICurrentWorkspaceContext） |
| Identifier 验证 + 唯一性     | API / Backend（handler 内 validator）                     | Database（唯一索引兜底）     | Plane 有 FORBIDDEN_IDENTIFIER_CHARS_PATTERN，转译到 FluentValidation + 唯一索引   |
| 软删除（slug epoch 释放）    | Database / EF（handler 显式 SoftDelete）                  | —                            | 贴 D-08 / Workspace 模式：delete 时 slug 追加 `__{epoch}`                         |
| 跨模块用户详情批量解析       | API / Backend（`IUserIdentityService`）                   | Identity.Contracts           | ProjectMember list 需 batch 查用户详情                                            |
| 曝光/可见性（Secret/Public） | Database / EF（Project.network 字段过滤查询）             | —                            | GET 列表时：成员可看全部；非成员只看到 network=Public(2) 的项目                   |
| Cover image                  | API / Entity（Project.CoverImageUrl 可空字符串字段）      | —                            | CONTEXT 决策：无独立实体，直接存 URL                                              |

## Standard Stack

### Core（全部已在项目中，0 新增包）

| Library                              | Version      | Purpose                                              | Why Standard                                             |
| ------------------------------------ | ------------ | ---------------------------------------------------- | -------------------------------------------------------- |
| `Microsoft.EntityFrameworkCore.*`    | .NET 10 内置 | `ProjectDbContext`、迁移、`IEntityTypeConfiguration` | 全项目统一 EF Core                                       |
| `Mediator`（source-gen）             | 现有         | CQRS command/query + handler + validator pipeline    | Workspace 模块已用，贴其 `Features/v1` 结构              |
| `FluentValidation`                   | 现有         | DTO 校验                                             | `AddValidatorsFromAssemblies` 已在 `AddModules` 自动注册 |
| `Finbuckle.MultiTenant.Abstractions` | 10.1.0       | `IMultiTenantContextAccessor`、`IHasTenant`          | Phase 1/2 已建，Project 实现 `IHasTenant` 即可           |
| `YH.Framework.Core.Domain`           | 现有         | `IHasTenant`、`ISoftDeletable`、`IAuditableEntity`   | 基接口                                                   |

### Supporting

| Library                                                       | Version | Purpose                                     | When to Use                                   |
| ------------------------------------------------------------- | ------- | ------------------------------------------- | --------------------------------------------- |
| `YH.Modules.Workspace.Contracts`                              | 现有    | `ICurrentWorkspaceContext`、`WorkspaceRole` | Project 端点读 workspace slug 和当前用户 role |
| `YH.Modules.Identity.Contracts.Services.IUserIdentityService` | 现有    | 批量查用户详情                              | ProjectMember list 端点                       |

### Alternatives Considered

| Instead of                                 | Could Use                            | Tradeoff                                                                                                                     |
| ------------------------------------------ | ------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| 复用 `WorkspaceRole` 枚举                  | 新建 `ProjectRole` 枚举              | Plane 项目成员就是用相同角色值（20/15/5）。复用减少类型数量，与 Workspace 授权模式一致。                                     |
| `Project` 实现 `IHasTenant`                | `Project` 也做 `IGlobalEntity`       | 项目是租户隔离的数据，必须 tenant-filtered。Workspace 才是 `IGlobalEntity`。                                                 |
| Identifier 用独立 `ProjectIdentifier` 实体 | Identifier 只是 Project 上的一个字段 | Plane 虽然有 `ProjectIdentifier` 表，但 CONTEXT 决策说"实体同时包含 Identifier 和 Slug"，不用额外实体。字段 + 唯一约束足够。 |

**Installation：**

```bash
# 无需安装任何新包 —— 全部依赖已在项目中
dotnet restore src/YH.Flow.slnx
```

## Package Legitimacy Audit

> Phase 3 **不安装任何新外部包**。所有依赖均复用 Phase 1/2 已在项目中的包。

| Package    | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
| ---------- | -------- | --- | --------- | ----------- | --------- | ----------- |
| （无新增） | —        | —   | —         | —           | N/A       | N/A         |

**Packages removed due to slopcheck [SLOP] verdict:** none
**Packages flagged as suspicious [SUS]:** none

_Phase 3 纯代码 + 配置变更，复用 Phase 1/2 已锁定的全部依赖，无需 planner 插入 `checkpoint:human-verify`。_

## Architecture Patterns

### System Architecture Diagram

```
HTTP Request: /api/v1/workspaces/{slug}/projects/{projectId}/...
   │
   ▼
[ASP.NET Core Pipeline]  (Phase 1 - already configured)
   │
   ├─ UseMultiTenant() / WorkspaceSlugStrategy  (Phase 2 - already wired)
   │   → Resolves {slug} → TenantId = workspaceGuid
   │
   ├─ UseAuthentication()  (Phase 1 - JWT/API Key/Session Cookie)
   │
   ├─ WorkspaceMembershipMiddleware  (Phase 2 - already wired)
   │   → Fills ICurrentWorkspaceContext (CurrentWorkspaceId/Slug/CurrentUserRole)
   │
   ├─ UseAuthorization()
   │   → [RequireWorkspaceRole] for workspace-level checks
   │   → (Project-specific authz done in handler or custom filter)
   │
   └─ Project Endpoint Handler (Mediator command/query)
        │
        ├─ REQ-3.1: Project CRUD (tenant-scoped ProjectDbContext)
        │   ├─ CreateProject → [RequireWorkspaceRole(Admin|Member)] + auto ProjectMember(Admin)
        │   ├─ GetProject → tenant-filtered + network=2 check for non-members
        │   ├─ UpdateProject → [RequireWorkspaceRole(Admin|Member)] + project member check
        │   ├─ DeleteProject → [RequireWorkspaceRole(Admin)] + soft delete + slug epoch
        │   └─ ListProjects → tenant-filtered + is_member annotation + network filter
        │
        ├─ REQ-3.2: ProjectMember CRUD
        │   ├─ Add Member → [RequireProjectRole(Admin)] (or workspace Admin)
        │   ├─ List Members → tenant-filtered + IUserIdentityService batch resolve
        │   ├─ Update Role → [RequireProjectRole(Admin)]
        │   └─ Remove Member → [RequireProjectRole(Admin)] + is_active=false
        │
        └─ REQ-3.3: Project Settings (same as UpdateProject)
            ├─ CoverImage → Project.CoverImageUrl field
            ├─ Identifier → validated + unique within workspace
            ├─ Description → text field
            └─ Network/Visibility → Project.Network enum
```

### Recommended Project Structure（贴 Workspace 模块模式 [VERIFIED]）

```
yh-flow/src/Modules/Project/
├── Modules.Project/
│   ├── ProjectModule.cs                  # IModule 实现
│   ├── ProjectModuleConstants.cs          # SchemaName = "yhschema.Project"
│   ├── AssemblyInfo.cs                    # [FshModule(typeof(ProjectModule), 400)]
│   ├── Domain/
│   │   ├── Project.cs                     # IHasTenant, ISoftDeletable, IAuditableEntity
│   │   └── ProjectMember.cs               # IHasTenant, ISoftDeletable, IAuditableEntity
│   ├── Data/
│   │   ├── ProjectDbContext.cs            # 派生 BaseDbContext（贴 WorkspaceDbContext 模式）
│   │   └── Configurations/
│   │       ├── ProjectConfiguration.cs
│   │       └── ProjectMemberConfiguration.cs
│   ├── Features/v1/
│   │   ├── Projects/
│   │   │   ├── CreateProject/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── GetProject/{Query, Handler, Endpoint}.cs
│   │   │   ├── UpdateProject/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── DeleteProject/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── ListProjects/{Query, Handler, Validator, Endpoint}.cs
│   │   │   └── ArchiveProject/{Command, Handler, Endpoint}.cs  [optional]
│   │   └── Members/
│   │       ├── AddMember/{Command, Handler, Validator, Endpoint}.cs
│   │       ├── ListMembers/{Query, Handler, Endpoint}.cs
│   │       ├── UpdateMemberRole/{Command, Handler, Validator, Endpoint}.cs
│   │       └── RemoveMember/{Command, Handler, Endpoint}.cs
│   └── Services/
│       ├── IProjectIdentifierService.cs / ProjectIdentifierService.cs  [optional: identifier gen]
│       └── ProjectMemberService.cs        [optional: member CRUD service]
└── Modules.Project.Contracts/
    ├── DTOs/
    │   ├── ProjectDto.cs
    │   ├── ProjectMemberDto.cs
    │   └── ProjectListDto.cs
    ├── Constants/
    │   └── ProjectConstants.cs             # forbidden chars, defaults
    └── v1/Projects/
        ├── CreateProject/CreateProjectCommand.cs (+Response)
        ├── GetProject/GetProjectQuery.cs
        ├── UpdateProject/UpdateProjectCommand.cs
        ├── DeleteProject/DeleteProjectCommand.cs
        ├── ListProjects/ListProjectsQuery.cs
        └── ArchiveProject/{ArchiveProjectCommand, UnarchiveProjectCommand}.cs
```

### Pattern 1: `Project` 实体（贴 Plane Project.py 字段 + Workspace 实体模式）

**What:** Project 是租户隔离的普通实体（`IHasTenant`，非 `IGlobalEntity`），自动受 `BaseDbContext` 的 tenant filter 保护。支持软删除（D-08）。

**Why:** 项目创建后属于某个 workspace，所有子实体（WorkItems/Cycle）继承此隔离。

**Plane 字段对照：**

| Plane 字段                 | .NET 字段               | 类型                  | 备注                                               |
| -------------------------- | ----------------------- | --------------------- | -------------------------------------------------- |
| `name`                     | `Name`                  | string(255)           | 必填                                               |
| `description`              | `Description`           | string?               | 文本描述                                           |
| `description_text`         | `DescriptionText`       | string?               | JSON 富文本（Phase 3 可选）                        |
| `description_html`         | `DescriptionHtml`       | string?               | HTML 富文本（Phase 3 可选）                        |
| `network`                  | `Network`               | `ProjectNetwork` enum | Secret=0, Public=2                                 |
| `identifier`               | `Identifier`            | string(12)            | 大写短前缀，如 "PROJ"                              |
| `slug`                     | `Slug`                  | string                | 从 name 生成的 URL slug，软删除时 `__{epoch}` 释放 |
| `project_lead`             | `ProjectLeadId`         | Guid?                 | 标量，无 FK                                        |
| `default_assignee`         | `DefaultAssigneeId`     | Guid?                 | 标量，无 FK                                        |
| `emoji`                    | `Emoji`                 | string?               | Emoji 图标                                         |
| `icon_prop`                | `IconProp`              | string?               | JSON 图标配置                                      |
| `cover_image`              | `CoverImageUrl`         | string?               | 可空 URL 字符串                                    |
| `module_view`              | `ModuleViewEnabled`     | bool                  | 默认 false                                         |
| `cycle_view`               | `CycleViewEnabled`      | bool                  | 默认 false                                         |
| `issue_views_view`         | `IssueViewsViewEnabled` | bool                  | 默认 false                                         |
| `page_view`                | `PageViewEnabled`       | bool                  | 默认 true                                          |
| `intake_view`              | `IntakeViewEnabled`     | bool                  | 默认 false                                         |
| `is_time_tracking_enabled` | `IsTimeTrackingEnabled` | bool                  | 默认 false                                         |
| `is_issue_type_enabled`    | `IsIssueTypeEnabled`    | bool                  | 默认 false                                         |
| `guest_view_all_features`  | `GuestViewAllFeatures`  | bool                  | 默认 false                                         |
| `archive_in`               | `ArchiveIn`             | int                   | 自动归档月数                                       |
| `close_in`                 | `CloseIn`               | int                   | 自动关闭月数                                       |
| `logo_props`               | `LogoProps`             | string?               | JSON                                               |
| `timezone`                 | `TimeZone`              | string                | 默认 "UTC"                                         |
| `estimated`                | `EstimateId`            | Guid?                 | Phase 4 才可用                                     |
| `archived_at`              | `ArchivedAt`            | DateTimeOffset?       | 归档时间                                           |
| `external_source`          | `ExternalSource`        | string?               | 导入来源                                           |
| `external_id`              | `ExternalId`            | string?               | 导入外部 ID                                        |

**关键：** `Project` 实体**必须**实现 `IHasTenant`（自动租户过滤），**不要**标 `IGlobalEntity`。

**Example：**

```csharp
// Source: 贴 Workspace.Domain.Workspace.cs 模式 + Plane project.py:68-177
public sealed class Project : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public ProjectNetwork Network { get; private set; } = ProjectNetwork.Public;
    public string Identifier { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public Guid OwnerId { get; private set; }  // D-06 scalar
    public Guid? ProjectLeadId { get; private set; }
    public Guid? DefaultAssigneeId { get; private set; }
    public string? Emoji { get; private set; }
    public string? IconProp { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public bool ModuleViewEnabled { get; private set; }
    public bool CycleViewEnabled { get; private set; }
    public bool IssueViewsViewEnabled { get; private set; }
    public bool PageViewEnabled { get; private set; } = true;
    public bool IntakeViewEnabled { get; private set; }
    public bool IsTimeTrackingEnabled { get; private set; }
    public bool IsIssueTypeEnabled { get; private set; }
    public bool GuestViewAllFeatures { get; private set; }
    public int ArchiveIn { get; private set; }
    public int CloseIn { get; private set; }
    public string? LogoProps { get; private set; }
    public string TimeZone { get; private set; } = "UTC";
    public DateTimeOffset? ArchivedAt { get; private set; }
    public string? ExternalSource { get; private set; }
    public string? ExternalId { get; private set; }

    // IHasTenant
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Project() { } // EF Core

    public static Project Create(
        string name,
        string identifier,
        string slug,
        Guid ownerUserId,
        Guid? projectLeadId = null,
        Guid? defaultAssigneeId = null,
        ProjectNetwork network = ProjectNetwork.Public,
        string? description = null,
        string? emoji = null,
        string? iconProp = null,
        string? coverImageUrl = null,
        string? timeZone = null,
        // ... optional boolean defaults
    ) { /* factory implementation */ }

    public void Update(/* mutable fields */)
    {
        // Update non-key mutable fields
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(DateTimeOffset now)  // D-08 pattern
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
        Slug = $"{Slug}__{(int)now.ToUnixTimeSeconds()}";  // release slug
    }

    public void Archive(DateTimeOffset now)
    {
        ArchivedAt = now;
        LastModifiedOnUtc = now;
    }

    public void Unarchive()
    {
        ArchivedAt = null;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}

public enum ProjectNetwork
{
    Secret = 0,
    Public = 2,
}
```

### Pattern 2: `ProjectMember` 实体（贴 Plane ProjectMember.py 模式）

**What:** 一等公民实体，独立于 `WorkspaceMember`。允许 workspace Guest 成为项目 Member。

**Key fields（Plane project.py:210-262）：**

| Plane 字段      | .NET 字段   | 类型                 | 备注               |
| --------------- | ----------- | -------------------- | ------------------ |
| `member`        | `UserId`    | Guid                 | 标量，无 FK (D-04) |
| `role`          | `Role`      | int（WorkspaceRole） | 复用 20/15/5       |
| `is_active`     | `IsActive`  | bool                 | 默认 true          |
| `sort_order`    | `SortOrder` | float                | 排序               |
| `view_props`    | —           | (暂不实现)           | Phase 8 View       |
| `default_props` | —           | (暂不实现)           | Phase 8 View       |
| `preferences`   | —           | (暂不实现)           | Phase 8 View       |

**Example：**

```csharp
// Source: 贴 Workspace.Domain.WorkspaceMember.cs 模式 + Plane project.py:210-262
public sealed class ProjectMember : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }  // FK-like scalar to Project
    public Guid UserId { get; private set; }      // scalar, no FK (D-04)
    public int Role { get; private set; }          // WorkspaceRole values: 5/15/20
    public bool IsActive { get; private set; } = true;
    public double SortOrder { get; private set; } = 65535;
    public string TenantId { get; private set; } = default!;

    // IAuditableEntity, ISoftDeletable, DomainEvents...

    private ProjectMember() { } // EF Core

    public static ProjectMember Create(Guid projectId, Guid userId, int role, bool isActive = true) { }

    public void UpdateRole(int role) { Role = role; LastModifiedOnUtc = DateTimeOffset.UtcNow; }
    public void Deactivate() { IsActive = false; LastModifiedOnUtc = DateTimeOffset.UtcNow; }
    public void Activate() { IsActive = true; LastModifiedOnUtc = DateTimeOffset.UtcNow; }
}
```

### Pattern 3: `ProjectDbContext`（贴 WorkspaceDbContext 模式）

**What:** 派生 `BaseDbContext`，schema = `yhschema.Project`。Project 实现 `IHasTenant`，自动受租户过滤。

**关键：** 与 WorkspaceDbContext 不同，ProjectDbContext 里的实体全部是 `IHasTenant`（没有 `IGlobalEntity` 实体）。

**Example：**

```csharp
// Source: 贴 WorkspaceDbContext.cs 模式
public sealed class ProjectDbContext : BaseDbContext
{
    public ProjectDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<ProjectDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> Members => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        // 1) ApplyConfigurationsFromAssembly FIRST
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
        // 2) base.OnModelCreating LAST → auto IsMultiTenant() + soft-delete filter
        base.OnModelCreating(modelBuilder);
    }
}
```

### Pattern 4: EF Core Configuration（贴 WorkspaceConfiguration 模式）

**ProjectConfiguration 关键点：**

- `Project` 是 `IHasTenant`，所以**不写** `.IsMultiTenant()`（由 `ApplyTenantIsolationByDefault()` 自动处理）
- Identifier 唯一约束：`(TenantId, Identifier)` — workspace 级别唯一
- Name 唯一约束：`(TenantId, Name)` — workspace 级别唯一
- Slug 唯一索引：普通唯一（软删除时 `__{epoch}` 释放）

```csharp
// Source: 贴 WorkspaceConfiguration.cs + Plane project.py Meta
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", ProjectModuleConstants.SchemaName).HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Identifier).IsRequired().HasMaxLength(12);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(100);

        // Identifier unique within workspace (TenantId auto-added by IsMultiTenant)
        builder.HasIndex(x => new { x.TenantId, x.Identifier })
            .IsUnique()
            .HasDatabaseName("IX_Projects_Tenant_Identifier");

        // Name unique within workspace
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_Projects_Tenant_Name");

        builder.Property(x => x.Network).HasConversion<int>().HasDefaultValue(ProjectNetwork.Public);
        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(64).HasDefaultValue("UTC");
        // ... other fields
    }
}
```

**ProjectMemberConfiguration 关键点：**

```csharp
public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers", ProjectModuleConstants.SchemaName).HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();  // Guid scalar, no FK (D-04)
        builder.Property(x => x.Role).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        // One membership per user per project (TenantId auto-widened)
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_ProjectMembers_Tenant_Project_User");

        builder.HasIndex(x => new { x.ProjectId, x.IsActive })
            .HasDatabaseName("IX_ProjectMembers_Project_Active");
    }
}
```

### Pattern 5: Project API 端点路由（贴 Plane URL 契约）

**Plane 项目路由结构（`api/urls/project.py` + `app/urls/project.py`）：**

```
# API（主要端点集）
GET    /api/v1/workspaces/{slug}/projects/          → ListProjects
POST   /api/v1/workspaces/{slug}/projects/          → CreateProject
GET    /api/v1/workspaces/{slug}/projects/{pk}/     → GetProject
PATCH  /api/v1/workspaces/{slug}/projects/{pk}/     → UpdateProject
DELETE /api/v1/workspaces/{slug}/projects/{pk}/     → DeleteProject

# App（额外端点集——Phase 3 按需实现）
GET    /api/v1/workspaces/{slug}/projects/{pk}/archive/     → 归档
DELETE /api/v1/workspaces/{slug}/projects/{pk}/archive/     → 取消归档
GET    /api/v1/workspaces/{slug}/projects/{pk}/members/     → ListProjectMembers
POST   /api/v1/workspaces/{slug}/projects/{pk}/members/     → AddProjectMember
GET    /api/v1/workspaces/{slug}/projects/{pk}/members/{mid}/   → GetProjectMember
PATCH  /api/v1/workspaces/{slug}/projects/{pk}/members/{mid}/   → UpdateProjectMember
DELETE /api/v1/workspaces/{slug}/projects/{pk}/members/{mid}/   → RemoveProjectMember
```

**ProjectModule.cs 中注册路由：**

```csharp
// Source: 贴 WorkspaceModule.cs MapEndpoints 模式
public void MapEndpoints(IEndpointRouteBuilder endpoints)
{
    var apiVersionSet = endpoints.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

    // All project routes are scoped under workspaces/{slug}/
    var projects = endpoints
        .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects")
        .WithTags("Projects")
        .WithApiVersionSet(apiVersionSet);

    projects.MapCreateProjectEndpoint();
    projects.MapListProjectsEndpoint();
    projects.MapGetProjectEndpoint();
    projects.MapUpdateProjectEndpoint();
    projects.MapDeleteProjectEndpoint();

    // Project member routes under projects/{projectId}/members/
    var members = endpoints
        .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/members")
        .WithTags("ProjectMembers")
        .WithApiVersionSet(apiVersionSet);

    members.MapAddMemberEndpoint();
    members.MapListMembersEndpoint();
    members.MapUpdateMemberRoleEndpoint();
    members.MapRemoveMemberEndpoint();
}
```

> **注意：** Plane 的 API 用 `pk`（primary key），app 用 `project_id`。Phase 3 统一用 `projectId` 作为路由参数名（贴 .NET 惯例）。

### Pattern 6: 权限模型（Plane `ProjectBasePermission` 翻译）

**Plane 权限规则（`app/permissions/project.py`）：**

| 方法                            | 所需权限                                              | .NET 映射                                                                        |
| ------------------------------- | ----------------------------------------------------- | -------------------------------------------------------------------------------- |
| SAFE_METHODS (GET/HEAD/OPTIONS) | workspace-member                                      | `[RequireWorkspaceRole(Member)]` — 任何人看到所有项目（network filter 在查询层） |
| POST (创建项目)                 | workspace Admin/Member                                | `[RequireWorkspaceRole(Admin, Member)]`                                          |
| PATCH/PUT/DELETE                | project-admin OR (project-member AND workspace-admin) | handler 内查 ProjectMember role                                                  |

**Phase 3 推荐实现策略：**

1. **创建项目**（POST）：用 Phase 2 的 `[RequireWorkspaceRole(Admin, Member)]` — workspace 级别角色检查，不需要项目存在
2. **列表/读取**（GET）：用 Phase 2 的 `[RequireWorkspaceRole(Member, Guest)]` — 任何 workspace 成员可见；非成员只能看到 network=Public 的项目（在查询过滤）
3. **更新/删除**（PATCH/DELETE）：handler 内查 `ProjectMember` 表检查 role
   - Project Admin（role=20）可以更新/删除
   - Project Member（role=15）可以更新内容但不能改设置
   - Workspace Admin 即使不是 Project Admin 也可以管理项目（Plane 行为）

> **建议：** 不要为 Phase 3 创建全新的 `[RequireProjectRole]` attribute。首次可以用 handler 内查 `ProjectMember` 表的方式。如果后续 Phase 4+ 多个模块都需要项目级权限，再提取为共享 attribute。

### Anti-Patterns to Avoid

- **Project 实体忘记实现 `IHasTenant`**：如果忘了实现，项目数据在所有 workspace 间泄露 — 无 tenant filter。**必须实现 `IHasTenant`**。
- **把 Identifier 当主键**：Project 仍然是 Guid Id，Identifier 是用户可变的业务标识符。不要用 Identifier 做 FK 引用。
- **软删除时不改 slug**：导致原 slug 永远无法复用。**必须遵循 D-08**：`Slug = $"{Slug}__{epoch}"`。
- **创建项目时不自动创建 ProjectMember**：创建者必须是 Admin 成员（Plane `ProjectListCreateAPIEndpoint.post()`:230 创建 `ProjectMember(role=20)`）。**必须自动插入**。
- **项目列表不做 network 过滤**：非成员看不到 Secret 项目。Plane 的 queryset 有 `Q(network=2)` OR `Q(is_member=True)`。**必须实现**。

## Don't Hand-Roll

| Problem             | Don't Build                          | Use Instead                                                        | Why                          |
| ------------------- | ------------------------------------ | ------------------------------------------------------------------ | ---------------------------- |
| 多租户 query filter | 自己写 `WHERE WorkspaceId = current` | `BaseDbContext` + `IHasTenant` + `ApplyTenantIsolationByDefault()` | 已实现、默认开启             |
| 软删除 filter       | 手动 `if (!deleted)`                 | `ISoftDeletable` + `AppendGlobalQueryFilter`                       | `BaseDbContext` 已挂         |
| 审计字段            | 手动赋值                             | `AuditableEntitySaveChangesInterceptor`                            | 已注册                       |
| 分页（Plane 格式）  | 自己拼 response                      | `PlanePagedResult<T>` + `IPagedQuery`                              | Phase 1 已适配               |
| 校验 pipeline       | 手动 if/throw                        | `FluentValidation` + `ValidationBehavior` pipeline                 | `AddModules` 自动注册        |
| 当前用户上下文      | 从 ClaimsPrincipal 手动取            | `ICurrentUser`（Phase 1）                                          | `CurrentUserMiddleware` 已填 |
| Workspace Slug 解析 | 自己查 workspace                     | Phase 2 `WorkspaceSlugStrategy` + `ICurrentWorkspaceContext`       | 已建好直接复用               |
| Unsplash 封面搜索   | 自己实现代理端点                     | 前端直接调 Unsplash API / 延后到 Phase 9                           | CONTEXT `<deferred>` 决策    |

**Key insight：** Phase 3 的工作量主要是"贴 Workspace 模块模式 + 映射 Plane 项目字段"。所有基础设施（多租户、软删除、审计、分页、校验）Phase 1/2 已建好。Plane 的数据模型直译到 EF Core 实体 + FluentValidation + Minimal API 即可。

## Runtime State Inventory

> Phase 3 是**新建模块（greenfield within existing solution）**，不涉及 rename/refactor/migration of existing strings。无运行时状态需要迁移。**N/A — verified by absence of rename/refactor scope in CONTEXT.md `<domain>` and `<deferred>`。**

## Common Pitfalls

### Pitfall 1: Project 实体被误标 `IGlobalEntity`

**What goes wrong:** Project 数据在所有 workspace 间泄露。Workspace A 的用户能查到 Workspace B 的项目。
**Why it happens:** 把 Project 类比 Workspace 也标了 `IGlobalEntity`（Workspace 是 `IGlobalEntity` 因为它 IS 租户）。
**How to avoid:** Project 是租户隔离的数据**消费者**，不是定义者。必须实现 `IHasTenant`，不要标 `IGlobalEntity`。
**Warning signs:** 跨 workspace 的 list query 返回了错误的数据。

### Pitfall 2: Identifier 未做规范性校验

**What goes wrong:** 用户输入 `&+,:;$^*}{=?@#|'<>.()%!-` 等特殊字符（Plane 禁止字符集），导致 Issue 引用格式混乱。
**Why it happens:** 没实现 Plane 的 `FORBIDDEN_IDENTIFIER_CHARS_PATTERN`。
**How to avoid:** FluentValidation 中校验 identifier 格式：只允许大写字母和数字，长度 < 12。
**Warning signs:** Identifier 包含特殊字符。

### Pitfall 3: 软删除后 identifier 未释放

**What goes wrong:** 删除 Project（identifier="PROJ"），想新建另一个项目用 "PROJ" 失败。
**Why it happens:** 没有在软删除时释放 identifier，DB 唯一约束阻止复用。
**How to avoid:** Plane 的约束是 `unique_together = ["identifier", "workspace", "deleted_at"]` — 软删除时 `deleted_at` 不为 null 即可释放。EF Core 用条件唯一索引实现：`HasFilter("[DeletedAt] IS NULL")`。**不需要修改 identifier 值**（slug 才需要改，因为 slug 是 URL 段）。
**Warning signs:** 软删除后无法用原 identifier 创建新项目。

### Pitfall 4: 项目列表不区分公开/私有

**What goes wrong:** 非 workspace-member 看不到任何项目（包括 Public 项目）；或非成员能看到所有项目（包括 Secret 项目）。
**Why it happens:** 没实现 Plane 的 `Q(network=2) | Q(is_member=True)` 过滤逻辑。
**How to avoid:** 查询时：成员用户看到所有项目（tenant-filtered）；非成员用户只看到 `network=2`（Public）的项目。用 `is_member` annotation 或单独的 filter。
**Warning signs:** 公开分享的项目无法被非成员访问。

### Pitfall 5: 创建项目没自动建 ProjectMember

**What goes wrong:** 创建者看不到自己刚创建的项目列表（因为 ProjectMember 表没有他/她的行）。
**Why it happens:** 忘记在 CreateProject handler 结束时插入 `ProjectMember(role=Admin)`。
**How to avoid:** 在 Create 后的同一个 SaveChanges 中插入 `ProjectMember.Create(projectId, ownerUserId, role=Admin)`。
**Warning signs:** 项目创建成功但创建者无法立即在列表中看到它（如果列表 filter 要求 is_active=true 的 ProjectMember）。

## Code Examples

### Example 1: Project 实体工厂（贴 Plane project.py 字段）

```csharp
// Source: 贴 Workspace.Domain.Workspace.Create() + Plane project.py:68-177
public static Project Create(
    string name,
    string identifier,
    string slug,
    Guid ownerUserId,
    Guid? projectLeadId = null,
    Guid? defaultAssigneeId = null,
    ProjectNetwork network = ProjectNetwork.Public,
    string? description = null,
    string? emoji = null,
    string? iconProp = null,
    string? coverImageUrl = null,
    string? timeZone = null,
    bool moduleViewEnabled = false,
    bool cycleViewEnabled = false,
    bool issueViewsViewEnabled = false,
    bool pageViewEnabled = true,
    bool intakeViewEnabled = false,
    bool guestViewAllFeatures = false,
    bool isTimeTrackingEnabled = false,
    bool isIssueTypeEnabled = false,
    int archiveIn = 0,
    int closeIn = 0,
    string? logoProps = null)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Project name is required.", nameof(name));
    if (string.IsNullOrWhiteSpace(identifier))
        throw new ArgumentException("Project identifier is required.", nameof(identifier));
    if (string.IsNullOrWhiteSpace(slug))
        throw new ArgumentException("Project slug is required.", nameof(slug));
    if (ownerUserId == Guid.Empty)
        throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));

    return new Project
    {
        Id = Guid.NewGuid(),
        Name = name,
        Identifier = identifier.ToUpperInvariant(),
        Slug = slug,
        OwnerId = ownerUserId,
        ProjectLeadId = projectLeadId,
        DefaultAssigneeId = defaultAssigneeId,
        Network = network,
        Description = description,
        Emoji = emoji,
        IconProp = iconProp,
        CoverImageUrl = coverImageUrl,
        ModuleViewEnabled = moduleViewEnabled,
        CycleViewEnabled = cycleViewEnabled,
        IssueViewsViewEnabled = issueViewsViewEnabled,
        PageViewEnabled = pageViewEnabled,
        IntakeViewEnabled = intakeViewEnabled,
        GuestViewAllFeatures = guestViewAllFeatures,
        IsTimeTrackingEnabled = isTimeTrackingEnabled,
        IsIssueTypeEnabled = isIssueTypeEnabled,
        ArchiveIn = archiveIn,
        CloseIn = closeIn,
        LogoProps = logoProps,
        TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "UTC" : timeZone,
        CreatedOnUtc = DateTimeOffset.UtcNow,
    };
}
```

### Example 2: Project DTO（Plane `ProjectSerializer` 响应字段）

```csharp
// Source: Plane api/serializers/project.py ProjectSerializer 字段
public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Network { get; set; }  // 0=Secret, 2=Public
    public string Identifier { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public Guid OwnerId { get; set; }
    public Guid? ProjectLeadId { get; set; }
    public Guid? DefaultAssigneeId { get; set; }
    public string? Emoji { get; set; }
    public string? IconProp { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? LogoProps { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public bool ModuleViewEnabled { get; set; }
    public bool CycleViewEnabled { get; set; }
    public bool IssueViewsViewEnabled { get; set; }
    public bool PageViewEnabled { get; set; }
    public bool IntakeViewEnabled { get; set; }
    public bool GuestViewAllFeatures { get; set; }
    public bool IsTimeTrackingEnabled { get; set; }
    public bool IsIssueTypeEnabled { get; set; }
    public int ArchiveIn { get; set; }
    public int CloseIn { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }

    // Plane list 端点附加字段
    public int TotalMembers { get; set; }
    public int TotalCycles { get; set; }
    public int TotalModules { get; set; }
    public bool IsMember { get; set; }
    public int? MemberRole { get; set; }
    public double? SortOrder { get; set; }

    // Audit
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
```

### Example 3: 项目列表查询（Plane `is_member` + `network` 过滤）

```csharp
// Source: Plane api/views/project.py ProjectListCreateAPIEndpoint.get_queryset() 模式
public async ValueTask<IResult> Handle(ListProjectsQuery query, CancellationToken ct)
{
    var projects = await _db.Projects
        .AsNoTracking()
        // Plane: member sees all; non-member sees only Public(2)
        .Where(p => p.Network == ProjectNetwork.Public
            || _db.Members.Any(m => m.ProjectId == p.Id && m.UserId == query.CurrentUserId && m.IsActive))
        .Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Identifier = p.Identifier,
            Slug = p.Slug,
            Network = (int)p.Network,
            IsMember = _db.Members.Any(m => m.ProjectId == p.Id && m.UserId == query.CurrentUserId && m.IsActive),
            MemberRole = _db.Members
                .Where(m => m.ProjectId == p.Id && m.UserId == query.CurrentUserId && m.IsActive)
                .Select(m => (int?)m.Role)
                .FirstOrDefault(),
            CoverImageUrl = p.CoverImageUrl,
            // ...
        })
        .OrderBy(p => p.Name)
        .ToListAsync(ct);

    return TypedResults.Ok(new { count = projects.Count, next = (string?)null, previous = (string?)null, results = projects });
}
```

### Example 4: CreateProject Handler + Auto Admin ProjectMember

```csharp
// Source: 贴 Workspace CreateWorkspaceCommandHandler 模式 + Plane api/views/project.py post():226-231
public sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly ProjectDbContext _db;
    private readonly ICurrentWorkspaceContext _workspaceContext;

    public CreateProjectCommandHandler(ProjectDbContext db, ICurrentWorkspaceContext workspaceContext)
    {
        _db = db;
        _workspaceContext = workspaceContext;
    }

    public async ValueTask<CreateProjectResponse> Handle(CreateProjectCommand command, CancellationToken ct)
    {
        // Generate slug from name (reuse/simplify slug generator logic)
        var slug = Slugify(command.Name);

        // Check identifier uniqueness within workspace (tenant-scoped by DbContext)
        if (await _db.Projects.AnyAsync(p => p.Identifier == command.Identifier, ct))
            throw new CustomException("Project identifier is already taken.",
                HttpStatusCode.Conflict);

        var project = Project.Create(
            name: command.Name,
            identifier: command.Identifier,
            slug: slug,
            ownerUserId: command.OwnerUserId,
            // ... other fields from command
        );

        _db.Projects.Add(project);

        // D-06: Auto-enrol creator as Admin ProjectMember
        // (Plane: _ = ProjectMember.objects.create(project_id=..., member=request.user, role=20))
        var ownerMember = ProjectMember.Create(
            projectId: project.Id,
            userId: command.OwnerUserId,
            role: (int)WorkspaceRole.Admin,
            isActive: true);
        _db.Members.Add(ownerMember);

        // If project_lead is different from owner, add them as Admin too (Plane behavior)
        if (command.ProjectLeadId.HasValue && command.ProjectLeadId.Value != command.OwnerUserId)
        {
            var leadMember = ProjectMember.Create(
                projectId: project.Id,
                userId: command.ProjectLeadId.Value,
                role: (int)WorkspaceRole.Admin,
                isActive: true);
            _db.Members.Add(leadMember);
        }

        await _db.SaveChangesAsync(ct);

        return new CreateProjectResponse(project.Id, project.Slug);
    }
}
```

### Example 5: Identifier 格式验证（Plane `FORBIDDEN_IDENTIFIER_CHARS_PATTERN`）

```csharp
// Source: Plane project.py:143 — FORBIDDEN_IDENTIFIER_CHARS_PATTERN
// Plane regex: r"^.*[&+,:;$^*}{=?@#|'<>.()%!-].*$"
private static readonly Regex ForbiddenCharsPattern =
    new(@"[&+,:;$^*}{=?@#|'<>.()%!\-]", RegexOptions.Compiled);

// Validator:
public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(255)
            .Must(name => !ForbiddenCharsPattern.IsMatch(name))
            .WithMessage("Project name contains forbidden characters.");

        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Project identifier is required.")
            .MaximumLength(12)
            .Must(id => !ForbiddenCharsPattern.IsMatch(id))
            .WithMessage("Project identifier contains forbidden characters.")
            .Must(id => id.All(char.IsAsciiLetterOrDigit))
            .WithMessage("Project identifier must be alphanumeric.")
            .Must(id => id == id.ToUpperInvariant())
            .WithMessage("Project identifier must be uppercase.");

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .When(x => x.Description is not null);
    }
}
```

## State of the Art

| Old Approach                                                    | Current Approach                                        | When Changed | Impact                      |
| --------------------------------------------------------------- | ------------------------------------------------------- | ------------ | --------------------------- |
| Plane Django `BaseModel` 自动 `deleted_at`                      | EF Core `ISoftDeletable` + 全局 filter                  | Phase 1/2    | 软删除模式已建              |
| Plane Django `unique_together` 包括 `deleted_at` with condition | EF Core `HasFilter("[DeletedAt] IS NULL")` 条件唯一索引 | Phase 2      | 适配 Plane 的软删除唯一约束 |

**Deprecated/outdated：**

- 不要在 Project entity Configuration 里显式调 `IsMultiTenant()` — `ApplyTenantIsolationByDefault()` 已自动处理。显式调用反而可能与 auto-default 冲突。

## Assumptions Log

| #   | Claim                                                                                    | Section                   | Risk if Wrong                                                                                                         |
| --- | ---------------------------------------------------------------------------------------- | ------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| A1  | Plane 项目创建时自动插入 `ProjectMember(role=20)` + 如果 `project_lead` 不同也插入 Admin | Pattern 5, Code Example 4 | 如果 Plane 未来变更为只插入 owner，我们的行为会超 Plane 行为（但更安全）                                              |
| A2  | Plane 的 `is_member` annotation 用于列表过滤（成员全见，非成员只见 Public）              | Code Example 3            | 列表可见性逻辑有误，但可以通过测试验证                                                                                |
| A3  | Identifier 只在大写字母+数字范围，不存储小写                                             | Code Example 5            | Plane `save()` 方法 `self.identifier.strip().upper()` 确保大写，我们的 validator + handler 也做 `.ToUpperInvariant()` |

## Open Questions

1. **Unsplash 封面搜索端点的集成方式？**
   - What we know: CONTEXT `<deferred>` 说"规划阶段确定"。Phase 3 只做封面 URL 存储。
   - 方案 A：Project 端点代理 Unsplash API（后端代理，Phase 9 实现）
   - 方案 B：前端直调 Unsplash API（不需要后端）
   - 方案 C：Phase 3 不做搜索，前端从 Unsplash 获取 URL 后直接 POST 到 Project 更新端点
   - Recommendation：**方案 C** — Phase 3 只做 `PUT /projects/{id}/` with `cover_image_url` 字段。搜索功能延后。

2. **项目列表是否支持 `order_by` / `fields` / `expand` 参数？**
   - What we know: Plane 项目列表 API 有多种参数支持（`CURSOR_PARAMETER`, `ORDER_BY_PARAMETER`, `FIELDS_PARAMETER`, `EXPAND_PARAMETER`）
   - Phase 2 的 Workspace 模块也实现了类似模式
   - Recommendation：Phase 3 支持基本的 `order_by` 和分页参数。`fields`/`expand` 延后。

## Environment Availability

| Dependency       | Required By             | Available             | Version      | Fallback |
| ---------------- | ----------------------- | --------------------- | ------------ | -------- |
| PostgreSQL       | ProjectDbContext 持久化 | ✓（Phase 1/2 已验证） | 15+          | —        |
| .NET 10 SDK      | 编译                    | ✓（Phase 1/2 已验证） | 10.x         | —        |
| Finbuckle 10.1.x | 多租户                  | ✓（Phase 1/2 已验证） | 10.1.0       | —        |
| EF Core          | ProjectDbContext        | ✓                     | .NET 10 内置 | —        |

**Missing dependencies with no fallback:** none
**Missing dependencies with fallback:** none — Phase 3 完全在 Phase 1/2 已验证的环境内。

## Validation Architecture

> `workflow.nyquist_validation` 在 `.planning/config.json` 中未显式设为 false ? 默认**启用**。本节必填。

### Test Framework

| Property           | Value                                                                             |
| ------------------ | --------------------------------------------------------------------------------- |
| Framework          | xUnit + FluentAssertions + NSubstitute（贴 Workspace.Tests 模式）                 |
| Config file        | `Directory.Packages.props` 锁定版本；`Tests/Project.Tests`                        |
| Quick run command  | `dotnet test src/Tests/Project.Tests --filter "FullyQualifiedName~Unit" --nologo` |
| Full suite command | `dotnet test src/YH.Flow.slnx --nologo`                                           |

### Phase Requirements ? Test Map

| Req ID  | Behavior                                  | Test Type        | Automated Command                                     | File Exists? |
| ------- | ----------------------------------------- | ---------------- | ----------------------------------------------------- | ------------ |
| REQ-3.1 | 创建项目 + 自动建 Admin ProjectMember     | unit/integration | `dotnet test ... --filter CreateProjectTests`         | ? Wave 0     |
| REQ-3.1 | Identifier 唯一性约束（workspace 级别）   | integration      | `dotnet test ... --filter IdentifierUniqueTests`      | ? Wave 0     |
| REQ-3.1 | Identifier 禁止特殊字符                   | unit             | `dotnet test ... --filter IdentifierValidationTests`  | ? Wave 0     |
| REQ-3.1 | 软删除释放 slug                           | integration      | `dotnet test ... --filter SoftDeleteSlugReleaseTests` | ? Wave 0     |
| REQ-3.1 | 项目列表 network 过滤（Secret vs Public） | integration      | `dotnet test ... --filter ProjectNetworkFilterTests`  | ? Wave 0     |
| REQ-3.1 | 软删除不释放 identifier（比 slug 不同）   | integration      | `dotnet test ... --filter SoftDeleteIdentifierTests`  | ? Wave 0     |
| REQ-3.2 | 添加成员到项目                            | integration      | `dotnet test ... --filter AddMemberTests`             | ? Wave 0     |
| REQ-3.2 | 项目成员列表 + 批量查用户                 | integration      | `dotnet test ... --filter ListMembersTests`           | ? Wave 0     |
| REQ-3.2 | 更新成员 Role                             | integration      | `dotnet test ... --filter UpdateMemberRoleTests`      | ? Wave 0     |
| REQ-3.2 | 移除成员（is_active=false）               | integration      | `dotnet test ... --filter RemoveMemberTests`          | ? Wave 0     |
| NFR-2   | 跨 workspace 数据隔离                     | integration      | `dotnet test ... --filter TenantIsolationTests`       | ? Wave 0     |
| NFR-2   | 非 workspace-member 不可见 Secret 项目    | integration      | `dotnet test ... --filter AccessControlTests`         | ? Wave 0     |

### Sampling Rate

- **Per task commit:** `dotnet test src/Tests/Project.Tests --filter "Unit" --nologo`
- **Per wave merge:** `dotnet test src/YH.Flow.slnx --nologo`（全量回归，确保不破坏 Phase 1/2）
- **Phase gate:** 全量绿 + 手工 smoke：用 API 创建 workspace ? 创建 project ? 添加 member ? 列表验证

### Wave 0 Gaps

- [ ] `src/Tests/Project.Tests/` 项目（csproj + 引用 Project + TestUtils）—— 模仿 `src/Tests/Workspace.Tests/`
- [ ] `src/Tests/Project.Tests/TestData/ProjectTestFixture.cs` —— 共享 fixture
- [ ] `src/Tests/Project.Tests/Domain/ProjectTests.cs` —— 实体工厂测试
- [ ] `src/Tests/Project.Tests/Integration/IdentifierUniqueTests.cs`
- [ ] `src/Tests/Project.Tests/Integration/ProjectNetworkFilterTests.cs`
- [ ] `src/Tests/Project.Tests/Integration/TenantIsolationTests.cs`

## Security Domain

> `security_enforcement` 在 `.planning/config.json` 中未显式设为 false ? 默认**启用**。

### Applicable ASVS Categories

| ASVS Category         | Applies | Standard Control                                                                       |
| --------------------- | ------- | -------------------------------------------------------------------------------------- |
| V2 Authentication     | no      | Phase 1 已实现（JWT/API Key/Session），Phase 3 复用                                    |
| V3 Session Management | no      | Phase 1 已实现                                                                         |
| V4 Access Control     | **yes** | `[RequireWorkspaceRole]`（workspace 级）+ handler 内 ProjectMember role 检查（项目级） |
| V5 Input Validation   | **yes** | FluentValidation（identifier 特殊字符校验、name/description 长度校验）                 |
| V6 Cryptography       | no      | Phase 3 无密码学需求                                                                   |
| V8 Data Protection    | **yes** | 多租户隔离（Project 实现 `IHasTenant`，自动过滤）                                      |

### Known Threat Patterns for ASP.NET Core + Finbuckle 多租户

| Pattern                            | STRIDE                             | Standard Mitigation                                              |
| ---------------------------------- | ---------------------------------- | ---------------------------------------------------------------- | ---------------------------- |
| 跨 workspace 项目泄露              | Tampering / Information Disclosure | `BaseDbContext` 自动 tenant filter（Project 实现 `IHasTenant`）  |
| 非成员访问 Secret 项目             | Information Disclosure             | Query filter: `network=2 OR is_member=true`                      |
| Identifier 注入                    | Tampering                          | FluentValidation：禁止特殊字符集 `[&+,:;$^\*}{=?@#               | '<>.()%!-]`，只允许大写+数字 |
| 权限提升（普通成员改自己为 Admin） | Elevation of Privilege             | handler 内查 ProjectMember role，只有 Admin 可改 role            |
| 创建项目后创建者不可见             | Availability                       | 自动插入 `ProjectMember(role=Admin)` 在同一个 SaveChanges 事务中 |

## Sources

### Primary（HIGH confidence）

- **Plane 源码**（API 契约 + 数据模型 ground truth）：
  - `apps/api/plane/db/models/project.py` — Project/ProjectMember/ProjectIdentifier 实体（全部字段、unique_together、FORBIDDEN_IDENTIFIER_CHARS_PATTERN）
  - `apps/api/plane/api/serializers/project.py` — ProjectCreateSerializer / ProjectSerializer / ProjectUpdateSerializer 字段和校验
  - `apps/api/plane/api/views/project.py` — ProjectListCreateAPIEndpoint / ProjectDetailAPIEndpoint / ProjectArchiveUnarchiveAPIEndpoint
  - `apps/api/plane/api/urls/project.py` — 项目路由契约
  - `apps/api/plane/app/urls/project.py` — App 级别项目路由（members/invitations/favorites/archive）
  - `apps/api/plane/api/views/member.py` — ProjectMemberListCreateAPIEndpoint / ProjectMemberDetailAPIEndpoint
  - `apps/api/plane/api/serializers/member.py` — ProjectMemberSerializer
  - `apps/api/plane/app/permissions/project.py` — ProjectBasePermission / ProjectMemberPermission / ProjectAdminPermission / ProjectEntityPermission
  - `apps/api/plane/utils/permissions/project.py` — 辅助 permission 类
- **现有模块代码**（直接读取，最高权威）：
  - `yh-flow/src/Modules/Workspace/` — 全部 Workspace 模块代码（Project 模块的直接模板）
  - `yh-flow/src/BuildingBlocks/` — BaseDbContext、IHasTenant、ISoftDeletable、IAuditableEntity

### Secondary（MEDIUM confidence）

- 所有 Phase 3 模式均已通过现有 Workspace 模块代码双重验证

### Tertiary（LOW confidence — 已标 [ASSUMED]）

- A1/A2/A3: 与 Plane 行为差异（均非阻断风险）

## Metadata

**Confidence breakdown：**

- Standard stack: **HIGH** — 全部依赖已在项目锁定版本，直接读源码确认
- Architecture（实体/DbContext/端点）: **HIGH** — Workspace 模块可完全复用模式
- Pitfalls: **HIGH** — 基于 Plane 源码对比 + Workspace 模块经验
- Plane 契约: **HIGH** — 直接读 Plane 源码（project.py / serializers / views / urls / permissions）
- Unsplash 集成: **LOW** — 未确认具体集成方式（CONTEXT 决定延后）

**Research date:** 2026-06-24
**Valid until:** 2026-07-24（30 天）

---

## RESEARCH COMPLETE

**Phase:** 03 - project
**Confidence:** HIGH

### Key Findings

1. **0 新增依赖** — Phase 3 完全复用 Phase 1/2 已锁定的所有包。所有模式（DbContext、实体、Mediator slice、授权）均从 Workspace 模块复制适配。
2. **Project 实体必须 `IHasTenant`（非 `IGlobalEntity`）** — 不同于 Workspace（它是租户），Project 是租户内的数据。自动受 `BaseDbContext` 的 tenant filter 保护。
3. **Identifier 和 Slug 是独立字段** — Identifier 是用户设置的短前缀（如 "PROJ"），Slug 来自名称自动生成。Identifier 在 workspace 级别唯一，软删除时**不需要**改 identifier（因为使用了 `deleted_at` 条件唯一索引）。Slug 需要 `__{epoch}` 释放。
4. **ProjectMember 是独立实体** — 独立于 WorkspaceMember，即使 workspace Guest 也可成为 Project Member。角色值复用 `WorkspaceRole` (20/15/5)。
5. **创建项目必须自动插入 ProjectMember(Admin)** — 遵循 Plane 行为：`ProjectMember.objects.create(project_id=..., member=request.user, role=20)`。
6. **项目列表需实现 network 过滤** — Secret(0) 仅成员可见；Public(2) 对所有人可见。Plane 用 `Q(network=2) | Q(is_member=True)`。

### File Created

`D:\github\akinix-plane\.planning\phases\03-project\03-RESEARCH.md`

### Confidence Assessment

| Area                                | Level | Reason                                                             |
| ----------------------------------- | ----- | ------------------------------------------------------------------ |
| Standard Stack                      | HIGH  | 全部已在项目，0 新增                                               |
| Architecture（实体/DbContext/端点） | HIGH  | Workspace 模块已验证的模式直接复制                                 |
| Pitfalls                            | HIGH  | Plane 源码 + Workspace 经验                                        |
| Plane 契约                          | HIGH  | 直接读 Plane project.py / serializers / views / urls / permissions |

### Open Questions

1. **Unsplash 封面搜索端点集成方式** — 建议 Phase 3 只做封面 URL 存储字段，搜索延后。
2. **项目列表参数支持** — 建议支持基本 `order_by` + 分页；`fields`/`expand` 延后。

### Ready for Planning

研究完成。Planner 可基于本 RESEARCH.md 创建 PLAN.md。建议 wave 划分：

- **Wave 0:** 项目脚手架（Project.Tests + ProjectModule + ProjectDbContext + 实体 + Migration）+ CONTEXT 中的灰色区域2（权限模型验证）
- **Wave 1:** Domain 实体（Project + ProjectMember）+ ProjectDbContext + 迁移 + ProjectModule 注册
- **Wave 2:** Project CRUD 端点（create/get/update/delete/list）— REQ-3.1
- **Wave 3:** ProjectMember 端点（add/list/update-role/remove）— REQ-3.2
- **Wave 4:** Project 设置（identifier/cover-image/visibility）集成— REQ-3.3 + 全量回归
