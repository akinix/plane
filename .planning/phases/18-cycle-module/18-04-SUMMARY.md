---
phase: 18-cycle-module
plan: 04
subsystem: ui
tags: [react, react-router, mobx, tanstack-query, plane-ui-fork]

requires:
  - plan: 18-02
    provides: Cycle 组件（CyclesView, ActiveCycleProgress, BurndownChart, CycleDeleteModal）
  - plan: 18-03
    provides: Module 组件（ModulesListView, ModuleStatusDropdown, ModuleLinksList, ModuleAnalyticsSidebar）

provides:
  - 4 个页面路由（cycles 列表 + 详情, modules 列表 + 详情）
  - Cycle 列表页（/cycles/）和详情页（/cycles/:cycleId）
  - Module 列表页（/modules/）和详情页（/modules/:moduleId）
  - Issue 详情侧栏 Cycle/Module 导航链接
  - 侧边栏中文标签（周期/模块）

affects: [18-05, 19]

tech-stack:
  added: []
  patterns:
    - 页面组件通过 useParams 获取路由参数，通过 useStore 管理 UI 状态
    - 详情页使用双栏布局方案
    - Issue 侧栏通过 useCycles/useModules 查询关联数据

key-files:
  created:
    - yh-flow/clients/web/app/cycles/page.tsx
    - yh-flow/clients/web/app/cycles/[cycleId]/page.tsx
    - yh-flow/clients/web/app/modules/page.tsx
    - yh-flow/clients/web/app/modules/[moduleId]/page.tsx
  modified:
    - yh-flow/clients/web/app/routes.ts
    - yh-flow/clients/web/app/components/sidebar/sidebar-tree.tsx
    - yh-flow/clients/web/app/components/issues/issue-detail-sidebar.tsx

key-decisions:
  - "Cycle 详情页左栏 Issue 列表使用 useCycleIssues 直接获取，不通过 IssueListView（后者不支持 cycleId 过滤）"
  - "Module 详情页左栏 Issue 列表使用 useModuleIssues 直接获取，不通过 IssueListView（后者不支持 moduleId 过滤）"
  - "Issue 添加功能使用简化内联弹窗占位，完整 Issue 选择器 deferred to Phase 19"
  - "Module 删除使用内联确认弹窗代替独立的 DeleteModuleModal（预置组件路径不一致）"

requirements-completed: [CYCLE-01, CYCLE-02, CYCLE-03, CYCLE-04, MODU-01, MODU-02, MODU-03]

duration: 15min
completed: 2026-06-30
---

# Phase 18 Plan 04: 路由注册和页面组件 Summary

**Cycles 和 Modules 的 4 个页面路由注册、侧边栏中文标签、Issue 详情侧栏导航集成**

## Performance

- **Duration:** 15 min
- **Started:** 2026-06-30T07:42:00Z
- **Completed:** 2026-06-30T07:57:00Z
- **Tasks:** 4
- **Files modified:** 7

## Accomplishments

- routes.ts 注册 4 个新路由（cycles 列表 + cycles 详情 + modules 列表 + modules 详情）
- 侧边栏 PROJECT_VIEWS 标签改为中文："周期" / "模块"
- Cycle 列表页（/cycles/）渲染 CyclesView + CycleModal
- Cycle 详情页（/cycles/:cycleId）双栏布局：左栏 Issue 列表/看板切换 + 右栏进度/Burndown/统计
- Module 列表页（/modules/）渲染 ModulesListView + ModuleModal
- Module 详情页（/modules/:moduleId）双栏布局：左栏 Issue 列表/甘特切换 + 右栏属性/进度/描述/链接
- Issue 详情侧栏显示关联 Cycle/Module 名称，可点击导航到对应详情页
- 无 cycle_id/module_ids 时不显示对应字段

## Task Commits

1. **Task 1: 注册 4 个 Cycle/Module 路由 + 侧边栏中文标签** - `a709d506c` (chore)
2. **Task 2: 创建 Cycle 列表页 + 详情页** - `c212553ae` (feat)
3. **Task 3: 创建 Module 列表页 + 详情页** - `8a3fb3027` (feat)
4. **Task 4: Issue 详情侧栏集成 Cycle/Module 导航** - `8aeedc856` (feat)

## Files Created/Modified

- `yh-flow/clients/web/app/routes.ts` - 添加 4 个新路由（cycles + modules 各 2 个）
- `yh-flow/clients/web/app/components/sidebar/sidebar-tree.tsx` - Cycles → 周期, Modules → 模块
- `yh-flow/clients/web/app/cycles/page.tsx` - Cycle 列表页
- `yh-flow/clients/web/app/cycles/[cycleId]/page.tsx` - Cycle 详情页（双栏 + 看板切换 + Burndown）
- `yh-flow/clients/web/app/modules/page.tsx` - Module 列表页
- `yh-flow/clients/web/app/modules/[moduleId]/page.tsx` - Module 详情页（双栏 + 甘特切换 + 属性/链接）
- `yh-flow/clients/web/app/components/issues/issue-detail-sidebar.tsx` - 添加 Cycle/Module 导航链接

## Decisions Made

- **IssueListView/KanbanView/GanttView 不支持 cycleId/moduleId 过滤**：这些组件内部使用 useIssues 查询全部项目 Issue，不支持按周期/模块过滤。因此详情页直接使用 useCycleIssues / useModuleIssues 获取过滤后的 Issue 列表，自行渲染 Issue 行。
- **Add Issue 功能简化占位**：完整 Issue 选择器（搜索已有 Issue + 关联操作）需要跨组件搜索和选择 UI，按计划 deferred to Phase 19。当前使用简化弹窗提示。
- **Module 删除使用内联弹窗**：项目中没有预置 DeleteModuleModal 组件（只有 CycleDeleteModal），使用内联确认弹窗替代。
- **路由顺序**：cycles/modules 路由放在 Issue 路由之后、layout 闭合之前。React Router v7 使用排名匹配，更具体的路由（4 段路径）优先于 `projects/*` splat，因此顺序不影响匹配。

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- **工作树不含 yh-flow 文件**：当前 worktree 基于较旧的 preview 分支 commit，不包含 yh-flow 子目录。通过 `git checkout preview -- yh-flow/` 从 preview 分支检出文件解决。

## Threat Flags

None - no new security-relevant surface introduced.

## Known Stubs

| File | Line | Description |
|------|------|-------------|
| app/cycles/[cycleId]/page.tsx | ~230 | "添加 Issue"按钮弹窗使用简化占位，完整 Issue 选择器 deferred to Phase 19 |
| app/modules/[moduleId]/page.tsx | ~260 | "添加 Issue"按钮弹窗使用简化占位，完整 Issue 选择器 deferred to Phase 19 |

## Next Phase Readiness

- 所有 4 个页面路由和组件已注册
- Cycle/Module 列表页和详情页可导航访问
- Issue 详情侧栏已集成 Cycle/Module 导航
- 侧边栏中文标签已更新
- 下一步（18-05 或 Phase 19）可添加完整 Issue 选择器和路由守卫

---
*Phase: 18-cycle-module / Plan: 04*
*Completed: 2026-06-30*
