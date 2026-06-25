# YH.Flow — Requirements

**Version:** 0.1.0  
**Date:** 2026-06-16  
**Status:** Draft

---

## Architecture Principles

- **AP**: API 兼容优先 — 新 API 路由和响应格式与 Plane 保持一致
- **MP**: 模块化单体 — 严格模块边界，Contracts 分离
- **FS**: Fullstackhero 规范 — 遵守 10 条黄金法则
- **NC**: 原始代码不可变 — Plane 源码仅作参考

---

## Phase 1: Foundation — 基础设施搭建

### REQ-1.1: 项目结构初始化

- 基于 fullstackhero dotnet-starter-kit 创建 YH.Flow 项目结构
- 配置 Directory.Build.props、Directory.Packages.props
- 建立 BuildingBlocks 引用（Core, Shared, Persistence, Web, Caching, Eventing, etc.）
- 设置 Host 项目（YH.Flow.Api, YH.Flow.DbMigrator）
- 配置 .NET Aspire AppHost（PostgreSQL, Redis, MinIO, API）
- 移除不需要的模块（Catalog, Tickets, Chat, Billing）

### REQ-1.2: 数据库基础

- 配置 PostgreSQL 连接和 EF Core
- 建立 DbMigrator 控制台项目
- 配置 Migration 项目结构
- 实现多租户基础设施（Finbuckle）
- 配置 Schema 命名规范

### REQ-1.3: 认证与授权

- 实现 JWT Bearer 认证
- 实现 API Key 认证（`X-Api-Key` header）
- 实现 Session Cookie 认证（兼容 Plane 前端）
- 实现 OAuth Provider 框架（GitHub, GitLab, Gitea, Google）
- 实现基于权限的授权（`RequirePermission`）
- 实现 API Token 管理（CRUD）

### REQ-1.4: API 基础设施

- 配置 CORS、Security Headers、Rate Limiting
- 实现全局异常处理（兼容 Plane 错误格式）
- 实现分页（兼容 Plane `count/next/previous/results` 格式）
- 实现 API 版本控制（v1）
- 配置 Scalar/OpenAPI 文档
- 实现 Idempotency 中间件

---

## Phase 2: Workspace — 工作区管理

### REQ-2.1: 工作区 CRUD

- 创建工作区（含 slug 生成）
- 获取工作区详情
- 更新工作区设置
- 删除工作区（软删除）
- 列出用户所属工作区

### REQ-2.2: 工作区成员管理

- 邀请成员（生成邀请链接）
- 列出成员（含角色）
- 更新成员角色（Admin=20, Member=15, Guest=5）
- 移除成员
- 成员加入/离开工作区

### REQ-2.3: 工作区设置

- 通用设置（名称、描述、Logo）
- 默认 Issue 状态配置
- 默认标签配置
- 估算系统配置

### REQ-2.4: 工作区邀请

- 生成邀请链接/邮件
- 列出待处理邀请
- 撤销邀请
- 接受/拒绝邀请

---

## Phase 3: Project — 项目管理

### REQ-3.1: 项目 CRUD

- 在工作区内创建项目
- 获取项目详情
- 更新项目设置
- 删除项目（软删除）
- 列出工作区项目（支持筛选、排序）

### REQ-3.2: 项目成员管理

- 添加成员到项目
- 列出项目成员
- 更新项目成员角色
- 移除项目成员

### REQ-3.3: 项目设置

- 项目封面图片（Unsplash 集成）
- 项目标识符/前缀
- 项目描述
- 项目网络/可见性设置

---

## Phase 4: WorkItems — 工作项管理

### REQ-4.1: 状态管理 (State)

- 创建项目工作流状态
- 列出项目状态（含排序）
- 更新状态（名称、颜色、顺序）
- 删除状态（含归档）
- 默认状态集（Backlog, Todo, In Progress, Done, Cancelled）

### REQ-4.2: 标签管理 (Label)

- 创建标签（名称、颜色）
- 列出项目标签
- 更新标签
- 删除标签
- 按标签筛选 Issues

### REQ-4.3: Issue CRUD

- 创建 Issue（标题、描述、优先级、负责人、状态、标签、估算）
- 获取 Issue 详情
- 更新 Issue 属性
- 删除 Issue（软删除）
- 列出项目 Issues（支持筛选、排序、分页）
- 批量操作 Issues（状态变更、指派、删除）

### REQ-4.4: Issue 属性

- 优先级（Urgent, High, Medium, Low, None）
- 指派（单个/多个成员）
- 截止日期
- 开始日期
- 目标日期
- 父 Issue / 子 Issue 关联
- Issue 链接（blocking, blocked_by, relates_to, duplicate, etc.）
- 附件/文件上传
- 自定义属性（Properties）

### REQ-4.5: Issue 评论

- 创建评论
- 列出评论
- 更新评论
- 删除评论（软删除）
- 评论中的 @提及

### REQ-4.6: Issue 活动日志

- 记录所有 Issue 变更（属性修改、状态变更等）
- 列出 Issue 活动历史
- 记录评论创建/编辑

### REQ-4.7: 估算管理

- 创建估算点值
- 列出项目估算点
- 更新估算点
- 删除估算点
- 为 Issue 设置估算

### REQ-4.8: 收件箱 (Intake)

- 列出收件箱 Issues
- 创建收件箱条目
- 将收件箱条目转为正式 Issue
- 删除收件箱条目

### REQ-4.9: 导入/导出

- CSV 导出（含 XLSX 公式注入保护）
- JSON 导出
- CSV 导入
- JSON 导入
- 异步导出任务（后台处理，生成下载链接）

---

## Phase 5: Cycle — 周期管理

### REQ-5.1: Cycle CRUD

- 创建 Cycle（名称、日期范围、描述）
- 列出项目 Cycles（活跃/已完成/所有）
- 获取 Cycle 详情
- 更新 Cycle
- 删除 Cycle
- Cycle 状态流转（创建 → 开始 → 完成）

### REQ-5.2: Cycle-Issue 关联

- 添加 Issues 到 Cycle
- 从 Cycle 中移除 Issues（回退到 Backlog）
- 列出 Cycle 中的 Issues
- Issue 在 Cycle 之间的迁移
- Cycle 进度跟踪（Burndown 数据）

---

## Phase 6: Module — 模块管理

### REQ-6.1: Module CRUD

- 创建 Module（名称、描述、负责人、日期范围）
- 列出项目 Modules
- 获取 Module 详情
- 更新 Module
- 删除 Module
- Module 状态管理（Backlog → Planned → In Progress → Paused → Done → Cancelled）

### REQ-6.2: Module-Issue 关联

- 添加 Issues 到 Module
- 从 Module 移除 Issues
- 列出 Module 中的 Issues
- Module 进度跟踪

---

## Phase 7: Page — 文档管理

### REQ-7.1: Page CRUD (无实时协作)

- 创建 Page（标题、内容、访问级别）
- 获取 Page 详情
- 更新 Page（保存内容）
- 删除 Page（软删除）
- 列出项目/工作区 Pages
- Page 层级结构（嵌套/树形）
- Page 归档

### REQ-7.2: Page 功能

- 富文本内容存储（HTML/Markdown）
- 附件/图片嵌入
- 访问权限（Public/Private）
- 收藏/星标

---

## Phase 8: View — 视图管理

### REQ-8.1: View CRUD [x]

- 创建 View（名称、筛选条件、排序、展示列）
- 列出项目 Views
- 获取 View 详情
- 更新 View
- 删除 View
- 设为默认 View

### REQ-8.2: View 筛选 [x]

- 按状态筛选
- 按优先级筛选
- 按负责人筛选
- 按标签筛选
- 按日期范围筛选
- 自定义排序
- 自定义显示列

---

## Phase 9: Integration — 集成管理

### REQ-9.1: GitHub 集成

- OAuth 连接 GitHub 账户
- 列出 GitHub 仓库
- 关联仓库到项目
- 同步 GitHub Issues 到项目
- 同步项目 Issues 到 GitHub
- 双向评论同步
- 配置同步的项目/标签映射

### REQ-9.2: GitLab 集成

- OAuth 连接 GitLab 账户（含自托管）
- 列出 GitLab 项目
- 同步配置和双向同步

### REQ-9.3: Gitea 集成

- OAuth 连接 Gitea 实例
- 同步配置和双向同步

### REQ-9.4: Slack 集成

- OAuth 连接 Slack 工作区
- 配置项目通知同步
- 事件类型筛选（Issue 创建/更新、评论等）

### REQ-9.5: Unsplash 集成

- 搜索 Unsplash 图片
- 设为项目/工作区封面

---

## Phase 10: Webhook — Webhook 管理

### REQ-10.1: Webhook CRUD

- 创建 Webhook（URL、事件类型、密钥）
- 列出工作区 Webhooks
- 更新 Webhook
- 删除 Webhook
- 启用/禁用 Webhook

### REQ-10.2: Webhook 投递

- 事件触发（Issue/Project/Cycle/Module/Comment 等）
- 异步投递（Hangfire 后台任务）
- HMAC-SHA256 签名
- 重试机制（可配置次数和间隔）
- SSRF 防护（IP 白名单、域名黑名单）
- 投递日志（保留 14 天）

---

## Phase 11: Notification — 通知系统

### REQ-11.1: 站内通知

- 通知生成（Issue 指派、评论提及、项目邀请等）
- 通知列表（分页、已读/未读筛选）
- 标记已读
- 标记全部已读
- 通知偏好设置

### REQ-11.2: 邮件通知

- 邮件模板
- 异步发送（Hangfire）
- 通知事件配置（用户可选择接收类型）

---

## Phase 12: Analytics — 分析仪表板

### REQ-12.1: 工作区分析

- Issue 统计（按状态、优先级、负责人）
- 完成率趋势
- Cycle 进度概览
- 成员工作量分布

### REQ-12.2: 导出报告

- 分析数据导出（CSV/JSON）
- 图表数据 API

---

## Phase 13: Frontend — Flow Web 前端

### REQ-13.1: 项目初始化

- 基于 fullstackhero clients/dashboard 模板创建
- React 19 + Vite 7 + Tailwind CSS 4 + Radix UI
- 配置与 .NET API 的连接

### REQ-13.2: 布局与导航

- 侧边栏工作区/项目导航（保持 Plane 风格）
- 顶部导航栏
- 面包屑导航
- 响应式布局
- 主题切换（亮/暗）

### REQ-13.3: 页面实现

- 工作区仪表板
- 项目列表/详情
- Issue 列表/详情/创建/编辑
- Cycle 列表/看板/详情
- Module 列表/详情
- Page 列表/编辑器（基于 TipTap）
- View 管理
- 设置页面（工作区、项目、成员）

### REQ-13.4: 表单与交互

- React Hook Form + Zod 验证
- TanStack Query 数据获取和缓存
- 乐观更新（Optimistic Updates）
- 拖拽排序（Pragmatic Drag and Drop）
- 实时通知（SSE）

---

## Non-Functional Requirements

### NFR-1: Performance

- API 响应时间 < 200ms (P95)
- 支持数据分页（默认 30 条/页）
- EF Core 查询优化（NoTracking, SplitQuery）

### NFR-2: Security

- 所有端点强制认证（除公开端点）
- 多租户数据隔离（Finbuckle + `IHasTenant`）
- SSRF 防护（Webhook 调用）
- 输入验证（FluentValidation）
- XSS 防护
- Rate Limiting（认证端点 10/min）

### NFR-3: Reliability

- 软删除 + 60 天自动清理
- 后台任务重试机制
- 全局异常处理
- Structured Logging (Serilog)

### NFR-4: Maintainability

- 模块化单体 — 模块独立可替换
- 严格代码规范（.editorconfig, SonarAnalyzer）
- XML 文档注释
- 架构测试（NetArchTest）
