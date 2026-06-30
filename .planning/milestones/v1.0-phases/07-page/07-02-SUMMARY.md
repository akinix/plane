---
phase: 07-page
plan: 02
subsystem: Page
tags: [page, api, endpoints, crud, archive, description, favorite, cqrs]
requires: [07-01]
provides: [REQ-7.1, REQ-7.2]
affects: [PageModule, PageDbContext]
tech-stack:
  added: []
  patterns:
    - Vertical-slice feature directories (Endpoint + Handler + Validator)
    - FluentValidation for input validation
    - Mediator CQRS (ICommand / IQuery pattern)
    - [RequireWorkspaceRole] authorization on write endpoints
    - [RequireAuthorization] on read-only endpoints
    - PageDtoMapper (ToDto / ToDetailDto) for entity-to-DTO projection
key-files:
  created:
    - src/Modules/Page/Modules.Page/Features/v1/Pages/PageDtoMapper.cs
    - src/Modules/Page/Modules.Page/Features/v1/Pages/CreatePage/(Endpoint+Handler+Validator)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/GetPage/(Endpoint+Handler)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/UpdatePage/(Endpoint+Handler+Validator)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/DeletePage/(Endpoint+Handler+Validator)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/ListPages/(Endpoint+Handler)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/GetPageSummary/(Endpoint+Handler)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/ArchivePage/(Archive/Unarchive Endpoint+Handler)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/GetPageDescription/(Endpoint+Handler)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/UpdatePageDescription/(Endpoint+Handler+Validator)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/AddFavorite/(Endpoint+Handler)
    - src/Modules/Page/Modules.Page/Features/v1/Pages/RemoveFavorite/(Endpoint+Handler)
  modified:
    - src/Modules/Page/Modules.Page/PageModule.cs (full endpoint registration)
    - src/Modules/Page/Modules.Page.Contracts/v1/Pages/AddFavorite/AddFavoriteCommand.cs
    - src/Modules/Page/Modules.Page.Contracts/v1/Pages/RemoveFavorite/RemoveFavoriteCommand.cs
    - src/Modules/Page/Modules.Page.Contracts/v1/Pages/CreatePage/CreatePageCommand.cs
decisions:
  - CreatePageEndpoint extracts projectId from route parameter and sets command.ProjectId
  - CreatePageCommandHandler auto-creates ProjectPage bridge entity alongside Page
  - ListPages defaults to top-level pages (ParentId == null), sorted by SortOrder then -CreatedAt
  - GetPage and GetPageDescription use [RequireAuthorization] (any authenticated user can read)
  - All write endpoints use [RequireWorkspaceRole(Admin, Member)]
  - Favorite operations are idempotent (re-adding returns true, removing non-existent returns true)
  - RemoveFavorite uses ISoftDeletable soft-delete pattern (not hard delete)
  - AddFavoriteCommand and RemoveFavoriteCommand extract UserId from ClaimsPrincipal (not client input)
metrics:
  duration: ~25 minutes
  completed_date: 2026-06-25
---

# Phase 7 Plan 2: Page API Endpoints Summary

**One-liner:** 12 Page API 端点实现 — 核心 CRUD、Summary、归档/恢复、Description CRUD、收藏管理，全部通过 PageModule.cs 路由注册。

## Tasks

### Task 1: Create PageDtoMapper + Create/Get/Update/Delete endpoints (e4a5db774)

**Files created (12):** PageDtoMapper.cs, CreatePage (Endpoint+Handler+Validator), GetPage (Endpoint+Handler), UpdatePage (Endpoint+Handler+Validator), DeletePage (Endpoint+Handler+Validator)

**Key implementation details:**

- PageDtoMapper 使用 `internal static class` 模式，提供 `ToDto()` 和 `ToDetailDto()` 两个方法
- CreatePage 自动创建 ProjectPage 桥接实体（Page ↔ Project M2M through）
- CreatePageCommand 添加 `[JsonIgnore] ProjectId` 字段，从路由参数提取
- GetPage 使用 `.RequireAuthorization()`（任意认证用户可读）
- Update/Delete 使用 `.RequireWorkspaceRole(Admin, Member)` 授权
- DeletePage 实现软删除（`SoftDelete(DateTimeOffset.UtcNow)`）

### Task 2: Create ListPages + GetPageSummary + Archive/Unarchive endpoints (54adefc54)

**Files created (8):** ListPages (Endpoint+Handler), GetPageSummary (Endpoint+Handler), ArchivePage (Endpoint+Handler), UnarchivePage (Endpoint+Handler)

**Key implementation details:**

- ListPages 默认仅返回顶层页面（`ParentId == null`），按 `SortOrder` + `-CreatedAt` 排序
- 支持分页（PageNumber/PageSize）和归档筛选（IsArchived）
- GetPageSummary 返回匿名对象：total_pages, total_archived_pages, recently_updated（Top 5）
- ArchivePage 设置 `ArchivedAt = DateTimeOffset.UtcNow`
- UnarchivePage 清除 `ArchivedAt = null`

### Task 3: Create Description CRUD + Favorite endpoints + PageModule wiring (836e40ce6)

**Files created/modified (10):** GetPageDescription (Endpoint+Handler), UpdatePageDescription (Endpoint+Handler+Validator), AddFavorite (Endpoint+Handler), RemoveFavorite (Endpoint+Handler), PageModule.cs (full wiring)

**Key implementation details:**

- GetPageDescription 使用 `.RequireAuthorization()`（公开读取）
- UpdatePageDescription 使用 `.RequireWorkspaceRole(Admin, Member)` 授权
- AddFavorite 幂等：检查是否已收藏，已存在直接返回 true
- RemoveFavorite 使用 ISoftDeletable 软删除（不硬删除）
- AddFavoriteCommand/RemoveFavoriteCommand 添加 `[JsonIgnore] UserId` 字段，从 ClaimsPrincipal 提取
- PageModule.cs MapEndpoints 注册全部 12 组端点

## API 端点总览

| 方法   | 路由                        | 端点名                | 授权          | 返回                 |
| ------ | --------------------------- | --------------------- | ------------- | -------------------- |
| POST   | /pages/                     | CreatePage            | Admin/Member  | 201 + PageDetailDto  |
| GET    | /pages/{pageId}             | GetPage               | Authenticated | 200 + PageDetailDto  |
| PATCH  | /pages/{pageId}             | UpdatePage            | Admin/Member  | 200 + PageDetailDto  |
| DELETE | /pages/{pageId}             | DeletePage            | Admin/Member  | 204 NoContent        |
| GET    | /pages/                     | ListPages             | Admin/Member  | 200 + PageDto[]      |
| GET    | /pages-summary/             | GetPageSummary        | Admin/Member  | 200 + summary object |
| POST   | /pages/{pageId}/archive     | ArchivePage           | Admin/Member  | 200 + bool           |
| DELETE | /pages/{pageId}/archive     | UnarchivePage         | Admin/Member  | 200 + bool           |
| GET    | /pages/{pageId}/description | GetPageDescription    | Authenticated | 200 + PageDetailDto  |
| PATCH  | /pages/{pageId}/description | UpdatePageDescription | Admin/Member  | 200 + PageDetailDto  |
| POST   | /pages/{pageId}/favorite    | AddFavorite           | Admin/Member  | 200 + bool           |
| DELETE | /pages/{pageId}/favorite    | RemoveFavorite        | Admin/Member  | 200 + bool           |

## Deviations from Plan

### Rule 2 - Auto-add missing critical functionality

**1. AddFavoriteCommand 缺少 UserId 字段**

- **Found during:** Task 1 (pre-task)
- **Issue:** AddFavoriteCommand 和 RemoveFavoriteCommand 在 Plan 01 中建立时缺少 UserId 字段，但 handler 需要从 ClaimsPrincipal 提取用户 ID
- **Fix:** 为两个 Command 添加 `[JsonIgnore] public string UserId { get; set; }` 字段
- **Files modified:**
  - `src/Modules/Page/Modules.Page.Contracts/v1/Pages/AddFavorite/AddFavoriteCommand.cs`
  - `src/Modules/Page/Modules.Page.Contracts/v1/Pages/RemoveFavorite/RemoveFavoriteCommand.cs`

**2. CreatePageCommand 缺少 ProjectId 字段**

- **Found during:** Task 1
- **Issue:** CreatePageCommand 缺少 `ProjectId` 字段，但 CreatePageHandler 需要它来创建 ProjectPage 桥接实体
- **Fix:** 添加 `[JsonIgnore] public Guid ProjectId { get; set; }` 字段，由 Endpoint 从路由参数 {projectId} 设置
- **Files modified:**
  - `src/Modules/Page/Modules.Page.Contracts/v1/Pages/CreatePage/CreatePageCommand.cs`
  - `src/Modules/Page/Modules.Page/Features/v1/Pages/CreatePage/CreatePageEndpoint.cs`

### Rule 1 - Auto-fix bugs

**1. PageDtoMapper.ToDetailDto 空引用错误**

- **Found during:** Task 1 build
- **Issue:** `ToDto(p) as PageDetailDto` 返回 null（ToDto 返回 PageDto，PageDetailDto 是子类，as 转换失败）
- **Fix:** 重写 ToDetailDto 直接创建 `new PageDetailDto { ... }` 并填充所有继承的属性和描述字段
- **Files modified:**
  - `src/Modules/Page/Modules.Page/Features/v1/Pages/PageDtoMapper.cs`

## Verification Results

### Build

```powershell
dotnet build src/YH.Flow.slnx --nologo
```

**Result:** 0 错误, 0 警告

### File Verification

All 30 files confirmed present (29 new + 1 modified: PageModule.cs).

### Endpoint Registration Checks

- [x] PageModule.cs MapEndpoints 注册了全部 12 个端点
- [x] 所有写端点使用 `[RequireWorkspaceRole(Admin, Member)]`
- [x] GetPage/GetPageDescription 使用 `[RequireAuthorization]`
- [x] CreatePage 自动创建 ProjectPage 桥接实体
- [x] ListPages 默认仅返回顶层页面，按 SortOrder + -CreatedAt 排序

## Threat Surface Scan

| Flag                          | File                       | Description                                                                                |
| ----------------------------- | -------------------------- | ------------------------------------------------------------------------------------------ |
| threat_flag: auth_enforcement | All Endpoint.cs files      | 写端点使用 `[RequireWorkspaceRole(Admin, Member)]`，读端点使用 `[RequireAuthorization]`    |
| threat_flag: sensitive_data   | AddFavorite/RemoveFavorite | UserId 使用 `[JsonIgnore]` 从 ClaimsPrincipal 提取，不接受客户端输入（T-7-02-03 mitigate） |

## Self-Check: PASSED

- [x] 所有 30 个文件已确认存在
- [x] 3 次提交全部通过 git 验证
- [x] Build 0 错误
- [x] PageModule.cs 包含完整端点注册
- [x] 偏差已记录
- [x] 威胁面已扫描
