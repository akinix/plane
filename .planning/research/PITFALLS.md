# Pitfalls: Plane Web → Flow Web 迁移陷阱

## 🔴 关键陷阱（阻塞级）

### P1: 分页格式不匹配

**问题：** Plane Web 使用自定义 `OffsetPaginator`，响应格式包含 `next_cursor`、`prev_cursor`、`total_pages`、`per_page`。而 YH.Flow 的 `PlanePagedResult` 目标格式是 DRF 的 `PageNumberPagination`（count/next/previous/results）。两种格式结构不兼容。

**影响：** 所有列表页面的分页都失效。

**解决：**

- 方案 A：调整 .NET 端 `PlanePagedResult` 匹配 Plane 的实际分页格式
- 方案 B：在前端写一个响应转换器（adapter pattern）
- **推荐：方案 A**（后端离前端更近，改一处全局生效）

**责任 Phase：** Phase 1

---

### P2: 认证流程差异

**问题：** Plane 前端依赖 Django Session Cookie + CSRF Token。`APIService` 全局设置 `withCredentials: true`，`signOut()` 使用 Django 特有的 HTML form POST。YH.Flow 支持 JWT Bearer、Session Cookie、X-Api-Key，但前端需要适配。

**影响：** 不做任何修改的话，登录/注册完全无法工作。

**解决：**

- 切换为 JWT Bearer：前端添加 token 管理（本地存储）、Axios 拦截器注入 `Authorization: Bearer` header
- 登出改为调用 .NET 登出端点（或本地清除 token）

**责任 Phase：** Phase 1（必须是最早的决策）

---

### P3: 错误响应格式

**问题：** Plane DRF 返回 `{"error": "message"}`。ASP.NET Core 默认返回 `ProblemDetails`（RFC 7807：`type`/`title`/`status`/`detail`/`traceId`）。前端所有错误处理都假设 `error.response?.data?.error`。

**影响：** 全部前端错误处理失效，用户看到不可读的错误。

**解决：**

- .NET 端调整错误格式兼容 Plane 格式（最简单的方案）
- 或者在 Axios 拦截器做响应格式转换

**责任 Phase：** Phase 1

---

### P4: MobX Store 与响应体深度耦合

**问题：** Plane Web 的 MobX stores 深度耦合到 DRF 的 snake_case 字段名、未包装的响应体（直接 `response?.data`）、以及特定的嵌套序列化器结构。

**影响：** 除非 `System.Text.Json` 全局配置 `PropertyNamingPolicy = SnakeCaseLower`，store 会静默地无法填充数据。

**解决：**

- .NET 端全局配置 `JsonNamingPolicy.SnakeCaseLower`
- 同时配置 DateTime 格式、枚举处理
- 这是一行代码的全局修复，但影响最大

**责任 Phase：** Phase 1

---

### P5: MobX / TanStack Query 混合陷阱

**问题：** Plane Web 模式是 `store.fetchWorkspaces()` 直接调用 API 并存入 MobX。Flow Web 要求严格分离：MobX 只负责 UI 状态，TanStack Query 负责服务器状态。

**影响：** 开发者可能沿袭 Plane Web 的习惯，在 MobX store 中做 API 调用，绕过 TanStack Query 的缓存和失效机制，导致数据不一致。

**解决：**

- Code review 规则：MobX store 中不允许 HTTP 请求
- TanStack Query linter 规则：`@tanstack/query/no-unnecessary-invalidation`
- MobX store 模板禁止 fetch 类方法

**责任 Phase：** 全部 Phase（但 Phase 1 建立模式最关键）

---

## 🟡 重要陷阱

### P6: Plane 包分叉发散

**问题：** 必须 fork @plane/propel、@plane/editor、@plane/types、@plane/utils。修改过多会导致无法合并上游更新。

**预防：**

- 保留 fork 作为副本（不是 symlink）
- 修改处加注释标记：`// FLOW: <reason>`
- 只改必要的部分（API 层替换）

### P7: TipTap + React 19 兼容性

**问题：** TipTap 历史上滞后于 React 大版本。如果 `@tiptap/react@2.22.3` 不兼容 React 19，需要 workaround。

**预防：** 验证后再进入 Phase 5（Pages）。

### P8: CSRF vs JWT 混合

**问题：** 如果在迁移过程中混用 Session Cookie（Plane 方式）和 JWT Bearer（Flow 方式），前端认证逻辑会非常混乱。

**预防：**

- Phase 1 就决定并统一认证方案
- 推荐：直接切换为 JWT Bearer，不要做过渡方案

### P9: CORS 配置

**问题：** 前端 dev server 默认 `localhost:5173`，.NET API 在 `localhost:5100`，需要配置 CORS。

**预防：** Phase 1 就在 .NET 端配置 CORS 白名单。

---

## 🟢 "看起来好了但实际没修" 检查清单

每个 Phase 完成时必须验证：

- [ ] API 响应 body 是 snake_case 命名
- [ ] 错误格式兼容 `response?.data?.error`（或全局适配了）
- [ ] 分页格式与前端期望一致
- [ ] 认证 token 格式正确（JWT header/key name）
- [ ] 时间/日期格式正确
- [ ] MobX store 中没有 HTTP 请求
- [ ] fork 的 Plane 包没有不必要的修改
