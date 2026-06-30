---
phase: 21-infrastructure-foundation
plan: 02
subsystem: frontend
tags:
  - tanstack-query
  - query-keys
  - cache-management
  - infrastructure
dependency_graph:
  requires: []
  provides:
    - queryKeys namespace for all 13 modules
    - Individual factory exports per module
  affects:
    - yh-flow/clients/web/src/lib/hooks/*.ts
tech-stack:
  added:
    - "@tanstack/react-query (v5) — query key factory pattern"
  patterns:
    - "as const readonly tuple keys for type-safe cache management"
key-files:
  created:
    - yh-flow/clients/web/src/lib/services/query-keys.ts
  modified: []
decisions:
  - "Use 'as const' readonly tuple pattern for all factory return types, fully compatible with TanStack Query v5's QueryKey (ReadonlyArray<unknown>)"
  - "Each module exports both its named factory object (e.g. issueKeys) and is included in the combined queryKeys namespace"
  - "Key naming follows plural convention for root keys (e.g. ['workspaces'], ['projects'], ['issues']) matching TanStack Query best practices for query key hierarchy"
metrics:
  duration: null
  completed_date: 2026-06-30
---

# Phase 21 Plan 02: 集中式查询键工厂 query-keys.ts — Summary

**Objective:** 创建集中式查询键工厂 `query-keys.ts`，统一管理所有模块的 TanStack Query 缓存键，消除 hooks 中的内联字符串键。

## 结果

- 创建 `yh-flow/clients/web/src/lib/services/query-keys.ts`，包含全部 13 个模块的查询键工厂
- 所有工厂函数返回 `as const` 只读元组，完全兼容 TanStack Query v5 的 `QueryKey` 类型
- 导出 `queryKeys` 组合命名空间和每个模块的独立工厂对象

## 任务执行情况

### Task 1: 创建 query-keys.ts 集中式查询键工厂

**状态:** 完成

**包含的 13 个模块:**

| 模块         | 工厂函数                                                                        | 键结构                                                                                                       |
| ------------ | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| workspace    | `all()`, `detail(id)`, `current()`                                              | `["workspaces"]`, `["workspaces", id]`, `["workspaces", "current"]`                                          |
| project      | `all()`, `list(wsId)`, `detail(wsId, projId)`                                   | `["projects"]`, `["projects", wsId]`, `["projects", wsId, projId]`                                           |
| member       | `all()`, `list(wsId)`                                                           | `["members"]`, `["members", wsId]`                                                                           |
| issue        | `all()`, `list(projId, filters?)`, `detail(projId, issueId)`                    | `["issues"]`, `["issues", projId, filters]`, `["issues", projId, issueId]`                                   |
| state        | `all()`, `list(projId)`                                                         | `["states"]`, `["states", projId]`                                                                           |
| label        | `all()`, `list(projId)`                                                         | `["labels"]`, `["labels", projId]`                                                                           |
| comment      | `all()`, `list(issueId)`                                                        | `["comments"]`, `["comments", issueId]`                                                                      |
| cycle        | `all()`, `list(projId)`, `detail(projId, cycleId)`, `progress(projId, cycleId)` | `["cycles"]`, `["cycles", projId]`, `["cycles", projId, cycleId]`, `["cycles", projId, cycleId, "progress"]` |
| module       | `all()`, `list(projId)`, `detail(projId, modId)`, `links(modId)`                | `["modules"]`, `["modules", projId]`, `["modules", projId, modId]`, `["modules", modId, "links"]`            |
| page         | `all()`, `list(wsId)`, `detail(wsId, pageId)`, `archived(wsId)`                 | `["pages"]`, `["pages", wsId]`, `["pages", wsId, pageId]`, `["pages", "archived", wsId]`                     |
| view         | `all()`, `list(projId)`, `detail(viewId)`                                       | `["views"]`, `["views", projId]`, `["views", viewId]`                                                        |
| notification | `all()`, `list(wsId)`, `unreadCount(wsId)`                                      | `["notifications"]`, `["workspace-notifications", wsId]`, `["workspace-unread-count", wsId]`                 |
| analytics    | `all()`, `dashboard(wsId)`                                                      | `["analytics"]`, `["analytics", wsId]`                                                                       |

**提交:** `5883e131d`

## Verification

- [x] `query-keys.ts` 文件存在
- [x] 包含全部 13 个模块的键工厂
- [x] 导出 `queryKeys` 命名空间对象
- [x] 每个模块工厂同时单独命名导出
- [x] 所有工厂函数返回 `as const` 只读元组
- [x] TypeScript 编译通过（无新错误）
- [x] 键命名与现有 hooks 的字符串键模式兼容

## Deviations from Plan

无 — 计划完全按照原样执行。

## Key Decisions

1. 采用 `as const` 只读元组模式，利用 TypeScript 的 `readonly` 推断完全兼容 TanStack Query v5 的 `ReadonlyArray<unknown>` QueryKey 类型
2. 每个模块同时提供命名导出和 `queryKeys` 组合命名空间两种导入方式，方便 hooks 灵活选择
3. 注意命名采用复数惯例（`["workspaces"]`、`["projects"]`、`["issues"]`），符合 TanStack Query 最佳实践中的键层次结构

## Self-Check: PASSED

- [x] `yh-flow/clients/web/src/lib/services/query-keys.ts` — 文件存在
- [x] 提交 `5883e131d` — 存在 (`git log --oneline | grep 5883e131d`)
