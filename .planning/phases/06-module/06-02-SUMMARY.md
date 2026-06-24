---
phase: 06-module
plan: 02
subsystem: WorkItems
tags: [module, crud, archive, vertical-slice]
dependency_graph:
  requires: [06-01]
  provides: [06-03]
  affects: []
tech-stack:
  added: []
  patterns:
    - Vertical Slice architecture (Feature folders: Endpoint + Handler + Validator)
    - CQRS via Mediator (BUnit's Mediator library)
    - Multi-tenant via IHasTenant + Finbuckle global query filter
    - Soft delete via ISoftDeletable
    - Issue annotation counts via ModuleIssues join pattern
key-files:
  created:
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/CreateModule/CreateModuleCommand.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/CreateModule/CreateModuleResponse.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/GetModule/GetModuleQuery.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/UpdateModule/UpdateModuleCommand.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/DeleteModule/DeleteModuleCommand.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ListModules/ListModulesQuery.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ArchiveModule/ArchiveModuleCommand.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ArchiveModule/UnarchiveModuleCommand.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ArchiveModule/ListArchivedModulesQuery.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ModuleDtoMapper.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleCommandHandler.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleCommandValidator.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModule/GetModuleEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModule/GetModuleQueryHandler.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UpdateModule/UpdateModuleEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UpdateModule/UpdateModuleCommandHandler.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UpdateModule/UpdateModuleCommandValidator.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/DeleteModule/DeleteModuleEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/DeleteModule/DeleteModuleCommandHandler.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListModules/ListModulesEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListModules/ListModulesQueryHandler.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ArchiveModule/ArchiveModuleEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ArchiveModule/ArchiveModuleCommandHandler.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UnarchiveModule/UnarchiveModuleEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UnarchiveModule/UnarchiveModuleCommandHandler.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListArchivedModules/ListArchivedModulesEndpoint.cs
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListArchivedModules/ListArchivedModulesQueryHandler.cs
  modified:
    - yh-flow/src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule.cs
decisions:
  - "Module DTO mapping: Status maps directly from stored field (m.Status), unlike Cycle which computes via CycleDtoMapper.ComputeStatus()"
  - "No COMPLETED edit restriction on Module (unlike Cycle D-03)"
  - "No date restrictions on Module archive — any status can be archived (unlike Cycle which requires EndDate in the past)"
  - "Module CRUD uses RequireWorkspaceRole(Admin, Member) for write, RequireAuthorization for read (per D-12)"
  - "ListModules status filter is DB-level (stored field), not in-memory filter (unlike Cycle's computed status)"
metrics:
  duration: ~15 min
  completed_date: 2026-06-24
---

# Phase 6 Plan 2: Module CRUD Endpoints Summary

**实现 8 个 Module Vertical Slice（Create/Get/Update/Delete/List + Archive/Unarchive/ListArchived）+ ModuleDtoMapper + WorkItemsModule 路由注册。**

Module CRUD 遵循与 Cycle 相同的垂直切片模式，关键差异点：Module Status 为存储字段（非动态计算）、Module 无 COMPLETED 编辑限制、Module 任何状态均可归档（无日期限制）。

## Tasks

| Task | Name                                                                                               | Commit    | Files                                          |
| ---- | -------------------------------------------------------------------------------------------------- | --------- | ---------------------------------------------- |
| 1    | Create Module CRUD contracts + implement Create/Get/Update/Delete feature slices + ModuleDtoMapper | 57e3727fe | 16 files (5 contracts, 11 features)            |
| 2    | Implement ListModules with status filter                                                           | 857cb3845 | 3 files (1 contract, 2 features)               |
| 3    | Implement Archive/Unarchive/ListArchivedModules + Wire endpoints in WorkItemsModule                | bec0c0537 | 10 files (3 contracts, 6 features, 1 modified) |

## Endpoints Summary

| HTTP   | Route                                                                      | Authorization | Description                                  |
| ------ | -------------------------------------------------------------------------- | ------------- | -------------------------------------------- |
| POST   | /api/v1/workspaces/{slug}/projects/{projectId}/modules/                    | Admin/Member  | Create module (status defaults to "planned") |
| GET    | /api/v1/workspaces/{slug}/projects/{projectId}/modules/                    | Authenticated | List modules (optional ?status= filter)      |
| GET    | /api/v1/workspaces/{slug}/projects/{projectId}/modules/{moduleId}          | Authenticated | Get module by id                             |
| PATCH  | /api/v1/workspaces/{slug}/projects/{projectId}/modules/{moduleId}          | Admin/Member  | Update module (no COMPLETED restriction)     |
| DELETE | /api/v1/workspaces/{slug}/projects/{projectId}/modules/{moduleId}          | Admin/Member  | Soft-delete module                           |
| POST   | /api/v1/workspaces/{slug}/projects/{projectId}/modules/{moduleId}/archive  | Admin/Member  | Archive module (any status)                  |
| DELETE | /api/v1/workspaces/{slug}/projects/{projectId}/archived-modules/{moduleId} | Admin/Member  | Unarchive module                             |
| GET    | /api/v1/workspaces/{slug}/projects/{projectId}/archived-modules/           | Authenticated | List archived modules                        |

## Key Design Decisions

1. **Status 映射**: ModuleDto 的 Status 直接从 `m.Status` 存储字段映射（Cycle 使用 `ComputeStatus()` 动态计算）
2. **编辑限制**: Module 无 COMPLETED 编辑限制（Cycle D-03 规定 COMPLETED 后只能更新 limited fields）
3. **归档限制**: Module 任何状态均可归档（Cycle 要求 EndDate < now）
4. **List 筛选**: ListModules 使用 DB-level filter（`Where(m => m.Status == status)`），非 in-memory 过滤
5. **独立路由组**: Archived modules 使用独立路由组 `/archived-modules/`（与 Cycle 模式一致）

## Verification

- [x] `dotnet build src/YH.Flow.slnx --nologo` — 0 errors, 0 warnings
- [x] 每个 Task 单独提交
- [x] WorkItemsModule.cs 注册了所有 8 个 Module 端点

## Deviations from Plan

None - 计划按预期完整执行，无偏差。

## Known Stubs

None - 所有端点已完全实现并注册。

## Threat Flags

None - 所有端点路由匹配计划的威胁模型（T-6-crud-01: 多租户隔离由 BaseDbContext 全局查询过滤器处理；T-6-crud-02: 写操作使用 RequireWorkspaceRole）。

## Self-Check: PASSED

所有文件的创建和提交已验证。

---

**Duration:** ~15 minutes
**Commits:** 57e3727fe, 857cb3845, bec0c0537
