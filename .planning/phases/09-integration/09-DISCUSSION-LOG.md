# Phase 9: Integration — 集成 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-25
**Phase:** 9-Integration
**Status:** Deferred to future milestone

---

## 模块架构

| Option                | Description                                          | Selected |
| --------------------- | ---------------------------------------------------- | -------- |
| 一个 Integration 模块 | 一个 `Modules.Integration` 模块包含所有 5 个集成     |          |
| 按提供者拆分子模块    | `Modules.Integration` 父模块，按提供者拆分子命名空间 | ✓        |
| 按领域拆分多模块      | GitHub 独立模块 + 其他集成合并                       |          |

**User's choice:** 按提供者拆分子模块
**Notes:** `Modules.Integration` 父模块，子命名空间分别为 GitHub / Slack / GitLab / Gitea / Unsplash

---

## 同步机制

| Option       | Description                                          | Selected |
| ------------ | ---------------------------------------------------- | -------- |
| 基于 Webhook | 实现 webhook 接收端点，GitHub 事件实时推送到 YH.Flow |          |
| 基于轮询     | Hangfire 定时任务查询 GitHub API                     |          |
| 混合         | Webhook 接收 + Hangfire 定时轮询兜底                 | ✓        |

**User's choice:** 混合
**Notes:** Webhook 接收端属于 Phase 9（接收 GitHub 事件），非 Phase 10（发往外部的 Webhook）

---

## 实现优先级

| Option                                   | Description             | Selected |
| ---------------------------------------- | ----------------------- | -------- |
| GitHub → Slack → Unsplash → GitLab/Gitea | 3 waves 由重到轻        |          |
| 所有集成并行框架再逐层深化               | 先建基础结构再深化 sync |          |
| GitHub 完整实现 → 其他统一 Wave          | GitHub 占大部分工作量   |          |

**User's choice:** 不集成了，全部放到以后的里程碑去实现
**Notes:** Phase 9 所有 5 个集成均推迟到后续里程碑，不在当前 v1.0 范围内

---

## Additional Discussions

| Question                   | Answer                          |
| -------------------------- | ------------------------------- |
| Phase 9 推迟后剩余阶段安排 | 继续执行后续阶段（Phase 10-13） |

---

## Deferred Ideas

### 整个 Phase 9 推迟到后续里程碑

- **GitHub 集成**（REQ-9.1）— GitHub OAuth + Issue/Comment/Repository 双向同步
- **GitLab 集成**（REQ-9.2）— OAuth（含自托管）+ 双向同步
- **Gitea 集成**（REQ-9.3）— OAuth + 双向同步
- **Slack 集成**（REQ-9.4）— OAuth + 项目通知同步
- **Unsplash 集成**（REQ-9.5）— 图片搜索 + 封面设置
