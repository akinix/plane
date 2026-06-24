# Phase 5: Cycle — 周期管理 - Research

**Researched:** 2026-06-24
**Domain:** 周期管理（Sprint/Iteration CRUD, Cycle-Issue 关联, Burndown 实时聚合）
**Confidence:** HIGH

## Summary

Phase 5 交付完整的 Cycle 管理能力。Cycle 实体直接纳入现有的 **WorkItemsDbContext** (`yhschema.WorkItems` schema)，与 Issue/State/Label/etc 共享同一个 DbContext。所有操作复用 Phase 3/4 建立的手写 Vertical Slice 模式（Minimal API + Mediator + FluentValidation）。

**Plane 参考实现**提供完整的参考：Cycle/CycleIssue 模型定义、CRUD 端点、状态动态计算（Case/When）、Burndown 算法（实时 TruncDate 分组聚合）、Issue 迁移逻辑（进度快照冻结 + 未完成 Issue 重分配）。

**Burndown 采用实时计算模式**（D-02）：GET 请求时动态查询 Issue 表，按 `completed_at` 日期分组统计完成量，从总量累减得到每日剩余量。`progress_snapshot` JSON 仅在 transfer-issues 时冻结保存。

**技术复杂度评估：** Phase 5 主要挑战在 EF Core 端：

1. TruncDate 聚合在 EF Core 中通过 `DbFunctions.TruncateTime()` 或 `EF.Property<DateTime>` 分组实现
2. Case/When status 注解（DRAFT/UPCOMING/CURRENT/COMPLETED）通过 `EF.Functions` 或三元条件投影
3. Burndown 响应包含 `completion_chart`（日期→剩余量字典）和 Issue 分布统计（按状态/指派人/标签分组）
4. Issue 迁移需在单次 SaveChanges 中完成快照冻结 + CycleIssue 批量更新

**Primary recommendation:** Cycle/CycleIssue 作为 WorkItems 模块内的新 Feature 切片实现，定义于 `Modules.WorkItems/Features/v1/Cycles/` 下。创建 Contracts 子模块 `Contracts.v1.Cycles` 存放请求/响应 DTO。新增 ~8-10 个端点，3 个新增 Domain 实体（Cycle、CycleIssue、CycleUserProperties 存根），并在 WorkItemsDbContext 注册。T5.5（Burndown）需单独的 QueryHandler 在 Issue 表上做聚合查询。

---

## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** Cycle 纳入 **WorkItemsDbContext**（`yhschema.WorkItems` schema），与 Issue/State/Label 共享 DbContext。CycleIssue 桥接表使用真正 FK 约束关联 Issue 和 Cycle，Burndown 查询可在一个 DbContext 内完成聚合操作。理由：Cycle 与 Issue 紧密耦合（多对多关联、Burndown 实时聚合查询），独立 DbContext 需要跨 DB 操作和 Guid 标量引用，复杂度大于收益。

- **D-02:** API **实时计算**模式。GET 请求时动态查询 Issue 表，按 `completed_at` 日期统计完成数量，从总量中累减得到每日剩余量。
  - Plane 实现参考：使用 `TruncDate(completed_at)` 按日期分组累加完成数，对日期范围内每一天计算 `total_scope - cumulative_completed`。未来日期设为 `None`。
  - `progress_snapshot`：仅在 **transfer-issues**（从已结束 Cycle 迁移 Issue）时冻结快照存为 JSON。活跃 Cycle 不做快照。
  - 不采用预计算快照模式（维护复杂、一致性问题）。

- **D-03:** **严格匹配 Plane 行为**：
  - COMPLETED 周期只允许修改 `sort_order`、`name`、`description`
  - 禁止添加新 Issue 到已结束周期
  - 允许将未完成 Issue（backlog/unstarted/started）通过 transfer-issues 迁移到新周期
  - 迁移时自动冻结 `progress_snapshot` 保存当前统计

### Claude's Discretion

- **Cycle 状态**：与 Plane 一致，**动态计算**（不存储 status 字段）。使用 `Case/When` 注解基于日期范围计算状态：
  - `DRAFT` — start_date 和 end_date 均为 null
  - `UPCOMING` — start_date > now
  - `CURRENT` — start_date <= now <= end_date
  - `COMPLETED` — end_date < now
- **CycleIssue 桥接表**：与 Plane 一致，`(Issue, Cycle)` 唯一约束 + 软删除唯一索引（Plane 使用 `unique_together = ["issue", "cycle", "deleted_at"]` + `UniqueConstraint(condition=Q(deleted_at__isnull=True))`）
- **sort_order**：与 Plane 一致，新 Cycle 自动设为当前项目最小 sort_order - 10000
- **归档**：实现 archive/unarchive 端点，只有 end_date < now 的 Cycle 可以归档
- **日期校验**：实现 Cycle 日期重叠检查（POST `cycles/date-check` 端点）
- **Cycle 列表参数**：`cycle_view` 查询参数筛选 `current`/`upcoming`/`completed`/`draft`/`incomplete`/`all`
- **路由格式**：`/api/v1/workspaces/{slug}/projects/{projectId}/cycles/` — 嵌套在 project 路由组下
- **权限**：复用 `[RequireWorkspaceRole]` + ProjectMember 角色检查（Admin/Member 可写，Guest 可读）
- **Burndown 响应格式**：返回 `{ "completion_chart": { "2026-06-01": 15, "2026-06-02": 12, ... } }`，支持按 `?type=issues|points` 切换计算基础
- **Cycle CRUD 序列化器**：响应包含注释字段（total_issues/completed_issues/cancelled_issues/started_issues/unstarted_issues/backlog_issues/status/is_favorite/assignee_ids）
- **归档端点**：独立路由 `/archived-cycles/` + POST archive/DELETE unarchive
- **CycleUserProperties** 和 **Favorites**：不在 Phase 5 实现，留待 Phase 13 前端支持时补充

### Deferred Ideas (OUT OF SCOPE)

- CycleUserProperties（每用户筛选/显示偏好）— 超出 Phase 5 scope，Phase 13 前端补充
- Cycle 收藏（Favorites）— 超出 Phase 5 scope，Phase 13 前端补充
- Cycle 进度通知/邮件 — Phase 11 Notification
- Cycle 分析图表 API（按指派人/标签分布）— 超出 Phase 5 scope，Phase 12 Analytics
- 甘特图/Gantt 视图 — 前端能力，Phase 13

---

## Phase Requirements

| ID      | Description                                              | Research Support                                                                                                                                                 |
| ------- | -------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| REQ-5.1 | Cycle CRUD（创建、列出、获取详情、更新、删除、状态流转） | Cycle 实体全字段映射完成（16 字段 + sort_order 自动计算）；动态状态通过 Case/When SQL 注解在 QueryHandler 中实现；COMPLETED 周期编辑限制在 CommandHandler 中校验 |
| REQ-5.2 | Cycle-Issue 关联（添加/移除/列表/迁移/Burndown）         | CycleIssue 桥接实体（真正 FK、唯一约束、软删除）；Burndown 实时聚合查询（TruncDate + 累积减法）；迁移冻结 progress_snapshot 并 bulk-update CycleIssue            |

---

## Architectural Responsibility Map

| Capability         | Primary Tier                  | Secondary Tier                | Rationale                                                                                               |
| ------------------ | ----------------------------- | ----------------------------- | ------------------------------------------------------------------------------------------------------- |
| Cycle 数据持久化   | Database (WorkItemsDbContext) | —                             | Cycle 实体纳入 WorkItemsDbContext（D-01），通过 EF Core 持久化到 `yhschema.WorkItems` schema            |
| Cycle CRUD         | API Backend                   | —                             | 全部 CRUD 操作通过 Mediator Command/Query + Minimal API Endpoint 暴露                                   |
| Cycle 动态状态计算 | API Backend                   | —                             | Case/When 注解在 QueryHandler 中通过 EF Core `Select` 投影动态计算，不存储 status 字段                  |
| Cycle-Issue 关联   | API Backend                   | Database                      | CycleIssue 桥接表管理多对多关联，API 端点提供 add/remove/list 操作                                      |
| Burndown 计算      | API Backend                   | Database (WorkItemsDbContext) | 实时聚合查询（D-02）：GET 时查询 Issue 表的 `completed_at` 字段做 TruncDate 分组统计                    |
| Issue 迁移         | API Backend                   | Database                      | `TransferCycleIssueEndpoint` 协调进度快照冻结 + CycleIssue 批量更新（同一 DbContext 事务）              |
| 权限控制           | API Backend                   | Workspace/Project Authz       | 复用 `[RequireWorkspaceRole]`（Admin/Member）+ 项目成员隐式角色过滤（已由 Plane 兼容模式建立）          |
| 归档管理           | API Backend                   | Database                      | 独立端点 `/archived-cycles/` （GET list + POST archive + DELETE unarchive），仅 completed cycles 可归档 |

---

## Standard Stack

### Core — 复用现有基础设施

| Library               | Version | Purpose                 | Why Standard                                         |
| --------------------- | ------- | ----------------------- | ---------------------------------------------------- |
| Entity Framework Core | .NET 10 | ORM / 数据持久化        | Phase 1 已配置，Phase 4 WorkItemsDbContext 已建立    |
| Mediator              | latest  | CQRS Command/Query 分发 | Phase 2/3/4 Vertical Slice 核心模式                  |
| FluentValidation      | latest  | 输入校验                | 已用于所有 Phase 3/4 操作                            |
| Finbuckle.MultiTenant | latest  | 多租户数据隔离          | 全局架构要求（`IHasTenant` + `AdjustUniqueIndexes`） |

### Supporting

| Library                               | Version  | Purpose                          | When to Use                        |
| ------------------------------------- | -------- | -------------------------------- | ---------------------------------- |
| Npgsql.EntityFrameworkCore.PostgreSQL | .NET 10  | PostgreSQL EF Core Provider      | 数据库层                           |
| Newtonsoft.Json / System.Text.Json    | built-in | JSON 序列化（progress_snapshot） | progress_snapshot 字段 JSON 序列化 |

### Alternatives Considered

| Instead of           | Could Use                      | Tradeoff                                                                                |
| -------------------- | ------------------------------ | --------------------------------------------------------------------------------------- |
| 实时 Burndown 查询   | 预计算快照 + Hangfire 定时更新 | 实时计算一致性好但每次请求计算开销；预计算快照性能好但维护复杂（D-02 决策锁定实时模式） |
| 独立 CyclesDbContext | WorkItemsDbContext             | 独立 DbContext 需要跨 DB 操作（D-01 决策锁定合并）                                      |

**版本验证：**

```bash
# 所有包均已存在于 yh-flow 的 Directory.Packages.props 中
# 无新增外部包依赖 — 仅需在现有 WorkItemsDbContext 中新增 DbSet
```

---

## Package Legitimacy Audit

> 此阶段**不需要**安装新的外部包。所有功能均基于现有 YH.Flow 基础设施（EF Core、Mediator、FluentValidation、Finbuckle、Npgsql）实现，这些包已在 Phase 1-4 验证通过。Cycle 实体纳入已有的 WorkItemsDbContext。

| Package  | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
| -------- | -------- | --- | --------- | ----------- | --------- | ----------- |
| 无新增包 | —        | —   | —         | —           | —         | N/A         |

---

## Architecture Patterns

### System Architecture Diagram

```
┌─ Request Flow ─────────────────────────────────────────────────────┐
│                                                                     │
│  GET /workspaces/{slug}/projects/{pid}/cycles/                      │
│       │                                                             │
│       ▼                                                             │
│  [RequireWorkspaceRole] middleware  ──►  Workspace authz            │
│       │                                                             │
│       ▼                                                             │
│  Minimal API Endpoint (ListCyclesEndpoint.cs)                       │
│       │                                                             │
│       ▼                                                             │
│  ListCyclesQuery ──► ListCyclesQueryHandler                         │
│       │                                                             │
│       ├── WorkItemsDbContext.Cycles (AsNoTracking)                  │
│       │     └── Annotate: total_issues, completed_issues, status,   │
│       │         assignee_ids (EF Core GroupBy/Count projection)     │
│       │     └── Filter: cycle_view (current/upcoming/completed/     │
│       │         draft/incomplete/all)                                │
│       │     └── Project → CycleListDto (Plane 兼容格式)             │
│       │                                                             │
│       ▼                                                             │
│  PlanePagedResult<CycleDto> ◄── 返回分页响应                        │
│                                                                     │
├─ Burndown Flow ─────────────────────────────────────────────────────┤
│                                                                     │
│  GET /cycles/{cycleId}/progress?type=issues                         │
│       │                                                             │
│       ▼                                                             │
│  GetCycleProgressQueryHandler                                       │
│       │                                                             │
│       ├── 1) 计算日期范围 (start_date → end_date)                    │
│       ├── 2) 查询 Issue 表: GroupBy TruncDate(completed_at)          │
│       │     累计每天完成的 Issue 数 (或 estimate 和)                 │
│       ├── 3) For each date: remaining = total - cumulative_completed │
│       ├── 4) 未来日期 → None                                         │
│       │                                                             │
│       ▼                                                             │
│  { "completion_chart": {"2026-06-01": 15, ...} }                    │
│                                                                     │
├─ Transfer-issues Flow ──────────────────────────────────────────────┤
│                                                                     │
│  POST /cycles/{cycleId}/transfer-issues { "new_cycle_id": "..." }   │
│       │                                                             │
│       ▼                                                             │
│  TransferCycleIssuesCommandHandler                                  │
│       │                                                             │
│       ├── 1) 验证 new_cycle 未 COMPLETED                            │
│       ├── 2) 聚合 old_cycle 的 Issue 分布统计                         │
│       ├── 3) 生成 completion_chart (调用 burndown 逻辑)              │
│       ├── 4) 保存 progress_snapshot JSON 到 old_cycle               │
│       ├── 5) 查询 backlog/unstarted/started 状态的 CycleIssue       │
│       ├── 6) BulkUpdate CycleId = new_cycle_id                      │
│       │                                                             │
│       ▼                                                             │
│  单次 SaveChanges (事务)    ──►  CycleIssue 表已更新                │
│                              ──►  Cycle.progress_snapshot 已冻结    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Recommended Project Structure

```
Modules.WorkItems/
├── Modules.WorkItems/
│   ├── Domain/
│   │   ├── Cycle.cs                      # NEW - 周期实体
│   │   └── CycleIssue.cs                 # NEW - Cycle-Issue 桥接实体
│   ├── Data/
│   │   ├── WorkItemsDbContext.cs          # MODIFIED - 新增 DbSet<Cycle>, DbSet<CycleIssue>
│   │   └── Configurations/
│   │       ├── CycleConfiguration.cs      # NEW - EF 配置
│   │       └── CycleIssueConfiguration.cs # NEW - EF 配置（唯一约束）
│   ├── Features/v1/Cycles/               # NEW - 所有 Cycle 操作的 Vertical Slice
│   │   ├── CreateCycle/                   # POST /cycles/
│   │   ├── GetCycle/                      # GET /cycles/{cycleId}
│   │   ├── UpdateCycle/                   # PATCH /cycles/{cycleId}
│   │   ├── DeleteCycle/                   # DELETE /cycles/{cycleId}
│   │   ├── ListCycles/                    # GET /cycles/ (cycle_view filter)
│   │   ├── DateCheckCycle/               # POST /cycles/date-check/
│   │   ├── AddIssuesToCycle/             # POST /cycles/{cycleId}/cycle-issues/
│   │   ├── RemoveIssueFromCycle/         # DELETE /cycles/{cycleId}/cycle-issues/{issueId}
│   │   ├── ListCycleIssues/              # GET /cycles/{cycleId}/cycle-issues/
│   │   ├── TransferCycleIssues/          # POST /cycles/{cycleId}/transfer-issues/
│   │   ├── GetCycleProgress/             # GET /cycles/{cycleId}/progress?type=issues|points
│   │   ├── ArchiveCycle/                 # POST /cycles/{cycleId}/archive/
│   │   ├── UnarchiveCycle/               # DELETE /archived-cycles/{cycleId}/unarchive/
│   │   └── ListArchivedCycles/           # GET /archived-cycles/
│   ├── Services/
│   │   └── BurndownCalculator.cs         # NEW - Burndown 计算服务（可复用给 Phase 6 Module）
│   └── WorkItemsModule.cs                # MODIFIED - 注册 Cycle 端点路由
│
├── Modules.WorkItems.Contracts/
│   ├── Constants/
│   │   └── CycleConstants.cs             # NEW - Validation limits
│   ├── DTOs/
│   │   ├── CycleDto.cs                   # NEW - Cycle 响应 DTO (含注释字段)
│   │   └── CycleProgressDto.cs           # NEW - Burndown 响应 DTO
│   └── v1/Cycles/
│       ├── CreateCycle/                   # CreateCycleCommand, CreateCycleResponse
│       ├── GetCycle/                      # GetCycleQuery
│       ├── UpdateCycle/                   # UpdateCycleCommand
│       ├── DeleteCycle/                   # DeleteCycleCommand
│       ├── ListCycles/                    # ListCyclesQuery
│       ├── DateCheckCycle/               # DateCheckCycleCommand
│       ├── Issues/                        # AddIssuesToCycle, RemoveIssueFromCycle, ListCycleIssues
│       ├── TransferCycleIssues/          # TransferCycleIssuesCommand
│       ├── GetCycleProgress/             # GetCycleProgressQuery
│       └── ArchiveCycle/                 # ArchiveCycleCommand, UnarchiveCycleCommand
│
└── Modules.WorkItems.Tests/              # 新增 Cycle 测试
    ├── Integration/
    │   ├── CycleCrudTests.cs             # Cycle CRUD 集成测试
    │   └── CycleBurndownTests.cs         # Burndown 计算测试
    └── Unit/
        ├── CycleDomainTests.cs           # Domain 实体测试
        └── CycleIssueDomainTests.cs      # CycleIssue 测试
```

### Pattern 1: Domain Entity — Cycle

**What:** Plane Cycle 模型的 C# 映射。实现 `IHasTenant`、`ISoftDeletable`、`IAuditableEntity`。使用私有构造函数 + 静态工厂方法模式（Phase 4 Issue 风格）。

**When to use:** 创建新 Domain 实体时

**Example（参考 Plane `cycle.py` + YH.Flow `Issue.cs` 模式）:**

```csharp
// Source: Plane cycle.py (lines 60-97) + YH.Flow Issue.cs (Phase 4 pattern)
public sealed class Cycle : IHasTenant, ISoftDeletable, IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public double SortOrder { get; private set; } = 65535.0;
    public string? ExternalSource { get; private set; }
    public string? ExternalId { get; private set; }
    public string? ProgressSnapshot { get; private set; } // JSON string
    public DateTimeOffset? ArchivedAt { get; private set; }
    public string? LogoProps { get; private set; } // JSON string
    public string Timezone { get; private set; } = "UTC";
    public int Version { get; private set; } = 1;
    public Guid ProjectId { get; private set; }

    // IHasTenant
    public string TenantId { get; private set; } = default!;
    // IAuditableEntity + ISoftDeletable fields omitted for brevity

    private Cycle() { } // EF Core

    public static Cycle Create(string name, Guid projectId,
        DateTimeOffset? startDate = null, DateTimeOffset? endDate = null,
        string? description = null, string? timezone = null)
    {
        // Plane validation: both start/end must be null or both set
        if ((startDate is null) != (endDate is null))
            throw new ArgumentException("Both start date and end date are either required or both null.");

        return new Cycle
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectId = projectId,
            StartDate = startDate,
            EndDate = endDate,
            Description = description ?? string.Empty,
            Timezone = timezone ?? "UTC",
            CreatedOnUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Auto-calculate sort_order: min existing sort_order - 10000 (Plane save() behavior).</summary>
    public void AssignSortOrder(double? smallestExistingSortOrder)
    {
        SortOrder = smallestExistingSortOrder.HasValue
            ? smallestExistingSortOrder.Value - 10000
            : 65535.0;
    }

    /// <summary>COMPLETED cycle: only sort_order, name, description allowed.</summary>
    public bool CanEdit => EndDate is null || EndDate > DateTimeOffset.UtcNow;

    /// <summary>For COMPLETED cycles, restricts allowed fields per D-03.</summary>
    public void UpdateRestricted(string? name = null, string? description = null, double? sortOrder = null)
    {
        if (name is not null) Name = name;
        if (description is not null) Description = description;
        if (sortOrder.HasValue) SortOrder = sortOrder.Value;
    }

    public void Archive() => ArchivedAt = DateTimeOffset.UtcNow;
    public void Unarchive() => ArchivedAt = null;

    public void FreezeSnapshot(string snapshotJson) => ProgressSnapshot = snapshotJson;
}
```

### Pattern 2: Bridge Entity — CycleIssue (软删除 + 唯一约束)

**What:** Cycle 和 Issue 的多对多桥接表。与 Plane 一致的 `(Issue, Cycle)` 唯一约束 + 软删除。

**When to use:** 多对多关联需要软删除 + 唯一约束时

**Example（参考 Plane `cycle.py` lines 104-127 + YH.Flow `IssueAssignee.cs` 模式）:**

```csharp
// Source: Plane cycle.py CycleIssue (unique_together + UniqueConstraint condition)
public sealed class CycleIssue : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }
    public Guid IssueId { get; private set; }
    public Guid CycleId { get; private set; }
    // IHasTenant + ISoftDeletable fields...

    private CycleIssue() { } // EF Core

    public static CycleIssue Create(Guid issueId, Guid cycleId) { /* ... */ }
}
```

### Pattern 3: EF Configuration — CycleIssue 唯一约束

**What:** 在 CycleIssue 上实现 Plane 风格的软删除唯一约束。

**When to use:** 需要软删除感知的唯一约束时

**Example（参考 Plane `cycle.py` lines 113-119 + YH.Flow `IssueAssigneeConfiguration.cs` 模式）:**

```csharp
// Source: Plane cycle.py Meta.unique_together + UniqueConstraint
public sealed class CycleIssueConfiguration : IEntityTypeConfiguration<CycleIssue>
{
    public void Configure(EntityTypeBuilder<CycleIssue> builder)
    {
        builder.ToTable("CycleIssues", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.IssueId).IsRequired();
        builder.Property(x => x.CycleId).IsRequired();

        // Plane-style: (Issue, Cycle) unique when deleted_at IS NULL
        builder.HasIndex(x => new { x.TenantId, x.IssueId, x.CycleId })
            .IsUnique()
            .HasDatabaseName("IX_CycleIssues_Tenant_Issue_Cycle")
            .HasFilter("[DeletedOnUtc] IS NULL");
    }
}
```

### Pattern 4: Burndown 实时计算（EF Core 查询）

**What:** Plane `burndown_plot()` 的 EF Core 等价实现。实时聚合 Issue 的 `completed_at` 字段。

**When to use:** 所有 Burndown/Progress 查询端点

**Example（参考 Plane `analytics_plot.py` 逻辑 + ListIssuesQueryHandler 模式）:**

```csharp
// Source: Plane analytics_plot.py burndown_plot() algorithm + YH.Flow query pattern
public sealed class GetCycleProgressQueryHandler : IQueryHandler<GetCycleProgressQuery, CycleProgressDto>
{
    private readonly WorkItemsDbContext _db;

    public async ValueTask<CycleProgressDto> Handle(GetCycleProgressQuery query, ...)
    {
        // 1) 获取 Cycle 的日期范围
        var cycle = await _db.Cycles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CycleId, ct);
        if (cycle?.StartDate is null || cycle?.EndDate is null)
            return EmptyResponse();

        // 2) 生成日期范围
        var dates = Enumerable.Range(0, (cycle.EndDate.Value.Date - cycle.StartDate.Value.Date).Days + 1)
            .Select(d => cycle.StartDate.Value.Date.AddDays(d))
            .ToList();

        // 3) 查询 Issue 表：按完成日期分组统计
        var totalIssues = await _db.CycleIssues
            .Where(ci => ci.CycleId == query.CycleId && !ci.IsDeleted)
            .CountAsync(ct);

        var completedPerDay = query.Type == "points"
            ? await _db.CycleIssues
                .Where(ci => ci.CycleId == query.CycleId && !ci.IsDeleted)
                .Join(_db.Issues.Where(i => i.CompletedAt != null),
                    ci => ci.IssueId, i => i.Id,
                    (ci, i) => new { i.CompletedAt.Value, i.EstimatePointId })
                .GroupBy(x => x.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Sum(x => /* estimate point value */) })
                .ToListAsync(ct)
            : await _db.CycleIssues
                .Where(ci => ci.CycleId == query.CycleId && !ci.IsDeleted)
                .Join(_db.Issues.Where(i => i.CompletedAt != null),
                    ci => ci.IssueId, i => i.Id,
                    (ci, i) => i.CompletedAt.Value.Date)
                .GroupBy(d => d)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(ct);

        // 4) 计算每日累积剩余量
        var cumulative = 0;
        var chart = new Dictionary<string, int?>();
        foreach (var date in dates)
        {
            cumulative += completedPerDay.Where(x => x.Date <= date).Sum(x => x.Count);
            chart[date.ToString("yyyy-MM-dd")] = date > DateTime.UtcNow.Date
                ? null : totalIssues - cumulative;
        }

        return new CycleProgressDto { CompletionChart = chart };
    }
}
```

### Pattern 5: Transfer-Issues 迁移逻辑

**What:** 从已结束 Cycle 迁移未完成 Issue 到新 Cycle，冻结进度快照。

**When to use:** Cycle 结束后遗留 Issue 处理

**Example（参考 Plane `cycle_transfer_issues.py` 算法）:**

```csharp
// 伪代码 — 迁移处理流程
// Key steps from Plane's transfer_cycle_issues():
//
// 1. Validate: new_cycle is not completed (end_date > now)
// 2. Build progress_snapshot from old_cycle annotations:
//    - total_issues, completed_issues, cancelled_issues, etc.
//    - distribution (assignees, labels, completion_chart)
// 3. Save snapshot: current_cycle.progress_snapshot = snapshot (JSON)
// 4. Query CycleIssues where state_group in ["backlog","unstarted","started"]
// 5. Bulk update: cycle_issue.cycle_id = new_cycle_id
// 6. SaveChanges (single transaction)
```

### Anti-Patterns to Avoid

- **在 Cycle 实体上存储 status 字段**：Plane 使用 Case/When 动态计算（DRAFT/UPCOMING/CURRENT/COMPLETED），存储字段会产生一致性问题（日期变更后不同步）
- **Cycle 软删除时直接硬删 CycleIssue**：Plane 有 bug（TODO in cycle.py ln 500），应使用软删除保持关联一致性
- **Burndown 预计算缓存**：D-02 锁定实时计算，不做缓存快照（progress_snapshot 仅在 transfer-issues 时写入）
- **CycleIssue 全量替换模式**：是 IssueAssignee/IssueLabel 使用的模式。CycleIssue 不同——Issue 属于一个 Cycle 的语义，使用逐条添加/删除 + bulk transfer

---

## Don't Hand-Roll

| Problem                          | Don't Build                   | Use Instead                                | Why                                    |
| -------------------------------- | ----------------------------- | ------------------------------------------ | -------------------------------------- |
| Burndown 日期范围生成            | 手动循环 + 时区转换           | `Enumerable.Range` + `DateTime.Date`       | 纯数据计算，无业务复杂度               |
| CycleIssue 唯一约束              | 在 Handler 中手动校验唯一性   | EF Core `HasFilter` 唯一索引               | 数据库约束提供原子性保障，避免并发竞态 |
| JSON 序列化（progress_snapshot） | 手动 JSON 拼接                | `System.Text.Json.JsonSerializer`          | 已内置于 .NET 10                       |
| 分页响应                         | 手动 count/next/previous      | `PlanePagedResultFactory` (Phase 4 已实现) | 统一 Plane 兼容格式，已测试验证        |
| 多租户隔离                       | 手动 `Where(TenantId == ...)` | Finbuckle + `IHasTenant` + 全局查询过滤器  | 全局已配置，新实体自动获得隔离         |

**Key insight:** Phase 5 的复杂度主要在 EF Core 聚合查询（Burndown）和事务协调（Transfer-Issues + Snapshot 冻结），不在基础设施。所有基础设施（DbContext、Mediator、分页、租户隔离、权限）均从 Phase 3/4 复用。

---

## Common Pitfalls

### Pitfall 1: EF Core TruncDate 查询差异

**What goes wrong:** Plane 使用 Django ORM `TruncDate(completed_at)` 按日期截断分组。EF Core 没有直接的 TruncDate，需要使用 `EF.Property<DateTime>(entity, "CompletedAt").Date` 或自定义 `DbFunction`。

**How to avoid:** 使用 `entity.CompletedAt.Value.Date` 在 LINQ 中分组，EF Core 6+ 支持翻译为 `CAST(completed_at AS date)`。

**Warning signs:** Burndown 返回空 chart 或错误分组 —— 检查生成的 SQL。

### Pitfall 2: Cycle 删除后的 CycleIssue 悬空

**What goes wrong:** Plane 代码中 `cycle.delete()` 注释了"TODO: Soft delete the cycle break the onetoone relationship with cycle issue"，说明删除后 CycleIssue 记录会悬空。

**How to avoid:** YH.Flow 使用软删除实现 `ISoftDeletable`，删除 Cycle 时：

1. Cycle 标记 `IsDeleted = true`
2. CycleIssue 记录保留（有独立的 `IsDeleted` 和软删除过滤器）
3. 查询时全局过滤器自动排除已删除记录

### Pitfall 3: COMPLETED 状态编辑限制的边界条件

**What goes wrong:** `end_date < now` 的判断在 UTC 和项目时区（Plane 使用 project.timezone）下有差异。

**How to avoid:** 如 D-03 所述，严格使用 `DateTimeOffset.UtcNow` 比较 `EndDate`。不需要像 Plane 那样转换到项目时区，因为所有日期时间存储为 UTC。

### Pitfall 4: CycleIssue 唯一约束的软删除冲突

**What goes wrong:** Plane 使用 `unique_together = ["issue", "cycle", "deleted_at"]` + `UniqueConstraint(condition=Q(deleted_at__isnull=True))`。EF Core 不能直接在唯一键中包含可为空列。

**How to avoid:** 使用 `HasFilter("[DeletedOnUtc] IS NULL")` 条件唯一索引（见 Pattern 3）。Finbuckle 的 `AdjustUniqueIndexes` 会自动将 `TenantId` 添加到索引列。

### Pitfall 5: Burndown 计算中未来日期处理

**What goes wrong:** Plane 将 `date > timezone.now().date()` 的每日剩余量设为 `None`。前端用 `null` 值指示未到达的日期。

**How to avoid:** 在循环中检查 `date > DateTime.UtcNow.Date`，将未来日期的值设为 `null`（而不是 0）。

### Pitfall 6: Progress Snapshot 结构不一致

**What goes wrong:** Plane 的 progress_snapshot JSON 有严格的结构：`{ total_issues, completed_issues, cancelled_issues, started_issues, unstarted_issues, backlog_issues, distribution: { labels: [...], assignees: [...], completion_chart: {...} }, estimate_distribution: {...} }`。前端解析依赖这个结构。

**How to avoid:** 使用强类型 DTO 序列化 snapshot，确保结构完全匹配 Plane 格式。

---

## Code Examples

### Cycle 实体 EF 配置（CycleConfiguration.cs）

```csharp
// Source: Plane cycle.py + YH.Flow IssueConfiguration.cs pattern
public sealed class CycleConfiguration : IEntityTypeConfiguration<Cycle>
{
    public void Configure(EntityTypeBuilder<Cycle> builder)
    {
        builder.ToTable("Cycles", WorkItemsModuleConstants.SchemaName)
            .HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(65535.0);
        builder.Property(x => x.ExternalSource).HasMaxLength(255);
        builder.Property(x => x.ExternalId).HasMaxLength(255);
        builder.Property(x => x.Timezone).IsRequired().HasMaxLength(255).HasDefaultValue("UTC");
        builder.Property(x => x.Version).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.ProgressSnapshot); // nvarchar(max)
        builder.Property(x => x.LogoProps);         // nvarchar(max)

        // Indexes
        builder.HasIndex(x => new { x.ProjectId, x.SortOrder })
            .HasDatabaseName("IX_Cycles_Project_SortOrder");

        builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.ProjectId })
            .HasDatabaseName("IX_Cycles_Tenant_Deleted_Project");
    }
}
```

### Burndown 算法核心逻辑（Plane 参考）

```python
# Source: Plane apps/api/plane/utils/analytics_plot.py burndown_plot()
#
# 算法（伪代码）:
# total = cycle.total_issues (or total_estimate_points)
# date_range = [start_date + timedelta(days=x) for x in range((end_date - start_date).days + 1)]
# chart = {str(date): 0 for date in date_range}
#
# completed = Issue.objects.filter(cycle=cycle)
#     .annotate(date=TruncDate("completed_at"))
#     .values("date")
#     .annotate(total_completed=Count("id"))
#     .order_by("date")
#
# for date in date_range:
#     cumulative = sum of total_completed where item.date <= date
#     remaining = total - cumulative
#     chart[str(date)] = None if date > now else remaining
#
# return chart
```

### CYcle 列表查询参数处理（Plane 参考）

```python
# Source: Plane apps/api/plane/app/views/cycle/base.py (lines 185-268)
# cycle_view 筛选:
# "current"    → start_date <= now AND end_date >= now
# "upcoming"   → start_date > now
# "completed"  → end_date < now
# "draft"      → end_date IS NULL AND start_date IS NULL
# "incomplete" → end_date >= now OR end_date IS NULL
# "all"        → no filter (default)
```

### Case/When 状态注解（Plane 参考 — 动态 status 注解）

```python
# Source: Plane apps/api/plane/app/views/cycle/base.py (lines 153-167)
# .annotate(
#     status=Case(
#         When(start_date__lte=now & end_date__gte=now, then=Value("CURRENT")),
#         When(start_date__gt=now, then=Value("UPCOMING")),
#         When(end_date__lt=now, then=Value("COMPLETED")),
#         When(start_date__isnull=True & end_date__isnull=True, then=Value("DRAFT")),
#         default=Value("DRAFT"),
#     )
# )
```

### 日期重叠检查逻辑（Plane 参考）

```python
# Source: Plane plane/app/views/cycle/base.py CycleDateCheckEndpoint (lines 520-556)
# 检查逻辑:
# cycles = Cycle.objects.filter(
#     Q(workspace__slug=slug) &
#     Q(project_id=project_id) &
#     (Q(start_date__lte=start_date, end_date__gte=start_date) |
#      Q(start_date__lte=end_date, end_date__gte=end_date) |
#      Q(start_date__gte=start_date, end_date__lte=end_date))
# ).exclude(pk=cycle_id)
```

---

## State of the Art

| Old Approach                                  | Current Approach                                   | When Changed | Impact                                                               |
| --------------------------------------------- | -------------------------------------------------- | ------------ | -------------------------------------------------------------------- |
| Plane 旧版 Cycle 端点（`api/views/cycle.py`） | Plane 新版 Cycle 端点（`app/views/cycle/base.py`） | Plane v0.22  | 新版端点在 CycleSerializer 直接 includes annotate fields，路由更扁平 |
| Django ORM TruncDate                          | EF Core `DateTime.Date`                            | N/A          | 等效功能，SQL 翻译为 `CAST(completed_at AS date)`                    |
| Django Case/When                              | EF Core `Select` 三元表达式                        | N/A          | `status == (StartDate <= now && EndDate >= now) ? "CURRENT" : ...`   |

**Deprecated/outdated:**

- Plane 旧版 `api/views/cycle.py` 中的 `CycleViewSet` 已被新一代 `app/views/cycle/base.py` 中的独立端点替代。YH.Flow 应直接参考新一代端点设计（职责单一、无 ViewSet 模式）。

---

## Assumptions Log

| #   | Claim                                                                              | Section  | Risk if Wrong                                                                                              |
| --- | ---------------------------------------------------------------------------------- | -------- | ---------------------------------------------------------------------------------------------------------- |
| A1  | DateTimeOffset.UtcNow 比较适用于 COMPLETED 判断，不需要像 Plane 那样转换到项目时区 | Pitfalls | 如果前端使用用户本地时区展示日期，UTC 边界可能导致 +/-1 天偏移（Plane 同样用 UTC + project.timezone 转换） |

---

## Open Questions

1. **Burndown 查询如何引用 EstimatePoint 的值？**
   - 已知：Issue 实体有 `EstimatePointId`，通过导航属性关联 `EstimatePoint`，其 `Value` 字段为 `int`
   - 方案：`db.Entry(issue).Reference(i => i.EstimatePoint).Load()` 或在查询中 Include
   - 推荐：Burndown 的 points 模式使用 join: `_db.CycleIssues.Join(_db.Issues).Join(_db.EstimatePoints)` 聚合求和

2. **ListCycles 时如何统计 total_issues/completed_issues 等注释字段？**
   - 已知：Plane 使用 `Count(... distinct=True, filter=Q(...))` 注解
   - 待定：在 EF Core 中通过 `Select` 投影时使用 `_db.CycleIssues.Count(ci => ci.CycleId == c.Id && ...)` 或 GroupBy 后 Count
   - 推荐：在 `ListCyclesQueryHandler` 中使用子查询投影（因 EF Core 不支持 Django ORM 的 annotate + filter）

3. **Cycle 状态 "DRAFT" 的判断阈值？**
   - Plane: `When(start_date__isnull=True & end_date__isnull=True, then=Value("DRAFT")), default=Value("DRAFT")`
   - 结论：只有 start_date 和 end_date 同时为 null 才视为 DRAFT，其他边界情况也为 DRAFT

---

## Environment Availability

> SKIPPED（Phase 5 无外部依赖变化——所有依赖在 Phase 1-4 已就绪）

---

## Validation Architecture

### Test Framework

| Property           | Value                                                                                  |
| ------------------ | -------------------------------------------------------------------------------------- |
| Framework          | xUnit + FluentAssertions + Testcontainers                                              |
| Config file        | `yh-flow/src/Tests/WorkItems.Tests/`                                                   |
| Quick run command  | `dotnet test yh-flow/src/Tests/WorkItems.Tests --no-restore --filter "Category=Cycle"` |
| Full suite command | `dotnet test yh-flow/src/Tests/WorkItems.Tests --no-restore`                           |

### Phase Requirements -> Test Map

| Req ID  | Behavior                                        | Test Type        | Automated Command                                            | File Exists? |
| ------- | ----------------------------------------------- | ---------------- | ------------------------------------------------------------ | ------------ |
| REQ-5.1 | Create Cycle with valid data                    | unit+integration | `dotnet test --filter "CreateCycle_Success"`                 | NEW          |
| REQ-5.1 | Create Cycle rejects only one of start/end null | integration      | `dotnet test --filter "CreateCycle_RequiresBothDatesOrNone"` | NEW          |
| REQ-5.1 | List Cycles with cycle_view filter              | integration      | `dotnet test --filter "ListCycles_FilterByView"`             | NEW          |
| REQ-5.1 | Update COMPLETED cycle restricted               | integration      | `dotnet test --filter "UpdateCycle_Completed_Restricted"`    | NEW          |
| REQ-5.1 | Archive only completed cycles                   | integration      | `dotnet test --filter "ArchiveCycle_OnlyCompleted"`          | NEW          |
| REQ-5.2 | Add Issue to open Cycle                         | integration      | `dotnet test --filter "AddIssueToCycle_Success"`             | NEW          |
| REQ-5.2 | Add Issue to COMPLETED Cycle rejected           | integration      | `dotnet test --filter "AddIssueToCycle_Completed_Rejected"`  | NEW          |
| REQ-5.2 | Remove Issue from Cycle                         | integration      | `dotnet test --filter "RemoveIssueFromCycle_Success"`        | NEW          |
| REQ-5.2 | Burndown returns correct chart                  | integration      | `dotnet test --filter "GetCycleProgress_BurndownChart"`      | NEW          |
| REQ-5.2 | Transfer-issues freezes snapshot + re-assigns   | integration      | `dotnet test --filter "TransferIssues_FreezesSnapshot"`      | NEW          |

### Sampling Rate

- **Per task commit:** `dotnet test yh-flow/src/Tests/WorkItems.Tests --filter "Category=Cycle" --no-restore`
- **Per wave merge:** `dotnet test yh-flow/src/Tests/WorkItems.Tests --no-restore` + regression: Workspace + Identity + Project
- **Phase gate:** Full suite green before `/gsd-verify-work`

### Wave 0 Gaps

- [ ] `tests/WorkItems.Tests/Integration/CycleCrudTests.cs` — covers REQ-5.1 (CREATE/LIST/GET/UPDATE/DELETE Cycle, cycle_view filter, COMPLETED restriction)
- [ ] `tests/WorkItems.Tests/Integration/CycleIssueTests.cs` — covers REQ-5.2 (add/remove/list issues, transfer, burndown)
- [ ] `tests/WorkItems.Tests/Unit/CycleDomainTests.cs` — covers domain factory validation, AssignSortOrder, Archive/Unarchive
- [ ] `tests/WorkItems.Tests/TestData/CycleTestFixture.cs` — shared test data for cycle integration tests

---

## Security Domain

> 此阶段使用 Phase 2 已建立的 Workspace 权限基础设施，无需新增安全控制。

### Applicable ASVS Categories

| ASVS Category       | Applies | Standard Control                                                       |
| ------------------- | ------- | ---------------------------------------------------------------------- |
| V5 Input Validation | yes     | FluentValidation（所有 Command 已验证）                                |
| V4 Access Control   | yes     | `[RequireWorkspaceRole]`（Admin/Member/Guest）+ ProjectMember 隐式过滤 |

### Known Threat Patterns for .NET 10

| Pattern        | STRIDE                 | Standard Mitigation                                             |
| -------------- | ---------------------- | --------------------------------------------------------------- |
| 跨租户数据泄露 | Information Disclosure | Finbuckle `IHasTenant` + 全局查询过滤器 + `AdjustUniqueIndexes` |

---

## Sources

### Primary (HIGH confidence)

- Plane `apps/api/plane/db/models/cycle.py` — Cycle/CycleIssue 模型定义（字段、约束、save 方法、唯一约束）
- Plane `apps/api/plane/app/views/cycle/base.py` — 新一代 Cycle 端点（状态 Case/When、注释字段查询、权限）
- Plane `apps/api/plane/api/views/cycle.py` — 旧版 Cycle 端点（详细 CRUD 逻辑、CycleIssue 增删、日期校验）
- Plane `apps/api/plane/app/views/cycle/archive.py` — 归档端点逻辑
- Plane `apps/api/plane/utils/analytics_plot.py` — Burndown 算法实现（TruncDate 分组、累积减法）
- Plane `apps/api/plane/utils/cycle_transfer_issues.py` — Issue 迁移 + snapshot 冻结实现
- Plane `apps/api/plane/api/serializers/cycle.py` — 序列化器字段定义
- Plane `apps/api/plane/app/serializers/cycle.py` — 新一代序列化器（注释字段、状态字段）
- YH.Flow `src/Modules/WorkItems/Modules.WorkItems/Domain/Issue.cs` — Domain 实体模式
- YH.Flow `src/Modules/WorkItems/Modules.WorkItems/Domain/IssueAssignee.cs` — M2M 桥接实体模式
- YH.Flow `src/Modules/WorkItems/Modules.WorkItems/Data/WorkItemsDbContext.cs` — DbContext 模式（Pitfall 8）
- YH.Flow `src/Modules/WorkItems/Modules.WorkItems/Features/v1/States/CreateState/` — Vertical Slice 模式（Endpoint + Command + Handler）
- YH.Flow `src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule.cs` — Module 注册 + 路由模式
- YH.Flow `src/Modules/Project/Modules.Project/ProjectModule.cs` — 嵌套路由组模式
- `.planning/phases/05-cycle/05-CONTEXT.md` — Phase 5 用户决策

### Secondary (MEDIUM confidence)

- Plane `apps/api/plane/api/urls/cycle.py` — 路由定义验证
- Plane `packages/types/src/cycle/cycle.ts` — 前端 TS 类型（验证 API 响应格式）
- YH.Flow `src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/IssueConfiguration.cs` — EF 配置模式（索引、schema）
- YH.Flow `src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/IssueAssigneeConfiguration.cs` — 唯一约束索引模式

### Tertiary (LOW confidence)

- 无——所有关键信息均从 HIGH 置信度来源验证

---

## Metadata

**Confidence breakdown:**

- Standard stack: HIGH — 所有基础设施已就绪，无新增外部包
- Architecture: HIGH — Plane 参考实现 + YH.Flow 已有模式完全覆盖
- Pitfalls: HIGH — 从 Plane 代码注释（cycle.py ln 500 TODO）和已知 EF Core 行为推导

**Research date:** 2026-06-24
**Valid until:** 2026-07-24（稳定技术栈，30 天有效期）
