---
status: passed
phase: 21
name: Infrastructure Foundation
date: 2026-06-30
---

# Phase 21: Infrastructure Foundation — Verification

## Result: ✅ PASSED

### Success Criteria Verification

| # | Criterion | Status | Evidence |
|---|-----------|--------|----------|
| 1 | POST/PUT/PATCH 请求体自动 camelCase→snake_case 转换 | ✅ | `flow-api.service.ts` request interceptor: `humps.decamelizeKeys(config.data)` + query params |
| 2 | 并发 401 触发单次 Token 刷新，排队请求恢复 | ✅ | `refreshPromise` + `failedQueue` + `processQueue` + `registerRefreshHandler` in `flow-api.service.ts` |
| 3 | 集中式 `query-keys.ts` 工厂 | ✅ | All 13 modules (workspace→analytics) with `as const` tuples in `query-keys.ts` |
| 4 | 错误格式标准化 (ProblemDetails/Plane/FluentValidation) | ✅ | `api-error.ts` + `standardizeApiError` in response interceptor |
| 5 | Service 层自动解包分页结果 | ✅ | `getList<T>()`/`unwrapPaginated<T>()` in `flow-api.service.ts` + `pagination.ts` types |

### Requirements Completion

| REQ-ID | Status | Notes |
|--------|--------|-------|
| INFRA-01 | ✅ | `decamelizeKeys` on request body + query params |
| INFRA-02 | ✅ | Token refresh queue with singleton promise |
| INFRA-03 | ✅ | `query-keys.ts` factory (13 modules) |
| INFRA-04 | ✅ | `standardizeApiError` handles ProblemDetails + PlaneError + FluentValidation |
| INFRA-05 | ✅ | `getList`/`getOne`/`postList`/`unwrapPaginated` in FlowApiService |

### Code Review

- 2 Critical + 4 Warning findings identified
- All 6 findings fixed and committed
- Re-review: all clean

### Files Modified/Created

- `yh-flow/clients/web/src/lib/services/flow-api.service.ts` — Interceptors + pagination methods
- `yh-flow/clients/web/src/lib/services/auth.service.ts` — Token refresh handler registration
- `yh-flow/clients/web/src/lib/services/query-keys.ts` — New: centralized query key factory
- `yh-flow/clients/web/src/lib/types/api-error.ts` — New: standardized error types
- `yh-flow/clients/web/src/lib/types/pagination.ts` — New: pagination response types
- `yh-flow/clients/web/src/lib/types/index.ts` — Re-exports
- `yh-flow/clients/web/src/lib/hooks/use-notifications.ts` — Updated query key prefix

### Build Verification

- TypeScript compilation: ✅ No new errors introduced (pre-existing errors unchanged)
- All existing HTTP methods preserved (backward compatible)
