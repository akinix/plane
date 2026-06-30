---
phase: 1
slug: foundation
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-06-17
---

# Phase 1 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property               | Value                                                            |
| ---------------------- | ---------------------------------------------------------------- |
| **Framework**          | xunit + Shouldly + AutoFixture                                   |
| **Config file**        | `yh-flow/src/Tests/Architecture.Tests/Architecture.Tests.csproj` |
| **Quick run command**  | `dotnet test yh-flow/src/Tests/Architecture.Tests/ --no-build`   |
| **Full suite command** | `dotnet test yh-flow/src/Tests/ --no-build`                      |
| **Estimated runtime**  | ~30 seconds                                                      |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test yh-flow/src/Tests/Architecture.Tests/ --no-build`
- **After every plan wave:** Run `dotnet test yh-flow/src/Tests/ --no-build`
- **Before `/gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID  | Plan | Wave | Requirement | Threat Ref | Secure Behavior                                     | Test Type    | Automated Command                          | File Exists | Status     |
| -------- | ---- | ---- | ----------- | ---------- | --------------------------------------------------- | ------------ | ------------------------------------------ | ----------- | ---------- |
| 01-01-01 | 01   | 1    | REQ-1.2     | —          | DbMigrator 执行所有模块迁移成功                     | integration  | `dotnet test --filter "DbMigrator"`        | ❌ W0       | ⬜ pending |
| 01-02-01 | 02   | 1    | REQ-1.3     | T-01-01    | JWT Bearer 认证签发和验证                           | unit         | `dotnet test --filter "JwtAuth"`           | ❌ W0       | ⬜ pending |
| 01-02-02 | 02   | 1    | REQ-1.3     | T-01-02    | API Key 认证验证（X-Api-Key header）                | unit         | `dotnet test --filter "ApiKeyAuth"`        | ❌ W0       | ⬜ pending |
| 01-02-03 | 02   | 1    | REQ-1.3     | T-01-03    | Session Cookie 认证验证                             | unit         | `dotnet test --filter "SessionCookieAuth"` | ❌ W0       | ⬜ pending |
| 01-02-04 | 02   | 1    | REQ-1.3     | T-01-04    | OAuth Provider 框架注册和路由                       | unit         | `dotnet test --filter "OAuthProvider"`     | ❌ W0       | ⬜ pending |
| 01-03-01 | 03   | 2    | REQ-1.4     | —          | Plane 错误格式输出（error/error_code/error_detail） | unit         | `dotnet test --filter "PlaneError"`        | ❌ W0       | ⬜ pending |
| 01-03-02 | 03   | 2    | REQ-1.4     | —          | Plane 分页格式输出（count/next/previous/results）   | unit         | `dotnet test --filter "PlanePaging"`       | ❌ W0       | ⬜ pending |
| 01-03-03 | 03   | 2    | REQ-1.4     | T-01-05    | Rate Limiting 四层策略验证                          | integration  | `dotnet test --filter "RateLimiting"`      | ❌ W0       | ⬜ pending |
| 01-04-01 | 04   | 1    | NFR-2       | T-01-06    | 多租户数据隔离（Finbuckle 查询过滤器）              | architecture | `dotnet test --filter "TenantIsolation"`   | ✅ exists   | ⬜ pending |

_Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky_

---

## Wave 0 Requirements

- [ ] `yh-flow/src/Tests/Identity.Tests/` — 新建测试项目，覆盖 REQ-1.3 认证测试
- [ ] `yh-flow/src/Tests/Identity.Tests/ApiKeyAuthenticationTests.cs` — API Key 认证
- [ ] `yh-flow/src/Tests/Identity.Tests/SessionCookieAuthenticationTests.cs` — Session Cookie 认证
- [ ] `yh-flow/src/Tests/Identity.Tests/OAuthProviderFrameworkTests.cs` — OAuth 框架
- [ ] `yh-flow/src/Tests/Identity.Tests/PlaneFormatAdapterTests.cs` — 分页和错误格式适配
- [ ] Test project 创建: `dotnet new xunit -o yh-flow/src/Tests/Identity.Tests`

---

## Manual-Only Verifications

| Behavior                    | Requirement | Why Manual                              | Test Instructions                       |
| --------------------------- | ----------- | --------------------------------------- | --------------------------------------- |
| OAuth 回调重定向 + 双发凭证 | REQ-1.3     | 需要真实 OAuth Provider（Phase 9 实现） | 手动验证回调 URL 格式和 Cookie/JWT 发放 |
| CORS 跨域请求               | REQ-1.4     | 需要前端应用实际发起跨域请求            | 用浏览器 DevTools 验证 CORS headers     |
| Scalar/OpenAPI 文档可访问   | REQ-1.4     | 需要运行中的 API 服务                   | 访问 /scalar/v1 验证文档生成            |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
