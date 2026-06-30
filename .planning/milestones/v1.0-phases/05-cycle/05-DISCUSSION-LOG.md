# Phase 5: Cycle — 周期管理 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-24
**Phase:** 5-cycle
**Areas discussed:** 模块归属, Burndown 计算, 已结束 Cycle 编辑策略

---

## 灰色区域 1：模块归属

| Option                  | Description                                                        | Selected |
| ----------------------- | ------------------------------------------------------------------ | -------- |
| 纳入 WorkItemsDbContext | Cycle 和 Issue 在同一个 DbContext 中管理，简化跨实体查询和关联操作 | ✓        |
| 独立 CycleDbContext     | 保持模块边界清晰，Cycle 作为独立领域模块有自己的 Schema            |          |

**User's choice:** 纳入 WorkItemsDbContext
**Notes:** 用户先确认了之前新实体都是独立 DbContext（Phase 3 ProjectDbContext, Phase 4 WorkItemsDbContext）。解释 Cycle 的特殊性（CycleIssue 关联、Burndown 跨实体查询）后，用户选择纳入 WorkItemsDbContext。

---

## 灰色区域 2：Burndown 计算

| Option       | Description                                       | Selected |
| ------------ | ------------------------------------------------- | -------- |
| API 实时计算 | 每次请求时从 Issue 表实时聚合计算进度数据         | ✓        |
| 预计算快照   | Issue 变更时异步更新 progress_snapshot            |          |
| 混合模式     | 活跃周期实时计算，完成后冻结快照（贴 Plane 行为） |          |

**User's choice:** API 实时计算
**Notes:** 与 Plane 行为一致，实时计算保证数据准确性。progress_snapshot 仅在 transfer-issues 时使用。

---

## 灰色区域 3：已结束 Cycle 编辑策略

| Option         | Description                                                                 | Selected |
| -------------- | --------------------------------------------------------------------------- | -------- |
| 严格匹配 Plane | 和 Plane 完全一致：COMPLETED 周期只允许修改 sort_order 和转移未完成的 Issue | ✓        |
| 适度宽松       | 允许编辑名称/描述/日期，但禁止添加新 Issue                                  |          |
| 完全不限制     | 允许任意修改，包括添加新 Issue                                              |          |

**User's choice:** 严格匹配 Plane

---

## Claude's Discretion

- Cycle 状态：采用 Plane 的 Case/When 动态计算逻辑（DRAFT/UPCOMING/CURRENT/COMPLETED）
- CycleIssue 桥接表结构：与 Plane 一致，(Issue, Cycle) 唯一约束
- sort_order 自动递减策略：新 Cycle 自动设为 min - 10000
- 归档/取消归档端点设计
- 日期重叠校验端点
- Burndown 支持 `?type=issues|points` 切换

## Deferred Ideas

- CycleUserProperties（每用户显示设置）— Phase 13 前端
- Cycle 收藏 — Phase 13 前端
- 进度通知/邮件 — Phase 11 Notification
- 分析图表 API — Phase 12 Analytics
