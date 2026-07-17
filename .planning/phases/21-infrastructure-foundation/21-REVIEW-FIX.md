---
phase: 21
fixed_at: 2026-06-30T17:50:00.000Z
review_path: .planning/phases/21-infrastructure-foundation/21-REVIEW.md
iteration: 1
findings_in_scope: 6
fixed: 6
skipped: 0
status: all_fixed
---

# Phase 21: Infrastructure Foundation Code Review Fix Report

**Fixed at:** 2026-06-30T17:50:00.000Z
**Source review:** .planning/phases/21-infrastructure-foundation/21-REVIEW.md
**Iteration:** 1

**Summary:**

- Findings in scope: 6
- Fixed: 6
- Skipped: 0

## Fixed Issues

### CR-01: Token 刷新队列竞态条件

**Files modified:** `yh-flow/clients/web/src/lib/services/flow-api.service.ts`
**Commit:** d68fffac3
**Applied fix:** 将 `refreshHandler` 空值检查移到设置 `isRefreshing` 之前，然后使用立即执行的 async IIFE 原子化赋值 `isRefreshing = true` 和 `refreshPromise = (async () => {...})()`。消除了 `isRefreshing = true` 和 `refreshPromise = doRefresh()` 之间的竞态窗口（另一个并发请求可能在间隙检查到 `refreshPromise === null` 并绕过队列）。

### CR-02: notificationKeys 前缀不一致

**Files modified:** `yh-flow/clients/web/src/lib/services/query-keys.ts`, `yh-flow/clients/web/src/lib/hooks/use-notifications.ts`
**Commit:** 533ceac93
**Applied fix:** 将 `notificationKeys.list()` 从 `["workspace-notifications", workspaceId]` 改为 `["notifications", "workspace", workspaceId]`；将 `unreadCount()` 从 `["workspace-unread-count", workspaceId]` 改为 `["notifications", "unread-count", workspaceId]`。同时同步更新了 `use-notifications.ts` 中全部 5 处硬编码的旧 key 值。现在 `notificationKeys.all()` 返回的 `["notifications"]` 是所有子键的前缀，`invalidateQueries({ queryKey: notificationKeys.all() })` 能正确级联失效所有通知查询。

### WR-01: getList 中 params 覆盖问题

**Files modified:** `yh-flow/clients/web/src/lib/services/flow-api.service.ts`
**Commit:** 49f8b7d7b
**Applied fix:** 在 `getList()` 方法中，将 `config.params` 与显式 `params` 参数合并，使用 `{ ...config, params: { ...(config.params || {}), ...params } }` 确保显式参数始终优先于 `config.params`，避免 `{ params, ...config }` 展开时 `config.params` 覆盖显式 `params`。

### WR-02: unwrapPaginated 类型安全

**Files modified:** `yh-flow/clients/web/src/lib/services/flow-api.service.ts`
**Commit:** 65abfea23
**Applied fix:** 将 `unwrapPaginated` 方法的最后一条 `return data`（当 `data` 既不是 `Array.isArray(data.results)` 也不是 `Array.isArray(data)` 时）改为 `return []`。同时更新了 JSDoc 注释。确保返回类型 `T[]` 始终是数组，避免调用方在非数组数据上调用 `.map()` 等方法时报错。

### WR-03: auth.service.ts catch 块抛出 undefined

**Files modified:** `yh-flow/clients/web/src/lib/services/auth.service.ts`
**Commit:** 79454a5a0
**Applied fix:** 将全部 6 处 `catch` 块中的 `throw err?.response?.data` 改为 `throw err?.response?.data ?? err`。当网络错误导致 `err.response` 为 `undefined` 时，原始的错误对象 `err` 会被作为兜底抛出，而不是抛出 `undefined`。

### WR-04: standardizeApiError JSON.stringify 循环引用

**Files modified:** `yh-flow/clients/web/src/lib/types/api-error.ts`
**Commit:** f38c1e469
**Applied fix:** 添加 `safeStringify()` 辅助函数，用 try-catch 包裹 `JSON.stringify()`，捕获 `TypeError: Converting circular structure to JSON` 异常并返回 `String(value)` 作为兜底。将 fallback 路径中的 `JSON.stringify(data)` 替换为 `safeStringify(data)`。

---

_Fixed: 2026-06-30T17:50:00.000Z_
_Fixer: Claude (gsd-code-fixer)_
_Iteration: 1_
