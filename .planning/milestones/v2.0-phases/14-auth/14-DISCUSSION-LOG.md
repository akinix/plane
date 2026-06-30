# Phase 14: 脚手架 & Auth - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-26
**Phase:** 14-脚手架 & Auth
**Areas discussed:** 项目目录结构, 包 Fork 策略, 认证页面设计, API 适配层

---

## 项目目录结构

| Option          | Description                                                                                       | Selected |
| --------------- | ------------------------------------------------------------------------------------------------- | -------- |
| 镜像 Plane 结构 | 镜像 Plane 的 apps/web/ 布局：app/（路由/页面）、core/（组件/Store/Hooks/服务）、styles/、public/ | ✓        |
| 按功能模块组织  | 按功能模块组织：auth/、workspace/、project/、issues/、shared/ 等                                  |          |
| 扁平化结构      | 简化版本：pages/、components/、hooks/、services/、store/ 扁平化                                   |          |

**User's choice:** 镜像 Plane 结构（推荐）
**Notes:** 保持与 Plane 一致的目录风格，降低开发者的迁移认知负担

| Option          | Description                                    | Selected |
| --------------- | ---------------------------------------------- | -------- |
| React Router v7 | 继续使用 Plane 的 React Router v7 文件系统路由 | ✓        |
| TanStack Router | 使用 TanStack Router，类型安全路由             |          |

**User's choice:** React Router v7（推荐）

| Option                | Description                                    | Selected |
| --------------------- | ---------------------------------------------- | -------- |
| clients/web/src/lib/  | 放在 clients/web/src/lib/ 下，每个包一个子目录 | ✓        |
| clients/web/packages/ | 放在 clients/web/packages/ 下                  |          |

**User's choice:** clients/web/src/lib/（推荐）

---

## 包 Fork 策略

| Option         | Description                                                 | Selected |
| -------------- | ----------------------------------------------------------- | -------- |
| 代码复制       | 直接复制代码到 src/lib/<package>/，修改处添加 // FLOW: 注释 | ✓        |
| Workspace 引用 | pnpm workspace 引用原始包路径                               |          |
| Git subtree    | git subtree 拆分原始包                                      |          |

**User's choice:** 代码复制（推荐）

| Option            | Description                                         | Selected |
| ----------------- | --------------------------------------------------- | -------- |
| Vite alias 映射   | 保持 @plane/\* 不变，Vite alias 直接映射到 src/lib/ | ✓        |
| 重命名为 @flow/\* | 改为 @flow/\* 命名空间                              |          |

**User's choice:** Vite alias 映射（推荐）

| Option             | Description                                     | Selected |
| ------------------ | ----------------------------------------------- | -------- |
| Fork 时剥离        | Fork @plane/editor 时即剥离 Yjs/Hocuspocus 依赖 | ✓        |
| 等 Phase 19 再剥离 | 先保留全部依赖，等 Phase 19（页面）时才剥离     |          |

**User's choice:** Fork 时剥离（推荐）

---

## 认证页面设计

| Option          | Description                               | Selected |
| --------------- | ----------------------------------------- | -------- |
| 保持 Plane 风格 | 保持 Plane 现有认证页面布局（居中卡片式） | ✓        |
| 重新设计简化版  | 重新设计，更简洁现代                      |          |

**User's choice:** 保持 Plane 风格（推荐）

| Option         | Description                            | Selected |
| -------------- | -------------------------------------- | -------- |
| 注册后自动登录 | 注册成功后自动登录，跳转到工作区创建页 | ✓        |
| 跳转到登录页   | 注册成功后跳转到登录页                 |          |

**User's choice:** 注册后自动登录（推荐）

| Option     | Description                         | Selected |
| ---------- | ----------------------------------- | -------- |
| 完整流程   | 实现完整的邮箱验证→重置密码页面流程 | ✓        |
| 简化为占位 | 仅预留跳转入口                      |          |

**User's choice:** 完整流程（推荐）

---

## API 适配层

| Option              | Description                                                            | Selected |
| ------------------- | ---------------------------------------------------------------------- | -------- |
| FlowApiService 基类 | 创建 FlowApiService 基类，request 拦截器加 JWT，response 处理 401 刷新 | ✓        |
| 全局 Axios 拦截器   | 仅使用全局拦截器单例，不修改 Service 类                                |          |

**User's choice:** 自定义 FlowApiService 基类（推荐）

| Option             | Description                                            | Selected |
| ------------------ | ------------------------------------------------------ | -------- |
| 响应拦截器转换     | response 拦截器用 humps 自动将 snake_case 转 camelCase | ✓        |
| 直接使用 SnakeCase | 前端直接使用 snake_case                                |          |

**User's choice:** 响应拦截器自动转换（推荐）

| Option              | Description                                            | Selected |
| ------------------- | ------------------------------------------------------ | -------- |
| 错误拦截器标准化    | 在 Axios 错误拦截器中标准化为统一的 ErrorResponse 格式 | ✓        |
| 各 Service 自行处理 | 每个 Service 自行处理格式转换                          |          |

**User's choice:** 错误拦截器标准化（推荐）

---

## Claude's Discretion

- 主题切换的 UI 位置（顶部导航栏 vs 侧边栏）留给后续实现决定
- 具体 Token 刷新策略（静默刷新 vs 弹出式重新登录）留待实现时根据后端 Token 过期策略决定

## Deferred Ideas

None — discussion stayed within phase scope.
