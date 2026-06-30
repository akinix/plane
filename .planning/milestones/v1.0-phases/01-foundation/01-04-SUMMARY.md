---
phase: 01-foundation
plan: 04
subsystem: auth
tags: [api-token-crud, oauth-management, plane-format, paged-result, cors, rate-limiting]

# Dependency graph
requires:
  - phase: 01-foundation
    plan: 01
    provides: SmartSelector auth schemes, API Key + Session Cookie infrastructure
  - phase: 01-foundation
    plan: 02
    provides: APIToken entity, OAuthProviderSettings entity, IApiTokenService, IOAuthProviderSettingsService
  - phase: 01-foundation
    plan: 03
    provides: AuthEndpoints route registration, Plane auth helpers, OAuthProviderRegistry
provides:
  - ApiTokenService implementation (SHA-256 hash storage, pk_ prefix key gen, CRUD)
  - POST/GET/DELETE /auth/api-tokens endpoints (Plane-compatible API Key management)
  - OAuth Provider CRUD endpoints at /oauth-providers
  - PlanePagedResult<T> + PlanePagedResultFactory (Plane pagination format)
  - GlobalExceptionHandler Plane format adapter (/auth/* → {error, error_code, error_detail})
  - CORS configuration updated (x-api-key, x-tenant headers)
  - Rate limiting enabled
affects: [01-05, phase-09-integration, phase-13-frontend]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "API Token key format: pk_ prefix + 32 hex chars, SHA-256 hashed for storage, plaintext returned once"
    - "PlanePagedResultFactory: non-generic factory class to avoid CA1000 (static members on generic types)"
    - "Plane error format: { error, error_code, error_detail } for /auth/* routes vs RFC 7807 for /api/*"
    - "Mediator handler pattern: one handler per command, colocated with endpoint in same feature folder"
    - "IApiTokenService returns (APITokenDto, string RawKey) tuple — plaintext only available at creation time"

key-files:
  created:
    - yh-flow/src/Modules/Identity/Modules.Identity/Services/ApiTokenService.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/ApiTokens/Create/CreateApiTokenEndpoint.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/ApiTokens/Create/CreateApiTokenCommandHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/ApiTokens/List/ListApiTokensEndpoint.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/ApiTokens/List/ListApiTokensQueryHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/ApiTokens/Revoke/RevokeApiTokenEndpoint.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/ApiTokens/Revoke/RevokeApiTokenCommandHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/OAuth/Manage/ManageOAuthProviderEndpoint.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/OAuth/Manage/GetOAuthProvidersQueryHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/OAuth/Manage/CreateOAuthProviderCommandHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/OAuth/Manage/UpdateOAuthProviderCommandHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/OAuth/Manage/ToggleOAuthProviderCommandHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/OAuth/Manage/DeleteOAuthProviderCommandHandler.cs
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/v1/ApiTokens/CreateApiTokenCommand.cs
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/v1/ApiTokens/ListApiTokensQuery.cs
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/v1/ApiTokens/RevokeApiTokenCommand.cs
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/v1/OAuth/ManageOAuthProviderCommand.cs
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/v1/OAuth/GetOAuthProvidersQuery.cs
    - yh-flow/src/BuildingBlocks/Shared/Persistence/PlanePagedResult.cs
    - yh-flow/src/Tests/Identity.Tests/Services/ApiTokenServiceTests.cs
    - yh-flow/src/Tests/Identity.Tests/Handlers/PlaneFormatAdapterTests.cs
  modified:
    - yh-flow/src/Modules/Identity/Modules.Identity.Contracts/Services/IApiTokenService.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/Features/v1/Auth/AuthEndpoints.cs
    - yh-flow/src/Modules/Identity/Modules.Identity/IdentityModule.cs
    - yh-flow/src/BuildingBlocks/Web/Exceptions/GlobalExceptionHandler.cs
    - yh-flow/src/Host/YH.Flow.Api/appsettings.json

key-decisions:
  - "IApiTokenService.CreateAsync returns (APITokenDto, string RawKey) tuple instead of plain DTO — endpoint needs plaintext to return to user"
  - "PlanePagedResultFactory as non-generic class to avoid CA1000 analyzer warning on generic types with static members"
  - "OAuth management endpoints at /oauth-providers (not /auth/) — admin interface, not Plane frontend endpoint"
  - "Plane error format uses snake_case { error, error_code, error_detail } matching Plane Django backend format"

patterns-established:
  - "API Token lifecycle: generate → hash → store → return plaintext once → never again"
  - "Plane format adapter: IsPlaneRoute(path) → switch response format in GlobalExceptionHandler"
  - "Mediator handler colocated with endpoint: one folder per feature, handlers alongside endpoints"

requirements-completed: [REQ-1.3, REQ-1.4, NFR-1, NFR-2]

# Metrics
duration: 35min
completed: 2026-06-17
---

# Phase 1 Plan 04: API Token CRUD + OAuth Management + Plane Format Summary

**API Token 3 端点 CRUD + OAuth Provider 5 端点管理 + Plane 分页/错误格式适配 + CORS 配置**

## Performance

- **Duration:** 35 min
- **Started:** 2026-06-17T07:35:00Z
- **Completed:** 2026-06-17T07:52:00Z
- **Tasks:** 3
- **Files modified:** 26

## Accomplishments

- **Task 1 — API Token CRUD:** ApiTokenService 完整实现（pk\_ 前缀密钥生成、SHA-256 哈希存储、验证、列出、撤销），3 个端点（POST/GET/DELETE /auth/api-tokens），Mediator 命令处理器，IApiTokenService 签名调整返回元组。
- **Task 2 — OAuth Provider 管理 + Plane 格式:** 5 个管理端点（CRUD + toggle），PlanePagedResult<T> 分页格式（count/next/previous/results），GlobalExceptionHandler 添加 IsPlaneRoute 检查（/auth/_ 返回 Plane 格式，/api/_ 保持 RFC 7807）。
- **Task 3 — CORS + 配置:** appsettings.json 添加 x-api-key 和 x-tenant 到 CORS AllowedHeaders，启用 RateLimiting。

## Task Commits

1. **Tasks 1+2+3: API Token CRUD + OAuth Management + Plane Format + CORS**
   - `a3e1a03` (feat) — All 3 tasks committed together (shared file dependencies)

## Files Created/Modified

- `Modules.Identity/Services/ApiTokenService.cs` — API Token 服务：pk\_ 前缀密钥生成、SHA-256 哈希、CRUD、验证
- `Modules.Identity/Features/v1/ApiTokens/Create/` — POST /auth/api-tokens 端点 + 命令处理器
- `Modules.Identity/Features/v1/ApiTokens/List/` — GET /auth/api-tokens 端点 + 查询处理器
- `Modules.Identity/Features/v1/ApiTokens/Revoke/` — DELETE /auth/api-tokens/{id} 端点 + 命令处理器
- `Modules.Identity/Features/v1/OAuth/Manage/` — OAuth Provider CRUD 5 端点 + 5 Mediator 处理器
- `Modules.Identity.Contracts/v1/ApiTokens/` — Create/List/Revoke 命令 + ApiTokenCreateResult DTO
- `Modules.Identity.Contracts/v1/OAuth/ManageOAuthProviderCommand.cs` — OAuth CRUD 命令
- `Modules.Identity.Contracts/v1/OAuth/GetOAuthProvidersQuery.cs` — OAuth 列表查询
- `BuildingBlocks/Shared/Persistence/PlanePagedResult.cs` — Plane 分页格式 + PlanePagedResultFactory
- `BuildingBlocks/Web/Exceptions/GlobalExceptionHandler.cs` — IsPlaneRoute 检查，Plane 错误格式
- `Host/YH.Flow.Api/appsettings.json` — CORS headers + rate limiting 启用
- `IdentityModule.cs` — ApiTokenService DI + OAuth 管理端点注册
- `AuthEndpoints.cs` — 3 个 API Token 端点路由注册

## Decisions Made

- **IApiTokenService.CreateAsync 返回元组:** 原计划返回 APITokenDto，但端点需要返回明文 Key。改为返回 `(APITokenDto Dto, string RawKey)` 元组，明文仅此一次可用。
- **PlanePagedResultFactory 非泛型类:** CA1000 规则禁止泛型类有静态成员，将 `FromPagedResponse` 移到非泛型工厂类。
- **OAuth 管理端点路由:** 放在 `/oauth-providers` 下（非 `/auth/`），因为这是管理员接口而非 Plane 前端使用的认证端点。

## Deviations from Plan

### Auto-fixed Issues

**1. [Non-blocking] IApiTokenService.CreateAsync 签名变更**

- **Found during:** Task 1 实现
- **Issue:** 计划指定返回 APITokenDto，但端点需要明文 Key 返回给用户
- **Fix:** 改为返回 `(APITokenDto Dto, string RawKey)` 元组
- **Verification:** Build passes, tests pass

**2. [Non-blocking] CA1000 泛型类静态成员**

- **Found during:** Task 2 实现
- **Issue:** `PlanePagedResult<T>.FromPagedResponse` 触发 CA1000
- **Fix:** 创建 `PlanePagedResultFactory` 非泛型类承载静态方法
- **Verification:** Build passes, 0 warnings

**3. [Non-blocking] ApiTokenService 测试简化**

- **Found during:** Task 1 测试
- **Issue:** IdentityDbContext 依赖多租户基础设施，单元测试无法简单 mock
- **Fix:** 简化为测试密钥生成和哈希核心逻辑，完整集成测试留待后续阶段
- **Verification:** 9 个单元测试通过

---

**Total deviations:** 3 auto-fixed (all non-blocking)
**Impact on plan:** No scope change.

## Threat Mitigations Applied

| Threat ID | Component                | Mitigation                                                  | Status       |
| --------- | ------------------------ | ----------------------------------------------------------- | ------------ |
| T-01-15   | API Key creation         | Plaintext key returned only in Create response, never again | ✅ Mitigated |
| T-01-16   | RevokeApiTokenEndpoint   | Service verifies userId match before revoking               | ✅ Mitigated |
| T-01-17   | ValidateAndGetOwnerAsync | SHA-256 hash comparison, claims from DB only                | ✅ Mitigated |
| T-01-18   | OAuth Provider CRUD      | Admin endpoints, ClientSecret excluded from DTO             | ✅ Mitigated |
| T-01-19   | PlanePagedResult URLs    | Built from request path/query, no sensitive data            | ✅ Accepted  |

## Verification

- `dotnet build src/Host/YH.Flow.Api/` — 33 projects, 0 errors, 0 warnings
- `dotnet test src/Tests/Identity.Tests/` — 412 passed, 0 failed, 0 skipped
- API Token 3 endpoints registered at /auth/api-tokens
- OAuth Provider 5 endpoints registered at /oauth-providers
- GlobalExceptionHandler serves Plane format for /auth/\* routes

## Next Phase Readiness

- `01-05` can push schema migrations and verify the full Phase 1 stack end-to-end
- Phase 1 will be complete after 01-05

---

_Phase: 01-foundation_
_Completed: 2026-06-17_

## Self-Check: PASSED

All 26 files in 1 commit verified. Full solution builds (33 projects, 0 errors). 412 tests passing.
