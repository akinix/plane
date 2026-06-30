# Phase 1: Foundation — 基础设施 - Context

**Gathered:** 2026-06-16
**Status:** Ready for planning

<domain>
## Phase Boundary

认证体系（JWT + API Key + Session + OAuth 框架）、API 基础设施（异常处理、分页、CORS、Rate Limiting、OpenAPI）、数据库迁移管道、多租户基础、可观测性（Serilog + OpenTelemetry）。

此阶段不涉及任何业务模块（Workspace/Project/WorkItems 等）的具体实现，仅搭建它们运行所需的基础设施。
</domain>

<decisions>
## Implementation Decisions

### 多认证方式协商

- **D-01:** 多 Scheme 自动协商 — JWT Bearer / API Key / Session Cookie 各注册一个 ASP.NET Core AuthenticationScheme，按请求 Header 自动选择
- **D-02:** OAuth 登录成功后双发 JWT + Session Cookie — 前端可选择用 Cookie（自动携带）或 JWT（手动 Header），兼容 Plane 前端和 API 客户端
- **D-03:** API Key 数据库存储 + 多 Key 模型 — 参考 Plane APIToken，每个用户可创建多个 API Key，含名称、过期时间。存储在 Identity 模块的数据库表中
- **D-04:** 复用 FSH 认证端点 + 适配格式 — /auth/sign-in、/auth/sign-up、/auth/me 等端点复用 FSH Identity 模块实现，调整响应格式匹配 Plane

### OAuth Provider 框架

- **D-05:** Phase 1 只搭框架 — 抽象 Provider 接口 + 注册机制 + 回调路由，不实现具体 Provider（GitHub/GitLab/Gitea/Google 在 Phase 9 Integration 实现）
- **D-06:** OAuth 回调重定向 + 双发凭证 — 登录成功后重定向到前端指定 URL，同时发放 JWT + Cookie
- **D-07:** Provider 配置数据库动态管理 — 创建 OAuthProviderSettings 实体（ProviderName/ClientId/ClientSecret/CallbackUrl/Enabled/AutoCreateAccount），提供 CRUD 管理端点
- **D-08:** 配置实体放在 Identity 模块 — 含管理端点，启动时加载到内存缓存，修改后自动刷新

### Rate Limiting 策略

- **D-09:** 直接复用 FSH 四层限流方案 — Tenant（1000/60s）+ User（200/60s）+ IP（300/60s）+ Auth（10/60s），通过 appsettings.json 调整参数
- **D-10:** 限流响应用 FSH 的 RFC 7807 ProblemDetails 格式 — 429 + Retry-After header，不修改为 Plane 格式
- **D-11:** 仅全局配置 — 不做端点级限流覆盖，所有端点用同一套策略

### EF Core + 数据库 Schema 策略

- **D-12:** 模块独立 Schema — 每个模块有自己的 Schema（yhschema.Identity, yhschema.Workspace, yhschema.Project 等），遵循 FSH 模块边界原则
- **D-13:** 每个模块独立管理迁移 — 迁移放在各模块的 Data/Migrations/ 目录下
- **D-14:** 单一连接字符串 — 所有 DbContext 共用同一数据库，通过 Schema 分离
- **D-15:** DbMigrator 启动时执行迁移 — Aspire 编排中 DbMigrator 先于 API 运行，执行所有模块迁移 + Seed 数据

### Claude's Discretion

- EF Core 实体配置的具体实现细节（IEntityTypeConfiguration 的组织方式）
- Serilog Sink 配置和 OpenTelemetry Exporter 的具体选择
- CORS 策略的具体域名配置（开发阶段可宽松）
- Security Headers 的具体配置（Helmet 等中间件参数）
- 全局异常处理的具体错误格式适配（Plane 错误字段映射）
- 分页格式适配的具体实现（count/next/previous/results）
- Idempotency 中间件的具体实现

</decisions>

<canonical_refs>

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 项目规划文档

- `.planning/PROJECT.md` — 项目愿景、架构策略、模块映射、约束条件
- `.planning/REQUIREMENTS.md` — Phase 1 需求（REQ-1.1 ~ REQ-1.4）、非功能需求（NFR-1 ~ NFR-4）
- `.planning/ROADMAP.md` — Phase 1 任务列表（T1.1 ~ T1.12）
- `.planning/STATE.md` — 项目状态和关键决策日志

### Phase 0 上下文

- `.planning/phases/00-init/00-CONTEXT.md` — Phase 0 决策（命名空间、项目结构、Aspire 编排）

### 研究文档

- `.planning/research/fullstackhero-patterns.md` — FSH 模式适配指南（10 条黄金法则、模块结构、API 端点模式、Plane 适配策略）
- `.planning/research/api-migration-mapping.md` — Django → .NET API 端点映射
- `.planning/research/domain-overview.md` — 领域实体模型

### 模板参考（Fullstackhero）

- `D:/github/fullstackhero-dotnet-starter-kit/AGENTS.md` — 模板编码规范和 10 条黄金法则
- `D:/github/fullstackhero-dotnet-starter-kit/.agents/rules/` — 模板 Agent 规则目录
- `D:/github/fullstackhero-docs/` — Fullstackhero 文档（Astro）

### 代码库分析（Plane 原始代码）

- `.planning/codebase/ARCHITECTURE.md` — Plane 架构分析（认证流程、多租户、API 设计）
- `.planning/codebase/STACK.md` — Plane 技术栈分析
- `.planning/codebase/CONVENTIONS.md` — Plane 编码约定

### 认证相关源码

- `yh-flow/src/Modules/Identity/` — FSH Identity 模块（JWT 认证、Token 生成、权限系统）
- `yh-flow/src/BuildingBlocks/Web/RateLimiting/` — FSH Rate Limiting 基础设施（Extensions.cs, RateLimitingOptions.cs）
- `yh-flow/src/BuildingBlocks/Web/Auth/` — FSH 认证中间件（CurrentUserMiddleware.cs）
- `yh-flow/src/Host/YH.Flow.DbMigrator/` — DbMigrator 项目
- `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/` — PostgreSQL 迁移项目

### Plane 认证参考

- `apps/api/plane/authentication/` — Plane 认证模块（adapter、provider、views）
- `apps/api/plane/authentication/provider/oauth/` — Plane OAuth Provider 实现（github.py, gitlab.py, gitea.py, google.py）
- `apps/api/plane/api/middleware/api_authentication.py` — Plane API Key 认证中间件

</canonical_refs>

<code_context>

## Existing Code Insights

### Reusable Assets

- **FSH Identity 模块** (`yh-flow/src/Modules/Identity/`) — 已有 JWT 认证、Token 生成/刷新、权限授权、角色管理。Phase 1 需扩展支持 API Key 和 Session Cookie schemes
- **FSH Rate Limiting** (`yh-flow/src/BuildingBlocks/Web/RateLimiting/`) — 完整的四层限流方案（Tenant/User/IP/Auth），可直接复用
- **FSH BaseDbContext** (`yh-flow/src/BuildingBlocks/Persistence/`) — 支持多租户过滤、Schema 分离、审计字段。每个模块继承即可
- **FSH 全局异常处理** — BuildingBlocks/Web 中已有异常处理中间件，需适配 Plane 错误格式
- **FSH Serilog 配置** — 已有结构化日志配置，需调整 Sink 和 OpenTelemetry 集成
- **FSH Scalar/OpenAPI** — 已有 API 文档配置

### Established Patterns

- **模块结构** — 每个模块含 `{Module}.csproj` + `{Module}.Contracts.csproj`，Contracts 暴露接口/DTO 供跨模块引用
- **认证 Scheme 注册** — `AddAuthentication().AddJwtBearer().AddCookie()` 多 Scheme 模式
- **权限模型** — `RequirePermission(PermissionConstant)` + `PermissionAuthorizationHandler`
- **多租户** — Finbuckle `IHasTenant` 接口 + 自动租户过滤
- **迁移模式** — DbMigrator 启动时执行 → 模块内迁移 → Seed 数据

### Integration Points

- **Identity 模块** — Phase 1 核心扩展点：添加 API Key scheme、Session Cookie scheme、OAuth Provider 框架
- **BuildingBlocks/Web** — 扩展点：异常处理格式适配、分页格式适配、CORS/Security Headers 配置
- **DbMigrator** — 需注册所有模块的 Mediator assemblies 和 module assemblies
- **API Host** — Program.cs 需注册所有模块、认证 schemes、中间件管道
- **Aspire AppHost** — 已有 PostgreSQL/Redis/MinIO 编排，DbMigrator 需作为 Resource 添加

</code_context>

<specifics>
## Specific Ideas

- OAuth Provider 回调 URL 格式：`/auth/oauth/{provider}/callback`（provider = github, gitlab, gitea, google）
- API Key 前缀可参考 Plane：`pk_` 前缀 + 随机字符串，数据库存储哈希值
- Schema 命名统一用 `yhschema.` 前缀（如 `yhschema.Identity`），避免与 PostgreSQL 系统 Schema 冲突
- 迁移历史表命名：`__EFMigrationsHistory_{Module}`（每个 Schema 独立）

</specifics>

<deferred>
## Deferred Ideas

- **具体 OAuth Provider 实现** — Phase 9 Integration 实现 GitHub/GitLab/Gitea/Google Provider
- **Slack OAuth 集成** — Phase 9 Integration（REQ-9.4）
- **端点级限流覆盖** — 后续如有特殊端点需要差异化限流，可在 Phase 2+ 添加
- **Magic Link 认证** — Plane 支持 magic code 登录（`apps/api/plane/authentication/provider/credentials/magic_code.py`），一期不实现
- **Per-模块独立数据库** — 一期用单一数据库 + Schema 分离，后续如需物理隔离可迁移

</deferred>

---

_Phase: 01-foundation_
_Context gathered: 2026-06-16_
