# YH.Flow — Roadmap

**Version:** 2.0.0
**Date:** 2026-06-26

---

## Overview

```
v1.0 (Backend Core): Phase 0~13 — 完成后端 API 核心
v2.0 (Flow Web Frontend): Phase 14~20 — 构建完整 React 前端
```

**v2.0 Strategy:** 基于 Plane Web（apps/web/）渐进改造，保留 UI 组件，逐步将 API 层从 Django REST 切换到 .NET 后端。

---

## Phases

- [x] **Phase 14: 脚手架 & Auth** — 前端开发环境搭建，Fork Plane 包，JWT 认证流程，API 适配层
- [ ] **Phase 15: 工作区 & 项目** — 工作区仪表板/设置/成员管理，项目管理，侧边栏/顶栏/Emoji 选择器
- [x] **Phase 16: Issue 列表/详情 & 看板** — Issue CRUD、列表视图、详情页、评论、批量操作、看板视图、命令面板
      (completed 2026-06-29)
- [ ] **Phase 17: 日历/甘特/电子表格 & 筛选引擎** — Issue 日历/甘特/电子表格视图，通用筛选/排序/自定义列引擎
- [ ] **Phase 18: 周期 & 模块** — Cycle 列表/详情/看板，Module 列表/详情
- [ ] **Phase 19: 页面 & 视图** — Page 树形层级/TipTap 编辑器/权限/收藏，自定义视图保存与应用
- [ ] **Phase 20: 通知 & 分析** — SSE 实时通知，分析仪表板，响应式布局收尾

---

## Phase Details

### Phase 14: 脚手架 & Auth

**Goal**: 前端开发环境就绪，已 Fork 的 Plane 包可编译，用户可使用 JWT 认证完成注册/登录/退出

**Depends on**: Phase 13 (v1.0 后端完成)

**Requirements**: SCAFF-01, SCAFF-02, SCAFF-03, SCAFF-04, SCAFF-05, SCAFF-06, SCAFF-07, SCAFF-08, SCAFF-09, AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05, AUTH-06, UI-03

**Success Criteria** (what must be TRUE):

1. 开发环境 React 19 + Vite 7 + TypeScript 5 + Tailwind CSS 4 可运行并显示页面
2. @plane/ui、@plane/editor、@plane/types、@plane/utils 四个包已 Fork 到 `src/lib/` 目录并可编译通过
3. 用户在登录页面可用邮箱密码登录（JWT），登录后 token 持久化到 localStorage，刷新页面不丢失会话
4. 用户可在注册页面创建账户，可使用忘记密码/重置密码功能
5. 用户可点击退出登录按钮清除 token 并跳转到登录页；未认证用户访问受保护路由自动重定向到登录页
6. Axios 实例已配置 JWT Bearer 拦截器自动附加 token；.NET API 返回 SnakeCase 格式和 `{"error":"message"}` 统一错误响应；分页响应格式兼容 Plane 前端
7. 用户可在暗色和亮色主题之间切换，切换后刷新保持偏好

**Plans**: 4 plans

```
Plans:
- [x] 14-01-PLAN.md — 脚手架 & 主题（Vite + React Router + Tailwind + Fork types/utils + 暗色/亮色主题）
- [x] 14-02-PLAN.md — Fork UI & 编辑器（Fork @plane/ui + @plane/editor 剥离 Yjs）
- [x] 14-03-PLAN.md — API 适配层（FlowApiService + JWT + humps + 错误标准化 + Vite proxy + .NET SnakeCase）
- [x] 14-04-PLAN.md — 认证页面（登录/注册/忘记密码/重置密码 + AuthStore + 路由保护）
```

**UI hint**: yes

---

### Phase 15: 工作区 & 项目

**Goal**: 用户可管理工作区和项目，侧边栏/顶栏提供全局导航

**Depends on**: Phase 14

**Requirements**: WORK-01, WORK-02, WORK-03, WORK-04, PROJ-01, PROJ-02, PROJ-03, PROJ-04, PROJ-05, UI-01, UI-02, UI-05

**Success Criteria** (what must be TRUE):

1. 用户登录后可看到工作区仪表板（项目概览、最近活动），可通过下拉菜单在工作区之间切换
2. 用户可管理工作区设置（名称、描述、Logo）和管理工作区成员（列表展示、角色变更）
3. 用户可在工作区内创建项目，看到项目列表（含搜索/排序），点击进入项目详情/仪表板
4. 用户可管理项目成员，通过项目选择器在项目间导航切换
5. 侧边栏显示工作区/项目的树形结构导航，当前所在位置高亮；顶部导航栏显示用户头像、通知图标和搜索入口
6. 用户在创建/编辑工作区和项目时可以使用 Emoji 图标选择器

**Plans**: 4 plans

```
Plans:
- [x] 15-01-PLAN.md — 基础设施（TanStack Query + MobX Stores + Mock 数据 + EmojiPicker）
- [x] 15-02-PLAN.md — 布局（侧边栏 + 顶栏 + 路由结构）
- [x] 15-03-PLAN.md — 工作区页面（Dashboard + 设置 + 成员 + 创建）
- [x] 15-04-PLAN.md — 项目页面（列表 + 创建 + Dashboard + 设置 + 成员 + 选择器）
```

**UI hint**: yes

---

### Phase 16: Issue 列表/详情 & 看板

**Goal**: 用户可使用完整 Issue 管理功能，包括列表视图和看板视图

**Depends on**: Phase 15

**Requirements**: ISSU-01, ISSU-02, ISSU-03, ISSU-04, ISSU-05, ISSU-06, ISSU-07, ISSU-08, KANB-01, KANB-02, KANB-03, KANB-05, UI-04

> **Note:** KANB-04 (子分组/Swimlane) 已推迟到 Phase 17 实现，per D-P16-15。

**Success Criteria** (what must be TRUE):

1. 用户可在项目下创建 Issue（填写标题、描述、选择优先级/负责人/标签），创建后自动跳转到详情页
2. 用户看到 Issue 列表视图，可按状态列筛选、按字段排序、分页浏览
3. 用户打开 Issue 详情页可看到属性面板（状态/优先级/负责人/标签/估算/截止日期）、评论列表和活动日志
4. 用户可编辑 Issue 属性（状态、优先级、负责人、标签、估算、截止日期），可软删除 Issue
5. 用户可创建/编辑/删除评论，可批量选中 Issue 执行状态变更/指派/删除操作
6. 用户在 Issue 描述中可使用 TipTap 富文本编辑器（加粗、列表、标题等）
7. 用户看到 Issue 看板视图（列 = 状态列），通过拖拽卡片在列之间移动（即变更状态）
8. 用户在看板中可按负责人/优先级分组，可在看板内筛选和排序
9. 用户可通过 Cmd+K 命令面板快速搜索和导航

**Plans**: TBD
**UI hint**: yes

---

### Phase 17: 日历/甘特/电子表格 & 筛选引擎

**Goal**: 用户可使用多种 Issue 视图和可复用的筛选/排序/自定义列引擎

**Depends on**: Phase 16

**Requirements**: CALN-01, CALN-02, GANT-01, GANT-02, SHEE-01, SHEE-02, FILT-01, FILT-02, FILT-03, FILT-04

**Success Criteria** (what must be TRUE):

1. 用户看到 Issue 日历视图（按截止日期/开始日期在日历上展示），可通过拖拽调整 Issue 日期
2. 用户看到 Issue 甘特图视图（横向时间轴展示 Issue 跨度），可查看 Issue 依赖关系连线
3. 用户看到 Issue 电子表格视图（表格形式展示字段列），可在表格中内联编辑 Issue 属性
4. 用户可按状态、优先级、负责人、标签组合筛选 Issues，可按任意字段升序/降序排序
5. 用户可自定义列表/看板/日历/甘特/电子表格视图中显示的列，可将当前筛选/排序/列配置保存为自定义视图（保存的视图在 Phase 19 中管理和应用）

**Plans**: TBD
**UI hint**: yes

---

### Phase 18: 周期 & 模块

**Goal**: 用户可使用周期（迭代/冲刺）和模块（功能分组/史诗）管理功能

**Depends on**: Phase 17（依赖筛选引擎）

**Requirements**: CYCLE-01, CYCLE-02, CYCLE-03, CYCLE-04, MODU-01, MODU-02, MODU-03

**Success Criteria** (what must be TRUE):

1. 用户看到 Cycle 列表页面（分为活跃/已完成/全部三个 Tab），可创建/编辑 Cycle（名称、描述、起止日期）
2. 用户打开 Cycle 详情页可看到进度条、Burndown 图表和关联 Issue 列表
3. 用户在 Cycle 看板视图中可管理 Issue 分配（将 Issue 添加/移出 Cycle、拖拽排序）
4. 用户看到 Module 列表页面，可创建/编辑 Module（名称、描述、状态）
5. 用户打开 Module 详情页可看到进度条和关联 Issue 列表

**Plans**: TBD
**UI hint**: yes

---

### Phase 19: 页面 & 视图

**Goal**: 用户可使用文档页面（TipTap 编辑器）和管理/应用自定义视图

**Depends on**: Phase 18

**Requirements**: PAGE-01, PAGE-02, PAGE-03, PAGE-04, PAGE-05, VIEW-01, VIEW-02, VIEW-03

**Success Criteria** (what must be TRUE):

1. 用户看到 Page 列表（树形层级结构展示），可展开/折叠子页面
2. 用户使用 TipTap 编辑器创建/编辑 Page 内容，可归档/删除 Page
3. 用户可将 Page 设置为 Public（工作区内所有人可查看）或 Private（仅创建者可查看），可收藏/星标 Page
4. 用户看到已保存的自定义视图列表，可点击应用视图（自动切换到该视图的筛选/排序/分组配置）
5. 用户在任意 Issue 视图中可将当前筛选/排序/分组/列配置保存为新视图

**Plans**: TBD
**UI hint**: yes

---

### Phase 20: 通知 & 分析

**Goal**: 用户可使用实时通知系统和分析仪表板，页面完成响应式适配

**Depends on**: Phase 19

**Requirements**: NOTI-01, NOTI-02, NOTI-03, NOTI-04, ANAL-01, ANAL-02, ANAL-03, UI-06

**Success Criteria** (what must be TRUE):

1. 用户看到站内通知列表（按时间倒序），可通过 SSE 接收实时通知并自动弹出提示
2. 用户可标记通知为已读（单条/全部），点击通知可跳转到关联的 Issue 或评论
3. 用户看到工作区分析仪表板（Issue 按状态/优先级的统计图表、完成率月度趋势）
4. 用户在项目级别查看分析图表，可导出分析数据为 CSV 文件
5. 所有页面在桌面端和平板设备上均可正常显示和操作（内容不溢出、布局不断裂）

**Plans**: TBD
**UI hint**: yes

---

## Progress

| Phase                             | Plans Complete | Status      | Completed  |
| --------------------------------- | -------------- | ----------- | ---------- |
| 14. 脚手架 & Auth                 | 4/4            | Delivered   | ✅         |
| 15. 工作区 & 项目                 | 4/4            | Delivered   | ✅         |
| 16. Issue 列表/详情 & 看板        | 5/5            | Complete    | 2026-06-29 |
| 17. 日历/甘特/电子表格 & 筛选引擎 | 4/6 | In Progress|  |
| 18. 周期 & 模块                   | 0/0            | Not started | -          |
| 19. 页面 & 视图                   | 0/0            | Not started | -          |
| 20. 通知 & 分析                   | 0/0            | Not started | -          |

---

## Requirement Coverage Map

| Requirement                | Phase    | Status      |
| -------------------------- | -------- | ----------- |
| SCAFF-01 ~ SCAFF-09        | Phase 14 | Delivered   |
| AUTH-01 ~ AUTH-06          | Phase 14 | Delivered   |
| UI-03                      | Phase 14 | Delivered   |
| WORK-01 ~ WORK-04          | Phase 15 | Pending     |
| PROJ-01 ~ PROJ-05          | Phase 15 | Pending     |
| UI-01, UI-02, UI-05        | Phase 15 | Pending     |
| ISSU-01 ~ ISSU-08          | Phase 16 | Pending     |
| KANB-01 ~ KANB-03, KANB-05 | Phase 16 | Pending     |
| KANB-04                    | Phase 17 | Pending     |
| UI-04                      | Phase 16 | Pending     |
| CALN-01 ~ CALN-02          | Phase 17 | Pending     |
| GANT-01 ~ GANT-02          | Phase 17 | In Progress |
| SHEE-01 ~ SHEE-02          | Phase 17 | Pending     |
| FILT-01 ~ FILT-04          | Phase 17 | Pending     |
| CYCLE-01 ~ CYCLE-04        | Phase 18 | Pending     |
| MODU-01 ~ MODU-03          | Phase 18 | Pending     |
| PAGE-01 ~ PAGE-05          | Phase 19 | Pending     |
| VIEW-01 ~ VIEW-03          | Phase 19 | Pending     |
| NOTI-01 ~ NOTI-04          | Phase 20 | Pending     |
| ANAL-01 ~ ANAL-03          | Phase 20 | Pending     |
| UI-06                      | Phase 20 | Pending     |

**Coverage:**

- v2.0 requirements: 75 total
- Mapped to phases: 75
- Unmapped: 0 ✓
