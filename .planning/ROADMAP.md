# YH.Flow — Roadmap

**Version:** 3.0.0
**Date:** 2026-06-30

---

## Overview

```
v1.0 (Backend Core): Phase 0~13 — 完成后端 API 核心 (12/14 phases shipped)
v2.0 (Flow Web Frontend): Phase 14~20 — 构建完整 React 前端 (SHIPPED 2026-06-30)
v3.0 (前后端打通): Phase 21~27 — Flow Web 前端从 Mock 数据切换到真实 .NET API
```

**Strategy:** 逐个模块将 TanStack Query 数据源从 Mock 数据替换为真实 API 调用，先基础设施后业务域，先核心后外围。

---

## Milestones

- ✅ **v1.0 Backend Core** — Phases 0-13 (shipped, 12/14 phases)
- ✅ **v2.0 Flow Web Frontend** — Phases 14-20 (shipped 2026-06-30)
- 🚧 **v3.0 前后端打通** — Phases 21-27 (planning)

---

## Phases

- [x] **Phase 21: Infrastructure Foundation** — API 请求格式转换、Token 刷新队列、集中式查询键工厂、错误标准化、分页提取层
- [ ] **Phase 22: Workspace & Project** — 工作区和项目 CRUD 接入真实 API，成员管理，slug 路由适配
- [ ] **Phase 23: WorkItems** — Issue/状态/标签/评论/活动日志/批量操作接入真实 API，乐观更新竞态处理
- [ ] **Phase 24: Cycles & Modules** — 周期和模块 CRUD 接入真实 API，与 Issue 关联
- [ ] **Phase 25: Pages & Views** — 页面和自定义视图接入真实 API，TipTap 编辑器兼容性验证
- [ ] **Phase 26: Notifications & Analytics + Cleanup** — 通知和分析接入真实 API，SSE 实时推送，Mock 数据清理
- [ ] **Phase 27: Webhook Admin UI** — Webhook 订阅管理前端页面（列表/创建/编辑/日志/测试发送）

---

## Phase Details

### Phase 21: Infrastructure Foundation

**Goal**: 前端 API 通信层正确处理请求/响应格式转换、Token 生命周期管理和集中式查询键管理
**Depends on**: Nothing (first phase of v3.0)
**Requirements**: INFRA-01, INFRA-02, INFRA-03, INFRA-04, INFRA-05
**Success Criteria** (what must be TRUE):

1. 所有 POST/PUT/PATCH 请求体自动执行 camelCase→snake_case 转换（含嵌套对象和查询字符串），在网络层可验证
2. 并发 401 响应触发单次 Token 刷新，排队请求自动恢复，不会重复刷新或强制登出
3. 每个模块的 TanStack Query 缓存键均使用集中式 `query-keys.ts` 工厂，无临时字符串拼写
4. API 错误响应（ProblemDetails + Plane 格式 + FluentValidation 字段级错误）在响应拦截器中统一提取为前端可消费格式
5. Service 层自动解包 `PlanePagedResult.results[]`，hooks 层继续使用 `T[]` 数组
   **Plans**: 3 plans

Plans:

**Wave 1**

- [x] 21-01-PLAN.md — FlowApiService 拦截器：请求体转换 + Token 刷新队列 + 错误标准化（INFRA-01, INFRA-02, INFRA-04）
- [x] 21-02-PLAN.md — 集中式查询键工厂 query-keys.ts（INFRA-03）

**Wave 2** _(blocked on Wave 1 completion)_

- [x] 21-03-PLAN.md — 分页提取层 getList / getOne 方法（INFRA-05）

### Phase 22: Workspace & Project

**Goal**: 工作区和项目的 CRUD 操作使用真实 API 数据，成员管理完善，路由基于 slug
**Depends on**: Phase 21
**Requirements**: WRKPRJ-01, WRKPRJ-02, WRKPRJ-03, WRKPRJ-04, WRKPRJ-05, OUT-02
**Success Criteria** (what must be TRUE):

1. 用户可以创建、列表、编辑和删除工作区，数据持久化到 PostgreSQL
2. 用户可以邀请成员、变更角色和从工作区移除成员
3. 用户可以创建、列表、编辑和删除项目
4. 项目成员可以被添加、移除和变更角色
5. 所有 API 路由使用工作区 slug（而非 UUID），从 MobX store 获取当前上下文
6. 硬编码 Mock ID（`ws-1`, `user-1` 等）已全局搜索替换为动态值
   **Plans**: TBD
   **UI hint**: yes

### Phase 23: WorkItems

**Goal**: 完整 Issue 生命周期（CRUD、状态、标签、评论、活动日志、批量操作）使用真实 API，乐观更新正确处理
**Depends on**: Phase 22
**Requirements**: ISSUE-01, ISSUE-02, ISSUE-03, ISSUE-04, ISSUE-05, ISSUE-06, ISSUE-07
**Success Criteria** (what must be TRUE):

1. 用户可以查看、创建、编辑和删除 Issue，数据持久化到 PostgreSQL
2. Issue 状态、标签和评论均使用真实 API 数据
3. Issue 活动日志显示来自 API 的真实事件历史
4. 拖拽 Issue 跨状态移动触发乐观更新，无数据闪烁或陈旧 UI
5. 批量操作（选择多个 Issue 执行状态变更/分配/删除）提交到真实 API
   **Plans**: TBD
   **UI hint**: yes

### Phase 24: Cycles & Modules

**Goal**: 周期和模块管理使用真实 API 数据，与 Issue 交叉关联
**Depends on**: Phase 23
**Requirements**: CYCLE-01, CYCLE-02, CYCLE-03
**Success Criteria** (what must be TRUE):

1. 用户可以创建、列表、编辑和删除周期，数据持久化
2. 用户可以创建、列表、编辑和删除模块，数据持久化
3. 周期详情页显示与该周期关联的真实 Issue 列表
4. 模块详情页显示与该模块关联的真实 Issue 列表
   **Plans**: TBD
   **UI hint**: yes

### Phase 25: Pages & Views

**Goal**: 页面和自定义视图使用真实 API，TipTap 编辑器 JSON 内容在格式转换中兼容
**Depends on**: Phase 24
**Requirements**: PAGEVW-01, PAGEVW-02, PAGEVW-03, PAGEVW-04
**Success Criteria** (what must be TRUE):

1. 用户可以创建、列表、编辑和删除页面，数据持久化
2. 页面权限和收藏状态与真实 API 同步
3. TipTap 编辑器内容（JSONContent 结构）经过 camelCase/snake_case 转换后无数据丢失或结构损坏
4. 自定义视图可以保存、应用和删除，数据持久化
5. 筛选/排序/分组参数正确映射到 API 查询参数格式
   **Plans**: TBD
   **UI hint**: yes

### Phase 26: Notifications & Analytics + Cleanup

**Goal**: 通知和分析使用真实 API，SSE 实时推送生效，所有 Mock 数据文件移除
**Depends on**: Phase 25
**Requirements**: NTFA-01, NTFA-02, NTFA-03, NTFA-04, OUT-01
**Success Criteria** (what must be TRUE):

1. 用户可以通过真实 API 查看、标记已读、归档和取消归档通知
2. SSE 实时推送通过 EventSource 接收通知事件并写入 TanStack Query 缓存
3. 分析仪表板显示来自真实 API 的图表数据
4. CSV 导出使用真实 API 数据
5. 所有 `mock-data.ts` 文件和硬编码 Mock 常量已移除，构建零错误
   **Plans**: TBD
   **UI hint**: yes

### Phase 27: Webhook Admin UI

**Goal**: Webhook 管理前端页面完整可用，直接连接真实后端 API
**Depends on**: Phase 26
**Requirements**: WEBHK-01, WEBHK-02, WEBHK-03, WEBHK-04
**Success Criteria** (what must be TRUE):

1. 用户可以查看 Webhook 订阅的分页列表
2. 用户可以创建、编辑、启用、禁用和删除 Webhook 订阅
3. 用户可以查看 Webhook 投递日志（状态、重试历史、时间戳）
4. 用户可以触发测试 Webhook 发送并查看响应
5. 页面正确处理 loading/error/empty 三态 UI
   **Plans**: TBD
   **UI hint**: yes

---

## Cross-Cutting Concerns

| Concern              | Scope                                   | Verification                                               |
| -------------------- | --------------------------------------- | ---------------------------------------------------------- |
| OUT-03: 三态 UI 验证 | 每个 Phase 22~27 迁移后                 | 确认 loading/error/empty 状态在对应模块 UI 中正确展示      |
| 错误标准化           | Phase 21 (INFRA-04) 建立，各 Phase 验证 | 每个模块的 API 错误在前端正确显示                          |
| 统一 Service 模式    | 所有 Phase                              | 每个模块的 Service 继承 FlowApiService，hooks 使用标准模式 |

---

## Progress

| Phase                                   | Plans Complete | Status      | Completed |
| --------------------------------------- | -------------- | ----------- | --------- |
| 21. Infrastructure Foundation           | 3/3            | Complete     | 2026-06-30 |
| 22. Workspace & Project                 | 0/0            | Not started | -         |
| 23. WorkItems                           | 0/0            | Not started | -         |
| 24. Cycles & Modules                    | 0/0            | Not started | -         |
| 25. Pages & Views                       | 0/0            | Not started | -         |
| 26. Notifications & Analytics + Cleanup | 0/0            | Not started | -         |
| 27. Webhook Admin UI                    | 0/0            | Not started | -         |

---

_Last updated: 2026-06-30 after v3.0 milestone definition_
