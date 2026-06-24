---
phase: 03-project
plan: 03
subsystem: Project
tags: [project, crud, endpoints, handlers, validators, mapper, modules]
requires: [03-02]
provides: [REQ-3.1]
affects: [Modules.Project, Modules.Project.Contracts]
tech-stack:
  added: []
  patterns: [Vertical Slice, Minimal API RouteGroupBuilder, FluentValidation, Mediator CQRS, IHasTenant, ISoftDeletable]
key-files:
  created:
    - src/Modules/Project/Modules.Project.Contracts/v1/Projects/CreateProject/CreateProjectCommand.cs
    - src/Modules/Project/Modules.Project.Contracts/v1/Projects/CreateProject/CreateProjectResponse.cs
    - src/Modules/Project/Modules.Project.Contracts/v1/Projects/GetProject/GetProjectQuery.cs
    - src/Modules/Project/Modules.Project.Contracts/v1/Projects/UpdateProject/UpdateProjectCommand.cs
    - src/Modules/Project/Modules.Project.Contracts/v1/Projects/DeleteProject/DeleteProjectCommand.cs
    - src/Modules/Project/Modules.Project.Contracts/v1/Projects/ListProjects/ListProjectsQuery.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/ProjectDtoMapper.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/CreateProject/CreateProjectEndpoint.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/CreateProject/CreateProjectCommandHandler.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/CreateProject/CreateProjectCommandValidator.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/GetProject/GetProjectEndpoint.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/GetProject/GetProjectQueryHandler.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/UpdateProject/UpdateProjectEndpoint.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/UpdateProject/UpdateProjectCommandHandler.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/UpdateProject/UpdateProjectCommandValidator.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/DeleteProject/DeleteProjectEndpoint.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/DeleteProject/DeleteProjectCommandHandler.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/DeleteProject/DeleteProjectCommandValidator.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/ListProjects/ListProjectsEndpoint.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/ListProjects/ListProjectsQueryHandler.cs
    - src/Modules/Project/Modules.Project/Features/v1/Projects/ListProjects/ListProjectsQueryValidator.cs
  modified:
    - src/Modules/Project/Modules.Project/Modules.Project.csproj
    - src/Modules/Project/Modules.Project/ProjectModule.cs
metrics:
  duration: 0h 45m
  completed_date: 2026-06-24
decisions:
  - "Added Modules.Workspace project reference to Modules.Project for RequireWorkspaceRole extension method access"
  - "ListProjects uses RequireWorkspaceRole(Guest, Member, Admin) workspace membership check before network filter"
  - "UpdateProject uses ICurrentUser + ICurrentWorkspaceContext for dual-gate authorization (workspace Admin bypass, Member requires project Admin)"
  - "Deleted migration files not regenerated — no schema changes in this plan"
---

# Phase 3 Wave 3: Project CRUD 端点实现

实现所有 5 个 Project CRUD 端点：Create/Get/Update/Delete/List。在 Workspace 模块的 Vertical Slice 模式基础上，完成 ProjectModule 的路由注册。

## 构建结果

- **Contracts 项目:** 0 警告, 0 错误
- **完整解决方案:** 0 警告, 0 错误 (所有 49 个项目)

## 任务完成情况

### Task 1: 创建 v1 Contracts (6 个文件)

| 文件                       | 说明                                                                                |
| -------------------------- | ----------------------------------------------------------------------------------- |
| `CreateProjectCommand.cs`  | 包含所有字段：name, identifier, network, 特性开关, OwnerUserId (JsonIgnore)         |
| `CreateProjectResponse.cs` | 记录类型 `(Guid Id, string Slug)`                                                   |
| `GetProjectQuery.cs`       | `IQuery<ProjectDto>`, ProjectId (JsonIgnore)                                        |
| `UpdateProjectCommand.cs`  | `ICommand<ProjectDto>`, 所有可变字段为 nullable (PATCH 语义)                        |
| `DeleteProjectCommand.cs`  | `ICommand`, ProjectId + CurrentUserId                                               |
| `ListProjectsQuery.cs`     | `IQuery<PlanePagedResult<ProjectDto>>`, 含 Network/OrderBy 过滤器, 实现 IPagedQuery |

### Task 2: 实现端点 + 处理程序 + 验证器 + Mapper (16 个文件)

| 端点          | HTTP                  | 路由                                         | 授权                                                | 特殊逻辑 |
| ------------- | --------------------- | -------------------------------------------- | --------------------------------------------------- | -------- |
| CreateProject | POST `/`              | `RequireWorkspaceRole(Admin, Member)`        | 自动创建 ProjectMember(Admin) 给 owner + lead       |
| GetProject    | GET `/{projectId}`    | `RequireAuthorization()`                     | 不含 network 过滤 (Plane 模式)                      |
| UpdateProject | PATCH `/{projectId}`  | `RequireWorkspaceRole(Admin, Member)`        | 双重授权：workspace Admin 绕过, Member 需项目 Admin |
| DeleteProject | DELETE `/{projectId}` | `RequireWorkspaceRole(Admin)`                | 软删除 + slug \_\_{epoch} 释放 (D-03)               |
| ListProjects  | GET `/`               | `RequireWorkspaceRole(Guest, Member, Admin)` | network 过滤器: Public(2) OR is_member              |

**关键行为:**

- **CreateProjectHandler:** 使用 `Project.Create(...)` 工厂方法创建项目，自动创建 `ProjectMember.Create(...)` 将创建者设为 Admin。如果 `ProjectLeadId` 不同于 owner，也将其添加为 Admin。
- **ListProjectsQueryHandler:** 实现 Plane 的 `Q(network=2) | Q(is_member=True)` 过滤模式。成员看到所有项目，非项目成员只看到 Public 项目。支持按 `name`/`created_at`/`updated_at`/`sort_order` 排序和分页。
- **DeleteProjectCommandHandler:** 双重授权检查 — 端点要求 workspace Admin，处理程序额外检查 ProjectMember 的 Admin 角色。调用 `project.SoftDelete(DateTimeOffset.UtcNow)` 释放 slug。
- **UpdateProjectCommandHandler:** 使用 `ICurrentUser` 和 `ICurrentWorkspaceContext` 进行项目级授权检查。workspace Admin 绕过项目角色检查。
- **ProjectModule.cs:** 将所有 5 个端点注册在 `api/v{version:apiVersion}/workspaces/{slug}/projects` 组下。

## 偏离说明

### 自动修复 (Rule 1/2/3)

| 规则   | 问题                                                                                | 修复                                                                      |
| ------ | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Rule 1 | `WithTags()` 在 `RouteGroupBuilder` 上缺失 — 缺少 `using Microsoft.AspNetCore.Http` | 已添加 using                                                              |
| Rule 1 | CA1861: 内联 `new[] { "PATCH" }` 数组参数                                           | 移至 `static readonly` 字段                                               |
| Rule 1 | CA1308: `ToLowerInvariant()` 用于 OrderBy 字段名比较                                | 添加 pragma suppression                                                   |
| Rule 1 | CA1862: `id == id.ToUpperInvariant()` 触发分析器警告                                | 改为 `string.Equals(id, id.ToUpperInvariant(), StringComparison.Ordinal)` |
| Rule 1 | CS1574: 无法解析的 cref `IHasTenant`                                                | 改为 `<c>IHasTenant</c>`                                                  |
| Rule 2 | Project 实现项目需要引用 Workspace 实现获取 `RequireWorkspaceRole` 扩展方法         | 已添加 `Modules.Workspace` csproj 引用                                    |

### 未修复的已知问题

无 — 计划按预期执行。

### 威胁标记

无 — 所有端点路由和授权均在计划的 threat_model 范围内。

### Known Stubs

无。

## 自检

- [x] 所有 22 个创建/修改的文件已确认存在
- [x] Task 1 提交: `de2fc433b`
- [x] Task 2 提交: `f1b6deaaf`
- [x] 完整解决方案构建: 0 警告, 0 错误
