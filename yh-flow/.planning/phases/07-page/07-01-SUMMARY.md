---
phase: 07-page
plan: 01
subsystem: "Page Module Scaffold"
tags: ["page", "scaffold", "domain", "ef-core", "contracts", "migrations"]
requires: []
provides:
  [
    "Page module infrastructure",
    "Domain entities",
    "EF configs",
    "Contracts DTOs",
    "Command/Query stubs",
    "EF migration",
    "Test scaffolding",
  ]
affects: ["Host/YH.Flow.Api", "Host/YH.Flow.DbMigrator", "Host/YH.Flow.Migrations.PostgreSQL"]
tech-stack:
  added: ["Modules.Page", "Modules.Page.Contracts"]
  patterns:
    [
      "IGlobalEntity entity",
      "Self-referencing FK",
      "Conditional unique index with HasFilter",
      "PATCH-semantics Update method",
    ]
key-files:
  created:
    - "src/Modules/Page/Modules.Page.Contracts/Modules.Page.Contracts.csproj"
    - "src/Modules/Page/Modules.Page.Contracts/AssemblyInfo.cs"
    - "src/Modules/Page/Modules.Page.Contracts/Constants/PageConstants.cs"
    - "src/Modules/Page/Modules.Page.Contracts/DTOs/PageDto.cs"
    - "src/Modules/Page/Modules.Page.Contracts/DTOs/PageDetailDto.cs"
    - "src/Modules/Page/Modules.Page.Contracts/DTOs/ProjectPageDto.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/CreatePage/CreatePageCommand.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/CreatePage/CreatePageResponse.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/GetPage/GetPageQuery.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/UpdatePage/UpdatePageCommand.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/DeletePage/DeletePageCommand.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/ListPages/ListPagesQuery.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/GetPageSummary/GetPageSummaryQuery.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/GetPageDescription/GetPageDescriptionQuery.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/UpdatePageDescription/UpdatePageDescriptionCommand.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/ArchivePage/ArchivePageCommand.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/ArchivePage/UnarchivePageCommand.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/AddFavorite/AddFavoriteCommand.cs"
    - "src/Modules/Page/Modules.Page.Contracts/v1/Pages/RemoveFavorite/RemoveFavoriteCommand.cs"
    - "src/Modules/Page/Modules.Page/Modules.Page.csproj"
    - "src/Modules/Page/Modules.Page/AssemblyInfo.cs"
    - "src/Modules/Page/Modules.Page/PageModule.cs"
    - "src/Modules/Page/Modules.Page/PageModuleConstants.cs"
    - "src/Modules/Page/Modules.Page/Domain/Page.cs"
    - "src/Modules/Page/Modules.Page/Domain/ProjectPage.cs"
    - "src/Modules/Page/Modules.Page/Domain/PageFavorite.cs"
    - "src/Modules/Page/Modules.Page/Data/PageDbContext.cs"
    - "src/Modules/Page/Modules.Page/Data/Configurations/PageConfiguration.cs"
    - "src/Modules/Page/Modules.Page/Data/Configurations/ProjectPageConfiguration.cs"
    - "src/Modules/Page/Modules.Page/Data/Configurations/PageFavoriteConfiguration.cs"
    - "src/Tests/Page.Tests/Page.Tests.csproj"
    - "src/Tests/Page.Tests/Usings.cs"
    - "src/Tests/Page.Tests/TestData/TestPageFactory.cs"
    - "src/Tests/Page.Tests/Domain/PageDomainTests.cs"
    - "src/Tests/Page.Tests/Domain/ProjectPageDomainTests.cs"
    - "src/Tests/Page.Tests/Domain/PageFavoriteDomainTests.cs"
    - "src/Host/YH.Flow.Migrations.PostgreSQL/Pages/20260625002919_AddPages.cs"
  modified:
    - "src/YH.Flow.slnx"
    - "src/Host/YH.Flow.DbMigrator/Program.cs"
    - "src/Host/YH.Flow.DbMigrator/YH.Flow.DbMigrator.csproj"
    - "src/Host/YH.Flow.Api/Program.cs"
    - "src/Host/YH.Flow.Migrations.PostgreSQL/YH.Flow.Migrations.PostgreSQL.csproj"
decisions:
  - "Page implements IGlobalEntity (no TenantId, tenant resolved via route slug per CONTEXT.md Claude's Discretion)"
  - "ProjectPage implements IHasTenant + ISoftDeletable (tenant-scoped bridge entity)"
  - "PageFavorite implements IHasTenant + ISoftDeletable (conditional unique index for dedup)"
  - "PageAccess enum: Public=0, Private=1 (match Plane API contract)"
  - "Self-referencing FK ParentId -> Page.Id with DeleteBehavior.SetNull (avoid circular cascade)"
  - "Not including PageConstants in Mediator assembly scan (Module.Page.Contracts doesn't reference Mediator.Abstractions)"
  - "Description/VewProps/LogoProps columns: PostgreSQL text type (not nvarchar(max))"
metrics:
  duration: "~45 min"
  completed_date: "2026-06-25"
---

# Phase 7 Plan 01: Page Module Scaffold Summary

Page 模块的基础设施创建完成：项目脚手架、3 个领域实体（Page / ProjectPage / PageFavorite）、EF 配置 + DbContext、AddPages 迁移、Contracts DTOs、13 个 Command/Query 桩代码、模块注册、测试脚手架。

## Key Decisions

### 实体模型设计

Page 实体选择 `IGlobalEntity` 而非 `IHasTenant`（per CONTEXT.md Claude's Discretion）。Page 通过 workspace slug 路由上下文确定租户，不需要 TenantId 列。ProjectPage 和 PageFavorite 则实现 `IHasTenant`，通过 Finbuckle 自动进行租户隔离。

### 层级结构

自引用 FK `ParentId → Page.Id` 实现树形嵌套，`DeleteBehavior.SetNull` 避免循环级联删除路径。

### EF 配置

- Page 表：无 TenantId 索引（IGlobalEntity），ProjectId + SortOrder 复合索引用于列表排序
- ProjectPages 表：条件唯一索引 `(TenantId, ProjectId, PageId)` 带 HasFilter
- PageFavorites 表：条件唯一索引 `(TenantId, PageId, UserId)` 带 HasFilter

### PostgreSQL 兼容性

描述和 JSON props 列使用 `text` 类型（而非 SQL Server 的 `nvarchar(max)`），确保与 PostgreSQL 兼容。

## Deviations from Plan

**1. [Rule 2 - Correctness] 未在 DbMigrator/API Mediator 扫描中添加 PageConstants**

- **Found during:** Task 1 构建
- **Issue:** `typeof(YH.Modules.Page.Contracts.PageConstants)` 在 MediatorOptions.Assemblies 列表中导致 MSG0007 错误，因为 Modules.Page.Contracts 不引用 Mediator.Abstractions
- **Fix:** 移除 PageConstants 的 Mediator 注册。PageConstants 不是 Mediator handler，不需要扫描
- **Files modified:** `src/Host/YH.Flow.Api/Program.cs`, `src/Host/YH.Flow.DbMigrator/Program.cs`

**2. [Rule 2 - Correctness] 使用 PostgreSQL text 类型而非 nvarchar(max)**

- **Found during:** Task 3 迁移生成
- **Issue:** `HasColumnType("nvarchar(max)")` 是 SQL Server 类型，PostgreSQL 不支持
- **Fix:** 将 PageConfiguration 中的 `nvarchar(max)` 改为 `text`
- **Files modified:** `src/Modules/Page/Modules.Page/Data/Configurations/PageConfiguration.cs`

## Threat Surface

| Flag                      | File            | Description                                                                                                                                                     |
| ------------------------- | --------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| threat_flag: new_endpoint | `PageModule.cs` | Page 路由组桩（`/api/v{version}/workspaces/{slug}/projects/{projectId}/pages`）在 plan 07-02 接入端点时需通过 `[RequireWorkspaceRole]` 进行租户隔离 (T-7-01-01) |

## Known Stubs

| Stub                              | File            | Line  | Reason                                     |
| --------------------------------- | --------------- | ----- | ------------------------------------------ |
| Route group placeholder           | `PageModule.cs` | 67-70 | Endpoints registered in plan 07-02         |
| Route group placeholder (summary) | `PageModule.cs` | 74-76 | Summary endpoints registered in plan 07-02 |

## Verification

- `dotnet build src/YH.Flow.slnx` — 0 errors, 0 warnings
- Page.Tests — 23/23 passed (domain unit tests)
- Workspace.Tests — 100/100 passed (regression)
- Project.Tests — passed (regression)
- WorkItems.Tests — 264/264 passed (regression)

## Self-Check: PASSED

All created files verified. All modified files verified. Both commits exist in git log.

## WAVE COMPLETE
