---
phase: 19-pages-views
plan: 01
type: execute
created: "2026-06-30T02:00:00Z"
duration_minutes: 15
tasks:
  total: 2
  completed: 2
files:
  created:
    - yh-flow/clients/web/app/store/page.store.ts
    - yh-flow/clients/web/src/lib/hooks/use-pages.ts
    - yh-flow/clients/web/src/lib/hooks/use-page-mutations.ts
  modified:
    - yh-flow/clients/web/app/store/root.store.ts
    - yh-flow/clients/web/app/store/types.ts
    - yh-flow/clients/web/src/lib/mock-data.ts
    - yh-flow/clients/web/src/lib/hooks/index.ts
  deleted: []
commits:
  - hash: 0e3ebafc6
    message: "feat(19-01): create PageStore MobX UI state"
    files:
      - page.store.ts
      - root.store.ts
      - types.ts
  - hash: efcf344df
    message: "feat(19-01): create MOCK_PAGES and TanStack Query hooks"
    files:
      - mock-data.ts
      - use-pages.ts
      - use-page-mutations.ts
      - index.ts
subsystem: flow-web-frontend
requirements:
  - PAGE-01
  - PAGE-02
  - PAGE-03
  - PAGE-04
  - PAGE-05
key_decisions:
  - "PageStore follows IModuleStore pattern with flat list state (no parent/child nesting)"
  - "PageStore uses object spread + Object.assign for filter updates, matching ModuleStore convention"
  - "Mock data follows Plane page structure with access levels, favorites, and archived states"
  - "usePageMutations returns createPage/updatePage/deletePage/archivePage/restorePage/favoritePage"
  - "archivePage is soft-delete (sets archived_at), restorePage clears archived_at"
  - "favoritePage accepts {pageId, is_favorite} with optimistic update pattern"
---

# Phase 19 Plan 01: Pages Data Layer (Mock + Hooks + Store)

## One-liner

创建 Pages 完整数据层：MOCK_PAGES（7 条样本）、TanStack Query hooks（usePages/usePageDetail/useArchivedPages + usePageMutations CRUD + archive/restore/favorite）、PageStore MobX UI 状态管理。

## Context

本计划为 Phase 19（页面与视图）的第一个子计划，负责 Pages 文档页面的数据基础设施层。后端使用 TanStack Query 管理的 Mock API 层，UI 状态由 MobX 管理。模式沿袭 Phase 18 的 useModules/ModuleStore 模式。

## Changes

### Created

| File                                  | Description                                                                                                                                                                                                                  |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app/store/page.store.ts`             | PageStore — IPageStore 接口 + PageStore 类实现。UI 状态：activeTab, selectedPageId, pageModalOpen/mode, deleteModal(Id/Name), archiveModal(Id/Name), filters(searchQuery/sortKey/sortBy), appliedViewId。14 个 action 方法。 |
| `src/lib/hooks/use-pages.ts`          | TanStack Query hooks：usePages(workspaceId), usePageDetail(workspaceId, pageId), useArchivedPages(workspaceId)。均使用 MOCK_PAGES 过滤 + delay。                                                                             |
| `src/lib/hooks/use-page-mutations.ts` | TanStack Query mutations：createPage, updatePage, deletePage, archivePage, restorePage, favoritePage。所有 mutations 完成时 invalidateQueries(["pages"])。Mock 模式操作 MOCK_PAGES 数组。                                    |

### Modified

| File                      | Change                                                                                                                      |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `app/store/root.store.ts` | 导入 PageStore/IPageStore，在 ICoreRootStore 和 CoreRootStore 中添加 `page: IPageStore`，构造函数中初始化 `new PageStore()` |
| `app/store/types.ts`      | 导入并 re-export `IPageStore` 类型                                                                                          |
| `src/lib/mock-data.ts`    | 追加 `TPage` 和 `EPageAccess` 导入；追加 `MOCK_PAGES` 数组（7 条样本页面，覆盖 public/private/archived/favorite 状态）      |
| `src/lib/hooks/index.ts`  | 追加 `export * from "./use-pages"` 和 `export * from "./use-page-mutations"`                                                |

## Key Decisions

1. **PageStore 模式**: 沿袭 ModuleStore 的 MobX UI 状态模式（observable.ref + action 装饰器）
2. **归档/收藏**: archivePage 设置 `archived_at`（软删除），restorePage 清空 `archived_at`，favoritePage 切换 `is_favorite`
3. **store-context.tsx 无需修改**: CoreRootStore 通过 StoreProvider 自动暴露所有 store 属性
4. **mock 数据类型对齐**: 修复了 `deleted_at: null` 导致的 TS 类型错误（TPage 要求 `Date | undefined`）

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2] 修复 TPage 类型兼容性**

- **发现于:** Task 2
- **问题:** MOCK_PAGES 中 `deleted_at: null` 不兼容 `TPage.deleted_at: Date | undefined`；`description_json` 属性缺失
- **修复:** `null` → `undefined`, 添加 `description_json: {}` 到每条样本数据
- **文件:** `yh-flow/clients/web/src/lib/mock-data.ts`

## Success Criteria

| Criteria                                                                 | Status              |
| ------------------------------------------------------------------------ | ------------------- |
| MOCK_PAGES 数据数组在 mock-data.ts 中定义，至少 5 条                     | ✅ 7 条样本数据     |
| usePages(workspaceId) 返回非归档页面列表                                 | ✅                  |
| useArchivedPages(workspaceId) 返回已归档页面列表                         | ✅                  |
| usePageDetail(workspaceId, pageId) 返回单个页面详情                      | ✅                  |
| usePageMutations().createPage mutate 后 MOCK_PAGES 长度 +1               | ✅                  |
| usePageMutations().archivePage 设置 archived_at 而非删除                 | ✅                  |
| usePageMutations().restorePage 清空 archived_at                          | ✅                  |
| usePageMutations().favoritePage 切换 is_favorite                         | ✅                  |
| PageStore 提供 activeTab/selectedPageId/pageModalOpen/filters 等 UI 状态 | ✅ 14 个 state 字段 |
| PageStore 通过 useStore().page 可访问                                    | ✅                  |

## Verification

- [x] TypeScript 编译通过（无新增错误，仅保留前阶段已有的 5 个预存错误）
- [x] export const MOCK_PAGES 存在于 mock-data.ts
- [x] use-pages.ts 导出 usePages / usePageDetail / useArchivedPages
- [x] use-page-mutations.ts 导出 usePageMutations，包含 6 个 mutation 函数
- [x] hooks/index.ts 包含两条新导出

## Threat Surface

None — 所有变更均为纯前端 mock 数据层和 UI store，无网络端点或 schema 变更。

## Known Stubs

None — 所有 mock 数据完整，Store 接口完整实现。
