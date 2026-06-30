---
phase: 06-module
plan: 01
subsystem: WorkItems
tags:
  - module
  - domain
  - entity
  - ef-migration
  - contracts
  - tests
dependency_graph:
  requires: []
  provides:
    - Module domain entities
    - Module EF configurations
    - AddModules migration
    - ModuleContracts (Constants + DTOs)
    - WorkItemsDbContext extension
  affects:
    - 06-02 Module CRUD endpoints
    - 06-03 Module-Issue association + integration tests
tech-stack:
  added:
    - YH.Modules.WorkItems.Contracts.Constants.ModuleConstants
    - YH.Modules.WorkItems.Contracts.DTOs.ModuleDto
    - YH.Modules.WorkItems.Domain.Module (IHasTenant, ISoftDeletable, IAuditableEntity)
    - YH.Modules.WorkItems.Domain.ModuleIssue (IHasTenant, ISoftDeletable)
    - YH.Modules.WorkItems.Domain.ModuleMember (IHasTenant, ISoftDeletable)
    - YH.Modules.WorkItems.Domain.ModuleLink (IHasTenant, ISoftDeletable)
    - YH.Modules.WorkItems.Data.Configurations.ModuleConfiguration (+3 query indexes)
    - YH.Modules.WorkItems.Data.Configurations.ModuleIssueConfiguration (+unique index)
    - YH.Modules.WorkItems.Data.Configurations.ModuleMemberConfiguration (+unique index)
    - YH.Modules.WorkItems.Data.Configurations.ModuleLinkConfiguration (+query index)
  patterns:
    - Domain entity pattern: IHasTenant + ISoftDeletable + IAuditableEntity, private ctor, static factory
    - EF Configuration pattern: ToTable with SchemaName, HasFilter unique indexes, query indexes
    - Contracts pattern: Constants class + DTO with JsonPropertyName snake_case
    - Test pattern: internal static factory class + sealed xUnit test class with Shouldly assertions
key-files:
  created:
    - src/Modules/WorkItems/Modules.WorkItems.Contracts/Constants/ModuleConstants.cs
    - src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/ModuleDto.cs
    - src/Modules/WorkItems/Modules.WorkItems/Domain/Module.cs
    - src/Modules/WorkItems/Modules.WorkItems/Domain/ModuleIssue.cs
    - src/Modules/WorkItems/Modules.WorkItems/Domain/ModuleMember.cs
    - src/Modules/WorkItems/Modules.WorkItems/Domain/ModuleLink.cs
    - src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleConfiguration.cs
    - src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleIssueConfiguration.cs
    - src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleMemberConfiguration.cs
    - src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleLinkConfiguration.cs
    - src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/20260624095427_AddModules.cs
    - src/Tests/WorkItems.Tests/TestData/TestModuleFactory.cs
    - src/Tests/WorkItems.Tests/Domain/ModuleDomainTests.cs
    - src/Tests/WorkItems.Tests/Domain/ModuleIssueDomainTests.cs
    - src/Tests/WorkItems.Tests/Domain/ModuleMemberDomainTests.cs
    - src/Tests/WorkItems.Tests/Domain/ModuleLinkDomainTests.cs
  modified:
    - src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule.cs (added Module route stub)
    - src/Modules/WorkItems/Modules.WorkItems/Data/WorkItemsDbContext.cs (added 4 DbSet properties)
    - src/Tests/WorkItems.Tests/Usings.cs (added ModuleConstants global using)
    - src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/WorkItemsDbContextModelSnapshot.cs
decisions: []
metrics:
  duration: null
  completed_date: 2026-06-24
---

# Phase 6 Plan 01: Module Domain Foundation — Summary

创建 Module 领域实体、EF 配置、AddModules 迁移、Contracts DTOs，以及测试脚手架。Module 纳入 WorkItemsDbContext（`yhschema.WorkItems` schema），与 Issue/Cycle 共享同一个 DbContext。

## Tasks

| #   | Name                                                                                                       | Type | Status | Commit      |
| --- | ---------------------------------------------------------------------------------------------------------- | ---- | ------ | ----------- |
| 1   | ModuleContracts（Constants + ModuleDto）+ WorkItemsModule Route stub                                       | auto | Done   | `15b3f2694` |
| 2   | Module/ModuleIssue/ModuleMember/ModuleLink domain entities + EF configurations + WorkItemsDbContext update | auto | Done   | `98780c521` |
| 3   | AddModules EF migration + TestModuleFactory + Domain tests                                                 | auto | Done   | `cda8c0487` |

## Key Results

### Task 1 — Contracts + Route Stub

- **ModuleConstants.cs**: `NameMaxLength=255`, `DescriptionMaxLength=10000`, `DefaultSortOrder=65535.0`, `DefaultStatus="planned"`, `StatusMaxLength=20`
- **ModuleDto.cs**: 所有 Module 字段 + 6 个 computed annotation 字段（TotalIssues, CompletedIssues, CancelledIssues, StartedIssues, UnstartedIssues, BacklogIssues）, snake_case JSON via JsonPropertyName
- **WorkItemsModule.cs**: 添加 Module 路由组桩代码 `api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/modules`
- 修复: SonarAnalyzer S1481 警告 — 添加 `#pragma warning disable S1481` 处理未使用的 stub 变量

### Task 2 — Domain Entities + EF Configurations

- **Module.cs**: IHasTenant + ISoftDeletable + IAuditableEntity, 13 个字段, Create/Update/UpdateStatus/Archive/Unarchive/FreezeSnapshot/SoftDelete 方法
- **ModuleIssue.cs**: M2M through entity (IssueId, ModuleId), IHasTenant + ISoftDeletable
- **ModuleMember.cs**: M2M through entity (MemberId, ModuleId), IHasTenant + ISoftDeletable
- **ModuleLink.cs**: Link entity (Title, Url, Metadata), IHasTenant + ISoftDeletable, Update PATCH 语义
- **ModuleConfiguration.cs**: 4 个索引 — 1 unique (TenantId, ProjectId, Name) HasFilter + 3 query indexes
- **ModuleIssueConfiguration.cs**: 唯一索引 (TenantId, ModuleId, IssueId) HasFilter
- **ModuleMemberConfiguration.cs**: 唯一索引 (TenantId, ModuleId, MemberId) HasFilter
- **ModuleLinkConfiguration.cs**: 查询索引 (TenantId, IsDeleted, ModuleId)
- **WorkItemsDbContext.cs**: 添加 4 个 DbSet（Modules, ModuleIssues, ModuleMembers, ModuleLinks）

### Task 3 — Migration + Tests

- **AddModules 迁移**: 在 `yhschema.WorkItems` 创建 Modules/ModuleIssues/ModuleMembers/ModuleLinks 表，所有唯一索引和查询索引正确
- **TestModuleFactory.cs**: 4 个工厂类（Module/ModuleIssue/ModuleMember/ModuleLink）
- **ModuleDomainTests.cs**: 13 个测试 — Create/Update/UpdateStatus/Archive/Unarchive/SoftDelete
- **ModuleIssueDomainTests.cs**: 5 个测试 — Create 验证 + SoftDelete
- **ModuleMemberDomainTests.cs**: 5 个测试 — Create 验证 + SoftDelete
- **ModuleLinkDomainTests.cs**: 6 个测试 — Create/Update/SoftDelete
- **总计 29 个测试全部通过**

## Deviations from Plan

### Rule 2 — Auto-fix missing critical functionality

1. **Module.cs 缺少 `using YH.Modules.WorkItems.Contracts.Constants`**
   - 发现于: Task 2
   - 问题: Module.cs 引用 `ModuleConstants.DefaultStatus` 和 `ModuleConstants.DefaultSortOrder`，但缺少 contracts 命名空间的 using 声明
   - 修复: 添加 `using YH.Modules.WorkItems.Contracts.Constants;`
   - 文件: `Domain/Module.cs`
   - 提交: `98780c521`

### Rule 1 — Auto-fix bugs

1. **SonarAnalyzer S1481 "unused local variable" — WorkItemsModule.cs**
   - 发现于: Task 1 构建
   - 问题: Module 路由组桩代码的 `var modules` 变量未使用，由于项目启用了 TreatWarningsAsErrors，构建失败
   - 修复: 添加 `#pragma warning disable S1481` / `#pragma warning restore S1481` 包裹 stub 代码
   - 文件: `WorkItemsModule.cs`
   - 提交: `15b3f2694`

## Verification Results

| Check                                                     | Result               |
| --------------------------------------------------------- | -------------------- |
| `dotnet build src/YH.Flow.slnx --nologo`                  | 0 errors, 0 warnings |
| Modules/ModuleIssues/ModuleMembers/ModuleLinks DbSet 存在 | Pass                 |
| ModuleConfiguration 4 个索引正确                          | Pass                 |
| ModuleIssueConfiguration 唯一索引正确                     | Pass                 |
| ModuleMemberConfiguration 唯一索引正确                    | Pass                 |
| ModuleLinkConfiguration 查询索引正确                      | Pass                 |
| AddModules 迁移在 `WorkItems/` 目录下                     | Pass                 |
| ModuleDto 包含 computed 字段                              | Pass                 |
| 29 个 Module 领域测试全部通过                             | Pass                 |

## Threat Surface Scan

无新增威胁面 — 所有新代码遵循现有 IHasTenant + ISoftDeletable + IAuditableEntity 模式，在现有 Trust Boundaries 内。

## Self-Check: PASSED

- [x] 18 个创建/修改文件全部存在
- [x] 3 个提交存在（15b3f2694, 98780c521, cda8c0487）
- [x] 构建 0 errors
- [x] 测试 29/29 通过
