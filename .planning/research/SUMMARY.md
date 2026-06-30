# Project Research Summary

**Project:** YH.Flow -- v3.0 Milestone “前后端打通”
**Domain:** 项目管理 SPA 前端从 Mock 数据切换到 .NET 后端 API
**Researched:** 2026-06-30
**Confidence:** HIGH

## Executive Summary

YH.Flow v3.0 里程碑的目标是将已完成的前端（Flow Web）从 Mock 数据切换到真实的 .NET 后端 API。前端基于 Plane Web（React 18/SWR/MobX）渐进式改造为 Flow Web（React 19/TanStack Query/MobX），目前已有完整的前端页面、路由和组件，但所有数据层（15 个 TanStack Query hooks + 3 个服务文件）均使用硬编码 Mock 数据。后端 15 个模块的 Minimal API 已完成，认证（JWT）、基础设施（Finbuckle 多租户、SnakeCaseLower 序列化）已就绪。核心任务是逐个模块将 hooks 的 queryFn 从 Mock 数据替换为真实 API 调用。

**推荐方案**：按照依赖关系分层推进 -- 先改造基础设施层（请求体格式转换、Token 刷新队列、集中式查询键工厂），再依次改造 Workspace、Project、WorkItems、Cycle/Module、Page/View，最后清理 Mock 数据和添加新功能。每个模块的改造都遵循统一模式：创建 Service 类（继承 FlowApiService） -> 更新 TanStack Query hook -> 验证三态（loading/error/empty）UI 处理。五维 Issue 布局（List/Kanban/Calendar/Gantt/Spreadsheet）中的拖拽和乐观更新逻辑需要特别注意竞态条件。

**关键风险**：6 个 Critical 级陷阱可能造成静默数据损坏 -- 请求体 camelCase/snake_case 不匹配（所有写入操作字段被忽略）、401 Token 刷新竞态（页面初始化时多个请求同时 401 导致强制登出）、乐观更新竞态（拖拽 Issue 后数据闪烁）、Mock 阶段从未暴露的 loading/error 分支缺失。这些风险在切换第一个数据 hook 前必须通过基础设施层的改造来防范。建议以 Workspace 模块作为首个验证试点。

## Key Findings

### Recommended Stack

核心变化是数据获取层：**SWR 替换为 TanStack Query v5**（更好的 mutation 支持、乐观更新、DevTools），**Django CSRF/session 替换为 JWT Bearer**（通过 Axios 拦截器注入，响应 401 时尝试刷新 token）。MobX 保留但职责收缩为纯客户端 UI 状态（过滤器、侧边栏、主题），**严禁**保存服务器返回数据。

Plane Web 的 UI 组件库 @plane/propel（30+ 组件）和 TipTap 编辑器 @plane/editor 被 Fork 到 Flow Web 本地目录，去除 Yjs/Hocuspocus 协作依赖。后端已实现完整的 15 模块 API，前端通过 FlowApiService（基于 Axios，支持 JWT 自动注入、camelCase/snake_case 转换）调用。实时更新使用浏览器原生 SSE（EventSource API），不需要额外库。

详见 [STACK.md](./STACK.md)。

**Core technologies:**

- **React 19**: UI 框架 -- Plane Web 从 React 18 升级，Flow Web 直接从 React 19 起步
- **TanStack Query v5**: 服务端状态管理 -- 替换 SWR，提供 mutation 生命周期和乐观更新模型
- **MobX 6**: 客户端 UI 状态 -- 保留但缩小范围，仅持有 UI 瞬态状态
- **Axios**: HTTP 客户端 -- 拦截器实现 JWT 注入、401 处理和格式转换
- **React Hook Form + Zod v4**: 表单验证 -- 前后端共享校验逻辑
- **Forked @plane/propel**: UI 组件库 -- 30+ 组件，基于 @base-ui-components/react
- **Forked @plane/editor**: TipTap 富文本编辑器 -- 去除 Yjs 协作，保留核心扩展

### Expected Features

前端已完成所有页面和路由，当前工作在**数据层替换**而非功能新增。所有 Must-have 功能已实现在前端 UI 中，但均基于 Mock 数据。

**Must have（已实现，待接驳真实 API）：**

- JWT 认证（登录/注册/退出） -- 唯一已接驳真实后端的模块
- 工作区 CRUD + 切换 + 成员管理
- 项目 CRUD + 成员管理
- Issue CRUD + 列表/详情/看板视图
- Cycle/迭代管理
- Module/史诗分组
- Page/文档管理 + 富文本编辑器
- 自定义视图 + 筛选/排序/分组
- 通知（站内 + SSE 实时推送）

**Should have（差异化功能，已实现 UI）：**

- TipTap 富文本编辑器（代码块、@提及、任务列表、emoji 选择器）
- 五维 Issue 布局：列表、看板（含子分组/Swimlane）、日历（拖拽排期）、甘特图（依赖关系）、电子表格
- 多级筛选引擎（按状态/优先级/负责人/标签/日期分组 + 子分组）
- Emoji 图标选择器 + 暗色/亮色主题切换
- 拖拽跨状态移动 Issues
- 命令面板（Cmd+K）

**Defer（推迟/删除）：**

- 实时协作编辑（Yjs/Hocuspocus） -- 后续里程碑
- 多语言 i18n -- 一期仅中文
- PDF 导出 -- 按需添加
- Microsoft Clarity 会话记录 -- 已移除
- Web Worker 编排（comlink） -- 已移除

详见 [FEATURES.md](./FEATURES.md)。

### Architecture Approach

采用分层 Service 模式：React 组件通过 TanStack Query hooks 调用 Service 类方法，Service 类继承 FlowApiService（Axios 封装），FlowApiService 通过请求拦截器自动注入 JWT token 并执行 camelCase->snake_case 格式转换，响应拦截器执行反向转换并标准化错误格式。MobX store 仅持有 UI 状态（当前工作区 slug、筛选器选择、侧边栏状态）。

**关键架构决策**：

1. 请求体格式：在 Axios 请求拦截器中统一 humps.decamelizeKeys（将前端 camelCase 转为后端 snake_case）
2. Token 刷新：实现单例 promise + 请求队列，防止并发 401 时的 race condition
3. 分页处理：Service 层提取 PlanePagedResult.results[]，hooks 继续返回 T[] 数组
4. 集中式查询键工厂：query-keys.ts 统一管理所有模块的 TanStack Query 缓存键
5. Slug 路由：前端 hooks 从 MobX store 获取 workspace slug（非 UUID），传递给后端 API

详见 [ARCHITECTURE.md](./ARCHITECTURE.md)。

**Major components:**

1. **FlowApiService** -- Axios 封装基类，处理 JWT、格式转换、401 刷新、错误标准化
2. **Domain Services（12 个）** -- 每个后端模块对应一个 Service 类
3. **TanStack Query Hooks（15 个）** -- 封装 useQuery/useMutation，实现乐观更新和缓存管理
4. **MobX Stores** -- UI 瞬态状态（sidebar, filters, selections, theme）
5. **SSE Dispatcher** -- 浏览器 EventSource 监听实时通知，更新 TanStack Query 缓存

### Critical Pitfalls

详见 [PITFALLS.md](./PITFALLS.md)。

| #   | Pitfall                                            | Severity | Prevention                                                         |
| --- | -------------------------------------------------- | -------- | ------------------------------------------------------------------ |
| 1   | 请求体 camelCase/snake_case 不匹配（静默失败）     | CRITICAL | FlowApiService 请求拦截器添加 humps.decamelizeKeys，包括查询字符串 |
| 2   | 401 Token 刷新竞态条件导致循环登出                 | CRITICAL | 请求队列 + 单例 refresh promise，仅刷新失败后跳转                  |
| 3   | staleTime 默认值 + Mock 从未展示真实 loading/error | CRITICAL | 每个 hook 迁移后确认三态处理，开发期设 retry: 0                    |
| 4   | 后端错误格式双轨制（ProblemDetails vs Plane）      | CRITICAL | 拦截器统一提取，FluentValidation errors 字段级映射                 |
| 5   | 乐观更新 + 后台 refetch 竞态（数据回滚闪烁）       | CRITICAL | 精确查询键取消/失效，避免宽泛 invalidateQueries                    |
| 6   | 无集中式查询键工厂 -- 缓存泄漏或不完整             | CRITICAL | 迁移前建立 query-keys.ts 集中管理                                  |
| 7   | 分页策略不一致（游标 vs 传统）                     | MODERATE | 确认后端分页格式；Service 层提取 results[]                         |
| 8   | humps.camelizeKeys 破坏特殊 JSON 结构              | MODERATE | 跳过 $ 开头的键；验证嵌套字段如 logo_props                         |
| 9   | 跨页面 MobX 和 TanStack Query 数据不一致           | MODERATE | 查询键包含所有决定参数；页面切换时批量失效                         |
| 10  | Mock 数据硬编码 ID（ws-1, user-1）                 | MINOR    | 全局搜索替换为动态 ID                                              |
| 11  | FluentValidation 错误未在前端字段级处理            | MINOR    | 拦截器统一转换，或创建 useServerValidationErrors hook              |
| 12  | 404 + 乐观删除交互（操作已删除数据）               | MINOR    | onMutate 中立即从缓存移除，404 处理为已删除                        |
| 13  | SSE 实时推送与 TanStack Query 缓存冲突             | MINOR    | SSE 事件写入 Query 缓存（setQueryData），不写入 MobX               |

## Implications for Roadmap

基于依赖图（ARCHITECTURE.md 第 5 节，6 层依赖）和陷阱优先级（PITFALLS.md 的阶段警告），建议分为 7 个阶段推进。阶段编号从 21 开始（续接 v2.0 的序列，原编号到 20 结束）。

### Phase 21: Infrastructure Foundation（基础设施层）

**Rationale:** 所有后续模块依赖正确的请求格式、Token 刷新和查询键管理。必须在切换第一个数据 hook 之前完成。
**Delivers:** query-keys.ts 集中式查询键工厂，FlowApiService 增强（decamelizeKeys 请求拦截器、Token 刷新队列）
**Addresses:** ARCHITECTURE.md Layer 0（基础设施变更）
**Avoids:** Pitfall 1（camelCase/snake_case）、Pitfall 2（Token 刷新竞态）、Pitfall 6（查询键工厂）
**Research flag:** 标准模式。注意验证 humps.decamelizeKeys 对嵌套对象和 FormData 的兼容性。

### Phase 22: Foundation Domain -- Workspace + Project + Members（基础域）

**Rationale:** Workspace slug 和 Project ID 是所有其他模块的前置依赖。Workspace 数据结构简单，适合作为首个真实 API 验证试点。
**Delivers:** workspace.service.ts, project.service.ts, member.service.ts + 对应 hooks 替换（共 6 个文件）
**Addresses:** 工作区 CRUD + 成员管理、项目 CRUD + 成员管理（FEATURES.md table stakes）
**Avoids:** Pitfall 4（错误格式标准化 -- 在此阶段验证双轨制处理）、Pitfall 10（硬编码 Mock ID 全局搜索替换）
**Verification:** 完成此阶段后应能创建/编辑/删除工作区和项目，数据持久化到 PostgreSQL。

### Phase 23: Core WorkItems -- Issues + States + Labels + Comments（核心业务）

**Rationale:** Issue 是项目管理最核心实体，涉及最复杂交互（拖拽、批量操作、乐观更新）。此阶段锁定 WorkItems 模块全部子实体。
**Delivers:** work-items.service.ts + use-issues.ts, use-comments.ts, use-issue-mutations.ts 替换
**Addresses:** Issue CRUD + 列表/详情/看板（FEATURES.md table stakes）
**Avoids:** Pitfall 5（乐观更新竞态 -- 精确查询键取消策略）、Pitfall 7（分页策略 -- 确认后端格式）、Pitfall 8（嵌套字段的 humps 兼容性）
**Research flag:** 需要确认后端 WorkItems 列表分页策略、乐观更新精确键取消方案、logo_props 等嵌套字段的 humps 兼容性。

### Phase 24: Cycles + Modules（周期与模块分组）

**Rationale:** Cycles 和 Modules 依赖 Workspace/Project 上下文（Phase 22），且与 WorkItems 共享数据（Cycle/Module 的 Issue 列表）。
**Delivers:** cycle.service.ts, module.service.ts + 对应 hooks 替换
**Addresses:** Cycle/迭代管理、Module/史诗分组（FEATURES.md table stakes）
**Avoids:** Pitfall 6（查询键一致性 -- 确认 cycles 和 issues 间的失效关系）
**Research flag:** 标准模式。模式与 Phase 23 一致。

### Phase 25: Secondary Domains -- Pages + Views（次要域）

**Rationale:** Pages 和 Views 功能相对独立，不依赖 Issue 数据，但 Pages 的 TipTap 编辑器富文本内容需特殊验证。
**Delivers:** page.service.ts, view.service.ts（更新） + 对应 hooks 替换
**Addresses:** Page/文档管理 + 富文本编辑、自定义视图 + 筛选排序分组（FEATURES.md table stakes）
**Avoids:** Pitfall 11（TipTap POST body -- 确认 decamelizeKeys 不破坏 JSONContent 结构）、Pitfall 12（删除后 404 处理）
**Research flag:** 需要验证 TipTap 编辑器 JSON 结构在 decamelizeKeys 下的兼容性。

### Phase 26: Utilities Cleanup -- Notifications + Analytics + Remove Mocks（工具清理）

**Rationale:** Notifications 和 Analytics 不依赖特定域上下文，适合最后处理。此阶段清理所有 Mock 数据文件。
**Delivers:** notification.service.ts（更新）、analytics.service.ts（更新）、移除 mock-data.ts
**Addresses:** 通知（站内 + SSE 实时推送，FEATURES.md table stakes）
**Avoids:** Pitfall 12（Notification mark-as-read 是 POST 而非 DELETE）、Pitfall 13（SSE 写入 Query 缓存而非 MobX store）
**Research flag:** 需要确认 Analytics 接口返回结构和 Notification SSE 端点路径。

### Phase 27: New Feature -- Webhook Admin UI（新功能）

**Rationale:** Webhook 是 v3.0 新增功能，无 Mock 数据阶段。直接从零接入真实后端，是最干净的实现。
**Delivers:** webhook.service.ts + hooks + admin UI 组件
**Addresses:** Webhook 订阅管理（新功能），用户可创建/查看/测试 Webhook
**Research flag:** 标准模式。直接按 Service + hooks 模式实现，注意与现有模块统一错误处理和认证。

### Phase Ordering Rationale

- **依赖优先**：Phase 21 的基础设施（格式转换、Token 刷新、查询键）是所有 API 调用的前提条件，必须最先完成
- **核心优先**：Phase 22-23 处理最常用的实体（Workspace -> Project -> Issue），解决最复杂的交互和陷阱
- **风险隔离**：先做 Workspace（简单实体验证基础设施），再做 Issue（复杂交互验证乐观更新），最后做 Page/View（验证富文本特殊场景）
- **清理收尾**：Phase 26 清理所有中间 Mock 数据，确保不遗留过渡代码
- **新功能独立**：Phase 27 的 Webhook 是干净实现，可最后执行

### Research Flags

需要额外研究的阶段：

- **Phase 23（WorkItems）**: 确认后端分页策略（游标 vs 传统）、乐观更新精确查询键策略、嵌套字段 humps 兼容性
- **Phase 25（Pages）**: 验证 TipTap 编辑器 JSON 结构与 decamelizeKeys 兼容性

标准模式（跳过 research-phase）：

- **Phase 21（Infrastructure）**: Token 刷新、格式转换、查询键模式均有成熟文档和实践
- **Phase 22（Workspace/Project）**: CRUD 模式简单直观，Service 层模式已建立
- **Phase 24（Cycles/Modules）**: 模式与 WorkItems 一致
- **Phase 26（Notifications/Analytics）**: Service 模式已存在，仅替换 Mock 逻辑
- **Phase 27（Webhook）**: 全新功能，直接按标准模式实现

## Confidence Assessment

| Area         | Confidence | Notes                                                                          |
| ------------ | ---------- | ------------------------------------------------------------------------------ |
| Stack        | HIGH       | 基于 Plane Web 源码 + npm registry 版本确认，代码分析已验证                    |
| Features     | HIGH       | 基于实际前端代码分析，所有功能已实现                                           |
| Architecture | HIGH       | 基于 FlowApiService 源码、hooks 实现、后端端点注册直接分析                     |
| Pitfalls     | HIGH       | 基于 FlowApiService 源码审计 + TanStack Query 官方文档 + ASP.NET Core 配置分析 |

**Overall confidence:** HIGH

### Gaps to Address

- **Pagination 策略确认**: ARCHITECTURE.md 假设 Service 层提取 results[]，但 PITFALLS.md 指出后端可能混合游标和传统分页。Phase 23 前需确认 WorkItems 列表接口具体分页格式。
- **Token refresh 端点未验证**: auth.service.ts 中的 refreshToken() 从未被调用。Phase 21 实现 Token 刷新队列时需验证该端点的响应格式和错误行为。
- **Zod v4 + @hookform/resolvers 兼容性**: 升级到 Zod v4 后需验证 @hookform/resolvers 兼容性（STACK.md Unknowns）。
- **MobX + React 19 兼容性**: mobx-react@9.1.1 在 React 19 下兼容性需验证（STACK.md Unknowns）。
- **后端 Analytics 和 Webhook 端点结构**: ARCHITECTURE.md 未读取这些模块的端点注册代码，Phase 26-27 前需确认 API 路径和返回结构。

## Sources

### Primary (HIGH confidence)

- Plane Web 源码分析 -- packages/services, apps/web, packages/editor, packages/propel
- Flow Web 代码库 -- clients/web/src/lib/services/flow-api.service.ts, src/lib/hooks/\*.ts, src/lib/mock-data.ts
- 后端代码分析 -- src/Host/YH.Flow.Api/Program.cs, \*Module.cs 端点注册
- TanStack Query v5 官方文档 -- https://tanstack.com/query/latest/docs/framework/react/
- ASP.NET Core JsonSerializerOptions.SnakeCaseLower -- Program.cs 源码确认

### Secondary (MEDIUM confidence)

- Plane Web pnpm-workspace.yaml -- 版本目录参考
- npm registry -- React 19.2.7, TanStack Query 5.101.0, Zod 4.4.3 版本确认
- humps NPM README -- camelizeKeys/decamelizeKeys 使用模式
- JWT refresh token race condition 分析 -- 社区文章参考

### Tertiary (LOW confidence)

- Plane Web mock 数据硬编码 ID（ws-1, user-1 等） -- 需全局搜索确认具体位置
- 后端 Analytics 和 Webhook 端点结构 -- 未读取对应 \*Module.cs
- Plane Web 中 comlink 实际使用情况 -- 需代码确认是否可安全移除

---

_Research completed: 2026-06-30_
_Ready for roadmap: yes_
