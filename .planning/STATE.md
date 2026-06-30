---
gsd_state_version: 1.0
milestone: v3.0
milestone_name: 前后端打通
current_phase: 21 (infrastructure-foundation)
status: executing
last_updated: "2026-06-30T09:00:41.495Z"
last_activity: 2026-06-30 -- Phase 21 planning complete
progress:
  total_phases: 7
  completed_phases: 0
  total_plans: 3
  completed_plans: 0
  percent: 0
---

# YH.Flow — Project State

**Last Updated:** 2026-06-30
**Current Phase:** 21 (infrastructure-foundation)
**Active Workstream:** v3.0 milestone — 前后端打通

---

## Project Snapshot

| Key               | Value                                      |
| ----------------- | ------------------------------------------ |
| Project           | YH.Flow (Flow)                             |
| Type              | Backend migration + Full-stack rewrite     |
| Source Reference  | Plane (AGPL-3.0) -- Not modified           |
| Template          | fullstackhero/dotnet-starter-kit (.NET 10) |
| Location          | `d:/github/akinix-plane/yh-flow/`          |
| API Compatibility | Maintain Plane API contract                |

---

## Current State

### Completed

- [x] v3.0 需求定义 (`.planning/REQUIREMENTS.md` — 35 requirements, 8 categories)
- [x] v3.0 路线图定义 (`.planning/ROADMAP.md` — Phases 21-27)
- [x] 领域研究完成 (`.planning/research/SUMMARY.md` — 7 phase recommendations)

### v1.0 Milestone: COMPLETE (12/14 phases)

- [x] Phase 0: 项目初始化 & scaffolding
- [x] Phase 1: Foundation (Auth, API 基础设施)
- [x] Phase 2: Workspace (工作区 CRUD, 成员, 邀请)
- [x] Phase 3: Project (项目 CRUD, 成员)
- [x] Phase 4: WorkItems (Issue, State, Label, Comment, Activity)
- [x] Phase 5: Cycle (周期管理)
- [x] Phase 6: Module (模块管理)
- [x] Phase 7: Page (文档管理)
- [x] Phase 8: View (视图管理)
- [x] Phase 10: Webhook (Webhook 管理)
- [x] Phase 11: Notification (通知系统)
- [x] Phase 12: Analytics (分析)
- [ ] Phase 9: Integration — DEFERRED to future milestone
- [ ] Phase 13: Flow Web — DEFERRED; now v2.0 Phases 14~20

### v2.0 Milestone: COMPLETE (7/7 phases)

- [x] **Phase 14: 脚手架 & Auth** — 4/4 plans, COMPLETE
- [x] **Phase 15: 工作区 & 项目** — 4/4 plans, COMPLETE
- [x] **Phase 16: Issue 列表/详情 & 看板** — 5/5 plans, COMPLETE
- [x] **Phase 17: 日历/甘特/电子表格 & 筛选引擎** — 6/6 plans, COMPLETE
- [x] **Phase 18: 周期 & 模块** — 4/4 plans, COMPLETE
- [x] **Phase 19: 页面 & 视图** — 4/4 plans, COMPLETE
- [x] **Phase 20: 通知 & 分析** — 3/3 plans, COMPLETE

### v3.0 Milestone: Planning

- [ ] **Phase 21: Infrastructure Foundation** — API 请求格式转换、Token 刷新队列、集中式查询键工厂、错误标准化、分页提取层
- [ ] **Phase 22: Workspace & Project** — 工作区和项目 CRUD 接入真实 API，成员管理，slug 路由适配
- [ ] **Phase 23: WorkItems** — Issue/状态/标签/评论/活动日志/批量操作接入真实 API，乐观更新竞态处理
- [ ] **Phase 24: Cycles & Modules** — 周期和模块 CRUD 接入真实 API，与 Issue 关联
- [ ] **Phase 25: Pages & Views** — 页面和自定义视图接入真实 API，TipTap 编辑器兼容性验证
- [ ] **Phase 26: Notifications & Analytics + Cleanup** — 通知和分析接入真实 API，SSE 实时推送，Mock 数据清理
- [ ] **Phase 27: Webhook Admin UI** — Webhook 订阅管理前端页面（列表/创建/编辑/日志/测试发送）

---

## Artifacts

| File                                           | Description                   | Status |
| ---------------------------------------------- | ----------------------------- | ------ |
| `.planning/config.json`                        | Project configuration         | ✅     |
| `.planning/PROJECT.md`                         | Project context & vision      | ✅     |
| `.planning/REQUIREMENTS.md`                    | v3.0 requirements (35 total)  | ✅     |
| `.planning/ROADMAP.md`                         | v3.0 phase structure (21~27)  | ✅     |
| `.planning/STATE.md`                           | This file                     | ✅     |
| `.planning/codebase/STACK.md`                  | Tech stack analysis (Plane)   | ✅     |
| `.planning/codebase/ARCHITECTURE.md`           | Architecture analysis (Plane) | ✅     |
| `.planning/codebase/STRUCTURE.md`              | Code structure (Plane)        | ✅     |
| `.planning/codebase/INTEGRATIONS.md`           | Integrations analysis (Plane) | ✅     |
| `.planning/codebase/CONVENTIONS.md`            | Conventions analysis (Plane)  | ✅     |
| `.planning/codebase/TESTING.md`                | Testing analysis (Plane)      | ✅     |
| `.planning/codebase/CONCERNS.md`               | Concerns analysis (Plane)     | ✅     |
| `.planning/research/SUMMARY.md`                | Research summary (Plane Web)  | ✅     |
| `.planning/research/domain-overview.md`        | Domain entity model           | ✅     |
| `.planning/research/fullstackhero-patterns.md` | FSH pattern adaptation        | ✅     |
| `.planning/research/api-migration-mapping.md`  | Django to .NET API mapping    | ✅     |

---

## Key Decisions Log

| Decision                                                      | Rationale                                                                                        | Date       |
| ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ | ---------- |
| Use fullstackhero template                                    | Mature .NET 10 modular monolith with built-in multi-tenancy, CQRS, and all infrastructure needed | 2026-06-16 |
| Maintain API compatibility                                    | Existing Plane frontend can be reused; enables incremental migration                             | 2026-06-16 |
| New database design                                           | Full EF Core advantage; no legacy schema constraints                                             | 2026-06-16 |
| No real-time collaboration in Phase 1                         | Reduces complexity; Pages as plain CRUD initially                                                | 2026-06-16 |
| Project in Plane repo subdirectory                            | Easier cross-reference; single repo for migration period                                         | 2026-06-16 |
| No CI/CD scripts                                              | Focus on core functionality first; CI/CD added later                                             | 2026-06-16 |
| v2.0: Fork Plane packages instead of rewrite                  | Preserve Plane UI consistency, reduce risk, enable incremental API layer migration               | 2026-06-26 |
| v2.0: TanStack Query for server state, MobX for UI state only | Strict separation of concerns; MobX avoids re-architecting UI state stores from Plane Web        | 2026-06-26 |
| v2.0: JWT Bearer replace Session Cookie + CSRF                | Match .NET Identity authentication scheme                                                        | 2026-06-26 |
| v3.0: Phase 21 Infrastructure first                           | 所有后续模块依赖正确的请求格式、Token 刷新和查询键管理，必须先于第一个数据 hook 切换完成         | 2026-06-30 |
| v3.0: Workspace 作为首个验证试点                              | Workspace 数据结构简单，适合验证基础设施层改造是否正确                                           | 2026-06-30 |
| v3.0: OUT-02 归属 Phase 22                                    | 硬编码 Mock ID 替换应随工作区/项目迁移一起完成，因为 ID 主要在这些模块中使用                     | 2026-06-30 |
| v3.0: OUT-03 为跨阶段关注点                                   | 三态 UI 验证在每个模块迁移后的阶段中进行，非独立阶段                                             | 2026-06-30 |
| v3.0: Webhook 作为独立新功能最后执行                          | Webhook 是唯一无 Mock 数据阶段的新功能，从零接入最干净，不依赖其他模块迁移                       | 2026-06-30 |

---

## Current Position

Phase 21: Infrastructure Foundation
Plan: Not started
Status: Ready to execute
Last activity: 2026-06-30 -- Phase 21 planning complete

## Next Steps

1. Approve v3.0 roadmap
2. Execute `/gsd-plan-phase 21` to create execution plans for Phase 21

## Deferred Items

Items acknowledged and deferred at milestone close on 2026-06-30 (v2.0 close, v1.0 legacy):

| Category          | Item                                             | Status       |
| ----------------- | ------------------------------------------------ | ------------ |
| uat_gaps          | Phase 00: 00-HUMAN-UAT.md (2 pending scenarios)  | acknowledged |
| uat_gaps          | Phase 02: 02-HUMAN-UAT.md (11 pending scenarios) | acknowledged |
| uat_gaps          | Phase 02: 02-UAT.md (0 pending scenarios)        | acknowledged |
| verification_gaps | Phase 00: 00-VERIFICATION.md (human_needed)      | acknowledged |
| verification_gaps | Phase 02: 02-VERIFICATION.md (human_needed)      | acknowledged |

---

## Reference Paths

```
Plane Backend:     d:/github/akinix-plane/apps/api/
Plane Frontend:    d:/github/akinix-plane/apps/web/
Plane Packages:    d:/github/akinix-plane/packages/
FSH Template:      D:/github/fullstackhero-dotnet-starter-kit/
FSH Docs:          D:/github/fullstackhero-docs/
YH.Flow Target:    d:/github/akinix-plane/yh-flow/
Flow Web Frontend: d:/github/akinix-plane/yh-flow/clients/web/
```

---

## Operator Next Steps

- `/gsd-plan-phase 21` 生成 Phase 21 的执行计划
