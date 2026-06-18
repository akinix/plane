# YH.Flow — Roadmap

**Version:** 0.1.0  
**Date:** 2026-06-16

---

## Overview

```
Foundation → Workspace → Project → WorkItems → Cycle/Module/Page/View → Integration/Webhook → Notification/Analytics → Frontend
```

---

## Phase 0: 项目初始化 & 脚手架

**Goal:** 创建 YH.Flow 项目结构，配置所有基础设施

**Dependencies:** None  
**Duration Estimate:** 2-3 days

### Tasks

- T0.1: 基于 fullstackhero 模板创建 `yh-flow/` 项目结构
- T0.2: 移除不需要的模块（Catalog, Tickets, Chat, Billing）
- T0.3: 配置 Directory.Build.props, Directory.Packages.props, NuGet.Config
- T0.4: 重命名命名空间 FSH → YH
- T0.5: 配置 BuildingBlocks 引用
- T0.6: 创建 YH.Flow.slnx 解决方案文件
- T0.7: 配置 .NET Aspire AppHost（PostgreSQL, Redis, MinIO, API）
- T0.8: 验证项目可编译运行

**Deliverables:**

- 可编译的 YH.Flow 解决方案
- Aspire 编排可启动所有基础设施

---

## Phase 1: Foundation — 基础设施

**Goal:** 认证、授权、API 基础设施、数据库迁移管道

**Dependencies:** Phase 0  
**Duration Estimate:** 3-5 days

### Tasks

- T1.1: 配置 PostgreSQL 连接和 EF Core
- T1.2: 建立 DbMigrator + Migrations 项目
- T1.3: 实现多租户基础设施（Finbuckle header/query-string）
- T1.4: 实现 JWT Bearer 认证
- T1.5: 实现 API Key 认证（`X-Api-Key` header）
- T1.6: 实现 Session/Cookie 认证
- T1.7: 实现 OAuth Provider 框架
- T1.8: 配置 CORS, Security Headers, Rate Limiting
- T1.9: 实现全局异常处理（Plane 兼容格式）
- T1.10: 实现分页（Plane 兼容格式）
- T1.11: 配置 Scalar/OpenAPI
- T1.12: 配置 Serilog + OpenTelemetry

### Plans

**Plans:** 5 plans (4 waves)

Plans:
**Wave 1**

- [x] 01-01-PLAN.md — 多 Scheme 认证协商（API Key + Session Cookie + PolicyScheme 路由器）
- [x] 01-02-PLAN.md — Domain 实体 + EF 配置（APIToken + OAuthProviderSettings + IdentityDbContext）

**Wave 2** _(blocked on Wave 1 completion)_

- [ ] 01-03-PLAN.md — OAuth Provider 框架 + Plane 兼容认证端点（/auth/\*）

**Wave 3** _(blocked on Wave 2 completion)_

- [ ] 01-04-PLAN.md — API Token CRUD + OAuth Provider 管理 + Plane 格式适配

**Wave 4** _(blocked on Wave 3 completion)_

- [ ] 01-05-PLAN.md — [BLOCKING] Schema Push + 全阶段验证

**Deliverables:**

- 完整认证流程（JWT + API Key + Session）
- API 基础设施就绪

---

## Phase 2: Workspace — 工作区

**Goal:** 工作区 CRUD、成员管理、邀请系统

**Dependencies:** Phase 1  
**Duration Estimate:** 3-4 days  
**Requirements:** REQ-2.1 ~ REQ-2.4

### Tasks

- T2.1: 创建 Workspace 实体和领域模型
- T2.2: 创建 WorkspaceMember 实体
- T2.3: 创建 WorkspaceInvitation 实体
- T2.4: 实现 Workspace CRUD 端点
- T2.5: 实现 Workspace Member 端点
- T2.6: 实现 Workspace Invitation 端点
- T2.7: 实现 Workspace 设置端点
- T2.8: 创建 WorkspaceDbContext + 迁移
- T2.9: 实现 Workspace 权限

### Plans

**Plans:** 6 plans (6 waves)

Plans:

**Wave 1**

- [ ] 02-PLAN-00-INDEX.md — Phase 2 计划索引（wave/plan/REQ/decision 覆盖矩阵）
- [x] 02-01-PLAN.md — Wave 0：spike Q1（Finbuckle 外部追加 strategy）+ Workspace.Tests 脚手架 + Workspace.Contracts 契约 + Identity.Contracts 扩展
- [x] 02-02-PLAN.md — Wave 1：Domain 实体（Workspace/Member/Invitation）+ WorkspaceDbContext + 3 个 IEntityTypeConfiguration + Finbuckle slug strategy/store wiring（D-01）

**Wave 2** _(blocked on Wave 1 completion)_

- [x] 02-03-PLAN.md — Wave 2：[BLOCKING] EF 迁移应用 + DbMigrator/Api Program.cs wiring + WorkspaceMembershipMiddleware（D-02）+ [RequireWorkspaceRole] authz（D-11）

**Wave 3** _(blocked on Wave 2 completion)_

- [x] 02-04-PLAN.md — Wave 3：SlugGenerator（D-07/09）+ 6 个 Workspace Feature slice（Create/Get/Update/Delete/ListUserWorkspaces/CheckSlug）

**Wave 4** _(blocked on Wave 3 completion)_

- [x] 02-05-PLAN.md — Wave 4：UserIdentityService（Identity 模块）+ InvitationTokenService（D-12）+ 4 Member + 5 Invitation Feature slice（D-04/05/10/12）

**Wave 5** _(blocked on Wave 4 completion)_

- [x] 02-06-PLAN.md — Wave 5：[BLOCKING] 全量回归 + WorkspaceLifecycleSmoke + WorkspaceRoleCapability + checkpoint:human-verify 手工 smoke（automated ✅; manual smoke pending HUMAN-UAT.md）

**Deliverables:**

- 完整工作区管理 API
- 多租户数据隔离

---

## Phase 3: Project — 项目

**Goal:** 项目 CRUD、成员管理

**Dependencies:** Phase 2  
**Duration Estimate:** 2-3 days  
**Requirements:** REQ-3.1 ~ REQ-3.3

### Tasks

- T3.1: 创建 Project 实体和领域模型
- T3.2: 创建 ProjectMember 实体
- T3.3: 实现 Project CRUD 端点
- T3.4: 实现 Project Member 端点
- T3.5: 实现 Project 设置端点
- T3.6: 实现 Unsplash 封面图片搜索
- T3.7: 创建 ProjectDbContext + 迁移
- T3.8: 实现 Project 权限

**Deliverables:**

- 完整项目管理 API

---

## Phase 4: WorkItems — 工作项

**Goal:** State, Label, Issue CRUD, Comments, Activity, Estimate, Intake, Import/Export

**Dependencies:** Phase 3  
**Duration Estimate:** 5-7 days  
**Requirements:** REQ-4.1 ~ REQ-4.9

### Tasks

- T4.1: 创建 State 实体 + CRUD 端点
- T4.2: 创建 Label 实体 + CRUD 端点
- T4.3: 创建 Estimate 实体 + CRUD 端点
- T4.4: 创建 Issue 实体（含优先级、日期、指派等）
- T4.5: 创建 Issue 父子关联 + Issue 链接
- T4.6: 创建 IssueComment 实体
- T4.7: 创建 IssueActivity 实体（审计日志）
- T4.8: 实现 Issue CRUD 端点
- T4.9: 实现 Issue 批量操作端点
- T4.10: 实现 Issue 评论端点
- T4.11: 实现 Issue 活动日志端点
- T4.12: 实现 Intake 收件箱端点
- T4.13: 实现 Import/Export 端点
- T4.14: 创建 WorkItemsDbContext + 迁移
- T4.15: 实现 WorkItems 权限

**Deliverables:**

- 完整工作项管理 API
- Import/Export 功能

---

## Phase 5: Cycle — 周期管理

**Goal:** Cycle CRUD, Cycle-Issue 关联, Burndown 数据

**Dependencies:** Phase 4  
**Duration Estimate:** 2-3 days  
**Requirements:** REQ-5.1 ~ REQ-5.2

### Tasks

- T5.1: 创建 Cycle 实体和领域模型
- T5.2: 创建 CycleIssue 关联实体
- T5.3: 实现 Cycle CRUD 端点
- T5.4: 实现 Cycle-Issue 关联端点
- T5.5: 实现 Cycle 进度跟踪
- T5.6: 创建 CycleDbContext + 迁移
- T5.7: 实现 Cycle 权限

**Deliverables:**

- 完整周期管理 API

---

## Phase 6: Module — 模块管理

**Goal:** Module CRUD, Module-Issue 关联

**Dependencies:** Phase 4  
**Duration Estimate:** 2-3 days  
**Requirements:** REQ-6.1 ~ REQ-6.2

### Tasks

- T6.1: 创建 Module 实体和领域模型
- T6.2: 创建 ModuleIssue 关联实体
- T6.3: 实现 Module CRUD 端点
- T6.4: 实现 Module-Issue 关联端点
- T6.5: 实现 Module 进度跟踪
- T6.6: 创建 ModuleDbContext + 迁移
- T6.7: 实现 Module 权限

**Deliverables:**

- 完整模块管理 API

---

## Phase 7: Page — 文档管理

**Goal:** Page CRUD, 层级结构, 富文本存储

**Dependencies:** Phase 3  
**Duration Estimate:** 2-3 days  
**Requirements:** REQ-7.1 ~ REQ-7.2

### Tasks

- T7.1: 创建 Page 实体和领域模型
- T7.2: 实现 Page 层级结构（父子关系）
- T7.3: 实现 Page CRUD 端点
- T7.4: 实现 Page 归档/恢复
- T7.5: 实现 Page 访问权限
- T7.6: 创建 PageDbContext + 迁移
- T7.7: 实现 Page 权限

**Deliverables:**

- 完整文档管理 API（不含实时协作）

---

## Phase 8: View — 视图管理

**Goal:** View CRUD, 筛选条件存储

**Dependencies:** Phase 4  
**Duration Estimate:** 1-2 days  
**Requirements:** REQ-8.1 ~ REQ-8.2

### Tasks

- T8.1: 创建 View 实体和领域模型
- T8.2: 实现 View CRUD 端点
- T8.3: 实现 View 筛选条件校验
- T8.4: 创建 ViewDbContext + 迁移
- T8.5: 实现 View 权限

**Deliverables:**

- 完整视图管理 API

---

## Phase 9: Integration — 集成

**Goal:** GitHub, GitLab, Gitea, Slack, Unsplash 集成

**Dependencies:** Phase 3, Phase 4  
**Duration Estimate:** 3-5 days  
**Requirements:** REQ-9.1 ~ REQ-9.5

### Tasks

- T9.1: 实现 GitHub OAuth + API 客户端
- T9.2: 实现 GitHub Issue 双向同步
- T9.3: 实现 GitHub 评论同步
- T9.4: 实现 GitLab OAuth + 同步
- T9.5: 实现 Gitea OAuth + 同步
- T9.6: 实现 Slack OAuth + 通知同步
- T9.7: 实现 Unsplash 图片搜索
- T9.8: 创建 IntegrationDbContext + 迁移
- T9.9: 实现 Integration 权限

**Deliverables:**

- 完整集成管理 API

---

## Phase 10: Webhook — Webhook 管理

**Goal:** Webhook CRUD, 异步投递, 签名, SSRF 防护

**Dependencies:** Phase 2  
**Duration Estimate:** 2-3 days  
**Requirements:** REQ-10.1 ~ REQ-10.2

### Tasks

- T10.1: 创建 Webhook 实体
- T10.2: 创建 WebhookLog 实体
- T10.3: 实现 Webhook CRUD 端点
- T10.4: 实现 Webhook 投递（Hangfire 后台任务）
- T10.5: 实现 HMAC-SHA256 签名
- T10.6: 实现 SSRF 防护
- T10.7: 实现投递重试
- T10.8: 创建 WebhookDbContext + 迁移

**Deliverables:**

- 完整 Webhook 系统

---

## Phase 11: Notification — 通知系统

**Goal:** 站内通知, 邮件通知

**Dependencies:** Phase 2+  
**Duration Estimate:** 2-3 days  
**Requirements:** REQ-11.1 ~ REQ-11.2

### Tasks

- T11.1: 创建 Notification 实体
- T11.2: 实现通知生成（领域事件 → 通知）
- T11.3: 实现通知列表端点
- T11.4: 实现已读/未读管理
- T11.5: 实现邮件通知（Hangfire + MailKit）
- T11.6: 创建 NotificationDbContext + 迁移

**Deliverables:**

- 完整通知系统

---

## Phase 12: Analytics — 分析

**Goal:** 工作区/项目分析数据

**Dependencies:** Phase 4, 5, 6  
**Duration Estimate:** 2-3 days  
**Requirements:** REQ-12.1 ~ REQ-12.2

### Tasks

- T12.1: 实现 Issue 统计分析
- T12.2: 实现 Cycle 进度分析
- T12.3: 实现成员工作量分析
- T12.4: 实现仪表板数据聚合
- T12.5: 实现分析数据导出
- T12.6: 创建 AnalyticsDbContext + 迁移

**Deliverables:**

- 完整分析 API

---

## Phase 13: Flow Web — 前端

**Goal:** React Web 前端，UI 风格与 Plane 保持一致

**Dependencies:** Phase 1~12 (后端全部完成)  
**Duration Estimate:** 10-15 days  
**Requirements:** REQ-13.1 ~ REQ-13.4

### Tasks

- T13.1: 项目初始化（基于 FSH clients/dashboard 模板）
- T13.2: 布局框架（侧边栏、导航、面包屑）
- T13.3: 认证页面（登录、注册、OAuth）
- T13.4: 工作区管理页面
- T13.5: 项目管理页面
- T13.6: Issue 列表/详情/创建/编辑页面
- T13.7: Cycle 管理页面
- T13.8: Module 管理页面
- T13.9: Page 管理页面
- T13.10: View 管理页面
- T13.11: 集成管理页面
- T13.12: Webhook 管理页面
- T13.13: 通知页面
- T13.14: 分析仪表板页面
- T13.15: 设置页面（工作区、项目）
- T13.16: 主题切换（亮/暗）

**Deliverables:**

- 完整 Flow Web 前端

---

## Phase Dependency Graph

```
Phase 0: Init
    │
    ▼
Phase 1: Foundation
    │
    ▼
Phase 2: Workspace ──────────────────────────┐
    │                                          │
    ▼                                          │
Phase 3: Project ───────────────┐              │
    │                             │              │
    ▼                             │              │
Phase 4: WorkItems ◄──┐          │              │
    │    │    │    │    │          │              │
    ▼    ▼    ▼    ▼    │          │              │
    5    6    8    7    │          │              │
Cycle Module View Page  │          │              │
    │    │              │          │              │
    └────┼──────────────┘          │              │
         │                          │              │
         ▼                          │              │
    Phase 9: Integration ◄─────────┘              │
         │                                          │
         ▼                                          │
    Phase 10: Webhook ◄───────────────────────────┘
         │
         ▼
    Phase 11: Notification (can start after Phase 2)
         │
         ▼
    Phase 12: Analytics
         │
         ▼
    Phase 13: Frontend
```

## Parallelizable Work

以下 Phase 在依赖满足后可以并行执行：

- **Phase 5 (Cycle) + Phase 6 (Module) + Phase 8 (View)**: 都仅依赖 Phase 4
- **Phase 7 (Page)**: 仅依赖 Phase 3
- **Phase 9 (Integration) + Phase 10 (Webhook)**: 依赖不同
- **Phase 11 (Notification)**: 可以较早开始（Phase 2 完成后即可启动基础结构）

## Total Estimate

| Category        | Phases        | Days           |
| --------------- | ------------- | -------------- |
| Foundation      | 0, 1          | 5-8            |
| Core Domain     | 2, 3, 4       | 10-14          |
| Extended Domain | 5, 6, 7, 8    | 7-11           |
| Infrastructure  | 9, 10, 11, 12 | 9-14           |
| Frontend        | 13            | 10-15          |
| **Total**       |               | **41-62 days** |

\*With parallel execution of eligible phases: estimated **30-45 working days\***.
