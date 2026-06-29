---
phase: 16-issue
plan: 02
subsystem: web-client
tags: [issue, list-view, create-modal, bulk-actions, project-dashboard]
requires: [16-01]
provides: [issue-list-ui, issue-create, bulk-operations, issues-navigation]
affects: [app-issues, project-dashboard]
tech-stack:
  added: []
  patterns:
    - "MobX observer() + useStore() for UI state in view components"
    - "TanStack Query hooks for server data (mock layer)"
    - "Tailwind CSS semantic tokens (bg-custom-*, text-custom-*, border-custom-*)"
    - "lucide-react icons for UI interactions"
key-files:
  created:
    - "yh-flow/clients/web/app/issues/page.tsx"
    - "yh-flow/clients/web/app/components/issues/list-view.tsx"
    - "yh-flow/clients/web/app/components/issues/issue-row.tsx"
    - "yh-flow/clients/web/app/components/issues/quick-filter-bar.tsx"
    - "yh-flow/clients/web/app/components/issues/pagination.tsx"
    - "yh-flow/clients/web/app/components/issues/issue-create-modal.tsx"
    - "yh-flow/clients/web/app/components/issues/bulk-action-bar.tsx"
  modified:
    - "yh-flow/clients/web/app/components/project/project-dashboard.tsx"
metrics:
  duration: "~45 min"
  completed_date: "2026-06-29"
  tasks_total: 2
  tasks_completed: 2
  commits: 2
  files_changed: 8
  type_check_errors: 0
---

# Phase 16 Plan 02: Issue 列表视图、创建和批量操作

实现 Issue 列表页面和 Issue 创建功能。创建 app/issues/page.tsx 作为 Issue 路由入口，实现 IssueListView（含 IssueRow、QuickFilterBar、Pagination）、BulkActionBar（批量操作）、IssueCreateModal（创建 Issue 表单）。更新 ProjectDashboard 的 Issues 标签导航到 Issue 列表页。

## Completed Tasks

| Task | Name | Commit | Key Files |
| ---- | ---- | ------ | --------- |
| 1 | 创建 Issue 列表页面、列表视图容器、Issue 行和快速筛选条 | 3ce90d1ee | page.tsx, list-view.tsx, issue-row.tsx, quick-filter-bar.tsx, pagination.tsx |
| 2 | 创建 IssueCreateModal、BulkActionBar、更新 ProjectDashboard | 776b56961 | issue-create-modal.tsx, bulk-action-bar.tsx, project-dashboard.tsx |

## Task Details

### Task 1: Issue 列表页面、列表视图容器、Issue 行和快速筛选条

**Commit:** 3ce90d1ee

创建了 5 个组件文件：

1. **app/issues/page.tsx** — Issue 路由入口页面
   - 使用 useParams() 获取 workspaceId/projectId
   - 使用 useStore().issue 获取 IssueStore
   - 页面顶部：标题 "Issues" + 列表/看板视图切换按钮（看板按钮目前 disabled）
   - "创建 Issue"按钮打开 IssueCreateModal
   - 根据 activeView 条件渲染 IssueListView

2. **app/components/issues/list-view.tsx** — Issue 列表视图容器
   - 通过 useStore().issue 读取 filters、sortBy 等 UI 状态
   - 使用 useIssues(projectId, filters) 获取数据
   - 客户端排序（useMemo）和分页计算
   - 渲染 QuickFilterBar + 排序按钮组 + 表头 + IssueRow 列表 + Pagination + BulkActionBar
   - 加载状态：Loader 骨架屏（5 行）
   - 空状态：无 Issue 提示 / 筛选无结果提示

3. **app/components/issues/issue-row.tsx** — Issue 单行组件
   - Checkbox（stopPropagation 防止冒泡导航）
   - Issue ID + 标题（整行点击 navigate 到详情页）
   - 优先级徽章 + 负责人头像 + 状态徽章 + 更新时间（formatDistanceToNow）

4. **app/components/issues/quick-filter-bar.tsx** — 快速筛选条
   - 搜索输入框 + 状态/优先级/负责人筛选下拉框
   - 已选条件 chip 显示 + 清除按钮

5. **app/components/issues/pagination.tsx** — 分页控件
   - << < 1 2 3 ... N > >> 格式，最多 7 个页码

### Task 2: IssueCreateModal、BulkActionBar、更新 ProjectDashboard

**Commit:** 776b56961

1. **app/components/issues/issue-create-modal.tsx** — 创建 Issue 模态框
   - 使用 @plane/ui ModalCore 作为模态容器
   - 标题（必填）+ 描述 + 优先级 + 负责人 + 标签字段
   - 提交后调用 createIssue.mutateAsync，成功后 navigate 到详情页

2. **app/components/issues/bulk-action-bar.tsx** — 批量操作浮动栏
   - 固定底部，显示选中计数
   - 状态变更 / 指派 / 删除操作
   - 删除使用 AlertModalCore 确认对话框

3. **app/components/project/project-dashboard.tsx** — 更新
   - Issues tab 使用 navigate 跳转到 Issue 列表路由

## Deviations from Plan

### Auto-fixed Issues

None — plan executed as written.

### Auto-Added Missing Functionality

**1. [Rule 2 - Missing] EmptyState 替换为内联空状态**
- 计划引用 EmptyState 但该组件在 UI 库中不存在
- Fix: 内联 SVG 图标 + 文本
- Files: list-view.tsx
- Commit: 3ce90d1ee

**2. [Rule 2 - Missing] LiteTextEditor 替换为 textarea 暂替**
- LiteTextEditorWithRef 需要 service 层接口（fileHandler, mentionHandler），mock 层不可用
- Fix: textarea 暂替，标注为 stub
- Files: issue-create-modal.tsx
- Commit: 776b56961

**3. [Rule 2 - Simpler] @plane/ui Dropdown 替换为原生 select**
- @plane/ui Dropdown 依赖 Headless UI Combobox + Popper.js，mock 层用原生 select 更简洁
- Files: issue-create-modal.tsx, bulk-action-bar.tsx
- Commit: 776b56961

**4. [Rule 2 - Missing] BulkActionBar 集成到 list-view.tsx**
- Task 1 创建的 list-view.tsx 未包含 BulkActionBar
- Files: list-view.tsx
- Commit: 776b56961

## Known Stubs

- **issue-create-modal.tsx 描述字段**: textarea 代替 @plane/editor LiteTextEditorWithRef（API 集成阶段替换）
- **quick-filter-bar.tsx 筛选**: 原生 select 代替 @plane/ui Dropdown，多选简化为单选（Phase 17 完善）
- **issue-row.tsx Issue ID**: 使用 sequence_id 数字，未拼接 project identifier 前缀

## Threat Flags

No threat flags — all components operate on mock data only.

## Self-Check: PASSED
