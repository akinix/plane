---
phase: 12-analytics
plan: 02
subsystem: api
tags: analytics, real-time-queries, ef-core, group-by, state-grouping

requires:
  - phase: 12-analytics_01
    provides: Analytics DTOs (AnalyticsOverviewDto, WorkItemStatsDto, ProjectStatsDto, AssigneeStatsDto)

provides:
  - AnalyticsQueryService (5 real-time aggregation methods with date/project filtering)
  - GET /analytics/ (tab=overview|work-items) — workspace-level overview and state distribution
  - GET /analytics/stats/ — workspace stats grouped by project
  - GET /projects/{projectId}/analytics/ — project-level state distribution
  - GET /projects/{projectId}/analytics/stats/ — project stats grouped by assignee

affects: []

tech-stack:
  added: []
  patterns:
    - Explicit LEFT JOIN with States table for state-group aggregation (Issue entity lacks State navigation property)
    - Private IssueStateView projection class for composable EF Core queries (avoids anonymous type limitations)
    - Date filter parsing supporting predefined ranges (this_month/last_7_days/last_30_days/last_3_months) and custom (YYYY-MM-DD)
    - IRequest<IResult> pattern for polymorphic endpoint responses (tab-based routing)

key-files:
  created:
    - Services/IAnalyticsQueryService.cs — 5-method interface
    - Services/AnalyticsQueryService.cs — full implementation with explicit joins, date filtering, named projection
    - Features/v1/Overview/GetWorkspaceAnalytics/*.cs — 3 files (Query, Handler, Endpoint)
    - Features/v1/Overview/GetProjectAnalytics/*.cs — 3 files
    - Features/v1/Stats/GetWorkspaceStats/*.cs — 3 files
    - Features/v1/Stats/GetProjectStats/*.cs — 3 files
  modified:
    - AnalyticsModule.cs — service registration + 4 endpoint group registrations

key-decisions:
  - "Explicit LEFT JOIN with States table instead of Issue.State navigation property — Issue entity lacks State navigation; adding would require modifying WorkItems module entity"
  - "Group by ProjectId scalar for project stats (no Project navigation, cross-module FK avoidance)"
  - "Two-step assignee stats: EF Core GROUP BY with string AssigneeId, then in-memory Guid.TryParse conversion (expression tree limitation)"
  - "Named projection class IssueStateView for composable queries (C# anonymous types can't be returned from methods)"
  - "Route structure: /workspaces/{slug}/analytics/ and /workspaces/{slug}/projects/{projectId}/analytics/ per Phase 12 CONTEXT decision"

duration: 31min
completed: 2026-06-26
---

# Phase 12 Plan 02: Analytics Overview + Stats Endpoints

**实现 Analytics 的核心聚合查询：Workspace/Project Overview + Stats 端点，包含实时 COUNT/GROUP BY 聚合和日期/项目过滤。**

## Performance

- **Duration:** 31 min
- **Completed:** 2026-06-26
- **Tasks:** 3
- **Files modified:** 16 (14 new + 1 modified + 1 reused stub modifications)

## Task Commits

| #   | Name                                                                    | Type | Hash        |
| --- | ----------------------------------------------------------------------- | ---- | ----------- |
| 1   | Create AnalyticsQueryService with shared query logic and filter parsing | feat | `d4db41fc4` |
| 2   | Create Workspace Overview + Project Overview endpoints                  | feat | `cc66b0dd0` |
| 3   | Create Workspace Stats + Project Stats endpoints                        | feat | `938d09265` |

## Accomplishments

### Task 1: AnalyticsQueryService

Created `IAnalyticsQueryService` 接口和 `AnalyticsQueryService` 实现，包含 5 个聚合查询方法：

- `GetWorkspaceOverviewAsync` — 工作区总览计数（TotalWorkItems, TotalCycles, TotalModules），支持 project_ids 和日期过滤
- `GetWorkItemStatsAsync` — 按 state group（Backlog/Unstarted/Started/Completed/Cancelled）分类的 Issue 统计，支持多项目过滤
- `GetProjectWorkItemStatsAsync` — 项目级 Issue 状态分布（限定单个 ProjectId）
- `GetProjectGroupedStatsAsync` — 按项目分组的工作区 Issue 统计，返回每个项目的状态分布
- `GetAssigneeGroupedStatsAsync` — 按负责人分组的项目 Issue 统计，返回每个负责人的状态分布

关键实现细节：

- **Explicit JOIN with States table** — Issue 实体没有 `State` 导航属性，所有 state group 聚合使用显式 LEFT JOIN
- **Date filter parsing** — 支持 `this_month`, `last_7_days`, `last_30_days`, `last_3_months`, `custom`（YYYY-MM-DD 格式）
- **IssueStateView 投影类** — 轻量级标量投影，避免匿名类型的方法签名限制，确保 EF Core 可翻译
- **StateGroupCount 命名类型** — LINQ GroupBy 结果使用命名类型而非匿名类型，避免辅助方法签名问题
- 服务通过 `builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>()` 注册

### Task 2: Overview 端点

创建了两个 Overvew 端点：

1. **GET /api/v1/workspaces/{slug}/analytics/** — 工作区分析端点
   - `tab=overview`: 返回 `AnalyticsOverviewDto`（总计数）
   - `tab=work-items`: 返回 `WorkItemStatsDto`（状态分布）
   - 查询参数: `date_filter`, `start_date`, `end_date`, `project_ids`
   - 使用 `IRequest<IResult>` 模式实现多态响应

2. **GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/** — 项目分析端点
   - 返回 `WorkItemStatsDto`（项目级 Issue 状态分布）
   - 查询参数: `date_filter`, `start_date`, `end_date`

### Task 3: Stats 端点

创建了两个 Stats 端点：

1. **GET /api/v1/workspaces/{slug}/analytics/stats/** — 工作区统计端点
   - 按项目分组的 Issue 统计，返回 `List<ProjectStatsDto>`
   - 查询参数: `type=work-items`, `date_filter`, `start_date`, `end_date`, `project_ids`
   - 按 ProjectId 分组（无 Project 导航属性，Group 通过 ProjectId 标量字段）

2. **GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/stats/** — 项目统计端点
   - 按负责人分组的 Issue 统计，返回 `List<AssigneeStatsDto>`
   - 查询参数: `type=work-items`, `date_filter`, `start_date`, `end_date`
   - 两阶段处理：EF Core 按字符串 AssigneeId 分组，内存中转换为 Guid?

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Explicit LEFT JOIN with States table instead of Issue.State navigation property**

- **Found during:** Task 1 (代码编写)
- **Issue:** 计划假设 `i.State.Group` 导航属性可用，但 Issue 实体没有 `State` 导航属性（只有 `StateId` 标量 FK）。添加导航属性需要修改 WorkItems 模块的 Issue 实体和配置，属于跨模块变更。
- **Fix:** 使用显式 LEFT JOIN 语法替代导航属性访问。创建 `IssueStateView` 投影类用于可组合的查询。
- **Files modified:** `Services/AnalyticsQueryService.cs` (未创建 IssueStateView 投影类和显式 JOIN 查询)
- **Verification:** 构建通过，查询语义等价
- **Committed in:** `d4db41fc4` (Task 1)

**2. [Rule 3 - Blocking] No Project navigation on Issue entity**

- **Found during:** Task 1 (代码编写)
- **Issue:** 计划通过 `i.Project.Name` 获取项目名称，但 Issue 没有 `Project` 导航属性（跨模块 FK 约束避免原则）。
- **Fix:** 按 `ProjectId` 标量字段分组，`ProjectName` 返回空字符串。客户端在需要时可以补全。
- **Files modified:** `Services/AnalyticsQueryService.cs`
- **Verification:** 构建通过
- **Committed in:** `d4db41fc4` (Task 1)

**3. [Rule 3 - Blocking] IssueAssignee lacks Issue/Member navigation properties**

- **Found during:** Task 1 (代码编写)
- **Issue:** 计划通过 `ia.Issue` 和 `ia.Member.DisplayName` 获取关联数据，但 IssueAssignee 实体只有标量字段（`IssueId`, `AssignbyId`）。
- **Fix:** 使用显式 JOIN 语法关联 Issues/IssueAssignees/States 三表。DisplayName 返回 null 并在内存中处理 Guid 解析。
- **Files modified:** `Services/AnalyticsQueryService.cs`
- **Verification:** 构建通过
- **Committed in:** `d4db41fc4` (Task 1)

**4. [Rule 3 - Blocking] CS8198 expression tree limitation with Guid.TryParse out var**

- **Found during:** Task 1 (构建错误)
- **Issue:** `Guid.TryParse(g.Key, out var id)` 在 EF Core LINQ 表达式树中不受支持（CS8198）。
- **Fix:** 将 Assignee stats 查询拆分为两步：EF Core 按字符串 AssigneeId 分组的聚合查询（.ToListAsync 物化），然后内存中 .Select 转换并调用 Guid.TryParse。
- **Files modified:** `Services/AnalyticsQueryService.cs`
- **Verification:** 构建通过
- **Committed in:** `d4db41fc4` (Task 1)

**5. [Rule 3 - Blocking] CA1849 Task.Result synchronous blocking**

- **Found during:** Task 1 (构建错误)
- **Issue:** 使用 `Task.WhenAll()` + `.Result` 属性同步阻塞（CA1849）。
- **Fix:** 改为顺序 `await` 三个计数查询。
- **Files modified:** `Services/AnalyticsQueryService.cs`
- **Verification:** 构建通过
- **Committed in:** `d4db41fc4` (Task 1)

---

**Total deviations:** 5 auto-fixed (all Rule 3 - blocking)
**Impact on plan:** 所有修复均为正确编译和运行的必要条件。核心查询逻辑保持一致。

## Files Created/Modified

### Services (NEW)

| 文件                                 | 行数 | 提供                                                             |
| ------------------------------------ | ---- | ---------------------------------------------------------------- |
| `Services/IAnalyticsQueryService.cs` | 46   | 5-method 接口定义                                                |
| `Services/AnalyticsQueryService.cs`  | 341  | 完整实现：日期过滤解析、显式 JOIN 聚合、5 个查询方法、内部投影类 |

### Workspace Overview 端点 (NEW)

| 文件                                                                              | 提供                                                                               |
| --------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `Features/v1/Overview/GetWorkspaceAnalytics/GetWorkspaceAnalyticsQuery.cs`        | `IRequest<IResult>` query (Tab, Slug, DateFilter, StartDate, EndDate, ProjectIds)  |
| `Features/v1/Overview/GetWorkspaceAnalytics/GetWorkspaceAnalyticsQueryHandler.cs` | Tab 路由：overview → GetWorkspaceOverviewAsync, work-items → GetWorkItemStatsAsync |
| `Features/v1/Overview/GetWorkspaceAnalytics/GetWorkspaceAnalyticsEndpoint.cs`     | `[HttpGet] /` with RequireWorkspaceRole(Admin, Member)                             |

### Project Overview 端点 (NEW)

| 文件                                                                          | 提供                                                   |
| ----------------------------------------------------------------------------- | ------------------------------------------------------ |
| `Features/v1/Overview/GetProjectAnalytics/GetProjectAnalyticsQuery.cs`        | `IRequest<IResult>` query                              |
| `Features/v1/Overview/GetProjectAnalytics/GetProjectAnalyticsQueryHandler.cs` | 调用 `GetProjectWorkItemStatsAsync`                    |
| `Features/v1/Overview/GetProjectAnalytics/GetProjectAnalyticsEndpoint.cs`     | `[HttpGet] /` with RequireWorkspaceRole(Admin, Member) |

### Workspace Stats 端点 (NEW)

| 文件                                                                   | 提供                                                        |
| ---------------------------------------------------------------------- | ----------------------------------------------------------- |
| `Features/v1/Stats/GetWorkspaceStats/GetWorkspaceStatsQuery.cs`        | `IRequest<List<ProjectStatsDto>>` query                     |
| `Features/v1/Stats/GetWorkspaceStats/GetWorkspaceStatsQueryHandler.cs` | 调用 `GetProjectGroupedStatsAsync`                          |
| `Features/v1/Stats/GetWorkspaceStats/GetWorkspaceStatsEndpoint.cs`     | `[HttpGet] /stats` with RequireWorkspaceRole(Admin, Member) |

### Project Stats 端点 (NEW)

| 文件                                                               | 提供                                                        |
| ------------------------------------------------------------------ | ----------------------------------------------------------- |
| `Features/v1/Stats/GetProjectStats/GetProjectStatsQuery.cs`        | `IRequest<List<AssigneeStatsDto>>` query                    |
| `Features/v1/Stats/GetProjectStats/GetProjectStatsQueryHandler.cs` | 调用 `GetAssigneeGroupedStatsAsync`                         |
| `Features/v1/Stats/GetProjectStats/GetProjectStatsEndpoint.cs`     | `[HttpGet] /stats` with RequireWorkspaceRole(Admin, Member) |

### Module (MODIFIED)

| 文件                 | 变更                                                                              |
| -------------------- | --------------------------------------------------------------------------------- |
| `AnalyticsModule.cs` | 添加 `IAnalyticsQueryService` DI 注册 + 4 个端点组注册（2 workspace + 2 project） |

## Verification

### Routes 注册验证

| 端点                  | 路径                                                              | 方法 |
| --------------------- | ----------------------------------------------------------------- | ---- |
| GetWorkspaceAnalytics | `/api/v1/workspaces/{slug}/analytics/`                            | GET  |
| GetWorkspaceStats     | `/api/v1/workspaces/{slug}/analytics/stats/`                      | GET  |
| GetProjectAnalytics   | `/api/v1/workspaces/{slug}/projects/{projectId}/analytics/`       | GET  |
| GetProjectStats       | `/api/v1/workspaces/{slug}/projects/{projectId}/analytics/stats/` | GET  |

### 构建验证

- `dotnet build src/YH.Flow.slnx` — **0 错误 0 警告**
- 包括 Modules.Analytics、YH.Flow.Api、YH.Flow.DbMigrator 等 57 个项目全部编译通过

### 授权验证

- 所有 4 个端点均使用 `.RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)` 授权

## Known Stubs

- **ProjectStatsDto.ProjectName** — 返回空字符串。Issue 实体没有跨模块 Project 导航属性，无法在聚合查询中获取项目名称。如需填充，客户端或 Wave 3/4 可增加查询。
- **AssigneeStatsDto.DisplayName** — 返回 null。IssueAssignee 没有 Member 导航属性，无法在聚合查询中获取显示名称。同上。
- **AnalyticsOverviewDto.TotalUsers/TotalAdmins/TotalMembers/TotalGuests/TotalProjects** — 返回默认值（0）。计划明确说明这些统计需通过 Handler 层的其他 DbContext（Workspace/Identity）补充，AnalyticsQueryService 只处理 WorkItemsDbContext 的 Issue/Cycle/Module 查询。

## Deviations from Plan Summary

| #   | Type   | Description                                          | Task | Commit    |
| --- | ------ | ---------------------------------------------------- | ---- | --------- |
| 1   | Rule 3 | Issue 缺少 State 导航属性 → 显式 JOIN                | 1    | d4db41fc4 |
| 2   | Rule 3 | Issue 缺少 Project 导航属性 → ProjectId 标量分组     | 1    | d4db41fc4 |
| 3   | Rule 3 | IssueAssignee 缺少 Issue/Member 导航属性 → 显式 JOIN | 1    | d4db41fc4 |
| 4   | Rule 3 | 表达式树不支持 `out var` → 两阶段处理                | 1    | d4db41fc4 |
| 5   | Rule 3 | Task.Result 同步阻塞 → 改为 await                    | 1    | d4db41fc4 |

## Self-Check: PASSED

- [x] `Services/IAnalyticsQueryService.cs` — 存在
- [x] `Services/AnalyticsQueryService.cs` — 存在，包含 5 个查询方法
- [x] `Features/v1/Overview/GetWorkspaceAnalytics/*.cs` — 3 个文件存在
- [x] `Features/v1/Overview/GetProjectAnalytics/*.cs` — 3 个文件存在
- [x] `Features/v1/Stats/GetWorkspaceStats/*.cs` — 3 个文件存在
- [x] `Features/v1/Stats/GetProjectStats/*.cs` — 3 个文件存在
- [x] `AnalyticsModule.cs` — 包含服务注册 + 4 个端点注册
- [x] `dotnet build src/YH.Flow.slnx` — **0 错误 0 警告**
- [x] 所有 4 个端点使用 `[RequireWorkspaceRole]` 授权
- [x] Commit hashes: `d4db41fc4`, `cc66b0dd0`, `938d09265`

## Next Phase Readiness

- Wave 2 (12-02) 完成：Overview + Stats 端点全部实现并编译通过
- 4 个端点组完整注册：Workspace/Project Overview + Workspace/Project Stats
- 所有端点支持 `date_filter` 日期过滤和 `RequireWorkspaceRole` 授权
- Wave 3 (12-03) 可直接开始实现 Chart + Export 端点
- 已知 3 个 Stub 项（ProjectName、DisplayName、workspace 级用户/项目统计）需在后续 Wave 或 Handler 层补充

---

_Phase: 12-analytics_
_Completed: 2026-06-26_
