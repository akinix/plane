# Phase 16: Issue 列表/详情 & 看板 - Context

**Gathered:** 2026-06-29
**Status:** Ready for planning
**Mode:** Smart Discuss (autonomous — batch proposals accepted)

<domain>
## Phase Boundary

实现完整的 Issue 管理功能，包括 Issue CRUD（创建/列表/详情/编辑/软删除）、Issue 评论系统（创建/编辑/删除）、Issue 属性编辑（状态/优先级/负责人/标签/估算/截止日期）、列表视图（筛选/排序/分页）、看板视图（拖拽/分组）、命令面板（Cmd+K 搜索导航），以及批量操作（状态变更/指派/删除）。

**Requirements covered:** ISSU-01~08, KANB-01~05, UI-04

</domain>

<decisions>
## Implementation Decisions

### Grey Area 1: 页面架构与状态管理

- **D-P16-01:** Issue 视图目录结构为 `app/issues/`（路由/页面）+ `app/components/issues/`（组件），贴近 Plane 的 `app/issues/` 布局
- **D-P16-02:** 使用 TanStack Query hooks（useIssues, useIssue, useIssueMutations etc.）管理服务端 Issue 数据；MobX IssueStore 仅管理 UI 状态（展开/折叠、选中项、看板滚动位置等）
- **D-P16-03:** 在现有 `mock-data.ts` 中扩展 issue/state/label/priority mock 数据，不新建独立文件
- **D-P16-04:** 路由格式为 `/workspaces/:workspaceId/projects/:projectId/issues/`（列表）+ `/workspaces/:workspaceId/projects/:projectId/issues/:issueId`（详情），不使用 React Router 嵌套路由

### Grey Area 2: Issue 列表视图

- **D-P16-05:** 列表行采用中等密度显示（标题、Issue ID、优先级标签、负责人头像、状态徽章、估算值）
- **D-P16-06:** 筛选/排序 UI 采用顶栏快速筛选条（快速筛选：状态/优先级/负责人）+ 展开面板提供更多条件。排序通过顶栏下拉选择
- **D-P16-07:** 采用分页方式（每页 20 条），带页面切换控件，不使用无限滚动
- **D-P16-08:** 初始实现固定列（ID + 标题 + 状态 + 优先级 + 负责人 + 更新时间），自定义列能力推迟到 Phase 17（FILT-03）

### Grey Area 3: Issue 详情页

- **D-P16-09:** 详情页采用双栏布局——左侧 Issue 描述区域，右侧属性面板，保持 Plane 风格
- **D-P16-10:** 属性字段采用内联编辑方式：点击字段直接展开下拉/弹窗编辑器（与 Plane 行为一致），不采用独立的编辑弹窗模式
- **D-P16-11:** 评论组件位于 Issue 描述下方，顺序为：评论列表 → 评论输入框
- **D-P16-12:** Issue 描述编辑器复用已 Fork 的 @plane/editor（已剥离 Yjs），不降级为 textarea/Markdown

### Grey Area 4: 看板视图 & 命令面板

- **D-P16-13:** 拖拽使用 @hello-pangea/dnd（react-beautiful-dnd 维护分支），与 Plane 使用的方案一致
- **D-P16-14:** 默认按状态列分组，支持通过 Group By 切换按负责人/优先级分组
- **D-P16-15:** 子分组（Swimlane）功能推迟到 Phase 17 实现，本 Phase 仅实现单层列分组
- **D-P16-16:** 命令面板作为独立 CommandPalette 组件实现，Cmd+K 全局触发，支持搜索项目/Issue/页面导航

### Claude's Discretion

- 看板视图的列折叠/展开 UI 具体实现方式
- 列表视图中行点击 vs checkbox 选中交互细节
- 空状态、加载状态、错误状态的具体呈现
- 评论编辑的 UI 模式（inline 编辑 vs 弹窗）
- 侧边栏树形结构中 Issue 视图路由占位更新的具体位置
- @hello-pangea/dnd 的具体动画和过渡配置
- 命令面板的搜索排序和键盘导航细节

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **类型定义**: `src/lib/types/issues.ts`, `src/lib/types/issues/issue.ts`, `src/lib/types/issues/base.ts` — TIssue, IIssueLabel, IIssueActivity 等完整类型
- **状态类型**: `src/lib/types/state.ts` — TStateGroups, IState 等 Plane 标准状态模型
- **Issue 常量**: `src/lib/constants/issue/common.ts`, `src/lib/constants/issue/layout.ts`, `src/lib/constants/issue/filter.ts` — 布局选项、筛选标签名、Issue 属性 key 等
- **UI 组件库**: `src/lib/ui/` — Avatar, Badge, Button, Dropdown, Modal, Tooltip, Loader, DragHandle, DropIndicator, Sortable 等
- **服务层**: `src/lib/services/flow-api.service.ts` — FlowApiService 基类（JWT Bearer 拦截器、snake_case 转换）
- **认证**: `src/lib/services/auth.service.ts` — AuthService 参考
- **Hooks**: `src/lib/hooks/use-members.ts`, `use-projects.ts`, `use-workspaces.ts` — TanStack Query hooks 模式参考
- **TanStack Query 配置**: `app/provider.tsx` — QueryClientProvider（staleTime: 5min, retry: 1）
- **MobX Store 模式**: `app/store/workspace.store.ts`, `app/store/project.store.ts` — 纯 UI 状态管理的 Store 参考
- **Root Store**: `app/store/root.store.ts` — CoreRootStore 注册新 Store 的模式
- **Mock 数据**: `src/lib/mock-data.ts` — 现有工作区/项目 mock 数据，需扩展 Issue/State/Label 数据
- **Plane 参考** (`apps/web/`):
  - `apps/web/core/components/issues/` — Issue 列表/详情组件参考
  - `apps/web/core/components/kanban/` — 看板组件参考
  - `apps/web/core/components/command-palette/` — 命令面板组件参考
  - `apps/web/core/components/issue/` — Issue 属性编辑组件参考
  - `apps/web/core/components/rich-text-editor/` — TipTap 编辑器集成参考

### Established Patterns

- **TanStack Query + MobX 分工**: 服务端数据用 TanStack Query（useWorkspaces, useProjects），纯 UI 状态用 MobX（sidebarCollapsed, expandedProjectIds）。Phase 16 沿用此模式。
- **MobX Store 模式**: `makeObservable` + `observable`/`computed`/`action` 声明 + `runInAction()` 包装异步操作
- **服务层模式**: FlowApiService 子类 × 领域，通过 barrel index.ts 统一导出
- **路由模式**: React Router v7，`app/routes.ts` 集中定义路由配置
- **import 分组**: `// ui` → `// types` → `// services` → `// helpers` → `// hooks` → `// components`

### Integration Points

- **routes.ts**: 需添加 Issue 列表和详情路由到 workspace-layout 内（Project 路由下）
- **SidebarTree**: `app/components/sidebar/sidebar-tree.tsx` — Issue 视图入口已预留节点占位，需链接到实际路由
- **TopBar**: 命令面板的全局键盘监听需在 Provider 层注册
- **Root Store**: 如果 IssueStore 需要 UI 状态，需在 root.store.ts 注册
- **Mock 数据**: `src/lib/mock-data.ts` — 扩展 Issue/State/Label/Activity mock 数据
- **Provider 链**: 如有全局命令面板，需在 provider.tsx 或 root.tsx 中注册

</code_context>

<specifics>
## Specific Ideas

- 列表视图和看板视图共享相同的筛选状态，切换视图时筛选条件不变
- 命令面板支持键盘导航（↑↓箭头 + Enter 选择 + Esc 关闭）
- Issue ID 格式保持 Plane 的 `{PROJECT_KEY}-{N}`（如 `PROJ-42`）
- Issue 创建通过半屏滑出面板或弹窗实现（参考 Plane 的 CreateIssueModal）
- 侧边栏树形中 Issue 视图入口列出 Issue 计数

</specifics>

<deferred>
## Deferred Ideas

- **Sub-group/Swimlane**: 推迟到 Phase 17 影格范围
- **自定义列表列配置（IIssueDisplayProperties）**: 推迟到 Phase 17 的 FILT-03
- **无限滚动/虚拟列表**: 不在 Phase 16 计划内
- **Issue 关联/依赖管理**: 不在 Phase 16 范围
- **Issue Link（关联链接）**: 不在 Phase 16 范围
  </deferred>

---

_Phase: 16-Issue 列表/详情 & 看板_
_Context gathered: 2026-06-29_
_Mode: Smart Discuss_
