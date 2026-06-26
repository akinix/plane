# Research Summary: Flow Web 前端 (v2.0)

**合成日期:** 2026-06-26
**来源:** STACK.md, FEATURES.md, ARCHITECTURE.md, PITFALLS.md

---

## 总体策略

基于 Plane Web（apps/web/）渐进改造，**保留 UI 组件，逐步替换 API 层**。不是全新重写，而是有针对性的迁移。

## 关键发现

### 技术栈方案

| 组件               | Plane Web（当前）               | Flow Web（目标）        | 理由                   |
| ------------------ | ------------------------------- | ----------------------- | ---------------------- |
| Web 框架           | React Router v7（来自 Next.js） | React Router v7（继承） | 已迁移完成，继承即可   |
| 状态管理（服务端） | SWR 2.2.4 + MobX（混合）        | TanStack Query v5       | 严格分离服务器/UI 状态 |
| 状态管理（UI）     | MobX 6.12（混合）               | MobX 6.12（仅 UI）      | 筛选器、侧边栏、主题   |
| HTTP 客户端        | Axios + `withCredentials:true`  | Axios + JWT 拦截器      | 切换认证方式           |
| UI 组件            | @plane/propel                   | Fork 到本地             | 继承 Plane 组件        |
| 编辑器             | @plane/editor（TipTap）         | Fork，剥离 Yjs          | 无需实时协作           |
| 类型定义           | @plane/types                    | Fork 到本地             | 适配 .NET API 响应     |
| 认证               | Session Cookie + CSRF           | JWT Bearer              | 匹配 .NET Identity     |

### 架构转换

```
当前（Plane Web）:
  Component → MobX Store (混合 UI+Server) → Service → Django REST API
                              SWR ————↑

目标（Flow Web）:
  Component → TanStack Query hook → Axios Service → .NET API
  MobX（仅 UI 状态，不作 API 调用）
  SSE → invalidateQueries → 自动刷新
```

### 页面结构与构建顺序

**~50+ 路由**，覆盖 7 大功能板块。建议构建顺序（按后端完成度）：

| 顺序 | 功能模块              | 后端状态 | 复杂度     |
| ---- | --------------------- | -------- | ---------- |
| 1    | 脚手架 + Auth         | ✅       | ⭐         |
| 2    | Workspace + Project   | ✅       | ⭐⭐       |
| 3    | Issues（List/Kanban） | ✅       | ⭐⭐⭐⭐⭐ |
| 4    | Cycles + Modules      | ✅       | ⭐⭐⭐     |
| 5    | Pages + Views         | ✅       | ⭐⭐⭐⭐   |
| 6    | Notifications         | ✅       | ⭐⭐       |
| 7    | Analytics             | ✅       | ⭐⭐       |

### 需要 Fork 的 Plane 包

| 包                                     | 修改内容                        |
| -------------------------------------- | ------------------------------- |
| @plane/ui → 本地 `src/lib/ui/`         | 基本不动，或按需微调            |
| @plane/editor → 本地 `src/lib/editor/` | 剥离 Yjs 协作，保留 TipTap 核心 |
| @plane/types → 本地 `src/lib/types/`   | 适配 .NET API 响应类型          |
| @plane/utils → 本地 `src/lib/utils/`   | 基本不动                        |

### 要删除的依赖

- `@hocuspocus/provider`、`yjs`、`y-indexeddb`（实时协作）
- `react-i18next`（多语言）
- `comlink`（Web Worker）
- `@microsoft/clarity`（会话记录）
- Next.js 兼容 shim

---

## ⚠️ 主要风险（必须提前处理）

### 🔴 级别：阻塞

| #   | 风险               | 影响                   | 解决方案                            | 必须在  |
| --- | ------------------ | ---------------------- | ----------------------------------- | ------- |
| 1   | **分页格式不匹配** | 所有列表页分页失效     | .NET 端 `PlanePagedResult` 调整格式 | Phase 1 |
| 2   | **认证流程差异**   | 登录/注册完全无法工作  | 切换 JWT Bearer + Axios 拦截器      | Phase 1 |
| 3   | **错误响应格式**   | 前端错误处理全部失效   | .NET 端兼容 `{"error": "msg"}` 格式 | Phase 1 |
| 4   | **字段命名不匹配** | Store 静默无法填充数据 | .NET 全局 `SnakeCaseLower`          | Phase 1 |

### 🟡 级别：重要

- **TipTap + React 19 兼容性** — 验证后再进 Phase 5
- **MobX/TanStack Query 混合** — Phase 1 就确立严格分离模式
- **Plane 包分叉发散** — 用 `// FLOW:` 标记所有修改

---

## 推荐 Phase 顺序（共 7 Phase）

| Phase | 域                                       | 需处理的 Pitfalls      |
| ----- | ---------------------------------------- | ---------------------- |
| 1     | 脚手架 + Auth（分页/错误/认证/字段适配） | P1, P2, P3, P4, P8, P9 |
| 2     | Workspace + Project                      | P5（建立模式）         |
| 3     | Issues（List + Kanban）                  | P5（沿用模式）         |
| 4     | Cycles + Modules                         | —                      |
| 5     | Pages + Views（编辑器）                  | P7（TipTap 兼容性）    |
| 6     | Notifications（SSE）                     | —                      |
| 7     | Analytics + Polish                       | —                      |
