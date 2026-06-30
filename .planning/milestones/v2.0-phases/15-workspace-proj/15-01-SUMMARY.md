---
phase: 15-workspace-proj
plan: 01
wave: 1
type: summary
status: delivered
requirements: [UI-05]
created: 2026-06-29
---

# Phase 15, Plan 01 — 基础设施 Summary

## Objective

搭建 Phase 15 基础设施层：安装 @tanstack/react-query 做服务端状态管理，创建 WorkspaceStore 和 ProjectStore（MobX，仅管理 UI 状态 per D-P15-08），创建 mock 数据层（D-P15-12）和 TanStack Query hooks，提取 EmojiPicker 组件（D-P15-10）。

## Changes Made

### New Files

| File                                       | Purpose                                                                                             |
| ------------------------------------------ | --------------------------------------------------------------------------------------------------- |
| `app/store/types.ts`                       | IWorkspaceStore / IProjectStore 接口定义                                                            |
| `app/store/workspace.store.ts`             | WorkspaceStore（sidebarCollapsed、workspaceSwitcherOpen、expandedWorkspaceIds、currentWorkspaceId） |
| `app/store/project.store.ts`               | ProjectStore（expandedProjectIds、projectFilterText）                                               |
| `src/lib/mock-data.ts`                     | Mock 数据：2 个工作区、4 个项目、7 个成员、8 条活动                                                 |
| `src/lib/hooks/use-workspaces.ts`          | useWorkspaces / useWorkspace / useCurrentWorkspace（TanStack Query）                                |
| `src/lib/hooks/use-projects.ts`            | useProjects / useProject（TanStack Query）                                                          |
| `src/lib/hooks/use-members.ts`             | useMembers（TanStack Query）                                                                        |
| `src/lib/ui/emoji-picker/emoji-picker.tsx` | EmojiPicker 组件（搜索 + 网格 + 弹窗）                                                              |
| `src/lib/ui/emoji-picker/index.ts`         | EmojiPicker barrel export                                                                           |

### Modified Files

| File                      | Change                                                                    |
| ------------------------- | ------------------------------------------------------------------------- |
| `package.json`            | 添加 `@tanstack/react-query@^5.0.0` 依赖                                  |
| `app/provider.tsx`        | 添加 QueryClientProvider（staleTime: 5min, retry: 1），包裹 StoreProvider |
| `app/store/root.store.ts` | 注册 WorkspaceStore 和 ProjectStore 到 CoreRootStore                      |
| `src/lib/hooks/index.ts`  | 添加 use-workspaces、use-projects、use-members barrel export              |
| `src/lib/ui/index.ts`     | 添加 emoji-picker barrel export                                           |

## Verification

- `npx tsc --noEmit` — 全部新创建/修改文件通过编译（仅有预存 fork 问题：React 19 类型兼容性导致 61 个 error，不在本 Plan 作用域内）

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing functionality] EmojiPicker 使用纯 div/button 而非 @headlessui/react Popover**

- **Found during:** Task 3
- **Issue:** @headlessui/react v2 + React 19 类型不兼容，Popover.Button children 的类型签名要求 `ReactNode | ((bag: ButtonRenderPropArg) => ReactElement)`，而 React 19 的 ReactNode 包含了 `Promise<AwaitedReactNode>` 导致 TS2322。同样 Transition as={Fragment} 也不兼容 React 19 Fragment 类型。
- **Fix:** 使用 useState + useOutsideClick 自定义实现弹窗，保留完整的 EmojiPicker API（value/onChange/label/className）和功能（搜索过滤、emoji 网格、面板关闭）。
- **Files modified:** `emoji-picker/emoji-picker.tsx`
- **Commit:** eb1b3fb08

**2. [Rule 3 - Blocking issue] worktree 缺少 node_modules 和 yh-flow 源码**

- **Found during:** Task 1
- **Issue:** 工作区基于 upstream Plane 代码库，不包含 yh-flow/clients/web 目录和 node_modules。pnpm workspace 未配置 yh-flow 路径导致依赖无法解析。
- **Fix:** 通过 `git checkout preview -- yh-flow/clients/web/` 恢复源码，临时添加 yh-flow 到 pnpm-workspace.yaml 运行 `pnpm install` 后恢复。
- **Commit:** 225631eb1

## Decisions Made

- **D-15-01-01:** 使用 `react-popper` 的 `useOutsideClick` 自定义实现 EmojiPicker 弹窗，替代 @headlessui/react Popover（原因：React 19 类型兼容性问题）
- **D-15-01-02:** ProjectStore 和 WorkspaceStore 只管理 UI 状态（sidebarCollapsed、expandedProjectIds 等），服务端数据通过 TanStack Query hooks 管理，符合 D-P15-08

## Key Metrics

- **Duration:** ~90 minutes
- **Commits:** 3 (Task 1: 225631eb1, Task 2: ad01c3c83, Task 3: eb1b3fb08)
- **Files created:** 10
- **Files modified:** 5

## Self-Check: PASSED

- All 10 created files verified on disk
- All 4 modified files verified on disk
- All 3 commit hashes verified in git log
- `npx tsc --noEmit` passes for all new/modified files (no new errors introduced)
