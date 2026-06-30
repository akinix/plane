# Phase 21: Infrastructure Foundation - Context

**Gathered:** 2026-06-30
**Status:** Ready for planning
**Mode:** Auto-generated (infrastructure phase — discuss skipped)

<domain>
## Phase Boundary

前端 API 通信层正确处理请求/响应格式转换、Token 生命周期管理和集中式查询键管理。

具体范围：

1. **FlowApiService** 请求拦截器添加 `humps.decamelizeKeys`，对所有 POST/PUT/PATCH 请求体进行 camelCase→snake_case 转换（含嵌套对象和查询字符串）
2. **Token 刷新队列** — 单例 refresh promise + 请求队列，防止并发 401 时重复刷新和强制登出
3. **集中式查询键工厂** — `query-keys.ts`，统一管理所有模块的 TanStack Query 缓存键
4. **错误格式标准化** — 在响应拦截器中统一提取 ProblemDetails 和 Plane 格式错误消息，包括 FluentValidation 字段级错误映射
5. **分页提取层** — Service 层自动提取 `PlanePagedResult.results[]`，hooks 继续返回 `T[]` 数组

不涉及业务模块的 Service 或 hooks 改造——那些在后阶段处理。

</domain>

<decisions>
## Implementation Decisions

### Claude's Discretion

All implementation choices are at Claude's discretion — pure infrastructure phase. Use ROADMAP phase goal, success criteria, codebase conventions, and existing patterns to guide decisions.

Key codebase context:

- `FlowApiService` at `clients/web/src/lib/services/flow-api.service.ts` is the base class for all services
- Current request interceptor attaches JWT Bearer token only
- Current response interceptor does `humps.camelizeKeys()` on responses only
- Current 401 handler redirects to sign-in (no refresh queue)
- Current error handling extracts `error || title || detail` (minimal)
- Services currently use mock data pattern (non-FlowApiService base class)
- No query key factory exists yet — hooks use inline string keys

</decisions>

<code_context>

## Existing Code Insights

### Key Files

- `clients/web/src/lib/services/flow-api.service.ts` — Base API service class (target for INFRA-01, INFRA-02, INFRA-04)
- `clients/web/src/lib/services/auth.service.ts` — AuthService (has `refreshToken()` method for INFRA-02)
- `clients/web/src/lib/constants/endpoints.ts` — API endpoint constants

### Established Patterns

- Axios-based HTTP client with request/response interceptors
- JWT Bearer token stored in localStorage
- `humps` library already a dependency (used on response side)
- Services extend `FlowApiService` (or standalone mock service classes)
- Hooks use TanStack Query (v5) with string query keys

### Integration Points

- `FlowApiService` is the base class — changes here propagate to all subclasses
- Token refresh endpoint: `POST /auth/refresh/` (already exists in AuthService)
- Response interceptor is the single point for error standardization
- Paginated responses use `PlanePagedResult` envelope (`.results[]` array)

</code_context>

<specifics>
## Specific Ideas

No specific requirements — infrastructure phase. Implementation guided by ROADMAP success criteria and INFRA requirements.

</specifics>

<deferred>
## Deferred Ideas

None — discussion skipped.
</deferred>
