---
phase: 21-infrastructure-foundation
plan: 01
subsystem: infra
tags: [axios, humps, token refresh, error handling, interceptors]

requires:
  - phase: 14-auth
    provides: FlowApiService base class, JWT Bearer auth, AuthService with refreshToken

provides:
  - 请求拦截器自动转换 camelCase -> snake_case（POST/PUT/PATCH 请求体和查询参数）
  - 并发安全的 Token 刷新队列（避免重复刷新，排队请求自动恢复）
  - 统一 API 错误标准化（ProblemDetails / PlaneError / FluentValidation 字段级错误）
  - 标准化的前端错误消费类型定义

affects: [phase 22, phase 23, phase 24, phase 25, phase 26, phase 27]

tech-stack:
  added: []
  patterns:
    - "函数注入模式：registerRefreshHandler 避免 AuthService 与 FlowApiService 循环依赖"
    - "刷新队列：单例 refresh promise + 请求队列 + processQueue 批量处理"
    - "错误标准化工厂：standardizeApiError 统一提取 ProblemDetails、PlaneError 等格式"

key-files:
  created:
    - yh-flow/clients/web/src/lib/types/api-error.ts
  modified:
    - yh-flow/clients/web/src/lib/types/index.ts
    - yh-flow/clients/web/src/lib/services/flow-api.service.ts
    - yh-flow/clients/web/src/lib/services/auth.service.ts

key-decisions:
  - "使用 @plane/types 路径别名导入（而非 @/lib/types），因 vite.config.ts 中 @plane/types -> src/lib/types"
  - "模块级 Token 刷新状态（isRefreshing/failedQueue/refreshHandler）保持为模块变量，非类字段，避免子类继承问题"
  - "refreshHandler 使用 async/await 替代 Promise chain，通过 oxlint 的 promise/always-return 和 no-promise-in-callback 规则"
  - "axiosInstance 从 private 改为 protected，便于子类在必要时访问 Axios 实例"
  - "标准 ErrorResponse 接口保留以供向后兼容（已有使用者）"

patterns-established:
  - "registerRefreshHandler 函数注入：AuthService 构造时注册刷新处理函数，FlowApiService 模块级变量持有引用"
  - "刷新队列：isRefreshing 标志 + failedQueue 数组 + refreshPromise 单例 + processQueue 批处理"
  - "错误拦截：standardizeApiError 检测 ProblemDetails（type/title/detail）vs PlaneError（error）vs 未知格式"

requirements-completed: [INFRA-01, INFRA-02, INFRA-04]

duration: 35min
completed: 2026-06-30
---

# Phase 21 Plan 01: FlowApiService 拦截器重构 总结

**请求体 camelCase→snake_case 转换、并发安全 Token 刷新队列、统一 API 错误标准化——FlowApiService 基础设施三层重构**

## Performance

- **Duration:** 35 min
- **Started:** 2026-06-30 current session
- **Completed:** 2026-06-30
- **Tasks:** 3
- **Files modified:** 4

## Accomplishments

- **标准化 API 错误类型定义**：创建 `api-error.ts`，含 ProblemDetails (RFC 7807)、PlaneError、ApiFieldError、StandardizedApiError 接口和 `standardizeApiError` 工具函数，统一处理五种错误输入格式
- **请求拦截器蛇形转换**：FlowApiService 请求拦截器对 POST/PUT/PATCH 请求体和查询参数自动执行 `humps.decamelizeKeys`（跳过 FormData 和字符串请求体），支持嵌套对象递归转换（INFRA-01）
- **Token 刷新队列**：模块级刷新状态（isRefreshing/failedQueue/refreshPromise），并发 401 仅触发一次 Token 刷新，排队请求自动恢复，`/auth/refresh/` 自身 401 直接登出避免死循环（INFRA-02）
- **错误响应标准化**：响应拦截器 error 回调自动通过 `standardizeApiError` 转换错误数据为前端统一消费格式（INFRA-04）
- **AuthService 注入**：AuthService 构造时通过 `registerRefreshHandler` 注册刷新函数，避免循环依赖

## Task Commits

Each task was committed atomically:

1. **Task 1: 创建标准化 API 错误类型定义文件** - `c65f760d1` (feat)
2. **Task 2: 请求拦截器添加 humps.decamelizeKeys 转换 (INFRA-01)** - `425c7f718` (feat)
3. **Task 3: 响应拦截器 Token 刷新队列 (INFRA-02) + 错误标准化 (INFRA-04)** - `47f8b0fa8` (feat)

## Files Created/Modified

- `yh-flow/clients/web/src/lib/types/api-error.ts` - **创建** — 标准化 API 错误类型定义（ProblemDetails、PlaneError、ApiFieldError、StandardizedApiError）及标准化工场函数
- `yh-flow/clients/web/src/lib/types/index.ts` - **修改** — 添加 `export * from "./api-error"` 导出
- `yh-flow/clients/web/src/lib/services/flow-api.service.ts` - **修改** — 重写请求拦截器（humps.decamelizeKeys）和响应拦截器（Token 刷新队列 + 错误标准化），保留向后兼容的 camelizeKeys
- `yh-flow/clients/web/src/lib/services/auth.service.ts` - **修改** — 构造时注册 registerRefreshHandler，导入 registerRefreshHandler

## Decisions Made

- **路径别名使用 `@plane/types`**：计划指定 `@/lib/types/api-error` 路径，但该项目中 `@/*` -> `app/*`，`@plane/types` -> `src/lib/types`。使用 `@plane/types` 与现有导入模式（如 auth.service.ts 中 `from "@plane/types"`）一致
- **模块级变量 vs 类字段**：Token 刷新状态（isRefreshing/failedQueue/refreshHandler）声明为模块级变量而非 FlowApiService 类字段。这是有意的设计选择——类字段会在子类中多重注册，模块级变量确保所有 FlowApiService 实例共享单一刷新队列，符合拦截器基础设施的单例语义
- **async/await 替代 Promise chain**：刷新处理使用 `async/await` 而非 `.then().catch()`，避免 oxlint `promise/always-return` 和 `no-promise-in-callback` 规则报警
- **axiosInstance 改为 protected**：从 `private` 改为 `protected`，保持子类扩展能力（虽然当前无子类直接使用，但匹配 open/closed 原则）
- **ErrorResponse 接口保留**：原 `ErrorResponse` 接口仍导出以保持向后兼容

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] 导入路径修正**

- **Found during:** Task 2（请求拦截器实现）
- **Issue:** 计划中导入路径 `@/lib/types/api-error` 解析为 `app/lib/types/api-error`（不存在），实际类型文件在 `src/lib/types/` 下
- **Fix:** 改为 `@plane/types`，与项目中现有导入模式一致（`@plane/types` -> `src/lib/types`）
- **Files modified:** `yh-flow/clients/web/src/lib/services/flow-api.service.ts`
- **Verification:** `npx tsc --noEmit` 通过
- **Committed in:** 425c7f718 (Task 2 commit)

**2. [Rule 1 - Bug] TypeScript 类型错误：`message` 字段期望 `string` 但得到 `unknown`**

- **Found during:** Task 1（api-error.ts 创建）
- **Issue:** `Record<string, unknown>` 上的 `??` 操作符返回 `unknown`，无法赋值给 `StandardizedApiError.message: string`，导致 TS2322 错误
- **Fix:** 使用 `String(...)` 包装确保返回值始终为 `string`
- **Files modified:** `yh-flow/clients/web/src/lib/types/api-error.ts`
- **Verification:** `npx tsc --noEmit` 通过
- **Committed in:** c65f760d1 (Task 1 commit)

**3. [Rule 1 - Bug] oxlint pre-commit hook 报警**

- **Found during:** Task 2/3 提交时
- **Issue:**
  - `StandardizedApiError` 类型导入未被直接使用
  - `refreshHandler().then().catch()` 触发了 `promise/always-return` 和 `no-promise-in-callback` 规则
  - `axios.create()` 触发了 `import/no-named-as-default-member` 规则
  - `this.refreshToken().then(() => {})` 触发了 `promise/always-return` 规则
- **Fix:** 移除未使用的类型导入；改用 `async/await` 重构刷新处理；为 `axios.create` 添加行内 lint disable 注释；用 `async` 函数包装 `registerRefreshHandler`
- **Files modified:** `yh-flow/clients/web/src/lib/services/flow-api.service.ts`, `yh-flow/clients/web/src/lib/services/auth.service.ts`
- **Verification:** 提交通过 pre-commit hook
- **Committed in:** 425c7f718, 47f8b0fa8

---

**Total deviations:** 3 auto-fixed (3 Rule 1 - Bug)
**Impact on plan:** 所有自动修复必要且无副作用。无范围蔓延。

## Issues Encountered

- **Vite 构建失败（预存在问题）**：`npx vite build` 失败，报错 `ENOENT: no such file or directory 'app/app/auth/sign-in/page.tsx'`。此为预先存在的 React Router 配置问题（路径重复 `app/app/`），与本次更改无关。`npx tsc --noEmit` 编译检查通过，无新增 TypeScript 错误。

## User Setup Required

None - 本次更改不涉及外部服务配置。

## Next Phase Readiness

- FlowApiService 基础设施改造完成，可直接在后续阶段（Workspace & Project、WorkItems、Cycles & Modules 等）中使用
- Token 刷新队列的端点 `/auth/refresh/` 后端需确保正常工作（已在 AuthService 中存在）
- 响应拦截器的 `standardizeApiError` 会在错误响应上自动运行，其他服务无需额外适配
- 请求拦截器的 `humps.decamelizeKeys` 对所有 POST/PUT/PATCH 请求体自动生效，子类无需修改

---

_Phase: 21-infrastructure-foundation_
_Completed: 2026-06-30_
