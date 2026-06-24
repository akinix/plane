# Fullstackhero Pattern Adaptation Guide

**Research Date:** 2026-06-16  
**Source:** `D:/github/fullstackhero-dotnet-starter-kit/` (v10)  
**Rules Index:** `.agents/rules/` (94 rules files)

## 10 Golden Rules (Must Follow)

| #   | Rule                                       | Implementation                                                               |
| --- | ------------------------------------------ | ---------------------------------------------------------------------------- |
| 1   | **Module boundary enforcement**            | 模块间只能通过 `.Contracts` 项目通信                                         |
| 2   | **4-places registration**                  | 新模块必须在 API + DbMigrator 各注册 Mediator assemblies + module assemblies |
| 3   | **Tenant isolation default-ON**            | 所有实体自动多租户隔离，除非标记 `IGlobalEntity`                             |
| 4   | **BuildingBlocks modification protection** | 不修改 BuildingBlocks，除非有充分理由                                        |
| 5   | **Mediator handler conventions**           | `public sealed class`, `ValueTask<T>`, `.ConfigureAwait(false)`              |
| 6   | **Structured logging**                     | 使用 Serilog structured logging，不字符串插值                                |
| 7   | **CancellationToken propagation**          | 所有异步操作传递 CancellationToken                                           |
| 8   | **Validator requirement**                  | 所有 Command + 分页 Query 必须有 FluentValidation validator                  |
| 9   | **Frontend mutation patterns**             | 使用 TanStack Query mutations + zod 验证                                     |
| 10  | **Docs-with-changelog**                    | 模块变更必须有文档 + CHANGELOG                                               |

## Module Structure Convention

```
Modules/{Name}/
├── {Name}.csproj                    # 运行时项目
├── {Name}Module.cs                  # IModule 实现 + 配置
├── Domain/
│   ├── {Entity}.cs                  # 聚合根 / 实体
│   └── Events/
│       └── {Entity}{Action}.Event.cs # 领域事件
├── Data/
│   ├── {Name}DbContext.cs           # 模块数据库上下文
│   └── Configurations/
│       └── {Entity}Configuration.cs # EF Core 配置
├── Features/
│   └── v1/
│       └── {Entity}/
│           └── {Action}/
│               └── {Action}{Entity}Endpoint.cs # Minimal API 端点
└── Events/
    └── {Event}Handler.cs            # 领域事件处理器

Modules/{Name}.Contracts/
├── {Name}.Contracts.csproj
├── IMaster{Name}Module.cs           # 模块标记接口
├── v1/
│   └── {Entity}/
│       ├── {Action}{Entity}Command.cs
│       ├── {Action}{Entity}Query.cs
│       └── Responses/
│           └── {Entity}Response.cs
├── Permissions/
│   └── {Name}Permissions.cs         # 权限常量
└── Dtos/
    └── {Entity}Dto.cs               # 共享 DTO
```

## Entity Patterns

```csharp
// Aggregate Root pattern
public sealed class Project : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable, IHasTenant
{
    private readonly List<ProjectMember> _members = [];

    // Private parameterless constructor for EF Core
    private Project() { }

    // Static factory method
    public static Project Create(string name, string slug, Guid workspaceId, Guid createdBy)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            WorkspaceId = workspaceId,
            // ... other init
        };
        project.AddDomainEvent(new ProjectCreatedEvent(project));
        return project;
    }

    // Properties with private setters
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    // Behavior methods
    public void UpdateName(string name) { /* validation + domain event */ }
    public void AddMember(Guid userId, int role) { /* ... */ }
}
```

## API Endpoint Patterns

```csharp
// Minimal API with permission + idempotency
internal static RouteHandlerBuilder MapCreateProjectEndpoint(this IEndpointRouteBuilder endpoints)
{
    return endpoints.MapPost("/api/v1/workspaces/{slug}/projects",
            async (string slug, CreateProjectCommand command, IMediator mediator, CancellationToken ct) =>
            {
                command = command with { WorkspaceSlug = slug };
                return Results.Ok(await mediator.Send(command, ct));
            })
        .WithName("CreateProject")
        .WithSummary("Create a new project in workspace")
        .RequirePermission(ProjectPermissions.Projects.Create)
        .WithIdempotency();
}
```

## Database Context Pattern

```csharp
public sealed class ProjectDbContext : BaseDbContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public ProjectDbContext(
        DbContextOptions<ProjectDbContext> options,
        ITenantInfo tenantInfo,
        // ... interceptors
    ) : base(tenantInfo, options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaNames.Project);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
        base.OnModelCreating(modelBuilder); // MUST be called last
    }
}
```

## Plane-to-FSH API Adaptation Strategy

由于需要保持 API 兼容，需要在 FSH 模式上做以下适配：

### 1. URL Routing Adaptation

Plane 使用 `workspace_slug` 作为 URL 路径参数，而 FSH 使用 header `tenant` 获取租户。
**策略**: 在 Module 的端点中接受 `slug` 路径参数，内部解析为 Tenant。

```csharp
// Plane-compatible routing
endpoints.MapGet("/api/v1/workspaces/{slug}/projects", async (string slug, ...) =>
{
    // Resolve tenant from slug
    var tenant = await tenantService.GetBySlugAsync(slug);
    // ... continue with tenant context
});
```

### 2. Authentication Adaptation

Plane 使用 Session + API Key 双重认证。
**策略**: 注册自定义 AuthenticationHandler 支持：

- `X-Api-Key` header → API Key 验证
- Cookie → JWT / Session 验证
- `Authorization: Bearer` → JWT 验证（FSH 默认）

### 3. Response Format Adaptation

Plane 使用 Django REST Framework 的分页格式。
**策略**: 自定义 `IPagedQuery` 和 `PagedResponse<T>` 以匹配 Plane 格式：

```json
{ "count": N, "next": "...", "previous": "...", "results": [...] }
```

### 4. Soft Delete Compatibility

Plane 使用 `deleted_at` 字段进行软删除，60 天后硬删除。
**策略**: 利用 FSH 的 `ISoftDeletable` 接口，添加后台清理任务。

## Dependency Injection Registration

每个模块通过 `[assembly: FshModule]` 属性注册：

```csharp
[assembly: FshModule(
    typeof(ProjectModule),
    order: ModuleOrder.Project,
    schema: SchemaNames.Project,
    isolationLevel: ModuleIsolationLevel.Standard)]
```

必须在 4 个位置注册：

1. `Program.cs` Mediator assemblies
2. `Program.cs` moduleAssemblies
3. `DbMigrator/Program.cs` Mediator assemblies
4. `DbMigrator/Program.cs` moduleAssemblies
