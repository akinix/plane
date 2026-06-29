# Gantt Chart View

**Source:** Forked from Plane `apps/web/core/components/gantt-chart/`

**Purpose:** 甘特图视图 — Issue 时间跨度显示 + 依赖关系连线 (GANT-01, GANT-02)

**Fork Date:** 2026-06-29 (Phase 17-03)

**Key Differences from Plane original:**
- 所有导入路径适配到 yh-flow 项目结构
- `useTimeLineChartStore` 替换为简化版 React hook（非 MobX 版本）
- 移除 Plane web-only 组件引用（plane-web/ 下的 gantt-chart 特定组件）
- 移除 `@plane/i18n` 国际化，使用硬编码中文文本
- `GanttChartMainContent` 移除了 Plane web 专用 imports（依赖路径、批量操作等）
- 依赖关系连线 (GANT-02) 保持原始 Plane SVG 逻辑，Phase 17 为只读显示

**FLOW markers:** 每个文件中使用 `// FLOW:` 标记标识修改处
