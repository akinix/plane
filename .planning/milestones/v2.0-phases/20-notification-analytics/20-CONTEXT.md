# Phase 20 Context: 通知 & 分析

**Created:** 2026-06-30
**Status:** Locked (after discuss)

---

## Domain Overview

### 通知系统 (NOTI)

Plane Web 的通知系统基于 **SWR 轮询 + MobX 乐观更新**，没有 SSE/WebSocket 实时通道。用户通过手动点击刷新按钮或切换 Tab 时重新获取数据。

- **数据结构**: `TNotification` (id, workspace, project, issue, actor, activity, content, created_at, read_at, archived_at, snoozed_till, etc.)
- **分页**: 300 条/页，通过 `next_page_results` 游标分页
- **操作**: 标记已读/未读、归档/取消归档、暂停/取消暂停、全部标记已读
- **过滤器**: 按类型筛选（已创建/已分配/已订阅），Tab 切换（全部/提及）
- **通知内容**: 基于 `issue_activity.field` 动态映射中文描述文本
- **通知偏好**: 邮件通知设置（属性变更、状态变更、评论、提及等 toggle）
- **Toast**: `@plane/propel/toast` 提供 `setToast`（success/error/info/warning/loading）

### 分析仪表板 (ANAL)

Plane Web 的分析使用 **recharts** (`@plane/propel/charts/*`) 作为图表库，通过 `AnalyticsService` 获取数据。

- **图表类型**: BarChart (条形/堆叠)、AreaChart (面积)、RadarChart (雷达)、PieChart (饼图)、LineChart (折线)
- **概览标签**: 总洞察数（InsightCard）、项目洞察（RadarChart）、活跃项目列表
- **工作项标签**: 创建 vs 解决趋势（AreaChart）、自定义洞察（BarChart，可选 X/Y/GroupBy）、数据表
- **CSV 导出**: 使用 `export-to-csv` npm 包，通过 `exportCSV` 工具函数
- **分析过滤**: 按项目、周期、模块、时间范围过滤

### UI-06 响应式布局

Plane Web 本身以桌面端为主，需要确保：

- 所有页面在 1024px+ 正常显示
- 平板端（768-1024px）不溢出、不布局断裂
- 侧边栏在窄屏可折叠

---

## Locked Decisions

### D-P20-01: 通知不实现 SSE/WebSocket 实时推送

- **决定**: 沿用 Plane Web 的 SWR 轮询 + 手动刷新模式
- **理由**:
  1. Plane 源码中通知没有 SSE/WebSocket 实现，fork 一致性原则（FD）
  2. 通知实时性不是关键路径（用户可接受数秒延迟）
  3. 减少后端基础设施复杂度
- **影响**: 不需要创建 SSE 服务或 WebSocket 连接管理；在通知列表中保留"刷新"按钮

### D-P20-02: 使用 recharts（通过 @plane/propel/charts）作为图表库

- **决定**: 复用 Plane 的 propel 图表封装，不引入新图表库
- **理由**: propel 已封装 recharts 并提供 BarChart/AreaChart/RadarChart/PieChart/LineChart，与 Plane 风格一致
- **影响**: 确认 `@plane/propel` 包已 fork 到本地构建流程中

### D-P20-03: CSV 导出使用 export-to-csv 包

- **决定**: 沿用 Plane 使用的 `export-to-csv` npm 包
- **理由**: Plane 的分析导出已使用该包并打包了 `exportCSV` 工具函数；不需要服务端导出
- **影响**: 确认 `export-to-csv` 在本地 yarn.lock/pnpm-lock 中可用

### D-P20-04: 通知数据挂在 workspace 级别

- **决定**: 通知列表入口在侧边栏/顶栏，路由为 `/workspaces/:workspaceId/notifications`
- **理由**: Plane 源码中通知是 workspace 级别的，不在 project 下
- **影响**: 需要创建通知路由和对应的 MobX store

### D-P20-05: 分析数据挂在 workspace 级别，支持 project 筛选

- **决定**: 分析仪表板在 workspace 级别展示，可选择特定 project 查看详情
- **理由**: Plane 的分析 API 是 `GET /api/workspaces/{slug}/advance-analytics`，底层支持 project 筛选
- **影响**: 主要入口为 workspace 分析页，project 级别的分析作为一个 Tab 或 filter

### D-P20-06: 通知和分析的 MobX store 放在 app/store/ 目录

- **决定**: 遵循 Phase 15 建立的 MobX store 模式（非 Plane 的 core/store/ 结构）
- **理由**: 保持 Flow Web 内部一致性（PageStore, ViewStore 等都在 app/store/）
- **影响**: 创建 `app/store/notification.store.ts` 和 `app/store/analytics.store.ts`

### D-P20-07: 通知/分析暂时使用 MOCK 数据

- **决定**: 与 Phase 15-19 保持一致，先用 MOCK_DATA 占位
- **理由**: 后端 .NET API 尚未部署通知和分析端点；mock 数据使 UI 可独立开发验证
- **影响**: 扩展 `src/lib/mock-data.ts` 添加 MOCK_NOTIFICATIONS 和 MOCK_ANALYTICS

### D-P20-08: 通知侧边栏采用简化版 UI（非 Plane 完整版）

- **决定**: 实现核心功能（列表、已读/未读、全部标记已读），暂不实现暂停/归档/提及过滤器
- **理由**: MVP 目标；暂停/归档/提及在 MVP 后可作为增强
- **影响**: 通知只实现 NOTI-01/03/04 的核心流程

### D-P20-09: UI-06 响应式布局推迟到 Phase 20 末尾

- **决定**: UI-06 作为 Phase 20 的最后一个 Plan 执行，确保不阻塞 NOTI/ANAL 功能
- **理由**: 响应式需要验证所有页面（Phase 14-20），放在最后做一次性全局扫描
- **影响**: Phase 20 的最后一个 Plan 处理所有页面 responsive 修复

---

## Requirements Coverage

| Req ID  | Description      | Assigned To        |
| ------- | ---------------- | ------------------ |
| NOTI-01 | 站内通知列表     | Plan 01            |
| NOTI-02 | SSE 实时通知     | SKIPPED (D-P20-01) |
| NOTI-03 | 标记已读/未读    | Plan 01            |
| NOTI-04 | 通知跳转 Issue   | Plan 01            |
| ANAL-01 | 工作区分析仪表板 | Plan 02            |
| ANAL-02 | 项目级别分析图表 | Plan 02            |
| ANAL-03 | CSV 导出         | Plan 02            |
| UI-06   | 响应式布局       | Plan 03            |

---

## Estimated Plans

| Plan  | Focus                                            | Depends On                       |
| ----- | ------------------------------------------------ | -------------------------------- |
| 20-01 | Notifications — store, hooks, list UI, mark-read | Phase 19 (routes, hooks pattern) |
| 20-02 | Analytics — store, charts, dashboard, CSV export | 20-01 (same data layer patterns) |
| 20-03 | Responsive layout — global scan + fixes          | 20-01, 20-02                     |

---

## Key Files & Patterns

### Reference files in plane-web:

- `apps/web/core/store/notifications/workspace-notifications.store.ts` — MobX store
- `apps/web/core/store/notifications/notification.ts` — Individual notification model
- `apps/web/core/hooks/store/notifications/use-workspace-notifications.ts` — Hook
- `apps/web/core/services/workspace-notification.service.ts` — Notification service
- `apps/web/core/services/analytics.service.ts` — Analytics service
- `apps/web/core/components/analytics/` — Analytics components
- `apps/web/core/components/workspace-notifications/` — Notification components
- `packages/types/src/workspace-notifications.ts` — Types
- `packages/types/src/analytics.ts` — Analytics types
- `packages/constants/src/notification.ts` — Notification constants
- `packages/constants/src/chart.ts` — Chart constants/colors
- `packages/propel/src/charts/` — Recharts wrappers

### Existing Flow Web patterns (follow these):

- `yh-flow/clients/web/app/store/page.store.ts` — MobX store pattern
- `yh-flow/clients/web/app/store/view.store.ts` — MobX store pattern
- `yh-flow/clients/web/src/lib/hooks/use-pages.ts` — TanStack Query hook pattern
- `yh-flow/clients/web/src/lib/mock-data.ts` — Mock data pattern
- `yh-flow/clients/web/src/lib/hooks/use-page-mutations.ts` — Mutation pattern
