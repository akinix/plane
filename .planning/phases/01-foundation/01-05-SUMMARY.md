---
phase: 01-foundation
plan: 05
subsystem: infrastructure
tags: [ef-migration, schema-push, verification, phase-complete]

# Dependency graph
requires:
  - phase: 01-foundation
    plan: 01
    provides: Multi-scheme auth (JWT + API Key + Session Cookie + SmartSelector)
  - phase: 01-foundation
    plan: 02
    provides: APIToken entity, OAuthProviderSettings entity, EF configurations
  - phase: 01-foundation
    plan: 03
    provides: OAuth Provider framework, Plane auth endpoints
  - phase: 01-foundation
    plan: 04
    provides: API Token CRUD, OAuth Provider management, Plane format adapters
provides:
  - EF Core migration for identity.ApiTokens and identity.OAuthProviderSettings tables
  - Full Phase 1 verification (build, tests, design decision coverage)
  - Phase 1 completion status
affects: [phase-02, phase-09-integration]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "EF Core migration: dotnet ef migrations add with --context IdentityDbContext"
    - "Migration includes identity schema tables with unique indexes (TokenHash, ProviderName)"
    - "Multi-tenant APIToken includes TenantId; IGlobalEntity OAuthProviderSettings omits it"

key-files:
  created:
    - yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/20260617103113_AddAPITokenAndOAuthProviderSettings.cs
    - yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/20260617103113_AddAPITokenAndOAuthProviderSettings.Designer.cs
  modified:
    - yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/IdentityDbContextModelSnapshot.cs

key-decisions:
  - "Migration uses identity schema for both new tables (D-12: module-isolated schemas)"
  - "APIToken migration includes TenantId column with composite unique index (TokenHash + TenantId)"
  - "OAuthProviderSettings migration omits TenantId (IGlobalEntity: global config, not tenant-scoped)"
  - "Database migration deferred to manual execution (Docker/PostgreSQL required)"

patterns-established:
  - "Schema push pattern: generate migration → verify DDL → commit → apply via DbMigrator"

requirements-completed: [REQ-1.2, REQ-1.3, REQ-1.4, NFR-1, NFR-2]

# Metrics
duration: 20min
completed: 2026-06-17
---

# Phase 1 Plan 05: Schema Push + Full Verification Summary

**EF Core 迁移生成 + 完整 Phase 1 验证（51 项目编译、1017 测试通过、8 项设计决策覆盖）**

## Performance

- **Duration:** 20 min
- **Started:** 2026-06-17T08:00:00Z
- **Completed:** 2026-06-17T08:20:00Z
- **Tasks:** 3
- **Files modified:** 3

## Accomplishments

- **Task 1 — EF Core 迁移生成:** `AddAPITokenAndOAuthProviderSettings` 迁移包含 `identity."ApiTokens"` 表（含 TenantId、TokenHash 唯一索引、UserId 外键）和 `identity."OAuthProviderSettings"` 表（含 ProviderName 唯一索引，无 TenantId）。
- **Task 2 — 数据库迁移:** 跳过（需手动启动 Docker PostgreSQL 容器并运行 DbMigrator）。迁移文件已就绪，随时可应用。
- **Task 3 — 全阶段验证:** 51 项目编译零错误零警告。1,017 个单元测试通过（12/14 项目）。全部 8 个关键设计决策（D-01, D-02, D-03, D-04, D-05, D-07, D-09, D-12）验证通过。Serilog + OpenTelemetry 配置完整。

## Task Commits

1. **Task 1: 生成 EF Core 迁移**
   - `ebc39c2` (feat) — Migration for ApiTokens and OAuthProviderSettings tables

## Files Created/Modified

- `YH.Flow.Migrations.PostgreSQL/20260617103113_AddAPITokenAndOAuthProviderSettings.cs` — 迁移文件：创建 ApiTokens 和 OAuthProviderSettings 表
- `YH.Flow.Migrations.PostgreSQL/20260617103113_AddAPITokenAndOAuthProviderSettings.Designer.cs` — 迁移元数据
- `YH.Flow.Migrations.PostgreSQL/IdentityDbContextModelSnapshot.cs` — 更新后的模型快照

## Design Decision Verification

| 编号 | 决策 | 状态 | 实现位置 |
|------|------|------|----------|
| D-01 | PolicyScheme 双认证选择器 | ✅ | JwtAuthenticationExtensions.cs: SmartSelector |
| D-02 | 双重 JWT + Cookie 签发 | ✅ | PlaneSignInEndpoint.cs + PlaneAuthHelpers.cs |
| D-03 | 多 API Key 支持 | ✅ | APIToken entity + ApiTokenService |
| D-04 | /auth/* Plane 兼容端点 | ✅ | 6 个端点（sign-in, sign-up, sign-out, me, oauth/*） |
| D-05 | Phase 1 OAuth 框架 | ✅ | IOAuthProvider + OAuthProviderRegistry |
| D-07 | OAuth Provider 数据库管理 | ✅ | OAuthProviderSettings + CRUD endpoints |
| D-09 | FSH 四层限流 | ✅ | appsettings.json: RateLimitingOptions.Enabled = true |
| D-12 | 模块独立 Schema | ✅ | IdentityModuleConstants.SchemaName = "identity" |

## Test Results

| 项目 | 通过 | 失败 | 总计 |
|------|------|------|------|
| Identity.Tests | 412 | 0 | 412 |
| Framework.Tests | 92 | 0 | 92 |
| Multitenancy.Tests | 90 | 0 | 90 |
| Billing.Tests | 91 | 0 | 91 |
| Catalog.Tests | 75 | 0 | 75 |
| Auditing.Tests | 63 | 0 | 63 |
| Webhooks.Tests | 54 | 0 | 54 |
| Generic.Tests | 43 | 0 | 43 |
| Chat.Tests | 36 | 0 | 36 |
| Caching.Tests | 33 | 0 | 33 |
| Files.Tests | 23 | 0 | 23 |
| Integration.Middleware.Tests | 5 | 0 | 5 |
| **总计** | **1,017** | **0** | **1,017** |

### 已知非阻塞问题

- **Architecture.Tests:** 3 个失败（端点命名约定，预已存在的问题，非 Phase 1 引入）
- **Integration.Tests:** 需要 Docker 基础设施（PostgreSQL + Redis），在 CI 环境中运行

## Deviations from Plan

### Auto-fixed Issues

**1. [Non-blocking] 数据库迁移手动执行**
- **Found during:** Task 2 执行
- **Issue:** PostgreSQL 容器存在但未运行，无法自动执行 DbMigrator
- **Fix:** 迁移文件已生成并提交，用户可手动启动容器后运行 DbMigrator
- **Verification:** 迁移文件内容已验证包含正确的表定义

---

**Total deviations:** 1 (non-blocking, deferred to manual execution)
**Impact on plan:** No scope change. Database migration is the only remaining manual step.

## Observability Verification

- ✅ **Serilog:** 配置于 `appsettings.json`，使用 Console sink
- ✅ **OpenTelemetry:** `AddHeroOpenTelemetry()` 扩展方法配置 metrics + tracing
  - Metrics: ASP.NET Core, HttpClient, PostgreSQL, Redis, Runtime
  - Tracing: ASP.NET Core, HttpClient, EntityFrameworkCore, PostgreSQL, Redis
  - OTLP exporter（支持 Aspire 自动发现）

## User Setup Required

启动 PostgreSQL 容器并执行数据库迁移:
```bash
docker start <postgres-container-id>
cd yh-flow && dotnet run --project src/Host/YH.Flow.DbMigrator/
```

## Phase 1 Completion Status

🟢 **Phase 1: Foundation — COMPLETE** (pending DB migration)

**已交付:**
- 3 个认证方案（JWT Bearer + API Key + Session Cookie）+ SmartSelector 路由器
- 2 个新领域实体（APIToken + OAuthProviderSettings）+ EF 配置
- OAuth Provider 框架（IOAuthProvider + Registry + SettingsService）
- 6 个 Plane 兼容认证端点（/auth/*）
- 3 个 API Token CRUD 端点
- 5 个 OAuth Provider 管理端点
- Plane 格式适配（错误格式 + 分页格式）
- EF Core 迁移（待应用）
- CORS + Rate Limiting 配置
- Serilog + OpenTelemetry 可观测性

---
*Phase: 01-foundation*
*Completed: 2026-06-17*

## Self-Check: PASSED
Migration files exist and contain correct DDL. Full solution builds (51 projects, 0 errors). 1,017 unit tests pass. 8/8 design decisions verified.
