---
phase: 18-cycle-module
plan: 01
type: execute
tags: [data-layer, mock-data, tanstack-query, mobx, cycle, module]
requires: [18-CONTEXT, 18-UI-SPEC]
provides: [useCycles, useModules, useCycleIssues, useModuleIssues, CycleStore, ModuleStore]
affects: [mock-data, root-store, store-types]
decisions:
  - "遵循 Phase 16 的 D-P16-02 混合状态管理：TanStack Query 存数据，MobX 存 UI 状态"
  - "CycleStore UI 状态覆盖 activeTab/selectedCycleId/viewLayout/cycleModal/deleting/transferModal"
  - "ModuleStore UI 状态覆盖 selectedModuleId/moduleModal/deleting/activeView/filters"
  - "Mock Cycles/Modules 与现有 Mock Issues 通过 cycle_id/module_ids 双向关联"
tech-stack:
  added:
    - "TanStack Query v5 — useCycles, useModules, useCycleIssues, useModuleIssues"
    - "MobX — CycleStore, ModuleStore (makeObservable/observable/action)"
  patterns:
    - "delay() + MOCK_* array direct mutation for mock TanStack Query hooks"
    - "乐观更新 (onMutate/onError/onSettled) on remove-from-cycle/module"
    - "invalidateQueries on mutations for cache refresh"
key-files:
  created:
    - yh-flow/clients/web/src/lib/hooks/use-cycles.ts
    - yh-flow/clients/web/src/lib/hooks/use-modules.ts
    - yh-flow/clients/web/src/lib/hooks/use-cycle-issues.ts
    - yh-flow/clients/web/src/lib/hooks/use-module-issues.ts
    - yh-flow/clients/web/app/store/cycle.store.ts
    - yh-flow/clients/web/app/store/module.store.ts
  modified:
    - yh-flow/clients/web/src/lib/mock-data.ts
    - yh-flow/clients/web/app/store/root.store.ts
    - yh-flow/clients/web/app/store/types.ts
metrics:
  duration: "~30 分钟"
  completed_date: "2026-06-29"
---

# Phase 18 Plan 01: 数据层基础设施 — Summary

**目的:** 为 Cycle/Module 组件层提供完整的数据获取和 UI 状态管理能力。

数据层基础设施完成：Mock 数据（5 Cycles + 4 Modules + Issue 关联）、4 个 TanStack Query hooks 文件、2 个 MobX stores，以及 root store 注册。

---

## 已完成任务

| 任务 | 名称                       | Commit      | 文件                        |
| ---- | -------------------------- | ----------- | --------------------------- |
| 1    | Mock 数据扩展              | `411c71b66` | `mock-data.ts`              |
| 2    | use-cycles.ts hooks        | `bedcb6ae9` | `use-cycles.ts`             |
| 3    | use-modules.ts hooks       | `4b1d25316` | `use-modules.ts`            |
| 4    | use-cycle-issues.ts hooks  | `4f3309f`   | `use-cycle-issues.ts`       |
| 5    | use-module-issues.ts hooks | `5e9fd7294` | `use-module-issues.ts`      |
| 6    | CycleStore                 | `d25033c06` | `cycle.store.ts`            |
| 7    | ModuleStore                | `b573e9287` | `module.store.ts`           |
| 8    | Root store 注册            | `ca1feb45a` | `root.store.ts`, `types.ts` |

---

## 各任务详情

### Task 1: Mock 数据扩展

- **MOCK_CYCLES**: 5 个 Cycle，覆盖 current/upcoming/completed(2)/draft 状态，跨 proj-1(4) 和 proj-2(1)
  - `cycle-active-1` — 当前活跃周期，start_date=今天-14天, end_date=今天+14天，含完整 progress_snapshot
  - `cycle-upcoming-1` — 即将开始的周期，start_date=今天+15天
  - `cycle-completed-1/2` — 两个已完成周期，含历史 progress_snapshot
  - `cycle-draft-1` — 草稿周期，start_date/end_date 均为 null

- **MOCK_MODULES**: 4 个 Module，覆盖 planned/in-progress/completed/cancelled 状态，跨 proj-1(3) 和 proj-2(1)
  - `mod-proj1-1` — 用户认证模块 (in-progress)
  - `mod-proj1-2` — UI 重构 (planned)
  - `mod-proj1-3` — 性能优化 (completed)
  - `mod-proj2-1` — Flow API 核心 (in-progress)

- **Issue 关联**:
  - issue-1, issue-2, issue-5 → cycle_id = "cycle-active-1"
  - issue-8 → cycle_id = "cycle-upcoming-1"
  - issue-4 → module_ids = ["mod-proj1-1"]
  - issue-7 → module_ids = ["mod-proj1-1", "mod-proj1-2"]
  - issue-9 → module_ids = ["mod-proj2-1"]

### Task 2: use-cycles.ts

4 个 TanStack Query hooks:

- `useCycles(projectId)` — projectId 过滤，queryKey: ["cycles", projectId]
- `useCycleDetail(projectId, cycleId)` — 单 Cycle 查询
- `useCycleMutations()` — createCycle/updateCycle/deleteCycle + invalidateQueries
- `useCycleProgress(projectId, cycleId)` — 进度快照查询

### Task 3: use-modules.ts

4 个 TanStack Query hooks:

- `useModules(projectId)` — projectId 过滤
- `useModuleDetail(projectId, moduleId)` — 单 Module 查询
- `useModuleMutations()` — createModule/updateModule/deleteModule
- `useModuleLinkMutations(moduleId)` — addLink/updateLink/removeLink 管理链接

### Task 4: use-cycle-issues.ts

4 个 TanStack Query hooks:

- `useCycleIssues(projectId, cycleId)` — 按 cycleId 过滤 MOCK_ISSUES
- `useAddIssueToCycle()` — 设置 issue.cycle_id
- `useRemoveIssueFromCycle()` — 乐观更新 + onMutate/onError/onSettled
- `useTransferCycleIssues()` — 批量将 fromCycleId 的 Issue 转移到 toCycleId

### Task 5: use-module-issues.ts

3 个 TanStack Query hooks:

- `useModuleIssues(projectId, moduleId)` — 按 module_ids.includes 过滤
- `useAddIssueToModule()` — 向 issue.module_ids 追加 moduleId
- `useRemoveIssueFromModule()` — 从 issue.module_ids 移除 moduleId，乐观更新

### Task 6: CycleStore (MobX)

`ICycleStore` 接口 + `CycleStore` 类：

- **UI 状态**: activeTab, selectedCycleId, viewLayout, cycleModalOpen/mode, cycleDeleting, transferModalOpen
- **Actions**: setActiveTab, setSelectedCycleId, setViewLayout, openCycleModal, closeCycleModal, setCycleDeleting, setTransferModalOpen, reset
- **默认值**: activeTab="active", viewLayout="list"

### Task 7: ModuleStore (MobX)

`IModuleStore` 接口 + `ModuleStore` 类：

- **UI 状态**: selectedModuleId, moduleModalOpen/mode, moduleDeleting, activeView, filters (status + searchQuery)
- **Actions**: setSelectedModuleId, openModuleModal, closeModuleModal, setModuleDeleting, setActiveView, setFilters, reset

### Task 8: Root Store 注册

- `ICoreRootStore` 新增 `cycle: ICycleStore` 和 `module: IModuleStore`
- `CoreRootStore` 构造函数创建 `CycleStore` + `ModuleStore` 实例
- `types.ts` 导出 `ICycleStore` 和 `IModuleStore` 类型
- `useStore().cycle.activeTab` 和 `useStore().module.selectedModuleId` 可直接访问

---

## 验证

```bash
# 所有 8 个关键文件存在
mock cycles ok
mock modules ok
useCycles ok
useModules ok
useCycleIssues ok
useModuleIssues ok
cycle store registered
module store registered
```

- TypeScript 编译 (`npx tsc --noEmit`)：Phase 18-01 文件零错误
- oxlint：零警告（Task 1、3、6、7 中见 lint 警告已修复）

---

## 关键决策

1. **TCycleGroups 类型不需要在 use-cycles.ts 中使用** — 移除 lint 警告
2. **TViewLayout 类型在 module.store.ts 未使用** — 移除以通过 lint
3. **MOCK_ISSUES 在 use-modules.ts 中未使用** — 移除导入

---

## Deviations from Plan

**无 — 计划完全按设计执行，未发现需要自动修正的 bug 或功能遗漏。**

### Auto-fixed Issues

**1. [Rule 3 - 阻塞] 修复 lint 警告导致的 commit 失败**

- **发现于:** Task 2, Task 3, Task 6, Task 7
- **问题:** 未使用的 import 导致 pre-commit hook (oxlint --deny-warnings) 失败
- **修复:** 移除未使用的类型 import (TCycleGroups, TModuleStatus, TViewLayout, MOCK_ISSUES)
- **文件:** use-cycles.ts, use-modules.ts, cycle.store.ts, module.store.ts
- **Commit:** `bedcb6ae9`, `4b1d25316`, `d25033c06`, `b573e9287`

---

## `useStore()` 访问路径

```typescript
// Cycle UI 状态
useStore().cycle.activeTab; // "active" | "completed" | "all"
useStore().cycle.selectedCycleId; // string | null
useStore().cycle.viewLayout; // "list" | "board"

// Module UI 状态
useStore().module.selectedModuleId; // string | null
useStore().module.activeView; // "list" | "gantt"
useStore().module.filters.status; // TModuleStatus[] | undefined
```

---

## Self-Check: PASSED

- [x] 所有 8 个文件存在
- [x] mock-data.ts: MOCK_CYCLES (5个) + MOCK_MODULES (4个) + issue 关联
- [x] use-cycles.ts: useCycles, useCycleDetail, useCycleMutations, useCycleProgress
- [x] use-modules.ts: useModules, useModuleDetail, useModuleMutations, useModuleLinkMutations
- [x] use-cycle-issues.ts: useCycleIssues, useAddIssueToCycle, useRemoveIssueFromCycle, useTransferCycleIssues
- [x] use-module-issues.ts: useModuleIssues, useAddIssueToModule, useRemoveIssueFromModule
- [x] cycle.store.ts: ICycleStore + CycleStore (MobX observable/action)
- [x] module.store.ts: IModuleStore + ModuleStore (MobX observable/action)
- [x] root.store.ts: cycle + module 注册到 ICoreRootStore
- [x] types.ts: ICycleStore + IModuleStore 导出
- [x] TypeScript 编译无新增错误
