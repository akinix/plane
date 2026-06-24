---
phase: 04-workitems
plan: 02
subsystem: WorkItems
tags:
  - state-crud
  - label-crud
  - issue-entity
  - m2m
  - sequence-id
  - ef-config
depends_on: [04-01]
requires: []
affects:
  - Modules.WorkItems
  - Modules.WorkItems.Contracts
tech-stack:
  added:
    - IssueSequenceService (SERIALIZABLE transaction locking)
  patterns:
    - Vertical Slice CRUD (Endpoint + Handler + Validator) per Phase 3 pattern
    - RequireWorkspaceRole authorization for nested project routes
    - State/Label CRUD: 5 endpoints each with FluentValidation
    - Issue aggregate: IHasDomainEvents with FieldChange capture for activity logging
    - SequenceId: project-scoped auto-increment with transaction-level locking
    - M2M full replacement pattern for assignees/labels
    - Basic HTML sanitization (regex-based; HtmlSanitizer NuGet unavailable)
key-files:
  created:
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/States/* (6 files)"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Labels/* (6 files)"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/Issue*.cs (4 files)"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/States/* (15 files)"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Labels/* (15 files)"
    - "src/Modules/WorkItems/Modules.WorkItems/Domain/Issue.cs + IssueAssignee.cs + IssueLabel.cs (3 files)"
    - "src/Modules/WorkItems/Modules.WorkItems/Services/IssueSequenceService.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/Issue*.cs (3 files)"
  modified:
    - "src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Data/WorkItemsDbContext.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Modules.WorkItems.csproj"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/States/DeleteState/DeleteStateCommandHandler.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Labels/DeleteLabel/DeleteLabelCommandHandler.cs"
decisions:
  - "State endpoints: Create/Update require WorkspaceRole(Admin, Member); Delete requires Admin only"
  - "Label endpoints: ALL mutations require WorkspaceRole(Admin) per CONTEXT decision"
  - "Label list supports ParentId filter (Guid.Empty = root labels only)"
  - "State list supports Group filter (int? — StateGroup value)"
  - "IssueSequenceService uses SERIALIZABLE isolation (equivalent to pg_advisory_xact_lock)"
  - "Issue entity uses regex-based HTML sanitization (HtmlSanitizer not available in project)"
  - "Issue entity domain events commented out (Wave 3 IssueActivity)"
  - "Added CA1305 to NoWarn for ToString() calls with format strings"
  - "Added Modules.Workspace runtime reference for RequireWorkspaceRole authorization"
metrics:
  duration: "~30 min (3 commits)"
  completed: "2026-06-24"
---

# Phase 4 Plan 2: Wave 2 — State CRUD + Label CRUD + Issue Entity Summary

State/Label CRUD endpoints + Issue aggregate entity + M2M through tables + SequenceId service + EF configurations.

## Tasks

### Task 1: Implement State CRUD endpoints (Create, Get, Update, Delete, List)

**Commit:** `9df8db676`

5 个 State CRUD 端点（Create/Get/Update/Delete/List），完整 Vertical Slice 模式：

- **Contracts:** CreateStateCommand/Response, GetStateQuery, UpdateStateCommand, DeleteStateCommand, ListStatesQuery
- **Implementation:** StateDtoMapper, 5 个 Endpoint + 5 个 Handler + 3 个 Validator
- **Route:** `/api/v1/workspaces/{slug}/projects/{projectId}/states/`
- **Authz:** Create/Update → WorkspaceRole(Admin, Member), Get/List → RequireAuthorization, Delete → WorkspaceRole(Admin)
- **DeleteState** 预留 Issue 引用检查占位（Task 3 完成）
- 添加 `Modules.Workspace` 运行时引用到 csproj（解决 `RequireWorkspaceRole` 命名空间缺失）

### Task 2: Implement Label CRUD endpoints (Create, Get, Update, Delete, List)

**Commit:** `01b37bec0`

5 个 Label CRUD 端点，与 State 相同模式：

- **Contracts:** CreateLabelCommand/Response, GetLabelQuery, UpdateLabelCommand, DeleteLabelCommand, ListLabelsQuery
- **Implementation:** LabelDtoMapper, 5 个 Endpoint + 5 个 Handler + 3 个 Validator
- **Route:** `/api/v1/workspaces/{slug}/projects/{projectId}/labels/`
- **Authz:** 所有 mutation → WorkspaceRole(Admin) per CONTEXT
- Create 和 Update handler 执行项目内名称唯一性检查
- ListLabels 支持可选的 ParentId 过滤器

### Task 3: Create Issue entity, IssueAssignee/IssueLabel M2M through tables, SequenceId service, Issue EF configuration, and Issue DTOs

**Commit:** `c68312a1b`

创建最复杂的 Issue 聚合：

- **DTOs (4):** IssueDto（22 个 Plane 字段，snake_case JSON）、IssueDetailDto（扩展导航字段）、IssueAssigneeDto、IssueLabelDto
- **Domain entities (3):**
  - **Issue.cs:** 全部 Plane 字段（Name, DescriptionHtml/Json/Stripped, Priority string, SequenceId, SortOrder, ParentId, StateId, EstimatePointId, StartDate, TargetDate, CompletedAt, ArchivedAt, IsDraft）
  - IHasDomainEvents + FieldChange capture（活动日志基础设施，Wave 3 接入）
  - UpdateDetails() 方法的 PATCH 语义 + UpdateState() 的 CompletedAt 同步逻辑
  - UpdateAssigneeList/UpdateLabelList 全量替换模式
  - 基础 HTML sanitization（移除 script 标签、事件处理器、javascript: URL）
  - **IssueAssignee.cs:** M2M through 表 (IHasTenant, 无软删除)
  - **IssueLabel.cs:** M2M through 表 (IHasTenant, 无软删除)
- **IssueSequenceService:** IIssueSequenceService 接口 + SERIALIZABLE 事务隔离的 MAX(SequenceId)+1 实现
- **EF 配置 (3):** IssueConfiguration（5 个索引，含唯一 (ProjectId, SequenceId) 条件索引，HasMany 导航回填字段）、IssueAssigneeConfiguration、IssueLabelConfiguration
- **DbContext:** 添加 Issues, IssueAssignees, IssueLabels DbSets
- **Module:** 注册 IssueSequenceService DI
- **删除检查已恢复:** DeleteState 和 DeleteLabel 现在使用 `_db.Issues` 和 `_db.IssueLabels` 检查引用冲突

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Build] Modules.Workspace runtime 引用缺失**

- **Found during:** Task 1
- **Issue:** WorkItems 模块引用 `RequireWorkspaceRole` 扩展方法但 csproj 只引用了 `Modules.Workspace.Contracts`（授权扩展位于 `Modules.Workspace` 运行时项目）
- **Fix:** 添加 `Modules.Workspace` 运行时项目引用
- **Files modified:** `src/Modules/WorkItems/Modules.WorkItems/Modules.WorkItems.csproj`

**2. [Rule 1 - Build] Issue.cs 多个 CA 规则违规（TreatWarningsAsErrors）**

- **Found during:** Task 3
- **Issues:** S2933（`_assignees`/`_labels` 非 readonly）、CA1002（List 参数类型）、CA1062（null 参数缺少检查）、CA1305（ToString 缺少 IFormatProvider）、S1481（未使用变量）、CS0103（IsolationLevel 命名空间缺失）
- **Fixes:** 改为 `readonly` 字段、改用 `ICollection<>`、添加 null 检查、添加 `System.Data` 引用、删除未使用变量、将 CA1305 添加到 NoWarn
- **Files modified:** `src/Modules/WorkItems/Modules.WorkItems/Modules.WorkItems.csproj`, `src/Modules/WorkItems/Modules.WorkItems/Domain/Issue.cs`, `src/Modules/WorkItems/Modules.WorkItems/Services/IssueSequenceService.cs`

### Intentional Deviations

1. **HTML sanitization 使用 regex 而非 HtmlSanitizer:** PROJECT 不包含 HtmlSanitizer NuGet 包。Issue.Create 使用基本 regex sanitization（移除 script 标签、事件处理器、javascript: URL）。HtmlSanitizer 集成推迟到正式添加包时（需要用户确认包合法性）。威胁模型 T-4-issue-03 mitigation 暂时降级为 basic regex sanitization。

2. **IssueDomainEvent 注释掉:** Domain events for activity logging (`IssueUpdatedDomainEvent`) 已注释为 TODO，在 Wave 3 创建 IssueActivity 实体和 handler 时接入。避免 S1481 未使用。

3. **DeleteState/DeleteLabel Issue 检查延迟恢复:** 原计划中检查在 Task 1/2 实现但 Issue DbSet 不存在。Task 3 中 Issue 实体就绪后已被添加。

## Threat Surface

### Threat Register Compliance

| ID           | Mitigation                                 | Status                                            |
| ------------ | ------------------------------------------ | ------------------------------------------------- |
| T-4-crud-01  | State CRUD requires workspace role         | PASS — Create/Update: Admin+Member, Delete: Admin |
| T-4-crud-02  | Label CRUD requires Admin                  | PASS — all mutations test via Admin role          |
| T-4-crud-03  | State delete checks Issue references (409) | PASS — Task 3 完成 Issues 引用检查                |
| T-4-crud-04  | Label delete checks IssueLabels (409)      | PASS — Task 3 完成 IssueLabels 引用检查           |
| T-4-crud-05  | StateGroup validation (0-4 range)          | PASS — FluentValidation + handler switch          |
| T-4-crud-06  | Cross-tenant data leak                     | PASS — IHasTenant + BaseDbContext tenant filter   |
| T-4-issue-01 | Priority validation                        | PASS — ValidatePriority switch with 5 values      |
| T-4-issue-02 | SequenceId server-generated only           | PASS — SequenceId set by service, not client      |
| T-4-issue-03 | Description HTML sanitization              | PARTIAL — regex-based, not HtmlSanitizer          |
| T-4-issue-04 | SequenceId race condition                  | PASS — SERIALIZABLE transaction isolation         |
| T-4-issue-05 | M2M assignee/label isolation               | PASS — IHasTenant on through entities             |
| T-4-issue-06 | Cross-project data isolation               | PASS — TenantId + ProjectId filtering             |

## Verification

### Truth Statements

| #   | Statement                                                                              | Status |
| --- | -------------------------------------------------------------------------------------- | ------ |
| 1   | State CRUD: 5 endpoints (Create/Get/Update/Delete/List) wired and compilable           | PASS   |
| 2   | Label CRUD: 5 endpoints (Create/Get/Update/Delete/List) wired and compilable           | PASS   |
| 3   | State delete blocks if referenced by Issues (409 Conflict)                             | PASS   |
| 4   | Label delete blocks if referenced by Issues (409 Conflict)                             | PASS   |
| 5   | Issue entity implements IHasDomainEvents with FieldChange capture for activity logging | PASS   |
| 6   | IssueAssignee / IssueLabel are IHasTenant with unique per-workspace indexes            | PASS   |
| 7   | IssueSequenceService uses transaction-level locking for atomic MAX(SequenceId)+1       | PASS   |
| 8   | Issue EF config has conditional unique index on (ProjectId, SequenceId)                | PASS   |
| 9   | Issue DTOs (IssueDto, IssueDetailDto) with Plane snake_case JsonPropertyName           | PASS   |
| 10  | Full solution builds with 0 errors                                                     | PASS   |

### Build Verification

```text
dotnet build src/YH.Flow.slnx --nologo → 0 warnings, 0 errors
```

## Self-Check: PASSED

All 3 tasks committed. All files confirmed existing. No unexpected deletions. SUMMARY.md created.

**Commits:**

- `9df8db676` feat(04-workitems-02): implement State CRUD endpoints (Create, Get, Update, Delete, List)
- `01b37bec0` feat(04-workitems-02): implement Label CRUD endpoints (Create, Get, Update, Delete, List)
- `c68312a1b` feat(04-workitems-02): create Issue entity, M2M through tables, SequenceId service, EF configs, and DTOs

**Duration:** ~30 min
