# Features Research: Plane Web → Flow Web

## Page/Route Structure

Plane Web 使用 React Router v7（CSR, `ssr: false`），路由在 `app/routes/core.ts` 中通过 `layout()` + `route()` 组合配置，约 50+ 页面路由。

### 路由板块

| 板块    | 路由前缀                                 | 页面                                 |
| ------- | ---------------------------------------- | ------------------------------------ |
| 认证    | `/accounts/`                             | 登录、注册、退出、忘记密码、重置密码 |
| 工作区  | `/workspace/`                            | 仪表板、项目列表、设置、成员管理     |
| 项目    | `/workspace/:slug/projects/`             | 项目仪表板、设置、成员               |
| Issues  | `/workspace/:slug/projects/:id/issues/`  | 列表、看板、日历、甘特、电子表格     |
| Cycles  | `/workspace/:slug/projects/:id/cycles/`  | 周期列表、详情、看板                 |
| Modules | `/workspace/:slug/projects/:id/modules/` | 模块列表、详情                       |
| Pages   | `/workspace/:slug/projects/:id/pages/`   | 页面列表、编辑器                     |
| Views   | `/workspace/:slug/projects/:id/views/`   | 视图列表、详情                       |

## 核心功能

### 五维 Issue 布局（核心差异化）

1. **List** — 表格列表，支持排序和列配置
2. **Kanban** — 看板视图，支持双分组（Group By + Sub-Group By/Swimlane）
3. **Calendar** — 日历视图，支持拖拽排期
4. **Gantt** — 甘特图，依赖关系可视化
5. **Spreadsheet** — 电子表格视图，批量编辑

所有 5 种布局共享筛选引擎、排序规则和显示属性配置（JSON 持久化）。

### 筛选/排序/分组引擎

- 按状态、优先级、负责人、标签、日期范围筛选
- 多级分组（Group By + Sub-Group By）
- 自定义排序字段
- 自定义显示列

### TipTap 编辑器

`@plane/editor` 独立包，20+ 自定义扩展：

- 斜杠命令 (`/`)
- @提及
- 图片嵌入
- 表格
- 代码块（含语法高亮）
- Callout / 警告框
- Emoji 选择器
- 任务列表
- 跨 Issue 和 Pages 复用

### 其他交互

- 拖拽排序（@atlaskit/pragmatic-drag-and-drop）
- 快捷键操作
- 暗色/亮色模式切换
- 命令面板（Cmd+K）

## 组件层次

```
packages/ui/        → @plane/ui — 30+ 基础 UI 组件
packages/editor/    → @plane/editor — TipTap 编辑器
packages/types/     → @plane/types — TypeScript 类型定义
packages/utils/     → @plane/utils — 工具函数

app/core/components/ → 业务组件
app/ce/             → 社区版扩展
app/ee/             → 企业版扩展
```

## Table Stakes vs Differentiators vs Anti-features

### Table Stakes（必须）

- 登录/注册（JWT 认证）
- 工作区 CRUD + 选择
- 项目 CRUD + 成员管理
- Issue CRUD + 列表/详情/看板
- Cycle/迭代管理
- Module/史诗分组
- Page/文档管理 + 富文本编辑器
- 自定义视图 + 筛选
- 通知（站内 + SSE）

### Differentiators（差异化功能）

- TipTap 富文本编辑器（代码块、任务列表、@提及）
- 看板 + 列表 + 日历 + 甘特 + 电子表格 五维 Issue 布局
- 强力筛选系统（多级分组、自定义排序）
- Emoji 图标选择器
- 暗色/高对比度主题
- 拖拽跨状态移动 Issues

### Anti-features（删除项）

- 实时协作编辑（Yjs/Hocuspocus）— 推迟到后续里程碑
- 多语言 i18n — 一期仅中文
- PDF 导出 — 按需添加
- Microsoft Clarity 会话记录
- Web Worker 编排（comlink）
