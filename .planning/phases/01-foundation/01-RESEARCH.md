# Phase 1: Foundation — 基础设施 - Research

**Researched:** 2026-06-16
**Domain:** 认证体系、API 基础设施、数据库迁移管道、多租户、可观测性
**Confidence:** HIGH

## Summary

Phase 1 的核心工作是在 Phase 0 已搭建的 FSH 脚手架上，扩展和适配基础设施层以支持 Plane API 兼容性。经过对 `yh-flow/src/` 全部源码和 Plane 原始认证代码的深度分析，研究发现：FSH 模板已经提供了约 70% 的 Phase 1 功能（JWT 认证、四层限流、全局异常处理、CORS/Security Headers、Serilog+OTel、Scalar/OpenAPI、DbMigrator 完整管道、Finbuckle 多租户），剩余 30% 需要新建或适配。

需要新建的核心组件包括：API Key 认证方案、Session Cookie 认证方案、OAuth Provider 框架（仅抽象，Phase 9 实现具体 Provider）、APIToken 实体和 CRUD 端点、Plane 兼容的错误格式和分页格式适配。需要适配的主要是端点路由映射（`/auth/sign-in` → FSH `GenerateToken` 等）和多 Scheme 协商注册。

**Primary recommendation:** 以 FSH 现有基础设施为基座，采用「扩展而非重写」策略——在 Identity 模块中添加新 AuthenticationScheme 和端点，在 BuildingBlocks/Web 中添加 Plane 格式适配层，保持 FSH 10 条黄金法则不变。

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** 多 Scheme 自动协商 — JWT Bearer / API Key / Session Cookie 各注册一个 ASP.NET Core AuthenticationScheme，按请求 Header 自动选择
- **D-02:** OAuth 登录成功后双发 JWT + Session Cookie — 前端可选择用 Cookie（自动携带）或 JWT（手动 Header），兼容 Plane 前端和 API 客户端
- **D-03:** API Key 数据库存储 + 多 Key 模型 — 参考 Plane APIToken，每个用户可创建多个 API Key，含名称、过期时间。存储在 Identity 模块的数据库表中
- **D-04:** 复用 FSH 认证端点 + 适配格式 — /auth/sign-in、/auth/sign-up、/auth/me 等端点复用 FSH Identity 模块实现，调整响应格式匹配 Plane
- **D-05:** Phase 1 只搭框架 — 抽象 Provider 接口 + 注册机制 + 回调路由，不实现具体 Provider（GitHub/GitLab/Gitea/Google 在 Phase 9 Integration 实现）
- **D-06:** OAuth 回调重定向 + 双发凭证 — 登录成功后重定向到前端指定 URL，同时发放 JWT + Cookie
- **D-07:** Provider 配置数据库动态管理 — 创建 OAuthProviderSettings 实体（ProviderName/ClientId/ClientSecret/CallbackUrl/Enabled/AutoCreateAccount），提供 CRUD 管理端点
- **D-08:** 配置实体放在 Identity 模块 — 含管理端点，启动时加载到内存缓存，修改后自动刷新
- **D-09:** 直接复用 FSH 四层限流方案 — Tenant（1000/60s）+ User（200/60s）+ IP（300/60s）+ Auth（10/60s），通过 appsettings.json 调整参数
- **D-10:** 限流响应用 FSH 的 RFC 7807 ProblemDetails 格式 — 429 + Retry-After header，不修改为 Plane 格式
- **D-11:** 仅全局配置 — 不做端点级限流覆盖，所有端点用同一套策略
- **D-12:** 模块独立 Schema — 每个模块有自己的 Schema（yhschema.Identity, yhschema.Workspace, yhschema.Project 等），遵循 FSH 模块边界原则
- **D-13:** 每个模块独立管理迁移 — 迁移放在各模块的 Data/Migrations/ 目录下
- **D-14:** 单一连接字符串 — 所有 DbContext 共用同一数据库，通过 Schema 分离
- **D-15:** DbMigrator 启动时执行迁移 — Aspire 编排中 DbMigrator 先于 API 运行，执行所有模块迁移 + Seed 数据

### Claude's Discretion

- EF Core 实体配置的具体实现细节（IEntityTypeConfiguration 的组织方式）
- Serilog Sink 配置和 OpenTelemetry Exporter 的具体选择
- CORS 策略的具体域名配置（开发阶段可宽松）
- Security Headers 的具体配置（Helmet 等中间件参数）
- 全局异常处理的具体错误格式适配（Plane 错误字段映射）
- 分页格式适配的具体实现（count/next/previous/results）
- Idempotency 中间件的具体实现

### Deferred Ideas (OUT OF SCOPE)

- **具体 OAuth Provider 实现** — Phase 9 Integration 实现 GitHub/GitLab/Gitea/Google Provider
- **Slack OAuth 集成** — Phase 9 Integration（REQ-9.4）
- **端点级限流覆盖** — 后续如有特殊端点需要差异化限流，可在 Phase 2+ 添加
- **Magic Link 认证** — Plane 支持 magic code 登录，一期不实现
- **Per-模块独立数据库** — 一期用单一数据库 + Schema 分离，后续如需物理隔离可迁移

</user_constraints>

<phase_requirements>

## Phase Requirements

| ID      | Description                                                     | Research Support                                                                |
| ------- | --------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| REQ-1.1 | 项目结构初始化                                                  | Phase 0 已完成，Phase 1 在此基础上扩展                                          |
| REQ-1.2 | 数据库基础 — PostgreSQL + EF Core + DbMigrator + 多租户         | FSH BaseDbContext、DbMigrator、Finbuckle 已就绪，仅需确认 Schema 策略和迁移管道 |
| REQ-1.3 | 认证与授权 — JWT + API Key + Session + OAuth 框架               | FSH JWT 完整可复用，需新建 API Key / Session Cookie schemes 和 OAuth 框架       |
| REQ-1.4 | API 基础设施 — CORS/Headers/RateLimit/异常/分页/OpenAPI/Serilog | FSH 全部已有实现，需适配 Plane 错误格式和分页格式                               |

</phase_requirements>

## Architectural Responsibility Map

| Capability                | Primary Tier                | Secondary Tier            | Rationale                                               |
| ------------------------- | --------------------------- | ------------------------- | ------------------------------------------------------- |
| 多租户解析 (Header/Query) | API Server (Middleware)     | —                         | Finbuckle 中间件在 UseRouting 前解析租户标识            |
| JWT Bearer 认证           | API Server (Auth Handler)   | —                         | ASP.NET Core JwtBearerHandler 处理 Authorization header |
| API Key 认证              | API Server (Auth Handler)   | Database (Lookup)         | 自定义 AuthenticationHandler 查 DB 验证 Key 哈希        |
| Session/Cookie 认证       | API Server (Auth Handler)   | —                         | ASP.NET Core CookieHandler 处理 Cookie header           |
| OAuth Provider 框架       | API Server (Endpoint)       | Database (Config Store)   | 回调端点处理 OAuth flow，配置存储在 DB                  |
| Rate Limiting             | API Server (Middleware)     | —                         | ASP.NET Core RateLimiter 四层策略                       |
| 全局异常处理              | API Server (Middleware)     | —                         | IExceptionHandler 拦截所有未处理异常                    |
| 分页格式适配              | API Server (Serialization)  | —                         | 自定义 JSON 序列化或 ResultFilter                       |
| CORS / Security Headers   | API Server (Middleware)     | —                         | ASP.NET Core 内置中间件                                 |
| Scalar / OpenAPI          | API Server (Endpoint)       | —                         | Scalar + AddOpenApi 自动生成文档                        |
| Serilog + OpenTelemetry   | API Server (Infrastructure) | Database (EF Core traces) | 结构化日志 + 分布式追踪                                 |
| EF Core 迁移管道          | Database                    | API Server (DbMigrator)   | DbMigrator 控制台执行，迁移文件在 Migrations 项目       |
| Schema 分离               | Database                    | API Server (DbContext)    | 每个模块 DbContext 设置 DefaultSchema                   |

## Standard Stack

### Core（全部已在 Phase 0 安装）

| Library               | Version          | Purpose                  | 状态      |
| --------------------- | ---------------- | ------------------------ | --------- |
| ASP.NET Core          | 10.0             | Web 框架 + 认证 + 中间件 | ✅ 已安装 |
| EF Core               | 10.0             | ORM + 迁移               | ✅ 已安装 |
| Finbuckle.MultiTenant | 10.x             | 多租户策略 + 隔离        | ✅ 已安装 |
| ASP.NET Identity      | 10.0             | 用户/角色管理            | ✅ 已安装 |
| Mediator              | 3.x (source-gen) | CQRS 命令/查询处理       | ✅ 已安装 |
| FluentValidation      | latest           | 请求验证                 | ✅ 已安装 |
| Serilog               | latest           | 结构化日志               | ✅ 已安装 |
| OpenTelemetry         | latest           | 分布式追踪 + 指标        | ✅ 已安装 |
| Scalar.AspNetCore     | latest           | API 文档 UI              | ✅ 已安装 |
| Npgsql                | latest           | PostgreSQL 驱动          | ✅ 已安装 |
| Hangfire              | latest           | 后台任务                 | ✅ 已安装 |

### Supporting（Phase 1 可能需要新增）

| Library                                     | Purpose                 | 何时使用                               |
| ------------------------------------------- | ----------------------- | -------------------------------------- |
| Microsoft.AspNetCore.Authentication.Cookies | Session Cookie 认证方案 | T1.6 — ASP.NET Core 内置，无需额外安装 |
| Microsoft.AspNetCore.Authentication.OAuth   | OAuth 基础框架          | T1.7 — ASP.NET Core 内置，无需额外安装 |

**注意：** Phase 1 不需要安装任何新的 NuGet 包。所有需要的认证方案（JWT Bearer、Cookie、OAuth 基础）都是 ASP.NET Core 10 的内置组件。API Key 认证需要自定义 AuthenticationHandler，不依赖外部库。

## Package Legitimacy Audit

> 本阶段不安装新的 NuGet 包，全部使用 Phase 0 已有的包。无需执行 slopcheck 验证。

## Architecture Patterns

### System Architecture Diagram

```
                         ┌─────────────────────────────────┐
                         │          Client Requests         │
                         │  (JWT Bearer / X-Api-Key / Cookie)│
                         └───────────────┬─────────────────┘
                                         │
                                         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          ASP.NET Core Middleware Pipeline                    │
│                                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  ┌────────────┐  │
│  │  Exception   │→ │    CORS      │→ │ Security Headers  │→ │  Routing   │  │
│  │  Handler     │  │              │  │                   │  │            │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  └─────┬──────┘  │
│                                                                   │         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐        │         │
│  │   Scalar     │← │   OpenAPI    │← │  Static Files    │←───────┤         │
│  │    UI        │  │   JSON       │  │                   │                  │
│  └──────────────┘  └──────────────┘  └──────────────────┘                  │
│                                                                   │         │
│  ┌──────────────────────────────────────────────────────────────┐ │         │
│  │              Finbuckle MultiTenant Middleware                  ││         │
│  │    Strategy Chain: Claim → Header(X-Tenant) → Query(tenant)  ││         │
│  └──────────────────────────┬───────────────────────────────────┘│         │
│                             │                                     │         │
│  ┌──────────────────────────▼───────────────────────────────────┐│         │
│  │           Authentication Middleware (Multi-Scheme)            ││         │
│  │  ┌──────────┐  ┌──────────────┐  ┌────────────────────────┐ ││         │
│  │  │  JWT     │  │  API Key     │  │  Session Cookie        │ ││         │
│  │  │  Bearer  │  │  X-Api-Key   │  │  .AspNetCore.Session   │ ││         │
│  │  └──────────┘  └──────────────┘  └────────────────────────┘ ││         │
│  └──────────────────────────┬───────────────────────────────────┘│         │
│                             │                                     │         │
│  ┌──────────────────────────▼───────────────────────────────────┐│         │
│  │           Rate Limiter (4-Layer Chained)                      ││         │
│  │    Tenant(1000/60s) → User(200/60s) → IP(300/60s) → Auth    ││         │
│  └──────────────────────────┬───────────────────────────────────┘│         │
│                             │                                     │         │
│  ┌──────────────────────────▼───────────────────────────────────┐│         │
│  │           Authorization (Permission-Based)                    ││         │
│  │    RequiredPermissionPolicy + FallbackPolicy                  ││         │
│  └──────────────────────────┬───────────────────────────────────┘│         │
└─────────────────────────────┼─────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │  Identity    │ │ Multitenancy │ │   Auditing   │  ... (more modules)
    │  Module      │ │   Module     │ │   Module     │
    │              │ │              │ │              │
    │ /auth/*      │ │ /tenants/*   │ │ /audit/*     │
    │ JWT/ApiKey/  │ │              │ │              │
    │ Cookie/OAuth │ │              │ │              │
    └──────┬───────┘ └──────┬───────┘ └──────┬───────┘
           │                │                │
           ▼                ▼                ▼
    ┌─────────────────────────────────────────────────┐
    │          PostgreSQL (Single Database)            │
    │  ┌──────────┐ ┌──────────┐ ┌──────────┐        │
    │  │identity  │ │tenant    │ │audit     │ ...    │
    │  │schema    │ │schema    │ │schema    │        │
    │  └──────────┘ └──────────┘ └──────────┘        │
    └─────────────────────────────────────────────────┘
```

### Recommended Project Structure (Phase 1 新增/修改)

```
yh-flow/src/
├── BuildingBlocks/
│   ├── Web/
│   │   ├── Exceptions/
│   │   │   ├── GlobalExceptionHandler.cs         # 修改：添加 Plane 格式适配
│   │   │   └── PlaneErrorFormat.cs               # 新增：Plane 错误格式模型
│   │   ├── Pagination/
│   │   │   └── PlanePagedResponse.cs              # 新增：Plane 分页格式
│   │   └── Auth/
│   │       ├── ApiKeyAuthenticationHandler.cs     # 新增：API Key Scheme
│   │       ├── ApiKeyAuthenticationOptions.cs     # 新增
│   │       ├── SessionCookieAuthenticationExtensions.cs  # 新增
│   │       └── CurrentUserMiddleware.cs           # 已有，可能需要适配
│   └── Shared/
│       └── Persistence/
│           ├── PagedResponse.cs                   # 已有
│           └── PlanePagedResult.cs                # 新增：{count, next, previous, results}
├── Modules/
│   └── Identity/
│       ├── Modules.Identity/
│       │   ├── Authorization/
│       │   │   ├── Jwt/                           # 已有 — JWT Bearer
│       │   │   ├── ApiKey/                        # 新增 — API Key 认证
│       │   │   │   ├── ApiKeyAuthenticationHandler.cs
│       │   │   │   └── ApiKeyAuthenticationExtensions.cs
│       │   │   └── SessionCookie/                 # 新增 — Session Cookie
│       │   │       └── SessionCookieAuthenticationExtensions.cs
│       │   ├── Domain/
│       │   │   ├── APIToken.cs                    # 新增：API Key 实体
│       │   │   └── OAuthProviderSettings.cs       # 新增：OAuth Provider 配置实体
│       │   ├── Data/
│       │   │   ├── Configurations/
│       │   │   │   ├── APITokenConfiguration.cs   # 新增
│       │   │   │   └── OAuthProviderSettingsConfiguration.cs  # 新增
│       │   │   └── IdentityDbContext.cs           # 修改：添加 DbSet<APIToken>, DbSet<OAuthProviderSettings>
│       │   ├── Features/v1/
│       │   │   ├── Auth/                          # 新增：Plane 兼容认证端点
│       │   │   │   ├── SignIn/PlaneSignInEndpoint.cs
│       │   │   │   ├── SignUp/PlaneSignUpEndpoint.cs
│       │   │   │   ├── SignOut/PlaneSignOutEndpoint.cs
│       │   │   │   ├── Me/PlaneMeEndpoint.cs
│       │   │   │   └── AuthEndpoints.cs           # MapGroup("/auth") 注册
│       │   │   ├── ApiTokens/                     # 新增：API Token CRUD
│       │   │   │   ├── Create/...
│       │   │   │   ├── List/...
│       │   │   │   ├── Revoke/...
│       │   │   │   └── Deactivate/...
│       │   │   └── OAuth/                         # 新增：OAuth Provider 框架
│       │   │       ├── IOAuthProvider.cs           # 抽象接口
│       │   │       ├── OAuthProviderRegistry.cs    # 注册机制 + 缓存
│       │   │       ├── OAuthCallbackEndpoint.cs    # 回调路由
│       │   │       ├── OAuthInitiateEndpoint.cs    # 发起 OAuth 流程
│       │   │       └── Manage/                    # Provider Settings CRUD
│       │   ├── Services/
│       │   │   ├── TokenService.cs                # 已有 — 扩展双发 JWT+Cookie
│       │   │   ├── ApiTokenService.cs             # 新增：API Key 管理
│       │   │   └── OAuthProviderService.cs        # 新增：Provider 配置管理
│       │   └── IdentityModule.cs                  # 修改：注册新 Scheme + 端点
│       └── Modules.Identity.Contracts/
│           ├── v1/
│           │   ├── ApiTokens/                     # 新增：Command/Query/Response
│           │   └── OAuth/                         # 新增：OAuth Commands
│           ├── DTOs/
│           │   ├── APITokenDto.cs                 # 新增
│           │   └── OAuthProviderSettingsDto.cs    # 新增
│           └── Services/
│               ├── IApiTokenService.cs            # 新增
│               └── IOAuthProviderService.cs       # 新增
├── Host/
│   ├── YH.Flow.Api/
│   │   ├── Program.cs                            # 修改：注册多 Scheme
│   │   └── appsettings.json                      # 修改：添加认证配置
│   ├── YH.Flow.DbMigrator/
│   │   └── Program.cs                            # 修改：注册新模块 assemblies
│   └── YH.Flow.Migrations.PostgreSQL/
│       ├── Identity/                              # 新增迁移文件
│       └── YH.Flow.Migrations.PostgreSQL.csproj   # 无需修改（已引用 Identity）
└── Tests/
    └── Identity.Tests/                            # 新增：认证测试
        ├── ApiKeyAuthenticationTests.cs
        ├── SessionCookieAuthenticationTests.cs
        ├── OAuthProviderFrameworkTests.cs
        └── PlaneFormatAdapterTests.cs
```

### Pattern 1: 多 Scheme 认证协商

**What:** 在同一 ASP.NET Core 应用中注册多个 AuthenticationScheme，根据请求中的 Header 自动选择合适的认证方式。

**When to use:** 所有需要认证的 API 端点。

**Key insight:** ASP.NET Core 的 AuthenticationMiddleware 按注册顺序尝试每个 Scheme。当某个 Scheme 成功时停止。当没有 Scheme 成功时返回 401。通过 `ForwardDefaultSelector` 或 `PolicyScheme` 可以实现智能路由。

**推荐实现方式：** 使用 PolicyScheme 作为 DefaultScheme，根据请求 Header 转发到具体 Scheme：

```csharp
// Source: yh-flow IdentityModule.cs + ASP.NET Core docs
// 在 IdentityModule.ConfigureServices 中扩展

services.AddAuthentication(options =>
{
    // PolicyScheme 作为入口，根据请求自动选择
    options.DefaultScheme = "SmartSelector";
    options.DefaultChallengeScheme = "SmartSelector";
    options.DefaultAuthenticateScheme = "SmartSelector";
})
.AddPolicyScheme("SmartSelector", "Smart Selector", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // 1. X-Api-Key header → API Key scheme
        if (context.Request.Headers.ContainsKey("X-Api-Key"))
            return "ApiKey";
        // 2. Cookie → Session Cookie scheme
        if (context.Request.Headers.ContainsKey("Cookie") &&
            context.Request.Cookies.ContainsKey(".YHFlow.Session"))
            return "SessionCookie";
        // 3. Authorization: Bearer → JWT (default)
        return "Bearer";
    };
})
.AddJwtBearer("Bearer", /* 已有配置 */)
.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null!)
.AddCookie("SessionCookie", options =>
{
    options.Cookie.Name = ".YHFlow.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        // API 不应返回 302 重定向，改为 401
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});
```

### Pattern 2: API Key 认证处理器

**What:** 自定义 AuthenticationHandler 从 X-Api-Key header 读取 Key，查数据库验证。

**When to use:** API 客户端（非浏览器）的程序化访问。

**参考 Plane 实现：** `apps/api/plane/api/middleware/api_authentication.py` — 从 `X-Api-Key` header 获取 token，查 APIToken 表验证。

```csharp
// Source: Plane APIKeyAuthentication + FSH JwtBearer pattern
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeader))
            return AuthenticateResult.NoResult();

        var rawKey = apiKeyHeader.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawKey))
            return AuthenticateResult.NoResult();

        // 查数据库验证（使用哈希比对，不存储明文）
        var apiTokenService = Context.RequestServices.GetRequiredService<IApiTokenService>();
        var token = await apiTokenService.ValidateAndGetOwnerAsync(rawKey, Context.RequestAborted);

        if (token is null)
            return AuthenticateResult.Fail("Invalid or expired API key");

        // 构建 ClaimsPrincipal（与 JWT 相同的 Claim 结构）
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, token.UserId.ToString()),
            new(ClaimTypes.Email, token.UserEmail),
            new(ClaimConstants.Tenant, token.TenantId),
            // ... permissions from user roles
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
```

### Pattern 3: APIToken 实体设计

**What:** 参考 Plane APIToken 模型的 API Key 数据库实体。

```csharp
// Source: Plane plane.db.models.APIToken
public sealed class APIToken : AggregateRoot<Guid>, IAuditableEntity, IHasTenant
{
    private APIToken() { }

    public static APIToken Create(string name, Guid userId, string tokenHash,
        string prefix, DateTime? expiredAt)
    {
        return new APIToken
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId,
            TokenHash = tokenHash,      // SHA-256 hash of the full key
            Prefix = prefix,             // "pk_" + first 8 chars for display
            ExpiredAt = expiredAt,
            IsActive = true,
            LastUsed = null,
        };
    }

    public string Name { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public string Prefix { get; private set; } = default!;  // 用于列表显示 "pk_abc123..."
    public DateTime? ExpiredAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastUsed { get; private set; }

    // 行为方法
    public void RecordUsage() => LastUsed = DateTime.UtcNow;
    public void Revoke() => IsActive = false;
    public bool IsExpired() => ExpiredAt.HasValue && ExpiredAt.Value < DateTime.UtcNow;
}
```

### Pattern 4: OAuth Provider 框架（Phase 1 只搭框架）

**What:** 定义 Provider 抽象接口 + 注册机制 + 回调路由。具体 Provider（GitHub 等）在 Phase 9 实现。

**参考 Plane 实现：** `apps/api/plane/authentication/adapter/oauth.py` — OauthAdapter 基类 + provider 子类。

```csharp
// Source: Plane OauthAdapter pattern → .NET 抽象
public interface IOAuthProvider
{
    string ProviderName { get; }           // "github", "gitlab", "gitea", "google"
    string GetAuthUrl(string state, string callbackUrl);
    Task<OAuthUserInfo> ExchangeCodeAsync(string code, string callbackUrl, CancellationToken ct);
}

public record OAuthUserInfo(
    string ProviderId,
    string Email,
    string? Avatar,
    string? FirstName,
    string? LastName
);

// Provider 注册表 — 启动时从 DB 加载配置到内存缓存
public sealed class OAuthProviderRegistry
{
    private readonly ConcurrentDictionary<string, IOAuthProvider> _providers = new();
    private readonly IOAuthProviderSettingsService _settingsService;

    public async Task InitializeAsync(CancellationToken ct)
    {
        var settings = await _settingsService.GetAllEnabledAsync(ct);
        foreach (var s in settings)
        {
            // Phase 9: 根据 s.ProviderName 实例化具体 Provider
            // Phase 1: 仅注册空壳，标记为 available=false
        }
    }

    public IOAuthProvider? GetProvider(string name)
        => _providers.TryGetValue(name, out var p) ? p : null;

    public IReadOnlyList<string> GetAvailableProviders()
        => _providers.Keys.ToList();
}

// OAuth 端点路由
// GET  /auth/oauth/{provider}          → 重定向到 Provider auth URL
// POST /auth/oauth/{provider}/callback → 处理回调，双发 JWT + Cookie
```

### Pattern 5: OAuthProviderSettings 实体

**What:** 数据库存储 OAuth Provider 配置，支持动态管理。

```csharp
// Source: D-07 decision
public sealed class OAuthProviderSettings : AggregateRoot<Guid>, IGlobalEntity
{
    public string ProviderName { get; private set; } = default!;  // "github", "gitlab", etc.
    public string ClientId { get; private set; } = default!;
    public string ClientSecret { get; private set; } = default!;  // 加密存储
    public string CallbackUrl { get; private set; } = default!;
    public bool Enabled { get; private set; }
    public bool AutoCreateAccount { get; private set; }  // 新用户自动创建账户
    public string? Scope { get; private set; }            // OAuth scope

    // IGlobalEntity — 全局配置，不按租户隔离
    // 或者根据需求改为 IHasTenant 实现 per-tenant OAuth 配置
}
```

### Pattern 6: Plane 兼容错误格式适配

**What:** 将 FSH 的 RFC 7807 ProblemDetails 转换为 Plane 前端期望的错误格式。

**Plane 错误格式：**

```json
{
  "error": "Error message",
  "error_code": 400,
  "error_detail": { "field": ["validation message"] }
}
```

**FSH 当前格式（RFC 7807 ProblemDetails）：**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/...",
  "errors": { "field": ["validation message"] },
  "traceId": "xxx",
  "correlationId": "yyy"
}
```

**推荐适配策略：** 在 GlobalExceptionHandler 中根据请求 Accept header 或路径前缀输出 Plane 格式。认证端点（`/auth/*`）使用 Plane 格式，API 端点（`/api/*`）保持 ProblemDetails 格式。

```csharp
// 在 GlobalExceptionHandler.TryHandleAsync 中
private static bool IsPlaneRoute(HttpContext context)
    => context.Request.Path.StartsWithSegments("/auth");

// Plane 格式输出
if (IsPlaneRoute(httpContext))
{
    var planeError = new
    {
        error = problemDetails.Detail ?? problemDetails.Title,
        error_code = statusCode,
        error_detail = problemDetails.Extensions.TryGetValue("errors", out var errs) ? errs : null
    };
    await httpContext.Response.WriteAsJsonAsync(planeError, cancellationToken);
    return true;
}
```

### Pattern 7: Plane 兼容分页格式

**What:** 将 FSH 的 PagedResponse<T> 转换为 Plane 前端期望的 `{count, next, previous, results}` 格式。

**Plane 分页格式：**

```json
{
  "count": 100,
  "next": "https://...?page=2",
  "previous": null,
  "results": [...]
}
```

**FSH 当前格式：**

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 30,
  "totalCount": 100,
  "totalPages": 4,
  "hasNext": true,
  "hasPrevious": false
}
```

**推荐适配策略：** 创建 `PlanePagedResult<T>` 包装类，在端点层做转换。

```csharp
// 新增 BuildingBlocks/Shared/Persistence/PlanePagedResult.cs
public sealed class PlanePagedResult<T>
{
    public long Count { get; init; }
    public string? Next { get; init; }
    public string? Previous { get; init; }
    public IReadOnlyCollection<T> Results { get; init; } = Array.Empty<T>();

    // 从 FSH PagedResponse 转换
    public static PlanePagedResult<T> FromPagedResponse(
        PagedResponse<T> paged, HttpContext httpContext)
    {
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}";
        var queryString = httpContext.Request.QueryString.Value ?? "";

        return new PlanePagedResult<T>
        {
            Count = paged.TotalCount,
            Results = paged.Items,
            Next = paged.HasNext
                ? $"{baseUrl}{queryString}{(queryString.Contains('?') ? "&" : "?")}page={paged.PageNumber + 1}"
                : null,
            Previous = paged.HasPrevious
                ? $"{baseUrl}{queryString}{(queryString.Contains('?') ? "&" : "?")}page={paged.PageNumber - 1}"
                : null,
        };
    }
}
```

### Pattern 8: Plane 兼容认证端点路由

**What:** 创建 `/auth/*` 路由组映射到 FSH 的 Identity 功能。

```csharp
// 新增 Identity/Features/v1/Auth/AuthEndpoints.cs
// 参考: apps/api/plane/authentication/urls.py

internal static class AuthEndpoints
{
    public static void MapPlaneAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/auth")
            .WithTags("Authentication");

        // 凭证认证
        auth.MapPost("sign-in", /* 复用 GenerateTokenCommand */);
        auth.MapPost("sign-up", /* 复用 RegisterUserCommand + 自动登录 */);
        auth.MapPost("sign-out", /* 清除 Session + 撤销 RefreshToken */);

        // 当前用户
        auth.MapGet("me", /* 复用 GetMeEndpoint 逻辑 */);

        // 密码管理
        auth.MapPost("forgot-password", /* 复用 ForgotPasswordCommand */);
        auth.MapPost("reset-password", /* 复用 ResetPasswordCommand */);
        auth.MapPost("change-password", /* 复用 ChangePasswordCommand */);

        // OAuth（Phase 1 框架）
        auth.MapGet("oauth/{provider}", OAuthInitiateEndpoint.Handle);
        auth.MapPost("oauth/{provider}/callback", OAuthCallbackEndpoint.Handle);

        // API Token 管理
        auth.MapPost("api-tokens", /* 创建 API Token */);
        auth.MapGet("api-tokens", /* 列出用户 API Tokens */);
        auth.MapDelete("api-tokens/{id}", /* 撤销 API Token */);
    }
}
```

### Pattern 9: DbMigrator 模块注册（4-places pattern）

**What:** 新模块（如未来添加的 Workspace）必须在 4 个位置注册。Phase 1 仅需确认 Identity 模块的注册完整性。

**当前状态：** Identity 模块已在 API Program.cs 和 DbMigrator Program.cs 中完成注册。Phase 1 不需要添加新模块，但需要为 Identity 的新实体（APIToken, OAuthProviderSettings）确保迁移管道正确。

**Key insight:** APIToken 和 OAuthProviderSettings 的迁移文件会自动由 DbMigrator 通过 `IdentityDbInitializer.MigrateAsync()` 执行——无需额外注册。

### Anti-Patterns to Avoid

- **不要修改 BuildingBlocks 核心代码：** FSH 黄金法则 #4，异常格式适配在 GlobalExceptionHandler 内完成，不新增 BuildingBlocks 接口。
- **不要在 AuthenticationHandler 中直接查 DbContext：** 通过 DI 获取 Service 层（IApiTokenService），Service 层查 DbContext。避免 Handler 直接依赖持久化层。
- **不要在 OAuth 回调中同步等待外部 HTTP：** OAuth token exchange 和 userinfo 获取使用 async/await + HttpClient，设置超时（30s）。
- **不要将 API Key 明文存储在数据库：** 只存储 SHA-256 哈希值。验证时计算输入 Key 的哈希并与数据库值比对。
- **不要忽略 CancellationToken：** FSH 黄金法则 #7，所有异步操作必须传递 CancellationToken。
- **不要对 /auth/\* 端点应用 FSH ProblemDetails 格式：** 这些端点需要 Plane 兼容格式，由前端解析。

## Don't Hand-Roll

| Problem        | Don't Build                    | Use Instead                                        | Why                                              |
| -------------- | ------------------------------ | -------------------------------------------------- | ------------------------------------------------ |
| JWT Token 生成 | 手写 HMAC-SHA256 签名          | `System.IdentityModel.Tokens.Jwt` (已有)           | Token 验证参数、时钟偏移、密钥轮换等边界条件极多 |
| 密码哈希       | 手写 SHA-256 哈希              | `ASP.NET Identity PasswordHasher` (已有)           | 自动 salt、PBKDF2 迭代、版本升级                 |
| 多租户过滤     | 手写 WHERE tenant_id = @tenant | `Finbuckle.MultiTenant` (已有)                     | 全局查询过滤器、自动注入、Schema 隔离            |
| Rate Limiting  | 手写计数器 + Redis             | `ASP.NET Core RateLimiter` (已有)                  | 固定窗口/滑动窗口/令牌桶/并发，分区限流          |
| 分布式追踪     | 手写 trace ID 传播             | `OpenTelemetry SDK` (已有)                         | 自动 HTTP/EF Core/Redis 注入，W3C TraceContext   |
| API 文档       | 手写 Swagger JSON              | `AddOpenApi + Scalar` (已有)                       | 自动从端点元数据生成，零维护                     |
| 结构化日志     | 手写 string.Format             | `Serilog structured logging` (已有)                | 属性化日志、多 Sink、OTLP 导出                   |
| 异常处理       | 手写 try-catch 中间件          | `IExceptionHandler` (已有)                         | 统一入口、与 ProblemDetails 集成                 |
| OAuth 基础流程 | 手写 HTTP 重定向 + code 交换   | `Microsoft.AspNetCore.Authentication.OAuth` (内置) | state 验证、nonce、PKCE 等安全机制               |
| Cookie 认证    | 手写 session 管理              | `ASP.NET Core Cookie Authentication` (内置)        | 自动 cookie 加密、sliding expiration、防篡改     |

## Common Pitfalls

### Pitfall 1: 多 Scheme 协商与 Finbuckle 租户解析的时序问题

**What goes wrong:** Finbuckle 的 `UseMultiTenant()` 在 `UseAuthentication()` 之前运行。如果使用 ClaimStrategy 从 JWT 中提取租户标识，此时 User 还是空的，解析不到租户。

**Why it happens:** ASP.NET Core 中间件管道是有序的：路由 → 多租户 → 认证 → 授权。

**How to avoid:** FSH 已经正确处理了这个问题 — ClaimStrategy 仅在后认证中间件中作为 fallback（见 MultitenancyModule.cs 第 96-109 行）。主要依赖 Header 和 Query 策略。对于 API Key 认证，需要在 ApiKeyAuthenticationHandler 成功后手动设置租户上下文（如果 Key 关联了特定租户）。

**Warning signs:** 请求返回 401 但日志显示 "tenant not resolved"。

### Pitfall 2: OAuth 回调中双发 JWT + Cookie 的序列化问题

**What goes wrong:** OAuth 回调端点在同一个 HTTP 响应中既要 Set-Cookie 又要返回 JWT token，但 OAuth 回调通常是浏览器重定向，无法直接在响应体中返回 JSON。

**Why it happens:** OAuth flow 的最后一步是 Provider 重定向回 callback URL，此时需要决定是重定向到前端（带 token 在 URL fragment 中）还是直接返回 JSON。

**How to avoid:** 采用 Plane 的策略 — callback 处理后重定向到前端 URL，将 JWT 放在 URL query string 或 fragment 中（`?token=xxx`），同时 Set-Cookie。前端从 URL 中提取 JWT 存储到 localStorage，后续请求可选择用 Cookie 或 JWT header。

**Warning signs:** OAuth 登录成功后前端拿不到 token。

### Pitfall 3: API Key 验证的性能问题

**What goes wrong:** 每个 API 请求都查数据库验证 API Key，在高并发下成为瓶颈。

**Why it happens:** API Key 验证是同步路径，每请求一次 DB 查询。

**How to avoid:** 添加 Redis 缓存层 — 验证时先查缓存（Key: `apikey:{hash_prefix}`），miss 时查 DB 并回填缓存。缓存过期时间 5 分钟。Key 撤销时主动清除缓存。参考 FSH Caching BuildingBlock 的 `IDistributedCache` 集成。

**Warning signs:** API Key 认证端点 P95 延迟 > 200ms。

### Pitfall 4: Plane 分页格式中 next/previous URL 构建

**What goes wrong:** 构建 next/previous URL 时需要保留原有的查询参数（筛选、排序等），不能只替换 page 参数。

**Why it happens:** 前端请求可能带有 `?state=active&priority=high&page=1`，next URL 需要变成 `?state=active&priority=high&page=2`。

**How to avoid:** 使用 `Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString` 或手动解析 QueryString，替换 page 参数值。在 `PlanePagedResult.FromPagedResponse` 中处理。

**Warning signs:** 分页后丢失筛选条件。

### Pitfall 5: 模块 Schema 与 Finbuckle MultiTenant 的交互

**What goes wrong:** 模块使用独立 Schema（如 `identity`），但 Finbuckle 的 `IsMultiTenant()` 会在唯一索引上追加 `TenantId`。如果 Schema 名称与 Finbuckle 的表名映射冲突，可能导致迁移失败。

**Why it happens:** Finbuckle 的 `AdjustUniqueIndexes()` 修改 EF Core model 中的索引定义，与 Schema 配置在 `OnModelCreating` 中的执行顺序有关。

**How to avoid:** 遵循 FSH 的已有模式 — `ApplyConfigurationsFromAssembly` 先执行（设置 Schema 和表名），然后 `ApplyTenantIsolationByDefault()` 最后执行。IdentityDbContext 中已经是这个顺序（第 59-72 行）。

**Warning signs:** `dotnet ef migrations add` 时报 "duplicate column" 或 "schema not found"。

### Pitfall 6: DbMigrator 中新实体的迁移遗漏

**What goes wrong:** 添加了 APIToken 和 OAuthProviderSettings 实体但忘记在 IdentityDbContext 中注册 DbSet，导致迁移不包含这些表。

**Why it happens:** EF Core 只迁移 DbContext 中注册的 DbSet 和 Configuration 中配置的实体。

**How to avoid:** 添加实体后立即：(1) 在 IdentityDbContext 添加 `DbSet<APIToken>` 和 `DbSet<OAuthProviderSettings>`；(2) 创建对应的 `IEntityTypeConfiguration<T>` 配置类；(3) 运行 `dotnet ef migrations add AddAPITokenAndOAuthSettings`。

**Warning signs:** API 运行时报 "table not found" 或 "invalid column name"。

### Pitfall 7: Security Headers 阻断 Scalar UI

**What goes wrong:** CSP 策略阻止了 Scalar API 文档页面的 JavaScript 执行。

**Why it happens:** FSH 的 SecurityHeadersMiddleware 默认排除 `/scalar` 和 `/openapi` 路径（SecurityHeadersOptions.cs 第 12 行），但如果添加了新的 API 文档路径，需要手动加入排除列表。

**How to avoid:** 在 appsettings.json 的 `SecurityHeadersOptions:ExcludedPaths` 中包含所有 API 文档路径。Phase 1 无需修改（已有默认值）。

**Warning signs:** Scalar UI 加载但显示空白或 JS 错误。

## Code Examples

### Token 生成后双发 JWT + Cookie

```csharp
// Source: FSH TokenService + GenerateTokenCommandHandler pattern
// 在 SignIn 端点 handler 中

public async ValueTask<PlaneAuthResponse> Handle(
    SignInCommand request, CancellationToken ct)
{
    // 复用 FSH 的 ValidateCredentials 逻辑
    var identityResult = await _identityService.ValidateCredentialsAsync(
        request.Email, request.Password, null, ct);

    if (identityResult is null)
        throw new UnauthorizedAccessException("Invalid credentials");

    var (subject, claims) = identityResult.Value;

    // 生成 JWT（复用 FSH TokenService）
    var token = await _tokenService.IssueAsync(subject, claims, null, ct);

    // 同时设置 Session Cookie（D-02 双发）
    await _signInManager.SignInWithCookieAsync(subject, claims);

    // 返回 Plane 兼容格式
    return new PlaneAuthResponse
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        AccessTokenExpiresAt = token.AccessTokenExpiresAt,
        User = await _userService.GetProfileAsync(subject, ct)
    };
}
```

### API Key 生成端点

```csharp
// Source: Plane APIToken model pattern
internal sealed class CreateApiTokenEndpoint
{
    public static RouteHandlerBuilder Map(ApiTokenEndpoints group)
    {
        return group.MapPost("/", async (
            CreateApiTokenCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(new
            {
                id = result.Id,
                name = result.Name,
                prefix = result.Prefix,     // "pk_abc1..."
                // 注意：仅在创建时返回完整 key，后续列表不返回
                token = result.FullToken,   // 明文，仅此一次
                expired_at = result.ExpiredAt,
                created_at = result.CreatedAt
            });
        })
        .WithName("CreateApiToken")
        .RequireAuthenticatedUser();
    }
}
```

### Serilog 配置（已有，确认不需要修改）

```json
// Source: appsettings.json — 已有配置
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console"],
    "Enrich": ["FromLogContext", "WithMachineName", "WithCorrelationId"],
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": { "restrictedToMinimumLevel": "Information" }
      }
    ]
  }
}
```

**Phase 1 调整：** 可以添加 `Serilog.Sinks.File` 用于本地开发日志持久化，但非必须。OpenTelemetry OTLP 导出已由 Aspire 自动注入 `OTEL_EXPORTER_OTLP_ENDPOINT` 环境变量，无需手动配置。

## State of the Art

| Old Approach          | Current Approach                    | When Changed | Impact                                        |
| --------------------- | ----------------------------------- | ------------ | --------------------------------------------- |
| 单一 JWT Scheme       | 多 Scheme PolicyScheme 协商         | .NET 8+      | 需要 PolicyScheme 作为路由器                  |
| DRF Pagination        | Plane {count/next/previous/results} | 始终         | 需要自定义序列化适配                          |
| DRF Exception Handler | IExceptionHandler + ProblemDetails  | .NET 8+      | FSH 已用 IExceptionHandler，需适配 Plane 格式 |
| Manual EF Migrations  | DbMigrator + Advisory Lock          | FSH pattern  | 已有完整实现                                  |
| Header-only Tenant    | Multi-strategy Finbuckle chain      | FSH pattern  | 已有 Header + Query + Claim                   |

## Assumptions Log

| #   | Claim                                                                                                      | Section   | Risk if Wrong                                |
| --- | ---------------------------------------------------------------------------------------------------------- | --------- | -------------------------------------------- |
| A1  | ASP.NET Core Cookie Authentication 中间件不需要额外 NuGet 包（内置于 Microsoft.AspNetCore.Authentication） | Pattern 1 | 低 — 这是 ASP.NET Core 标准组件              |
| A2  | Plane 前端使用 Cookie 名 `.AspNetCore.Session` 或类似格式                                                  | Pattern 1 | 中 — 需要检查 Plane 前端源码中的 cookie 名称 |
| A3  | OAuth callback 重定向到前端时使用 URL query string 传递 JWT（`?token=xxx`）                                | Pitfall 2 | 中 — Plane 前端可能使用不同的 token 传递机制 |

## Open Questions (ALL RESOLVED)

1. **[RESOLVED] Plane 前端的 Cookie 认证具体使用什么 Cookie 名？** — 决定: 使用 `.YHFlow.Session` 作为 Cookie 名 (见 Plan 01-01 SessionCookieAuthenticationDefaults)
   - What we know: Plane 使用 Django session framework，默认 Cookie 名为 `sessionid`
   - What's unclear: .NET 端是否应使用相同的 Cookie 名 `sessionid` 以保持前端兼容
   - Recommendation: 使用 `sessionid` 作为 Cookie 名，确保与 Plane 前端的 `withCredentials: true` 配置兼容

2. **[RESOLVED] OAuth 回调后 JWT 传递给前端的具体机制？** — 决定: 使用 query string `?token=xxx&refresh_token=yyy` (见 Plan 01-03 OAuthCallbackEndpoint)
   - What we know: Plane 使用 URL 重定向，前端从 URL 中提取认证信息
   - What's unclear: 具体是 query string 还是 hash fragment
   - Recommendation: 使用 query string `?token=xxx&refresh_token=yyy`，Phase 9 实现具体 Provider 时验证

3. **[RESOLVED] OAuthProviderSettings 是全局配置还是 per-tenant 配置？** — 决定: 实现为 IGlobalEntity (全局)，见 Plan 01-02 OAuthProviderSettings
   - What we know: D-08 说放在 Identity 模块，D-07 说数据库动态管理
   - What's unclear: 不同租户是否需要不同的 OAuth 配置
   - Recommendation: 一期实现为 IGlobalEntity（全局），后续如有需求可改为 IHasTenant

## Environment Availability

| Dependency           | Required By             | Available | Version        | Fallback          |
| -------------------- | ----------------------- | --------- | -------------- | ----------------- |
| .NET SDK 10          | 编译运行                | ✓         | 10.0.x         | —                 |
| PostgreSQL 16+       | EF Core + 迁移          | ✓         | Aspire 编排    | —                 |
| Redis (Valkey)       | 缓存 (API Key 验证缓存) | ✓         | Aspire 编排    | InMemory fallback |
| ASP.NET Core Auth 包 | 多 Scheme 认证          | ✓         | 内置于 .NET 10 | —                 |
| Finbuckle 10.x       | 多租户                  | ✓         | Phase 0 已安装 | —                 |

**Missing dependencies with no fallback:** None

**Missing dependencies with fallback:** None

## Validation Architecture

### Test Framework

| Property           | Value                                                    |
| ------------------ | -------------------------------------------------------- |
| Framework          | xunit + Shouldly + AutoFixture (已有)                    |
| Config file        | `src/Tests/Architecture.Tests/Architecture.Tests.csproj` |
| Quick run command  | `dotnet test src/Tests/Architecture.Tests/ --no-build`   |
| Full suite command | `dotnet test yh-flow/src/Tests/ --no-build`              |

### Phase Requirements → Test Map

| Req ID  | Behavior                    | Test Type    | Automated Command                          | File Exists? |
| ------- | --------------------------- | ------------ | ------------------------------------------ | ------------ |
| REQ-1.2 | DbMigrator 执行所有模块迁移 | integration  | `dotnet test --filter "DbMigrator"`        | ❌ Wave 0    |
| REQ-1.3 | JWT Bearer 认证签发和验证   | unit         | `dotnet test --filter "JwtAuth"`           | ❌ Wave 0    |
| REQ-1.3 | API Key 认证验证            | unit         | `dotnet test --filter "ApiKeyAuth"`        | ❌ Wave 0    |
| REQ-1.3 | Session Cookie 认证验证     | unit         | `dotnet test --filter "SessionCookieAuth"` | ❌ Wave 0    |
| REQ-1.3 | OAuth Provider 框架注册     | unit         | `dotnet test --filter "OAuthProvider"`     | ❌ Wave 0    |
| REQ-1.4 | Plane 错误格式输出          | unit         | `dotnet test --filter "PlaneError"`        | ❌ Wave 0    |
| REQ-1.4 | Plane 分页格式输出          | unit         | `dotnet test --filter "PlanePaging"`       | ❌ Wave 0    |
| REQ-1.4 | Rate Limiting 四层策略      | integration  | `dotnet test --filter "RateLimiting"`      | ❌ Wave 0    |
| NFR-2   | 多租户数据隔离              | architecture | `dotnet test --filter "TenantIsolation"`   | ✅ 已有      |

### Sampling Rate

- **Per task commit:** `dotnet test src/Tests/Architecture.Tests/ --no-build`
- **Per wave merge:** `dotnet test yh-flow/src/Tests/ --no-build`
- **Phase gate:** Full suite green + 手动验证认证流程

### Wave 0 Gaps

- [ ] `src/Tests/Identity.Tests/` — 新建测试项目，覆盖 REQ-1.3 认证测试
- [ ] `src/Tests/Identity.Tests/ApiKeyAuthenticationTests.cs` — API Key 认证
- [ ] `src/Tests/Identity.Tests/SessionCookieAuthenticationTests.cs` — Session Cookie 认证
- [ ] `src/Tests/Identity.Tests/OAuthProviderFrameworkTests.cs` — OAuth 框架
- [ ] `src/Tests/Identity.Tests/PlaneFormatAdapterTests.cs` — 分页和错误格式适配
- [ ] Test project 创建: `dotnet new xunit -o src/Tests/Identity.Tests`

## Security Domain

### Applicable ASVS Categories

| ASVS Category         | Applies | Standard Control                                         |
| --------------------- | ------- | -------------------------------------------------------- |
| V2 Authentication     | yes     | ASP.NET Identity + JWT Bearer + API Key + Cookie         |
| V3 Session Management | yes     | Cookie Authentication + Refresh Token rotation           |
| V4 Access Control     | yes     | Permission-based authorization (RequiredPermission)      |
| V5 Input Validation   | yes     | FluentValidation on all Commands/Queries                 |
| V6 Cryptography       | yes     | SHA-256 for API Key hashing, HMAC-SHA256 for JWT signing |

### Known Threat Patterns for Auth Infrastructure

| Pattern               | STRIDE                 | Standard Mitigation                                                |
| --------------------- | ---------------------- | ------------------------------------------------------------------ |
| API Key 泄露          | Spoofing               | 数据库存储哈希而非明文；Key 可撤销；过期时间                       |
| JWT Token 劫持        | Spoofing               | HTTPS only；短过期时间（30min）；Refresh Token rotation            |
| OAuth CSRF            | Tampering              | state 参数验证（ASP.NET Core OAuth 内置）                          |
| 暴力破解认证端点      | DoS                    | Rate Limiting "auth" policy (10/60s)；Account lockout (5 attempts) |
| SQL Injection in Auth | Tampering              | EF Core parameterized queries；无原始 SQL                          |
| Multi-tenant 数据泄露 | Information Disclosure | Finbuckle 全局查询过滤器；IHasTenant 默认开启                      |

## Sources

### Primary (HIGH confidence)

- `yh-flow/src/Modules/Identity/` — FSH Identity 模块源码（JWT、Token、权限、Session）
- `yh-flow/src/BuildingBlocks/Web/` — FSH Web 基础设施（RateLimiting、CORS、SecurityHeaders、ExceptionHandling、OpenAPI）
- `yh-flow/src/BuildingBlocks/Persistence/` — FSH BaseDbContext + TenantIsolation
- `yh-flow/src/Host/YH.Flow.DbMigrator/` — DbMigrator 完整实现
- `yh-flow/src/Modules/Multitenancy/` — Finbuckle 多租户配置（Strategy chain）
- `apps/api/plane/authentication/` — Plane 认证参考实现（OAuth adapter、API Key middleware）
- `apps/api/plane/api/middleware/api_authentication.py` — Plane API Key 认证中间件

### Secondary (MEDIUM confidence)

- `.planning/research/fullstackhero-patterns.md` — FSH 模式适配指南
- `.planning/research/api-migration-mapping.md` — Django → .NET API 端点映射
- `.planning/codebase/ARCHITECTURE.md` — Plane 架构分析

### Tertiary (LOW confidence)

- N/A — 所有关键发现均通过源码验证

## Metadata

**Confidence breakdown:**

- Standard stack: HIGH — 全部基于 Phase 0 已安装的包，无新增依赖
- Architecture: HIGH — 基于实际源码分析，FSH 模式清晰可复用
- Pitfalls: HIGH — 基于 FSH 已有实现中的处理方式和 Plane 参考代码
- Multi-scheme auth: MEDIUM — PolicyScheme 模式已验证可行，但 API Key + Cookie 与 Finbuckle 交互需实际测试
- OAuth framework: MEDIUM — Phase 1 仅搭框架，具体交互在 Phase 9 验证
- Plane format compatibility: MEDIUM — 需要验证 Plane 前端对错误/分页格式的具体解析逻辑

**Research date:** 2026-06-16
**Valid until:** 2026-07-16 (30 days — 稳定架构，不依赖外部快速变化的库)
