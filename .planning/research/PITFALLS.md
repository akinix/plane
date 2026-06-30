# Domain Pitfalls: Mock Data to Real .NET API Integration

**Domain:** React + TanStack Query 前端从 Mock 数据切换到 .NET Minimal API 后端
**Researched:** 2026-06-30
**Overall confidence:** HIGH

## Critical Pitfalls

Mistakes that cause silent data corruption, infinite loops, or full-page blank screens.

### Pitfall 1: 请求体 camelCase / snake_case 不匹配（静默失败）

**What goes wrong:** .NET 后端全局配置了 `JsonNamingPolicy.SnakeCaseLower`，期望接收 snake_case 字段（如 `workspace_id`, `sort_order`）。当前 FlowApiService 的响应拦截器有 `humps.camelizeKeys` 将后端返回的 snake_case 转成前端 TypeScript 类型的 camelCase，但**请求拦截器中完全没有 `humps.decamelizeKeys`**。这意味着所有 POST/PUT/PATCH 请求的 JSON body 将以 camelCase 发送（React/TypeScript 类型自然使用 camelCase 字段），而后端将无法正确反序列化。

**Why it happens:** Mock 数据阶段不需要发送真实 HTTP 请求，所以脱掉请求转换从来没有被暴露。AuthService.signIn() 目前是唯一发送真实请求的地方，且因为 auth 端点使用 `PlaneAuthResponse` 手动指定 JsonPropertyName，请求体也可能由 ASP.NET 自动绑定（不区分大小写默认开启），所以一直没有报错。切换到真实后端后所有 mutation 会静默失败——后端收到 `sortOrder: 10000` 而非 `sort_order: 10000`，EF Core SaveChanges 会跳过该字段。

**Consequences:** 所有写入操作（创建 Issue、更新 Cycle、修改 Page 等）的字段将被静默忽略。可能造成：

- 创建记录时默认值写入
- 更新时字段不变，用户以为保存成功
- 数据校验验证通过但字段未被写入

**Prevention:**

1. 在 FlowApiService 请求拦截器中添加 `humps.decamelizeKeys(config.data)` 和 `humps.decamelizeKeys(config.params)`
2. 跳过 `FormData` 和已经手动构造的 snake_case payload
3. 写一个集成测试验证：发送 camelCase body → 后端接收 snake_case → 返回 snake_case 响应 → 前端收到 camelCase

**Detection:**

- 比对 WireShark/浏览器 Network tab 中的 request payload 和后端 Application Insights 的 request body
- 在请求拦截器中加 console.debug 打印转换前后的 data
- 后端开启敏感数据日志查 EF Core 生成的 SQL

---

### Pitfall 2: 401 Token 刷新竞态条件导致循环登出

**What goes wrong:** 当前 FlowApiService 的响应拦截器在收到 401 时直接 `removeToken()` + `window.location.href = "/auth/sign-in"`。切换到真实后端后，当 access token 过期且页面同时发起多个 API 请求（如仪表板同时加载 Workspace、Project、Issue 列表），**所有请求都会返回 401**，第一个 401 触发跳转登录页，后面还在处理中的请求可能在跳转后继续触发错误，造成页面闪烁甚至白屏。

更深层的问题：`AuthService.refreshToken()` 方法已存在但**从未被调用**。拦截器没有尝试刷新 token 就立即登出。

**Why it happens:** Mock 数据阶段没有真实 token 过期场景。切换到真实后端后：

1. 页面初始化时多个查询同时触发
2. Token 已过期，全部返回 401
3. 第一个 401 → 清除 token → 页面跳转登录
4. 第二个 401 race → 可能继续执行 or 在已卸载的组件上 setState
5. 用户被强制登出，即使 refresh token 仍然有效

**Consequences:** 用户频繁被登出、无法完成正常的工作流程、token 完全浪费。

**Prevention:**

1. **必须实现**请求队列 + 单例 refresh promise 模式（不要重复刷新 token）
2. 拦截器逻辑改为：
   - 收到 401 → 标记 `_retry` → 检查是否正在刷新（是则排队等待）
   - 不是则发起 refresh token 请求
   - refresh 成功 → 存储新 token → 重放队列中所有等待请求
   - refresh 失败 → 才清除 token 并跳转登录
3. refresh 端点本身要排除在 401 拦截之外（否则循环）
4. 设置 refresh 请求超时（`timeout: 10000`），防止挂起阻塞队列

**Detection:**

- 页面初始化时在 Network tab 看到一片红色 401
- 用户反馈"刚登录就被踢出"
- 控制台出现"Failed to execute 'removeChild' on 'Node'"（组件已卸载）

---

### Pitfall 3: TanStack Query 的 `staleTime: 0` 默认值 + 模拟开发的 retry 假象

**What goes wrong:** 当前 QueryClient 全局设置 `staleTime: 5 * 60 * 1000`（5 分钟）和 `retry: 1`，看起来合理。但**几乎所有 hook 的 `queryFn` 都直接访问 `MOCK_*` 数组**——不需要 await 网络请求（虽然有 `delay()` 但数据是内存即时返回的）。切换到真实后端后：

- Query 会立刻进入 loading 状态（之前 delay 模拟但从未真正 loading）
- 网络延迟（200-800ms）暴露了之前没有 loading/error 处理的 UI 缺陷
- `retry: 1` 意味着每个失败请求会自动重试，如果后端返回 500 会延迟 UI 进入错误状态
- 组件中没有 `isLoading`/`isError` 的分支处理（mock 数据从不 loading/error）

**Why it happens:** Mock 阶段的 `queryFn` 使用 `delay(ms)` + 内存数据，`delay` 期间 React Query 将 `isLoading` 设为 true，但时间太短（150-300ms）UI 不会闪烁。真实后端延迟不定，且会有 4xx/5xx 错误。

**Consequences:** 用户在页面加载时看到闪烁的 loading spinner、空内容、或者空白页。某些列表页面在 API 响应前展示"无数据"状态。

**Prevention:**

1. **先完成一个模块的完整 API 集成再推广到其他模块**——不要一次性换所有 hook
2. 每个 hook 切换后确认 UI 有 `isPending`、`isError`、`isEmpty` 三态处理
3. 全局 QueryClient 增加 `queryCache.onError` 处理未捕获的错误
4. 开发阶段设置 `retry: 0` 方便调试，生产恢复 `retry: 1`
5. 使用 `placeholderData: keepPreviousData` 避免切换参数时闪白

**Detection:**

- 切换一个 hook 后查看 Network tab 请求瀑布流
- 检查组件是否有 `if (isPending) return <Loader />` 代码
- 检查是否有 global error boundary

---

### Pitfall 4: 后端错误格式双轨制（ProblemDetails vs Plane 格式）导致前端错误处理不完整

**What goes wrong:** 后端 `GlobalExceptionHandler` 对非 `/auth` 路由返回 RFC 7807 `ProblemDetails`（格式：`{ type, title, status, detail, instance, traceId, correlationId, errors? }`），对 `/auth` 路由返回 Plane 兼容格式（`{ error, error_code, error_detail }`）。当前 FlowApiService 的 error 标准化逻辑是：

```typescript
const standardized = {
  error: data.error || data.title || data.detail || "Unknown error",
};
```

这有三个问题：

1. `ProblemDetails` 的 `title` 是错误类型名称（如"Validation error"），`detail` 才是可读消息——当前逻辑会将 `title` 当 error message，丢失 `detail`
2. FluentValidation 错误放在 `errors` 扩展属性中（对象格式），被完全忽略
3. 认证错误走 Plane 格式，`error_detail` 字段未被提取

**Why it happens:** 后端同时支持两套错误格式（为了与 Plane 前端兼容）。前端错误处理写成"哪个字段非空取哪个"的启发式，但不同字段包含不同语义级别的信息。

**Consequences:**

- 验证错误：用户看到"Validation error"而非具体的字段错误，无法修复表单
- 认证错误：可能丢失具体的错误详情（如"密码太弱"的具体规则）
- 开发者调试困难：`traceId` 和 `correlationId` 被丢弃

**Prevention:**

1. **统一错误格式**：后端对所有路由返回一致的格式，或前端拦截器识别并转换两种格式
2. 前端 error 类型扩展为携带更多信息：
   ```typescript
   interface NormalizedError {
     message: string;
     status?: number;
     errors?: Record<string, string[]>; // FluentValidation errors
     traceId?: string;
     correlationId?: string;
   }
   ```
3. 对于 FluentValidation 错误，前端需要在表单字段级别映射错误（每个字段的 error message 绑定到对应 input）

**Detection:**

- 提交表单看到 "Validation error" 而非字段级别的红色提示
- 认证失败看到 "error" 而非具体原因

---

### Pitfall 5: 乐观更新 + 后台重新获取的竞态条件（数据回滚）

**What goes wrong:** 当前 `useIssueMutations` 的 `updateIssue` mutation 实现了 `onMutate` 乐观更新 + `onError` 回滚的完整模式。但切换到真实后端后会出现经典竞态：

1. 用户拖拽 Issue 到新状态（乐观更新立即生效）
2. `onMutate` 调用了 `queryClient.cancelQueries({ queryKey: ["issues"] })` 取消正在发出的 refetch
3. 但 `onSettled` 中调用了 `queryClient.invalidateQueries({ queryKey: ["issues"] })`
4. 如果 invalidation 触发了一个 refetch，而这个 refetch 在 PATCH 请求完成之前返回了旧数据，**乐观更新会被旧数据覆盖**

另一个问题：`onMutate` 使用了 `queryClient.getQueriesData` 保存所有匹配 `["issues"]` 的缓存。在复杂查询键结构下（`["issues", projectId, filters]`），这会保存大量数据快照，可能导致性能问题。

**Why it happens:** Mock 阶段没有网络延迟，乐观更新和 refetch 之间时间差为 0，不会触发竞态。真实网络的 200-500ms 延迟使得 PATCH 请求和 GET refetch 之间的时序不确定。

**Consequences:**

- Issue 拖拽后状态闪烁：用户看到新的状态 → 回滚到旧状态 → PATCH 请求完成 → 变回新状态
- 严重情况下用户对 UI 失去信任
- 列表排序/筛选后的批量操作可能产生部分生效的假象

**Prevention:**

1. `onMutate` 中 `cancelQueries` 后，使用**查询键工厂**精确取消（而非宽泛的 `["issues"]`）
2. `onSettled` 中的 `invalidateQueries` 不要使用宽泛 key，使用精确 key 减少不必要 refetch
3. 考虑使用 `mutationFn` 返回的数据更新缓存（而非 refetch），仅在 mutation 完成后 `setQueryData`
4. 对于拖拽等高频操作，使用 debounce 合并多个快速 mutation

**Detection:**

- UI 中看到内容"闪烁"
- Debug 日志中看到多次相同数据的 GET 请求
- 用户的 issue 状态变化后短暂消失又恢复

---

### Pitfall 6: 无查询键工厂（Query Key Factory）导致缓存失效不可预测

**What goes wrong:** 当前前端所有 hook 使用不一致的字符串查询键散布在各个文件中：

- `["workspaces"]`
- `["projects", workspaceId]`
- `["issues", projectId, filters]`
- `["cycles", projectId]`
- `["comments", issueId]`
- `["pages", workspaceId]`
- `["workspace-notifications", workspaceId]`

当某个 mutation 完成后要失效缓存时，各个 hook 各自写各自的 `invalidateQueries`，容易出现：

1. **漏失效**：创建了一个 Issue 相关的 Comment，但 `onSuccess` 只 invalidate 了 `["comments"]` 而没有 invalidate `["issues"]`（因为 Issue 列表显示评论数）
2. **过度失效**：更新一个 Issue 的描述，失效了所有 `["issues"]` 键（包括其他项目、其他筛选条件下的 issue 列表），造成不必要的网络请求
3. **失效错位**：`["issues", projectId]` 和 `["issues", projectId, filters]` 是两个独立的缓存条目，invalidating `["issues"]` 只能匹配前缀——但如果某个筛选条件下的 issue 在 mutation 后应该变化，它可能没有被刷新

**Why it happens:** TanStack Query 的查询键是扁平的字符串/数组。没有集中的键定义，各模块各自为政。

**Consequences:**

- 用户看到过期的数据（评论数不更新、Issue 状态不反映实际变化）
- 或者相反，过多不必要的网络请求
- 缓存失效的 bug 难以排查：只看某个 hook 的代码看不出问题

**Prevention:**

1. **在切换到真实后端之前建立集中式 Query Key Factory**：
   ```typescript
   // lib/query-keys.ts
   export const qk = {
     workspaces: {
       all: () => ["workspaces"] as const,
       detail: (slug: string) => ["workspaces", slug] as const,
     },
     projects: {
       all: (wsId: string) => ["projects", wsId] as const,
       detail: (wsId: string, projId: string) => ["projects", wsId, projId] as const,
     },
     issues: {
       all: (projId: string) => ["issues", projId] as const,
       list: (projId: string, filters: IssueFilters) => ["issues", projId, "list", filters] as const,
       detail: (projId: string, issueId: string) => ["issues", projId, issueId] as const,
       comments: (issueId: string) => ["issues", "comments", issueId] as const,
     },
   };
   ```
2. 在 mutation 的 `onSuccess` 中使用精确的键失效，辅以必要的父级键失效

**Detection:**

- Issue 列表页显示的数据与 Issue 详情页不一致
- 修改数据后刷新页面才看到变化
- Network tab 中看到大量重复的 GET 请求

---

## Moderate Pitfalls

### Pitfall 7: 分页策略不一致——Mock 返回全部数据，真实 API 使用游标分页

**What goes wrong:** 当前前端 hook 返回完整的数组（`TIssue[]`），组件直接 `.map()` 渲染。Plane API 使用游标分页（`TPaginatedResponse` 包含 `next_cursor`, `prev_cursor`, `total_count`）。切换到真实后端后：

1. 后端返回的是 `{ results: TIssue[], next_cursor: "abc", prev_cursor: null, total_count: 150 }` 而非 `TIssue[]`
2. 当前 `useQuery` 无法处理分页——需要用 `useInfiniteQuery` 替代
3. 前端现有的 `Pagination` 组件（`app/components/issues/pagination.tsx`）与 TanStack Query 的 `useInfiniteQuery` 不兼容，因为 Plane 的分页组件假设 total_pages 和 current_page（页码式分页），而非游标式

**What goes wrong (continued):** 后端目前可能尚未实现所有模块的返回分页。一部分接口返回完整列表，一部分返回分页数据。前端无法推断哪种格式。

**Prevention:**

1. 确认后端所有列表接口的分页策略：统一使用游标分页还是传统分页
2. 创建 `usePaginatedQuery` 或 `useInfiniteQuery` 的通用封装，统一处理 `TPaginatedResponse`
3. 前端 `useIssues` 等 hook 在切换到真实后端时必须也切换返回类型——不能直接替换 `queryFn` 了事
4. 修改 `Pagination` 组件支持 `useInfiniteQuery` 的 `fetchNextPage` / `hasNextPage`

### Pitfall 8: `humps.camelizeKeys` 对 Date 对象和 null 值的破坏

**What goes wrong:** 当前 FlowApiService 在响应拦截器中调用 `humps.camelizeKeys(response.data)`。`humps` 会递归遍历所有嵌套对象转换 key。遇到 `null` 或 `undefined` 时它跳过，但遇到 ISO 日期字符串（如 `"2026-06-30T10:00:00Z"`）它不会破坏。**问题在于**：如果后端返回的某些字段的值本身就是对象属性被序列化为嵌套结构，或者某些枚举值被转为对象，`camelizeKeys` 可能会导致运行时异常或数据丢失。

具体到这个项目：`logo_props` 字段包含 `{ in_use: "emoji", emoji: { value: "128640" } }`——`camelizeKeys` 会正常转换。但如果后端在 JSON 中包含了 .NET 的 `System.Text.Json` 序列化出的特殊结构（如 `$id`、`$values` 引用循环标记），`humps` 可能破坏这些特殊标记。

**Prevention:**

1. `camelizeKeys` 的第二个参数提供回调：`(key, convert) => key.startsWith('$') ? key : convert(key)` 来保留特殊键
2. 如果确认后端永远不输出特殊结构，这条可以跳过
3. 在切换一个模块后验证 logo_props、view_props、display_filters 等 JSON 嵌套字段的转换结果

### Pitfall 9: 跨组件页共享缓存导致 MobX 和 TanStack Query 数据不一致

**What goes wrong:** 当前前端同时使用 MobX（UI/Client 状态）和 TanStack Query（Server 状态）。几个 MobX store 中存储了业务数据（如 `workspace.currentWorkspaceId`、`issue` store），而实际列表数据来自 TanStack Query 的 useQuery。当用户在两个页面之间导航：

1. 页面 A 加载了 Issue 列表到 TanStack Query 缓存
2. 用户通过 MobX store 更新了当前工作区（不涉及 TanStack Query）
3. 页面 B 的 Issue 列表通过 `useIssues(projectId, filters)` 读取，key 包含 `projectId`
4. 如果 `projectId` 变了但 filters 对象的引用相同，TanStack Query 可能从缓存中返回了旧项目的 Issue 数据

**Prevention:**

1. 确保所有查询键包含了所有决定性参数（projectId, workspaceId, filters）
2. 建立明确的规则：MobX store 中不要缓存 API 返回数据，只存 UI 状态
3. 在 Route 级别的 loader 中预备查询（prefetch），避免页面切换时的 loading 状态
4. 使用 `queryClient.invalidateQueries({ queryKey: ["issues"] })` 在关键事件（工作区切换、项目切换）后批量失效

---

## Minor Pitfalls

### Pitfall 10: Mock 数据中的 null/id 假设

Mock 数据中的所有 ID 都是预定义的字符串（`"ws-1"`, `"proj-1"`, `"user-1"`）。真实后端返回的 ID 是 PostgreSQL 生成的 UUID（GUID 格式如 `"a1b2c3d4-..."`）。如果在某个组件或 store 中硬编码了 `"user-1"` 比较（如权限判断），切换到真实后端后会全部失效。

**Prevention:** 全局搜索任何硬编码的 Mock ID（`"ws-1"`, `"proj-1"`, `"user-1"`, `"state-ff-1"` 等），替换为从 API 响应中获取的动态 ID。

### Pitfall 11: 后端 Validation Errors 的 FluentValidation 格式在前端没处理

FluentValidation 默认的错误格式为：

```json
{
  "errors": {
    "Name": ["'Name' must not be empty."],
    "StartDate": ["'Start Date' must be a valid date."]
  }
}
```

当前前端只在 mutation 的 catch 中做了 `throw err?.response?.data`，没有 schema-level 验证错误映射。每个需要服务器验证反馈的表单组件都需要处理这个结构。

**Prevention:** 在拦截器中统一将 `errors` 转换为前端标准格式，或创建一个 `useServerValidationErrors` hook 从 mutation 的 `error` 状态中提取字段级错误。

### Pitfall 12: 后端 404 和前端乐观删除的交互

Mock 阶段删除一条记录后立即从内存数组中移除。真实删除是异步的：发送 DELETE → 后端返回 204 → 前端刷新列表。在这之间如果用户操作已删除的数据（如快速打开 Issue 详情），会触发一个针对已删除资源的 GET，后端返回 404。当前组件没有 404 处理——可能显示"加载中"或空白页。

**Prevention:** 在删除 mutation 的 `onMutate` 中立即从缓存中移除该数据，并处理 404 响应为"已删除"状态（而非错误状态）。

### Pitfall 13: SSE 实时推送与 TanStack Query 缓存的冲突

后端使用 SSE（Server-Sent Events）实现实时更新。当后端通过 SSE 通知前端某个 Issue 已更新时，如果 TanStack Query 同时发起了一个 refetch，两路更新可能产生竞争。SSE 更新直接进入 MobX store，而 TanStack Query 的 refetch 更新 Query 缓存，导致同一个数据在两个状态容器中有不一致的值。

**Prevention:** 制定 SSE 数据的流向规则：SSE 事件到达后写入 TanStack Query 缓存（通过 `queryClient.setQueryData`），而非写入 MobX store。MobX store 只消费 Query 缓存中的数据（通过 useQuery hook 读取）。

---

## Phase-Specific Warnings

| Phase              | Likely Pitfall                                            | Mitigation                                                                                          |
| ------------------ | --------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| Auth (JWT)         | Pitfall 2 (Token refresh race) + Pitfall 4 (Error format) | 先实现 refresh queue 再连接真实 auth API；通用错误处理最好 auth 和 data 模块共享同一套              |
| Workspace          | Pitfall 1 (camelCase 请求体) + Pitfall 10 (Hardcoded ID)  | Workspace 作为"第一个真实 API 连接"最适合——数据结构简单、依赖少；写完请求体转换器后先测试 workspace |
| Project            | Pitfall 8 (嵌套字段 logo_props)                           | 验证 `humps.camelizeKeys` 对嵌套对象的处理                                                          |
| WorkItems          | Pitfall 5 (乐观更新竞争) + Pitfall 7 (分页)               | 这是最复杂的转换；建议先做 Issue 列表（只读），再做 Issue 详情，最后做拖拽/批量操作（涉及乐观更新） |
| Cycle/Module       | Pitfall 6 (查询键不一致)                                  | 确认 `["cycles", projectId]` 和 `["issues", projectId, { cycle_id }]` 之间的失效关系                |
| Page               | Pitfall 11 (有 TipTap 富文本的 POST body)                 | 确认 `camelizeKeys` 不破坏 `comment_json`（TipTap JSONContent 结构）                                |
| Notification       | Pitfall 12 (404 + optimistic delete)                      | Notification 的 mark-as-read 是 POST 而非 DELETE，需要注意                                          |
| Analytics          | Pitfall 7 (分页)                                          | Analytics 通常不分页，但返回结构（TChart）与 DB 模型不同，需验证转换                                |
| Webhook (新增功能) | 无 Mock 阶段——直接从零接入 API                            | 直接用 service 模式写真实查询，是最干净的实现; 注意与现有模块使用统一错误处理、认证等基础设施       |

## Sources

- 代码库分析：FlowApiService (`.planning/research/PITFALLS.md` 来源于项目代码阅读)
- TanStack Query 官方文档 — Optimistic Updates 最佳实践: https://tanstack.com/query/latest/docs/framework/react/guides/optimistic-updates
- TanStack Query 官方文档 — Infinite Queries: https://tanstack.com/query/latest/docs/framework/react/guides/infinite-queries
- TanStack Query Important Defaults: https://tanstack.com/query/v5/docs/framework/react/guides/important-defaults
- humps NPM README — camelizeKeys/decamelizeKeys: https://www.npmjs.com/package/humps
- ASP.NET Core 全局异常处理 + ProblemDetails 格式 (查看 `src/BuildingBlocks/Web/Exceptions/GlobalExceptionHandler.cs`)
- ASP.NET Core JsonSerializerOptions SnakeCaseLower 配置 (查看 `src/Host/YH.Flow.Api/Program.cs`)
- Plane auth error handler 代码 (查看 `src/lib/utils/auth.ts`)
- JWT refresh token race condition 分析: https://dev.to/tai_tran_36c0d039fde1e560/handling-jwt-refresh-tokens-in-axios-without-the-headache-56nb
