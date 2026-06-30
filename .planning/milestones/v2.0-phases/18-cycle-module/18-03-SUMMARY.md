---
phase: 18-cycle-module
plan: 03
type: execute
wave: 2
status: complete
tasks: 2/2
created: 2026-06-30
completed: 2026-06-30
depends_on: [18-01]
provides: [Module 组件复刻（完整 Module 组件目录）]
requirements: [MODU-01, MODU-02, MODU-03]
---

# SUMMARY — 18-03: Module 组件复刻

## 目标

从 Plane apps/web/core/components/modules/ Fork Module 相关 UI 组件到 yh-flow，适配导入路径和类型系统。

## 任务完成情况

| #   | 任务                                                                        | 状态 | 提交      |
| --- | --------------------------------------------------------------------------- | ---- | --------- |
| 1   | Fork Module 列表组件 + 创建/编辑/删除（12 文件）                            | ✓    | `8e9b916` |
| 2   | Fork Module 详情相关组件（gantt/links/select/analytics/dropdowns，15 文件） | ✓    | `8e9b916` |

## 创建的文件

**Task 1 — Module 列表 + CRUD（12 文件）：**

- `modules-list-view.tsx` — 卡片网格/列表双视图 + 加载/空/错误状态
- `module-card-item.tsx` — 4 色状态徽章（待开始/进行中/已完成/已取消）+ 进度条 + Issue 计数
- `module-list-item.tsx` — 紧凑列表行（进度圆环 + 名称 + 状态 + 计数 + 操作）
- `module-list-item-action.tsx` — 行操作按钮
- `module-peek-overview.tsx` — 详情弹窗（名称/描述/状态/进度）
- `module-view-header.tsx` — 页头（标题"模块" + 创建按钮 + 视图切换 + 搜索）
- `form.tsx` — 创建/编辑表单（名称/描述/日期/状态/负责人，react-hook-form）
- `modal.tsx` — 创建/编辑弹窗（使用 @plane/ui Modal 风格）
- `module-status-dropdown.tsx` — 状态下拉（4 选项：待开始/进行中/已完成/已取消）
- `module-layout-icon.tsx` — 布局图标
- `delete-module-modal.tsx` — 删除确认弹窗（"模块内的 Issue 不会被删除"）
- `quick-actions.tsx` — 快速操作菜单（编辑/复制链接/新标签页/删除）

**Task 2 — Module 详情子组件（15 文件）：**

- `gantt-chart/blocks.tsx` — 甘特图区块（复用 Phase 17 GanttView per D-P18-11）
- `gantt-chart/modules-list-layout.tsx` — 甘特双栏布局
- `links/list-item.tsx` — 链接列表项 + URL 校验（T-18-MD-02 mitigate）
- `links/list.tsx` — 链接列表（添加/内联编辑/删除）
- `select/status.tsx` — Module 状态选择器（用于 Issue 属性面板）
- `sidebar-select/select-status.tsx` — 侧栏 Module 状态选择器
- `analytics-sidebar/root.tsx` — Module 分析侧栏
- `analytics-sidebar/issue-progress.tsx` — Issue 进度分组
- `analytics-sidebar/progress-stats.tsx` — 进度统计
- `dropdowns/order-by.tsx` — 排序切换

## 配套修复（同一 commit）

- **Cycle 组件导入路径修复** — `@/lib/hooks/` → `@/../src/lib/hooks/`（tsconfig `@/*` → `app/*`，hooks 在 `src/lib/` 下）
- **module.store.ts** — 添加 `deleteModuleId`/`deleteModuleName`/`openDeleteModal`/`closeDeleteModal`/`reset` 扩展
- **use-cycle-issues.ts** — 添加 `useCycleProgress` hook（按 state group 统计进度）
- **package.json** — 添加 `react-hook-form` 依赖（ModuleForm 使用）

## 关键决策

1. **import 路径约定**：组件在 `app/components/` 下，hooks 在 `src/lib/hooks/` 下，使用 `@/../src/lib/hooks/` 跨目录引用（tsconfig `@/*` → `app/*`）
2. **<div onClick> 改为 <button>**：满足 jsx-a11y 规范，避免 lint 警告
3. **Module 链接 URL 校验**：list-item.tsx 导出 `isValidUrl` 函数，要求 `http://` 或 `https://` 开头（T-18-MD-02）
4. **Module 甘特图复用 Phase 17**：gantt-chart/blocks.tsx 作为甘特图区块组件，通过 moduleId 过滤 Issue（D-P18-11）
5. **中文文案**：所有文案遵循 18-UI-SPEC Copywriting Contract

## 修正的 Lint 警告

- **jsx-a11y/click-events-have-key-events**: `<div onClick>` → `<button type="button">`（blocks.tsx, module-list-item.tsx）
- **jsx-a11y/no-static-element-interactions**: 同上
- **no-unused-vars**: 移除 `completedModuleCheck`、`draftCycles`、`workspaceSlug` 等未使用变量
- **react/no-array-index-key**: skeleton key `{i}` → `{skeleton-${i}}`
- **no-unused-vars (imports)**: 移除 `CycleDetailsSidebar` 等未使用导入

## 自检清单

- [x] Task 1: 12 个 Module 列表/CRUD 组件全部创建
- [x] Task 2: 15 个 Module 详情子组件全部创建（6 子目录）
- [x] 所有文件标记 `// FLOW: Forked from Plane`
- [x] ModuleCardItem 4 色状态徽章（待开始/进行中/已完成/已取消）
- [x] ModuleForm 创建/编辑模式（react-hook-form 验证）
- [x] links/list-item URL 校验（T-18-MD-02 mitigated）
- [x] 中文文案遵循 18-UI-SPEC
- [x] module.store.ts 扩展已注册到 root store
- [x] 配套导入路径修复（Cycle 组件 + hooks）
