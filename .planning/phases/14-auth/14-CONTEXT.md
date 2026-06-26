# Phase 14: 脚手架 & Auth - Context

**Gathered:** 2026-06-26
**Status:** Ready for planning

<domain>
## Phase Boundary

搭建 Flow Web 前端开发环境（React 19 + Vite 7 + TS 5 + Tailwind CSS 4），Fork 4 个 Plane 核心包（@plane/ui、@plane/editor、@plane/types、@plane/utils）到 `src/lib/`，实现完整的 JWT 认证流程（注册/登录/退出/忘记密码/重置密码），配置 API 适配层（JWT Bearer 拦截器、SnakeCase→CamelCase 转换、分页适配），实现暗色/亮色主题切换。

**Requirements covered:** SCAFF-01~09, AUTH-01~06, UI-03
</domain>

<decisions>
## Implementation Decisions

### 项目目录结构

- **D-01:** 镜像 Plane 的 apps/web/ 布局结构——使用 app/（路由/页面）、core/（组件/Store/Hooks/服务）、styles/、public/ 顶层目录。保持与 Plane 一致的目录风格，降低开发者的迁移认知负担。
- **D-02:** 使用 React Router v7 作为路由方案（与 Plane 现有方案一致），不使用 TanStack Router。
- **D-03:** Fork 后的 4 个 Plane 包统一放在 `clients/web/src/lib/` 下，每个包一个子目录。产出的 Vite 构建目标在 `clients/web/` 中。
- **D-04:** 通过 Vite alias 将 `@plane/*` 映射到 `src/lib/*`，使 Fork 后的代码无需修改 import 语句即可编译。

### 包 Fork 策略

- **D-05:** 采用代码复制方式（直接复制 Plane 包源码到 `src/lib/<package>/`），不采用 git subtree 或 workspace 引用。简单直接，但放弃上游同步能力——此为有意识的选择，因 API 层切换后 Plane 包中的服务层代码已不适用。
- **D-06:** 所有修改处添加 `// FLOW:` 标记注释，方便后续审计和回溯。
- **D-07:** Fork `@plane/editor` 时即剥离 Yjs/Hocuspocus 协作依赖，使编辑器包更轻量。TipTap 核心编辑功能保留，协作相关代码移除。

### 认证页面设计

- **D-08:** 保持 Plane 现有认证页面的居中卡片式布局风格——登录页面左侧 Logo、右侧表单，保持用户视觉连续性。
- **D-09:** 注册成功后自动登录（后端直接颁发 JWT token），跳转到工作区创建页面。不跳转到登录页。
- **D-10:** 实现完整的忘记密码/重置密码流程（发送重置链接→重置密码页面→完成），与 Plane 功能一致。

### API 适配层

- **D-11:** 创建 FlowApiService 基类（替代 Plane 的 APIService），在 request 拦截器中自动附加 `Authorization: Bearer <token>` header，在 response 拦截器中处理 401 响应（自动尝试 token 刷新，失败则清除 token 跳转到登录页）。
- **D-12:** 在 Axios response 拦截器中使用 humps 库自动将 .NET API 的 snake_case JSON 响应转换为 camelCase，前端代码无需感知。分页响应格式（PlanePagedResult）也在此层适配。
- **D-13:** 在 Axios 错误拦截器中统一将 .NET 的 `{"error": "message"}` 格式标准化为前端可消费的 ErrorResponse 类型，避免每个 Service 单独处理。

### Claude's Discretion

- 主题切换的 UI 位置（顶部导航栏 vs 侧边栏）留给后续实现决定。
- 具体 Token 刷新策略（静默刷新 vs 弹出式重新登录）留待实现时根据后端 Token 过期策略决定。

</decisions>

<canonical_refs>

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements & Architecture

- `.planning/REQUIREMENTS.md` — 完整 v2.0 需求定义（SCAFF-01~09, AUTH-01~06, UI-03 为本阶段需求）
- `.planning/PROJECT.md` — 项目架构策略、约束条件和模块映射
- `.planning/ROADMAP.md` — Phase 14 详细成功标准和范围定义

### Plane 前端模式参考

- `packages/services/src/api.service.ts` — Plane 现有 APIService 基类（withCredentials: true + CSRF 模式，将被 FlowApiService 替代）
- `packages/services/src/auth/auth.service.ts` — Plane 现有 AuthService 实现参考
- `apps/web/core/lib/wrappers/authentication-wrapper.tsx` — 路由保护模式参考
- `apps/web/core/components/account/auth-forms/` — 认证表单组件参考（email.tsx、password.tsx、forgot-password.tsx 等）
- `apps/web/helpers/authentication.helper.tsx` — 认证辅助函数参考
- `packages/services/src/index.ts` — 服务层 barrel exports 模式参考

### Plane 包参考

- `apps/web/app/(all)/sign-up/page.tsx` — 注册页面参考
- `apps/web/app/(all)/accounts/forgot-password/page.tsx` — 忘记密码页面参考
- `packages/ui/src/` — @plane/ui 组件源（将被 Fork 到 src/lib/）
- `packages/editor/src/` — @plane/editor（TipTap）源（Fork 时需剥离 Yjs）
- `packages/types/src/` — @plane/types 类型定义源
- `packages/utils/src/` — @plane/utils 工具函数源

### 代码库映射

- `.planning/codebase/STACK.md` — 技术栈分析
- `.planning/codebase/STRUCTURE.md` — 代码库结构
- `.planning/codebase/CONVENTIONS.md` — 编码规范和模式

</canonical_refs>

<code_context>

## Existing Code Insights

### Reusable Assets

- **AuthService** (`packages/services/src/auth/auth.service.ts`): 可直接移植并改造为 JWT 版本，保留一致的 API 语义
- **authentication-wrapper** (`apps/web/core/lib/wrappers/authentication-wrapper.tsx`): 路由保护组件模式可复用，但需从 SWR + MobX 改为 TanStack Query + MobX
- **auth-forms 组件** (`apps/web/core/components/account/auth-forms/`): 现成的表单组件（email.tsx、password.tsx、forgot-password-popover.tsx）可直接 Fork 到新的 lib 下
- **APIService** (`packages/services/src/api.service.ts`): 作为 FlowApiService 的设计参考，保留 get/post/put/patch/delete 方法签名一致性

### Established Patterns

- **MobX Store 模式**: `makeObservable` + 显式 observable/computed/action 声明 + `runInAction()` 包装异步状态变更
- **服务层模式**: 每个领域一个 Service 类继承 `APIService`，通过 barrel index.ts 统一导出
- **import 分组约定**: 按 `// ui`、`// types`、`// services`、`// helpers`、`// hooks`、`// components` 等注释头排序 import

### Integration Points

- **StoreProvider**: 通过 MobX 的 StoreProvider（React Context）注入全局 Store，新 AuthStore 需注入到此链
- **AuthenticationWrapper**: 根布局级别的路由保护组件，控制未认证用户的重定向
- **Provider 链** (`apps/web/app/provider.tsx`): 应用的 Provider 组合链，Flow Web 需建立类似的 Provider 结构

</code_context>

<specifics>
## Specific Ideas

- 登录/注册页保持 Plane 现有居中卡片式布局风格
- Fork 时立即剥离 @plane/editor 的 Yjs/Hocuspocus 依赖
- 使用 humps 库在 Axios 拦截器层自动完成 snake_case↔camelCase 转换
- Token 存储在 localStorage，Axios 拦截器自动附加到请求头
- 错误响应统一标准化为 `{error: string}` 格式

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

_Phase: 14-脚手架 & Auth_
_Context gathered: 2026-06-26_
