---
phase: 16-issue
plan: 01
subsystem: web-client
tags: [issue, mock-data, tanstack-query, mobx, routes]
requires: [15-04]
provides: [issue-data-layer, issue-hooks, issue-store, issue-routes]
affects: [mock-data, hooks, store, routes]
tech-stack:
  added:
    - "@hello-pangea/dnd@18.0.1: Kanban drag-and-drop"
    - "cmdk@1.1.1: Command palette primitive"
  patterns:
    - "TanStack Query v5 hooks with mock data + delay"
    - "MobX IssueStore managing UI-only state"
    - "Flat route definitions in workspace-layout group"
key-files:
  created:
    - "yh-flow/clients/web/src/lib/hooks/use-issues.ts"
    - "yh-flow/clients/web/src/lib/hooks/use-comments.ts"
    - "yh-flow/clients/web/app/store/issue.store.ts"
  modified:
    - "yh-flow/clients/web/package.json"
    - "yh-flow/clients/web/src/lib/mock-data.ts"
    - "yh-flow/clients/web/src/lib/hooks/index.ts"
    - "yh-flow/clients/web/app/store/types.ts"
    - "yh-flow/clients/web/app/store/root.store.ts"
    - "yh-flow/clients/web/app/routes.ts"
    - "pnpm-lock.yaml"
metrics:
  duration: "~25 min"
  completed_date: "2026-06-29"
  tasks_total: 3
  tasks_completed: 3
  commits: 3
  files_changed: 10
type_check_errors: 0 (in our files; 36 pre-existing in editor subpackage)
---

# Phase 16 Plan 01: Issue Infrastructure Layer

搭建 Phase 16 的基础设施层：安装新依赖（@hello-pangea/dnd、cmdk），扩展 mock-data.ts 添加 Issue/State/Label/Comment/Activity 数据，创建 TanStack Query hooks，创建 MobX IssueStore（仅管理 UI 状态），注册 IssueStore 到 CoreRootStore，添加 Issue 列表和详情路由。

## Completed Tasks

| Task | Name                                               | Commit    | Key Files                                                    |
| ---- | -------------------------------------------------- | --------- | ------------------------------------------------------------ |
| 1    | Install deps + extend mock data                    | 9a917dfe7 | package.json, mock-data.ts, pnpm-lock.yaml                   |
| 2    | Create TanStack Query hooks                        | cf23f31e7 | use-issues.ts, use-comments.ts, hooks/index.ts, mock-data.ts |
| 3    | Create IssueStore, register routes, register store | 6b1771539 | issue.store.ts, types.ts, root.store.ts, routes.ts           |

## Task Details

### Task 1: Dependencies and Mock Data

- Installed `@hello-pangea/dnd@18.0.1` and `cmdk@1.1.1` (runtime dependencies)
- Extended `mock-data.ts` with 6 new exports:
  - **MOCK_STATES** (10 states): 5 per project (proj-1/proj-2), standard Plane state groups (backlog/unstarted/started/completed/cancelled)
  - **MOCK_LABELS** (6 labels): 前端/后端/Bug for proj-1; API/数据库/文档 for proj-2
  - **MOCK_ISSUES** (20 issues): 12 in proj-1 (Flow 前端), 8 in proj-2 (Flow API), all TIssue fields populated
  - **MOCK_COMMENTS** (16 comments across 7 issues): Full TIssueComment type with actor/workspace/project details
  - **MOCK_ISSUE_ACTIVITIES** (15 records): TIssueActivity type with field/verb/actor tracking
  - Exported helper types (\_ws1Detail, \_proj1Detail, \_proj2Detail, \_user1Detail) for hook usage

### Task 2: TanStack Query Hooks

- **use-issues.ts**: 3 named exports + 1 type export
  - `useIssues(projectId, filters?)` — filtered issue list with state/priority/assignee/searchQuery support
  - `useIssue(projectId, issueId)` — single issue detail
  - `useIssueMutations()` — createIssue, updateIssue (with optimistic update + rollback), deleteIssue (soft delete), bulkUpdateIssues
  - `IssueFilters` type export for filter parameter typing
- **use-comments.ts**: 4 named exports
  - `useComments(issueId)` — comment list for an issue
  - `useCreateComment()` / `useUpdateComment()` / `useDeleteComment()` — CRUD mutations with cache invalidation
- **hooks/index.ts**: added barrel exports for both new hook modules

### Task 3: IssueStore, Routes, Store Registration

- **IssueStore** (`app/store/issue.store.ts`): MobX UI state store
  - Observable fields: selectedIssueIds, activeView, currentPage, pageSize, groupBy, expandedColumnIds, filters (4 sub-fields), sortBy, sortDirection
  - Actions: toggleIssueSelection, selectAll, clearSelection, setActiveView, setCurrentPage, setGroupBy, toggleColumnExpand, setFilters (partial merge), setSortBy, setSortDirection, clearFilters
  - All fields/methods decorated via `makeObservable` with appropriate observable/action annotations
- **types.ts**: re-exports `IIssueStore` from issue.store.ts
- **root.store.ts**: added `issue: IIssueStore` field to ICoreRootStore interface, instantiated in CoreRootStore constructor
- **routes.ts**: added 2 issue routes inside workspace-layout:
  - `workspaces/:workspaceId/projects/:projectId/issues` → `app/issues/page.tsx`
  - `workspaces/:workspaceId/projects/:projectId/issues/:issueId` → `app/issues/[issueId]/page.tsx`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Pre-existing oxlint warnings blocking commit (erasing-op on dayMs \* 0)**

- Found during: Task 1 commit
- Issue: husky pre-commit hook runs `oxlint --fix --deny-warnings` and 3 pre-existing warnings in MOCK_ACTIVITIES (existing `dayMs * 0` expressions) were treated as errors by `--deny-warnings`, blocking the commit
- Fix: Simplified `epoch: now - dayMs * 0` to `epoch: now`, and date calls from `new Date(now - dayMs * 0)` to `new Date(now)` — same behavior, zero warnings
- Files modified: mock-data.ts
- Commit: 9a917dfe7

**2. [Rule 1 - Bug] TIssueComment and TIssueActivity field type mismatches in mock-data.ts**

- Found during: Task 1 type check
- Issue: Missing `TIssuePriorities` import (used in helper function type assertions); `old_value`/`new_value` using `?? null` but types expect `string | undefined`; `source_data.extra` required but missing
- Fix: Added TIssuePriorities to imports, changed `?? null` to `?? undefined`, added `extra: {}` to source_data
- Files modified: mock-data.ts
- Commit: 9a917dfe7

**3. [Rule 3 - Blocking] package.json reading fixed after replace_all mistake**

- Found during: Task 1 (replace_all replaced ALL "0" characters instead of matching specific line)
- Issue: Wrong Edit replace_all matched too broadly
- Fix: Used `git checkout -- mock-data.ts` to restore, then reapplied changes carefully
- Files modified: mock-data.ts
- No separate commit (part of Task 1 commit)

### Auto-Added Missing Functionality

**1. [Rule 2 - Missing export] mock-data.ts helper objects**

- Found during: Task 2 implementation
- Issue: use-comments.ts needs to construct minimal TIssueComment objects but required detail types (\_ws1Detail, \_proj1Detail, \_user1Detail) were module-private
- Fix: Exported these 4 helper objects from mock-data.ts so hooks can construct minimal TIssueComment/TIssueActivity objects
- Files modified: mock-data.ts
- Commit: cf23f31e7

**2. [Rule 2 - Missing type export] IssueFilters type**

- Found during: Task 2 implementation
- Issue: useIssues function uses a filters parameter with a structured type, but without an exported type consumers can't properly type their filter objects
- Fix: Exported `IssueFilters` type from use-issues.ts
- Files modified: use-issues.ts
- Commit: cf23f31e7

## Known Stubs

- `app/issues/page.tsx` and `app/issues/[issueId]/page.tsx` do not exist yet — routes reference them but pages will be created in Plan 16-02 (Issue list view) and Plan 16-03 (Issue detail view). This is intentional per the plan's scope boundary.
- All hooks return mock data — the real API integration is deferred to future phases. This is the established Phase 15 mock-layer pattern.

## Verification

- `npx tsc --noEmit`: 0 new errors (36 pre-existing errors in editor subpackage only)
- `pnpm ls @hello-pangea/dnd cmdk`: both installed at expected versions
- 10 files created/modified, all committed individually

## Self-Check: PASSED
