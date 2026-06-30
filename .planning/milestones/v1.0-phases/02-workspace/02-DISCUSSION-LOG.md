# Phase 2: Workspace — 工作区 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-17
**Phase:** 02-workspace
**Areas discussed:** 租户解析与隔离策略, 跨模块 User 引用方式, Slug 生成与软删除兼容, 邀请机制与权限模型

---

## 租户解析与隔离策略

### Q1: Finbuckle 的租户怎么定义与解析？

| Option                 | Description                                                                                 | Selected |
| ---------------------- | ------------------------------------------------------------------------------------------- | -------- |
| 路由解析器（推荐）     | TenantId=WorkspaceId，自定义 ITenantResolver 从 URL {slug} 解析；子实体 IHasTenant 自动过滤 | ✓        |
| 手动 scope（贴 Plane） | 每实体显式 WorkspaceId FK，handler 手动过滤；样板多、漏过滤风险                             |          |
| 你来定                 | 留给 Claude 调研 Finbuckle 路由解析器后权衡                                                 |          |

### Q2: WorkspaceMember 成员资格校验放哪里？

| Option             | Description                                                                       | Selected |
| ------------------ | --------------------------------------------------------------------------------- | -------- |
| 独立中间件（推荐） | resolver 只解析，成员/角色校验放独立 middleware/handler；公开端点可解析不要求成员 | ✓        |
| 合并进解析器       | resolver 内同时校验，混淆职责                                                     |          |
| 每端点手动         | RequirePermission/手动查，跨模块易漏                                              |          |

### Q3: 下游模块如何获取当前 workspace 租户上下文？

| Option                 | Description                                                      | Selected |
| ---------------------- | ---------------------------------------------------------------- | -------- |
| Contracts 服务（推荐） | Workspace.Contracts 暴露 ICurrentWorkspaceContext 封装 Finbuckle | ✓        |
| 直连 Finbuckle         | 下游直接注入 IMultiTenantContextAccessor，跨模块边界             |          |
| 你来定                 | 留给规划阶段                                                     |          |

**Notes:** 架构级决策，影响后续所有 phase。用户全部选推荐项。

---

## 跨模块 User 引用方式

### Q1: WorkspaceMember 如何引用 Identity 的 User？

| Option                 | Description                                                | Selected |
| ---------------------- | ---------------------------------------------------------- | -------- |
| Contracts 服务（推荐） | 只存 UserId（无跨模块 FK），经 IUserIdentityService 取详情 | ✓        |
| 共享 User 内核         | User 下沉共享，多模块直接 FK，违背边界                     |          |
| 无 FK 跨表引用         | EF Core 映射 Identity 表但不强制 FK，折中但有隐患          |          |
| 你来定                 | 调研 FSH 跨模块 User 惯例                                  |          |

### Q2: 列出成员时如何获取用户详情？

| Option             | Description                                        | Selected |
| ------------------ | -------------------------------------------------- | -------- |
| 同步批量查（推荐） | 收集 UserIds→批量查 Identity→拼装，无冗余强一致    | ✓        |
| 冗余快照           | 领域事件同步用户字段到冗余列，eventual consistency |          |
| 你来定             | 权衡读写比                                         |          |

### Q3: Workspace 的 owner 如何表达？

| Option                   | Description                                    | Selected |
| ------------------------ | ---------------------------------------------- | -------- |
| OwnerId + Member（推荐） | 贴 Plane，存 OwnerId 标量+自动建 Admin Member  | ✓        |
| IsOwner 标记             | 不存 owner，加 IsOwner 布尔位，偏离 Plane 响应 |          |
| 你来定                   | 对照 Plane 序列化器决定                        |          |

---

## Slug 生成与软删除兼容

### Q1: Workspace 的 slug 从哪来？

| Option       | Description                                                  | Selected |
| ------------ | ------------------------------------------------------------ | -------- |
| 混合（推荐） | 接受用户提交（校验）；未提供则 slugify(name)；冲突加随机后缀 | ✓        |
| 仅用户提交   | 严格贴 Plane，冲突报错                                       |          |
| 仅自动生成   | 不接受用户提交，偏离 Plane 契约                              |          |
| 你来定       | 对照 Plane create 序列化器                                   |          |

### Q2: 软删除时 slug 怎么处理？

| Option               | Description                                                  | Selected |
| -------------------- | ------------------------------------------------------------ | -------- |
| 追加 epoch（推荐）   | 软删除追加 \_\_{epoch} 释放 slug 复用+deleted_at（贴 Plane） | ✓        |
| FSH 标准 soft-delete | deleted_at，slug 不变，占用不复用                            |          |
| 硬删除               | 物理删除，违背 NFR-3                                         |          |
| 你来定               | 规划阶段权衡                                                 |          |

### Q3: Slug 受限词与格式校验？

| Option              | Description                                                          | Selected |
| ------------------- | -------------------------------------------------------------------- | -------- |
| 黑名单+格式（推荐） | 移植 RESTRICTED_WORKSPACE_SLUGS + 格式校验（小写/数字/连字符/max48） | ✓        |
| 仅格式校验          | 不维护黑名单，可能占用保留路由                                       |          |
| 你来定              | 从 Plane 常量移植                                                    |          |

---

## 邀请机制与权限模型

### Q1: Phase 2 邀请是否实际发送邮件？

| Option             | Description                                                                             | Selected |
| ------------------ | --------------------------------------------------------------------------------------- | -------- |
| 只生成链接（推荐） | 生成 token+链接+记录+accept/reject/revoke；邮件延后 Phase 11；预留 INotificationService | ✓        |
| Phase 2 发邮件     | 用 FSH Mailing 发，跨 phase，与 Phase 11 重复                                           |          |
| 你来定             | 权衡 phase 边界与 REQ-2.4                                                               |          |

### Q2: 如何实现 workspace 权限（per-workspace 角色）？

| Option                      | Description                                                                           | Selected |
| --------------------------- | ------------------------------------------------------------------------------------- | -------- |
| Workspace-role 授权（推荐） | [RequireWorkspaceRole] handler 从 ICurrentWorkspaceContext 读角色；Admin/Member/Guest | ✓        |
| 复用 FSH 全局 permission    | 全局 permission 不天然支持 per-workspace 角色，需额外映射                             |          |
| 你来定                      | 调研 FSH 授权管线                                                                     |          |

### Q3: 邀请 token 格式与生命周期？

| Option                | Description                                       | Selected |
| --------------------- | ------------------------------------------------- | -------- |
| 随机+哈希+TTL（推荐） | 随机安全 token+哈希存储+可配置 TTL+失效；安全最佳 | ✓        |
| 贴 Plane 明文         | 随机 token 明文比对，无 TTL，泄露风险             |          |
| JWT                   | 无状态但撤销难（需黑名单）                        |          |
| 你来定                | 规划阶段权衡                                      |          |

**Notes:** 用户在每个 Q3 后选择"下一个领域"，全部 4 领域按推荐项锁定。

---

## Claude's Discretion

- TenantId=Guid(workspace Id)、slug→id Redis 缓存、解析失败 404/非成员 403、Workspace 实体不实现 IHasTenant、顶层无 slug 端点走 user-scoped
- 审计字段复用 ICurrentUser；Identity.Contracts 暴露 UserSummary+IUserIdentityService（复用现有 v1/Users DTO）
- slugify 库/随机后缀字母表、唯一性竞态重试、workspace-slug-check 端点
- role→能力矩阵贴 Plane（Admin/Member/Guest）；仅 owner 可删工作区
- token 哈希 SHA-256、TTL 默认值、接受端点建 WorkspaceMember
- INotificationService 抽象位置、EF 配置组织、Contracts DTO 结构、Mediator feature 切分、WorkspaceDbContext 边界

## Deferred Ideas

- 邀请邮件实际发送 → Phase 11 Notification
- State/Label/Estimate 实体 CRUD → Phase 4 WorkItems（REQ-2.3 配置仅 JSON 占位）
- 公开端点 /api/public/workspaces/{slug}/ → 规划阶段决定
- Plane workspace 边缘子资源（themes/quick-links/stickies/preferences/draft-issues 等）→ 一期不做
- Team 实体 → 一期不实现
- 工作区封面 Unsplash → Phase 3/9
