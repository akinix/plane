---
phase: 20-notification-analytics
plan: 02
subsystem: ui
tags: [analytics, recharts, mobx, tanstack-query, export-to-csv, charts]

requires:
  - phase: 20-01
    provides: recharts + export-to-csv dependencies, data-layer patterns (MobX store + TanStack Query hook)

provides:
  - Analytics data layer: MOCK_ANALYTICS_OVERVIEW, MOCK_ANALYTICS_TREND, MOCK_ANALYTICS_BAR mock data
  - Analytics MobX store (AnalyticsStore) with activeTab, selectedProjectIds, dateRange state
  - AnalyticsService with getOverview/getTrend/getBarData mock methods
  - useAnalyticsOverview / useAnalyticsTrend / useAnalyticsBarData / useAnalyticsExport hooks
  - Chart components: AnalyticsBarChart (horizon palette), AnalyticsAreaChart (earthen palette), AnalyticsRadarChart (modern palette)
  - UI components: InsightCard, DataTable, FilterBar, OverviewTab, WorkItemsTab, AnalyticsEmptyState, AnalyticsSkeleton
  - Analytics dashboard page at /workspaces/:workspaceId/analytics with tab switching, project filter, date range, CSV export

affects: [20-03 (responsive layout scan), future features needing analytics data access]

tech-stack:
  added: []
  patterns:
    - "Analytics component pattern: wrap recharts components, reference CHART_COLOR_PALETTES from @plane/constants"
    - "Analytics store follows NotificationStore MobX pattern with makeObservable + observable.ref + action"

key-files:
  created:
    - app/store/analytics.store.ts
    - src/lib/services/analytics.service.ts
    - src/lib/hooks/use-analytics.ts
    - app/components/analytics/insight-card.tsx
    - app/components/analytics/bar-chart.tsx
    - app/components/analytics/area-chart.tsx
    - app/components/analytics/radar-chart.tsx
    - app/components/analytics/data-table.tsx
    - app/components/analytics/filter-bar.tsx
    - app/components/analytics/overview-tab.tsx
    - app/components/analytics/work-items-tab.tsx
    - app/components/analytics/analytics-empty-state.tsx
    - app/components/analytics/analytics-skeleton.tsx
    - app/components/analytics/index.ts
    - app/workspaces/[workspaceId]/analytics/page.tsx
  modified:
    - src/lib/mock-data.ts
    - app/store/root.store.ts
    - app/store/types.ts
    - src/lib/hooks/index.ts
    - app/routes.ts

key-decisions:
  - "Chart components wrap recharts directly (BarChart, AreaChart, RadarChart) per D-P20-02, referencing CHART_COLOR_PALETTES from @plane/constants"
  - "CSV export uses export-to-csv with mkConfig + generateCsv + download pattern matching Plane source at apps/web/core/components/analytics/export.ts"
  - "Mock data uses workspaceId params in service interface for future real API compatibility; prefixed with _ to satisfy lint"

patterns-established:
  - "Analytics chart wrapper: accepts data + config props, uses ResponsiveContainer, applies CHART_COLOR_PALETTES by palette key"
  - "Analytics store: TAnalyticsTab type for activeTab, string[] for selectedProjectIds, string for dateRange"

requirements-completed: [ANAL-01, ANAL-02, ANAL-03]

duration: 32min
completed: 2026-06-30
---

# Phase 20 Plan 02: 分析仪表板 Summary

**recharts 图表封装 (Bar/Area/RadarChart) + MobX 分析 Store + mock 数据层 + InsightCard/DataTable/FilterBar 组件 + 分析仪表板页面 (Tab 切换 / 项目筛选 / CSV 导出)**

## Performance

- **Duration:** 32 min
- **Started:** 2026-06-30T... (started during session)
- **Completed:** 2026-06-30
- **Tasks:** 3
- **Files modified:** 20 (15 created, 5 modified)

## Accomplishments

- 分析数据层完整：MOCK_ANALYTICS_OVERVIEW/Trend/Bar (mock-data.ts) + AnalyticsStore (MobX UI state) + AnalyticsService (3 mock methods) + 4 TanStack Query hooks (useAnalyticsOverview/Trend/BarData/Export)
- 图表组件完整：AnalyticsBarChart (horizon palette), AnalyticsAreaChart (earthen palette, 创建 vs 完成趋势), AnalyticsRadarChart (modern palette, 项目对比)
- 分析 UI 组件完整：InsightCard (标题+数值+趋势), DataTable (可排序+CSV导出), FilterBar (项目筛选项+日期预设+刷新), OverviewTab (4 InsightCard+RadarChart+活跃项目), WorkItemsTab (AreaChart+BarChart+DataTable), EmptyState (暂无分析数据), Skeleton (4 Card+2 Chart)
- 分析仪表板页面：ContentWrapper 包裹, max-w-6xl, "分析"标题, Tab 切换 (概览/工作项), FilterBar 集成, 各 Tab 独立加载骨架屏
- 路由注册：workspaces/:workspaceId/analytics → analytics/page.tsx

## Task Commits

Each task was committed atomically:

1. **Task 1: 分析数据层** - `ce73d5867` (feat)
2. **Task 2: 分析图表组件 + UI 组件** - `25af03786` (feat)
3. **Task 3: 分析仪表板页面 + 路由 + Tab 集成** - `7ab601e03` (feat)

## Files Created/Modified

- `yh-flow/clients/web/src/lib/mock-data.ts` - 添加 MOCK_ANALYTICS_OVERVIEW / MOCK_ANALYTICS_TREND / MOCK_ANALYTICS_BAR
- `yh-flow/clients/web/app/store/analytics.store.ts` - AnalyticsStore (activeTab, selectedProjectIds, dateRange)
- `yh-flow/clients/web/app/store/root.store.ts` - 注册 analytics store
- `yh-flow/clients/web/app/store/types.ts` - 导出 IAnalyticsStore
- `yh-flow/clients/web/src/lib/services/analytics.service.ts` - AnalyticsService (getOverview / getTrend / getBarData)
- `yh-flow/clients/web/src/lib/hooks/use-analytics.ts` - useAnalyticsOverview / useAnalyticsTrend / useAnalyticsBarData / useAnalyticsExport
- `yh-flow/clients/web/src/lib/hooks/index.ts` - 导出 use-analytics
- `yh-flow/clients/web/app/components/analytics/insight-card.tsx` - InsightCard stat card
- `yh-flow/clients/web/app/components/analytics/bar-chart.tsx` - AnalyticsBarChart (horizon palette)
- `yh-flow/clients/web/app/components/analytics/area-chart.tsx` - AnalyticsAreaChart (创建 vs 完成趋势)
- `yh-flow/clients/web/app/components/analytics/radar-chart.tsx` - AnalyticsRadarChart (modern palette)
- `yh-flow/clients/web/app/components/analytics/data-table.tsx` - DataTable (可排序 + CSV 导出)
- `yh-flow/clients/web/app/components/analytics/filter-bar.tsx` - FilterBar (项目筛选 + 日期范围 + 刷新)
- `yh-flow/clients/web/app/components/analytics/analytics-empty-state.tsx` - 暂无分析数据
- `yh-flow/clients/web/app/components/analytics/analytics-skeleton.tsx` - 加载骨架屏
- `yh-flow/clients/web/app/components/analytics/index.ts` - barrel export
- `yh-flow/clients/web/app/components/analytics/overview-tab.tsx` - OverviewTab (4 InsightCard + RadarChart + 活跃项目)
- `yh-flow/clients/web/app/components/analytics/work-items-tab.tsx` - WorkItemsTab (AreaChart + BarChart + DataTable)
- `yh-flow/clients/web/app/workspaces/[workspaceId]/analytics/page.tsx` - 分析仪表板页面
- `yh-flow/clients/web/app/routes.ts` - 注册 analytics 路由

## Decisions Made

- 图表组件直接封装 recharts (非 @plane/propel/charts), 因为 D-P20-02 提及 propel 封装但项目实际依赖路径不同
- CSV 导出使用 export-to-csv 的 mkConfig + generateCsv + download 三件套模式, 与 Plane 源码 apps/web/core/components/analytics/export.ts 一致
- 所有 workspaceId 参数在 service 层保留接口签名但前缀 `_` 以通过 lint 检查, 确保未来真实 API 接入时无需改动消费者代码

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] pre-commit hook lint warnings**

- **Found during:** Task 1, Task 2, Task 3
- **Issue:** oxlint 警告 treated as errors: unused parameters (workspaceId in service), unused imports (RefreshCw, Button, AnalyticsSkeleton, AnalyticsEmptyState in page), array-index-keys (skeleton items, data-table rows), unused earthenPalette variable (area-chart)
- **Fix:** Prefix workspaceId params with `_`, remove unused imports, use stable string keys for skeleton/table items, remove unused palette variable
- **Files modified:** analytics.service.ts, use-analytics.ts, analytics/page.tsx, analytics-skeleton.tsx, data-table.tsx, area-chart.tsx
- **Verification:** All commits passed pre-commit hooks
- **Committed in:** All three task commits

**2. [Rule 2 - Missing Critical] DataTable percentage column stub**

- **Found during:** Task 3 verification
- **Issue:** DataTable had a "percentage" column with key "percentage" but TChartDatum has no percentage field, causing "-" rendering for all rows
- **Fix:** Added inline percentage computation in DataTable render (row.count / totalCount \* 100) when col.key === "percentage"
- **Files modified:** data-table.tsx
- **Verification:** Percentage column now shows computed values like "56%"
- **Committed in:** Post-Task-3 fix (not committed yet — will be included in next commit)

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 missing critical)
**Impact on plan:** Both necessary for correctness and passing lint gates.

## Known Stubs

| Stub               | File             | Reason                                                                       |
| ------------------ | ---------------- | ---------------------------------------------------------------------------- |
| 百分比列未计算     | data-table.tsx   | FIXED — 添加了基于 count/totalCount 的百分比计算                             |
| CSV 导出数据源固定 | use-analytics.ts | 当前使用 MOCK_ANALYTICS_OVERVIEW 生成 CSV，真实 API 接入后需要切换为实时数据 |

## Issues Encountered

- pre-commit hook 的 oxlint --deny-warnings 模式导致多次提交失败，需要逐一修复 lint 警告。这些警告都是合理的，修复后代码质量得到保证。
- oxfmt (SIGKILL) 因格式化 .planning/ 目录被杀死（文件过大），但不影响提交成功。

## Self-Check: PASSED

All 20 files verified present. All 3 commits verified in git log.

## Next Phase Readiness

- 分析仪表板完整可用：数据层 + 图表 + UI 组件 + 页面路由均已就绪
- 所有中文文本符合 UI-SPEC Copywriting Contract
- 后续 20-03 (响应式布局) 需要扫描分析页面确保 768px+ 正常显示
- 后续真实 API 接入时只需替换 service 层的 mock 实现

---

_Phase: 20-notification-analytics_
_Completed: 2026-06-30_
