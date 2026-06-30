---
phase: 19-pages-views
plan: 02
type: execute
created: "2026-06-30T10:30:00Z"
duration_minutes: 35
tasks:
  total: 3
  completed: 3
files:
  created:
    - yh-flow/clients/web/app/workspaces/[workspaceId]/pages/page.tsx
    - yh-flow/clients/web/app/components/pages/pages-list-main-content.tsx
    - yh-flow/clients/web/app/components/pages/pages-list-view.tsx
    - yh-flow/clients/web/app/components/pages/list/tab-navigation.tsx
    - yh-flow/clients/web/app/components/pages/list/search-input.tsx
    - yh-flow/clients/web/app/components/pages/list/order-by.tsx
    - yh-flow/clients/web/app/components/pages/list/root.tsx
    - yh-flow/clients/web/app/components/pages/list/block.tsx
    - yh-flow/clients/web/app/components/pages/list/block-item-action.tsx
    - yh-flow/clients/web/app/components/pages/list/filters/root.tsx
    - yh-flow/clients/web/app/components/pages/list/applied-filters/root.tsx
    - yh-flow/clients/web/app/components/pages/header/favorite-control.tsx
    - yh-flow/clients/web/app/components/pages/header/archived-badge.tsx
    - yh-flow/clients/web/app/components/pages/modals/create-page-modal.tsx
    - yh-flow/clients/web/app/components/pages/modals/page-form.tsx
    - yh-flow/clients/web/app/components/pages/modals/delete-page-modal.tsx
    - yh-flow/clients/web/app/components/pages/dropdowns/actions.tsx
    - yh-flow/clients/web/app/components/pages/loaders/page-content-loader.tsx
  modified:
    - yh-flow/clients/web/app/routes.ts
    - yh-flow/clients/web/app/components/sidebar/sidebar-tree.tsx
  deleted: []
commits:
  - hash: be4331767
    message: "feat(19-02): add pages list route, entry page, and sidebar navigation"
    files:
      - routes.ts
      - pages/page.tsx
      - sidebar-tree.tsx
  - hash: 3c27ba9f8
    message: "feat(19-02): add pages list container, tab navigation, search, and order components"
    files:
      - pages-list-main-content.tsx
      - pages-list-view.tsx
      - tab-navigation.tsx
      - search-input.tsx
      - order-by.tsx
      - filters/root.tsx
      - applied-filters/root.tsx
      - page-content-loader.tsx
  - hash: 2601c636b
    message: "feat(19-02): add page cards, modals, and action components"
    files:
      - list/root.tsx
      - list/block.tsx
      - list/block-item-action.tsx
      - header/favorite-control.tsx
      - header/archived-badge.tsx
      - modals/create-page-modal.tsx
      - modals/page-form.tsx
      - modals/delete-page-modal.tsx
      - dropdowns/actions.tsx
subsystem: flow-web-frontend
requirements:
  - PAGE-01
  - PAGE-04
  - PAGE-05
key_decisions:
  - "Pages 列表组件全部 Fork 自 Plane，适配 Flow Web 的 MobX store 模式（useStore().page）"
  - "搜索/排序为客户端过滤（PageStore.filters 控制），mock 阶段无需后端支持"
  - "所有文案为中文（per UI-SPEC Copywriting Contract）"
  - "create-page-modal 和 delete-page-modal 使用 props isOpen/onClose 控制弹窗，而非 PageStore 直接控制"
  - "block-item-action 重构为只显示访问权限徽标和更多菜单（收藏功能由 block.tsx 的星标按钮处理）"
---

# Phase 19 Plan 02: Pages 列表页面 UI

## One-liner

创建 Pages 完整列表页面 UI：路由注册、侧边栏导航、Tab 切换（公开/私人/已归档）、搜索/排序、页面卡片网格、创建/删除弹窗、收藏功能和操作下拉菜单。

## Context

本计划为 Phase 19（页面与视图）的第二个子计划，负责 Pages 文档页面的列表 UI 层。基于 Plan 19-01 创建的 PageStore（MobX UI 状态）和 usePages/usePageMutations（TanStack Query hooks）构建。

所有组件均 Fork 自 Plane 的 pages 列表组件，适配到 Flow Web 的 store 模式（`useStore().page`）和中文文案。

## Changes

### Created (17 files)

| File                                                   | Description                                                   |
| ------------------------------------------------------ | ------------------------------------------------------------- |
| `app/workspaces/[workspaceId]/pages/page.tsx`          | Pages 列表页入口路由组件，使用 useParams 获取 workspaceId     |
| `app/components/pages/pages-list-main-content.tsx`     | 列表主容器：组合 Tab + Search + Order + 客户端过滤/排序逻辑   |
| `app/components/pages/pages-list-view.tsx`             | 网格容器，代理到 list/root.tsx                                |
| `app/components/pages/list/tab-navigation.tsx`         | Tab 导航（公开/私人/已归档），使用 PageStore.setActiveTab     |
| `app/components/pages/list/search-input.tsx`           | 搜索输入框（受控 + 展开动画），placeholder: "搜索页面..."     |
| `app/components/pages/list/order-by.tsx`               | 排序下拉（名称/创建时间/更新时间 + 升序/降序）                |
| `app/components/pages/list/root.tsx`                   | 列表核心渲染：排序后的卡片网格 + 空状态（按 Tab 不同文案）    |
| `app/components/pages/list/block.tsx`                  | 页面卡片：emoji/icon 图标、名称、访问权限徽章、收藏星标、日期 |
| `app/components/pages/list/block-item-action.tsx`      | 悬停操作：访问权限图标、创建日期 info、更多下拉菜单           |
| `app/components/pages/list/filters/root.tsx`           | 筛选器容器（预留扩展）                                        |
| `app/components/pages/list/applied-filters/root.tsx`   | 已应用筛选器（预留扩展）                                      |
| `app/components/pages/header/favorite-control.tsx`     | 收藏星标控制（filled accent / empty grey，28px touch target） |
| `app/components/pages/header/archived-badge.tsx`       | 已归档徽章（archive icon + 日期）                             |
| `app/components/pages/modals/create-page-modal.tsx`    | 创建页面弹窗（ModalCore + PageForm）                          |
| `app/components/pages/modals/page-form.tsx`            | 页面创建表单（名称输入 + 访问权限切换 + 中文文案）            |
| `app/components/pages/modals/delete-page-modal.tsx`    | 删除确认弹窗（确认文案 + 取消/删除按钮）                      |
| `app/components/pages/dropdowns/actions.tsx`           | 页面操作下拉菜单（复制链接、归档/恢复、删除）                 |
| `app/components/pages/loaders/page-content-loader.tsx` | 加载骨架（Tab bar + 卡片网格占位）                            |

### Modified (2 files)

| File                                      | Change                                                                                        |
| ----------------------------------------- | --------------------------------------------------------------------------------------------- |
| `app/routes.ts`                           | 添加 PAGE ROUTES section：`workspaces/:workspaceId/pages` → pages/page.tsx（workspace-level） |
| `app/components/sidebar/sidebar-tree.tsx` | 在项目树上方添加 "页面" workspace 级导航链接，点击跳转到 /workspaces/:wsId/pages              |

## Key Decisions

1. **客户端过滤/排序**: 搜索和排序逻辑在 pages-list-main-content.tsx 中客户端实现（filter/sort MOCK_PAGES），不依赖后端 API
2. **弹窗状态控制**: create-page-modal 和 delete-page-modal 使用 props isOpen/onClose 模式，由父组件基于 PageStore（如 pageModalOpen）控制
3. **block-item-action 精简**: 移除 handleFavorite（已由 block.tsx 的星标按钮控制），只显示权限图标、日期 info 和更多菜单

## Deviations from Plan

None — 计划按预期完整执行。

## Verification

- [x] `routes.ts` 包含 `workspaces/:workspaceId/pages` 路由
- [x] `workspaces/[workspaceId]/pages/page.tsx` 已创建并导出默认组件
- [x] `sidebar-tree.tsx` 包含"页面" workspace 级导航链接
- [x] 所有 17 个组件文件已创建
- [x] Tab 组件使用 PageStore.setActiveTab 控制状态
- [x] 搜索组件使用 PageStore.setSearchQuery
- [x] 排序组件使用 PageStore.setSortKey/setSortBy
- [x] block.tsx 使用 usePageMutations().favoritePage 处理收藏
- [x] block.tsx 使用 useNavigate 导航到编辑器
- [x] 所有文案为中文 per UI-SPEC

## Threat Surface

None — 所有变更为纯前端 UI 组件，无新增网络端点或 schema 变更。

## Known Stubs

- `filters/root.tsx` 和 `applied-filters/root.tsx` 为预留扩展，尚未实现完整筛选功能（属于未来计划范围）
