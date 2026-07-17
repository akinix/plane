# Feature Research — Mock-to-Real API 集成

**Domain:** SPA 前端 Mock 数据到 .NET 后端 API 集成
**Researched:** 2026-06-30
**Confidence:** HIGH

## Feature Landscape

### Table Stakes (必备)

API 集成功能中，以下是每个模块不可或缺的。缺少这些 = 集成不完整。

| Feature                                 | Why Expected                                       | Complexity | Notes                                                    |
| --------------------------------------- | -------------------------------------------------- | ---------- | -------------------------------------------------------- |
| **认证 API 集成**                       | Auth 是入口，已使用 FlowApiService + 真实 API      | LOW        | 已完成，无需改动。AuthService 是唯一直接调用后端的服务   |
| **工作区 CRUD API**                     | 所有功能依赖工作区上下文                           | MEDIUM     | 当前 use-workspaces.ts 直接从 MOCK_WORKSPACES 返回       |
| **项目 CRUD API**                       | 工作和导航的基本单元                               | MEDIUM     | 当前 use-projects.ts 直接从 MOCK_PROJECTS 返回           |
| **Issue CRUD + 筛选/排序 API**          | 核心工作项。列表、详情、创建、更新、删除、批量操作 | HIGH       | 最复杂：use-issues.ts 包含筛选逻辑 + 乐观更新 + 批量操作 |
| **Issue 评论 CRUD API**                 | 与 Issue 强关联                                    | MEDIUM     | 评论 + 活动日志两个关联资源                              |
| **状态/标签/估算 API**                  | Issue 的引用数据                                   | MEDIUM     | State 和 Label 是 Issue 的前提数据                       |
| **周期 (Cycle) CRUD + Issue 关联 API**  | 迭代管理核心                                       | MEDIUM     | 含进度计算，依赖 Issue                                   |
| **模块 (Module) CRUD + Issue 关联 API** | 功能分组                                           | MEDIUM     | 含链接管理，依赖 Issue                                   |
| **页面 (Page) CRUD API**                | 文档管理基础                                       | MEDIUM     | CRUD + 归档/收藏/锁定                                    |
| **视图 (View) CRUD API**                | 自定义筛选/保存                                    | MEDIUM     | IssueViewService 已基于 FlowApiService，最容易迁移       |
| **通知 API**                            | 站内通知                                           | MEDIUM     | use-notifications.ts 已通过 service 类调用               |
| **分析仪表板 API**                      | 图表数据                                           | MEDIUM     | use-analytics.ts 已通过 service 类调用                   |
| **Webhook 管理 API**                    | 新建功能                                           | MEDIUM     | 后端已有完整模块                                         |

### 代码修改复杂度对比

| 集成方式                     | 涉及文件                                                                                              | 修改范围                                 | 复杂度 |
| ---------------------------- | ----------------------------------------------------------------------------------------------------- | ---------------------------------------- | ------ |
| **FlowApiService 实现**      | IssueViewService                                                                                      | 替换 `// FUTURE:` 注释代码块的 mock 实现 | 低     |
| **Service 类重写**           | NotificationService, AnalyticsService                                                                 | 保留类结构，替换数据来源为 HTTP          | 中     |
| **Hooks 直接引用 mock 数据** | useWorkspaces, useProjects, useIssues, useComments, useCycles, useModules, usePages, usePageMutations | queryFn 完整重写                         | 中-高  |
| **Mutation 乐观更新**        | useIssues（updateIssue）                                                                              | 需调整乐观更新逻辑匹配后端响应           | 高     |

## 当前架构状态

### 数据连接现状

| 模块          | 连接方式                  | 文件                                | 状态           |
| ------------- | ------------------------- | ----------------------------------- | -------------- |
| Auth          | 真实 API (FlowApiService) | auth.service.ts                     | ✅ 已完成      |
| Workspace     | Mock 数据 (hooks直接引用) | use-workspaces.ts                   | need migration |
| Project       | Mock 数据 (hooks直接引用) | use-projects.ts                     | need migration |
| Issue         | Mock 数据 (hooks直接引用) | use-issues.ts                       | need migration |
| Comments      | Mock 数据 (hooks直接引用) | use-comments.ts                     | need migration |
| Cycle         | Mock 数据 (hooks直接引用) | use-cycles.ts                       | need migration |
| Module        | Mock 数据 (hooks直接引用) | use-modules.ts                      | need migration |
| Page          | Mock 数据 (hooks直接引用) | use-pages.ts, use-page-mutations.ts | need migration |
| Issue Views   | Service类 (mock实现)      | issue-view.service.ts               | need migration |
| Notifications | Service类 (mock实现)      | notification.service.ts             | need migration |
| Analytics     | Service类 (mock实现)      | analytics.service.ts                | need migration |

### API 客户端架构

```
FlowApiService (抽象基类)
├── Request 拦截器: 自动附加 JWT Bearer Token
├── Response 拦截器:
│   ├── humps.camelizeKeys() → snake_case 到 camelCase 转换
│   └── 错误标准化 (401 → 清除token并跳转 / error/title/detail)
├── 方法: get, post, put, patch, delete, request
│
├── AuthService                 → 真实 API (已完成)
├── IssueViewService            → Mock 实现 (未完成，但已有 FUTURE 注释)
├── NotificationService         → Mock 实现 (未继承 FlowApiService)
└── AnalyticsService            → Mock 实现 (未继承 FlowApiService)
```

### 数据层模式

```
Hooks (TanStack Query v5)
├── Pattern A: 直接操作 Mock 数组 (workspaces, projects, issues, comments, cycles, modules, pages)
│   └── queryFn = async () => { await delay(N); return MOCK_X.filter(...) }
│   └── mutationFn = async () => { MOCK_X[idx] = { ... } }
│
└── Pattern B: 通过 Service 类调用 (notifications, analytics, views)
    └── queryFn = async () => service.method()
    └── service.method() 内部 = { await delay(N); return MOCK_X.filter(...) }
```

## 迁移策略

### 推荐策略: 增量式逐个模块迁移 (Per-Module Incremental)

**反对: 大爆炸式切换 (Big Bang)**

- 13+ 个模块同时切换风险极高
- 无法渐进式测试
- 后端未完成的端点导致前端大面积失效

**推荐: 增量式，每个模块独立完成 mock→real 切换**

### 模块迁移顺序

```
Phase 1: Auth (已完成) → Workspace → Project
          理由: 认证完成后，工作区和项目是后续所有操作的上下文

Phase 2: Issue (含 Status/Label/Estimate)
          理由: Issue 是核心实体，Cycle/Module/View 都依赖 Issue

Phase 3: Cycle → Module
          理由: 依赖 Issue 数据，同时与 Workspace/Project 交互

Phase 4: Page → View
          理由: 独立实体，不依赖其他模块

Phase 5: Notification → Analytics
          理由: 独立模块，变更风险低
```

### 每个模块的迁移模式

```
Step 1: 创建 Service 类 (如果不存在)
   ├── extends FlowApiService
   ├── BASE_URL = import.meta.env.VITE_API_BASE_URL
   └── 方法: 调用 this.get/this.post/this.put/this.patch/this.delete

Step 2: 替换 Hooks 中的 queryFn
   ├── 旧: queryFn = async () => { await delay(200); return MOCK_X.filter(...) }
   └── 新: queryFn = async () => { return myService.getList(workspaceId) }

Step 3: 替换 Hooks 中的 mutationFn
   ├── 旧: mutationFn = async (data) => { MOCK_X.push({...data, id: `mock-${Date.now()}`}) }
   └── 新: mutationFn = async (data) => { return myService.create(data) }

Step 4: 调整乐观更新逻辑
   └── onMutate 中的 setQueryData 可能需要适配后端实际响应

Step 5: 移除 mock-data.ts 中对应模块的数据
   └── 迁移完成后清理
```

## Feature Dependencies (模块间依赖关系)

```
Auth (已完成)
  └──requires──> 无

Workspace
  └──requires──> Auth (JWT token)

Project
  └──requires──> Workspace (slug/ID)
  └──requires──> Auth

Issue
  ├──requires──> Project (ID)
  ├──requires──> Workspace (slug)
  └──requires──> State/Label (引用数据)

Cycle
  ├──requires──> Project (ID)
  ├──requires──> Workspace (slug)
  └──enhances──> Issue (关联到 Cycle)

Module
  ├──requires──> Project (ID)
  ├──requires──> Workspace (slug)
  └──enhances──> Issue (关联到 Module)

Page
  ├──requires──> Workspace (slug)
  └──independent──> Issue

View
  ├──requires──> Project (ID)
  └──enhances──> Issue (筛选/排序)

Notification
  └──requires──> Workspace (slug)

Analytics
  └──requires──> Workspace (slug)

Webhook (新建功能)
  └──requires──> Workspace (slug)
  └──independent──> 其他模块
```

### 依赖说明

- **Cycle/Module → Issue**: Cycle-Issue 关联操作 (`useAddIssueToCycle`、`useRemoveIssueFromCycle`) 同时修改 Cycle 和 Issue 两端的数据，需要两个模块的 API 都就绪
- **View → Issue**: 自定义视图本质上是 Issue 筛选条件和展示布局的组合，不修改 Issue 数据，只需 Issue 可读
- **Page 独立性**: Page 是独立模块，不依赖 Issue/Cycle/Module，可以单独迁移
- **Webhook 独立性**: 管理页面是新建功能，不涉及 mock→real 迁移

## 关键实现细节

### 代理配置 (已存在)

```typescript
// vite.config.ts — API 代理已配置
proxy: {
  "/api": {
    target: process.env.VITE_API_PROXY_TARGET || "https://localhost:7030",
    changeOrigin: true,
    secure: false,
  },
}
```

VITE_API_BASE_URL=http://localhost:5173/api/v1 → Vite 代理 → https://localhost:7030

### 认证流程 (已存在)

AuthService 使用 JWT Bearer。登录成功后 token 存储到 localStorage，FlowApiService 的 request 拦截器自动附加到所有后续请求。

### SnakeCase 转换 (已存在)

FlowApiService 的 response 拦截器自动使用 `humps.camelizeKeys()` 将后端 snake_case 响应转换为前端 camelCase。

需要确认: 请求体是否需要 `humps.decamelizeKeys()` 转换为 snake_case 发送？当前代码中没有 request body 转换逻辑。

### 错误处理 (已存在)

```
401 → 清除 token → 重定向到 /auth/sign-in
    错误对象标准化: error || title || detail || "Unknown error"
```

### 分页参数

当前 mock 数据没有分页。实际 API 集成需要添加分页处理（Plane API 使用 `?limit=N&offset=M` 模式）。

### 缓存策略

TanStack Query v5 配置:

- 大多数 query 使用默认 staleTime
- `useUnreadCount` 使用 `refetchInterval: 30000`（30秒轮询）
- 乐观更新使用 `onMutate` → `setQueriesData` 模式（issues hooks 已实现回滚）

## 风险与注意事项

| 风险                              | 影响             | 缓解措施                               |
| --------------------------------- | ---------------- | -------------------------------------- |
| 后端端点尚未完成                  | 前端无法通过验收 | 按模块顺序集成，优先测试已完成后端模块 |
| snake_case 双向转换不一致         | 请求参数格式错误 | 确认是否需要 `humps.decamelizeKeys()`  |
| 乐观更新与后端响应冲突            | UI 状态不一致    | 迁移后验证乐观更新逻辑，可能需移除     |
| 分页处理缺失                      | 大量数据加载失败 | 添加分页参数支持                       |
| Mock ID (mock-xxx) 与真实 ID 冲突 | 缓存混乱         | 迁移后清除缓存，确保所有 ID 来自后端   |

## MVP 定义

### 当前 Milestone (v3.0) — 前端全部模块完成 API 集成

- [x] Auth (已完成)
- [ ] Workspace CRUD
- [ ] Project CRUD + 成员
- [ ] Issue CRUD + 筛选/排序 + 批量操作
- [ ] Issue 评论 + 活动日志
- [ ] Cycle CRUD + Issue 关联
- [ ] Module CRUD + Issue 关联
- [ ] Page CRUD + 归档/收藏
- [ ] View CRUD + 收藏
- [ ] Notification CRUD
- [ ] Analytics
- [ ] Webhook 管理页面（新建）

### 集成顺序建议 (按风险从低到高)

```
Phase A: Auth (已完成) + Views → Notifications → Analytics
  理由: IssueViewService 已基于 FlowApiService，Notification/Analytics 已是 service 模式
  改造成本最低，可快速验证集成Pipeline

Phase B: Workspace → Project
  理由: 基础上下文，数据模型简单，验证路由/参数/分页

Phase C: Page
  理由: 独立模块，模型简单

Phase D: Issue (核心)
  理由: 最大最复杂的模块，需要序列化/反序列化、乐观更新、分页全面验证

Phase E: Cycle → Module
  理由: 依赖 Issue 数据就绪

Phase F: Webhook (新建)
  理由: 独立功能，不涉及迁移
```

## Sources

- 代码分析: `yh-flow/clients/web/src/lib/services/flow-api.service.ts`
- 代码分析: `yh-flow/clients/web/src/lib/services/auth.service.ts`
- 代码分析: `yh-flow/clients/web/src/lib/services/issue-view.service.ts`（含 FUTURE 迁移注释）
- 代码分析: `yh-flow/clients/web/src/lib/services/notification.service.ts`
- 代码分析: `yh-flow/clients/web/src/lib/services/analytics.service.ts`
- 代码分析: `yh-flow/clients/web/src/lib/mock-data.ts`（17+ 个数据集合）
- 代码分析: `yh-flow/clients/web/src/lib/hooks/`（14个 hook 文件）
- 代码分析: `yh-flow/clients/web/vite.config.ts`（代理配置）
- 代码分析: `yh-flow/clients/web/.env`（环境变量）
- 项目文档: `.planning/PROJECT.md`

---

_Feature research for: Mock-to-Real API Integration_
_Researched: 2026-06-30_
