# Phase 2: Workspace — 工作区 - Research

**Researched:** 2026-06-17
**Domain:** 多租户工作区管理（Finbuckle 路由租户解析 + EF Core 模块化迁移 + Plane API 契约迁移）
**Confidence:** HIGH（核心栈已通过官方文档 + 现有代码双重验证；仅 slugify 库选择 / TTL 默认值为 ASSUMED）

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions（D-01 ~ D-12，权威规格，规划阶段不可改）

**租户解析与隔离（架构级，影响后续 12 个 phase）：**

- **D-01** — Finbuckle `TenantId = WorkspaceId`。自定义 `ITenantResolver`/strategy 从 URL 段 `/api/v1/workspaces/{slug}/` 解析 slug → 查 workspace → 设 TenantId。所有子实体实现 `IHasTenant`，`BaseDbContext` 自动按 `WorkspaceId` 过滤。保留 Plane URL 契约 + 复用 Phase 1 已建的 FSH 自动过滤。
- **D-02** — resolver 只解析租户；成员资格 + 角色校验独立放 `WorkspaceMembershipMiddleware`/authz handler（填充 `ICurrentWorkspaceContext`）。公开端点 `/api/public/...` 可解析租户但不要求成员资格。
- **D-03** — `Workspace.Contracts` 暴露 `ICurrentWorkspaceContext`（封装 Finbuckle `IMultiTenantContextAccessor`，提供 `CurrentWorkspaceId/Slug/CurrentUserRole`）。下游模块依赖 Contracts 不直接耦合 Finbuckle。

**跨模块 User 引用：**

- **D-04** — Workspace 只存 `UserId`（Guid 标量，无跨模块 FK）；需用户详情时调 `Identity.Contracts` 的 `IUserIdentityService`。
- **D-05** — 同步批量查用户详情（list 成员 → 收集 UserIds → 一次批量查 Identity → 拼装响应）。
- **D-06** — Workspace 存 `OwnerId`（Guid 标量，无 FK）；创建时自动建一条 Admin 角色的 `WorkspaceMember`。

**Slug 生成与软删除兼容：**

- **D-07** — 混合 slug 来源：创建负载接受 slug（贴 Plane）；未提供时 `slugify(name)`；冲突时加短随机后缀（如 `acme-x7k2`）。
- **D-08** — 软删除时追加 `__{epoch_seconds}` 到 slug（释放原 slug）+ `deleted_at` 标记。
- **D-09** — 受限 slug 黑名单（移植 Plane `RESTRICTED_WORKSPACE_SLUGS`）+ 格式校验（小写/数字/连字符，max 48）。

**邀请机制与权限模型：**

- **D-10** — Phase 2 只生成 token + 邀请链接 + 持久化邀请记录 + accept/reject/revoke 逻辑，返回链接给调用方；实际邮件发送延后到 Phase 11（Hangfire + MailKit）。预留 `INotificationService` 抽象。
- **D-11** — 自定义 `[RequireWorkspaceRole(Admin|Member|Guest)]` 授权 requirement + handler，从 `ICurrentWorkspaceContext` 读当前 workspace 角色。Admin=全权限/Member=读写/Guest=只读。
- **D-12** — 邀请 token：随机安全（`CryptographicallyRandom`）+ 可配置 TTL（如 7 天）+ 接受/拒绝/撤销后失效；数据库存 SHA-256 哈希不明文，查询时哈希比对。

### Claude's Discretion（规划阶段可自由决定）

- `TenantId` 类型 = Guid（= workspace 的 Id）；slug → workspaceId 解析结果走 Redis 缓存（Phase 1 Caching BuildingBlock），workspace 更新时失效
- 解析失败/slug 不存在 → 404（不泄露存在性）；非成员/角色不足 → 403
- Workspace 实体本身**不**实现 `IHasTenant`（它 IS 租户）；其下所有实体实现
- 顶层端点（`GET/POST /workspaces/`、`/users/me/workspaces/invitations/`）路由无 slug 时 resolver 不设租户，走 user-scoped 逻辑
- `created_by/updated_by` 审计字段复用 Phase 1 的 `ICurrentUser`
- `Identity.Contracts` 暴露 `UserSummary` DTO（id/display_name/email/avatar_url）+ `IUserIdentityService.GetUsersByIdsAsync(IEnumerable<Guid>)`；尽量复用现有 `v1/Users` DTO
- slugify 库选择（Slugify/SlugBuilder 或自实现）+ 随机后缀长度/字母表；slug 唯一性竞态用 DB 唯一约束 + 冲突重试兜底
- role → 能力矩阵贴 Plane：Admin（CRUD 工作区/设置/成员/邀请）、Member（读写工作区内容，不能管成员/设置）、Guest（只读）；仅 owner 可删工作区（D-06）
- 邀请 token 哈希用 SHA-256；TTL 默认值与配置键；邀请接受端点接受时创建 `WorkspaceMember`（role 取邀请记录）
- `INotificationService` 抽象放 `Workspace.Contracts` 或共享 BuildingBlocks（规划阶段定）
- EF Core 实体配置（`IEntityTypeConfiguration`）组织方式、Contracts DTO 结构、Mediator feature 切分 —— 贴 Phase 1 Identity 模块 `Features/v1` 模式
- `WorkspaceDbContext` 边界 —— 只含 Workspace/Member/Invitation 等本模块实体

### Deferred Ideas（OUT OF SCOPE — 规划阶段完全忽略）

- 邀请邮件实际发送 → Phase 11 Notification
- State/Label/Estimate 实体 CRUD → Phase 4 WorkItems（REQ-2.3 在 Phase 2 仅 JSON 配置/偏好占位）
- 公开端点 `/api/public/workspaces/{slug}/` → 规划阶段决定是否纳入
- Plane workspace 边缘子资源（themes / quick-links / stickies / home-preferences / sidebar-preferences / user-properties / draft-issues）→ 一期不做
- Team 实体（Plane workspace.py 有 Team）→ 一期不实现
- 工作区封面图片（Unsplash 集成）→ Phase 3/9
  </user_constraints>

<phase_requirements>

## Phase Requirements

| ID      | Description                                            | Research Support                                                                                                                        |
| ------- | ------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------- |
| REQ-2.1 | 工作区 CRUD（含 slug 生成/软删除）                     | §Slug Generation, §Soft-Delete, §Standard Stack（EF Core + Finbuckle），§Code Examples（Workspace entity + DbContext），§Plane URL 契约 |
| REQ-2.2 | 成员管理（邀请/列表/更新角色/移除/加入离开）           | §Cross-Module User Reference（IUserIdentityService），§Plane WorkspaceMember 实体字段，§Role 能力矩阵（20/15/5）                        |
| REQ-2.3 | 工作区设置（名称/描述/Logo + 默认 Issue 配置占位）     | §Plane Workspace 实体字段（logo/timezone/organization_size/background_color），§Deferred（默认 state/label/estimate 实体 → Phase 4）    |
| REQ-2.4 | 邀请（生成链接/列待处理/撤销/接受拒绝）                | §Invitation Token，§Plane WorkspaceMemberInvite 实体字段，§Code Examples（token gen + SHA-256）                                         |
| NFR-1   | 性能（P95 < 200ms，分页 30/页，NoTracking/SplitQuery） | §Don't Hand-Roll（复用 Phase 1 Plane 分页），§Common Pitfalls（N+1）                                                                    |
| NFR-2   | 安全（强制认证/多租户隔离/输入验证/Rate Limiting）     | §Security Domain，§Pitfall（租户过滤绕过），§Don't Hand-Roll（FluentValidation）                                                        |
| NFR-3   | 可靠（软删除 + 60 天清理 + 全局异常 + Serilog）        | §Soft-Delete（epoch append），§现有 BaseDbContext 已有 ISoftDeletable filter                                                            |
| NFR-4   | 可维护（模块化单体 + NetArchTest + XML 注释）          | §Architecture Patterns（模块结构贴 Identity）                                                                                           |

</phase_requirements>

## Project Constraints (from CLAUDE.md)

- **Namespace 约定**：`YH.Framework.{Name}`（BuildingBlocks）、`YH.Modules.{Name}`（Modules）、`YH.Flow.{Name}`（Host）。新建 `YH.Modules.Workspace` + `YH.Modules.Workspace.Contracts`。
- **`TreatWarningsAsErrors`** — 警告即编译失败，所有新代码必须 0 warning。
- **`AGENTS.md` 是规范准则**（10 条黄金法则）—— 规划阶段须遵守。
- **Quick Commands**：`dotnet build src/YH.Flow.slnx`、`dotnet test src/YH.Flow.slnx`、API 端口 7030/5030。
- **MinimAPI + Vertical Slice** —— 所有端点用 Minimal API，feature 切分贴 Identity `Features/v1/{Feature}/{Endpoint.cs, Handler.cs, Validator.cs}` 模式。

## Summary

Phase 2 在 Phase 1 已就绪的 Finbuckle 多租户基座之上，落地 Plane 的"workspace 即租户"模型：把 `/api/v1/workspaces/{slug}/...` 路径段的 slug 解析成 workspace，并把它设为当前请求的 `TenantId`，让 `BaseDbContext` 的自动租户过滤接管所有子实体的隔离。这与 Phase 1 现有 `MultitenancyModule`（claim/header/query-string strategy + `EFCoreStore<TenantDbContext, AppTenantInfo>`）存在**两套租户模型并存**的设计张力 —— 详见 §Architecture Pattern 1 的并存策略（**不是冲突，是分层**：FSH 原 tenant 管平台级订阅/quota，workspace tenant 管 Plane 业务隔离）。

技术栈 100% 复用现有：Finbuckle 10.1.1（已锁定在 `Directory.Packages.props`）、EF Core、`BaseDbContext`/`MultiTenantDbContext`、Mediator 3.x source-gen CQRS、FluentValidation。**无需新增任何 NuGet 包**（slugify 自实现 ~20 行，crypto 用 BCL `RandomNumberGenerator` + `SHA256`，与 `ApiTokenService` 完全一致）。

**Primary recommendation：** 采用 Finbuckle `WithDelegateStrategy<HttpContext, AppTenantInfo>` 实现 D-01 的 slug 解析（首个非 null 标识符胜出，排在现有 claim/header 策略之前）；用一个**轻量 `IWorkspaceTenantStore : IMultiTenantStore<AppTenantInfo>`** 把 slug→workspace 解析结果包装成 `AppTenantInfo(Id=workspaceGuid, Identifier=slug)`，复用 Phase 1 的 `WithDistributedCacheStore` 做 slug→tenant 缓存。成员资格/角色校验用独立的 `WorkspaceMembershipMiddleware` 填 `ICurrentWorkspaceContext`，`[RequireWorkspaceRole]` 走标准 `IAuthorizationHandler`。**这是与现有代码零冲突、最小惊讶的实现路径。**

## Architectural Responsibility Map

| Capability                       | Primary Tier                                        | Secondary Tier           | Rationale                                                                                                         |
| -------------------------------- | --------------------------------------------------- | ------------------------ | ----------------------------------------------------------------------------------------------------------------- |
| URL slug → TenantId 解析（D-01） | API / Middleware（Finbuckle `UseMultiTenant`）      | —                        | 必须在认证之后、DbContext 构造之前完成，否则 `BaseDbContext` 拿不到 `MultiTenantContext`                          |
| 成员资格 + 角色填充（D-02）      | API / Middleware（`WorkspaceMembershipMiddleware`） | Authz handler            | 解析完 tenant 后立即查 `WorkspaceMember` 表填 `ICurrentWorkspaceContext`；authz handler 只读上下文不查 DB         |
| Workspace-role 授权（D-11）      | API / Authorization（`[RequireWorkspaceRole]`）     | —                        | 标准 `IAuthorizationHandler`，从 `ICurrentWorkspaceContext.CurrentUserRole` 读，不重复查 DB                       |
| Workspace CRUD 业务逻辑          | API / Backend（Mediator handler）                   | Database                 | Vertical slice，handler 在 workspace-scoped `WorkspaceDbContext` 上操作                                           |
| Slug 生成 + 受限词校验           | API / Backend（domain service）                     | Database（唯一约束兜底） | 服务层做 slugify + 黑名单 + 格式校验，DB 唯一索引兜底竞态                                                         |
| 软删除（slug epoch 释放）        | Database / EF（override `SaveChanges` 或 handler）  | —                        | 贴 Plane：delete 时同步把 slug 改成 `{slug}__{epoch}`，让原 slug 可复用                                           |
| 邀请 token 生成/校验             | API / Backend（domain service）                     | Database（存 hash）      | BCL `RandomNumberGenerator` 生成，`SHA256.HashData` 落库，与 `ApiTokenService` 一致                               |
| 跨模块用户详情批量解析           | API / Backend（`IUserIdentityService`）             | Identity.Contracts       | Workspace 不建 FK，调 Identity.Contracts 一次批量查，避免 N+1                                                     |
| 跨模块数据隔离                   | Database（`BaseDbContext` 自动 `IHasTenant` 过滤）  | —                        | FSH 已建：实体实现 `IHasTenant` → `ApplyTenantIsolationByDefault()` 自动加 filter；**默认开启、漏加实体不会泄露** |
| Slug→workspaceId 缓存            | Caching（Redis via Phase 1 `AddHeroCaching`）       | —                        | workspace 更新/删除时失效缓存键                                                                                   |

## Standard Stack

### Core（全部已在项目中，0 新增包）

| Library                                                         | Version                                   | Purpose                                                            | Why Standard                                                                                                    |
| --------------------------------------------------------------- | ----------------------------------------- | ------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------- |
| `Finbuckle.MultiTenant`                                         | 10.1.0（`Directory.Packages.props` 锁定） | 租户解析、strategy 链、store、`IMultiTenantContextAccessor`        | 项目唯一多租户库；docs v10.1.1 与项目版本对齐 [CITED: finbuckle.com/MultiTenant/Docs/v10.1.1]                   |
| `Finbuckle.MultiTenant.AspNetCore`                              | 10.1.0                                    | `WithDelegateStrategy<HttpContext,...>`、`UseMultiTenant()` 中间件 | 提供 HttpContext-typed strategy，是 D-01 的实现载体 [VERIFIED: Directory.Packages.props + 官方 Strategies 文档] |
| `Finbuckle.MultiTenant.EntityFrameworkCore`                     | 10.1.0                                    | `MultiTenantDbContext`、`IsMultiTenant()`、`AdjustUniqueIndexes()` | `BaseDbContext` 已继承；自动 filter + 唯一索引补 tenant 列 [VERIFIED: BaseDbContext.cs]                         |
| `Microsoft.EntityFrameworkCore.*`                               | .NET 10 内置                              | `WorkspaceDbContext`、迁移、`IEntityTypeConfiguration`             | 全项目统一 EF Core                                                                                              |
| `Mediator`（source-gen）                                        | 现有                                      | CQRS command/query + handler + validator pipeline                  | Identity 模块已用，贴其 `Features/v1` 结构 [VERIFIED: IdentityModule.cs]                                        |
| `FluentValidation`                                              | 现有                                      | DTO 校验，NFR-2 要求                                               | `AddValidatorsFromAssemblies` 已在 `AddModules` 自动注册 [VERIFIED: ModuleLoader.cs]                            |
| `System.Security.Cryptography.RandomNumberGenerator` / `SHA256` | BCL                                       | 邀请 token 生成 + 哈希（D-12）                                     | `ApiTokenService.GenerateRawKey/HashToken` 已用同样模式 [VERIFIED: ApiTokenService.cs:177-185]                  |

### Supporting

| Library                              | Version | Purpose                                              | When to Use                                |
| ------------------------------------ | ------- | ---------------------------------------------------- | ------------------------------------------ |
| Redis（Phase 1 `AddHeroCaching`）    | 现有    | slug→workspaceId 缓存                                | D-01 slug 解析高频，缓存命中避免每次 DB 查 |
| `Microsoft.AspNetCore.Authorization` | BCL     | `[RequireWorkspaceRole]` requirement + handler       | D-11                                       |
| `IEntityTypeConfiguration<T>`        | EF Core | 实体映射（贴 Identity `APITokenConfiguration` 模式） | 所有实体                                   |

### Alternatives Considered

| Instead of                                                         | Could Use                                                                                      | Tradeoff（为什么不选）                                                                                                                                                                                                                                |
| ------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Finbuckle `WithDelegateStrategy`                                   | 自己写 middleware 直接设 `IMultiTenantContextSetter`                                           | Finbuckle strategy 链与 `UseMultiTenant()` 一体，自动填充 `IMultiTenantContextAccessor`（`BaseDbContext` 依赖）；手写要绕开框架、易漏。**D-01 明确要求用 Finbuckle resolver，故选 strategy**                                                          |
| 自实现 slugify（20 行）                                            | NuGet `Slugify` 0.1.0（已确认存在于 NuGet） [VERIFIED: api.nuget.org/v3-flatcontainer/slugify] | Plane slug 规则极简（小写+数字+连字符），引入第三方包不值得；项目惯例是 BCL 优先（见 ApiTokenService）。slugify 包仅 3 个版本、维护稀疏，[ASSUMED] 生产成熟度未知                                                                                     |
| 单独 slug → tenant 中间件                                          | 把 slug 解析塞进现有 `ClaimStrategy`/`HeaderStrategy`                                          | D-01 要求从 URL 路径段解析，claim/header 拿不到 slug；strategy 链才是正确分层                                                                                                                                                                         |
| `EFCoreStore<WorkspaceDbContext, AppTenantInfo>` 作为 tenant store | 自定义 `IWorkspaceTenantStore` 实现 `IMultiTenantStore<AppTenantInfo>`                         | Phase 1 已注册 `EFCoreStore<TenantDbContext, AppTenantInfo>` 给平台级 tenant；再注册一个 EFCoreStore 会两套 store 互相尝试解析（Finbuckle 多 store 按顺序遍历）。**用 `IMultiTenantStore` 自定义实现**显式只认 slug 标识符，避免歧义（见 §Pattern 1） |

**Installation：**

```bash
# 无需安装任何新包 —— 全部依赖已在 Directory.Packages.props 锁定
dotnet restore src/YH.Flow.slnx
```

**Version verification（已在 `Directory.Packages.props` 确认）：**

```
Finbuckle.MultiTenant                      10.1.0
Finbuckle.MultiTenant.AspNetCore           10.1.0
Finbuckle.MultiTenant.EntityFrameworkCore  10.1.0
```

## Package Legitimacy Audit

> Phase 2 **不安装任何新外部包**（slugify/crypto/Redis 全部已在项目内）。slopcheck 检查无新增包可审。

| Package    | Registry | Age | Downloads | Source Repo | slopcheck | Disposition |
| ---------- | -------- | --- | --------- | ----------- | --------- | ----------- |
| （无新增） | —        | —   | —         | —           | N/A       | N/A         |

**Packages removed due to slopcheck [SLOP] verdict:** none
**Packages flagged as suspicious [SUS]:** none

_Phase 2 纯代码 + 配置变更，复用 Phase 1 已锁定的全部依赖，无需 planner 插入 `checkpoint:human-verify`。_

## Architecture Patterns

### System Architecture Diagram

```
HTTP Request: /api/v1/workspaces/{slug}/members/
   │
   ▼
[1] ASP.NET Core Pipeline
   │
   ├─ UseRouting()                          ← Minimal API route 匹配 {slug} 段
   │
   ├─ UseMultiTenant()  (Finbuckle)         ← D-01 入口
   │     │
   │     ├─ Strategy 链（注册顺序 = 优先级）：
   │     │   ① WithDelegateStrategy<HttpContext>   【新增】从 route value 取 slug
   │     │      → 返回 slug 字符串作为 identifier
   │     │   ② WithClaimStrategy (existing)        【Phase 1】平台级 root/subscription tenant
   │     │   ③ WithHeaderStrategy (existing)
   │     │   ④ WithDelegateStrategy (existing)     query-string ?tenant=
   │     │   首个非 null 胜出，其余跳过
   │     │
   │     └─ Store 链（按 identifier 解析 AppTenantInfo）：
   │         ① IWorkspaceTenantStore 【新增】 slug→查 WorkspaceDbContext→
   │              返回 AppTenantInfo(Id=workspaceGuid, Identifier=slug, Name=name)
   │              （slug 标识符才命中；其他标识符返回 null 让下个 store 接）
   │         ② DistributedCacheStore (existing)    Redis 缓存解析结果（60min 滑动）
   │         ③ EFCoreStore<TenantDbContext, AppTenantInfo> (existing)
   │              平台级 tenant store（subscription/quota 用）
   │
   │   → IMultiTenantContextAccessor.MultiTenantContext.TenantInfo 被填充
   │   → BaseDbContext 构造时拿到 TenantId = workspaceGuid
   │
   ├─ UseAuthentication() (existing)        ← Phase 1: JWT / API Key / Session Cookie
   │
   ├─ UseHeroPlatform → CurrentUserMiddleware (existing)  ← 填 ICurrentUser
   │
   ├─ UseAuthorization()                    ← ASP.NET Core 标准授权
   │
   ├─ WorkspaceMembershipMiddleware 【新增】 ← D-02
   │     读 IMultiTenantContextAccessor.TenantInfo.Id (=workspaceGuid)
   │     + ICurrentUser.GetUserId()
   │     → 查 WorkspaceMember(workspaceId, userId, is_active=true)
   │     → 填 ICurrentWorkspaceContext (CurrentWorkspaceId/Slug/CurrentUserRole)
   │     （未找到 → 不填 role，后续 [RequireWorkspaceRole] handler 返回 403）
   │
   └─ Endpoint Handler（Mediator command/query）
         读 ICurrentWorkspaceContext → 自动 tenant-scoped WorkspaceDbContext
         → 查 WorkspaceMember/Invitation 等（auto-filter by WorkspaceId）
         → 返回响应（Plane 格式：count/next/previous/results for list）
```

### Recommended Project Structure（贴 Identity 模块结构 [VERIFIED]）

```
yh-flow/src/Modules/Workspace/
├── Modules.Workspace/
│   ├── WorkspaceModule.cs                  # IModule 实现（ConfigureServices/ConfigureMiddleware/MapEndpoints）
│   ├── WorkspaceModuleConstants.cs         # SchemaName = "yhschema.Workspace"，ApiPrefix = "workspaces"
│   ├── Authorization/
│   │   ├── RequireWorkspaceRoleAttribute.cs      # [RequireWorkspaceRole(Admin|Member|Guest)] + IAuthorizationRequirement 元数据
│   │   ├── RequireWorkspaceRoleAuthorizationHandler.cs
│   │   └── WorkspaceRole.cs                      # 常量 Admin=20/Member=15/Guest=5
│   ├── Middleware/
│   │   └── WorkspaceMembershipMiddleware.cs       # D-02 填 ICurrentWorkspaceContext
│   ├── MultiTenancy/
│   │   ├── WorkspaceSlugStrategy.cs              # 【可选用 DelegateStrategy 内联】或独立 IMultiTenantStrategy
│   │   ├── WorkspaceTenantStore.cs               # IMultiTenantStore<AppTenantInfo>，slug→workspace
│   │   └── ICurrentWorkspaceContext.cs           # 实现放 Contracts
│   ├── Data/
│   │   ├── WorkspaceDbContext.cs                 # 派生 BaseDbContext（贴 AuditDbContext 模式）
│   │   ├── Configurations/
│   │   │   ├── WorkspaceConfiguration.cs         # 注：Workspace 实体 IsGlobalEntity 或显式不 IsMultiTenant
│   │   │   ├── WorkspaceMemberConfiguration.cs
│   │   │   └── WorkspaceInvitationConfiguration.cs
│   │   └── Migrations/                           # 模块内管理（D-13）
│   ├── Domain/
│   │   ├── Workspace.cs                          # ISoftDeletable, IGlobalEntity（它 IS 租户）
│   │   ├── WorkspaceMember.cs                    # IHasTenant, ISoftDeletable
│   │   └── WorkspaceInvitation.cs                # IHasTenant, ISoftDeletable
│   ├── Services/
│   │   ├── ISlugGenerator.cs / SlugGenerator.cs  # slugify + 受限词 + 冲突重试
│   │   ├── IInvitationTokenService.cs / InvitationTokenService.cs  # RandomNumberGenerator + SHA256
│   │   └── WorkspaceMembershipService.cs         # 成员 CRUD、role 变更
│   └── Features/v1/
│       ├── Workspaces/                           # CRUD + slug-check（顶层路由，无 slug）
│       │   ├── CreateWorkspace/{CreateWorkspaceCommand,Handler,Validator,Endpoint}.cs
│       │   ├── GetWorkspace/
│       │   ├── UpdateWorkspace/
│       │   ├── DeleteWorkspace/
│       │   ├── ListUserWorkspaces/
│       │   └── CheckWorkspaceSlug/
│       ├── Members/                              # /workspaces/{slug}/members/
│       │   ├── ListMembers/                      # 批量调 IUserIdentityService 拼装
│       │   ├── UpdateMemberRole/
│       │   └── RemoveMember/
│       └── Invitations/                          # /workspaces/{slug}/invitations/ + /users/me/workspaces/invitations/
│           ├── CreateInvitation/
│           ├── ListInvitations/
│           ├── RevokeInvitation/
│           ├── AcceptInvitation/
│           └── RejectInvitation/
└── Modules.Workspace.Contracts/
    ├── ICurrentWorkspaceContext.cs               # D-03
    ├── IWorkspaceTenantResolver.cs               # 给下游模块的可选 facade
    ├── INotificationService.cs                   # D-10 预留（Phase 11 实现）
    ├── DTOs/
    │   ├── WorkspaceDto.cs                       # id/slug/name/logo/timezone/owner/...
    │   ├── WorkspaceMemberDto.cs                 # id/role/user(UserSummary)/is_active
    │   └── WorkspaceInvitationDto.cs             # id/email/role/accepted/responded_at
    ├── Constants/
    │   └── RestrictedSlugs.cs                    # 移植 Plane RESTRICTED_WORKSPACE_SLUGS（66 项）
    └── v1/Users/                                 # 跨模块 batch 解析契约（D-04/D-05）
        └── IUserIdentityService.cs               # 【放在 Identity.Contracts，见下文】
```

> **注意**：`UserSummary` + `IUserIdentityService.GetUsersByIdsAsync` 应放 **`Identity.Contracts`**（不是 Workspace.Contracts），因为 Workspace 调它、其它 11 个模块也调它，是 Identity 暴露的能力。但 `ICurrentWorkspaceContext` 放 **`Workspace.Contracts`**（D-03），下游模块依赖它。

### Pattern 1: 双租户模型并存（关键架构决策）

**What:** Phase 1 已有的 `MultitenancyModule`（claim/header/query strategy + `EFCoreStore<TenantDbContext, AppTenantInfo>`）服务 FSH 的**平台级 tenant**（订阅、quota、过期、计费）。Phase 2 新增的 workspace slug resolver 服务 Plane 的**业务级 tenant**（数据隔离、成员角色）。

**为什么不是冲突：** Finbuckle 的 strategy 链是"首个非 null 标识符胜出"，store 链是"按注册顺序遍历到首个匹配"。两者通过**标识符来源 + store 命中条件**自然分层：

| 来源                                                    | 标识符                          | 命中的 store                                     | 填充的 TenantInfo 含义                   |
| ------------------------------------------------------- | ------------------------------- | ------------------------------------------------ | ---------------------------------------- |
| `/api/v1/workspaces/{slug}/...` URL 段                  | `slug`（如 "acme"）             | `WorkspaceTenantStore`                           | workspaceGuid = 当前工作区，用于业务隔离 |
| JWT `__tenant__` claim / `X-Tenant` header / `?tenant=` | 平台 tenant identifier          | `EFCoreStore<TenantDbContext>`                   | 平台 tenant（root/订阅/quota）           |
| 顶层端点 `/api/v1/workspaces/`（无 slug）               | null（slug strategy 返回 null） | 跳过 workspace store，由 claim/header 接管或留空 | user-scoped，不强制 tenant               |

**实现要点：**

1. `WorkspaceSlugStrategy`（或 `WithDelegateStrategy<HttpContext>`）**必须排在 strategy 链第一位**，且仅当 route value 含 `slug` key 且路径匹配 `/api/v1/workspaces/...` 时返回 slug；否则返回 null，让现有 claim/header strategy 接管（保持 Phase 1 行为不变）。
2. `WorkspaceTenantStore` 仅当标识符**形如 slug 且能在 `WorkspaceDbContext.Workspaces` 找到**时返回 `AppTenantInfo`；否则返回 null（让 `EFCoreStore<TenantDbContext>` 继续尝试解析为平台 tenant）。**不能**对所有标识符都返回 workspace，否则会破坏 Phase 1 的 root operator header override（`MultitenancyModule.cs:129`）。
3. **不要重复调用 `AddMultiTenant<AppTenantInfo>`** —— 在 `WorkspaceModule.ConfigureServices` 里通过 `services.Configure<...>` 或 `services.AddTransient<IMultiTenantStrategy<AppTenantInfo>, WorkspaceSlugStrategy>()` 追加注册；若必须改 builder 配置，需重构 `MultitenancyModule` 暴露一个 hook，或在 `MultitenancyModule` 里直接加 workspace strategy（更干净，但跨模块耦合）。**推荐方案：在 `WorkspaceModule` 用 `services.Insert/Configure` 注册 strategy + store，让 Finbuckle DI 容器自然合并** —— 但要先验证 Finbuckle 是否允许在 `AddMultiTenant` 之外追加。**若不行，回退方案：在 `MultitenancyModule` 里加 workspace strategy/store 的注册（Workspace.Contracts 暴露静态注册扩展方法供 MultitenancyModule 调用）。** [ASSUMED：Finbuckle 10.1.1 是否允许 builder 外追加 strategy —— 需 Wave 0 spike 验证，见 §Open Questions Q1]

**Anti-Pattern：** 重新写一遍 `AddMultiTenant<AppTenantInfo>(...)` 会覆盖 Phase 1 的整个配置（claim/header/query strategy 全丢），导致 quota/billing 模块崩。**绝对禁止。**

### Pattern 2: `WorkspaceDbContext` 派生（贴 AuditDbContext 模式 [VERIFIED: AuditDbContext.cs]）

```csharp
// Source: yh-flow/src/Modules/Auditing/Modules.Auditing/Persistence/AuditDbContext.cs（贴此结构）
namespace YH.Modules.Workspace.Data;

public sealed class WorkspaceDbContext : BaseDbContext
{
    public WorkspaceDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<WorkspaceDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    public DbSet<Domain.Workspace> Workspaces => Set<Domain.Workspace>();
    public DbSet<WorkspaceMember> Members => Set<WorkspaceMember>();
    public DbSet<WorkspaceInvitation> Invitations => Set<WorkspaceInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        // 1. 先 ApplyConfigurations，让 per-entity 配置就位（AdjustUniqueIndexes 依赖它）
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkspaceDbContext).Assembly);
        // 2. base.OnModelCreating 跑在最后，自动给非 IGlobalEntity 实体加 IsMultiTenant + 软删除 filter
        base.OnModelCreating(modelBuilder);
    }
}
```

**关键：`Workspace` 实体本身**必须**实现 `IGlobalEntity`（不 IsMultiTenant）—— 因为它 IS 租户，不能自己过滤自己。`WorkspaceMember` / `WorkspaceInvitation` 不实现 `IGlobalEntity`，会被 `ApplyTenantIsolationByDefault()` 自动加 `TenantId` 列 + query filter。** [VERIFIED: TenantIsolationExtensions.cs + BaseDbContext.OnModelCreating]

### Pattern 3: `[RequireWorkspaceRole]` 授权（贴 `RequiredPermissionAuthorizationHandler` 模式 [VERIFIED]）

```csharp
// Source: 模仿 yh-flow/src/Modules/Identity/Modules.Identity/Authorization/RequiredPermissionAuthorizationHandler.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireWorkspaceRoleAttribute : Attribute, IAuthorizationRequirement
{
    public WorkspaceRole[] Roles { get; }
    public RequireWorkspaceRoleAttribute(params WorkspaceRole[] roles) => Roles = roles;
}

public sealed class RequireWorkspaceRoleAuthorizationHandler(
    ICurrentWorkspaceContext workspaceContext)  // 从 middleware 填的上下文读
    : AuthorizationHandler<RequireWorkspaceRoleAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RequireWorkspaceRoleAttribute requirement)
    {
        var role = workspaceContext.CurrentUserRole;
        if (role is null) { context.Fail(); return Task.CompletedTask; }
        if (requirement.Roles.Contains(role.Value))
            context.Succeed(requirement);
        else
            context.Fail();
        return Task.CompletedTask;
    }
}
```

端点用法：

```csharp
group.MapPost("/", async (...) => {...})
     .RequireAuthorization(new RequireWorkspaceRoleAttribute(WorkspaceRole.Admin));
// 或在 endpoint filter 里手动检查（贴 Identity 的 .RequirePermission() 扩展方法模式）
```

### Anti-Patterns to Avoid

- **在 `WorkspaceDbContext.OnModelCreating` 里 base 调用顺序颠倒**：必须先 `ApplyConfigurationsFromAssembly` 再 `base.OnModelCreating`，否则 `ApplyTenantIsolationByDefault()` 看不到配置好的唯一索引，`AdjustUniqueIndexes()` 失败。AuditDbContext 注释已明确警告。
- **把 slug → tenant 解析放认证之前**：Phase 1 注释（`MultitenancyModule.cs:96`）明确 `UseMultiTenant()` 在 `UseAuthentication()` 之前跑，User 是 anonymous —— 所以 slug strategy **不能依赖 User**。成员资格校验必须在认证之后的 middleware 做（D-02）。
- **给 Workspace 实体加 `IHasTenant`/`IsMultiTenant()`**：它会自己过滤自己（filter `WHERE TenantId = self.Id`），导致永远查不到。Workspace 是 `IGlobalEntity`。
- **跨模块建 FK**：`WorkspaceMember.UserId` 不能有 `HasOne<FshUser>()` FK（D-04），EF 会要求 DbContext 同时 know FshUser，破坏模块边界。只配 `HasMaxLength(450)` 标量 + 索引。
- **重复 `AddMultiTenant<AppTenantInfo>`**：覆盖 Phase 1 配置（见 Pattern 1 Anti-Pattern）。
- **slug 唯一性只靠应用层 check-then-insert**：竞态必中。必须 DB 唯一索引 + insert 冲突重试（`catch (UniqueConstraintException) retry with new suffix`）。

## Don't Hand-Roll

| Problem                                      | Don't Build                          | Use Instead                                                                   | Why                                                              |
| -------------------------------------------- | ------------------------------------ | ----------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 多租户 query filter                          | 自己写 `WHERE WorkspaceId = current` | `BaseDbContext` + `IHasTenant` + `ApplyTenantIsolationByDefault()`            | 已实现、默认开启、漏加实体也安全 [VERIFIED]                      |
| 软删除 query filter                          | 手动 `if (!deleted)`                 | `ISoftDeletable` + `AppendGlobalQueryFilter(SoftDelete, ...)`                 | `BaseDbContext.OnModelCreating` 已挂 [VERIFIED]                  |
| 审计字段（created_by/updated_by/deleted_at） | 手动赋值                             | `AuditableEntitySaveChangesInterceptor`（已注册为 `ISaveChangesInterceptor`） | `PersistenceExtensions.AddHeroDatabaseOptions` 已注册 [VERIFIED] |
| Domain event 派发                            | 手动 publish                         | `DomainEventsInterceptor`                                                     | 已注册 [VERIFIED]                                                |
| 分页（count/next/previous/results）          | 自己拼响应                           | `PlanePagedResult<T>` + `IPagedQuery`                                         | Phase 1 已适配 Plane 格式 [VERIFIED: PlanePagedResult.cs]        |
| 全局异常 → Plane 错误格式                    | 自己 try/catch                       | `GlobalExceptionHandler`                                                      | 已注册 [VERIFIED: Extensions.cs]                                 |
| 校验 pipeline                                | 手动 if/throw                        | `FluentValidation` + `ValidationBehavior` pipeline                            | `AddModules` 自动 `AddValidatorsFromAssemblies` [VERIFIED]       |
| Token 随机生成                               | `Random` / `Guid.NewGuid()`          | `RandomNumberGenerator.GetBytes(32)`                                          | 密码学安全；`ApiTokenService` 已用 [VERIFIED]                    |
| Token 哈希                                   | 自定义算法                           | `SHA256.HashData` + hex lower                                                 | `ApiTokenService.HashToken` 已用 [VERIFIED]                      |
| Rate Limiting                                | 自己写                               | FSH 四层（已在 Phase 1 配置）                                                 | NFR-2 要求                                                       |
| 当前用户上下文                               | 从 ClaimsPrincipal 手动取            | `ICurrentUser`（Phase 1）                                                     | `CurrentUserMiddleware` 已填 [VERIFIED]                          |

**Key insight：** Phase 2 的本质是"配置 + 粘合"，不是"造轮子"。FSH 模板 + Phase 1 已经把多租户、审计、校验、异常、分页、限流、缓存全部建好，Phase 2 只需：(1) 新增 workspace strategy/store 接入 Finbuckle；(2) 新增 3 张表 + DbContext；(3) 写业务 feature slice；(4) 写 workspace-role authz。**0 新依赖、0 重新发明。**

## Runtime State Inventory

> Phase 2 是**新建模块（greenfield within existing solution）**，不涉及 rename/refactor/migration of existing strings。无运行时状态需要迁移。**N/A — verified by absence of rename/refactor scope in CONTEXT.md `<domain>` and `<deferred>`。**

## Common Pitfalls

### Pitfall 1: 两套 Finbuckle `AddMultiTenant` 注册互相覆盖

**What goes wrong:** 在 `WorkspaceModule.ConfigureServices` 里调用 `services.AddMultiTenant<AppTenantInfo>(...)` 试图追加 workspace strategy，覆盖了 Phase 1 `MultitenancyModule` 的全部 strategy/store 配置，导致 quota/billing 模块拿不到平台 tenant。
**Why it happens:** `AddMultiTenant` 返回 builder，第二次调用会重置 builder 状态。
**How to avoid:** **不要**再调 `AddMultiTenant`。改为：(a) 在 `WorkspaceModule` 用 `services.TryAddEnumerable(ServiceDescriptor.Singleton<IMultiTenantStrategy<AppTenantInfo>, WorkspaceSlugStrategy>())` 注册 strategy；(b) store 同理用 `IMultiTenantStore<AppTenantInfo>`。若 Finbuckle DI 不支持外部追加（见 Open Question Q1），则把 workspace strategy/store 的**注册扩展方法**暴露在 `Workspace.Contracts`，由 `MultitenancyModule` 调用（保持单一 `AddMultiTenant` 入口）。
**Warning signs:** Phase 1 quota endpoint 突然返回 "tenant not found"；root operator header override 失效。

### Pitfall 2: slug strategy 在 `/api/v1/workspaces/`（无 slug 顶层端点）误解析

**What goes wrong:** `WithDelegateStrategy<HttpContext>` 实现不当，对 `POST /api/v1/workspaces/`（无 slug）也返回了某个值，导致创建工作区请求被错误地 tenant-scoped，handler 拿到一个不存在的 TenantId。
**Why it happens:** 没检查 route value 是否真有 `slug` key。
**How to avoid:** strategy lambda 严格检查 `httpContext.GetRouteValue("slug") is string slug && !string.IsNullOrEmpty(slug)`，否则 return null。顶层端点（list/create/check-slug）走 user-scoped，不设 tenant。
**Warning signs:** `POST /workspaces/` 创建的工作区被绑定到错误 workspace。

### Pitfall 3: N+1 查询（list members）

**What goes wrong:** `GET /workspaces/{slug}/members/` 每个成员单独查 Identity 取 email/display_name，30 个成员 = 31 次查询，P95 远超 200ms（违反 NFR-1）。
**Why it happens:** handler 循环里调 `IUserIdentityService.GetAsync(userId)`。
**How to avoid:** D-05：handler 收集所有 `member.UserId` → 一次 `IUserIdentityService.GetUsersByIdsAsync(userIds)` → 拼 DTO。配合 EF `AsNoTracking()`。
**Warning signs:** list 接口 P95 > 200ms；EF Core 日志显示重复 SQL。

### Pitfall 4: 软删除后 slug 未释放，新工作区创建失败

**What goes wrong:** 删除 workspace A（slug="acme"），想新建 workspace B 用 "acme" 失败 —— A 的 slug 还在 DB 唯一索引里。
**Why it happens:** 没在软删除时执行 D-08 的 slug epoch 追加。
**How to avoid:** 在 delete handler 里同步：`workspace.IsDeleted = true; workspace.DeletedOnUtc = now; workspace.Slug = $"{workspace.Slug}__{(int)now.UnixTimeSeconds}";` 然后 SaveChanges。**或**用 EF `ISaveChangesInterceptor` 的子类统一处理（但 workspace 特有逻辑，建议放 handler 显式做，更易测）。
**Warning signs:** 软删除后无法用原 slug 创建新 workspace。

### Pitfall 5: 邀请 token 用明文存储 / 不失效

**What goes wrong:** DB 存明文 token，或 accept/revoke 后仍可用。
**Why it happens:** 图省事没哈希；状态机漏改 accepted 标志。
**How to avoid:** D-12：`SHA256.HashData(tokenBytes)` → hex lower 存；查询时先 hash 再比对。accept/reject/revoke 都置 `Accepted/RespondedAt` 并在 accept 端点校验 `Accepted == false && RespondedAt == null`。
**Warning signs:** token 泄露后无法撤销；DB dump 暴露可邀用的 token。

### Pitfall 6: `Workspace` 实体被误加 `IHasTenant`

**What goes wrong:** `Workspace` 实体自动 `IsMultiTenant()`，`SELECT` 加 `WHERE Id == Id` 永远返回 0 行，所有 workspace 查询空。
**Why it happens:** `ApplyTenantIsolationByDefault()` 默认对所有非 `IGlobalEntity` 实体加。
**How to avoid:** `Workspace : IGlobalEntity`（明确标注它是跨租户的全局实体，自身就是租户定义）。
**Warning signs:** workspace 列表/详情永远空。

## Code Examples

### Example 1: Slug → Tenant 解析（D-01 核心）

```csharp
// Source: 模式来自 Finbuckle v10.1.1 Strategies 文档 + 项目 MultitenancyModule.cs:100-109
// 注册（在 WorkspaceModule.ConfigureServices 或 MultitenancyModule 内）：
services.AddMultiTenant<AppTenantInfo>(/* existing options */)
    .WithDelegateStrategy<HttpContext, AppTenantInfo>(async httpContext =>
    {
        // 仅当路由匹配 /api/v1/workspaces/{slug}/... 且 slug 段存在
        if (httpContext.GetRouteValue("slug") is string slug &&
            !string.IsNullOrEmpty(slug))
        {
            return await Task.FromResult<string?>(slug);
        }
        return null;  // 顶层端点 / 路由无 slug → 让 claim/header strategy 接管
    })
    // ... existing claim/header/query strategies ...
    ;
```

### Example 2: Slug → Workspace 解析 store

```csharp
// Source: 模式来自 Finbuckle v10.1.1 Stores 文档（IMultiTenantStore<TTenantInfo>）
public sealed class WorkspaceTenantStore(
    WorkspaceDbContext db,                       // 用专门的 DbContext 查 workspace（无 tenant filter）
    IDistributedCache cache)                     // Redis 缓存（Phase 1 AddHeroCaching）
    : IMultiTenantStore<AppTenantInfo>
{
    public async Task<AppTenantInfo?> TryGetAsync(string identifier)  // identifier = slug
    {
        var cacheKey = $"ws:slug:{identifier}";
        var cached = await cache.GetStringAsync(cacheKey);
        if (cached is not null) return JsonSerializer.Deserialize<AppTenantInfo>(cached);

        // 注意：Workspace 是 IGlobalEntity，不受 tenant filter 影响
        var ws = await db.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Slug == identifier && !w.IsDeleted);
        if (ws is null) return null;  // null → 让下个 store（EFCoreStore<TenantDbContext>）尝试

        var info = new AppTenantInfo(id: ws.Id.ToString(), identifier: ws.Slug, name: ws.Name);
        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(info),
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) });
        return info;
    }
    // ... AddAsync/UpdateAsync/RemoveAsync/GetAllAsync 转发到 cache invalidate
}
```

### Example 3: 邀请 token 生成 + 哈希（D-12，贴 ApiTokenService [VERIFIED]）

```csharp
// Source: yh-flow/src/Modules/Identity/Modules.Identity/Services/ApiTokenService.cs:177-185
private const int TokenByteLength = 32;

public static (string RawToken, string Hash) GenerateInvitationToken()
{
    var bytes = RandomNumberGenerator.GetBytes(TokenByteLength);
    var raw = Convert.ToHexString(bytes).ToLowerInvariant();  // 返回给调用方 / 拼 URL，不落库
    var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
    return (raw, hash);
}

public static string HashToken(string raw) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
```

### Example 4: Slug 生成（D-07/D-09，自实现 ~20 行）

```csharp
// Plane slug 规则：小写 + 数字 + 连字符，max 48，禁用受限词
private static readonly HashSet<string> RestrictedSlugs = new(StringComparer.OrdinalIgnoreCase)
{
    "404","accounts","api","admin","settings","auth","web","billing",
    "sign-in","sign-up","signin","signup","profile","invitations",
    /* ... 完整 66 项从 RESTRICTED_WORKSPACE_SLUGS 移植 */
};

public static string Slugify(string input)
{
    var slug = new StringBuilder();
    foreach (var c in input.ToLowerInvariant())
        slug.Append(char.IsLetterOrDigit(c) ? c : '-');
    var result = Regex.Replace(slug.ToString(), "-{2,}", "-").Trim('-');
    return result.Length > 48 ? result[..48] : result;
}

public static bool IsValidSlug(string slug) =>
    slug.Length >= 3 && slug.Length <= 48 &&
    Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$") &&
    !RestrictedSlugs.Contains(slug);
```

### Example 5: Workspace 实体（贴 Plane Workspace.py 字段 [VERIFIED]）

```csharp
// Plane apps/api/plane/db/models/workspace.py:88-110 字段对照
public sealed class Workspace : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable, IGlobalEntity
{
    public string Name { get; set; } = default!;              // Plane name max 80
    public string? Logo { get; set; }                         // Plane logo (text URL)
    public string Slug { get; set; } = default!;              // Plane slug max 48 unique
    public Guid OwnerId { get; set; }                         // Plane owner FK → 这里是标量 (D-06)
    public string? OrganizationSize { get; set; }             // Plane organization_size max 20
    public string TimeZone { get; set; } = "UTC";             // Plane timezone
    public string BackgroundColor { get; set; } = "#000000";  // Plane background_color
    // 审计字段由 AuditableEntitySaveChangesInterceptor 填
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }
}
```

## State of the Art

| Old Approach                                       | Current Approach                | When Changed    | Impact                                                                                                                                                                                              |
| -------------------------------------------------- | ------------------------------- | --------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Finbuckle 9.x（旧 FSH 模板）                       | Finbuckle 10.1.x（项目锁定）    | .NET 10 release | API 略有变化：`WithDelegateStrategy<TContext, TTenantInfo>` 新增 typed overload；`TenantResolver` 内部协调 strategy/store，无需手写 ITenantResolver [CITED: finbuckle.com/MultiTenant/Docs/v10.1.1] |
| 手写 tenant middleware                             | Finbuckle strategy 链           | 一直            | 多 strategy 优先级链 + 多 store 遍历，是 D-01 双租户模型并存的基础                                                                                                                                  |
| Plane Django `BaseModel.delete(soft=True)` 改 slug | EF 软删除 + handler 显式改 slug | —               | D-08 复刻 Plane 语义，必须 handler 显式做（EF 没有自动 hook）                                                                                                                                       |

**Deprecated/outdated:**

- 不要再用 Finbuckle 9.x 的 `ITenantResolver`（已内部化为 `TenantResolver` + strategy/store）；v10 文档明确"Custom strategies can be created by implementing `IMultiTenantStrategy` or using `DelegateStrategy`" [CITED]。
- 不要把 slug 解析塞进 `ClaimStrategy`（Phase 1 注释明确 `UseMultiTenant()` 在认证之前，User 是 anonymous）。

## Assumptions Log

| #   | Claim                                                                                                                                   | Section                       | Risk if Wrong                                                                                                                                                                                           |
| --- | --------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A1  | Finbuckle 10.1.1 允许在 `AddMultiTenant` builder 之外追加 strategy/store（通过 `services.TryAddEnumerable<IMultiTenantStrategy<...>>`） | Pattern 1, Pitfall 1          | 若不允许，需重构 `MultitenancyModule` 暴露 workspace 注册 hook；Wave 0 spike 验证（见 Open Question Q1）                                                                                                |
| A2  | slugify 自实现（20 行 Regex）足够，不引第三方包                                                                                         | Standard Stack, Code Examples | 边缘 case（Unicode/emoji/CJK）可能 slug 化不完美；Plane 前端校验在前，风险低；可后续换 `Slugify` NuGet                                                                                                  |
| A3  | 邀请 token TTL 默认 7 天（D-12 写"如 7 天"）                                                                                            | Invitation Token              | Plane 默认值未在源码中显式确认；规划阶段可定，配置键 `Workspace:InvitationTokenTtlDays`                                                                                                                 |
| A4  | `AppTenantInfo.Id` 存 workspace Guid 的字符串形式（D-01）即可作为 `TenantId` 给 `WorkspaceMember.TenantId` 用                           | Pattern 1                     | Finbuckle `TenantId` 是 string；EF `IHasTenant.TenantId` 在 `TenantIsolationExtensions` 也是 string。**Guid → string.ToString() 一致，已验证 IHasTenant.cs 是 string** [VERIFIED]。低风险               |
| A5  | workspace slug → tenant 缓存用 Phase 1 Redis（`IDistributedCache`）                                                                     | Pattern 1, Don't Hand-Roll    | `AddHeroCaching` 已注册 `IDistributedCache`；Redis 不可用时 fallback in-memory，Phase 1 已处理                                                                                                          |
| A6  | `WorkspaceMembershipMiddleware` 在 `UseAuthorization` 之后、endpoint 之前注册                                                           | Architecture Diagram          | 顺序错误会导致 `ICurrentWorkspaceContext` 在 authz handler 里为空；`[RequireWorkspaceRole]` 全部失败。规划阶段必须明确中间件注册位置（在 `WorkspaceModule.ConfigureMiddleware` 或 `Program.cs` 显式插） |

## Open Questions

1. **Q1: Finbuckle 10.1.1 是否允许在 `AddMultiTenant` builder 之外追加 strategy/store？**
   - What we know: Phase 1 `MultitenancyModule` 在 builder 链里注册所有 strategy/store。`services.AddMultiTenant` 第二次调用会覆盖。
   - What's unclear: 是否能用 `services.TryAddEnumerable(ServiceDescriptor.Singleton<IMultiTenantStrategy<AppTenantInfo>, WorkspaceSlugStrategy>())` 从外部模块追加，让 Finbuckle 的 `TenantResolver` 自然拾取。
   - Recommendation: Wave 0 spike —— 写一个最小 `WorkspaceSlugStrategy`，在 `WorkspaceModule` 用 `TryAddEnumerable` 注册，跑 `dotnet test` 验证 `IMultiTenantContextAccessor` 在 `/workspaces/{slug}/` 请求里被正确填充。若失败，回退：在 `Workspace.Contracts` 暴露 `AddWorkspaceTenantResolution(this MultiTenantBuilder<AppTenantInfo>)` 扩展方法，由 `MultitenancyModule` 调用（保持单一 builder 入口）。

2. **Q2: 公开端点 `/api/public/workspaces/{slug}/` 是否纳入 Phase 2？**
   - What we know: CONTEXT.md `<deferred>` 标记"规划阶段决定是否纳入"。
   - Recommendation: **延后**到含 Space app 的阶段（Plane Space app 无认证读）。Phase 2 集中精力把 tenant 解析 + 成员授权基座建稳；公开端点只需让 slug strategy 解析 tenant 但 middleware 不强制成员资格 —— 这是 D-02 已支持的（"公开端点可解析租户但不要求成员资格"），代码已为此设计，未来加端点零成本。

3. **Q3: `INotificationService` 抽象放 `Workspace.Contracts` 还是共享 BuildingBlocks？**
   - Recommendation: 放 `Workspace.Contracts`（D-10 明确"Phase 2 预留"）。Phase 11 实现 Notification 模块时，若发现多模块都要发通知，再提升到 BuildingBlocks。YAGNI。

## Environment Availability

| Dependency       | Required By               | Available                                               | Version            | Fallback                            |
| ---------------- | ------------------------- | ------------------------------------------------------- | ------------------ | ----------------------------------- |
| PostgreSQL       | WorkspaceDbContext 持久化 | ✓（Phase 1 已迁移，`identity` schema 已验证）           | 15+（Aspire 编排） | —                                   |
| Redis            | slug→tenant 缓存（D-01）  | ✓（Phase 1 `AddHeroCaching` + Redis health check 已配） | Valkey 7.2         | in-memory cache（Phase 1 fallback） |
| .NET 10 SDK      | 编译                      | ✓（Phase 1 已建 51 项目）                               | 10.x               | —                                   |
| Finbuckle 10.1.x | 多租户解析                | ✓（`Directory.Packages.props` 锁定）                    | 10.1.0             | —                                   |
| EF Core          | WorkspaceDbContext        | ✓                                                       | .NET 10 内置       | —                                   |

**Missing dependencies with no fallback:** none
**Missing dependencies with fallback:** none — Phase 2 完全在 Phase 1 已验证的环境内。

## Validation Architecture

> `workflow.nyquist_validation` 在 `.planning/config.json` 中未设置 → 默认**启用**。本节必填。

### Test Framework

| Property           | Value                                                                               |
| ------------------ | ----------------------------------------------------------------------------------- |
| Framework          | xUnit + FluentAssertions（贴 Phase 1 Identity.Tests，412 测试已过）                 |
| Config file        | `Directory.Packages.props` 锁定版本；各模块 `*.Tests` 项目                          |
| Quick run command  | `dotnet test src/Tests/Workspace.Tests --filter "FullyQualifiedName~Unit" --nologo` |
| Full suite command | `dotnet test src/YH.Flow.slnx --nologo`                                             |

### Phase Requirements → Test Map

| Req ID  | Behavior                                        | Test Type        | Automated Command                                                     | File Exists? |
| ------- | ----------------------------------------------- | ---------------- | --------------------------------------------------------------------- | ------------ |
| REQ-2.1 | slug 生成 + 受限词拒绝 + 冲突重试               | unit             | `dotnet test src/Tests/Workspace.Tests --filter SlugGenerator`        | ❌ Wave 0    |
| REQ-2.1 | 软删除追加 epoch 释放 slug                      | unit/integration | `dotnet test src/Tests/Workspace.Tests --filter SoftDelete`           | ❌ Wave 0    |
| REQ-2.1 | slug 唯一性竞态（DB 唯一约束）                  | integration      | `dotnet test src/Tests/Workspace.Tests --filter SlugUnique`           | ❌ Wave 0    |
| REQ-2.1 | 创建 workspace 自动建 Admin member              | unit             | `dotnet test src/Tests/Workspace.Tests --filter CreateWorkspace`      | ❌ Wave 0    |
| D-01    | slug → tenant 解析正确填充 TenantInfo           | integration      | `dotnet test src/Tests/Workspace.Tests --filter SlugTenantResolve`    | ❌ Wave 0    |
| NFR-2   | 跨 workspace 数据隔离（A 的 member 不出现在 B） | integration      | `dotnet test src/Tests/Workspace.Tests --filter TenantIsolation`      | ❌ Wave 0    |
| D-02    | 非成员访问 workspace 端点返回 403               | integration      | `dotnet test src/Tests/Workspace.Tests --filter Membership`           | ❌ Wave 0    |
| D-11    | role 不足返回 403（Member 改设置、Guest 写）    | integration      | `dotnet test src/Tests/Workspace.Tests --filter RequireWorkspaceRole` | ❌ Wave 0    |
| REQ-2.2 | list members 批量查用户无 N+1（单次 SQL）       | integration      | `dotnet test src/Tests/Workspace.Tests --filter ListMembers`          | ❌ Wave 0    |
| REQ-2.4 | 邀请 token 哈希存储（DB 无明文）                | unit             | `dotnet test src/Tests/Workspace.Tests --filter InvitationHash`       | ❌ Wave 0    |
| REQ-2.4 | accept/reject/revoke 后 token 失效              | integration      | `dotnet test src/Tests/Workspace.Tests --filter InvitationInvalidate` | ❌ Wave 0    |
| D-12    | token 过期拒绝                                  | unit             | `dotnet test src/Tests/Workspace.Tests --filter InvitationTtl`        | ❌ Wave 0    |

### Sampling Rate

- **Per task commit:** `dotnet test src/Tests/Workspace.Tests --filter "Unit" --nologo`（快速单测）
- **Per wave merge:** `dotnet test src/YH.Flow.slnx --nologo`（全量回归，确保不破坏 Phase 1 + 其它模块）
- **Phase gate:** 全量绿 + 手工 smoke：用 API Key/JWT 创建 workspace → 邀请 → 接受 → list members → 改 role → 删除（验证 slug 释放）

### Wave 0 Gaps

- [ ] `src/Tests/Workspace.Tests/` 项目（csproj + 引用 Workspace + TestUtils）—— 模仿 `src/Tests/Identity.Tests/`
- [ ] `src/Tests/Workspace.Tests/TestData/WorkspaceTestFixture.cs` —— 共享 fixture：起 PostgreSQL（Testcontainers 或 Aspire）+ seed root user
- [ ] `src/Tests/Workspace.Tests/Services/SlugGeneratorTests.cs` —— REQ-2.1 slug 规则
- [ ] `src/Tests/Workspace.Tests/Authorization/RequireWorkspaceRoleHandlerTests.cs` —— D-11
- [ ] `src/Tests/Workspace.Tests/Integration/TenantIsolationTests.cs` —— NFR-2 跨 workspace 隔离（贴 `src/Tests/Architecture.Tests/TenantIsolationTests.cs` 模式）
- [ ] Q1 spike: `src/Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs` —— 验证 builder 外追加 strategy

## Security Domain

> `security_enforcement` 在 `.planning/config.json` 中未显式设为 false → 默认**启用**。

### Applicable ASVS Categories

| ASVS Category         | Applies | Standard Control                                                                                                              |
| --------------------- | ------- | ----------------------------------------------------------------------------------------------------------------------------- |
| V2 Authentication     | no      | Phase 1 已实现（JWT/API Key/Session），Phase 2 复用                                                                           |
| V3 Session Management | no      | Phase 1 已实现                                                                                                                |
| V4 Access Control     | **yes** | `[RequireWorkspaceRole]` + `ICurrentWorkspaceContext`（D-11）；workspace-role 能力矩阵贴 Plane；**默认拒绝**（无 role → 403） |
| V5 Input Validation   | **yes** | FluentValidation（每 command/DTO）+ slug 格式校验（D-09）+ email 校验（贴 Plane `validate_email`）                            |
| V6 Cryptography       | **yes** | 邀请 token：`RandomNumberGenerator`（CSPRNG）+ `SHA256` at-rest hash（D-12）；**禁止明文 token 落库**                         |
| V7 Error Handling     | yes     | `GlobalExceptionHandler`（Phase 1），不泄露存在性（slug 不存在 → 404）                                                        |
| V8 Data Protection    | yes     | 多租户隔离（`IHasTenant` auto-filter，**默认开启**）                                                                          |
| V13 API & Web Service | yes     | 强制认证（除预留公开端点）+ Rate Limiting（Phase 1）                                                                          |

### Known Threat Patterns for ASP.NET Core + Finbuckle 多租户

| Pattern                                   | STRIDE                             | Standard Mitigation                                                                                    |
| ----------------------------------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------ |
| 跨 workspace 数据泄露（IDOR）             | Tampering / Information Disclosure | `BaseDbContext` 自动 tenant filter + `ApplyTenantIsolationByDefault`（漏加实体也安全） [VERIFIED]      |
| 租户过滤绕过（手动 `IgnoreQueryFilters`） | Elevation of Privilege             | 禁止业务代码调 `IgnoreQueryFilters`；如必须（管理员视图）需 Architecture Test 守护                     |
| Slug 枚举（404 vs 403 泄露存在性）        | Information Disclosure             | slug 不存在 → 404；存在但非成员 → 403（D-02 决策已定，Phase 2 接受此泄露 —— Plane 行为一致）           |
| 邀请 token 伪造                           | Spoofing                           | CSPRNG + 32 字节（256 bit）；SHA256 at-rest；TTL + 单次使用                                            |
| 邀请 token 重放                           | Repudiation                        | accept/reject/revoke 后置 `RespondedAt`，accept 端点校验未响应                                         |
| Slug 注入（XSS via slug）                 | Tampering                          | slugify 严格 `[a-z0-9-]`，DB 唯一约束；响应序列化自动 HTML 编码                                        |
| 权限提升（Member 改自己为 Admin）         | Elevation of Privilege             | `[RequireWorkspaceRole(Admin)]` 守护 update-role 端点；handler 额外校验 target user != self 或显式允许 |
| Rate Limit 绕过                           | DoS                                | Phase 1 四层限流已配                                                                                   |

## Sources

### Primary (HIGH confidence)

- **Finbuckle MultiTenant Docs v10.1.1** — Strategies, Stores, EFCore, ASP.NET Core Integration — https://www.finbuckle.com/MultiTenant/Docs/v10.1.1/Strategies （`WithDelegateStrategy`、`WithRouteStrategy`、`IMultiTenantStrategy`、`IMultiTenantStore` API）
- **项目源码**（直接读取，最高权威）：
  - `yh-flow/src/Modules/Multitenancy/Modules.Multitenancy/MultitenancyModule.cs` — 现有 Finbuckle strategy/store 注册（D-01 集成基础）
  - `yh-flow/src/BuildingBlocks/Persistence/Context/BaseDbContext.cs` — `MultiTenantDbContext` 派生、软删除 filter、tenant isolation
  - `yh-flow/src/BuildingBlocks/Persistence/TenantIsolationExtensions.cs` — `ApplyTenantIsolationByDefault` 机制
  - `yh-flow/src/Modules/Auditing/Modules.Auditing/Persistence/AuditDbContext.cs` — `BaseDbContext` 派生模板（OnModelCreating 顺序）
  - `yh-flow/src/Modules/Identity/Modules.Identity/Authorization/RequiredPermissionAuthorizationHandler.cs` — authz handler 模板（D-11 仿照）
  - `yh-flow/src/Modules/Identity/Modules.Identity/Services/ApiTokenService.cs:170-200` — `RandomNumberGenerator` + `SHA256` 模板（D-12 仿照）
  - `yh-flow/src/BuildingBlocks/Core/Domain/{IHasTenant,IGlobalEntity,ISoftDeletable}.cs` — 实体 marker 接口
  - `yh-flow/src/Host/YH.Flow.Api/Program.cs` — 模块注册、`UseHeroMultiTenantDatabases()` 调用
  - `yh-flow/src/Host/YH.Flow.DbMigrator/Program.cs` — 模块 assembly 注册（Phase 2 须加 WorkspaceModule）
  - `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/Identity/` — 模块迁移目录结构模板
- **Plane 源码**（API 契约 + 数据模型 ground truth）：
  - `apps/api/plane/db/models/workspace.py` — Workspace/WorkspaceMember/WorkspaceMemberInvite 实体（slug/role/token 字段、软删除 epoch append）
  - `apps/api/plane/app/urls/workspace.py` — workspace 路由契约
  - `apps/api/plane/api/urls/{member,invite}.py` — 子路由
  - `apps/api/plane/api/serializers/{member,invite}.py` — 响应字段
  - `apps/api/plane/app/permissions/workspace.py` — role 能力矩阵（20/15/5）
  - `apps/api/plane/utils/constants.py` — `RESTRICTED_WORKSPACE_SLUGS`（66 项，已提取）

### Secondary (MEDIUM confidence)

- WebSearch: Finbuckle.MultiTenant .NET 10 route resolver / ITenantResolver / custom strategy — 多源印证 v10.x API（custom strategy via `IMultiTenantStrategy`/`DelegateStrategy`，无独立 `ITenantResolver`） — https://www.finbuckle.com/multitenant/docs/CoreConcepts
- NuGet registry: `Slugify` 包存在（0.1.0，3 版本，维护稀疏）→ 选自实现 — https://api.nuget.org/v3-flatcontainer/slugify/index.json

### Tertiary (LOW confidence — 已标 [ASSUMED])

- A1（builder 外追加 strategy 可行性）—— 需 Wave 0 spike
- A2（自实现 slugify 覆盖边缘 case）—— 风险低
- A3（邀请 token TTL 默认 7 天）—— 规划阶段定

## Metadata

**Confidence breakdown:**

- Standard stack: **HIGH** — 全部依赖已在项目锁定版本，直接读源码确认
- Architecture（双租户并存）: **MEDIUM-HIGH** — Finbuckle 文档 + 现有 MultitenancyModule 源码印证；唯一不确定点是 A1（builder 外追加），有明确回退方案
- Pitfalls: **HIGH** — 全部基于现有代码读 + Plane 源码对比
- Plane 契约: **HIGH** — 直接读 Plane 源码

**Research date:** 2026-06-17
**Valid until:** 2026-07-17（30 天；Finbuckle 10.1.x 稳定，无重大变更风险）

---

## RESEARCH COMPLETE

**Phase:** 02 - workspace
**Confidence:** HIGH（核心栈 HIGH；仅 Finbuckle builder 外追加 strategy 需 Wave 0 spike 验证，有明确回退）

### Key Findings

1. **0 新增依赖** —— Phase 2 完全复用 Phase 1 锁定的 Finbuckle 10.1.0 + EF Core + Mediator + BCL crypto。slugify 自实现 ~20 行（贴 `ApiTokenService` 的 `RandomNumberGenerator`/`SHA256` 模式）。
2. **双租户模型并存是分层不是冲突** —— Phase 1 的 `MultitenancyModule`（claim/header/query + `EFCoreStore<TenantDbContext>`）管平台级订阅/quota；Phase 2 新增 slug strategy + `WorkspaceTenantStore` 管 Plane 业务隔离。两者通过标识符来源 + store 命中条件自然分层。**绝对不能再调 `AddMultiTenant<AppTenantInfo>`**（会覆盖 Phase 1 配置）。
3. **D-01 用 Finbuckle `WithDelegateStrategy<HttpContext, AppTenantInfo>` 实现** —— 从 route value `slug` 取标识符，排 strategy 链首位；顶层无 slug 端点返回 null 让 Phase 1 strategy 接管。`WorkspaceTenantStore : IMultiTenantStore<AppTenantInfo>` 把 slug 解析成 `AppTenantInfo(Id=workspaceGuid, Identifier=slug)`，复用 Phase 1 Redis 缓存。
4. **`Workspace` 实体必须 `IGlobalEntity`**（它 IS 租户，不能 `IsMultiTenant()`）；`WorkspaceMember`/`WorkspaceInvitation` 不标，由 `ApplyTenantIsolationByDefault()` 自动加 tenant filter。**默认开启、漏加实体也安全** —— 这是跨 12 模块零漏过滤的保障。
5. **Wave 0 必做 spike**（Open Question Q1）：验证 Finbuckle 10.1.1 是否允许 builder 外用 `services.TryAddEnumerable<IMultiTenantStrategy<AppTenantInfo>>` 追加 strategy；若不行回退到 `Workspace.Contracts` 暴露 `AddWorkspaceTenantResolution(this MultiTenantBuilder)` 扩展方法供 `MultitenancyModule` 调用。

### File Created

`D:\github\akinix-plane\.planning\phases\02-workspace\02-RESEARCH.md`

### Confidence Assessment

| Area                                       | Level       | Reason                                                             |
| ------------------------------------------ | ----------- | ------------------------------------------------------------------ |
| Standard Stack                             | HIGH        | 全部依赖已在 `Directory.Packages.props` + 现有代码确认，0 新增     |
| Architecture（tenant 解析）                | MEDIUM-HIGH | Finbuckle 文档 + MultitenancyModule 源码印证；A1 需 spike，有回退  |
| Architecture（DbContext/authz/middleware） | HIGH        | 直接读 AuditDbContext/IdentityModule/BaseDbContext 源码确认模式    |
| Pitfalls                                   | HIGH        | 全部基于代码读 + Plane 源码对比，6 项均有具体 warning sign         |
| Plane 契约                                 | HIGH        | 直接读 workspace.py / urls / serializers / permissions / constants |

### Open Questions

1. **Q1 (blocking Wave 1):** Finbuckle 10.1.1 builder 外追加 strategy 的可行性 —— Wave 0 spike，回退方案已定
2. **Q2:** 公开端点是否纳入 Phase 2 —— 建议**延后**（设计已预留）
3. **Q3:** `INotificationService` 放哪 —— 建议 `Workspace.Contracts`（YAGNI）

### Ready for Planning

研究完成。Planner 可基于本 RESEARCH.md 创建 PLAN.md。建议 wave 划分：

- **Wave 0:** spike Q1 + Workspace.Tests 项目脚手架 + Workspace.Contracts（ICurrentWorkspaceContext/Constants/DTOs）
- **Wave 1:** Domain + WorkspaceDbContext + 迁移 + Finbuckle strategy/store 接入（D-01）
- **Wave 2:** WorkspaceMembershipMiddleware + `[RequireWorkspaceRole]` + ICurrentWorkspaceContext 实现（D-02/D-11）
- **Wave 3:** Workspace CRUD + slug service（REQ-2.1）
- **Wave 4:** Member 端点 + IUserIdentityService（REQ-2.2）+ Invitation 端点 + token service（REQ-2.4）
- **Wave 5:** 全量回归 + Phase gate（手工 smoke + 全 suite 绿）
