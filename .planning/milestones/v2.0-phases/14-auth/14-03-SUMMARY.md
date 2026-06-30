---
phase: 14-auth
plan: 03
subsystem: flow-web-api-adapter
tags:
  - api-adapter
  - jwt-auth
  - vite-proxy
  - snake-case
  - flow-web
requires:
  - 14-01 (Frontend scaffolding)
  - 14-02 (Package fork)
provides:
  - SCAFF-05 (Vite proxy)
  - SCAFF-06 (SnakeCase serialization)
  - SCAFF-07 (JWT Bearer interceptor)
  - SCAFF-08 (humps camelCase conversion)
  - SCAFF-09 (Pagination type adapter)
affects:
  - yh-flow/src/Host/YH.Flow.Api/Program.cs
tech-stack:
  added:
    - axios (HTTP client)
    - humps (snake_case/camelCase conversion)
  patterns:
    - Axios interceptor pattern (request + response)
    - Singleton service class pattern extending abstract base
key-files:
  created:
    - clients/web/src/lib/services/flow-api.service.ts
    - clients/web/src/lib/services/auth.service.ts
    - clients/web/src/lib/services/index.ts
    - clients/web/src/lib/types/error.ts
    - clients/web/src/lib/types/pagination.ts
    - clients/web/src/lib/types/auth.ts
    - clients/web/src/lib/types/index.ts
    - clients/web/vite.config.ts
    - clients/web/.env
  modified:
    - yh-flow/src/Host/YH.Flow.Api/Program.cs
decisions:
  - "Auth types use camelCase to match humps post-camelization output (D-12)"
  - "Token refresh strategy uses /auth/refresh/ endpoint (deferred to implementation time per D-11 Claude's discretion)"
  - ".env file committed to repo for developer convenience (standard Vite convention)"
metrics:
  duration: 35m
  completed_date: "2026-06-26"
---

# Phase 14 Plan 03: API 适配层 (FlowApiService + Vite 代理 + SnakeCase) 摘要

构建 Flow Web 前端的 API 适配层：创建 FlowApiService 基类（JWT Bearer 认证、humps 转换、错误标准化），AuthService（邮箱登录/注册/重置密码/刷新 Token），配置 Vite 开发代理转发到 .NET API，以及 .NET API 的 SnakeCase JSON 序列化。

## 任务执行

### Task 1: 创建 FlowApiService 基类

**文件创建：**

- `clients/web/src/lib/services/flow-api.service.ts` — 核心基类，包含：
  - Request 拦截器：自动附加 `Authorization: Bearer <token>`（D-11）
  - Response 拦截器：humps.camelizeKeys 将 snake_case 转换为 camelCase（D-12）
  - 错误拦截器：将 .NET 标准错误格式统一为 `{error: string}`（D-13）
  - 401 自动处理：清除 token + 跳转登录页
  - 方法签名与 Plane APIService 保持一致（get/post/put/patch/delete/request）
- `clients/web/src/lib/services/auth.service.ts` — AuthService 扩展 FlowApiService，提供：
  - emailCheck、signIn、signUp、sendResetPasswordLink、resetPassword、signOut、refreshToken
  - 登录/注册成功后自动将 JWT token 存入 localStorage
- `clients/web/src/lib/services/index.ts` — barrel export
- `clients/web/src/lib/types/error.ts` — ErrorResponse 类型
- `clients/web/src/lib/types/pagination.ts` — PlanePagedResult 分页适配类型
- `clients/web/src/lib/types/auth.ts` — Auth 类型定义（camelCase，匹配 humps 输出）
- `clients/web/src/lib/types/index.ts` — @plane/types 别名 barrel

**类型适配说明：**
由于 D-04 将 `@plane/types` 映射到 `src/lib/types`，而 humps 拦截器会自动将 .NET API 的 snake_case 响应转换为 camelCase，本地类型定义使用 camelCase 字段名（如 `accessToken` 而非 `access_token`），与前端消费代码一致。

**Commit:** `aaa513c17` — feat(14-auth-03): create FlowApiService base class and AuthService with JWT Bearer

### Task 2: 配置 Vite 代理和 .NET API SnakeCase 设置

**前端配置：**

- `clients/web/vite.config.ts`：
  - 添加 Vite resolve aliases 将 `@plane/*` 映射到 `src/lib/*`（D-04）
  - 添加 dev server proxy：`/api` → `https://localhost:7030`（SCAFF-05）
- `clients/web/.env`：
  - `VITE_API_BASE_URL=http://localhost:5173/api/v1`
  - `VITE_API_PROXY_TARGET=https://localhost:7030`

**后端配置：**

- `yh-flow/src/Host/YH.Flow.Api/Program.cs`：
  - `ConfigureHttpJsonOptions` 中添加 `JsonNamingPolicy.SnakeCaseLower`（SCAFF-06）
  - 添加 `JsonIgnoreCondition.WhenWritingNull`（SCAFF-06）
  - 新增 `Configure<Mvc.JsonOptions>` 同样配置，覆盖 `[ApiController]` 端点

**Commit:** `ae528af22` — feat(14-auth-03): configure Vite proxy and .NET API SnakeCase serialization

## 验证结果

```text
# TypeScript 编译
cd clients/web && npx tsc --noEmit  # Passed (no errors)

# .NET 构建
dotnet build yh-flow/src/YH.Flow.slnx  # 0 warnings, 0 errors
```

## 关键决策

1. **Auth 类型使用 camelCase**：与 humps 转换后的输出一致，前端代码直接使用 `accessToken` 而非 `access_token`
2. **Token 刷新端点**：使用 `/auth/refresh/` POST 端点，具体刷新策略（静默 vs 弹出）留待后续实现
3. **.env 文件提交**：按标准 Vite 项目惯例提交 .env（非 .env.local），方便开发者快速启动

## Deviations from Plan

None — plan executed exactly as written.

## 附加的脚手架文件

为支持 TypeScript 编译，还创建了以下不在计划范围内的文件（Rule 2 — 必要基础设施）：

- `clients/web/package.json` — axios、humps、typescript、vite 依赖
- `clients/web/tsconfig.json` — TypeScript 配置，含 @/_ 和 @plane/_ paths 映射

## Threat Flags

None — 新创建的 API 适配层运行在浏览器端，未引入新的攻击面。T-14-03（localStorage JWT）和 T-14-04（Axios interceptor）已按威胁缓解计划实现。
