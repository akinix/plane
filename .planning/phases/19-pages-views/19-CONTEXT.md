# Phase 19: 页面 & 视图 — Context

**Gathered:** 2026-06-30
**Status:** Ready for planning
**Mode:** Autonomous (user decisions captured)

<domain>
## Phase Boundary

实现 Page（文档页面）和 View（自定义视图）的完整管理功能。用户可在工作区下看到 Page 列表（平铺展示），使用 TipTap 富文本编辑器创建/编辑 Page 内容，归档/删除 Page，设置 Public/Private 权限，收藏/星标 Page。同时，用户可看到已保存的自定义视图列表，将当前 Issue 视图的筛选/排序/分组/列配置保存为新视图，并应用已保存的视图。

**Requirements covered:** PAGE-01, PAGE-02, PAGE-03, PAGE-04, PAGE-05, VIEW-01, VIEW-02, VIEW-03
</domain>

<decisions>
## Locked Decisions

### Page 作用域
- Pages 属于**工作区级别**（`/workspaces/:ws/pages/`），不绑定到特定项目
- 工作区内所有成员可看到工作区下的 Pages
- 这与 Plane 原生一致

### TipTap 编辑器
- 使用**独立编辑页面**模式。点击页面跳转到独立的编辑器页面进行全屏编辑
- 复用已 fork 的 `@plane/editor` 包（Phase 14）

### 页面层级
- **单级（平铺）** — 无父子级嵌套，所有 Page 在同一层级展示
- 通过收藏/星标来组织常用 Page

### 收藏机制
- 使用 **Mock API（后端模式）** — 通过 useModuleMutations 风格的 TanStack Query mutations操作
- 未来可直接对接 .NET 后端 API

### 视图保存配置
- 保存**完整的视图配置**：筛选条件 + 排序规则 + 分组方式 + 显示的列
- 用户在任意 Issue 视图（列表/看板/日历/甘特/电子表格）中可将当前配置保存为新视图

### 技术模式（延续 Phase 15-17 约定）
- TanStack Query 管理服务端数据（Mock API）
- MobX 仅管理 UI 状态（selectedPageId, viewModalOpen, activeViewTab 等）
- 所有文案使用中文（遵循 Copywriting Contract）
- Plane 源码 fork 标记 `// FLOW: Forked from Plane`
</decisions>

<codebase_context>
## Reusable Assets

### Forked Packages
| Package | Source | Status |
|---------|--------|--------|
| `@plane/editor` | `src/lib/editor/` | Forked Phase 14 (Yjs stripped) |
| `@plane/types` | `src/lib/types/` | Forked Phase 14 |
| `@plane/ui` | `src/lib/ui/` | Forked Phase 14 |

### Plane Source Scope
- `apps/web/core/components/pages/` — editor, header, list, loaders, modals, navigation-pane
- `apps/web/core/components/views/` — applied-filters, filters, form, modal, views-list, delete modal

### Existing Patterns
- `useModules` / `useCycleIssues` pattern → reuse for `usePages` / `useFavorites`
- `ModuleStore` / `CycleStore` MobX pattern → extend for `PageStore` / `ViewStore`
- Mock data pattern (`MOCK_PAGES` in `mock-data.ts`)
- Cycle 详情页双栏布局 → applicable to Page 详情页
</codebase_context>

<canonical_refs>
- `yh-flow/clients/web/src/lib/editor/` — Forked @plane/editor with Yjs stripped
- `yh-flow/clients/web/app/components/issues/` — Existing issue view components (filters, sorting, columns)
- `yh-flow/clients/web/src/lib/mock-data.ts` — Mock data extension point
- `.planning/phases/16-issue/16-CONTEXT.md` — Issue view filter/sort/group patterns
- `.planning/REQUIREMENTS.md` — PAGE-01~05, VIEW-01~03
</canonical_refs>

<deferred>
No deferred ideas for this phase.
</deferred>
