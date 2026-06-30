# Phase 15: 工作区 & 项目 - Context

**Gathered:** 2026-06-29
**Status:** Ready for planning

<domain>
## Phase Boundary

用户登录后可管理工作区和项目，侧边栏/顶栏提供全局导航。核心交付包括：

- 工作区仪表板（项目概览、最近活动）
- 工作区设置（名称、描述、Logo）和成员管理（列表、角色变更）
- 项目创建、列表（含搜索/排序）、详情/仪表板框架
- 项目成员管理，项目选择器导航
- 侧边栏树形导航 + 顶部导航栏
- Emoji 图标选择器

**Requirements covered:** WORK-01~04, PROJ-01~05, UI-01, UI-02, UI-05
</domain>

<decisions>
## Implementation Decisions

### 页面布局与路由结构（Grey Area 1）

- **D-P15-01:** 沿用 Plane 布局——左侧侧边栏 + 右侧主内容区，仪表板包含项目概览卡片和最近活动列表
- **D-P15-02:** 路由格式为 `/workspaces/:workspaceId/` + `/workspaces/:workspaceId/projects/:projectId/`，保持与 Plane 语义一致
- **D-P15-03:** 工作区切换下拉菜单位于侧边栏顶部，显示当前工作区名称 + 展开箭头
- **D-P15-04:** 若用户无工作区 → 自动跳转创建工作区；若有工作区 → 跳到第一个工作区仪表板

### 侧边栏 & 顶栏（Grey Area 2）

- **D-P15-05:** 侧边栏树形结构：工作区（可展开→项目列表）→ 项目（可展开→项目内视图如 Issue/Cycle/Module），当前项高亮
- **D-P15-06:** 侧边栏支持折叠/展开，折叠时仅显示图标，保持 Plane 风格
- **D-P15-07:** 顶栏左侧显示当前页面标题，右侧显示搜索图标 + 通知铃铛 + 用户头像下拉菜单（设置/退出）
- **D-P15-08:** TanStack Query 预取工作区和项目列表数据，MobX 仅管理 UI 展开/收起等状态

### 功能实现策略（Grey Area 3）

- **D-P15-09:** 工作区仪表板上方显示项目概览卡片网格，下方显示最近活动列表
- **D-P15-10:** Emoji 选择器复用 Plane 现有组件（从 @plane/ui fork 的副本中提取），支持搜索 Emoji
- **D-P15-11:** Phase 15 仅实现项目详情页基本框架和项目设置页面，项目内视图在后续 Phase 实现
- **D-P15-12:** 先用 mock 数据实现 UI 和交互，逐步替换为真实 API 调用

### Claude's Discretion

- 工作区成员角色的具体 UI 呈现方式（下拉选择 vs 弹窗表单）
- 项目搜索/排序的具体实现（前端过滤 vs 后端查询）
- 侧边栏动画过渡细节
- 空状态设计（无工作区、无项目、无成员等场景）

</decisions>

<code_context>

## Existing Code Insights

### Reusable Assets

- **AuthStore/FlowApiService**: Phase 14 已实现的认证和 API 基类（`src/lib/services/`），可直接用于工作区/项目 API 调用
- **@plane/types**: 已包含 `IWorkspace`、`IProject` 等类型定义（`src/lib/types/`）
- **@plane/ui**: Fork 的 UI 组件库包含侧边栏、下拉菜单、头像等基础组件
- **@plane/utils**: 包含 `cn()`、颜色处理等工具函数
- **Plane 参考** (`apps/web/`):
  - `apps/web/core/components/workspace/` — 工作区组件（侧边栏、下拉、设置）
  - `apps/web/core/components/project/` — 项目组件（列表、卡片、选择器）
  - `apps/web/core/components/sidebar/` — 侧边栏实现
  - `apps/web/core/components/navigation/` — 导航栏实现

### Established Patterns

- **MobX Store 模式**: makeObservable + observable/computed/action + runInAction() 包装异步。
  Phase 14 的 AuthStore 模式需扩展为全局 Store 链（WorkspaceStore, ProjectStore）
- **API 服务层**: FlowApiService 子类 × 领域（参考 `auth.service.ts`），每个领域一个 Service
- **路由模式**: React Router v7，`app/routes.ts` 集中定义路由配置
- **import 分组**: // ui → // types → // services → // helpers → // hooks → // components

### Integration Points

- **app/provider.tsx**: 需添加 WorkspaceProvider/ProjectProvider 到全局 Provider 链
- **app/routes.ts**: 需添加工作区和项目的路由配置
- **AuthStore**: 认证状态决定是否显示工作区内容（已认证 → 加载工作区）
- **ThemeProvider**: 延续 Phase 14 的主题方案

</code_context>

<specifics>
## Specific Ideas

- 侧边栏数据使用 TanStack Query 的 prefetchQuery 在路由加载前预取
- 工作区创建页面在首次登录时自动引导
- 项目选择器与侧边栏树形结构联动，侧边栏展开/折叠状态持久化（localStorage）
- Emoji 选择器作为独立组件复用，不绑定具体业务逻辑

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

_Phase: 15-工作区 & 项目_
_Context gathered: 2026-06-29_
