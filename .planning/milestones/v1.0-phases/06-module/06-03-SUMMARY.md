---
phase: 06-module
plan: 03
subsystem: WorkItems
tags: [module, module-issues, module-links, progress, integration-tests]
requires: [06-02]
provides: [ModuleIssue-association, ModuleLink-CRUD, Module-progress-tracking]
affects: [WorkItemsDbContext, WorkItemsModule]
tech-stack:
  added: []
  patterns:
    - "Module-Issue M2M via ModuleIssue bridge entity (soft-deletable)"
    - "ModuleLink CRUD with title/url/metadata and soft-delete"
    - "Module progress: real-time LINQ aggregation via ModuleIssues -> Issues -> States -> StateGroup"
    - "Module has no COMPLETED restriction (unlike Cycle)"
key-files:
  created:
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Issues/AddIssuesToModuleCommand.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Issues/RemoveIssueFromModuleCommand.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Issues/ListModuleIssuesQuery.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/GetModuleProgress/GetModuleProgressQuery.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Links/AddModuleLinkCommand.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Links/RemoveModuleLinkCommand.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Links/ListModuleLinksQuery.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/ModuleProgressDto.cs"
    - "src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/ModuleLinkDto.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/AddIssuesToModule/Endpoint.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/AddIssuesToModule/Handler.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/AddIssuesToModule/Validator.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/RemoveIssueFromModule/Endpoint.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/RemoveIssueFromModule/Handler.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/ListModuleIssues/Endpoint.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/ListModuleIssues/Handler.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModuleProgress/Endpoint.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModuleProgress/Handler.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/AddModuleLink/Endpoint.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/AddModuleLink/Handler.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/AddModuleLink/Validator.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/RemoveModuleLink/Endpoint.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/RemoveModuleLink/Handler.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/ListModuleLinks/Endpoint.cs"
    - "src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/ListModuleLinks/Handler.cs"
    - "src/Tests/WorkItems.Tests/Integration/ModuleCrudTests.cs"
    - "src/Tests/WorkItems.Tests/Integration/ModuleIssueAndProgressTests.cs"
  modified:
    - "src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule.cs"
decisions: []
metrics:
  duration: ""
  completed_date: "2026-06-24"
---

# Phase 6 Plan 3: Module-Issue Association + ModuleLink CRUD + Progress Tracking

Module-Issue 多对多关联管理（Add/Remove/List）、ModuleLink 外部资源链接 CRUD（Add/Remove/List）、实时进度聚合端点，以及端到端集成测试。

## Task Summary

| #   | Name                                                                                            | Type | Status | Commit      |
| --- | ----------------------------------------------------------------------------------------------- | ---- | ------ | ----------- |
| 1   | ModuleIssue contracts + AddIssuesToModule/RemoveIssueFromModule/ListModuleIssues feature slices | auto | done   | `bfb83f713` |
| 2   | GetModuleProgress + ModuleLink (Add/Remove/List) feature slices                                 | auto | done   | `a4e3b3c73` |
| 3   | Wire endpoints in WorkItemsModule + create integration tests                                    | auto | done   | `dd3e86d0e` |

## Verification Results

### Build

- `dotnet build src/YH.Flow.slnx` -- 0 errors, 0 warnings

### Endpoints Registered

On module route group (`api/v1/workspaces/{slug}/projects/{projectId}/modules/`):

- **POST** `/{moduleId}/module-issues` -- AddIssuesToModule (Admin/Member)
- **DELETE** `/{moduleId}/module-issues/{issueId}` -- RemoveIssueFromModule (Admin/Member)
- **GET** `/{moduleId}/module-issues` -- ListModuleIssues (Auth)
- **GET** `/{moduleId}/progress` -- GetModuleProgress (Auth)
- **POST** `/{moduleId}/links` -- AddModuleLink (Admin/Member)
- **DELETE** `/{moduleId}/links/{linkId}` -- RemoveModuleLink (Admin/Member)
- **GET** `/{moduleId}/links` -- ListModuleLinks (Auth)

### Total Module endpoints in this wave: 7

### Total Module endpoints (Phase 6): 15 (5 CRUD + 3 archive + 3 Module-Issue + 3 ModuleLink + 1 Progress)

## Architecture Decisions

1. **Module has no COMPLETED restriction** -- Unlike Cycle, any Module (including completed modules) can accept issues. This matches Plane behavior.
2. **Real-time progress aggregation** -- No progress snapshot caching. Each request computes via LINQ join: ModuleIssues -> Issues -> States -> Group by StateGroup. Future optimization can add caching if needed.
3. **ModuleLink DTO uses snake_case `created_at`** -- Follows Plane JSON naming convention established in ModuleDto/CycleDto.
4. **Soft-delete for all associations** -- ModuleIssue and ModuleLink both use ISoftDeletable, consistent with CycleIssue pattern.

## Deviations from Plan

None -- plan executed exactly as written.

## Self-Check: PASSED

All files verified:

- 10 contracts/feature files for Module-Issue endpoints
- 11 contracts/feature/DTO files for GetModuleProgress + ModuleLink
- 1 modified WorkItemsModule.cs with all 7 endpoint registrations
- 2 integration test files (ModuleCrudTests: 10 tests, ModuleIssueAndProgressTests: 7 tests)
- Build 0 errors 0 warnings
