# Requirements: YH.Flow (Flow)

**Defined:** 2026-06-30
**Milestone:** v3.0 — 前后端打通
**Core Value:** 将 Flow Web 前端从 Mock 数据切换到真实 .NET API，实现端到端全栈集成

## v3.0 Requirements

### INFRA — 基础设施

- [ ] **INFRA-01**: FlowApiService 请求拦截器添加 `humps.decamelizeKeys`，对所有 POST/PUT/PATCH 请求体进行 camelCase→snake_case 转换（含嵌套对象和查询字符串）
- [ ] **INFRA-02**: 实现 Token 刷新队列（单例 refresh promise + 请求队列），防止并发 401 时重复刷新和强制登出
- [x] **INFRA-03**: 创建集中式查询键工厂 `query-keys.ts`，统一管理所有模块的 TanStack Query 缓存键
- [ ] **INFRA-04**: 错误格式标准化 — 在响应拦截器中统一提取 ProblemDetails 和 Plane 格式错误消息，包括 FluentValidation 字段级错误映射
- [ ] **INFRA-05**: 分页提取层 — Service 层自动提取 `PlanePagedResult.results[]`，hooks 继续返回 `T[]` 数组

### WRKPRJ — 工作区 & 项目

- [ ] **WRKPRJ-01**: 工作区 Service（`workspace.service.ts`）接入真实 API，支持 CRUD 和切换
- [ ] **WRKPRJ-02**: 工作区成员 Service + hooks，支持邀请、角色变更、移除
- [ ] **WRKPRJ-03**: 项目 Service（`project.service.ts`）接入真实 API，支持 CRUD
- [ ] **WRKPRJ-04**: 项目成员 Service + hooks，支持成员管理
- [ ] **WRKPRJ-05**: API 路由从 UUID 适配为 Slack（从 MobX store 获取当前工作区 slug）

### ISSUES — 工作项

- [ ] **ISSUE-01**: Issue Service + hooks 接入真实 API，支持列表/详情/筛选
- [ ] **ISSUE-02**: Issue 状态（States）Service + hooks 接入真实 API
- [ ] **ISSUE-03**: Issue 标签（Labels）Service + hooks 接入真实 API
- [ ] **ISSUE-04**: Issue 评论（Comments）Service + hooks 接入真实 API
- [ ] **ISSUE-05**: Issue 活动日志（Activity）Service + hooks 接入真实 API
- [ ] **ISSUE-06**: 实现精确查询键失效策略，解决拖拽 Issue 跨状态移动时的乐观更新竞态
- [ ] **ISSUE-07**: 批量操作（选择多个 Issue 执行状态变更/分配/删除）接入真实 API

### CYCLE — 周期 & 模块

- [ ] **CYCLE-01**: Cycle Service + hooks 接入真实 API，支持 CRUD + 详情 + 看板
- [ ] **CYCLE-02**: Module Service + hooks 接入真实 API，支持 CRUD + 详情
- [ ] **CYCLE-03**: Cycle/Module 与 Issue 关联 API 对接

### PAGEVW — 页面 & 视图

- [ ] **PAGEVW-01**: Page Service + hooks 接入真实 API，支持 CRUD + 权限 + 收藏
- [ ] **PAGEVW-02**: 验证 TipTap 编辑器 JSON 结构在 `decamelizeKeys` 下的兼容性（跳过 `$` 开头的键）
- [ ] **PAGEVW-03**: 视图（Views）Service + hooks 接入真实 API，保存/应用自定义视图
- [ ] **PAGEVW-04**: 筛选/排序/分组条件对接真实 API 参数格式

### NTFA — 通知 & 分析

- [ ] **NTFA-01**: 通知 Service + hooks 接入真实 API（列表/已读/未读/归档）
- [ ] **NTFA-02**: SSE 实时推送对接 — EventSource 接收通知事件并写入 TanStack Query 缓存
- [ ] **NTFA-03**: 分析（Analytics）Service + hooks 接入真实 API（仪表板图表数据）
- [ ] **NTFA-04**: CSV 导出功能接入真实 API

### WEBHK — Webhook（新建前端管理页面）

- [ ] **WEBHK-01**: Webhook Service + hooks（继承 FlowApiService 标准模式）
- [ ] **WEBHK-02**: Webhook 订阅管理页面（列表/创建/编辑/删除/启用/禁用）
- [ ] **WEBHK-03**: Webhook 日志查看页面（最近发送记录、状态、重试）
- [ ] **WEBHK-04**: Webhook 测试发送功能（手动触发并查看响应）

### OUT-01 — 清理与移除

- [ ] **OUT-01**: 清理 Mock 数据 — 移除 `mock-data.ts` 和所有硬编码 Mock 常量
- [ ] **OUT-02**: 全局搜索替换硬编码 Mock ID（`ws-1`, `user-1` 等）
- [ ] **OUT-03**: 确认每个模块迁移后的 loading/error/empty 三态 UI 正常展示

## Out of Scope

| Feature                                       | Reason                               |
| --------------------------------------------- | ------------------------------------ |
| GitHub/GitLab/Gitea/Slack 集成（Integration） | 从 v1.0 Phase 9 继续推迟，非核心路径 |
| 实时协作编辑（Yjs/Hocuspocus）                | 后续里程碑，与 API 对接无关          |
| 多语言 i18n                                   | 一期仅中文，不纳入本次               |
| PDF 导出                                      | 按需添加，非本次范围                 |
| 桌面端/移动端原生                             | Web 优先，本次专注于 Web API 对接    |

## Traceability

| Requirement | Phase      | Status  |
| ----------- | ---------- | ------- |
| INFRA-01    | Phase 21   | Pending |
| INFRA-02    | Phase 21   | Pending |
| INFRA-03    | Phase 21   | Done    |
| INFRA-04    | Phase 21   | Pending |
| INFRA-05    | Phase 21   | Pending |
| WRKPRJ-01   | Phase 22   | Pending |
| WRKPRJ-02   | Phase 22   | Pending |
| WRKPRJ-03   | Phase 22   | Pending |
| WRKPRJ-04   | Phase 22   | Pending |
| WRKPRJ-05   | Phase 22   | Pending |
| ISSUE-01    | Phase 23   | Pending |
| ISSUE-02    | Phase 23   | Pending |
| ISSUE-03    | Phase 23   | Pending |
| ISSUE-04    | Phase 23   | Pending |
| ISSUE-05    | Phase 23   | Pending |
| ISSUE-06    | Phase 23   | Pending |
| ISSUE-07    | Phase 23   | Pending |
| CYCLE-01    | Phase 24   | Pending |
| CYCLE-02    | Phase 24   | Pending |
| CYCLE-03    | Phase 24   | Pending |
| PAGEVW-01   | Phase 25   | Pending |
| PAGEVW-02   | Phase 25   | Pending |
| PAGEVW-03   | Phase 25   | Pending |
| PAGEVW-04   | Phase 25   | Pending |
| NTFA-01     | Phase 26   | Pending |
| NTFA-02     | Phase 26   | Pending |
| NTFA-03     | Phase 26   | Pending |
| NTFA-04     | Phase 26   | Pending |
| WEBHK-01    | Phase 27   | Pending |
| WEBHK-02    | Phase 27   | Pending |
| WEBHK-03    | Phase 27   | Pending |
| WEBHK-04    | Phase 27   | Pending |
| OUT-01      | Phase 26   | Pending |
| OUT-02      | Phase 22   | Pending |
| OUT-03      | \*各 Phase | Pending |

**Coverage:**

- v3.0 requirements: 35 total
- Mapped to phases: 35
- Unmapped: 0 ✓

---

_Requirements defined: 2026-06-30_
_Last updated: 2026-06-30 after initial definition_
