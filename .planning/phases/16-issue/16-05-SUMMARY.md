---
phase: 16-issue
plan: 05
subsystem: web-client
tags: [command-palette, cmdk, global-navigation, keyboard-shortcut]
requires: [16-01]
provides: [command-palette-component]
affects: [app-components, workspace-layout]
tech-stack:
  added: []
  patterns:
    - "cmdk Command.Dialog for command palette UI"
    - "Global keydown listener for Cmd+K / Ctrl+K shortcut"
    - "useMemo for client-side search filtering"
    - "Semantic Tailwind tokens (bg-custom-background-*, text-custom-text-*)"
key-files:
  created:
    - "yh-flow/clients/web/app/components/command-palette.tsx"
  modified:
    - "yh-flow/clients/web/app/layouts/workspace-layout.tsx"
metrics:
  duration: "~30 min"
  completed_date: "2026-06-29"
  tasks_total: 2
  tasks_completed: 2
  commits: 2
  files_changed: 2
  type_check_errors: 0 (in our files; 75 pre-existing in editor subpackage and React type mismatch)
---

# Phase 16 Plan 05: 全局命令面板（Cmd+K）

实现全局命令面板（Command Palette），用户可通过 `Cmd+K`（Mac）或 `Ctrl+K`（Windows）快捷键触发，搜索 Issue 标题和项目名称，并导航到对应页面。使用 `cmdk` 库的 `Command.Dialog` 实现。

## Completed Tasks

| Task | Name                                     | Commit    | Key Files            |
| ---- | ---------------------------------------- | --------- | -------------------- |
| 1    | 创建 CommandPalette 组件                 | 23bff51e2 | command-palette.tsx  |
| 2    | 在 WorkspaceLayout 中集成 CommandPalette | 761b45243 | workspace-layout.tsx |

## Task Details

### Task 1: 创建 CommandPalette 组件

- 创建 `app/components/command-palette.tsx`，使用 cmdk 的 `Command.Dialog` 实现
- 键盘快捷键：监听全局 `keydown` 事件，`Cmd+K` / `Ctrl+K` 时 `setOpen(true)`
- 搜索数据源：
  - **Issue 标题搜索**：从 `MOCK_ISSUES` 中按 `name` 包含匹配（大小写不敏感），最多 10 条
  - **项目名称搜索**：从 `MOCK_PROJECTS` 中按 `name` 包含匹配（大小写不敏感），最多 5 条
- 搜索过滤使用 `useMemo` 优化性能
- 导航：选中结果后调用 `navigate()` 并关闭面板
  - Issue 导航：`/workspaces/{wsId}/projects/{projectId}/issues/{issueId}`
  - 项目导航：`/workspaces/{wsId}/projects/{projectId}`
- 显示格式：
  - Issue 行：ListTodo 图标 + 名称 + 项目标识符-序号（如 `FF-1`）
  - 项目行：LayoutGrid 图标 + 名称
- 空状态：搜索无匹配时显示"没有匹配结果"
- 样式：
  - 遮罩层：rgba(0,0,0,0.5) + backdrop-filter: blur(4px)
  - 对话框：bg-custom-background-100 + shadow-2xl + rounded-lg
  - 输入框：左侧 Search 图标 + outline-none + placeholder 样式
  - Item：hover:bg-custom-background-80 + data-[selected=true]:bg-custom-background-80
- 键盘导航由 cmdk 内置支持：ArrowUp/ArrowDown 移动选择，Enter 选中，Escape 关闭

### Task 2: 在 WorkspaceLayout 中集成 CommandPalette

- 在 `workspace-layout.tsx` 中导入 `CommandPalette` 组件
- 在 `AuthenticationWrapper` 内部、main flex 容器之前渲染：
  ```tsx
  <CommandPalette workspaceId={workspaceStore.currentWorkspaceId} />
  ```
- CommandPalette 作为全局浮动层，在需要时显示覆盖遮罩，与 sidebar/main 内容无关

## Cherry-Picked Dependencies

由于该工作树的基准提交（065af9cce）不包含 Plan 16-01 的更改，本计划 cherry-pick 了 16-01 的三个提交作为依赖：

| Cherry-Pick Commit | Original Plan | Content                                       |
| ------------------ | ------------- | --------------------------------------------- |
| 011f62d8b          | 16-01 Task 1  | 安装 cmdk + @hello-pangea/dnd，扩展 mock-data |
| 769e8d1f6          | 16-01 Task 2  | 创建 TanStack Query hooks                     |
| 619330894          | 16-01 Task 3  | 创建 IssueStore + 路由注册                    |

## Deviations from Plan

### Auto-Fixed Issues

**1. [Rule 3 - Blocking] 工作树缺少 Plan 16-01 的依赖**

- Found during: Task 1 创建后 TypeScript 检查
- Issue: 工作树的 `yh-flow/clients/web/package.json` 不包含 `cmdk` 依赖，`mock-data.ts` 缺少 `MOCK_ISSUES`/`MOCK_PROJECTS` 等数据。这是平行执行工作树的设计特性——各工作树从同一基准提交开始。
- Fix: 从 `preview` 分支 cherry-pick 了 16-01 的三个实现提交
- Files modified: mock-data.ts, package.json, use-issues.ts, use-comments.ts, hooks/index.ts, issue.store.ts, types.ts, root.store.ts, routes.ts, pnpm-lock.yaml
- Commits: 011f62d8b, 769e8d1f6, 619330894

**2. [Rule 3 - Blocking] 导入路径 `@/lib/mock-data` 无法解析**

- Found during: Task 1 TypeScript 检查
- Issue: 该项目的 tsconfig 将 `@/*` 映射到 `./app/*`，而非 `./src/*`。`@/lib/mock-data` 会解析到 `./app/lib/mock-data` 但实际文件在 `./src/lib/mock-data`。项目中的 hooks 文件（位于 `src/lib/hooks/`）使用相对路径 `../mock-data`。
- Fix: 将导入路径从 `@/lib/mock-data` 改为相对路径 `../../src/lib/mock-data`
- Files modified: command-palette.tsx
- Commit: 23bff51e2

**3. [Rule 1 - Bug] `issue.project_id` 类型为 `string | null`，与 `getProjectIdentifier` 参数类型不匹配**

- Found during: Task 1 TypeScript 检查
- Issue: `TBaseIssue` 中 `project_id` 字段类型为 `string | null`，而 `getProjectIdentifier` 函数参数声明为 `string`
- Fix: 将 `getProjectIdentifier` 参数类型改为 `string | null`，增加空值检查
- Files modified: command-palette.tsx
- Commit: 23bff51e2

## Verification

| Check               | Command                                         | Result                      |
| ------------------- | ----------------------------------------------- | --------------------------- |
| 类型检查            | `npx tsc --noEmit`                              | 0 新增错误（75 个预存错误） |
| cmdk 使用           | `grep -c "cmdk" command-palette.tsx`            | 4 处引用                    |
| CommandPalette 渲染 | `grep -c "CommandPalette" workspace-layout.tsx` | 2 处引用                    |
| Cmd+K 快捷键        | `grep -c "Cmd" command-palette.tsx`             | 2 处引用                    |

## Success Criteria

- [x] Cmd+K / Ctrl+K 触发命令面板
- [x] 搜索可匹配 Issue 标题和项目名称
- [x] 键盘导航（↑↓ + Enter + Esc）由 cmdk 内置支持
- [x] 选中结果后导航到对应页面并关闭面板
- [x] 无结果时显示"没有匹配结果"
- [x] `npx tsc --noEmit` 零新增错误

## Self-Check: PASSED
