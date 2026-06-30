# Phase 2: Workspace — 工作区 - Context

**Gathered:** 2026-06-17
**Status:** Ready for planning

<domain>
## Phase Boundary

交付 `YH.Modules.Workspace` 模块 —— 与 Plane API 契约兼容的**多租户工作区管理**能力：工作区 CRUD（含 slug 生成、软删除）、成员管理（角色 Admin=20/Member=15/Guest=5）、邀请系统（token/链接、接受/拒绝/撤销）、工作区设置、`WorkspaceDbContext` + 迁移（`yhschema.Workspace`）、工作区权限。

**关键：本阶段同时确立后续所有 phase（Project/WorkItems/Cycle/Module/Page/View/...）依赖的多租户解析与隔离栈** —— 所有 `/api/v1/workspaces/{slug}/...` 端点都建立在此处决策之上。

**In scope（ROADMAP T2.1~T2.9，REQ-2.1~2.4）：**

- Workspace 实体 + 领域模型（slug/owner/logo/timezone 等）
- WorkspaceMember 实体（角色、激活态）
- WorkspaceInvitation 实体（email/token/role/accepted）
- Workspace CRUD 端点（含 slug-check）
- Workspace Member 端点（list/invite/update-role/remove/leave）
- Workspace Invitation 端点（create/list/revoke/join）
- Workspace 设置端点（通用设置）
- WorkspaceDbContext + 迁移（`yhschema.Workspace`）
- Workspace 权限 + **多租户解析/隔离基座**（Finbuckle 路由解析器 + 成员中间件 + Contracts 上下文）

**Out of scope（讨论中明确的边界）：**

- 邮件实际发送 → Phase 11 Notification（Phase 2 只生成邀请链接，预留 `INotificationService` 抽象）
- State/Label/Estimate **实体** CRUD → Phase 4 WorkItems；REQ-2.3 "默认 states/labels/estimate 配置" 在 Phase 2 仅存 JSON 配置/偏好占位，不建实体
- 公开端点 `/api/public/workspaces/{slug}/`（Space app 无认证读）→ 规划阶段决定是否纳入 Phase 2
  </domain>

<decisions>
## Implementation Decisions

### 租户解析与隔离策略（架构级 —— 影响后续所有 phase）

- **D-01: 路由租户解析器** — Finbuckle `TenantId = WorkspaceId`，自定义 `ITenantResolver` 从 URL 段 `/api/v1/workspaces/{slug}/` 解析 slug→查 workspace→设 TenantId。所有子实体实现 `IHasTenant`，BaseDbContext 自动按 WorkspaceId 过滤。保留 Plane URL 契约 + 复用 Phase 1 已建的 FSH 自动过滤，跨 12 模块零漏过滤风险。
- **D-02: 成员资格校验独立中间件** — resolver 只解析租户；成员资格+角色校验放独立 `WorkspaceMembershipMiddleware`/授权 handler（负责填充 `ICurrentWorkspaceContext`）。公开端点 `/api/public/...` 可解析租户但不要求成员资格。
- **D-03: Contracts 暴露租户上下文** — `Workspace.Contracts` 暴露 `ICurrentWorkspaceContext`（封装 Finbuckle `IMultiTenantContextAccessor`，提供 `CurrentWorkspaceId/Slug/CurrentUserRole`），下游模块依赖 Contracts 不直接耦合 Finbuckle。

### 跨模块 User 引用方式

- **D-04: Contracts 服务引用** — Workspace 只存 `UserId`（Guid 标量，无跨模块 FK），需要用户详情时调 `Identity.Contracts` 的 `IUserIdentityService`（批量查 email/名/头像）。严守模块边界，跨 12 模块一致（Phase 3 ProjectMember 等复用此模式）。
- **D-05: 同步批量查用户详情** — list 成员→收集 UserIds→一次批量查 Identity→拼装响应。无冗余、强一致，单库同进程延迟低。
- **D-06: Owner 表达贴 Plane** — Workspace 存 `OwnerId`（Guid 标量，无 FK），创建时自动建一条 Admin 角色的 `WorkspaceMember`。用于所有权转移、仅 owner 可删工作区等语义，与 Plane 响应字段一致。

### Slug 生成与软删除兼容

- **D-07: 混合 slug 来源** — 创建负载接受 slug（贴 Plane，前端可输入+唯一性/受限词校验）；未提供时自动 `slugify(name)`；冲突时加短随机后缀（如 `acme-x7k2`）。
- **D-08: 软删除追加 epoch** — 复制 Plane，软删除时追加 `__{epoch}` 到 slug（释放原 slug 可被新工作区复用）+ `deleted_at` 标记。
- **D-09: 受限词黑名单 + 格式校验** — 维护受限 slug 黑名单（移植 Plane `RESTRICTED_WORKSPACE_SLUGS`，保留路由/系统词如 api/admin/settings/auth/web）+ 格式校验（小写/数字/连字符，max48）。

### 邀请机制与权限模型

- **D-10: Phase 2 只生成邀请链接** — 生成 token+邀请链接+持久化邀请记录+accept/reject/revoke 逻辑，返回链接给调用方；实际邮件发送延后到 Phase 11（Hangfire+Mailkit）。预留 `INotificationService` 抽象待 Phase 11 接入。
- **D-11: Workspace-role 授权** — 自定义 `[RequireWorkspaceRole(Admin|Member|Guest)]` 授权 requirement+handler，从 `ICurrentWorkspaceContext` 读当前 workspace 角色。Admin=全权限/Member=读写/Guest=只读。基于 ASP.NET Core 授权管线、workspace 感知，与 Plane role 语义一致。
- **D-12: 邀请 token 随机+哈希+TTL** — 随机安全 token（CryptographicallyRandom）+ 可配置 TTL（如 7 天）+ 接受/拒绝/撤销后失效；数据库存哈希不明文，查询时哈希比对。

### Claude's Discretion

- `TenantId` 类型 = Guid（= workspace 的 Id）；slug→workspaceId 解析结果走 Redis 缓存（Phase 1 Caching BuildingBlock），workspace 更新时失效
- 解析失败/slug 不存在返回 404（不泄露存在性），非成员/角色不足返回 403
- Workspace 实体本身**不**实现 `IHasTenant`（它 IS 租户），其下所有实体实现
- 顶层端点（`GET/POST /workspaces/`、`/users/me/workspaces/invitations/`）路由无 slug 时 resolver 不设租户，走 user-scoped 逻辑
- `created_by/updated_by` 审计字段复用 Phase 1 的 `ICurrentUser`（取 UserId）
- `Identity.Contracts` 暴露 `UserSummary` DTO（id/display_name/email/avatar_url）+ `IUserIdentityService.GetUsersByIdsAsync(IEnumerable<Guid>)`；尽量复用现有 `v1/Users` DTO，避免重复定义
- slugify 库选择（Slugify/SlugBuilder 或自实现）+ 随机后缀长度/字母表；slug 唯一性竞态用 DB 唯一约束+冲突重试兜底；`workspace-slug-check/` 端点贴 Plane
- role→能力矩阵贴 Plane：Admin（CRUD 工作区/设置/成员/邀请）、Member（读写工作区内容，不能管成员/设置）、Guest（只读）；仅 owner 可删工作区（D-06）
- 邀请 token 哈希用 SHA-256（token 非密码，足够）；TTL 默认值与配置键；邀请接受端点接受时创建 `WorkspaceMember`（role 取邀请记录）
- `INotificationService` 抽象放 `Workspace.Contracts` 或共享 BuildingBlocks（规划阶段定）
- EF Core 实体配置（IEntityTypeConfiguration）组织方式、Contracts DTO 结构、Mediator feature 切分 —— 贴 Phase 1 Identity 模块 `Features/v1` 模式
- `WorkspaceDbContext` 边界 —— 只含 Workspace/Member/Invitation 等本模块实体

</decisions>

<canonical_refs>

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 项目规划文档

- `.planning/PROJECT.md` — 项目愿景、架构策略、模块映射、约束（AP API 兼容优先 / MP 模块化单体 / FS FSH 规范 / NC 原始代码不可变）
- `.planning/REQUIREMENTS.md` — Phase 2 需求（REQ-2.1~REQ-2.4）、非功能需求（NFR-1~NFR-4）
- `.planning/ROADMAP.md` — Phase 2 任务列表（T2.1~T2.9）
- `.planning/STATE.md` — 项目状态和关键决策日志

### 先验阶段上下文

- `.planning/phases/01-foundation/01-CONTEXT.md` — Phase 1 决策（D-12~D-15 Schema/迁移策略、多租户 Finbuckle `IHasTenant`、模块结构、权限模型）— **直接继承，本 phase 基于其搭建**

### 研究文档

- `.planning/research/domain-overview.md` — 领域实体模型（Workspace/Project/Issue 关系）
- `.planning/research/api-migration-mapping.md` §Workspace Endpoints — Django→.NET workspace 端点映射表
- `.planning/research/fullstackhero-patterns.md` — FSH 模式适配（10 条黄金法则、模块结构、API 端点模式）

### 代码库分析（Plane 原始代码）

- `.planning/codebase/ARCHITECTURE.md` §Multi-Tenancy Architecture — Plane workspace/project 层级、`WorkspaceMember` 角色（20/15/5）、数据隔离机制
- `.planning/codebase/INTEGRATIONS.md` §Authentication & Identity / Session Management — session/API key auth（Phase 1 已实现）

### Plane 源码（API 契约与数据模型依据 —— MUST 对照）

- `apps/api/plane/db/models/workspace.py` — Workspace/WorkspaceMember/WorkspaceMemberInvite 实体（slug/role/token 字段、软删除追加 epoch、ROLE_CHOICES）
- `apps/api/plane/app/urls/workspace.py` — workspace 级路由完整契约（CRUD/members/invitations/join/leave/slug-check 等）
- `apps/api/plane/api/urls/member.py` — member 子路由
- `apps/api/plane/api/urls/invite.py` — invitation 子路由
- `apps/api/plane/api/views/member.py` — member 视图实现
- `apps/api/plane/api/views/invite.py` — invitation 视图实现
- `apps/api/plane/api/serializers/member.py` — member 序列化器（响应字段依据）
- `apps/api/plane/api/serializers/invite.py` — invitation 序列化器
- `apps/api/plane/app/permissions/workspace.py` — workspace role 权限（20/15/5 能力矩阵）
- `apps/api/plane/utils/constants.py` — `RESTRICTED_WORKSPACE_SLUGS` 黑名单

### 模板参考（Fullstackhero）

- `D:/github/fullstackhero-dotnet-starter-kit/AGENTS.md` — 模板编码规范和 10 条黄金法则
- `D:/github/fullstackhero-dotnet-starter-kit/.agents/rules/` — 模板 Agent 规则目录

### YH.Flow 现有代码（集成点）

- `yh-flow/src/Modules/Identity/` — FSH Identity 模块（用户/角色/权限、`ICurrentUser`、Phase 1 三种认证 scheme）
- `yh-flow/src/Modules/Identity/Modules.Identity.Contracts/` — Identity Contracts（Users/Roles/Permissions DTO）— 扩展暴露 UserSummary/IUserIdentityService
- `yh-flow/src/BuildingBlocks/Persistence/` — BaseDbContext（多租户过滤、Schema 分离、审计字段、`IHasTenant`、软删除）
- `yh-flow/src/BuildingBlocks/Web/` — FSH 中间件/授权、异常处理、Plane 分页格式
- `yh-flow/src/Host/YH.Flow.DbMigrator/` — DbMigrator（注册新模块迁移）
- `yh-flow/src/Host/YH.Flow.Api/` — API Host（注册新模块、路由、中间件管道）

</canonical_refs>

<code_context>

## Existing Code Insights

### Reusable Assets

- **FSH Identity 模块** (`yh-flow/src/Modules/Identity/`) — 用户/角色/权限系统、`ICurrentUser`、Phase 1 三种认证 scheme。WorkspaceMember 经 Contracts 引用其用户
- **FSH BaseDbContext** (`yh-flow/src/BuildingBlocks/Persistence/`) — 多租户过滤（`IHasTenant`）、Schema 分离（`yhschema.Workspace`）、审计字段（created_by/updated_by/deleted_at）、软删除。Workspace 模块继承即可
- **FSH 认证/授权管线** — `RequirePermission`/`PermissionAuthorizationHandler` 模式可参考；workspace 用自定义 `[RequireWorkspaceRole]`（per-workspace 角色）
- **FSH 全局异常处理 + Plane 分页格式**（count/next/previous/results）— Phase 1 已适配，复用
- **FSH Caching BuildingBlock** — slug→workspaceId 解析缓存
- **FSH Mediator 3.x source-gen CQRS** — feature 切分贴 Identity 模块 `Features/v1` 模式

### Established Patterns

- **模块结构** — `{Module}.csproj` + `{Module}.Contracts.csproj`；Contracts 暴露接口/DTO 供跨模块引用
- **模块独立 Schema + 模块内管理迁移**（Phase 1 D-12~D-15）
- **多租户** — Finbuckle `IHasTenant` + 自动过滤（本 phase 用路由解析器解析 tenant=workspace）
- **API 兼容** — Plane 路由结构 `/api/v1/workspaces/{slug}/...`、响应/错误格式、分页
- **认证** — JWT + API Key + Session Cookie 三 scheme 自动协商（Phase 1）

### Integration Points

- **API Host Program.cs** — 注册 Workspace 模块（`AddWorkspaceModule`）、路由、中间件管道顺序（认证 → 租户解析 → 成员中间件 → 授权 → handler）
- **DbMigrator** — 注册 Workspace 模块 Mediator assemblies + module assemblies + 迁移
- **Identity.Contracts** — 扩展暴露 UserSummary/IUserIdentityService（跨模块用户引用）
- **BuildingBlocks/Persistence** — BaseDbContext 派生 `WorkspaceDbContext`
- **后续模块**（Project/WorkItems/...）— 全部依赖 `ICurrentWorkspaceContext`（D-03）+ `[RequireWorkspaceRole]`（D-11）+ `IHasTenant` 租户过滤

</code_context>

<specifics>
## Specific Ideas

- `WorkspaceMemberInvite` 实体字段贴 Plane：email / token(哈希) / role / accepted / responded_at / message
- 邀请 URL 形态：`{WEB_URL}/{slug}/invitation?token=...`（具体 URL 由前端/Phase 13 定，Phase 2 只产 token+slug）
- slug 受限词黑名单从 `apps/api/plane/utils/constants.py` 的 `RESTRICTED_WORKSPACE_SLUGS` 移植
- Workspace 软删除后 slug 形态 `{original}__{epoch_seconds}`（贴 Plane）
- 角色数值常量 Admin=20 / Member=15 / Guest=5（贴 Plane `ROLE_CHOICES`，与 ARCHITECTURE.md / domain-overview.md 一致）
- 工作区创建：自动为 owner 建 Admin `WorkspaceMember` + 设 OwnerId（D-06）

</specifics>

<deferred>
## Deferred Ideas

- **邀请邮件实际发送** → Phase 11 Notification（Hangfire + Mailkit）；Phase 2 预留 `INotificationService` 抽象
- **State/Label/Estimate 实体 CRUD** → Phase 4 WorkItems；REQ-2.3 "默认 states/labels/estimate 配置" 在 Phase 2 仅存 JSON 配置/偏好占位
- **公开端点 `/api/public/workspaces/{slug}/`**（Space app 无认证读）→ 规划阶段决定是否纳入 Phase 2 或延后到含 Space 的阶段
- **Plane workspace 边缘子资源**（workspace-themes / quick-links / stickies / home-preferences / sidebar-preferences / user-properties / draft-issues）→ 一期不做，后续按需评估
- **Team 实体**（Plane workspace.py 有 Team）→ 一期不实现
- **工作区封面图片（Unsplash 集成）** → Phase 3/9（REQ-3.3 / REQ-9.5）

</deferred>

---

_Phase: 02-workspace_
_Context gathered: 2026-06-17_
