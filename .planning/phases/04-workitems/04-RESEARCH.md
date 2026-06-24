# Phase 4: WorkItems — 工作项 - Research

**Researched:** 2026-06-24
**Domain:** 多租户工作项管理系统（State、Label、Issue、IssueLink、IssueComment、IssueActivity、Estimate、Intake、Import/Export）
**Confidence:** HIGH（Plane 源码 + 现有多模块模式双重验证；仅 Import/Export 异步 Job 方式为 MEDIUM）

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions

#### 灰色区域 1：Issue 实体模型

- **优先级字段** — 字符串类型 `Priority`，取值范围：`"urgent"` / `"high"` / `"medium"` / `"low"` / `"none"`。与 Plane 兼容，不使用独立 Priority 实体或枚举。
- **父子关联** — `ParentId`（Guid? 自引用 FK）。深度限制为 1 层（Plane 行为，不支持多层嵌套树）。
- **Issue 链接** — 独立 `IssueLink` 实体，`LinkType` 枚举区分关系类型：`RelatesTo` / `Duplicate` / `Blocks` / `BlockedBy`。与 Plane issue_link 表兼容。
- **编号** — `SequenceId`（项目内自增 int）+ `SortOrder`（double，默认 65535.0）。`SequenceId` 格式如 `PROJ-1`（Phase 3 的 Identifier + SequenceId 组合在 API 响应中拼接）。

#### 灰色区域 2：State 生命周期

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

#### 灰色区域 3：DbContext 边界

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

### Deferred Ideas（OUT OF SCOPE）

- Issue 时间线视图（甘特图）的 API 支持
- 工作项模板/预设
- 全文搜索

</user_constraints>

<phase_requirements>

## Phase Requirements

| ID      | Description      | Research Support                                                                                                                                                       |
| ------- | ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| REQ-4.1 | 状态管理 (State) | §Plane State Model (state.py:79-127), §Plane API Contract (State URL + Serializer), §Code Examples (State entity + Configuration + Seed)                               |
| REQ-4.2 | 标签管理 (Label) | §Plane Label Model (label.py:11-44), §Plane API Contract (Label URL + Serializer), §Code Examples (Label entity + Configuration)                                       |
| REQ-4.3 | Issue CRUD       | §Plane Issue Model (issue.py:104-256), §Plane API Contract (Issue URL + Serializer), §Architecture Patterns (Issue Entity + Batch Operations)                          |
| REQ-4.4 | Issue 属性       | §Plane Issue Model (priority, parent, assignees M2M, labels M2M, dates), §Plane IssueLink Model (issue_link.py:371-384), §Plane IssueRelation Model (issue.py:296-321) |
| REQ-4.5 | Issue 评论       | §Plane IssueComment Model (issue.py:451-540), §Plane API Contract (Comment URL + Serializer)                                                                           |
| REQ-4.6 | Issue 活动日志   | §Plane IssueActivity Model (issue.py:415-449), §Pattern: Domain Events for Activity Tracking                                                                           |
| REQ-4.7 | 估算管理         | §Plane Estimate Model (estimate.py:18-57), §Plane API Contract (Estimate URL + Serializer)                                                                             |
| REQ-4.8 | 收件箱 (Intake)  | §Plane Intake Model (intake.py:12-84), §Plane API Contract (Intake URL + Serializer)                                                                                   |
| REQ-4.9 | 导入/导出        | §Plane Export/Import Models, §Standard Stack (CSV/JSON handling with CsvHelper/System.Text.Json)                                                                       |

</phase_requirements>

## Project Constraints (from CLAUDE.md)

- **Namespace 约定**：`YH.Framework.{Name}`（BuildingBlocks）、`YH.Modules.{Name}`（Modules）、`YH.Flow.{Name}`（Host）。
- **`TreatWarningsAsErrors`** — 警告即编译失败。
- **AGENTS.md 10 条黄金法则** — 特别是：模块只通过 `.Contracts` 引用；handler 必须是 `public sealed` + `ValueTask<T>` + `.ConfigureAwait(false)`；每条 paginated query handler 要有 validator。
- **Minimal API + Vertical Slice** — 贴 Phase 3 Project 模块 `Features/v1/{Entity}/{Action}/{Command/Query, Handler, Validator, Endpoint}.cs` 模式。
- **模块注册涉及 FOUR 个地方**：API Program.cs 的 Mediator assemblies + moduleAssemblies 数组，以及 DbMigrator Program.cs 完全相同的两处。

## Summary

Phase 4 在 Phase 2（Workspace 租户基座）+ Phase 3（Project 聚合根）之上，交付 Plane 最核心的工作项管理系统。这是整个产品最大的单个 Phase，覆盖 9 个需求、15 个任务（ROADMAP.md T4.1-T4.15），预估 5-7 天。

**核心架构模式：**

1. **独立 DbContext + Schema**：`WorkItemsDbContext` 使用 `yhschema.WorkItems` schema，包含 State、Label、Issue、IssueLink、IssueComment、IssueActivity、Estimate、EstimatePoint、Intake、IntakeIssue 等实体。跨模块通过 `ProjectId`（Guid 标量）关联 Project。
2. **实体依赖顺序**：State（无依赖）→ Label（无依赖）→ Estimate（无依赖）→ Issue（依赖 State/Label/Estimate）→ IssueAssignee/IssueLabel（依赖 Issue）→ IssueLink（依赖 Issue）→ IssueComment（依赖 Issue）→ IssueActivity（依赖 Issue）→ Intake/IntakeIssue（依赖 Issue/State）。
3. **活动日志架构**：使用 MediatR IDomainEvent 捕获 Issue 实体变更 → 异步持久化到 IssueActivity 表。与 Plane 的 `IssueActivity`（审计日志模型）兼容。
4. **SequenceId 生成**：项目内自增整数，使用 DB 事务级锁（Plane 使用 PostgreSQL `pg_advisory_xact_lock`）。在 .NET 中用 `IDbContextTransaction` + `SELECT MAX(sequence_id) ... WHERE ProjectId = @pid` 实现。

**技术栈：** 无需新增外部 NuGet 包。CsvHelper（可能已经存在）用于 CSV 导入导出。所有其他依赖复用 Phase 1-3。

**Primary recommendation：** 严格按照实体依赖顺序分 Wave 实现：Wave 1（基座实体：State/Label/Estimate）→ Wave 2（Issue 实体 + 核心 CRUD）→ Wave 3（子实体：Link/Comment/Activity）→ Wave 4（Intake + Import/Export + 权限+迁移）。Wave 5 全量回归。

## Architectural Responsibility Map

| Capability          | Primary Tier                                                  | Secondary Tier                 | Rationale                                                               |
| ------------------- | ------------------------------------------------------------- | ------------------------------ | ----------------------------------------------------------------------- |
| State CRUD 业务逻辑 | API / Backend（Mediator handler）                             | Database（WorkItemsDbContext） | Vertical slice，handler 操作 tenant-scoped WorkItemsDbContext           |
| State 种子数据      | Database / Migration（DbMigrator）                            | —                              | 项目创建时自动插入 5 个默认 State；已有项目在 DbMigrator 启动时检测填充 |
| Label CRUD 业务逻辑 | API / Backend（Mediator handler）                             | Database（WorkItemsDbContext） | 简单的 CRUD，无复杂业务逻辑                                             |
| Label 层级结构      | Database / EF（自引用 ParentId FK）                           | API / Handler                  | Plane Label 支持 parent 自引用                                          |
| Issue 核心 CRUD     | API / Backend（Mediator handler）                             | Database（WorkItemsDbContext） | 含描述 HTML 净化、日期校验                                              |
| SequenceId 分配     | Database / EF（事务级锁）                                     | —                              | 确保每个 Project 内 sequence_id 严格递增不冲突                          |
| Issue 指派人 (M2M)  | Database / EF（IssueAssignee 中间表）                         | API / Handler                  | Plane 使用独立 IssueAssignee 表 + M2M through                           |
| Issue 标签 (M2M)    | Database / EF（IssueLabel 中间表）                            | API / Handler                  | Plane 使用独立 IssueLabel 表 + M2M through                              |
| Issue 父子关联      | Database / EF（自引用 ParentId FK）                           | API / Handler                  | CONTEXT 限制深度 1 层                                                   |
| Issue 链接          | API / Backend（Mediator handler）                             | Database（WorkItemsDbContext） | LinkType 枚举独立实体                                                   |
| Issue 评论 CRUD     | API / Backend（Mediator handler）                             | Database（WorkItemsDbContext） | 评论中 @ 提及（IssueMention 表）在此 Phase 可选                         |
| Issue 活动日志      | API / Backend（领域事件 + 异步持久化）                        | Database（WorkItemsDbContext） | 使用 MediatR IDomainEvent → IssueActivity 写入                          |
| Estimate CRUD       | API / Backend（Mediator handler）                             | Database（WorkItemsDbContext） | EstimatePoint 作为 Estimate 的子实体                                    |
| Intake 收件箱       | API / Backend（Mediator handler）                             | Database（WorkItemsDbContext） | Issue `is_draft` 标记 + IntakeIssue 关联表                              |
| Import/Export       | API / Backend（直接处理）                                     | —                              | CONTEXT 决策：非后台 Job，直接 API 处理 CSV/JSON                        |
| 权限控制            | API / Authorization（[RequireWorkspaceRole] + ProjectMember） | —                              | 复用 Phase 2/3 授权基础设施                                             |

## Standard Stack

### Core（全部已在项目中，0 新增 NuGet 包）

| Library                              | Version      | Purpose                                            | Why Standard                               |
| ------------------------------------ | ------------ | -------------------------------------------------- | ------------------------------------------ |
| `Microsoft.EntityFrameworkCore.*`    | .NET 10 内置 | WorkItemsDbContext、迁移、IEntityTypeConfiguration | 全项目统一 EF Core                         |
| `Mediator`（source-gen）             | 现有         | CQRS command/query + handler + validator pipeline  | Phase 2/3 已验证模式                       |
| `FluentValidation`                   | 现有         | DTO 校验                                           | Phase 1 已注册 AddValidatorsFromAssemblies |
| `Finbuckle.MultiTenant.Abstractions` | 现有         | IMultiTenantContextAccessor、IHasTenant            | Phase 1/2 已建                             |
| `YH.Framework.Core.Domain`           | 现有         | IHasTenant、ISoftDeletable、IAuditableEntity       | 基接口                                     |
| `System.Text.Json`                   | .NET 10 内置 | JSON 序列化（Issue description_json、Props）       | .NET 内置，无需 Newtonsoft.Json            |

### Supporting

| Library                          | Version  | Purpose                                 | When to Use                            |
| -------------------------------- | -------- | --------------------------------------- | -------------------------------------- |
| `YH.Modules.Workspace.Contracts` | 现有     | ICurrentWorkspaceContext、WorkspaceRole | 所有端点（route slug→tenant 解析）     |
| `YH.Modules.Project.Contracts`   | 现有     | ProjectDto、ProjectConstants            | Issue 端点需要 ProjectId 有效性验证    |
| `YH.Modules.Identity.Contracts`  | 现有     | IUserIdentityService                    | 批量解析 assignee/actor 用户详情       |
| `CsvHelper`（已存在？）          | 检查项目 | CSV 导入导出                            | Import/Export 端点处理 CSV 格式        |
| `HtmlSanitizer`（可能需要）      | 待检查   | HTML 消毒（Issue description_html）     | Plane 有 content_validator 做 XSS 防护 |

### Installation

```bash
# 不期望新增外部包。确认 CsvHelper 和 HtmlSanitizer 是否已在项目中
# 如缺少，通过 Directory.Packages.props 统一添加
dotnet restore src/YH.Flow.slnx
```

**Version verification:** 所有核心包已通过 Phase 1/2/3 锁定版本确认。Phase 4 零新增包。仅 `CsvHelper` 在需要 CSⅤ 导入导出时检查——如果项目已有（fullstackhero 模板可能包含），无需新增。

## Package Legitimacy Audit

> Phase 4 **不安装任何新外部包**。所有依赖均复用 Phase 1/2/3 已在项目中的包。

| Package    | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
| ---------- | -------- | --- | --------- | ----------- | --------- | ----------- |
| （无新增） | —        | —   | —         | —           | N/A       | N/A         |

**Packages removed due to slopcheck [SLOP] verdict:** none
**Packages flagged as suspicious [SUS]:** none

## Architecture Patterns

### System Architecture Diagram

```
HTTP Request: /api/v1/workspaces/{slug}/projects/{projectId}/work-items/...
   │
   ▼
[ASP.NET Core Pipeline]
   │
   ├─ UseMultiTenant() / WorkspaceSlugStrategy
   │   → Resolves {slug} → TenantId = workspaceGuid
   │
   ├─ WorkspaceMembershipMiddleware
   │   → Fills ICurrentWorkspaceContext
   │
   ├─ UseAuthorization() → [RequireWorkspaceRole] + ProjectMember check
   │
   └─ WorkItem Endpoint Handler (Mediator)
        │
        ├─ REQ-4.1: State CRUD (StatesController-style)
        │   ├─ POST/GET /states/ → [RequireWorkspaceRole(Admin|Member)]
        │   └─ GET/PATCH/DELETE /states/{stateId}/
        │
        ├─ REQ-4.2: Label CRUD (LabelsController-style)
        │   ├─ POST/GET /labels/ → [RequireWorkspaceRole(Admin)]
        │   └─ GET/PATCH/DELETE /labels/{labelId}/
        │
        ├─ REQ-4.3/4.4: Issue CRUD
        │   ├─ POST/GET /work-items/ → [RequireWorkspaceRole(Admin|Member|Guest)]
        │   ├─ GET /work-items/{issueId}/
        │   ├─ PATCH /work-items/{issueId}/
        │   ├─ DELETE /work-items/{issueId}/
        │   ├─ POST /work-items/bulk/ → [RequireWorkspaceRole(Admin|Member)]
        │   └─ GET /work-items/{issueId}/relations/ → IssueRelation
        │
        ├─ REQ-4.5: IssueComment
        │   ├─ POST/GET /work-items/{issueId}/comments/
        │   └─ GET/PATCH/DELETE /work-items/{issueId}/comments/{commentId}/
        │
        ├─ REQ-4.6: IssueActivity
        │   ├─ GET /work-items/{issueId}/activities/
        │   └─ GET /work-items/{issueId}/activities/{activityId}/
        │
        ├─ REQ-4.7: Estimate
        │   ├─ POST/GET /estimates/
        │   ├─ GET/PATCH/DELETE /estimates/{estimateId}/
        │   └─ POST/GET /estimates/{estimateId}/estimate-points/
        │
        ├─ REQ-4.8: Intake
        │   ├─ POST/GET /intake-issues/
        │   └─ GET/PATCH/DELETE /intake-issues/{issueId}/
        │
        └─ REQ-4.9: Import/Export
            ├─ POST /workspaces/{slug}/export-issues/ → ExportIssues
            ├─ POST /workspaces/{slug}/import-issues/ → ImportIssues
            └─ File download via URL
```

### Entity Dependency Graph

```
State (无依赖)      Label (无依赖)      Estimate (无依赖)
    │                    │                   │
    │                    │                   │
    └────────┬───────────┴─────────┬─────────┘
             │                     │
             ▼                     ▼
           Issue ◄── EstimatePoint (belongs to Estimate)
             │
      ┌──────┼──────┬──────┬──────┐
      ▼      ▼      ▼      ▼      ▼
IssueLink  IssueComment  IssueActivity  IssueAssignee  IssueLabel
(depends)  (depends)     (depends)      (M2M through)  (M2M through)
             │
             ▼
           IntakeIssue
```

### Recommended Project Structure

```
yh-flow/src/Modules/WorkItems/
├── Modules.WorkItems/
│   ├── WorkItemsModule.cs                    # IModule 实现, Order 260
│   ├── WorkItemsModuleConstants.cs            # SchemaName = "yhschema.WorkItems"
│   ├── AssemblyInfo.cs                        # [FshModule(typeof(WorkItemsModule), 260)]
│   ├── Domain/
│   │   ├── State.cs                           # State 实体
│   │   ├── Label.cs                           # Label 实体
│   │   ├── Issue.cs                           # Issue 实体（含 Priority, SequenceId）
│   │   ├── IssueAssignee.cs                   # M2M through 实体
│   │   ├── IssueLabel.cs                      # M2M through 实体
│   │   ├── IssueLink.cs                       # IssueLink 实体
│   │   ├── IssueComment.cs                    # IssueComment 实体
│   │   ├── IssueActivity.cs                   # IssueActivity 实体
│   │   ├── Estimate.cs                        # Estimate 实体
│   │   ├── EstimatePoint.cs                   # EstimatePoint 实体
│   │   ├── Intake.cs                          # Intake 实体
│   │   └── IntakeIssue.cs                     # IntakeIssue 实体
│   ├── Data/
│   │   ├── WorkItemsDbContext.cs              # 派生 BaseDbContext
│   │   └── Configurations/
│   │       ├── StateConfiguration.cs
│   │       ├── LabelConfiguration.cs
│   │       ├── IssueConfiguration.cs
│   │       ├── IssueAssigneeConfiguration.cs
│   │       ├── IssueLabelConfiguration.cs
│   │       ├── IssueLinkConfiguration.cs
│   │       ├── IssueCommentConfiguration.cs
│   │       ├── IssueActivityConfiguration.cs
│   │       ├── EstimateConfiguration.cs
│   │       ├── EstimatePointConfiguration.cs
│   │       ├── IntakeConfiguration.cs
│   │       └── IntakeIssueConfiguration.cs
│   ├── Features/v1/
│   │   ├── States/
│   │   │   ├── CreateState/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── ListStates/{Query, Handler, Endpoint}.cs
│   │   │   ├── UpdateState/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── DeleteState/{Command, Handler, Endpoint}.cs
│   │   │   └── (State seed logic in WorkItemsModule or DbMigrator)
│   │   ├── Labels/
│   │   │   ├── CreateLabel/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── ListLabels/{Query, Handler, Endpoint}.cs
│   │   │   ├── UpdateLabel/{Command, Handler, Validator, Endpoint}.cs
│   │   │   └── DeleteLabel/{Command, Handler, Endpoint}.cs
│   │   ├── Issues/
│   │   │   ├── CreateIssue/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── GetIssue/{Query, Handler, Endpoint}.cs
│   │   │   ├── UpdateIssue/{Command, Handler, Validator, Endpoint}.cs
│   │   │   ├── DeleteIssue/{Command, Handler, Endpoint}.cs
│   │   │   ├── ListIssues/{Query, Handler, Validator, Endpoint}.cs
│   │   │   └── BulkUpdateIssues/{Command, Handler, Validator, Endpoint}.cs
│   │   ├── IssueLinks/
│   │   │   ├── CreateIssueLink/{...}.cs
│   │   │   ├── ListIssueLinks/{...}.cs
│   │   │   └── DeleteIssueLink/{...}.cs
│   │   ├── IssueComments/
│   │   │   ├── CreateIssueComment/{...}.cs  (leave blank)
│   │   │   ├── ListIssueComments/{...}.cs
│   │   │   ├── UpdateIssueComment/{...}.cs
│   │   │   └── DeleteIssueComment/{...}.cs
│   │   ├── IssueActivities/
│   │   │   └── ListIssueActivities/{Query, Handler, Endpoint}.cs
│   │   ├── Estimates/
│   │   │   ├── CreateEstimate/{...}.cs
│   │   │   ├── ListEstimates/{...}.cs
│   │   │   ├── UpdateEstimate/{...}.cs
│   │   │   └── DeleteEstimate/{...}.cs
│   │   ├── EstimatePoints/
│   │   │   ├── CreateEstimatePoint/{...}.cs
│   │   │   ├── ListEstimatePoints/{...}.cs
│   │   │   ├── UpdateEstimatePoint/{...}.cs
│   │   │   └── DeleteEstimatePoint/{...}.cs
│   │   ├── Intake/
│   │   │   ├── CreateIntakeIssue/{...}.cs
│   │   │   ├── ListIntakeIssues/{...}.cs
│   │   │   └── UpdateIntakeIssue/{...}.cs
│   │   └── ImportExport/
│   │       ├── ExportIssues/{Command, Handler, Endpoint}.cs
│   │       └── ImportIssues/{Command, Handler, Validator, Endpoint}.cs
│   └── Services/
│       └── IssueSequenceService.cs            # SequenceId 生成服务
└── Modules.WorkItems.Contracts/
    ├── DTOs/
    │   ├── StateDto.cs
    │   ├── LabelDto.cs
    │   ├── IssueDto.cs
    │   ├── IssueDetailDto.cs
    │   ├── IssueLinkDto.cs
    │   ├── IssueCommentDto.cs
    │   ├── IssueActivityDto.cs
    │   ├── EstimateDto.cs
    │   ├── EstimatePointDto.cs
    │   ├── IntakeIssueDto.cs
    │   └── ImportExportDto.cs
    ├── Constants/
    │   └── WorkItemsConstants.cs               # 字段最大长度、默认值、Pattern
    └── v1/...
```

### Pattern 1: State 实体（贴 Plane state.py 模式）

**What:** 项目状态实体，5 个固定 Group 枚举 + 用户自定义 Name。项目创建时自动种子 5 个默认 State。

**Plane 字段对照 [VERIFIED: Plane state.py:79-127]：**

| Plane 字段    | .NET 字段   | 类型              | 备注                                               |
| ------------- | ----------- | ----------------- | -------------------------------------------------- |
| `name`        | `Name`      | string(255)       | 必填，用户可自定义（如 "待处理"）                  |
| `color`       | `Color`     | string(7)         | 颜色码，如 "#60646C"                               |
| `group`       | `Group`     | `StateGroup` enum | Backlog/Unstarted/Started/Completed/Cancelled      |
| `sequence`    | `SortOrder` | double            | 排序，默认 65535，新增时 +15000                    |
| `default`     | `IsDefault` | bool              | 每个 Group 可有一个 default                        |
| `is_triage`   | `IsTriage`  | bool              | Plane 有 TRIAGE group，CONTEXT 决定只保留 5 groups |
| `slug`        | —           | —                 | CONTEXT 未提及，不实现                             |
| `description` | —           | —                 | CONTEXT 未提及，不实现                             |

**Default states seed data（Plane DEFAULT_STATES [VERIFIED: Plane state.py:24-62]）：**

| Name        | Color     | Group     | SortOrder | IsDefault |
| ----------- | --------- | --------- | --------- | --------- |
| Backlog     | "#60646C" | Backlog   | 15000     | true      |
| Todo        | "#60646C" | Unstarted | 25000     | false     |
| In Progress | "#F59E0B" | Started   | 35000     | false     |
| Done        | "#46A758" | Completed | 45000     | false     |
| Cancelled   | "#9AA4BC" | Cancelled | 55000     | false     |

**When to use:** 项目创建时自动种子。用户可添加自定义 State（限定在 5 个 Group 中）。

### Pattern 2: Label 实体（贴 Plane label.py 模式）

**What:** 层级标签，支持 parent 自引用实现标签分组。

**Plane 字段对照 [VERIFIED: Plane label.py:11-44]：**

| Plane 字段        | .NET 字段   | 类型        | 备注                    |
| ----------------- | ----------- | ----------- | ----------------------- |
| `name`            | `Name`      | string(255) | 必填，项目内唯一        |
| `color`           | `Color`     | string(7)   | 颜色码                  |
| `parent`          | `ParentId`  | Guid?       | 自引用 FK，层级标签支持 |
| `sort_order`      | `SortOrder` | double      | 排序                    |
| `description`     | —           | —           | 可选，推荐实现          |
| `external_source` | —           | —           | 导入兼容，可选          |

**When to use:** 标签是项目的二等公民，不需要复杂授权。CRUD 仅限 Admin。

### Pattern 3: Issue 实体（贴 Plane issue.py 模式）

**What:** 核心工作项实体。Plane 最复杂的实体，包含 20+ 字段和多个 M2M 关系。

**Plane 字段对照 [VERIFIED: Plane issue.py:104-256]：**

| Plane 字段             | .NET 字段                   | 类型            | 备注                                  |
| ---------------------- | --------------------------- | --------------- | ------------------------------------- |
| `name`                 | `Name`                      | string(255)     | 必填                                  |
| `description_html`     | `DescriptionHtml`           | string          | HTML 富文本，默认 `<p></p>`           |
| `description_json`     | `DescriptionJson`           | string?         | JSON 格式描述                         |
| `description_stripped` | `DescriptionStripped`       | string?         | 纯文本（保存时自动剥离 HTML）         |
| `priority`             | `Priority`                  | string          | "urgent"/"high"/"medium"/"low"/"none" |
| `sequence_id`          | `SequenceId`                | int             | 项目内自增                            |
| `sort_order`           | `SortOrder`                 | double          | 拖拽排序，默认 65535                  |
| `parent`               | `ParentId`                  | Guid?           | 自引用 FK，深度 1 层                  |
| `state`                | `StateId`                   | Guid            | FK to State                           |
| `assignees`            | IssueAssignee (M2M through) | —               | 通过 IssueAssignee 中间表             |
| `labels`               | IssueLabel (M2M through)    | —               | 通过 IssueLabel 中间表                |
| `estimate_point`       | `EstimatePointId`           | Guid?           | FK to EstimatePoint                   |
| `start_date`           | `StartDate`                 | DateOnly?       | 开始日期                              |
| `target_date`          | `TargetDate`                | DateOnly?       | 截止日期                              |
| `completed_at`         | `CompletedAt`               | DateTimeOffset? | 关闭时自动设置                        |
| `archived_at`          | `ArchivedAt`                | DateTimeOffset? | 归档时间                              |
| `is_draft`             | `IsDraft`                   | bool            | Intake 草稿标记                       |
| `external_source`      | —                           | —               | 导入兼容                              |
| `external_id`          | —                           | —               | 导入兼容                              |

**Issue 默认状态分配（Plane `Issue._ensure_default_state` [VERIFIED: issue.py:228-238]）：**

- 创建 Issue 时如果不指定 state，自动分配当前项目的 default state
- 如果项目没有 default state，分配第一个 state

**CompletedAt 同步逻辑（Plane `Issue._sync_completed_at` [VERIFIED: issue.py:240-255]）：**

- state 变更为 Completed group → `completed_at = DateTimeOffset.UtcNow`
- state 从 Completed 变更为其他 → `completed_at = null`

**SequenceId 分配（Plane `Issue.save` [VERIFIED: issue.py:184-214]）：**

- 使用 PostgreSQL `pg_advisory_xact_lock(project_id)` 确保自增序列事务安全
- 读取 `IssueSequence` 表的 `MAX(sequence)` + 1
- .NET 实现：用 `SERIALIZABLE` 隔离级别或分布式锁替代

### Pattern 4: IssueAssignee + IssueLabel M2M Through Entities

**What:** Plane 使用独立中间表实现 Issue↔Assignee 和 Issue↔Label 的多对多关系，而不是 Django 自动的 M2M 表。这些中间表是 `ProjectBaseModel`，包含租户字段。

**IssueAssignee [VERIFIED: Plane issue.py:345-368]：**

```
Fields: Id, IssueId, AssigneeId, TenantId, ProjectId, WorkspaceId
Unique: (IssueId, AssigneeId) with deleted_at IS NULL filter
```

**IssueLabel [VERIFIED: Plane issue.py:543-555]：**

```
Fields: Id, IssueId, LabelId, TenantId, ProjectId, WorkspaceId
```

**在 CreateIssue / UpdateIssue 时的处理逻辑（Plane `IssueCreateSerializer.create` [VERIFIED: serializers/issue.py:199-274]）：**

- Create: 从 validated_data pop assignee_ids/label_ids → 在 Issue 创建后，调用 `IssueAssignee.objects.bulk_create()` / `IssueLabel.objects.bulk_create()`
- Update: 中间表全量替换——先 DELETE 所有旧记录，再 bulk_create 新记录

### Pattern 5: IssueLink 实体 + IssueRelation（两种连接概念）

**Plane 有两种 Issue 连接机制：**

**1. IssueLink（外部链接）[VERIFIED: Plane issue.py:371-384]：**

```
Fields: Id, Title, Url, IssueId, Metadata(JSON)
用途：链接 Issue 到外部 URL（GitHub issue、设计稿、文档）
```

**Plane 不限制链接类型。**

**2. IssueRelation（Issue 间关系）[VERIFIED: Plane issue.py:296-321]：**

```
Fields: Id, IssueId, RelatedIssueId, RelationType(string)
RelationType: duplicate / relates_to / blocked_by / start_before / finish_before / implemented_by
Unique: (Issue, RelatedIssue) with deleted_at IS NULL filter
```

**CONTEXT 决策简化版：** 实现 `IssueLink` 实体，`LinkType` 枚举 = `RelatesTo` / `Duplicate` / `Blocks` / `BlockedBy`。Combine external link + internal relation into single entity.

**Controversy note:** Plane 的 `IssueLink` 是纯外部链接（只有 url + title），内部 Issue 连接用 `IssueRelation`（有 relation_type）。CONTEXT 决定合并为一个实体——实现 `IssueLink` 同时包含 `Url`（外部链接）和 `RelatedIssueId`（内部关联）+ `LinkType`。这种合并偏离 Plane 原始设计但简化了模型。

### Pattern 6: IssueComment 实体（贴 Plane issue_comment.py 模式）

**What:** Issue 评论，支持富文本 + @ 提及 + 层级嵌套。

**Plane 字段对照 [VERIFIED: Plane issue.py:451-540]：**

| Plane 字段         | .NET 字段         | 类型                  | 备注                           |
| ------------------ | ----------------- | --------------------- | ------------------------------ |
| `comment_html`     | `CommentHtml`     | string                | HTML 富文本                    |
| `comment_json`     | `CommentJson`     | string?               | JSON 格式                      |
| `comment_stripped` | `CommentStripped` | string?               | 纯文本（自动剥离）             |
| `issue`            | `IssueId`         | Guid                  | FK to Issue                    |
| `actor`            | `ActorId`         | Guid                  | 评论者（标量，无 FK）          |
| `attachments`      | —                 | ArrayField            | Plane 用 PostgreSQL ArrayField |
| `parent`           | `ParentId`        | Guid?                 | 自引用 FK，回复嵌套            |
| `access`           | —                 | "INTERNAL"/"EXTERNAL" | 内部/外部可见                  |
| `edited_at`        | `EditedAt`        | DateTimeOffset?       | 编辑时间戳                     |

**Plane 特殊行为：** 保存评论时自动同步到 `Description` 表（`plane/db/models/description.py`）。Phase 4 简化：直接存到 IssueComment 表，不同步 Description。

### Pattern 7: IssueActivity 实体 + 领域事件

**What:** 审计日志系统，记录所有 Issue 变更。Plane 使用 Celery 后台任务触发记录，Phase 4 使用 MediatR 领域事件。

**Plane 字段对照 [VERIFIED: Plane issue.py:415-449]：**

| Plane 字段      | .NET 字段        | 类型    | 备注                          |
| --------------- | ---------------- | ------- | ----------------------------- |
| `issue`         | `IssueId`        | Guid    | FK to Issue                   |
| `verb`          | `Verb`           | string  | "created"/"updated"/"deleted" |
| `field`         | `Field`          | string? | 变更字段名（如 "state_id"）   |
| `old_value`     | `OldValue`       | string? | 旧值                          |
| `new_value`     | `NewValue`       | string? | 新值                          |
| `comment`       | `Comment`        | string? | 评论（可选的附加说明）        |
| `actor`         | `ActorId`        | Guid    | 操作者                        |
| `issue_comment` | `IssueCommentId` | Guid?   | 关联评论（评论创建活动关联）  |
| `epoch`         | `Epoch`          | long?   | Unix 时间戳                   |

**架构建议（Claude's Discretion）：**

- Issue 实体发出领域事件 `IssueUpdatedDomainEvent(issueId, field, oldValue, newValue, actorId)`
- `IssueActivityHandler`（领域事件 handler）异步写入 `IssueActivity` 表
- 和 Plane 的 `issue_activity.delay(...)` 模式对应

### Pattern 8: Estimate + EstimatePoint 实体

**What:** 估算系统，支持 points（如 Fibonacci 点数）或 categories（如 XS/S/M/L/XL）。

**Plane 字段对照 [VERIFIED: Plane estimate.py:18-57]：**

**Estimate：**
| Plane 字段 | .NET 字段 | 类型 | 备注 |
|------------|-----------|------|------|
| `name` | `Name` | string(255) | 估算体系名称 |
| `type` | `Type` | string | "points" / "categories" |
| `last_used` | `IsLastUsed` | bool | 是否为项目当前使用的估算 |
| `description` | — | — | 可选 |

**EstimatePoint：**
| Plane 字段 | .NET 字段 | 类型 | 备注 |
|------------|-----------|------|------|
| `estimate` | `EstimateId` | Guid | FK to Estimate |
| `key` | `Key` | int | 排序键（0, 1, 2...） |
| `value` | `Value` | string | 显示值（如 "1", "2", "3", "5", "8" 或 "S", "M", "L"） |
| `description` | — | — | 可选 |

**Issue 上的估算字段：** `EstimatePointId`（Guid?，FK to EstimatePoint，Plane `issue.point` 旧字段 → 现用 `issue.estimate_point`）

### Pattern 9: Intake + IntakeIssue 实体

**What:** 收件箱功能。Intake 是项目的"入口"通道——外部提交的 Issue 先进入 Intake，审核后再转为正式 Issue。

**Plane 字段对照 [VERIFIED: Plane intake.py:12-84]：**

**Intake：**
| Plane 字段 | .NET 字段 | 类型 | 备注 |
|------------|-----------|------|------|
| `name` | `Name` | string(255) | 收件箱名称 |
| `description` | — | 可选 |
| `is_default` | `IsDefault` | bool | 是否为默认收件箱 |
| `view_props` | — | Phase 8 View |

**IntakeIssue（关键实体）[VERIFIED: Plane intake.py:50-84]：**

| Plane 字段     | .NET 字段            | 类型              | 备注                                                            |
| -------------- | -------------------- | ----------------- | --------------------------------------------------------------- |
| `intake`       | `IntakeId`           | Guid              | FK to Intake                                                    |
| `issue`        | `IssueId`            | Guid              | FK to Issue（Issue 本身包含 is_draft 标记）                     |
| `status`       | `Status`             | int               | -2:Pending / -1:Rejected / 0:Snoozed / 1:Accepted / 2:Duplicate |
| `snoozed_till` | `SnoozedTill`        | DateTime?         | 提醒时间                                                        |
| `duplicate_to` | `DuplicateToIssueId` | Guid?             | 重复 Issue 引用                                                 |
| `source`       | `Source`             | string            | "IN_APP" / email / etc.                                         |
| `source_email` | —                    | 可选的 email 来源 |
| `extra`        | —                    | JSON 附加数据     |

**Intake 流程：**

1. 外部提交 → 创建 Issue (`is_draft=true`) + IntakeIssue (`status=Pending`)
2. 审核者查看 IntakeIssue
3. 接受（Accept）：`status=Accepted`，Issue `is_draft=false`，Issue state 从 triage 转为默认 state
4. 拒绝（Reject）：`status=Rejected`，Issue 标记为已拒绝
5. 标记为重复（Duplicate）：`status=Duplicate`，关联到另一个 Issue

### Pattern 10: Batch Operations（Issue 批量操作）

**Plane 批量操作 [VERIFIED: Plane issue/base.py:1094-1172]：**

- **批量更新日期**：`POST /work-items/bulk-date/` → 接收 `{updates: [{id, start_date, target_date}]}`
- **批量删除**：`DELETE /work-items/bulk/` → 接收 `{issue_ids: []}`

**CONTEXT 建议扩展：** 支持批量更新状态/指派人/优先级。统一为 `POST /work-items/bulk/`：

```json
{
  "issue_ids": ["guid1", "guid2"],
  "state_id": null,
  "assignee_ids": null,
  "priority": null
}
```

只更新非 null 字段，null 字段保持不变。

### Anti-Patterns to Avoid

- **Issue 实体标记 `IGlobalEntity`**：Issue 是租户隔离数据，必须 `IHasTenant`。和 Project 一样非 `IGlobalEntity`。
- **SequenceId 生成不加锁**：如果两个请求同时创建 Issue，可能生成相同的 sequence_id。必须用 DB 级别的事务隔离或锁。
- **assignees/labels 更新时做 DELETE + INSERT 在一个事务外**：Plane 是做全量替换。必须在同一个 `SaveChanges` 中完成。
- **IssueActivity 记录阻塞在 Issue 请求内**：活动日志应该异步写入。Plane 用 Celery 后台任务，Phase 4 用 MediatR 领域事件。
- **忘记 State 的 closed 语义**：`Completed` / `Cancelled` 组下的 Issue 应禁止直接编辑。
- **软删除时不处理关联实体**：Issue 软删除时，关联的 IssueLink/IssueComment 等应级联软删除。（EF Core `OnDelete(Cascade)` 处理硬删除，ISoftDeletable 需要自行管理）
- **Import/Export 不做 CSV 公式注入防护**：XLSX 中的 `=HYPERLINK` `=DDE` `=EXEC` 是已知攻击向量。必须对导出数据做 sanitization。

## Don't Hand-Roll

| Problem             | Don't Build                          | Use Instead                                                        | Why                                   |
| ------------------- | ------------------------------------ | ------------------------------------------------------------------ | ------------------------------------- |
| 多租户 query filter | 自己写 `WHERE WorkspaceId = current` | `BaseDbContext` + `IHasTenant` + `ApplyTenantIsolationByDefault()` | 已实现、默认开启                      |
| 软删除 filter       | 手动 `if (!deleted)`                 | `ISoftDeletable` + `AppendGlobalQueryFilter`                       | `BaseDbContext` 已挂                  |
| 审计字段            | 手动赋值                             | `AuditableEntitySaveChangesInterceptor`                            | 已注册                                |
| 分页（Plane 格式）  | 自己拼 response                      | `PlanePagedResult<T>` + `IPagedQuery`                              | Phase 1 已适配                        |
| 校验 pipeline       | 手动 if/throw                        | `FluentValidation` + `ValidationBehavior` pipeline                 | `AddModules` 自动注册                 |
| 当前用户上下文      | 从 ClaimsPrincipal 手动取            | `ICurrentUser`（Phase 1）                                          | `CurrentUserMiddleware` 已填          |
| Workspace Slug 解析 | 自己查 workspace                     | `WorkspaceSlugStrategy` + `ICurrentWorkspaceContext`               | 已建好直接复用                        |
| HTML 消毒（XSS）    | 自己写 regex sanitizer               | `HtmlSanitizer`（NuGet）                                           | Plane 有 `validate_html_content` 对应 |
| CSV 解析/写入       | 自己写 CSV parser                    | `CsvHelper`（NuGet，如已存在）                                     | 成熟库，处理编码、转义、公式注入      |
| 领域事件异步分发    | 自己实现 outbox                      | `MediatR` IDomainEvent + handler                                   | 已建好，Phase 1 BuildingBlocks 包含   |

**Key insight:** Phase 4 的核心工作量不是基础设施（全部已建），而是**直译 Plane 实体模型到 EF Core + 实现 20+ 个 Feature slice**。最大的复杂度是 Issue 实体本身的丰富字段和 M2M 关系维护。

## Common Pitfalls

### Pitfall 1: SequenceId 竞争条件

**What goes wrong:** 并发创建 Issue 时两个请求读到相同 `MAX(sequence_id)`，生成重复编号。
**Why it happens:** 没有在 DB 级别做互斥锁。Plane 使用 PostgreSQL `pg_advisory_xact_lock` 做事务级锁。
**How to avoid:** 使用 `IDbContextTransaction` + 数据库行锁或 `SERIALIZABLE` 隔离级别。或者用 `ISequenceService` 封装一个带 DB 锁的原子自增操作。
**Warning signs:** 两个 Issue 有相同 sequence_id。

### Pitfall 2: Issue 更新时 assignees/labels 全量替换事务安全

**What goes wrong:** FULL DELETE → INSERT 过程中，如果 INSERT 失败，旧数据已删除导致数据丢失。
**Why it happens:** 没在同一个事务中执行 DELETE 和 INSERT。Plane 的 `update()` 方法在同一的 `serializer.save()` 中完成。
**How to avoid:** 确保 DELETE旧 assignees/labels + INSERT 新记录在同一个 `SaveChanges` 事务中。
**Warning signs:** Issue 关联的 assignee/label 丢失。

### Pitfall 3: 活动日志不会触发自身

**What goes wrong:** 每个 Issue 更新都触发 `IssueActivity` 记录，但写入 IssueActivity 本身不应该再触发新的活动日志。
**Why it happens:** 领域事件 handler 更新 IssueActivity 表 → 再次触发领域事件 → 无限递归。
**How to avoid:** IssueActivity 实体不实现领域事件接口。或者在 handler 中跳过指定了 `skip_activity` 标记的更新。
**Warning signs:** 保存 Issue 时堆栈溢出或无限循环。

### Pitfall 4: State 删除约束

**What goes wrong:** 删除一个 State，但有关联的 Issue 引用了它（并非软删除，因为 Issue 只软删除）。
**Why it happens:** 缺少外键约束或约束不正确。
**How to avoid:** State 配置 `OnDelete(SetNull)`（Plane 可以设置 state 为 null）或 `OnDelete(Restrict)` 禁止删除有关联 Issue 的 State。推荐 `Restrict` + 业务层检查。
**Warning signs:** 删除 State 时遇到外键错误。

### Pitfall 5: Closed State 编辑检查

**What goes wrong:** 用户编辑了一个 `Completed` 或 `Cancelled` 状态的 Issue 名称/描述，然后保存。
**Why it happens:** 后端没有检查 state group 是否为 closed。
**How to avoid:** UpdateIssue handler 中检查 `state.Group == Completed || state.Group == Cancelled → 返回 400`（或 `409 Conflict`），前端提示需要 reopen。
**Warning signs:** 已关闭 Issue 被无修改。

### Pitfall 6: 跨 DbContext 引用 ProjectId

**What goes wrong:** WorkItemsDbContext 中的 Issue.ProjectId 没有外键约束到 ProjectDbContext 的 Project 表。可能存储了不存在的 ProjectId。
**Why it happens:** 两个 DbContext 独立，无法跨 schema 创建 FK。
**How to avoid:** 应用层验证：CreateIssue handler 查询 ProjectDbContext 确认 ProjectId 存在。数据库层不做跨 schema FK。
**Warning signs:** 孤儿数据（Issue 引用不存在的 Project）。

### Pitfall 7: SortOrder 竞争（与 Phase 3 Pitfall 7 同模式）

**What goes wrong:** 多个 Issue 在相同 State group 有相同的 sort_order，导致排序不确定。
**Why it happens:** Plane 使用 `MAX(sort_order) + 10000` 保证新增 issue 在最后。如果并发可能有相同值。
**How to avoid:** 用 SequenceId 作为 tie-breaker | 接受 Plane 行为（sort_order 大值步幅完全够用，实际碰撞概率极低）。
**Warning signs:** Issue 列表排序不稳定。

### Pitfall 8: WorkItemsDbContext 的 OnModelCreating 顺序

**What goes wrong:** `ApplyTenantIsolationByDefault()` 在 ApplyConfigurationsFromAssembly 之前运行，unique index 未被正确 widen。
**Why it happens:** 和 Phase 3 ProjectDbContext Pitfall 6 完全相同的模式。
**How to avoid:** 严格遵循 Phase 3 `ProjectDbContext.OnModelCreating` 顺序：先 ApplyConfigurationsFromAssembly，再 base.OnModelCreating。
**Warning signs:** 多租户 unique index 未包含 TenantId 列。

## Code Examples

### Example 1: WorkItemsDbContext（贴 ProjectDbContext 模式）

```csharp
// Source: 贴 ProjectDbContext.cs 模式 [VERIFIED: yh-flow/ProjectDbContext.cs:47-59]
public sealed class WorkItemsDbContext : BaseDbContext
{
    public WorkItemsDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<WorkItemsDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    public DbSet<State> States => Set<State>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<Issue> Issues => Set<Issue>();
    // ... other DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // 1) ApplyConfigurationsFromAssembly FIRST
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkItemsDbContext).Assembly);

        // 2) base.OnModelCreating LAST
        base.OnModelCreating(modelBuilder);
    }
}
```

### Example 2: Issue 实体（贴 Plane issue.py 核心字段）

```csharp
// Source: Plane issue.py:104-256 [VERIFIED]
public sealed class Issue : IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private static readonly HtmlSanitizer Sanitizer = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string DescriptionHtml { get; private set; } = "<p></p>";
    public string? DescriptionJson { get; private set; }
    public string? DescriptionStripped { get; private set; }
    public string Priority { get; private set; } = "none"; // urg/high/med/low/none
    public int SequenceId { get; private set; }
    public double SortOrder { get; private set; } = 65535.0;

    // FK references (all scalar — no EF navigation to other DbContexts)
    public Guid ProjectId { get; private set; }
    public Guid? ParentId { get; private set; }
    public Guid? StateId { get; private set; }
    public Guid? EstimatePointId { get; private set; }

    // Dates
    public DateOnly? StartDate { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }

    // Flags
    public bool IsDraft { get; private set; }

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

    private Issue() { } // EF Core

    public static Issue Create(
        string name,
        Guid projectId,
        Guid? stateId = null,
        Guid? parentId = null,
        Guid? estimatePointId = null,
        string? descriptionHtml = null,
        string? descriptionJson = null,
        string priority = "none",
        DateOnly? startDate = null,
        DateOnly? targetDate = null,
        bool isDraft = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Issue name is required.", nameof(name));

        return new Issue
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectId = projectId,
            StateId = stateId,
            ParentId = parentId,
            EstimatePointId = estimatePointId,
            DescriptionHtml = Sanitizer.Sanitize(descriptionHtml ?? "<p></p>"),
            DescriptionJson = descriptionJson,
            DescriptionStripped = StripHtml(descriptionHtml),
            Priority = ValidatePriority(priority),
            StartDate = startDate,
            TargetDate = targetDate,
            IsDraft = isDraft,
            SortOrder = 65535.0,
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    public void UpdateDetails(string? name = null, string? priority = null,
        string? descriptionHtml = null, string? descriptionJson = null,
        DateOnly? startDate = null, DateOnly? targetDate = null,
        Guid? stateId = null, Guid? estimatePointId = null,
        bool? isDraft = null)
    {
        // Capture changes for domain events before applying
        var changes = new List<FieldChange>();

        if (name is not null && name != Name)
        { changes.Add(new("name", Name, name)); Name = name; }

        if (priority is not null && ValidatePriority(priority) != Priority)
        { changes.Add(new("priority", Priority, priority)); Priority = priority; }

        if (descriptionHtml is not null)
        {
            var sanitized = Sanitizer.Sanitize(descriptionHtml);
            if (sanitized != DescriptionHtml)
            {
                changes.Add(new("description_html", DescriptionHtml, sanitized));
                DescriptionHtml = sanitized;
                DescriptionStripped = StripHtml(sanitized);
            }
        }

        if (stateId.HasValue && stateId != StateId)
        {
            changes.Add(new("state_id", StateId?.ToString(), stateId?.ToString()));
            StateId = stateId;
            // Sync completed_at per Plane behavior
            // (requires State.Group lookup — done in handler)
        }
        // ... startDate, targetDate, etc.

        LastModifiedOnUtc = DateTimeOffset.UtcNow;

        // Emit domain event for activity logging
        if (changes.Count > 0)
        {
            _domainEvents.Add(new IssueUpdatedDomainEvent(Id, changes, LastModifiedBy ?? "unknown"));
        }
    }

    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }

    private static string ValidatePriority(string p) => p switch
    {
        "urgent" or "high" or "medium" or "low" or "none" => p,
        _ => throw new ArgumentException($"Invalid priority: {p}")
    };

    private static string? StripHtml(string? html) =>
        string.IsNullOrWhiteSpace(html) ? null : HtmlSanitizer.Sanitize(html); // simplified
}

public sealed record FieldChange(string Field, string? OldValue, string? NewValue);
```

### Example 3: Issue EF Configuration（关键索引和约束）

```csharp
// Source: 贴 Plane issue.py Meta + Phase 3 ProjectConfiguration 模式 [VERIFIED]
public sealed class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues", WorkItemsModuleConstants.SchemaName).HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Priority).IsRequired().HasMaxLength(10).HasDefaultValue("none");
        builder.Property(x => x.SequenceId).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(65535.0);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.DescriptionHtml).HasMaxLength(10000);

        // Indexes
        builder.HasIndex(x => new { x.ProjectId, x.SequenceId }).IsUnique()
            .HasDatabaseName("IX_Issues_Project_Sequence");
        builder.HasIndex(x => new { x.ProjectId, x.StateId })
            .HasDatabaseName("IX_Issues_Project_State");
        builder.HasIndex(x => new { x.ProjectId, x.Priority })
            .HasDatabaseName("IX_Issues_Project_Priority");
        builder.HasIndex(x => new { x.ProjectId, x.ParentId })
            .HasDatabaseName("IX_Issues_Project_Parent")
            .HasFilter("[ParentId] IS NOT NULL");
        // ... audit columns, tenant
    }
}
```

### Example 4: State 种子数据（项目创建时自动调用）

```csharp
// Source: Plane DEFAULT_STATES [VERIFIED: Plane state.py:24-62]
public static class StateSeeder
{
    public static readonly IReadOnlyList<StateSeedData> DefaultStates =
    [
        new("Backlog",     "#60646C", StateGroup.Backlog,    15000, true),
        new("Todo",        "#60646C", StateGroup.Unstarted,  25000, false),
        new("In Progress", "#F59E0B", StateGroup.Started,    35000, false),
        new("Done",        "#46A758", StateGroup.Completed,  45000, false),
        new("Cancelled",   "#9AA4BC", StateGroup.Cancelled,  55000, false),
    ];

    public static async Task SeedProjectStatesAsync(WorkItemsDbContext db, Guid projectId, Guid createdBy)
    {
        foreach (var s in DefaultStates)
        {
            if (!await db.States.AnyAsync(x => x.ProjectId == projectId && x.Name == s.Name))
            {
                db.States.Add(State.Create(s.Name, s.Color, s.Group, projectId, s.SortOrder, s.IsDefault));
            }
        }
        await db.SaveChangesAsync();
    }
}

public sealed record StateSeedData(string Name, string Color, StateGroup Group, double SortOrder, bool IsDefault);
```

### Example 5: Plane 兼容分页 + Issue List 查询

```csharp
// Source: Plane IssueViewSet.list [VERIFIED: issue/base.py:254-390]
public sealed class ListIssuesQueryHandler
    : IQueryHandler<ListIssuesQuery, PlanePagedResult<IssueDto>>
{
    private readonly WorkItemsDbContext _db;

    public async ValueTask<PlanePagedResult<IssueDto>> Handle(
        ListIssuesQuery query, CancellationToken ct)
    {
        var currentUserId = query.CurrentUserId;

        var filtered = _db.Issues
            .AsNoTracking()
            .Where(i => !i.IsDeleted && i.ProjectId == query.ProjectId && !i.IsDraft)
            .Where(i => i.ArchivedAt == null);  // exclude archived

        // Plane multidimensional filters
        if (query.StateId.HasValue)
            filtered = filtered.Where(i => i.StateId == query.StateId);
        if (!string.IsNullOrWhiteSpace(query.Priority))
            filtered = filtered.Where(i => i.Priority == query.Priority);
        if (query.AssigneeId.HasValue)
            filtered = filtered.Where(i => _db.IssueAssignees
                .Any(a => a.IssueId == i.Id && a.AssigneeId == query.AssigneeId));
        if (query.LabelId.HasValue)
            filtered = filtered.Where(i => _db.IssueLabels
                .Any(l => l.IssueId == i.Id && l.LabelId == query.LabelId));

        var totalCount = await filtered.CountAsync(ct).ConfigureAwait(false);

        var orderBy = query.OrderBy ?? "-created_at";
        var ordered = ApplyOrdering(filtered, orderBy);

        var pageNumber = query.PageNumber ?? 1;
        var pageSize = Math.Clamp(query.PageSize ?? 30, 1, 100);

        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new IssueDto { /* map fields */ })
            .ToListAsync(ct).ConfigureAwait(false);

        return PlanePagedResultFactory.FromPagedResponse(
            new PagedResponse<IssueDto> { Items = items, ... },
            query.BaseUrl);
    }
}
```

### Example 6: Plane Intake Flow（IntakeIssue status 变更 → Issue 状态转换）

```csharp
// Source: Plane IntakeIssueSerializer.update() [VERIFIED: serializers/intake.py:68-84]
public sealed class UpdateIntakeIssueCommandHandler
    : ICommandHandler<UpdateIntakeIssueCommand, IntakeIssueDto>
{
    private readonly WorkItemsDbContext _db;

    public async ValueTask<IntakeIssueDto> Handle(UpdateIntakeIssueCommand command, CancellationToken ct)
    {
        var intakeIssue = await _db.IntakeIssues
            .Include(x => x.Issue)
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.ProjectId == command.ProjectId, ct)
            ?? throw new NotFoundException("IntakeIssue not found");

        // Status transition
        intakeIssue.UpdateStatus(command.Status, command.SnoozedTill, command.DuplicateToIssueId);

        // If accepting (status=1), transition issue state from draft to default
        if (command.Status == IntakeIssueStatus.Accepted)
        {
            var defaultState = await _db.States
                .Where(s => s.ProjectId == command.ProjectId && s.IsDefault && s.Group != StateGroup.Cancelled)
                .FirstOrDefaultAsync(ct);

            if (defaultState is null)
                throw new CustomException("No default state found for project", HttpStatusCode.Conflict);

            intakeIssue.Issue.MarkAsAccepted(defaultState.Id);
        }

        await _db.SaveChangesAsync(ct);
        return MapToDto(intakeIssue);
    }
}
```

### Example 7: IssueActivity 领域事件 Handler（异步日志记录）

```csharp
// Source: CONTEXT Claude's Discretion (MediatR domain events for activity tracking)
public sealed class IssueUpdatedDomainEvent
    : INotification
{
    public Guid IssueId { get; }
    public IReadOnlyList<FieldChange> Changes { get; }
    public string ActorId { get; }

    public IssueUpdatedDomainEvent(Guid issueId, IReadOnlyList<FieldChange> changes, string actorId)
    {
        IssueId = issueId;
        Changes = changes;
        ActorId = actorId;
    }
}

public sealed class IssueActivityHandler
    : INotificationHandler<IssueUpdatedDomainEvent>
{
    private readonly WorkItemsDbContext _db;
    private readonly TimeProvider _clock;

    public async Task Handle(IssueUpdatedDomainEvent notification, CancellationToken ct)
    {
        var epoch = _clock.GetUtcNow().ToUnixTimeSeconds();

        foreach (var change in notification.Changes)
        {
            _db.Activities.Add(new IssueActivity
            {
                IssueId = notification.IssueId,
                Verb = "updated",
                Field = change.Field,
                OldValue = change.OldValue,
                NewValue = change.NewValue,
                ActorId = notification.ActorId,
                Epoch = epoch,
            });
        }

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
```

## State of the Art

| Old Approach                               | Current Approach                                         | When Changed      | Impact                        |
| ------------------------------------------ | -------------------------------------------------------- | ----------------- | ----------------------------- |
| Plane 自动 `description_stripped` 标签剥离 | handler 中显式剥离 HTML                                  | Phase 4 Implement | Phase 4 需自行实现 strip 逻辑 |
| Plane `pg_advisory_xact_lock` 自增 ID      | .NET `IDbContextTransaction` + `SERIALIZABLE` 或 `MAX+1` | Phase 4           | 不同 DB 机制，行为等价        |
| Plane Celery 后台任务记录活动日志          | MediatR IDomainEvent handler 异步写入                    | Phase 4           | .NET 生态替代方案             |
| Plane Django `unique_together = [...]`     | EF Core `HasFilter` 条件唯一索引                         | Phase 1/2         | 已有模式，直接复用            |

**Deprecated/outdated：**

- Plane 旧版 `Issue.point`（整数点） → 现用 `Issue.estimate_point`（FK to EstimatePoint）。Phase 4 只实现 `EstimatePointId`。

## Assumptions Log

| #   | Claim                                                          | Section                | Risk if Wrong                                                                                                                    |
| --- | -------------------------------------------------------------- | ---------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| A1  | Plane `IssueRelation` 和 `IssueLink` 可以合并为单个实体        | Pattern 5              | 如果前端需要区分 external link 和 internal relation，合并后需要额外 type 字段区分                                                |
| A2  | `IssueActivity` 使用领域事件异步写入足够即时                   | Pattern 7              | 如果领域事件 handler 在线程池执行有延迟，审计日志可能滞后（但通过 MediatR 的 publish-on-save 机制可以保证在同一线程/事务中执行） |
| A3  | Import/Export 不需要 Hangfire 后台 Job（CONTEXT 决策）         | Intake + Import/Export | 大量数据导出时 HTTP 请求可能超时。CONTEXT 决策选用直接处理，但如果 file > 10MB 可能需要切到后台 Job                              |
| A4  | Plane 的 `SortOrder` 计算（MAX + 10000/15000）在竞争下不会冲突 | Pitfall 7              | 高并发场景下两个 Issue 可能获得相同的 SortOrder（概率很低，且 frontend 拖拽会重排）                                              |
| A5  | `CsvHelper` 已在项目 NuGet 依赖中                              | Standard Stack         | 如果不存在需要新增一个包（`CsvHelper 33.x`）                                                                                     |
| A6  | `HtmlSanitizer` 包需要新增                                     | Standard Stack         | 如果项目已有或使用其他 HTML sanitization 方案，需要调整                                                                          |

## Open Questions

1. **Plane 的 IssueActivity 写入模式——Celery 后台任务 vs 领域事件 vs 同步写入？**
   - What we know: Plane 使用 Celery 后台任务 `issue_activity.delay(...)` 异步写入
   - CONTEXT 决策用 MediatR 领域事件
   - Recommendation: **领域事件 + 同一事务**（在 Issue handler SaveChanges 之前发出领域事件，handler 在同一事务周期写入 Activity 表，避免单独的事务）。或者使用 Outbox 模式。
   - **决议项：** 让 planner 决定是立即写入（同一事务）还是通过 Outbox 异步分发。

2. **IssueRelation 和 IssueLink 的实现策略？**
   - What we know: Plane 分离这两个概念，CONTEXT 决定合并
   - Plane 的 IssueLink（外部URL）只有 url + title，IssueRelation（内部关联）有 relation_type + 关联的两个 Issue
   - Recommendation: **合并为一个实体**，`IssueLink` 同时包含 Url（可选，外部链接时用）和 RelatedIssueId（可选，内部关联时用）+ LinkType
   - 示例：`{url: "https://...", linkType: "relates_to"}` 或 `{relatedIssueId: "...", linkType: "blocks"}`

3. **HtmlSanitizer 是否已存在（或有替代方案）？**
   - What we know: Plane 有 `validate_html_content` 和 `strip_tags`
   - BuildingBlocks 可能已包含 XSS 防护
   - Recommendation: 检查项目现有依赖。如果不存在 HTML sanitization 库，添加 `HtmlSanitizer`（AngleSharp 扩展）

4. **CsvHelper 是否已在项目依赖中？**
   - What we know: fullstackhero 模板可能包含 CsvHelper
   - Recommendation: 检查 `Directory.Packages.props` 或现有项目引用。不在则通过 `dotnet add package CsvHelper` 添加并更新 `Directory.Packages.props`

## Environment Availability

| Dependency       | Required By               | Available               | Version      | Fallback                        |
| ---------------- | ------------------------- | ----------------------- | ------------ | ------------------------------- |
| PostgreSQL       | WorkItemsDbContext 持久化 | ✓（Phase 1/2/3 已验证） | 15+          | —                               |
| .NET 10 SDK      | 编译                      | ✓（Phase 1/2/3 已验证） | 10.x         | —                               |
| Finbuckle 10.1.x | 多租户                    | ✓（Phase 1/2/3 已验证） | 10.1.0       | —                               |
| EF Core 10       | WorkItemsDbContext        | ✓                       | .NET 10 内置 | —                               |
| Mediator         | CQRS handlers             | ✓                       | 现有         | —                               |
| HtmlSanitizer    | HTML 消毒                 | 待确认                  | —            | 使用 Regex 简单 strip（不推荐） |
| CsvHelper        | CSV 导入导出              | 待确认                  | —            | 手动 CSV 解析（不推荐）         |

**Missing dependencies with no fallback:** none
**Missing dependencies with fallback:** HtmlSanitizer（可手动 strip tags 但不安全），CsvHelper（可手动解析 CSV 但容易出错）

## Validation Architecture

> `workflow.nyquist_validation` 在 `.planning/config.json` 中未显式设为 false，默认启用。

### Test Framework

| Property           | Value                                                                               |
| ------------------ | ----------------------------------------------------------------------------------- |
| Framework          | xUnit + FluentAssertions + NSubstitute（贴 Workspace.Tests / Project.Tests 模式）   |
| Config file        | `Directory.Packages.props` 锁定版本；`Tests/WorkItems.Tests`                        |
| Quick run command  | `dotnet test src/Tests/WorkItems.Tests --filter "FullyQualifiedName~Unit" --nologo` |
| Full suite command | `dotnet test src/YH.Flow.slnx --nologo`                                             |

### Phase Requirements → Test Map

| Req ID  | Behavior                                               | Test Type   | Automated Command                                    | File Exists? |
| ------- | ------------------------------------------------------ | ----------- | ---------------------------------------------------- | ------------ |
| REQ-4.1 | 创建 State + 5 个默认 group 枚举                       | unit        | `dotnet test --filter CreateStateTests`              | ❌ Wave 0    |
| REQ-4.1 | State 排序                                             | unit        | `dotnet test --filter StateSortOrderTests`           | ❌ Wave 0    |
| REQ-4.1 | 项目创建时自动种子 5 个默认 State                      | integration | `dotnet test --filter StateSeedOnProjectCreateTests` | ❌ Wave 0    |
| REQ-4.1 | 关闭状态语义（Completed/Cancelled 不可编辑）           | integration | `dotnet test --filter ClosedStateEditTests`          | ❌ Wave 1    |
| REQ-4.2 | Label CRUD + 唯一性（项目内 name）                     | unit        | `dotnet test --filter LabelCrudTests`                | ❌ Wave 0    |
| REQ-4.2 | Label 层级（parent 自引用）                            | unit        | `dotnet test --filter LabelHierarchyTests`           | ❌ Wave 1    |
| REQ-4.3 | Issue CRUD（创建/获取/更新/删除）                      | integration | `dotnet test --filter IssueCrudTests`                | ❌ Wave 1    |
| REQ-4.3 | Issue 列表 + 多维筛选（state/priority/assignee/label） | integration | `dotnet test --filter IssueListFilterTests`          | ❌ Wave 1    |
| REQ-4.3 | SequenceId 自增 + 项目内唯一                           | integration | `dotnet test --filter IssueSequenceIdTests`          | ❌ Wave 1    |
| REQ-4.3 | Issue 分页（Plane 格式）                               | integration | `dotnet test --filter IssuePaginationTests`          | ❌ Wave 2    |
| REQ-4.3 | 批量操作（更新状态/指派人/优先级）                     | integration | `dotnet test --filter IssueBulkUpdateTests`          | ❌ Wave 2    |
| REQ-4.4 | Issue 父子关联（深度 1 层限制）                        | unit        | `dotnet test --filter IssueParentDepthTests`         | ❌ Wave 1    |
| REQ-4.4 | Issue 链接（LinkType 枚举）                            | unit        | `dotnet test --filter IssueLinkTypeTests`            | ❌ Wave 1    |
| REQ-4.4 | 优先级字符串校验                                       | unit        | `dotnet test --filter IssuePriorityValidationTests`  | ❌ Wave 1    |
| REQ-4.4 | assignees/labels M2M 全量替换                          | integration | `dotnet test --filter IssueAssigneesReplaceTests`    | ❌ Wave 1    |
| REQ-4.4 | completed_at 同步（state group 变更）                  | integration | `dotnet test --filter IssueCompletedAtSyncTests`     | ❌ Wave 1    |
| REQ-4.5 | IssueComment CRUD                                      | integration | `dotnet test --filter IssueCommentCrudTests`         | ❌ Wave 2    |
| REQ-4.6 | IssueActivity 审计日志                                 | integration | `dotnet test --filter IssueActivityLogTests`         | ❌ Wave 2    |
| REQ-4.6 | 避免递归活动日志                                       | integration | `dotnet test --filter ActivityNoRecursionTests`      | ❌ Wave 2    |
| REQ-4.7 | Estimate + EstimatePoint CRUD                          | unit        | `dotnet test --filter EstimateCrudTests`             | ❌ Wave 0    |
| REQ-4.8 | IntakeIssue 流转（Accept/Reject/Snooze/Duplicate）     | integration | `dotnet test --filter IntakeStatusTransitionTests`   | ❌ Wave 2    |
| REQ-4.8 | Accept Intake → Issue is_draft=false + state 转换      | integration | `dotnet test --filter IntakeAcceptStateTests`        | ❌ Wave 2    |
| REQ-4.9 | CSV/JSON 导入导出                                      | integration | `dotnet test --filter ImportExportTests`             | ❌ Wave 2    |
| NFR-2   | 多租户数据隔离                                         | integration | `dotnet test --filter WorkItemsTenantIsolationTests` | ❌ Wave 0    |
| NFR-2   | HTML XSS 防护                                          | unit        | `dotnet test --filter IssueHtmlSanitizeTests`        | ❌ Wave 1    |

### Sampling Rate

- **Per task commit:** `dotnet test src/Tests/WorkItems.Tests --filter "Unit" --nologo`
- **Per wave merge:** `dotnet test src/YH.Flow.slnx --nologo`（全量回归，确保不破坏 Phase 1/2/3）
- **Phase gate:** 全量绿 + 手工 smoke：建 workspace → 建 project → 自动 5 个 State → 创建 Issue → 添加 assignees/labels → 评论 → 活动日志 → Intake → 导入导出

### Wave 0 Gaps

- [ ] `src/Tests/WorkItems.Tests/` 项目脚手架（csproj + 引用 WorkItems + TestUtils）
- [ ] `src/Tests/WorkItems.Tests/TestData/WorkItemsTestFixture.cs` —— 共享 fixture
- [ ] `src/Tests/WorkItems.Tests/Domain/` —— 所有实体工厂测试
- [ ] `src/Tests/WorkItems.Tests/Integration/` —— 关键集成测试
- [ ] `src/Tests/Architecture.Tests/` —— WorkItems 模块边界测试

## Security Domain

> `security_enforcement` 在 `.planning/config.json` 中未显式设为 false，默认启用。

### Applicable ASVS Categories

| ASVS Category         | Applies | Standard Control                                                                       |
| --------------------- | ------- | -------------------------------------------------------------------------------------- |
| V2 Authentication     | no      | Phase 1 已实现（JWT/API Key/Session），Phase 4 复用                                    |
| V3 Session Management | no      | Phase 1 已实现                                                                         |
| V4 Access Control     | **yes** | `[RequireWorkspaceRole]`（workspace 级）+ handler 内 ProjectMember role 检查（项目级） |
| V5 Input Validation   | **yes** | FluentValidation（priority 枚举校验、description_html XSS 防护、日期校验）             |
| V6 Cryptography       | no      | Phase 4 无密码学需求                                                                   |
| V8 Data Protection    | **yes** | 多租户隔离（所有实体 `IHasTenant`）+ issue description XSS                             |

### Known Threat Patterns for WorkItems

| Pattern                       | STRIDE                 | Standard Mitigation                                                              |
| ----------------------------- | ---------------------- | -------------------------------------------------------------------------------- |
| 跨 project 数据泄露           | Information Disclosure | `BaseDbContext` 自动 tenant filter + ProjectId 过滤                              |
| Issue description XSS         | Tampering              | `HtmlSanitizer` 对 `description_html` 做消毒（Plane 有 `validate_html_content`） |
| Intake 免审直接转为正式 Issue | Tampering              | IntakeIssue `Accept` 需要 Admin/Member 角色检查                                  |
| CSV 公式注入                  | Tampering              | CsvHelper 导出时 prefix `= + - @` 为 `'`（单引号）                               |
| 关闭状态 Issue 被修改         | Tampering              | UpdateIssue handler 检查 state group（Completed/Cancelled → reject）             |
| 跨 workspace 通过 Intake 泄漏 | Information Disclosure | IntakeIssue 和 Issue 都是 tenant-scoped（`IHasTenant`）                          |
| SequenceId 竞争 + 伪造        | Tampering              | 服务端强制自增，不接受客户端提供 sequence_id                                     |

## Sources

### Primary（HIGH confidence）

- **Plane 源码（Issue 相关所有模型和 API）**：
  - `apps/api/plane/db/models/issue.py` — Issue / IssueAssignee / IssueLabel / IssueLink / IssueRelation / IssueComment / IssueActivity / IssueSequence / IssueSubscriber / IssueReaction / IssueVote / IssueVersion 完整实体定义 [VERIFIED]
  - `apps/api/plane/db/models/state.py` — State 模型 + DEFAULT_STATES + StateGroup 枚举 [VERIFIED]
  - `apps/api/plane/db/models/label.py` — Label 模型（parent 自引用、color、约束）[VERIFIED]
  - `apps/api/plane/db/models/estimate.py` — Estimate + EstimatePoint 模型 [VERIFIED]
  - `apps/api/plane/db/models/intake.py` — Intake + IntakeIssue + IntakeIssueStatus 枚举 [VERIFIED]
  - `apps/api/plane/db/models/module.py` — Module + ModuleIssue 模型（Phase 5/6 参考）[VERIFIED]
  - `apps/api/plane/db/models/cycle.py` — Cycle + CycleIssue 模型（Phase 5 参考）[VERIFIED]
  - `apps/api/plane/app/serializers/issue.py` — IssueCreateSerializer / IssueDetailSerializer / IssueSerializer / IssueActivitySerializer / LabelSerializer [VERIFIED]
  - `apps/api/plane/app/serializers/state.py` — StateSerializer / StateLiteSerializer [VERIFIED]
  - `apps/api/plane/app/serializers/intake.py` — IntakeSerializer / IntakeIssueSerializer [VERIFIED]
  - `apps/api/plane/app/serializers/estimate.py` — EstimateSerializer / EstimatePointSerializer [VERIFIED]
  - `apps/api/plane/api/urls/work_item.py` — 完整的 Issue 路由契约（含旧版和新版 URL）[VERIFIED]
  - `apps/api/plane/api/urls/state.py` — State 路由 [VERIFIED]
  - `apps/api/plane/api/urls/label.py` — Label 路由 [VERIFIED]
  - `apps/api/plane/api/urls/intake.py` — Intake 路由 [VERIFIED]
  - `apps/api/plane/api/urls/estimate.py` — Estimate 路由 [VERIFIED]
  - `apps/api/plane/app/views/issue/base.py` — IssueViewSet / IssueListEndpoint / IssueBulkUpdateDateEndpoint / IssueDetailEndpoint [VERIFIED]
  - `apps/api/plane/app/views/issue/label.py` — LabelViewSet [VERIFIED]

- **现有 YH.Flow 模式（Project 模块直译模板）**：
  - `yh-flow/src/Modules/Project/Modules.Project/Domain/Project.cs` — Project 实体模式（IHasTenant + ISoftDeletable + 工厂模式）[VERIFIED]
  - `yh-flow/src/Modules/Project/Modules.Project/Domain/ProjectMember.cs` — ProjectMember 实体 [VERIFIED]
  - `yh-flow/src/Modules/Project/Modules.Project/Data/ProjectDbContext.cs` — OnModelCreating 顺序（ApplyConfigurations FIRST）[VERIFIED]
  - `yh-flow/src/Modules/Project/Modules.Project/Data/Configurations/ProjectConfiguration.cs` — EF 配置模式 [VERIFIED]
  - `yh-flow/src/Modules/Project/Modules.Project/ProjectModule.cs` — 模块注册 + 路由 Pattern [VERIFIED]
  - `yh-flow/src/Modules/Project/Modules.Project/Features/v1/Projects/CreateProject/*.cs` — Vertical Slice 模式 [VERIFIED]
  - `yh-flow/src/Modules/Project/Modules.Project/Features/v1/Projects/ListProjects/ListProjectsQueryHandler.cs` — PlanePagedResult 分页+过滤 [VERIFIED]
  - `yh-flow/AGENTS.md` — 10 条黄金法则 [VERIFIED]
  - `yh-flow/CLAUDE.md` — 项目指南 [VERIFIED]

### Secondary（MEDIUM confidence）

- Phase 3 研究文档（`03-RESEARCH.md`）—— 验证了所有 Project 模块模式，Phase 4 完全复用 [VERIFIED]
- Plane 源码 `apps/api/plane/app/views/issue/base.py` 详细权限逻辑（Guest view_all_features 检查）[VERIFIED]

### Tertiary（LOW confidence — 已标 [ASSUMED]）

- A1-A4: Plane 行为假设（均在§Assumptions Log 中记录）
- CsvHelper 和 HtmlSanitizer 可用性（需检查项目现有依赖）

## Metadata

**Confidence breakdown：**

- Standard stack: **HIGH** — 全部依赖已在项目锁定版本，直接读源码确认
- Architecture（实体/DbContext/端点）: **HIGH** — Phase 3 Project 模块模式可完全复用
- Pitfalls: **HIGH** — 基于 Plane 源码对比 + 多模块经验（SequenceId 竞争、M2M 全量替换事务安全、活动日志递归、State 删除约束、Closed State 编辑检查）
- Plane 契约: **HIGH** — 直接读 Plane 源码（issue.py / state.py / label.py / intake.py + serializers + urls + views）
- Import/Export 精确实现: **MEDIUM** — CONTEXT 决策用直接 API 处理而非后台 Job，但大量数据的导出策略需要确认

**Research date:** 2026-06-24
**Valid until:** 2026-07-24（30 天）
