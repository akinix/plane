---
phase: 04-workitems
plan: 01
subsystem: WorkItems
tags:
  - scaffold
  - domain
  - migration
  - module
depends_on: [03-04]
requires: []
affects:
  - YH.Flow.Api
  - YH.Flow.DbMigrator
  - YH.Flow.Migrations.PostgreSQL
  - YH.Flow.slnx
tech-stack:
  added:
    - Modules.WorkItems.Contracts
    - Modules.WorkItems
    - WorkItems.Tests
  patterns:
    - IHasTenant + ISoftDeletable + IAuditableEntity per-entity
    - BaseDbContext with ApplyConfigurationsFirst pattern
    - Finbuckle AdjustUniqueIndexes for tenant-widened unique indexes
    - FshModule auto-discovery (Order 260)
key-files:
  created:
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/* (7 files)"
    - "src/Modules/WorkItems/Modules.WorkItems/Domain/* (5 files)"
    - "src/Modules/WorkItems/Modules.WorkItems/Data/* (5 files)"
    - "src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule*.cs (2 files)"
    - "src/Tests/WorkItems.Tests/* (3 files)"
    - "src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/* (3 files)"
  modified:
    - "src/YH.Flow.slnx"
    - "src/Host/YH.Flow.Api/Program.cs"
    - "src/Host/YH.Flow.DbMigrator/Program.cs"
    - "src/Host/YH.Flow.Migrations.PostgreSQL/*.csproj"
decisions:
  - "WorkItemsModule Order=260 (after Project 250, before Auditing 300)"
  - "Schema yhschema.WorkItems — per-module schema strategy"
  - "WorkItemsConstants skipped in Mediator assemblies list (MSG0007)"
metrics:
  duration: "~28 min (3 commits)"
  completed: "2026-06-24"
---

# Phase 4 Plan 1: Wave 1 — Scaffold + Domain + Migration Summary

WorkItems 模块基础骨架：创建 Contracts 项目（DTOs/Constants）、Module 项目（领域实体/DbContext/EF 配置）、Test 项目，注册到 Host 各入口，生成 EF 迁移 InitialWorkItems。

## Tasks

### Task 1: Scaffold Contracts + Module + Tests + Host wiring

**Commit:** `f59202355`

创建了完整的模块骨架：

- **Modules.WorkItems.Contracts**: 4 个 DTO（StateDto/LabelDto/EstimateDto/EstimatePointDto）+ WorkItemsConstants 常量类
- **Modules.WorkItems**: WorkItemsModule（Order 260, 骨架 ConfigureServices 暂 placeholder）+ ModuleConstants
- **WorkItems.Tests**: xunit + Shouldly + NSubstitute + AutoFixture
- **Host wiring**: slnx 添加 WorkItems 目录条目，API/DbMigrator 注册 WorkItemsModule，Migration csproj 添加 WorkItems 引用

### Task 2: Domain entities + WorkItemsDbContext + EF configurations

**Commit:** `d30e27343`

创建了 5 个领域实体 + DbContext + 4 个配置：

- **StateGroup** enum: Backlog(0)/Unstarted(1)/Started(2)/Completed(3)/Cancelled(4)
- **State**: IHasTenant + ISoftDeletable + IAuditableEntity, 工厂模式 Create/Update/SoftDelete
- **Label**: IHasTenant + ISoftDeletable + IAuditableEntity, ParentId 自引用层级支持
- **Estimate**: IHasTenant + ISoftDeletable + IAuditableEntity, EstimatePoint 子集合 (cascade delete)
- **EstimatePoint**: IHasTenant + IAuditableEntity（无软删除 — 父级级联）
- **WorkItemsDbContext**: ApplyConfigurationsFromAssembly FIRST → base.OnModelCreating LAST
- **Configurations**: 正确索引、TenantId 包含、HasFilter 条件唯一索引、SetNull/Label 自引用、Cascade/EstimatePoint

### Task 3: Complete WorkItemsModule + EF migration

**Commit:** `e92f0665e`

完成模块注册并生成迁移：

- WorkItemsModule.ConfigureServices: 注册 WorkItemsDbContext + health check
- MapEndpoints: 定义 work-items 路由组，TODO 端点标记
- **EF 迁移 InitialWorkItems**: 在 `yhschema.WorkItems` 创建 4 个表

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Build] WorkItemsConstants 导致 Mediator 源生成器 MSG0007**

- **Found during:** Task 1
- **Issue:** 将 `typeof(WorkItemsConstants)` 添加到 Mediator `o.Assemblies` 导致源生成器错误，因为 WorkItems.Contracts 不包含 Mediator 消息/Handler
- **Fix:** 从 API 和 DbMigrator 的 Mediator assemblies 列表中移除 WorkItemsConstants（Constants 类不需要 Mediator 扫描）
- **Files modified:** `src/Host/YH.Flow.Api/Program.cs`, `src/Host/YH.Flow.DbMigrator/Program.cs`

**2. [Rule 1 - Build] XML cref 无法解析枚举成员和集合属性**

- **Found during:** Task 2
- **Issue:** State.cs 的 `<see cref="Completed"/>` 和 `<see cref="Cancelled"/>` 使用裸枚举成员名，TreatWarningsAsErrors 导致 CS1574
- **Fix:** 改为 `<see cref="StateGroup.Completed"/>` 和 `<see cref="StateGroup.Cancelled"/>`
- **Files modified:** `src/Modules/WorkItems/Modules.WorkItems/Domain/State.cs`

**3. [Rule 1 - Build] Estimate.cs cref "Estimates" 集合属性无法解析**

- **Found during:** Task 2
- **Issue:** XML 文档 cref 指向 `Estimates` 集合而不是 `EstimatePoints`
- **Fix:** 修正为 `EstimatePoints`
- **Files modified:** `src/Modules/WorkItems/Modules.WorkItems/Domain/Estimate.cs`

**4. [Rule 1 - Build] Contracts DTO EstimateDto CA2227/CA1002 — List<T> 属性警告**

- **Found during:** Task 1
- **Issue:** `TreatWarningsAsErrors` 导致 List<EstimatePointDto> 属性被标记为 CA2227/CA1002
- **Fix:** 在 Contracts csproj 中添加 `CA1002;CA2227` 到 NoWarn
- **Files modified:** `src/Modules/WorkItems/Modules.WorkItems.Contracts/Modules.WorkItems.Contracts.csproj`

### Intentional Deviations from Plan

1. **Contracts csproj 额外引用:** 添加了 `Workspace.Contracts` 和 `Mediator.Abstractions` 以匹配 Project.Contracts 的依赖链，避免 Mediator 源生成器错误
2. **EF migration startup-project:** 使用 `YH.Flow.Api`（含 Design 包）代替 `YH.Flow.DbMigrator`（不含 Design 包）

## Verification

### Truth Statements

| #   | Statement                                                      | Status |
| --- | -------------------------------------------------------------- | ------ |
| 1   | WorkItems contracts and module projects compile independently  | PASS   |
| 2   | State entity implements IHasTenant with 5 Group enum values    | PASS   |
| 3   | Label entity exists with Name, Color, ParentId                 | PASS   |
| 4   | Estimate entity exists with EstimatePoint child entities       | PASS   |
| 5   | WorkItemsDbContext exists in yhschema.WorkItems schema         | PASS   |
| 6   | EF migration creates States, Labels, Estimates, EstimatePoints | PASS   |
| 7   | WorkItemsModule registered with Order 260                      | PASS   |
| 8   | Test project compiles and references the module                | PASS   |

### Build Verification

```text
dotnet build src/YH.Flow.slnx --nologo → 62 projects, 0 warnings, 0 errors
```

### Migration Verification

Migration `20260624062358_InitialWorkItems` creates:

- `yhschema.WorkItems.States` — with unique (TenantId, ProjectId, Name) filtered index, query indexes
- `yhschema.WorkItems.Labels` — with unique (TenantId, ProjectId, Name) filtered index, ParentId FK (SetNull)
- `yhschema.WorkItems.Estimates` — with tenant index, IsLastUsed index
- `yhschema.WorkItems.EstimatePoints` — with unique (EstimateId, Key, TenantId) index, Estimate FK (Cascade)

## Self-Check: PASSED

All 3 tasks committed. All files confirmed existing. SUMMARY.md created.
