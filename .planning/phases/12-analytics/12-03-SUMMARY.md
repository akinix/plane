---
phase: 12-analytics
plan: 03
subsystem: api
tags: analytics, charts, csv-export, hangfire, ef-core, group-by, monthly-aggregation, daily-aggregation

requires:
  - phase: 12-analytics_02
    provides: AnalyticsQueryService (5 aggregation methods), AnalyticsChartDto/ChartDataPoint DTOs, AnalyticsModule routing pattern

provides:
  - IAnalyticsQueryService 扩展（3 个 Chart 查询方法）
  - GET /analytics/charts/?type=work-items — 工作区月度 Issue 创建/完成趋势（连续月份填充 0-count）
  - GET /analytics/charts/?type=projects — 工作区汇总计数（work_items, cycles, modules, intake）
  - GET /projects/{projectId}/analytics/charts/?type=work-items — 项目级月度图表（可选 cycle_id/module_id 日级聚合）
  - POST /analytics/export/ — Hangfire 后台异步 CSV 导出
  - IAnalyticsExportService + AnalyticsExportService（RFC 4180 CSV 生成）
  - ExportAnalyticsJob（Hangfire 背景作业写入 temp directory）

affects:
  - phase: 13-flow-web (前端将消费 Chart/Export 端点)

tech-stack:
  added: []
  patterns:
    - Monthly aggregation with continuous date range filling（循环遍历确保 0-count 月份也返回）
    - Daily aggregation for cycle/module scoped ranges（按 Year/Month/Day GroupBy）
    - Bulk assignee lookup for CSV export（避免 N+1 查询问题）
    - Hangfire job 在无 HttpContext 环境下通过 IServiceScopeFactory 解析 scoped 服务

key-files:
  created:
    - Features/v1/Charts/GetWorkspaceChart/*.cs — 2 个文件（Endpoint + QueryHandler）
    - Features/v1/Charts/GetProjectChart/*.cs — 2 个文件（Endpoint + QueryHandler）
    - Features/v1/Export/ExportAnalytics/*.cs — 3 个文件（Endpoint + CommandHandler + Job）
    - Services/IAnalyticsExportService.cs — 接口定义
    - Services/AnalyticsExportService.cs — CSV 生成服务（BOM-prefixed UTF-8, RFC 4180 转义）
  modified:
    - Services/IAnalyticsQueryService.cs — 添加 3 个图表查询方法
    - Services/AnalyticsQueryService.cs — 实现 3 个图表查询方法（月度/日级聚合+连续序列填充）
    - AnalyticsModule.cs — 注册 Export 服务 + Chart/Export 端点

key-decisions:
  - "Chart 端点使用 IRequest<IResult> + Handler 模式（与 Wave 2 一致），type 参数路由到不同查询方法"
  - "GetProjectSummaryChartAsync 的 members/pages/views 返回 0（WorkItemsDbContext 无跨模块数据），与 Wave 2 的 stub 模式一致"
  - "Export 端点接受可选的 project_ids body 参数（POST json），但首次实现仅返回默认全量导出"
  - "CSV 文件写入服务器 temp directory（可扩展为邮件发送或下载链接）"

requirements-completed: [REQ-12.2]

duration: 28min
completed: 2026-06-26
---

# Phase 12 Plan 03: Analytics Chart + Export Endpoints

**月度 Issue 趋势图表（work-items/projects 两种类型）和 Hangfire 后台 CSV 导出端点**

## Performance

- **Duration:** 28 min
- **Completed:** 2026-06-26
- **Tasks:** 2
- **Files modified:** 12 (9 new + 3 modified)

## Task Commits

| #   | Name                                               | Type | Hash        |
| --- | -------------------------------------------------- | ---- | ----------- |
| 1   | 添加 Workspace/Project Chart 端点（月度/日级聚合） | feat | `a3297db1f` |
| 2   | 添加 Export 端点 + Hangfire CSV Job                | feat | `2d8a8bf07` |

## Accomplishments

### Task 1: Chart 端点（月度趋势 + 项目汇总）

扩展 `IAnalyticsQueryService` + `AnalyticsQueryService` 添加 3 个图表查询方法：

1. **GetWorkspaceWorkItemChartAsync** — 工作区月度 Issue 创建/完成趋势（Plane `work_item_completion_chart` 兼容）
   - TruncMonth 聚合（GroupBy Year/Month），按月遍历确保连续序列
   - 支持 `project_ids` 多项目过滤和预设/自定义日期范围
   - 返回 `AnalyticsChartDto { data[], schema }` 格式

2. **GetProjectSummaryChartAsync** — 工作区汇总计数
   - work_items, cycles, modules, intake（draft issue）4 项计数
   - members/pages/views 返回 0（跨模块 stub，与 Wave 2 一致）
   - 返回 `List<ChartDataPoint>` 格式

3. **GetProjectWorkItemChartAsync** — 项目级图表（支持 cycle/module 范围日级聚合）
   - `cycleId` 有值：Cycle.StartDate/EndDate 范围，日级聚合（Year/Month/Day GroupBy）
   - `moduleId` 有值：Module.StartDate/TargetDate 范围，日级聚合
   - 无范围：项目级月度聚合（与 workspace 版行为一致）

创建两个 Chart 端点：

- **GET /api/v1/workspaces/{slug}/analytics/charts/?type=work-items|projects**
  - type=work-items -> GetWorkspaceWorkItemChartAsync
  - type=projects -> GetProjectSummaryChartAsync
  - 查询参数：date_filter, start_date, end_date, project_ids

- **GET /api/v1/workspaces/{slug}/projects/{projectId}/analytics/charts/?type=work-items&cycle_id=&module_id=**
  - type=work-items -> GetProjectWorkItemChartAsync
  - 默认不支持其他 type（返回 400）

两个端点均使用 `.RequireWorkspaceRole(Admin, Member)` 授权。

### Task 2: Export 端点 + Hangfire CSV 作业

创建 CSV 导出基础设施：

1. **IAnalyticsExportService / AnalyticsExportService**
   - `GenerateCsvAsync()` 方法：查询 Issue + 显式 JOIN States 获取 StateGroup
   - 批量 Assignee 查询（Bulk GroupBy + Dictionary 映射，避免 N+1）
   - RFC 4180 CSV 格式：逗号/引号/换行转义
   - BOM-prefixed UTF-8 输出
   - 列：Issue ID, Name, State Group, Priority, Project Name, Assignee, Created At, Completed At

2. **ExportAnalyticsJob** — Hangfire 后台作业类
   - `RunAsync(tenantId, slug, projectIds)` 入口方法
   - 通过 IServiceScopeFactory 创建 scope 解析 scoped 服务
   - CSV 文件写入 `Path.GetTempPath()`

3. **ExportAnalyticsEndpoint** — `POST /api/v1/workspaces/{slug}/analytics/export/`
   - 接收可选的 `{ projectIds: string }` body
   - 通过 IJobService.Enqueue<> 入队 ExportAnalyticsJob
   - 返回 200 + `{ message: "Once the export is ready..." }`
   - 401 未授权（tenantId 不存在时）

4. **AnalyticsModule.cs 注册**
   - `builder.Services.AddTransient<IAnalyticsExportService, AnalyticsExportService>()`
   - `workspaceAnalytics.MapExportAnalyticsEndpoint()`

## Files Created/Modified

### Chart 端点组（NEW）

| 文件                                                                    | 提供                                                     |
| ----------------------------------------------------------------------- | -------------------------------------------------------- | --------- |
| `Features/v1/Charts/GetWorkspaceChart/GetWorkspaceChartEndpoint.cs`     | `[HttpGet] /charts?type=work-items                       | projects` |
| `Features/v1/Charts/GetWorkspaceChart/GetWorkspaceChartQueryHandler.cs` | Query record + Handler（type 路由）                      |
| `Features/v1/Charts/GetProjectChart/GetProjectChartEndpoint.cs`         | `[HttpGet] /projects/{projectId}/charts?type=work-items` |
| `Features/v1/Charts/GetProjectChart/GetProjectChartQueryHandler.cs`     | Query record + Handler（cycle_id/module_id）             |

### Export 端点 + Job（NEW）

| 文件                                                                  | 提供                                                              |
| --------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `Services/IAnalyticsExportService.cs`                                 | 2-method 接口（GenerateCsvAsync + ContentType）                   |
| `Services/AnalyticsExportService.cs`                                  | CSV 生成（BOM-prefixed UTF-8, RFC 4180 转义, 批量 assignee 查询） |
| `Features/v1/Export/ExportAnalytics/ExportAnalyticsEndpoint.cs`       | `[HttpPost] /export` + ExportAnalyticsRequest record              |
| `Features/v1/Export/ExportAnalytics/ExportAnalyticsCommandHandler.cs` | ExportAnalyticsCommand + Handler（入队 Hangfire）                 |
| `Features/v1/Export/ExportAnalytics/ExportAnalyticsJob.cs`            | Hangfire RunAsync + temp directory 写入                           |

### Module（MODIFIED）

| 文件                 | 变更                                                           |
| -------------------- | -------------------------------------------------------------- |
| `AnalyticsModule.cs` | 添加 `IAnalyticsExportService` DI 注册 + Chart/Export 端点注册 |

### Services（MODIFIED）

| 文件                                 | 变更                                     |
| ------------------------------------ | ---------------------------------------- |
| `Services/IAnalyticsQueryService.cs` | 添加 3 个图表查询方法接口                |
| `Services/AnalyticsQueryService.cs`  | 实现 3 个图表查询方法（~240 行新增代码） |

## Verification

### 构建验证

- `dotnet build src/YH.Flow.slnx` — **0 错误 0 警告**
- 包括 Modules.Analytics 和全量 57+ 项目

### 端点注册验证

| 端点              | 路径                                                               | 方法 |
| ----------------- | ------------------------------------------------------------------ | ---- |
| GetWorkspaceChart | `/api/v1/workspaces/{slug}/analytics/charts/`                      | GET  |
| GetProjectChart   | `/api/v1/workspaces/{slug}/projects/{projectId}/analytics/charts/` | GET  |
| ExportAnalytics   | `/api/v1/workspaces/{slug}/analytics/export/`                      | POST |

### 授权验证

- 所有 3 个端点使用 `.RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)` 授权

### 月度序列验证

- GetWorkspaceWorkItemChartAsync 和 GetProjectWorkItemChartAsync（无范围）使用 `while (current <= lastMonth)` 循环填充
- 0 count 的月份也返回 `{ key, name, count: 0, completed_issues: 0, created_issues: 0 }`

## Known Stubs

- **GetProjectSummaryChartAsync.members/pages/views** — 返回 0。这些数据需要跨模块查询（Workspace/Identity/Page/View），AnalyticsQueryService 只访问 WorkItemsDbContext。与 Wave 2 的 stub 模式一致。

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] CA1305: DateTime.ToString missing IFormatProvider**

- **Found during:** Task 1（第一次构建）
- **Issue:** `DateTime.ToString("yyyy-MM-dd")` 和 `DateTime.ToString("yyyy-MM")` 缺少 CultureInfo 参数导致 CA1305 错误
- **Fix:** 添加 `CultureInfo.InvariantCulture` 作为 IFormatProvider 参数
- **Files modified:** `Services/AnalyticsQueryService.cs`
- **Verification:** 构建通过
- **Committed in:** `a3297db1f` (Task 1)

**2. [Rule 3 - Blocking] CA1307: string.Contains/Replace missing StringComparison**

- **Found during:** Task 2（第二次构建）
- **Issue:** `string.Contains(char)` 和 `string.Replace(string, string?)` 缺少 StringComparison 参数导致 CA1307 错误
- **Fix:** 添加 `StringComparison.Ordinal` 参数
- **Files modified:** `Services/AnalyticsExportService.cs`
- **Verification:** 构建通过
- **Committed in:** `2d8a8bf07` (Task 2)

**3. [Rule 3 - Blocking] Missing IServiceScopeFactory using directive**

- **Found during:** Task 2（第二次构建）
- **Issue:** `IServiceScopeFactory` 在 AnalyticsExportService.cs 和 ExportAnalyticsJob.cs 中未引用
- **Fix:** 添加 `using Microsoft.Extensions.DependencyInjection;`
- **Files modified:** `Services/AnalyticsExportService.cs`, `Features/v1/Export/ExportAnalytics/ExportAnalyticsJob.cs`
- **Verification:** 构建通过
- **Committed in:** `2d8a8bf07` (Task 2)

---

**Total deviations:** 3 auto-fixed (all Rule 3 - blocking)
**Impact on plan:** 三个修复均为 TreatWarningsAsErrors 下的必须修复项。核心逻辑无变更。

## Self-Check: PASSED

- [x] `Services/IAnalyticsQueryService.cs` — 存在，扩展 3 个 Chart 方法
- [x] `Services/AnalyticsQueryService.cs` — 存在，实现 3 个 Chart 方法
- [x] `Features/v1/Charts/GetWorkspaceChart/*.cs` — 2 个文件
- [x] `Features/v1/Charts/GetProjectChart/*.cs` — 2 个文件
- [x] `Services/IAnalyticsExportService.cs` — 存在
- [x] `Services/AnalyticsExportService.cs` — 存在，CSV 生成实现
- [x] `Features/v1/Export/ExportAnalytics/*.cs` — 3 个文件（Endpoint + Handler + Job）
- [x] `AnalyticsModule.cs` — Export 服务注册 + Chart/Export 端点注册
- [x] `dotnet build src/YH.Flow.slnx` — **0 错误 0 警告**
- [x] 所有 3 个端点使用 `.RequireWorkspaceRole` 授权
- [x] Commit hashes: `a3297db1f`, `2d8a8bf07`

## Next Phase Readiness

- Wave 3 (12-03) 完成：Chart + Export 端点全部实现并编译通过
- 3 个新端点：Workspace Chart / Project Chart / Export
- 所有端点支持 `RequireWorkspaceRole` 授权和 `date_filter` 日期过滤
- Wave 4 (12-04) 可直接开始集成测试 + 全量回归
- 已知 3 个 Stub 项（members/pages/views 在 summary chart 中返回 0）与 Wave 2 stub 模式一致

---

_Phase: 12-analytics_
_Completed: 2026-06-26_
