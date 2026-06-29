# YH.Flow — Requirements

**Version:** 2.0.0
**Date:** 2026-06-26
**Status:** Draft — v2.0 Milestone

---

## Architecture Principles

- **AP**: API 兼容优先 — 前端保持与 Plane Web 一致的 UI 风格和交互模式
- **PR**: 渐进迁移 — 保留 Plane Web 组件，逐步替换 API 层调用目标
- **SS**: 严格状态分离 — TanStack Query 管理服务端数据，MobX 仅管理 UI 状态
- **JM**: JWT 管理 — 统一认证方案为 JWT Bearer，废弃 Session Cookie + CSRF
- **FD**: Fork 不修改 — Plane 包 fork 后只在必要时修改，用 `// FLOW:` 标记
- **NC**: 原始代码不可变 — Plane 源码仅作参考

---

## v2.0 Requirements

### 脚手架 & 基础设施 (SCAFF)

- [ ] **SCAFF-01**: Flow Web 开发环境搭建（React 19 + Vite 7 + TypeScript 5 + Tailwind CSS 4）
- [ ] **SCAFF-02**: Fork @plane/ui 组件库到本地并配置构建流程
- [ ] **SCAFF-03**: Fork @plane/editor（TipTap）到本地，剥离 Yjs 协作依赖
- [ ] **SCAFF-04**: Fork @plane/types 和 @plane/utils 到本地
- [ ] **SCAFF-05**: 配置 Vite proxy 或 CORS 使前端可调用 .NET API
- [ ] **SCAFF-06**: 配置 .NET 端全局 `JsonNamingPolicy.SnakeCaseLower` 及日期格式
- [ ] **SCAFF-07**: 实现 Axios 实例 + JWT Bearer 拦截器（替代 `withCredentials`）
- [ ] **SCAFF-08**: 适配 .NET 错误响应格式为 `{"error": "message"}` 兼容 Plane 前端
- [ ] **SCAFF-09**: 构建 PlanePagedResult 与前端分页的适配层

### 认证 (AUTH)

- [ ] **AUTH-01**: 用户可以在登录页面使用邮箱密码登录（JWT）
- [ ] **AUTH-02**: 用户可以在注册页面创建账户
- [ ] **AUTH-03**: 用户可以使用忘记密码/重置密码功能
- [ ] **AUTH-04**: 用户登录后 session 持久化（JWT token 存储 + 刷新）
- [ ] **AUTH-05**: 用户可以直接退出登录
- [ ] **AUTH-06**: 未认证用户自动重定向到登录页

### 工作区 (WORK)

- [ ] **WORK-01**: 用户可以看到工作区仪表板（项目概览、最近活动）
- [ ] **WORK-02**: 用户可以在工作区之间切换
- [ ] **WORK-03**: 用户可以管理工作区设置（名称、描述、Logo）
- [ ] **WORK-04**: 用户可以管理工作区成员（列表、角色管理）

### 项目 (PROJ)

- [ ] **PROJ-01**: 用户可以在工作区内创建项目
- [ ] **PROJ-02**: 用户可以看到项目列表（含搜索/排序）
- [ ] **PROJ-03**: 用户可以看到项目详情/仪表板
- [ ] **PROJ-04**: 用户可以管理项目成员
- [ ] **PROJ-05**: 用户可以在项目间导航切换

### Issue 列表 & 详情 (ISSU)

- [ ] **ISSU-01**: 用户可以在项目下创建 Issue（标题、描述、优先级、负责人、标签）
- [ ] **ISSU-02**: 用户可以看到 Issue 列表视图（列状态筛选、排序、分页）
- [ ] **ISSU-03**: 用户可以看到 Issue 详情页（属性面板、评论、活动日志）
- [ ] **ISSU-04**: 用户可以编辑 Issue 属性（状态、优先级、负责人、标签、估算、截止日期）
- [ ] **ISSU-05**: 用户可以删除 Issue（软删除）
- [ ] **ISSU-06**: 用户可以创建/编辑/删除评论
- [ ] **ISSU-07**: 用户可以对 Issue 执行批量操作（状态变更、指派、删除）
- [ ] **ISSU-08**: 用户可以在 Issue 描述中使用 TipTap 富文本编辑器

### Issue 看板视图 (KANB)

- [ ] **KANB-01**: 用户可以看到 Issue 看板视图（列 = 状态列）
- [ ] **KANB-02**: 用户可以通过拖拽在列之间移动 Issue（变更状态）
- [ ] **KANB-03**: 用户可以在看板中按负责人/优先级等字段分组（Group By）
- [ ] **KANB-04**: 用户可以在看板中做子分组（Sub-Group By Swimlane）
- [ ] **KANB-05**: 用户可以在看板内筛选和排序

### Issue 日历视图 (CALN)

- [ ] **CALN-01**: 用户可以看到 Issue 日历视图（按截止日期/开始日期展示）
- [ ] **CALN-02**: 用户可以通过拖拽调整 Issue 日期

### Issue 甘特图 (GANT)

- [x] **GANT-01**: 用户可以看到 Issue 甘特图视图
- [x] **GANT-02**: 用户可以在甘特图中查看 Issue 依赖关系

### Issue 电子表格 (SHEE)

- [ ] **SHEE-01**: 用户可以看到 Issue 电子表格视图（表格批量编辑）
- [ ] **SHEE-02**: 用户可以在电子表格中内联编辑属性

### 筛选 & 排序引擎 (FILT)

- [ ] **FILT-01**: 用户可以按状态、优先级、负责人、标签筛选 Issues
- [ ] **FILT-02**: 用户可以按任意字段排序 Issues
- [ ] **FILT-03**: 用户可以自定义显示列
- [ ] **FILT-04**: 用户可以将筛选配置保存为自定义视图

### 周期 (CYCLE)

- [ ] **CYCLE-01**: 用户可以看到 Cycle 列表（活跃/已完成/全部）
- [ ] **CYCLE-02**: 用户可以看到 Cycle 详情（进度、Burndown、关联 Issues）
- [ ] **CYCLE-03**: 用户可以在 Cycle 看板中管理 Issue 分配
- [ ] **CYCLE-04**: 用户可以创建/编辑 Cycle

### 模块 (MODU)

- [ ] **MODU-01**: 用户可以看到 Module 列表
- [ ] **MODU-02**: 用户可以看到 Module 详情（进度、关联 Issues）
- [ ] **MODU-03**: 用户可以创建/编辑 Module

### 页面 (PAGE)

- [ ] **PAGE-01**: 用户可以看到 Page 列表（含树形层级）
- [ ] **PAGE-02**: 用户可以使用 TipTap 编辑器创建/编辑 Page
- [ ] **PAGE-03**: 用户可以归档/删除 Page
- [ ] **PAGE-04**: 用户可以为 Page 设置访问权限（Public/Private）
- [ ] **PAGE-05**: 用户可以收藏/星标 Page

### 视图 (VIEW)

- [ ] **VIEW-01**: 用户可以看到已保存的自定义视图列表
- [ ] **VIEW-02**: 用户可以将当前筛选/排序/分组配置保存为新视图
- [ ] **VIEW-03**: 用户可以应用已保存的视图

### 通知 (NOTI)

- [ ] **NOTI-01**: 用户可以看到站内通知列表
- [ ] **NOTI-02**: 用户可以通过 SSE 接收实时通知（自动弹出）
- [ ] **NOTI-03**: 用户可以标记通知为已读
- [ ] **NOTI-04**: 用户可以点击通知跳转到关联 Issue/评论

### 分析 (ANAL)

- [ ] **ANAL-01**: 用户可以看到工作区分析仪表板（Issue 统计、完成率趋势）
- [ ] **ANAL-02**: 用户可以在项目级别查看分析图表
- [ ] **ANAL-03**: 用户可以导出分析数据（CSV）

### 全局 UI & 交互 (UI)

- [ ] **UI-01**: 侧边栏导航（工作区/项目树形结构，保持 Plane 风格）
- [ ] **UI-02**: 顶部导航栏（头像、通知、搜索）
- [ ] **UI-03**: 暗色/亮色主题切换
- [ ] **UI-04**: 命令面板（Cmd+K）
- [ ] **UI-05**: Emoji 图标选择器
- [ ] **UI-06**: 响应式布局（桌面优先，平板兼容）

---

## Deferred to Future Milestones

| Requirement                               | Reason                   |
| ----------------------------------------- | ------------------------ |
| Calendar/Gantt/Spreadsheet 中国际化(i18n) | 一期仅中文               |
| 实时协作编辑（Yjs/Hocuspocus）            | 技术复杂度高，非核心功能 |
| PDF/图片导出                              | 按需添加                 |
| 移动端适配                                | Web 优先，移动端后续     |
| 集成设置 UI（GitHub/GitLab/Slack）        | 后端 Phase 9 尚未完成    |

## Out of Scope

| Feature             | Reason                             |
| ------------------- | ---------------------------------- |
| 修改 Plane 原始代码 | 保持原始代码不变，方便拉取上游更新 |
| CI/CD 配置          | Phase 0 已决定暂不做               |
| 实时协作（Pages）   | Phase 0 已决定推迟                 |

## Traceability

| Requirement         | Phase    | Status    |
| ------------------- | -------- | --------- |
| SCAFF-01 ~ SCAFF-09 | Phase 14 | Pending   |
| AUTH-01 ~ AUTH-06   | Phase 14 | Pending   |
| UI-03               | Phase 14 | Pending   |
| WORK-01 ~ WORK-04   | Phase 15 | Pending   |
| PROJ-01 ~ PROJ-05   | Phase 15 | Pending   |
| UI-01, UI-02, UI-05 | Phase 15 | Pending   |
| ISSU-01 ~ ISSU-08   | Phase 16 | Pending   |
| KANB-01 ~ KANB-05   | Phase 16 | Pending   |
| UI-04               | Phase 16 | Pending   |
| CALN-01 ~ CALN-02   | Phase 17 | Pending   |
| GANT-01 ~ GANT-02   | Phase 17 | Completed |
| SHEE-01 ~ SHEE-02   | Phase 17 | Pending   |
| FILT-01 ~ FILT-04   | Phase 17 | Pending   |
| CYCLE-01 ~ CYCLE-04 | Phase 18 | Pending   |
| MODU-01 ~ MODU-03   | Phase 18 | Pending   |
| PAGE-01 ~ PAGE-05   | Phase 19 | Pending   |
| VIEW-01 ~ VIEW-03   | Phase 19 | Pending   |
| NOTI-01 ~ NOTI-04   | Phase 20 | Pending   |
| ANAL-01 ~ ANAL-03   | Phase 20 | Pending   |
| UI-06               | Phase 20 | Pending   |

**Coverage:**

- v2.0 requirements: 75 total
- Mapped to phases: 75
- Unmapped: 0 ✓

---

_Requirements defined: 2026-06-26_
_Last updated: 2026-06-26 after v2.0 roadmap creation_
