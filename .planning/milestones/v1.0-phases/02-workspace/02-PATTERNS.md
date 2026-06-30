# Phase 2: Workspace — 工作区 - Pattern Map

**Mapped:** 2026-06-17
**Files analyzed:** 31 (new) + 4 (modified) = 35
**Analogs found:** 33 / 35 (94%); 2 require synthesis (slug strategy + workspace tenant store — no precedent in codebase)

## File Classification

> Roles use the planner-friendly set: `entity` / `dbcontext` / `configuration` / `mediator-command` / `mediator-query` / `handler` / `endpoint` / `validator` / `authorization-handler` / `middleware` / `contracts-dto` / `contracts-interface` / `service` / `tenant-strategy` / `tenant-store` / `migration` / `test` / `config`.
> Data Flow uses: `CRUD` / `request-response` / `event-driven` / `batch` / `transform` / `streaming-none`.

### Workspace Module (`Modules.Workspace`)

| New File                                                                                           | Role                                              | Data Flow        | Closest Analog                                                                                                                          | Match Quality                                                      |
| -------------------------------------------------------------------------------------------------- | ------------------------------------------------- | ---------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `Modules.Workspace/WorkspaceModule.cs`                                                             | config                                            | CRUD             | `Modules.Identity/IdentityModule.cs` + `Modules.Auditing/AuditingModule.cs`                                                             | exact                                                              |
| `Modules.Workspace/WorkspaceModuleConstants.cs`                                                    | config                                            | CRUD             | `Modules.Identity/IdentityModuleConstants.cs`                                                                                           | exact                                                              |
| `Modules.Workspace/AssemblyInfo.cs`                                                                | config                                            | CRUD             | `Modules.Auditing/AssemblyInfo.cs`                                                                                                      | exact                                                              |
| `Modules.Workspace/Domain/Workspace.cs`                                                            | entity                                            | CRUD             | `Modules.Identity/Domain/APIToken.cs` + Plane `db/models/workspace.py`                                                                  | role-match (no IGlobalEntity precedent — use IHasTenant.cs marker) |
| `Modules.Workspace/Domain/WorkspaceMember.cs`                                                      | entity                                            | CRUD             | `Modules.Identity/Domain/UserSession.cs`                                                                                                | role-match                                                         |
| `Modules.Workspace/Domain/WorkspaceInvitation.cs`                                                  | entity                                            | CRUD             | `Modules.Identity/Domain/APIToken.cs` (token-hash pattern)                                                                              | role-match                                                         |
| `Modules.Workspace/Data/WorkspaceDbContext.cs`                                                     | dbcontext                                         | CRUD             | `Modules.Auditing/Persistence/AuditDbContext.cs`                                                                                        | exact                                                              |
| `Modules.Workspace/Data/Configurations/WorkspaceConfiguration.cs`                                  | configuration                                     | CRUD             | `Modules.Identity/Data/Configurations/UserSessionConfiguration.cs`                                                                      | exact                                                              |
| `Modules.Workspace/Data/Configurations/WorkspaceMemberConfiguration.cs`                            | configuration                                     | CRUD             | `Modules.Identity/Data/Configurations/UserSessionConfiguration.cs`                                                                      | exact                                                              |
| `Modules.Workspace/Data/Configurations/WorkspaceInvitationConfiguration.cs`                        | configuration                                     | CRUD             | `Modules.Identity/Data/Configurations/UserSessionConfiguration.cs`                                                                      | exact                                                              |
| `Modules.Workspace/Authorization/WorkspaceRole.cs`                                                 | config                                            | CRUD             | `Modules.Identity/Authorization/PermissionAuthorizationRequirement.cs` + Plane ROLE_CHOICES                                             | role-match                                                         |
| `Modules.Workspace/Authorization/RequireWorkspaceRoleAttribute.cs`                                 | authorization-handler                             | request-response | (synthesis) `Modules.Identity/Authorization/RequiredPermissionAuthorizationExtensions.cs` + `RequiredPermissionAuthorizationHandler.cs` | role-match                                                         |
| `Modules.Workspace/Authorization/RequireWorkspaceRoleAuthorizationHandler.cs`                      | authorization-handler                             | request-response | `Modules.Identity/Authorization/RequiredPermissionAuthorizationHandler.cs`                                                              | role-match                                                         |
| `Modules.Workspace/Middleware/WorkspaceMembershipMiddleware.cs`                                    | middleware                                        | request-response | `Modules.Multitenancy/MultitenancyModule.cs:122-207` (post-auth `app.Use` lambdas)                                                      | role-match                                                         |
| `Modules.Workspace/MultiTenancy/WorkspaceSlugStrategy.cs`                                          | tenant-strategy                                   | transform        | `Modules.Multitenancy/MultitenancyModule.cs:100-109` (`WithDelegateStrategy`)                                                           | partial (first slug-aware strategy in codebase)                    |
| `Modules.Workspace/MultiTenancy/WorkspaceTenantStore.cs`                                           | tenant-store                                      | CRUD             | (synthesis) `EFCoreStore<TenantDbContext, AppTenantInfo>` referenced at `MultitenancyModule.cs:111` + RESEARCH §Example 2               | no analog (new `IMultiTenantStore<AppTenantInfo>` impl)            |
| `Modules.Workspace/Services/ISlugGenerator.cs` + `SlugGenerator.cs`                                | service                                           | transform        | (no analog) `Modules.Identity/Services/ApiTokenService.cs` (BCL-only service style)                                                     | role-match (RESEARCH §Example 4)                                   |
| `Modules.Workspace/Services/IInvitationTokenService.cs` + `InvitationTokenService.cs`              | service                                           | CRUD             | `Modules.Identity/Services/ApiTokenService.cs`                                                                                          | exact                                                              |
| `Modules.Workspace/Services/WorkspaceMembershipService.cs`                                         | service                                           | CRUD             | `Modules.Identity/Services/SessionService.cs` (CRUD service over DbContext)                                                             | role-match                                                         |
| `Modules.Workspace/Features/v1/Workspaces/CreateWorkspace/{Command,Handler,Validator,Endpoint}.cs` | mediator-command / handler / endpoint / validator | request-response | `Modules.Identity/Features/v1/Users/RegisterUser/{*}.cs`                                                                                | exact                                                              |
| `Modules.Workspace/Features/v1/Workspaces/GetWorkspace/{Endpoint,Handler}.cs`                      | mediator-query / handler / endpoint               | request-response | `Modules.Identity/Features/v1/Users/GetUserById/{*}.cs`                                                                                 | exact                                                              |
| `Modules.Workspace/Features/v1/Workspaces/UpdateWorkspace/{*}.cs`                                  | mediator-command / handler / endpoint / validator | request-response | `Modules.Identity/Features/v1/Users/UpdateUser/{*}.cs`                                                                                  | exact                                                              |
| `Modules.Workspace/Features/v1/Workspaces/DeleteWorkspace/{*}.cs`                                  | mediator-command / handler / endpoint             | request-response | `Modules.Identity/Features/v1/Users/DeleteUser/{*}.cs`                                                                                  | exact                                                              |
| `Modules.Workspace/Features/v1/Workspaces/ListUserWorkspaces/{*}.cs`                               | mediator-query / handler / endpoint               | request-response | `Modules.Identity/Features/v1/Users/GetUsers/{*}.cs` + `PlanePagedResult`                                                               | exact                                                              |
| `Modules.Workspace/Features/v1/Workspaces/CheckWorkspaceSlug/{*}.cs`                               | mediator-query / handler / endpoint               | request-response | `Modules.Identity/Features/v1/Users/SearchUsers/{*}.cs`                                                                                 | role-match                                                         |
| `Modules.Workspace/Features/v1/Members/ListMembers/{*}.cs`                                         | mediator-query / handler / endpoint               | batch            | `Modules.Identity/Features/v1/Users/GetUsers/GetUsersQueryHandler.cs` (batch + AsNoTracking)                                            | role-match (N+1 avoid via D-05)                                    |
| `Modules.Workspace/Features/v1/Members/UpdateMemberRole/{*}.cs`                                    | mediator-command / handler / endpoint / validator | request-response | `Modules.Identity/Features/v1/Users/AssignUserRoles/{*}.cs`                                                                             | role-match                                                         |
| `Modules.Workspace/Features/v1/Members/RemoveMember/{*}.cs`                                        | mediator-command / handler / endpoint             | request-response | `Modules.Identity/Features/v1/Users/DeleteUser/{*}.cs`                                                                                  | role-match                                                         |
| `Modules.Workspace/Features/v1/Invitations/CreateInvitation/{*}.cs`                                | mediator-command / handler / endpoint / validator | request-response | `Modules.Identity/Features/v1/Users/RegisterUser/{*}.cs` + `ApiTokenService.CreateAsync`                                                | role-match                                                         |
| `Modules.Workspace/Features/v1/Invitations/{List,Revoke,Accept,Reject}Invitation/{*}.cs`           | mediator-command-or-query / handler / endpoint    | request-response | `Modules.Identity/Features/v1/Users/{GetUsers,DeleteUser,ConfirmEmail}/{*}.cs`                                                          | role-match                                                         |

### Workspace.Contracts Module (`Modules.Workspace.Contracts`)

| New File                                                                                          | Role                | Data Flow        | Closest Analog                                                                        | Match Quality                                     |
| ------------------------------------------------------------------------------------------------- | ------------------- | ---------------- | ------------------------------------------------------------------------------------- | ------------------------------------------------- |
| `Modules.Workspace.Contracts/ICurrentWorkspaceContext.cs`                                         | contracts-interface | CRUD             | `BuildingBlocks/Core/Context/ICurrentUser.cs`                                         | role-match (synthesis — wraps Finbuckle accessor) |
| `Modules.Workspace.Contracts/IWorkspaceTenantResolver.cs`                                         | contracts-interface | CRUD             | (no analog) — new facade                                                              | no analog                                         |
| `Modules.Workspace.Contracts/INotificationService.cs`                                             | contracts-interface | event-driven     | (no analog) — Phase 11 placeholder                                                    | no analog (stub only)                             |
| `Modules.Workspace.Contracts/DTOs/{Workspace,WorkspaceMember,WorkspaceInvitation}Dto.cs`          | contracts-dto       | CRUD             | `Modules.Identity.Contracts/DTOs/UserDto.cs` + `APITokenDto.cs`                       | exact                                             |
| `Modules.Workspace.Contracts/Constants/RestrictedSlugs.cs`                                        | config              | CRUD             | (no analog) — Plane `utils/constants.py` port                                         | no analog (literal port)                          |
| `Modules.Workspace.Contracts/v1/Workspaces/CreateWorkspace/CreateWorkspaceCommand.cs` (+Response) | contracts-dto       | request-response | `Modules.Identity.Contracts/v1/Users/RegisterUser/RegisterUserCommand.cs` (+Response) | exact                                             |
| `Modules.Workspace.Contracts/v1/Workspaces/{Get,Update,Delete,ListUserWorkspaces,CheckSlug}/*.cs` | contracts-dto       | request-response | `Modules.Identity.Contracts/v1/Users/GetUsers/GetUsersQuery.cs`                       | role-match                                        |
| `Modules.Workspace.Contracts/AssemblyInfo.cs`                                                     | config              | CRUD             | `Modules.Auditing.Contracts/obj/.../AssemblyInfo.cs` (set on real AssemblyInfo)       | exact                                             |

### Identity.Contracts extension (D-04/D-05)

| New File                                                      | Role                | Data Flow | Closest Analog                                        | Match Quality |
| ------------------------------------------------------------- | ------------------- | --------- | ----------------------------------------------------- | ------------- |
| `Modules.Identity.Contracts/DTOs/UserSummary.cs`              | contracts-dto       | CRUD      | `Modules.Identity.Contracts/DTOs/UserDto.cs`          | exact         |
| `Modules.Identity.Contracts/Services/IUserIdentityService.cs` | contracts-interface | batch     | `Modules.Identity.Contracts/Services/IUserService.cs` | role-match    |

### Migration & Host wiring (modified)

| New/Modified File                                                                         | Role      | Data Flow | Closest Analog                                                            | Match Quality |
| ----------------------------------------------------------------------------------------- | --------- | --------- | ------------------------------------------------------------------------- | ------------- |
| `Host/YH.Flow.Migrations.PostgreSQL/Workspace/InitialWorkspace.cs` (+Designer + Snapshot) | migration | CRUD      | `Host/.../Identity/20260617103113_AddAPITokenAndOAuthProviderSettings.cs` | exact         |
| `Host/YH.Flow.Migrations.PostgreSQL/YH.Flow.Migrations.PostgreSQL.csproj`                 | config    | CRUD      | existing csproj (add Folder + ProjectReference)                           | exact         |
| `Host/YH.Flow.DbMigrator/Program.cs` (modified)                                           | config    | CRUD      | existing `Program.cs:79-119` (mediator assemblies + module assemblies)    | exact         |
| `Host/YH.Flow.DbMigrator/YH.Flow.DbMigrator.csproj` (modified)                            | config    | CRUD      | existing csproj (add ProjectReference)                                    | exact         |
| `Host/YH.Flow.Api/Program.cs` (modified)                                                  | config    | CRUD      | existing `Program.cs:57-65` (`moduleAssemblies` array)                    | exact         |
| `Multitenancy/MultitenancyModule.cs` (modified — Wave 0 spike may relocate)               | config    | CRUD      | existing `MultitenancyModule.cs:77-111` (strategy/store chain)            | exact         |

### Tests

| New File                                                                         | Role   | Data Flow        | Closest Analog                                                                         | Match Quality |
| -------------------------------------------------------------------------------- | ------ | ---------------- | -------------------------------------------------------------------------------------- | ------------- |
| `Tests/Workspace.Tests/Workspace.Tests.csproj`                                   | config | CRUD             | `Tests/Identity.Tests/Identity.Tests.csproj`                                           | exact         |
| `Tests/Workspace.Tests/GlobalUsings.cs`                                          | config | CRUD             | `Tests/Identity.Tests/GlobalUsings.cs`                                                 | exact         |
| `Tests/Workspace.Tests/Services/SlugGeneratorTests.cs`                           | test   | transform        | `Tests/Identity.Tests/Services/ApiTokenServiceTests.cs` (BCL static-method test style) | role-match    |
| `Tests/Workspace.Tests/Services/InvitationTokenServiceTests.cs`                  | test   | CRUD             | `Tests/Identity.Tests/Services/ApiTokenServiceTests.cs`                                | exact         |
| `Tests/Workspace.Tests/Authorization/RequireWorkspaceRoleHandlerTests.cs`        | test   | request-response | (synthesis) `Tests/Identity.Tests/Authorization/*.cs`                                  | role-match    |
| `Tests/Workspace.Tests/Domain/WorkspaceTests.cs` + `WorkspaceInvitationTests.cs` | test   | CRUD             | `Tests/Identity.Tests/Domain/APITokenTests.cs`                                         | exact         |
| `Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs`      | test   | request-response | (no analog — Wave 0 spike for Q1)                                                      | no analog     |

---

## Pattern Assignments

### `Modules.Workspace/WorkspaceModule.cs` (config, CRUD)

**Analogs:** `yh-flow/src/Modules/Identity/IdentityModule.cs` (full IModule wiring) + `yh-flow/src/Modules/Auditing/AuditingModule.cs` (slim IModule with ConfigureMiddleware)

**Imports pattern** — copy from `AuditingModule.cs:1-24`:

```csharp
using Asp.Versioning;
using YH.Framework.Persistence;             // AddHeroDbContext
using YH.Framework.Web.Modules;             // IModule
using YH.Modules.Workspace.Contracts;       // contracts
using YH.Modules.Workspace.Features.v1.Workspaces.CreateWorkspace;
// ...one using per Feature slice
using YH.Modules.Workspace.Persistence;     // WorkspaceDbContext
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
```

**IModule shell** — copy from `AuditingModule.cs:27-107` (identity is bigger; auditing is the right skeleton):

```csharp
public sealed class WorkspaceModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHeroDbContext<WorkspaceDbContext>();
        builder.Services.AddScoped<ISlugGenerator, SlugGenerator>();
        builder.Services.AddScoped<IInvitationTokenService, InvitationTokenService>();
        builder.Services.AddScoped<WorkspaceMembershipService>();
        builder.Services.AddScoped<ICurrentWorkspaceContext, CurrentWorkspaceContext>(); // implementation
        // Wave 0 spike Q1 outcome decides: TryAddEnumerable IMultiTenantStrategy/IMultiTenantStore here,
        //   OR expose AddWorkspaceTenantResolution(builder) called from MultitenancyModule.
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<WorkspaceDbContext>(name: "db:workspace", failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseMiddleware<WorkspaceMembershipMiddleware>();  // D-02
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersion()
            .Build();
        // Two route groups: top-level (no slug) + workspace-scoped (with {slug})
        var topLevel = endpoints.MapGroup("api/v{version:apiVersion}/workspaces").WithTags("Workspaces").WithApiVersionSet(apiVersionSet);
        var scoped   = endpoints.MapGroup("api/v{version:apiVersion}/workspaces/{slug}").WithTags("Workspaces").WithApiVersionSet(apiVersionSet);
        topLevel.MapCreateWorkspaceEndpoint();
        topLevel.MapListUserWorkspacesEndpoint();
        topLevel.MapCheckWorkspaceSlugEndpoint();
        scoped.MapGetWorkspaceEndpoint();
        // ... members, invitations
    }
}
```

**Module registration attribute** — copy from `Modules.Auditing/AssemblyInfo.cs:1-3` (literal pattern):

```csharp
using YH.Framework.Web.Modules;
[assembly: FshModule(typeof(YH.Modules.Workspace.WorkspaceModule), 200)]  // Order=200; pick slot between Identity/Multitenancy (lower) and Auditing (300)
```

---

### `Modules.Workspace/WorkspaceModuleConstants.cs` (config)

**Analogs:** `yh-flow/src/Modules/Identity/IdentityModuleConstants.cs` (literal copy + adapt).

```csharp
using YH.Framework.Web.Modules;

namespace YH.Modules.Workspace;

public sealed class WorkspaceModuleConstants : IModuleConstants
{
    public string ModuleId => "Workspace";
    public string ModuleName => "Workspace";
    public string ApiPrefix => "workspaces";
    public const string SchemaName = "yhschema.Workspace";   // CONTEXT.md D-13 / Phase 1 schema strategy
    public const int SlugMaxLength = 48;                     // D-09
}
```

---

### `Modules.Workspace/Data/WorkspaceDbContext.cs` (dbcontext, CRUD)

**Analog (exact, copy the pattern):** `yh-flow/src/Modules/Auditing/Persistence/AuditDbContext.cs`

**Imports + ctor + OnModelCreating order** — copy verbatim from `AuditDbContext.cs:1-52`, only swap `AuditRecord`→ three DbSets:

```csharp
using Finbuckle.MultiTenant.Abstractions;
using YH.Framework.Persistence.Context;
using YH.Framework.Shared.Multitenancy;
using YH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace YH.Modules.Workspace.Persistence;

public sealed class WorkspaceDbContext : BaseDbContext
{
    public WorkspaceDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<WorkspaceDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment)
        : base(multiTenantContextAccessor, options, settings, environment) { }

    public DbSet<Domain.Workspace> Workspaces => Set<Domain.Workspace>();
    public DbSet<WorkspaceMember> Members => Set<WorkspaceMember>();
    public DbSet<WorkspaceInvitation> Invitations => Set<WorkspaceInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        // 1) ApplyConfigurationsFromAssembly FIRST so per-entity configs (unique indexes, owned types)
        //    are in place before BaseDbContext calls ApplyTenantIsolationByDefault().
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkspaceDbContext).Assembly);
        // 2) base.OnModelCreating runs LAST → auto IsMultiTenant() on every non-IGlobalEntity + soft-delete filter.
        base.OnModelCreating(modelBuilder);
    }
}
```

**Key adaptation (CONTEXT.md `<code_context>` + RESEARCH Pitfall 6):** `Workspace` entity MUST implement `IGlobalEntity` (not `IHasTenant`). `BaseDbContext.OnModelCreating:39` → `ApplyTenantIsolationByDefault()` skips `IGlobalEntity` (`TenantIsolationExtensions.cs:41`). `WorkspaceMember` + `WorkspaceInvitation` deliberately omit `IGlobalEntity` so they get auto tenant-scoped.

---

### `Modules.Workspace/Data/Configurations/*.cs` (configuration)

**Analogs:** `yh-flow/src/Modules/Identity/Data/Configurations/UserSessionConfiguration.cs` (multi-tenant-aware config with ToTable + indexes) + `yh-flow/src/Modules/Auditing/Persistence/AuditRecordConfiguration.cs` (explicit `IsMultiTenant()` + composite indexes).

**Imports pattern** — copy from `AuditRecordConfiguration.cs:1-4`:

```csharp
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;  // IsMultiTenant(), AdjustUniqueIndexes()
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
```

**Per-entity config pattern** — copy structure from `UserSessionConfiguration.cs:7-79`:

```csharp
public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("WorkspaceMembers", WorkspaceModuleConstants.SchemaName);
        // Do NOT call IsMultiTenant() explicitly — BaseDbContext.ApplyTenantIsolationByDefault() adds it.
        // (AuditRecordConfiguration.cs:13 calls it because Auditing needs it explicit; the auto-default is now in place.)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);  // cross-module scalar, NO FK (D-04)
        builder.Property(x => x.Role).HasConversion<int>();
        builder.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();  // one membership per user per workspace
        // TenantId column appears via IsMultiTenant() — AdjustUniqueIndexes() widens this composite.
    }
}
```

**WorkspaceConfiguration (CRITICAL — Workspace is IGlobalEntity):**

```csharp
public void Configure(EntityTypeBuilder<Workspace> builder)
{
    builder.ToTable("Workspaces", WorkspaceModuleConstants.SchemaName);
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Slug).IsRequired().HasMaxLength(48);
    builder.HasIndex(x => x.Slug).IsUnique();                 // case-sensitive unique on active slug (D-08: soft-delete appends __{epoch})
    builder.Property(x => x.OwnerId).IsRequired();            // Guid scalar, NO FK (D-06)
    builder.Ignore(x => x.TenantId);                          // Workspace is IGlobalEntity — has no TenantId
    // NEVER call builder.IsMultiTenant() here (Pitfall 6).
}
```

---

### `Modules.Workspace/Authorization/RequireWorkspaceRoleAuthorizationHandler.cs` (authorization-handler)

**Analogs:** `yh-flow/src/Modules/Identity/Authorization/RequiredPermissionAuthorizationHandler.cs` (handler shape) + `RequiredPermissionAuthorizationExtensions.cs:21-35` (policy registration pattern).

**Imports pattern** — copy from `RequiredPermissionAuthorizationHandler.cs:1-6`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using YH.Modules.Workspace.Contracts;   // ICurrentWorkspaceContext lives in Contracts (D-03)
```

**Handler shape** — adapt `RequiredPermissionAuthorizationHandler.cs:9-41`:

```csharp
public sealed class RequireWorkspaceRoleAuthorizationHandler(ICurrentWorkspaceContext workspaceContext)
    : AuthorizationHandler<RequireWorkspaceRoleAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RequireWorkspaceRoleAttribute requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        // Difference from analog: identity reads from IUserService.HasPermissionAsync; workspace reads
        // pre-populated context (no DB hit — D-02 middleware already filled it).
        var role = workspaceContext.CurrentUserRole;
        if (role is null) { context.Fail(); return Task.CompletedTask; }
        if (requirement.Roles.Contains(role.Value)) context.Succeed(requirement);
        else context.Fail();
        return Task.CompletedTask;
    }
}
```

**Attribute + IAuthorizationRequirement** — adapt `PermissionAuthorizationRequirement.cs:1-4` (one-liner marker) into an attribute carrying roles:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireWorkspaceRoleAttribute : Attribute, IAuthorizationRequirement
{
    public WorkspaceRole[] Roles { get; }
    public RequireWorkspaceRoleAttribute(params WorkspaceRole[] roles) => Roles = roles;
}
```

**Policy/handler DI registration** — copy `RequiredPermissionAuthorizationExtensions.cs:30-33` pattern:

```csharp
builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, RequireWorkspaceRoleAuthorizationHandler>());
```

**Endpoint usage** — mirror Identity's `.RequirePermission(...)` extension by adding `.RequireAuthorization(new RequireWorkspaceRoleAttribute(WorkspaceRole.Admin))` to the route group (see `RegisterUserEndpoint.cs:28`).

---

### `Modules.Workspace/Middleware/WorkspaceMembershipMiddleware.cs` (middleware)

**Analogs:** `yh-flow/src/Modules/Multitenancy/MultitenancyModule.cs:122-207` (post-auth `app.Use(async (ctx, next) => ...)` inline middleware pattern, reads `IMultiTenantContextAccessor` + `ClaimsPrincipal`).

**Inline middleware pattern to copy** — `MultitenancyModule.cs:129-148`:

```csharp
app.Use(async (ctx, next) =>
{
    // Read tenant (workspaceGuid) resolved by Finbuckle slug strategy in UseMultiTenant() phase.
    var accessor = ctx.RequestServices.GetRequiredService<IMultiTenantContextAccessor<AppTenantInfo>>();
    var tenantInfo = accessor.MultiTenantContext?.TenantInfo;

    // Skip on top-level endpoints (no slug → no tenant → user-scoped only).
    if (tenantInfo is not null && Guid.TryParse(tenantInfo.Id, out var workspaceId))
    {
        var userId = ctx.User?.GetUserId();  // extension from YH.Framework.Shared.Identity.Claims (used at RequiredPermissionAuthorizationHandler.cs:36)
        if (userId is { } uid)
        {
            var db = ctx.RequestServices.GetRequiredService<WorkspaceDbContext>();
            // Tenant-filtered query — DbContext already scoped to this workspace.
            var member = await db.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == uid.ToString() && m.IsActive, ctx.RequestAborted)
                .ConfigureAwait(false);
            // Fill ICurrentWorkspaceContext (CurrentWorkspaceId / Slug / CurrentUserRole). null role = not a member.
            ctx.RequestServices.GetRequiredService<ICurrentWorkspaceContext>().Set(workspaceId, tenantInfo.Identifier, member?.Role);
        }
    }
    await next(ctx).ConfigureAwait(false);
});
```

**Recommendation:** implement as a typed `WorkspaceMembershipMiddleware : IMiddleware` class (instead of inline `app.Use`), registered in `WorkspaceModule.ConfigureMiddleware` (see `AuditingModule.cs:68-72` for `app.UseMiddleware<T>()` style).

---

### `Modules.Workspace/MultiTenancy/WorkspaceSlugStrategy.cs` (tenant-strategy)

**Analog (partial — first slug-aware strategy in codebase):** `yh-flow/src/Modules/Multitenancy/MultitenancyModule.cs:100-109` (`WithDelegateStrategy` inline lambda) + RESEARCH §Example 1.

**Inline delegate pattern from the analog** — `MultitenancyModule.cs:100-109`:

```csharp
.WithDelegateStrategy(async context =>
{
    if (context is not HttpContext httpContext) return null;
    if (!httpContext.Request.Query.TryGetValue("tenant", out var tenantIdentifier) ||
        string.IsNullOrEmpty(tenantIdentifier))
        return null;
    return await Task.FromResult(tenantIdentifier.ToString());
})
```

**Adaptation (Pitfall 2)** — pull slug from **route value**, not query string:

```csharp
.WithDelegateStrategy<HttpContext, AppTenantInfo>(async httpContext =>
{
    if (httpContext.GetRouteValue("slug") is string slug && !string.IsNullOrEmpty(slug))
        return await Task.FromResult<string?>(slug);
    return null;  // top-level /workspaces/, /users/me/... → next strategy (claim/header) takes over
})
```

**Registration order is critical** (RESEARCH §Pattern 1): this delegate MUST be appended FIRST in the strategy chain (before existing `WithClaimStrategy`/`WithHeaderStrategy`). Wave 0 spike Q1 decides whether to register here (if Finbuckle DI allows external `IMultiTenantStrategy` append) or relocate into `MultitenancyModule` via a `Workspace.Contracts` extension method.

---

### `Modules.Workspace/MultiTenancy/WorkspaceTenantStore.cs` (tenant-store)

**Analogs (synthesis — no `IMultiTenantStore<AppTenantInfo>` implementation exists):** RESEARCH §Example 2 + the store referenced at `MultitenancyModule.cs:111` (`WithStore<EFCoreStore<TenantDbContext, AppTenantInfo>>`).

**Imports pattern** — based on RESEARCH Example 2 + MultitenancyModule usings:

```csharp
using Finbuckle.MultiTenant.Abstractions;          // IMultiTenantStore<TTenantInfo>, AppTenantInfo (via Shared.Multitenancy)
using Finbuckle.MultiTenant.Stores;               // base helpers
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;    // Phase 1 AddHeroCaching IDistributedCache
using System.Text.Json;
using YH.Modules.Workspace.Persistence;
```

**Store skeleton** — RESEARCH §Example 2 is the authoritative blueprint (only `TryGetAsync` matters for resolution; `AddAsync/UpdateAsync/RemoveAsync/GetAllAsync` delegate to cache-invalidate or no-op since workspace is the source of truth):

```csharp
public sealed class WorkspaceTenantStore(WorkspaceDbContext db, IDistributedCache cache) : IMultiTenantStore<AppTenantInfo>
{
    public async Task<AppTenantInfo?> TryGetAsync(string identifier)  // identifier = slug
    {
        // 1) Cache lookup
        // 2) DB lookup against IGlobalEntity Workspace (NOT tenant-filtered — it IS the tenant)
        // 3) Return null if not found → next store (EFCoreStore<TenantDbContext>) tries
    }
}
```

**Registration (depends on Wave 0 spike):** preferred `services.TryAddEnumerable(ServiceDescriptor.Scoped<IMultiTenantStore<AppTenantInfo>, WorkspaceTenantStore>())` in `WorkspaceModule.ConfigureServices`; fallback is `Workspace.Contracts` extension `AddWorkspaceTenantResolution(this MultiTenantBuilder<AppTenantInfo>)` invoked from `MultitenancyModule`.

---

### `Modules.Workspace/Services/IInvitationTokenService.cs` + `InvitationTokenService.cs` (service, CRUD)

**Analog (exact):** `yh-flow/src/Modules/Identity/Services/ApiTokenService.cs`

**Imports pattern** — copy from `ApiTokenService.cs:1-9`:

```csharp
using System.Security.Cryptography;
using System.Text;
using YH.Modules.Workspace.Contracts;
using YH.Modules.Workspace.Persistence;
using YH.Modules.Identity.Contracts.Services;   // IUserIdentityService for email lookup (D-04)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
```

**Token generation + SHA-256 hash** — copy verbatim from `ApiTokenService.cs:175-187`:

```csharp
private const int TokenByteLength = 32;

private static string GenerateRawToken()
{
    var bytes = RandomNumberGenerator.GetBytes(TokenByteLength);
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

private static string HashToken(string rawToken)
{
    var bytes = Encoding.UTF8.GetBytes(rawToken);
    var hashBytes = SHA256.HashData(bytes);
    return Convert.ToHexString(hashBytes).ToLowerInvariant();
}
```

**CreateAsync + RevokeAsync shape** — mirror `ApiTokenService.cs:35-108` (generate raw → hash → persist hash only → return raw to caller; revoke flips `IsActive`). For invitations: `Accepted`/`RespondedAt` flag replaces `IsActive`; TTL via `ExpiredAt` (same field pattern at `ApiTokenService.cs:110-139` IsExpired check). Validation-lookup mirrors `ValidateAndGetOwnerAsync:110-173`: hash the incoming raw token, query by `TokenHash`, check `!Accepted && RespondedAt == null && !IsExpired`.

---

### `Modules.Workspace/Services/ISlugGenerator.cs` + `SlugGenerator.cs` (service, transform)

**Analogs:** `yh-flow/src/Modules/Identity/Services/ApiTokenService.cs` (BCL-only service style: static crypto helpers + DbContext-scoped CRUD) + RESEARCH §Example 4 (slugify self-impl).

**Slugify + validate** — RESEARCH §Example 4 is the authoritative blueprint (it itself references the same BCL-only convention):

```csharp
private static readonly HashSet<string> RestrictedSlugs = new(StringComparer.OrdinalIgnoreCase)
{
    /* port 66 entries from Plane utils/constants.py RESTRICTED_WORKSPACE_SLUGS
       via Workspace.Contracts/Constants/RestrictedSlugs.cs */
};

public static string Slugify(string input) { /* RESEARCH §Example 4 */ }
public static bool IsValidSlug(string slug) { /* RESEARCH §Example 4 */ }
```

**Conflict retry pattern** (CONTEXT D-07 + RESEARCH Pitfall 4):

```csharp
public async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken ct)
{
    var base = string.IsNullOrWhiteSpace(name) ? null : Slugify(name);
    for (int attempt = 0; attempt < 5; attempt++)
    {
        var candidate = attempt == 0 ? base : $"{base}-{GenerateShortSuffix()}";
        if (!await _db.Workspaces.AnyAsync(w => w.Slug == candidate, ct).ConfigureAwait(false))
            return candidate;
    }
    throw new ConflictException("Could not generate a unique slug after 5 attempts.");
}
```

---

### `Modules.Workspace/Services/WorkspaceMembershipService.cs` (service, CRUD)

**Analog:** `yh-flow/src/Modules/Identity/Services/SessionService.cs` (CRUD-over-DbContext service exposing Add/Revoke/List, constructor-injects DbContext + ILogger).

**Imports pattern** — copy from any Identity service (e.g. `ApiTokenService.cs:1-9`).

**Operation set to implement (CONTEXT D-06 + REQ-2.2):**

- `AddOwnerAsync(workspaceId, userId, ct)` — called from CreateWorkspace handler, inserts `WorkspaceMember(role=Admin, isActive=true)`.
- `ListAsync(workspaceId, ct)` — returns members (tenant-filtered automatically).
- `UpdateRoleAsync(memberId, role, ct)`.
- `RemoveAsync(memberId, ct)` — soft-delete (`IsDeleted=true`).

---

### `Modules.Workspace/Domain/Workspace.cs` + `WorkspaceMember.cs` + `WorkspaceInvitation.cs` (entity)

**Analog:** `yh-flow/src/Modules/Identity/Domain/APIToken.cs` (Phase 1 newest entity, demonstrates: `IHasDomainEvents` + `IHasTenant` + private setters + `Create` factory + `Revoke` state-transition method + `IsExpired()` check).

**Imports pattern** — copy from `APIToken.cs:1-3`:

```csharp
using YH.Framework.Core.Domain;     // IHasDomainEvents, IDomainEvent, IHasTenant, IGlobalEntity, ISoftDeletable
```

**Entity shell** — copy structure from `APIToken.cs:10-58`:

```csharp
public sealed class Workspace : IHasDomainEvents, IGlobalEntity   // NOT IHasTenant (RESEARCH Pitfall 6)
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public Guid Id { get; private set; }
    // ...fields (RESEARCH §Example 5 enumerates them, mapped from Plane workspace.py:88-110)
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private Workspace() { }                                       // EF Core
    public static Workspace Create(...) { /* like APIToken.Create */ }
    public void SoftDelete(DateTimeOffset now)
    {
        IsDeleted = true; DeletedOnUtc = now;
        Slug = $"{Slug}__{(int)now.ToUnixTimeSeconds()}";         // D-08 epoch release
    }
}
```

**WorkspaceMember / WorkspaceInvitation** — same shell, but `IHasTenant` + `ISoftDeletable` (auto tenant-scoped by `BaseDbContext`). WorkspaceInvitation mirrors `APIToken` token-hash fields (`TokenHash`, `ExpiredAt`, state-transition `Accept()`/`Reject()`/`Revoke()` methods analogous to `APIToken.Revoke():71-74`).

---

### `Modules.Workspace/Features/v1/Workspaces/CreateWorkspace/{Command,Handler,Validator,Endpoint}.cs` (mediator-command slice)

**Analogs (exact 4-file slice):**

- Command: `Modules.Identity.Contracts/v1/Users/RegisterUser/RegisterUserCommand.cs` + `RegisterUserResponse.cs`
- Handler: `Modules.Identity/Features/v1/Users/RegisterUser/RegisterUserCommandHandler.cs`
- Validator: `Modules.Identity/Features/v1/Users/RegisterUser/RegisterUserCommandValidator.cs`
- Endpoint: `Modules.Identity/Features/v1/Users/RegisterUser/RegisterUserEndpoint.cs`

**Command + Response** — copy from `RegisterUserCommand.cs:1-18` (records, `[JsonIgnore]` for server-injected fields like `Origin`):

```csharp
using Mediator;
using System.Text.Json.Serialization;

namespace YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace;

public class CreateWorkspaceCommand : ICommand<CreateWorkspaceResponse>
{
    public string Name { get; set; } = default!;
    public string? Slug { get; set; }     // optional — slugify(Name) if null (D-07)
    public string? Logo { get; set; }
    public string? TimeZone { get; set; }
    [JsonIgnore] public Guid OwnerUserId { get; set; }  // server-injected from ICurrentUser
}

public record CreateWorkspaceResponse(Guid Id, string Slug);
```

**Handler** — copy from `RegisterUserCommandHandler.cs:1-33` (constructor-injects service, validates args, calls service, maps to response):

```csharp
public sealed class CreateWorkspaceCommandHandler(ISlugGenerator slugGenerator, WorkspaceMembershipService memberships, WorkspaceDbContext db, ICurrentUser user)
    : ICommandHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
{
    public async ValueTask<CreateWorkspaceResponse> Handle(CreateWorkspaceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var slug = string.IsNullOrWhiteSpace(cmd.Slug)
            ? await slugGenerator.GenerateUniqueSlugAsync(cmd.Name, ct).ConfigureAwait(false)
            : cmd.Slug;
        // Validate restricted words / format via ISlugGenerator.IsValidSlug
        var ws = Workspace.Create(cmd.Name, slug, ownerUserId: user.GetUserId(), cmd.Logo, cmd.TimeZone);
        db.Workspaces.Add(ws);
        await memberships.AddOwnerAsync(ws.Id, ws.OwnerId, ct).ConfigureAwait(false);  // D-06 auto Admin member
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return new CreateWorkspaceResponse(ws.Id, ws.Slug);
    }
}
```

**Validator** — copy from `RegisterUserCommandValidator.cs:1-39`:

```csharp
public sealed class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);   // Plane name max 80
        RuleFor(x => x.Slug).MaximumLength(48)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .When(x => x.Slug is not null)
            .WithMessage("Slug must be lowercase letters, digits, and hyphens.");
        // Restricted-word check done in handler/service (requires DI on ISlugGenerator).
    }
}
```

**Endpoint** — copy from `RegisterUserEndpoint.cs:1-36`:

```csharp
public static class CreateWorkspaceEndpoint
{
    internal static RouteHandlerBuilder MapCreateWorkspaceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", async (CreateWorkspaceCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(command, ct);
                return Results.Created($"/api/v1/workspaces/{result.Slug}", result);
            })
            .WithName("CreateWorkspace")
            .WithSummary("Create a workspace")
            .RequireAuthorization()  // any authenticated user can create; no [RequireWorkspaceRole] (top-level route)
            .Produces<CreateWorkspaceResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
```

> **Note:** Identity uses `.RequirePermission(...)` (FSH platform-level). Workspace uses `.RequireAuthorization()` for top-level routes and `.RequireAuthorization(new RequireWorkspaceRoleAttribute(...))` for scoped routes. The two attribute systems coexist.

---

### `Modules.Workspace/Features/v1/Workspaces/GetWorkspace/{Endpoint,Handler}.cs` + `ListUserWorkspaces/*` + `UpdateWorkspace/*` + `DeleteWorkspace/*` + `CheckWorkspaceSlug/*`

**Analogs:** `Modules.Identity/Features/v1/Users/GetUserById/`, `GetUsers/`, `UpdateUser/`, `DeleteUser/`, `SearchUsers/` respectively.

**List endpoint pattern** — copy from `GetUsersListEndpoint.cs:1-26` (one-liner `MapGet`, mediator → service).
**List handler pattern (N+1 avoid — Pitfall 3):** copy the batch-resolve shape from `GetUsersQueryHandler.cs:8-21`, but for `ListMembers` extend with `IUserIdentityService.GetUsersByIdsAsync(userIds)` per CONTEXT D-05.
**DeleteWorkspace handler adaptation (RESEARCH Pitfall 4):** before SaveChanges, call `workspace.SoftDelete(now)` (entity method) which rewrites `Slug` with `__{epoch}` suffix to release the slug for reuse.

---

### `Modules.Workspace/Features/v1/Members/ListMembers/{*}.cs` (batch)

**Analogs:** `GetUsersQueryHandler.cs:8-21` (batch + AsNoTracking) + `ApiTokenService.ListForUserAsync` (`ApiTokenService.cs:64-86` for the AsNoTracking Select-into-DTO projection pattern).

**Critical adaptation (CONTEXT D-05 + RESEARCH Pitfall 3):** collect `member.UserId`s in one pass, call `IUserIdentityService.GetUsersByIdsAsync(IEnumerable<Guid>)` once, then zip into `WorkspaceMemberDto` (NEVER loop-call per member).

---

### `Modules.Workspace.Contracts/ICurrentWorkspaceContext.cs` (contracts-interface)

**Analogs:** `yh-flow/src/BuildingBlocks/Core/Context/ICurrentUser.cs` (request-scoped context interface with read methods) + RESEARCH §D-03.

```csharp
namespace YH.Modules.Workspace.Contracts;

public interface ICurrentWorkspaceContext
{
    Guid? CurrentWorkspaceId { get; }
    string? Slug { get; }
    WorkspaceRole? CurrentUserRole { get; }
    // Set by WorkspaceMembershipMiddleware (scoped implementation)
    void Set(Guid workspaceId, string slug, WorkspaceRole? role);
}
```

**Implementation lives in `Modules.Workspace`** (not Contracts), registered `Scoped` in `WorkspaceModule.ConfigureServices`. Pattern: `CurrentUserService` in Identity implements `ICurrentUser` (the analog at `Modules.Identity/Services/CurrentUserService.cs`).

---

### `Modules.Workspace.Contracts/DTOs/{Workspace,WorkspaceMember,WorkspaceInvitation}Dto.cs` (contracts-dto)

**Analogs:** `Modules.Identity.Contracts/DTOs/UserDto.cs` (plain POCO with public setters) + `APITokenDto.cs` (record-style DTO).

```csharp
namespace YH.Modules.Workspace.Contracts.DTOs;

public class WorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public Guid OwnerId { get; set; }
    public string? Logo { get; set; }
    public string TimeZone { get; set; } = "UTC";
    // ... Plane-mapped fields per RESEARCH §Example 5
}

public class WorkspaceMemberDto
{
    public Guid Id { get; set; }
    public int Role { get; set; }                       // 20/15/5
    public bool IsActive { get; set; }
    public UserSummary? User { get; set; }              // joined from IUserIdentityService (D-04/D-05)
}
```

---

### `Modules.Identity.Contracts/DTOs/UserSummary.cs` + `Modules.Identity.Contracts/Services/IUserIdentityService.cs` (cross-module contracts)

**Analogs:** `Modules.Identity.Contracts/DTOs/UserDto.cs` (slimmed) + `Modules.Identity.Contracts/Services/IUserService.cs` (interface).

**UserSummary** — slim `UserDto` to 4 fields (CONTEXT `Claude's Discretion`):

```csharp
public record UserSummary(Guid Id, string? DisplayName, string? Email, string? AvatarUrl);
```

**IUserIdentityService** — extend the existing `IUserService.cs:6-36` pattern with one batch method:

```csharp
public interface IUserIdentityService
{
    Task<IReadOnlyDictionary<Guid, UserSummary>> GetUsersByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct);
}
```

> Implement in `Modules.Identity/Services/` (analog `UserService.cs`), registered `Transient` in `IdentityModule.ConfigureServices:118`. Phase 2 only ADDS this method — it does not modify existing `IUserService` surface.

---

### `Host/YH.Flow.Migrations.PostgreSQL/Workspace/{Initial + Designer + Snapshot}.cs` (migration)

**Analog (exact):** `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/Identity/20260617103113_AddAPITokenAndOAuthProviderSettings.cs` (Phase 1 latest migration — schema + table + unique indexes).

**Migration body pattern** — copy from `AddAPITokenAndOAuthProviderSettings.cs:12-104`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Workspaces",
        schema: "yhschema.Workspace",     // CONTEXT D-13
        columns: table => new { /* ... */ },
        constraints: table => table.PrimaryKey("PK_Workspaces", x => x.Id));

    // WorkspaceMembers: include TenantId column (EFCore IsMultiTenant adds it); composite unique on (UserId, TenantId)
    // WorkspaceInvitations: TokenHash unique + (Accepted, TenantId) index
    // Indexes mirror the Configuration classes
}
```

**Designer + Snapshot:** EF Core scaffolds these automatically from `dotnet ef migrations add Initial -c WorkspaceDbContext -o Workspace -p src/Host/YH.Flow.Migrations.PostgreSQL -s src/Host/YH.Flow.DbMigrator`. Snapshot file: `WorkspaceDbContextModelSnapshot.cs` next to the migration (analog: `Identity/IdentityDbContextModelSnapshot.cs`).

**Project edits:** `YH.Flow.Migrations.PostgreSQL.csproj` — add `<ProjectReference Include="..\..\Modules\Workspace\Modules.Workspace\Modules.Workspace.csproj" />` (pattern at csproj:13-22) and `<Folder Include="Workspace\" />` (csproj:26-34).

---

### `Host/YH.Flow.DbMigrator/Program.cs` (modified) + `.csproj` (modified)

**Analogs (exact):** existing `DbMigrator/Program.cs:79-119` (mediator + module assembly arrays) + `DbMigrator.csproj:45-49` (ProjectReference lines).

**Mediator assemblies array** — append two entries to the array at `Program.cs:82-104`:

```csharp
typeof(YH.Modules.Workspace.Contracts.v1.Workspaces.CreateWorkspace.CreateWorkspaceCommand),
typeof(YH.Modules.Workspace.WorkspaceModule),
```

**Module assemblies array** — append one entry to the array at `Program.cs:107-119`:

```csharp
typeof(YH.Modules.Workspace.WorkspaceModule).Assembly,
```

**Csproj** — add ProjectReferences for both `Modules.Workspace` and `Modules.Workspace.Contracts` (pattern at `DbMigrator.csproj:45-49`).

---

### `Host/YH.Flow.Api/Program.cs` (modified)

**Analog (exact):** existing `Program.cs:57-65` (`moduleAssemblies` array).

```csharp
var moduleAssemblies = new Assembly[]
{
    typeof(IdentityModule).Assembly,
    typeof(MultitenancyModule).Assembly,
    typeof(AuditingModule).Assembly,
    typeof(YH.Modules.Files.FilesModule).Assembly,
    typeof(WebhooksModule).Assembly,
    typeof(YH.Modules.Notifications.NotificationsModule).Assembly,
    typeof(YH.Modules.Workspace.WorkspaceModule).Assembly,   // NEW
};
```

> `AddModules(moduleAssemblies)` (`ModuleLoader.cs:15-55`) auto-discovers `[FshModule]` on the assembly and runs `ConfigureServices` / `ConfigureMiddleware` / `MapEndpoints`. No further host edits required.

---

### `Tests/Workspace.Tests/*` (test)

**Analogs:** `yh-flow/src/Tests/Identity.Tests/` (project structure) + `Identity.Tests/Services/ApiTokenServiceTests.cs` (BCL static-method test style) + `Identity.Tests/Domain/APITokenTests.cs` (entity test style).

**Csproj** — copy verbatim from `Identity.Tests.csproj:1-30`, change `ProjectReference` lines:

```xml
<ProjectReference Include="..\..\Modules\Workspace\Modules.Workspace\Modules.Workspace.csproj" />
<ProjectReference Include="..\..\Modules\Workspace\Modules.Workspace.Contracts\Modules.Workspace.Contracts.csproj" />
```

**Test framework stack** — `Identity.Tests.csproj:11-25`: xunit + Shouldly + AutoFixture + NSubstitute + Microsoft.EntityFrameworkCore.InMemory. FluentAssertions is mentioned in RESEARCH §Validation Architecture but the actual project uses **Shouldly** — follow the actual csproj.

**SlugGeneratorTests.cs / InvitationTokenServiceTests.cs** — copy `ApiTokenServiceTests.cs:1-60` style (static `[Fact]` methods, `ShouldBe`/`ShouldStartWith` assertions, no DbContext needed for crypto/slugify math).

**RequireWorkspaceRoleHandlerTests.cs** — new pattern (no direct analog in Identity.Tests/Authorization); model on `PathAwareAuthorizationHandler` shape via NSubstitute: stub `ICurrentWorkspaceContext`, build `AuthorizationHandlerContext`, assert `HasSucceeded`/`HasFailed`.

**FinbuckleExternalStrategyRegistrationTests.cs (Wave 0 spike)** — no analog. Validates RESEARCH Open Question Q1: does `services.TryAddEnumerable<IMultiTenantStrategy<AppTenantInfo>, WorkspaceSlugStrategy>()` register successfully and resolve in a `/workspaces/{slug}/` request? If it fails, the spike produces the fallback design (`AddWorkspaceTenantResolution` extension called from `MultitenancyModule`).

---

## Shared Patterns

### Authentication

**Source:** `yh-flow/src/Host/YH.Flow.Api/Program.cs` (auth scheme wiring from Phase 1) + `Modules.Identity/Authorization/RequiredPermissionAuthorizationExtensions.cs:25-31`.
**Apply to:** All Workspace endpoints. Use `.RequireAuthorization()` (any authenticated user) for top-level workspace routes (`POST/GET /workspaces/`, `/users/me/workspaces/invitations/`) and `.RequireAuthorization(new RequireWorkspaceRoleAttribute(...))` for `{slug}`-scoped routes.

### Multi-Tenant Resolution & Isolation (Phase 1 baseline)

**Source:** `Modules.Multitenancy/MultitenancyModule.cs:77-111` (strategy/store chain) + `BuildingBlocks/Persistence/TenantIsolationExtensions.cs` (auto-isolation) + `BaseDbContext.cs:32-40` (soft-delete filter + ApplyTenantIsolationByDefault).
**Apply to:** Every Workspace entity below the `Workspace` root.

- `Workspace` = `IGlobalEntity` (no auto-tenant)
- `WorkspaceMember` + `WorkspaceInvitation` = (no marker) → auto `IsMultiTenant()` + `AdjustUniqueIndexes()` widens unique indexes with `TenantId`.
- **NEVER call `services.AddMultiTenant<AppTenantInfo>(...)` again** (RESEARCH Pitfall 1). Append via `services.TryAddEnumerable<...>` (preferred) or extension method (fallback).

### Permission / Role Authorization

**Source:** `Modules.Identity/Authorization/RequiredPermissionAuthorizationHandler.cs:9-41` (handler) + `RequiredPermissionAuthorizationExtensions.cs:21-35` (policy + DI registration).
**Apply to:** All `RequireWorkspaceRole*` files. Adapt: identity calls `userService.HasPermissionAsync` (DB hit); workspace reads `ICurrentWorkspaceContext.CurrentUserRole` (no DB hit — pre-filled by middleware).

### Token Generation + Hashing

**Source:** `Modules.Identity/Services/ApiTokenService.cs:175-187` (RandomNumberGenerator + SHA256) + `ApiTokenService.cs:35-108` (Create/Revoke state-transition pattern) + `ApiTokenService.cs:110-173` (Validate-lookup-by-hash pattern).
**Apply to:** `InvitationTokenService` (D-12). Verbatim copy of the static crypto helpers. Adapt state transitions: `Accept()`/`Reject()`/`Revoke()` instead of `Revoke()` alone; `Accepted + RespondedAt` instead of `IsActive`.

### Slug Generation (project convention: BCL-only, no NuGet)

**Source:** `Modules.Identity/Services/ApiTokenService.cs` (project precedent for BCL-only utilities) + RESEARCH §Example 4 (slugify algorithm).
**Apply to:** `SlugGenerator`. ~20-line Regex-based implementation; restricted-word list ported from Plane `utils/constants.py` (66 entries) into `Workspace.Contracts/Constants/RestrictedSlugs.cs`.

### Domain Entity Shape

**Source:** `Modules.Identity/Domain/APIToken.cs` (Phase 1 newest entity) + `Modules.Identity/Domain/UserSession.cs` (richer entity with domain events).
**Apply to:** All 3 Workspace entities. Common shape: `IHasDomainEvents`, private setters, EF Core `private` ctor, `static Create(...)` factory, state-transition methods (`SoftDelete`, `Accept`, `Revoke`). `IHasTenant.cs:6-12` is one property (`string TenantId`).

### Mediator Vertical Slice (4-file Feature)

**Source:** `Modules.Identity/Features/v1/Users/RegisterUser/` (Command in Contracts + Handler/Validator/Endpoint in module).
**Apply to:** Every Workspace Feature (Workspaces, Members, Invitations). Identical 4-file layout: command/query + response in `Contracts/v1/...`, handler + validator + endpoint in `Features/v1/...`. `Mediator` source-gen scans via `AddMediator(... assemblies ...)` registered in DbMigrator and API (must append Workspace assemblies).

### DbContext Derivation

**Source:** `Modules.Auditing/Persistence/AuditDbContext.cs` (slim `BaseDbContext` subclass with the correct `OnModelCreating` ordering).
**Apply to:** `WorkspaceDbContext`. Hard rule: `ApplyConfigurationsFromAssembly(...)` BEFORE `base.OnModelCreating(...)` (so `ApplyTenantIsolationByDefault()` sees configs — comment in `AuditDbContext.cs:48-51`).

### Module Self-Registration

**Source:** `Modules.Auditing/AssemblyInfo.cs:1-3` + `BuildingBlocks/Web/Modules/ModuleLoader.cs:32-49` (auto-discovery via `[FshModule]` attribute).
**Apply to:** `Modules.Workspace/AssemblyInfo.cs`. `[assembly: FshModule(typeof(WorkspaceModule), 200)]`. `Order` is a hint — pick a slot that runs AFTER Identity/Multitenancy but BEFORE Auditing (which uses 300).

### Plane-Compatible Pagination

**Source:** `BuildingBlocks/Shared/Persistence/PlanePagedResult.cs:9-22` (`count`/`next`/`previous`/`results`) + `PlanePagedResultFactory.FromPagedResponse` (lines 34-67).
**Apply to:** `ListUserWorkspaces`, `ListMembers`, `ListInvitations` handlers/endpoints.

### Error Handling & Validation

**Source:** Phase 1 already-registered: `GlobalExceptionHandler` + `FluentValidation` `ValidationBehavior` pipeline (`ModuleLoader.cs:26` auto-runs `AddValidatorsFromAssemblies`).
**Apply to:** All Workspace endpoints. No try/catch in handlers; throw `ConflictException`/`NotFoundException`/`ForbiddenException` (see `MultitenancyModule.cs:175-176` for `ForbiddenException` usage). Validators are picked up automatically by namespace convention.

---

## No Analog Found

| File                                                                                     | Role                | Data Flow        | Reason                                                                                                                                                                                                                                                 | Fallback Source                                                                                                    |
| ---------------------------------------------------------------------------------------- | ------------------- | ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------ |
| `Modules.Workspace/MultiTenancy/WorkspaceTenantStore.cs`                                 | tenant-store        | CRUD             | No `IMultiTenantStore<AppTenantInfo>` implementation exists in the codebase (only `EFCoreStore<TenantDbContext, AppTenantInfo>` is referenced, not implemented locally).                                                                               | RESEARCH §Example 2 (authoritative blueprint) + Finbuckle v10.1.1 Stores docs.                                     |
| `Modules.Workspace/MultiTenancy/WorkspaceSlugStrategy.cs` (as standalone strategy class) | tenant-strategy     | transform        | No route-value-based strategy exists. Existing strategies are claim/header/query (`MultitenancyModule.cs:98-109`). Prefer `WithDelegateStrategy<HttpContext, AppTenantInfo>` inline (analog at `MultitenancyModule.cs:100-109`) over a separate class. | RESEARCH §Example 1 + the inline `WithDelegateStrategy` analog.                                                    |
| `Modules.Workspace.Contracts/IWorkspaceTenantResolver.cs`                                | contracts-interface | CRUD             | Pure facade for downstream modules — no precedent; may not be needed if downstream modules read `ICurrentWorkspaceContext` directly.                                                                                                                   | Defer until a downstream module (Phase 3 Project) actually needs it (YAGNI — Q in RESEARCH Open Questions spirit). |
| `Modules.Workspace.Contracts/INotificationService.cs`                                    | contracts-interface | event-driven     | Phase 11 placeholder (CONTEXT `<deferred>`); no notification abstraction in current code.                                                                                                                                                              | RESEARCH §D-10: stub interface only, no implementation in Phase 2.                                                 |
| `Modules.Workspace.Contracts/Constants/RestrictedSlugs.cs`                               | config              | CRUD             | Literal port of Plane `RESTRICTED_WORKSPACE_SLUGS` (66 strings). No slug-restriction precedent in codebase.                                                                                                                                            | RESEARCH §Example 4 + Plane `apps/api/plane/utils/constants.py`.                                                   |
| `Tests/Workspace.Tests/Spike/FinbuckleExternalStrategyRegistrationTests.cs`              | test                | request-response | No analog — Wave 0 spike validating RESEARCH Open Question Q1.                                                                                                                                                                                         | RESEARCH §Open Questions Q1 — write minimal strategy + assert `IMultiTenantContextAccessor` populated.             |

---

## Metadata

**Analog search scope:**

- `yh-flow/src/Modules/Identity/` (primary — most complete module)
- `yh-flow/src/Modules/Auditing/` (DbContext derivation template)
- `yh-flow/src/Modules/Multitenancy/` (Finbuckle strategy/store registration)
- `yh-flow/src/BuildingBlocks/{Persistence,Web,Core,Shared}/` (interfaces, base classes, pagination)
- `yh-flow/src/Host/{YH.Flow.Api,YH.Flow.DbMigrator,YH.Flow.Migrations.PostgreSQL}/` (host wiring)
- `yh-flow/src/Tests/Identity.Tests/` (test scaffolding)

**Files scanned:** 23 analog files read in full (all small, ≤ 250 lines each).
**Pattern extraction date:** 2026-06-17
**Cross-references:** CONTEXT.md (D-01~D-12 decisions), RESEARCH.md (§Architecture Patterns, §Code Examples 1-5, §Pitfalls 1-6, §Open Questions Q1-Q3).

---

## PATTERN MAPPING COMPLETE

**Phase:** 02 - workspace
**Files classified:** 35 (31 new + 4 modified)
**Analogs found:** 33 / 35

### Coverage

- Files with exact analog: 22 (DbContext, configurations, entities, mediator slices, migration, csproj edits, module registration, DTOs, command/response contracts, csproj test scaffolding)
- Files with role-match analog: 11 (authorization handler, membership middleware, slug service, invitation token service, query endpoints, member-list batch handler, role/attr, user-summary extension)
- Files with no analog: 6 (WorkspaceTenantStore, WorkspaceSlugStrategy-as-class, IWorkspaceTenantResolver, INotificationService, RestrictedSlugs constant, Wave 0 spike test) — all have explicit fallback sources in RESEARCH.md or are deferred/stub.

### Key Patterns Identified

- **DbContext derivation** — `WorkspaceDbContext` copies `AuditDbContext.cs` verbatim (ctor + `OnModelCreating` order: `ApplyConfigurationsFromAssembly` → `base.OnModelCreating`); `Workspace` entity MUST be `IGlobalEntity` (RESEARCH Pitfall 6).
- **Module self-registration** — `[FshModule(typeof(WorkspaceModule), 200)]` on `AssemblyInfo.cs`; `AddModules` auto-discovers, no host boilerplate beyond appending to the `moduleAssemblies` array.
- **Authorization** — `RequireWorkspaceRoleAuthorizationHandler` adapts `RequiredPermissionAuthorizationHandler.cs` shape; reads pre-populated `ICurrentWorkspaceContext` instead of hitting `IUserService` (no DB call in handler).
- **Token crypto** — `ApiTokenService.cs:175-187` `RandomNumberGenerator.GetBytes(32)` + `SHA256.HashData` is the project's BCL-only crypto precedent; `InvitationTokenService` copies it verbatim.
- **Slug resolution** — Finbuckle `WithDelegateStrategy<HttpContext, AppTenantInfo>` (analog at `MultitenancyModule.cs:100-109`) reads `HttpContext.GetRouteValue("slug")`; `WorkspaceTenantStore : IMultiTenantStore<AppTenantInfo>` is the only true no-analog file (RESEARCH Example 2 is the blueprint).
- **Batch user resolution (N+1 avoid)** — `ListMembers` must call `IUserIdentityService.GetUsersByIdsAsync` once, not per-member; modeled on the `GetUsersQueryHandler.cs` batch shape + `ApiTokenService.ListForUserAsync` AsNoTracking projection.

### File Created

`D:\github\akinix-plane\.planning\phases\02-workspace\02-PATTERNS.md`

### Ready for Planning

Pattern mapping complete. Planner can now reference analog file paths + line-number ranges directly in PLAN.md action sections; the 6 no-analog files each have a named fallback (RESEARCH section or "stub/defer") so no plan is blocked.
