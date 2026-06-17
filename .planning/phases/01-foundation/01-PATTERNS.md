# Phase 1: Foundation — 基础设施 - Pattern Map

**Mapped:** 2026-06-17
**Files analyzed:** 28
**Analogs found:** 24 / 28

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `BuildingBlocks/Web/Auth/ApiKeyAuthenticationHandler.cs` | middleware | request-response | `Authorization/Jwt/ConfigureJwtBearerOptions.cs` | role-match |
| `BuildingBlocks/Web/Auth/ApiKeyAuthenticationOptions.cs` | config | — | `Authorization/Jwt/JwtOptions.cs` | exact |
| `BuildingBlocks/Web/Auth/SessionCookieAuthenticationExtensions.cs` | middleware | request-response | `Authorization/Jwt/JwtAuthenticationExtensions.cs` | exact |
| `Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationHandler.cs` | middleware | request-response | `Authorization/Jwt/ConfigureJwtBearerOptions.cs` | role-match |
| `Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationExtensions.cs` | config | — | `Authorization/Jwt/JwtAuthenticationExtensions.cs` | exact |
| `Modules.Identity/Authorization/SessionCookie/SessionCookieAuthenticationExtensions.cs` | config | — | `Authorization/Jwt/JwtAuthenticationExtensions.cs` | exact |
| `Modules.Identity/Domain/APIToken.cs` | model | CRUD | `Domain/UserSession.cs` | exact |
| `Modules.Identity/Domain/OAuthProviderSettings.cs` | model | CRUD | `Domain/ImpersonationGrant.cs` | exact |
| `Modules.Identity/Data/Configurations/APITokenConfiguration.cs` | config | CRUD | `Data/Configurations/UserSessionConfiguration.cs` | exact |
| `Modules.Identity/Data/Configurations/OAuthProviderSettingsConfiguration.cs` | config | CRUD | `Data/ImpersonationGrantConfig.cs` | exact |
| `Modules.Identity/Data/IdentityDbContext.cs` | model | CRUD | (self — 修改) | — |
| `Modules.Identity/Features/v1/Auth/AuthEndpoints.cs` | route | request-response | `IdentityModule.cs` (MapEndpoints) | role-match |
| `Modules.Identity/Features/v1/Auth/SignIn/PlaneSignInEndpoint.cs` | controller | request-response | `Features/v1/Tokens/TokenGeneration/GenerateTokenEndpoint.cs` | exact |
| `Modules.Identity/Features/v1/Auth/SignUp/PlaneSignUpEndpoint.cs` | controller | request-response | `Features/v1/Users/RegisterUser/RegisterUserEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/Auth/SignOut/PlaneSignOutEndpoint.cs` | controller | request-response | `Features/v1/Sessions/RevokeSession/RevokeSessionEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/Auth/Me/PlaneMeEndpoint.cs` | controller | request-response | `Features/v1/Sessions/GetMySessions/GetMySessionsEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/ApiTokens/Create/CreateApiTokenEndpoint.cs` | controller | CRUD | `Features/v1/Tokens/TokenGeneration/GenerateTokenEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/ApiTokens/List/ListApiTokensEndpoint.cs` | controller | CRUD | `Features/v1/Sessions/GetMySessions/GetMySessionsEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/ApiTokens/Revoke/RevokeApiTokenEndpoint.cs` | controller | CRUD | `Features/v1/Sessions/RevokeSession/RevokeSessionEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/OAuth/IOAuthProvider.cs` | model | — | (新建 — 无 analog) | no-analog |
| `Modules.Identity/Features/v1/OAuth/OAuthProviderRegistry.cs` | service | CRUD | (新建 — 无 analog) | no-analog |
| `Modules.Identity/Features/v1/OAuth/OAuthCallbackEndpoint.cs` | controller | request-response | `Features/v1/Tokens/TokenGeneration/GenerateTokenEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/OAuth/OAuthInitiateEndpoint.cs` | controller | request-response | `Features/v1/Tokens/TokenGeneration/GenerateTokenEndpoint.cs` | role-match |
| `Modules.Identity/Features/v1/OAuth/Manage/ManageOAuthProviderEndpoint.cs` | controller | CRUD | `Features/v1/Roles/UpsertRole/CreateOrUpdateRoleEndpoint.cs` | role-match |
| `Modules.Identity/Services/ApiTokenService.cs` | service | CRUD | `Services/TokenService.cs` | role-match |
| `Modules.Identity/Services/OAuthProviderService.cs` | service | CRUD | `Services/TokenService.cs` | role-match |
| `Modules.Identity/IdentityModule.cs` | config | — | (self — 修改) | — |
| `BuildingBlocks/Web/Exceptions/GlobalExceptionHandler.cs` | middleware | request-response | (self — 修改) | — |
| `BuildingBlocks/Web/Pagination/PlanePagedResponse.cs` | model | — | `Shared/Persistence/PagedResponse.cs` | exact |
| `BuildingBlocks/Shared/Persistence/PlanePagedResult.cs` | model | — | `Shared/Persistence/PagedResponse.cs` | exact |
| `Modules.Identity.Contracts/v1/ApiTokens/*.cs` | model | CRUD | `Contracts/v1/Tokens/TokenGeneration/GenerateTokenCommand.cs` | exact |
| `Modules.Identity.Contracts/v1/OAuth/*.cs` | model | CRUD | `Contracts/v1/Sessions/GetMySessions/GetMySessionsQuery.cs` | role-match |
| `Modules.Identity.Contracts/DTOs/APITokenDto.cs` | model | — | `Contracts/DTOs/UserSessionDto.cs` | exact |
| `Modules.Identity.Contracts/DTOs/OAuthProviderSettingsDto.cs` | model | — | `Contracts/DTOs/UserSessionDto.cs` | role-match |
| `Modules.Identity.Contracts/Services/IApiTokenService.cs` | model | — | `Contracts/Services/ITokenService.cs` | exact |
| `Modules.Identity.Contracts/Services/IOAuthProviderService.cs` | model | — | `Contracts/Services/ITokenService.cs` | role-match |
| `Host/YH.Flow.Api/Program.cs` | config | — | (self — 修改) | — |
| `Host/YH.Flow.DbMigrator/Program.cs` | config | — | (self — 修改) | — |
| `Host/YH.Flow.Api/appsettings.json` | config | — | (self — 修改) | — |
| `Tests/Identity.Tests/Authorization/ApiKeyAuthenticationTests.cs` | test | — | `Tests/Identity.Tests/Services/TokenServiceTests.cs` | role-match |
| `Tests/Identity.Tests/Authorization/SessionCookieAuthenticationTests.cs` | test | — | `Tests/Identity.Tests/Services/TokenServiceTests.cs` | role-match |
| `Tests/Identity.Tests/Authorization/OAuthProviderFrameworkTests.cs` | test | — | `Tests/Identity.Tests/Services/TokenServiceTests.cs` | role-match |
| `Tests/Identity.Tests/Handlers/PlaneFormatAdapterTests.cs` | test | — | `Tests/Identity.Tests/Handlers/GenerateTokenCommandHandlerTests.cs` | role-match |

## Pattern Assignments

### 1. API Key 认证方案 (BuildingBlocks/Web/Auth/ + Modules.Identity/Authorization/ApiKey/)

**Analog:** `Modules.Identity/Authorization/Jwt/JwtAuthenticationExtensions.cs`

**Extension 注册模式** (lines 1-42):
```csharp
// Source: JwtAuthenticationExtensions.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace YH.Modules.Identity.Authorization.Jwt;

internal static class JwtAuthenticationExtensions
{
    internal static IServiceCollection ConfigureJwtAuth(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(nameof(JwtOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, null!);

        services.AddAuthorizationBuilder().AddRequiredPermissionPolicy();
        // ...
        return services;
    }
}
```

**新建 ApiKeyAuthenticationExtensions 应遵循的模式:**
```csharp
// 新建: Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationExtensions.cs
namespace YH.Modules.Identity.Authorization.ApiKey;

internal static class ApiKeyAuthenticationExtensions
{
    internal static IServiceCollection ConfigureApiKeyAuth(this IServiceCollection services)
    {
        services.AddOptions<ApiKeyAuthenticationOptions>()
            .BindConfiguration(nameof(ApiKeyAuthenticationOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication()
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationDefaults.AuthenticationScheme, null!);

        return services;
    }
}
```

**ApiKeyAuthenticationHandler 模式** (参考 RESEARCH.md Pattern 2):
```csharp
// 新建: Modules.Identity/Authorization/ApiKey/ApiKeyAuthenticationHandler.cs
// 关键：通过 DI 获取 IApiTokenService，不直接访问 DbContext
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

        // 通过 DI 获取 Service 层
        var apiTokenService = Context.RequestServices.GetRequiredService<IApiTokenService>();
        var token = await apiTokenService.ValidateAndGetOwnerAsync(rawKey, Context.RequestAborted);

        if (token is null)
            return AuthenticateResult.Fail("Invalid or expired API key");

        // 构建 ClaimsPrincipal — 与 JWT 相同的 Claim 结构
        var claims = new List<Claim> { /* ... */ };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
```

---

### 2. Session Cookie 认证方案

**Analog:** `Modules.Identity/Authorization/Jwt/JwtAuthenticationExtensions.cs`

**Session Cookie 注册模式:**
```csharp
// 新建: Modules.Identity/Authorization/SessionCookie/SessionCookieAuthenticationExtensions.cs
namespace YH.Modules.Identity.Authorization.SessionCookie;

internal static class SessionCookieAuthenticationExtensions
{
    internal static IServiceCollection ConfigureSessionCookieAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddCookie(SessionCookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = ".YHFlow.Session";  // 或 "sessionid" 兼容 Plane 前端
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

        return services;
    }
}
```

---

### 3. 多 Scheme 协商注册 (IdentityModule.cs 修改)

**Analog:** `Modules.Identity/IdentityModule.cs` (ConfigureServices, line 168)

**当前注册点:**
```csharp
// Source: IdentityModule.cs line 168
services.ConfigureJwtAuth();
```

**修改为多 Scheme 注册:**
```csharp
// IdentityModule.ConfigureServices 中扩展
services.ConfigureJwtAuth();               // 已有 — JWT Bearer
services.ConfigureApiKeyAuth();            // 新增 — API Key
services.ConfigureSessionCookieAuth();     // 新增 — Session Cookie

// PolicyScheme 作为 DefaultScheme 路由器（在 ConfigureJwtAuth 中修改）
// 见 RESEARCH.md Pattern 1
```

---

### 4. Domain 实体 — APIToken

**Analog:** `Modules.Identity/Domain/UserSession.cs`

**实体模式** (lines 1-95):
```csharp
// Source: UserSession.cs
using YH.Framework.Core.Domain;
using YH.Modules.Identity.Domain.Events;

namespace YH.Modules.Identity.Domain;

public class UserSession : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = default!;
    public string RefreshTokenHash { get; private set; } = default!;
    // ... 其他属性

    // Navigation property
    public virtual FshUser? User { get; init; }

    // IHasDomainEvents implementation
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    private UserSession() { } // EF Core

    public static UserSession Create(/* factory parameters */) { /* ... */ }

    // 行为方法
    public void Revoke(string? revokedBy = null, string? reason = null) { /* ... */ }
}
```

**APIToken 应遵循的模式:**
```csharp
// 新建: Modules.Identity/Domain/APIToken.cs
// 使用 IHasTenant 实现租户隔离（多租户 API Key）
public class APIToken : IHasDomainEvents, IHasTenant
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private APIToken() { } // EF Core

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string UserId { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;  // SHA-256 hash
    public string Prefix { get; private set; } = default!;     // "pk_abc1..."
    public string TenantId { get; private set; } = default!;   // IHasTenant
    public DateTime? ExpiredAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual FshUser? User { get; init; }

    // Factory method + 行为方法
    public static APIToken Create(/* ... */) { /* ... */ }
    public void RecordUsage() => LastUsed = DateTime.UtcNow;
    public void Revoke() => IsActive = false;
    public bool IsExpired() => ExpiredAt.HasValue && ExpiredAt.Value < DateTime.UtcNow;
}
```

---

### 5. Domain 实体 — OAuthProviderSettings

**Analog:** `Modules.Identity/Domain/ImpersonationGrant.cs`

**IGlobalEntity 模式** (lines 17-50):
```csharp
// Source: ImpersonationGrant.cs
using YH.Framework.Core.Domain;

namespace YH.Modules.Identity.Domain;

/// <summary>
/// Implements IGlobalEntity to opt out of automatic tenant isolation.
/// </summary>
public class ImpersonationGrant : IGlobalEntity
{
    public Guid Id { get; private set; }
    public string Jti { get; private set; } = default!;
    // ... 属性

    private ImpersonationGrant() { } // EF Core

    public static ImpersonationGrant Create(/* factory parameters */) { /* ... */ }
    public void MarkEnded(DateTime endedAtUtc) { /* ... */ }
    public void Revoke(/* ... */) { /* ... */ }
    public bool IsTerminal => EndedAtUtc.HasValue || RevokedAtUtc.HasValue;
}
```

**OAuthProviderSettings 应遵循的模式:**
```csharp
// 新建: Modules.Identity/Domain/OAuthProviderSettings.cs
// 使用 IGlobalEntity — 全局配置，不按租户隔离（一期）
public class OAuthProviderSettings : IGlobalEntity
{
    private OAuthProviderSettings() { } // EF Core

    public Guid Id { get; private set; }
    public string ProviderName { get; private set; } = default!;  // "github", "gitlab", etc.
    public string ClientId { get; private set; } = default!;
    public string ClientSecret { get; private set; } = default!;  // 加密存储
    public string CallbackUrl { get; private set; } = default!;
    public bool Enabled { get; private set; }
    public bool AutoCreateAccount { get; private set; }
    public string? Scope { get; private set; }

    public static OAuthProviderSettings Create(/* ... */) { /* ... */ }
    public void Update(/* ... */) { /* ... */ }
    public void ToggleEnabled(bool enabled) => Enabled = enabled;
}
```

---

### 6. EF Core 配置

**Analog:** `Modules.Identity/Data/Configurations/UserSessionConfiguration.cs` + `Data/ImpersonationGrantConfig.cs`

**租户隔离实体配置模式** (UserSessionConfiguration.cs, lines 1-79):
```csharp
// Source: UserSessionConfiguration.cs
using YH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YH.Modules.Identity.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("UserSessions", IdentityModuleConstants.SchemaName)
            .HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired().HasMaxLength(450);
        builder.Property(s => s.RefreshTokenHash).IsRequired().HasMaxLength(256);
        // ... 字段配置

        builder.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.RefreshTokenHash);
    }
}
```

**全局实体配置模式** (ImpersonationGrantConfig.cs, lines 7-53):
```csharp
// Source: ImpersonationGrantConfig.cs
// 注意: IGlobalEntity 不需要显式配置多租户 — ApplyTenantIsolationByDefault 会跳过
public class ImpersonationGrantConfig : IEntityTypeConfiguration<ImpersonationGrant>
{
    public void Configure(EntityTypeBuilder<ImpersonationGrant> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("ImpersonationGrants", IdentityModuleConstants.SchemaName);

        // NOT multitenant — cross-tenant impersonations
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Jti).IsRequired().HasMaxLength(64);
        builder.HasIndex(g => g.Jti).IsUnique();
        // ...
    }
}
```

**Schema 常量** (IdentityModuleConstants.cs, line 12):
```csharp
// Source: IdentityModuleConstants.cs
public const string SchemaName = "identity";
```

---

### 7. IdentityDbContext 修改

**Analog:** `Modules.Identity/Data/IdentityDbContext.cs`

**当前 DbSet 注册模式** (lines 29-43):
```csharp
// Source: IdentityDbContext.cs
public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
public DbSet<UserSession> UserSessions => Set<UserSession>();
public DbSet<Group> Groups => Set<Group>();
public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
public DbSet<UserGroup> UserGroups => Set<UserGroup>();
public DbSet<ImpersonationGrant> ImpersonationGrants => Set<ImpersonationGrant>();
```

**需添加:**
```csharp
public DbSet<APIToken> ApiTokens => Set<APIToken>();
public DbSet<OAuthProviderSettings> OAuthProviderSettings => Set<OAuthProviderSettings>();
```

**OnModelCreating 顺序** (lines 59-72):
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    builder.ApplyConfiguration(new OutboxMessageConfiguration(IdentityModuleConstants.SchemaName));
    builder.ApplyConfiguration(new InboxMessageConfiguration(IdentityModuleConstants.SchemaName));
    // Default-on tenant isolation — auto-applies IsMultiTenant() to non-IGlobalEntity entities
    builder.ApplyTenantIsolationByDefault();
}
```

---

### 8. Feature Endpoint 模式

**Analog:** `Features/v1/Tokens/TokenGeneration/GenerateTokenEndpoint.cs`

**Endpoint 注册模式** (lines 1-68):
```csharp
// Source: GenerateTokenEndpoint.cs
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.v1.Tokens.TokenGeneration;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace YH.Modules.Identity.Features.v1.Tokens.TokenGeneration;

public static class GenerateTokenEndpoint
{
    public static RouteHandlerBuilder MapGenerateTokenEndpoint(this IEndpointRouteBuilder endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        return endpoint.MapPost("/token/issue",
            [AllowAnonymous] async Task<Results<Ok<TokenResponse>, UnauthorizedHttpResult, ProblemHttpResult>>
            ([FromBody] GenerateTokenCommand command,
            [FromHeader] string tenant,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
            {
                var token = await mediator.Send(command, ct);
                return token is null
                    ? TypedResults.Unauthorized()
                    : TypedResults.Ok(token);
            })
            .WithName("IssueJwtTokens")
            .WithSummary("Issue JWT access and refresh tokens")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
```

**简单查询 Endpoint 模式** (GetMySessionsEndpoint.cs, lines 1-26):
```csharp
// Source: GetMySessionsEndpoint.cs
public static class GetMySessionsEndpoint
{
    internal static RouteHandlerBuilder MapGetMySessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sessions/me", async (CancellationToken cancellationToken, IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetMySessionsQuery(), cancellationToken)))
        .WithName("GetMySessions")
        .WithSummary("Get current user's sessions")
        .RequirePermission(IdentityPermissions.Sessions.View)
        .Produces<IEnumerable<UserSessionDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
```

---

### 9. Command/Query + Handler 模式

**Analog:** `Contracts/v1/Tokens/TokenGeneration/GenerateTokenCommand.cs` + `Features/v1/Tokens/TokenGeneration/GenerateTokenCommandHandler.cs`

**Command 定义模式** (Contracts):
```csharp
// Source: GenerateTokenCommand.cs
using YH.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace YH.Modules.Identity.Contracts.v1.Tokens.TokenGeneration;

public record GenerateTokenCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null)
    : ICommand<TokenResponse>;
```

**Handler 模式** (lines 16-152):
```csharp
// Source: GenerateTokenCommandHandler.cs
public sealed class GenerateTokenCommandHandler
    : ICommandHandler<GenerateTokenCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GenerateTokenCommandHandler> _logger;

    public GenerateTokenCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        ILogger<GenerateTokenCommandHandler> logger)
    {
        // 构造函数注入
    }

    public async ValueTask<TokenResponse> Handle(
        GenerateTokenCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        // 业务逻辑 + CancellationToken 传递
        return token;
    }
}
```

---

### 10. Service 接口模式

**Analog:** `Modules.Identity.Contracts/Services/ITokenService.cs`

```csharp
// Source: ITokenService.cs
using YH.Modules.Identity.Contracts.DTOs;
using System.Security.Claims;

namespace YH.Modules.Identity.Contracts.Services;

public interface ITokenService
{
    Task<TokenResponse> IssueAsync(
        string subject,
        IEnumerable<Claim> claims,
        string? tenant = null,
        CancellationToken ct = default);

    Task<(string AccessToken, DateTime ExpiresAtUtc)> IssueAccessOnlyAsync(
        string subject,
        IEnumerable<Claim> claims,
        TimeSpan? lifetime = null,
        CancellationToken ct = default);
}
```

**新建 IApiTokenService 应遵循的模式:**
```csharp
// 新建: Modules.Identity.Contracts/Services/IApiTokenService.cs
namespace YH.Modules.Identity.Contracts.Services;

public interface IApiTokenService
{
    Task<APITokenDto> CreateAsync(CreateApiTokenCommand command, CancellationToken ct);
    Task<IReadOnlyList<APITokenDto>> ListForUserAsync(string userId, CancellationToken ct);
    Task RevokeAsync(Guid tokenId, CancellationToken ct);
    Task<APIToken?> ValidateAndGetOwnerAsync(string rawKey, CancellationToken ct);
}
```

---

### 11. DTO/Response 模式

**Analog:** `Modules.Identity.Contracts/DTOs/TokenResponse.cs` + `DTOs/UserSessionDto.cs`

```csharp
// Source: TokenResponse.cs
namespace YH.Modules.Identity.Contracts.DTOs;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    DateTime AccessTokenExpiresAt);
```

---

### 12. GlobalExceptionHandler 修改 — Plane 格式适配

**Analog:** `BuildingBlocks/Web/Exceptions/GlobalExceptionHandler.cs`

**当前完整实现** (lines 1-107):
```csharp
// Source: GlobalExceptionHandler.cs
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails { Instance = httpContext.Request.Path };
        var statusCode = StatusCodes.Status500InternalServerError;

        if (exception is FluentValidation.ValidationException fluentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            problemDetails.Status = statusCode;
            problemDetails.Title = "Validation error";
            problemDetails.Extensions["errors"] = fluentException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }
        else if (exception is CustomException e) { /* ... */ }
        else if (exception is UnauthorizedAccessException) { /* ... */ }
        // ...

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
```

**需要添加的 Plane 格式适配:**
```csharp
// 在 WriteAsJsonAsync 之前添加判断
private static bool IsPlaneRoute(HttpContext context)
    => context.Request.Path.StartsWithSegments("/auth");

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

// 默认: RFC 7807 ProblemDetails
await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
```

---

### 13. Plane 分页格式适配

**Analog:** `BuildingBlocks/Shared/Persistence/PagedResponse.cs`

**FSH 当前分页格式** (lines 1-18):
```csharp
// Source: PagedResponse.cs
namespace YH.Framework.Shared.Persistence;

public sealed class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public long TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
}
```

**新建 Plane 分页格式:**
```csharp
// 新建: BuildingBlocks/Shared/Persistence/PlanePagedResult.cs
namespace YH.Framework.Shared.Persistence;

public sealed class PlanePagedResult<T>
{
    public long Count { get; init; }
    public string? Next { get; init; }
    public string? Previous { get; init; }
    public IReadOnlyCollection<T> Results { get; init; } = Array.Empty<T>();

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

---

### 14. IdentityModule 端点注册修改

**Analog:** `Modules.Identity/IdentityModule.cs` (MapEndpoints, lines 171-259)

**当前端点注册模式:**
```csharp
// Source: IdentityModule.cs MapEndpoints
public void MapEndpoints(IEndpointRouteBuilder endpoints)
{
    var apiVersionSet = endpoints.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

    var group = endpoints
        .MapGroup("api/v{version:apiVersion}/identity")
        .WithTags("Identity")
        .WithApiVersionSet(apiVersionSet);

    // tokens
    group.MapGenerateTokenEndpoint().AllowAnonymous().RequireRateLimiting("auth");
    group.MapRefreshTokenEndpoint().AllowAnonymous().RequireRateLimiting("auth");

    // users
    group.MapGetMeEndpoint();
    // ...
}
```

**需要添加的 Plane 认证端点组:**
```csharp
// 在 MapEndpoints 中添加 /auth/* 路由组
endpoints.MapPlaneAuthEndpoints();  // 新的扩展方法，注册 /auth/* 路由
```

---

### 15. Test 模式

**Analog:** `Tests/Identity.Tests/Services/TokenServiceTests.cs`

**Test 项目结构:**
```csharp
// Source: Identity.Tests.csproj
// 测试框架: xunit + Shouldly + AutoFixture + NSubstitute
```

**Test 模式** (lines 18-56):
```csharp
// Source: TokenServiceTests.cs
namespace Identity.Tests.Services;

public sealed class TokenServiceTests : IDisposable
{
    private readonly ILogger<TokenService> _logger;
    private readonly IdentityMetrics _metrics;

    public TokenServiceTests()
    {
        _logger = Substitute.For<ILogger<TokenService>>();
        var meterFactory = Substitute.For<IMeterFactory>();
        meterFactory.Create(Arg.Any<MeterOptions>()).Returns(_ => new Meter(IdentityMetrics.MeterName));
        _metrics = new IdentityMetrics(meterFactory);
    }

    [Fact]
    public async Task IssueAsync_Should_ReturnTokenResponseWithAllFields()
    {
        // Arrange
        var service = CreateService();

        // Act
        var response = await service.IssueAsync("user-123", SampleClaims());

        // Assert
        response.ShouldNotBeNull();
        response.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }
}
```

---

## Shared Patterns

### Authentication Scheme 注册
**Source:** `Modules.Identity/Authorization/Jwt/JwtAuthenticationExtensions.cs`
**Apply to:** ApiKeyAuthenticationExtensions, SessionCookieAuthenticationExtensions
```csharp
// 所有 Scheme 注册遵循相同模式：
// 1. AddOptions<T>().BindConfiguration().ValidateDataAnnotations().ValidateOnStart()
// 2. services.AddAuthentication().AddXxx(schemeName, null!)
// 3. 在 IdentityModule.ConfigureServices 中调用 services.ConfigureXxxAuth()
```

### EF Core Entity 模式
**Source:** `Domain/UserSession.cs` + `Domain/ImpersonationGrant.cs`
**Apply to:** APIToken, OAuthProviderSettings
```csharp
// 1. private 构造函数 (EF Core materialization)
// 2. static Create() 工厂方法
// 3. private set 属性 + 行为方法封装状态变更
// 4. IHasDomainEvents 用于领域事件
// 5. IHasTenant 用于租户隔离 / IGlobalEntity 用于全局共享
```

### EF Core Configuration 模式
**Source:** `Data/Configurations/UserSessionConfiguration.cs` + `Data/ImpersonationGrantConfig.cs`
**Apply to:** APITokenConfiguration, OAuthProviderSettingsConfiguration
```csharp
// 1. IEntityTypeConfiguration<T> 接口
// 2. ToTable("TableName", IdentityModuleConstants.SchemaName) — Schema 固定 "identity"
// 3. ArgumentNullException.ThrowIfNull(builder) — 防御性检查
// 4. HasKey, Property(IsRequired + MaxLength), HasIndex
// 5. ApplyConfigurationsFromAssembly 自动发现
```

### Endpoint + Mediator CQRS 模式
**Source:** `GenerateTokenEndpoint.cs` + `GenerateTokenCommandHandler.cs`
**Apply to:** 所有新建 Feature Endpoint
```csharp
// 1. Endpoint: static class + MapXxxEndpoint 扩展方法
// 2. 使用 Minimal API + TypedResults
// 3. Command/Query 定义在 Contracts 项目
// 4. Handler 在 Features 项目，实现 ICommandHandler/IQueryHandler
// 5. CancellationToken 必须传递
```

### Rate Limiting 策略
**Source:** `BuildingBlocks/Web/RateLimiting/Extensions.cs`
**Apply to:** /auth/* 端点
```csharp
// 认证端点使用 .RequireRateLimiting("auth") — 10/60s 策略
// 已在 RateLimitingExtensions.cs 第 109-113 行定义 "auth" policy
```

## No Analog Found

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| `Features/v1/OAuth/IOAuthProvider.cs` | model | — | 新建抽象接口，无现有 OAuth 框架 |
| `Features/v1/OAuth/OAuthProviderRegistry.cs` | service | CRUD | 新建注册表，无现有 Provider 注册机制 |

**说明:** OAuth Provider 框架是 Phase 1 全新搭建的抽象层，RESEARCH.md Pattern 4 提供了详细的参考设计。

## Key Patterns Summary

1. **所有认证 Scheme 通过 `AddAuthentication().AddXxx()` 链式注册，PolicyScheme 作为 DefaultScheme 路由器**
2. **Domain 实体使用 private 构造函数 + static Create() 工厂方法 + private set 属性封装**
3. **EF Core 配置统一使用 `IdentityModuleConstants.SchemaName`（"identity"）作为 Schema**
4. **Feature Endpoint 使用 Minimal API + Mediator CQRS，Command/Query 在 Contracts 项目中定义**
5. **认证端点使用 `.RequireRateLimiting("auth")` 限流策略**
6. **GlobalExceptionHandler 中根据 `/auth` 路径前缀输出 Plane 兼容错误格式**
7. **IGlobalEntity 标记全局共享实体，IHasTenant 标记租户隔离实体**
8. **测试使用 xunit + Shouldly + NSubstitute，遵循 Arrange/Act/Assert 模式**

## Metadata

**Analog search scope:** `yh-flow/src/Modules/Identity/`, `yh-flow/src/BuildingBlocks/`, `yh-flow/src/Tests/Identity.Tests/`
**Files scanned:** ~100+
**Pattern extraction date:** 2026-06-17
