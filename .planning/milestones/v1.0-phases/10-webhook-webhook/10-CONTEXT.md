# Phase 10: Webhook — Webhook 管理 - Context

**Gathered:** 2026-06-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Webhook CRUD、异步投递、签名、SSRF 防护。模块从 fullstackhero 模板继承，已有完整基础设施。

**范围内（REQ-10.1 ~ REQ-10.2）：**

- WebhookSubscription CRUD（创建/删除/列表）
- 事件驱动异步投递（Hangfire）
- HMAC-SHA256 签名
- 重试机制（指数退避：30s, 2m, 10m, 1h）
- 投递日志记录
- 测试投递端点

**决策：保持 FSH 现有设计，不做 Plane 特定适配**（路由保持 `/api/v1/webhooks/*`，不改为 workspace-scoped 格式）

</domain>

<decisions>
## Implementation Decisions

### 模块状态

- **模块已完整存在** — FSH 模板自带的 Webhook 模块已全部就位
- **54 个单元测试全部通过** — Domain/Services/Validators 全覆盖
- **已接入 Host** — Api/Program.cs + DbMigrator/Program.cs 已注册
- **迁移已存在** — `Webhooks/20260403090248_InitialWebhooks` + `20260424165422_WebhooksMultiTenant`

### 适配策略

- **保持 FSH 现有设计** — 不做 Plane 兼容适配
- 不需要：SSRF 防护、localhost 验证、ProjectWebhook、路由改为 workspace-scoped、日志增强

### 主要工作

- **添加集成测试** — 类似其他模块的 Testcontainers/InMemory 集成测试覆盖 Webhook 功能
- 验证全链路（创建 → 事件触发 → 投递 → 日志记录）

### Claude's Discretion

- 集成测试模式沿用现有 Workspace.Tests 的 Testcontainers + InMemory 双重测试策略
- 事件触发测试：通过 IntegrationEvent 总线模拟触发 FanoutHandler → 验证 DispatchJob 调用
- 无需新增功能端点，现有 5 端点已覆盖需求

</decisions>

<canonical_refs>

## Canonical References

### 现有实现

- `yh-flow/src/Modules/Webhooks/` — 完整模块实现
- `yh-flow/src/Modules/Webhooks/Modules.Webhooks/Services/` — Dispatcher/DispatchJob/FanoutHandler/Signer/SecretProtector
- `yh-flow/src/Modules/Webhooks/Modules.Webhooks/Domain/` — WebhookSubscription, WebhookDelivery
- `yh-flow/src/Modules/Webhooks/Modules.Webhooks/Data/` — DbContext + EF 配置
- `yh-flow/src/Tests/Webhooks.Tests/` — 现有 54 个单元测试

### 集成测试模式参考

- Workspace.Tests Testcontainers 集成测试（`yh-flow/src/Tests/Workspace.Tests/Fixtures/`）
- 使用 InMemory EF Core 进行单用例测试 + Testcontainers Postgres 进行关系测试

</canonical_refs>

<code_context>

## Existing Code Insights

### Reusable Assets

- **WebhookSubscription**: `Create(url, events, secretHash)` 工厂方法, `GetEvents()`, `MatchesEvent(eventType)`, `Deactivate()`
- **WebhookDelivery**: 每个投递尝试独立行, `RecordResult(statusCode, success, errorMessage)`
- **WebhookDispatcher**: 封装 Hangfire `IBackgroundJobClient.Enqueue<T>()`
- **WebhookDispatchJob**: `[AutomaticRetry(4次)]`, 指数退避, 临时错误重试, 4xx 永久失败
- **WebhookFanoutHandler<TEvent>**: 领域事件桥接, 订阅匹配, 逐订阅入队
- **WebhookPayloadSigner**: HMAC-SHA256, header `X-Webhook-Signature: sha256=...`
- **WebhookSecretProtector**: ASP.NET Data Protection 加密密钥存储
- **WebhookDeliveryService**: 直接投递（用于测试端点）

### Integration Points

- Domain events: `IIntegrationEventHandler<T>` 通过 FanoutHandler 自动触发 webhook
- Hangfire: `IBackgroundJobClient` 入队 + `[AutomaticRetry]` 重试策略
- DbContext: `webhooks` schema, 多租户 (Finbuckle `IsMultiTenant`)

</code_context>

<deferred>
## Deferred Ideas

- SSRF 防护 — 保持 FSH 现有设计，不做添加
- Plane 兼容路由（workspace slug 前缀）— 保持 FSH 现有 `/api/v1/webhooks`
- ProjectWebhook 绑定 — 按项目绑定 webhook，延后
- WebhookLog request/response 跟踪 — 当前 WebhookDelivery 只记录 status code，不记录 headers/body
- 14 天日志保留自动清理 — 延后

</deferred>

---

_Phase: 10-Webhook_
_Context gathered: 2026-06-25_
