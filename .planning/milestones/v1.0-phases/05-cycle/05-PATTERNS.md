# Phase 5: Cycle — Pattern Map

**Mapped:** 2026-06-24
**Files analyzed:** ~30 new/modified files across 5 categories
**Analogs found:** 24 mapped / ~30 total (6 unique patterns with no direct analog)

## File Classification

| New/Modified File                                                  | Role                   | Data Flow         | Closest Analog                                                 | Match Quality    |
| ------------------------------------------------------------------ | ---------------------- | ----------------- | -------------------------------------------------------------- | ---------------- |
| `Modules.WorkItems/Domain/Cycle.cs`                                | domain-entity          | CRUD              | `Issue.cs`                                                     | exact            |
| `Modules.WorkItems/Domain/CycleIssue.cs`                           | domain-entity (bridge) | CRUD              | `IssueAssignee.cs`                                             | exact            |
| `Modules.WorkItems/Data/Configurations/CycleConfiguration.cs`      | config                 | schema            | `IssueConfiguration.cs`                                        | role-match       |
| `Modules.WorkItems/Data/Configurations/CycleIssueConfiguration.cs` | config                 | schema            | `IssueAssigneeConfiguration.cs` + `IssueLabelConfiguration.cs` | exact            |
| `Modules.WorkItems/Data/WorkItemsDbContext.cs`                     | config                 | schema            | existing `DbSet` pattern                                       | exact (modified) |
| `Modules.WorkItems.Contracts/Constants/CycleConstants.cs`          | constants              | none              | `WorkItemsConstants.cs`                                        | exact            |
| `Modules.WorkItems.Contracts/DTOs/CycleDto.cs`                     | dto                    | request-response  | `StateDto.cs`                                                  | exact            |
| `Modules.WorkItems.Contracts/DTOs/CycleProgressDto.cs`             | dto                    | request-response  | none (new format)                                              | no-analog        |
| `Modules.WorkItems.Contracts/v1/Cycles/*/`                         | contracts              | CRUD              | `States.CreateState.Command/Response`                          | exact            |
| `Modules.WorkItems/Features/v1/Cycles/CreateCycle/`                | controller+service     | CRUD              | `States/CreateState/`                                          | exact            |
| `Modules.WorkItems/Features/v1/Cycles/GetCycle/`                   | controller+service     | CRUD              | `States/GetState/`                                             | exact            |
| `Modules.WorkItems/Features/v1/Cycles/UpdateCycle/`                | controller+service     | CRUD              | `States/UpdateState/`                                          | exact            |
| `Modules.WorkItems/Features/v1/Cycles/DeleteCycle/`                | controller+service     | CRUD              | `States/DeleteState/`                                          | exact            |
| `Modules.WorkItems/Features/v1/Cycles/ListCycles/`                 | controller+service     | CRUD+filter       | `States/ListStates/`                                           | role-match       |
| `Modules.WorkItems/Features/v1/Cycles/DateCheckCycle/`             | controller+service     | request-response  | none (unique endpoint)                                         | no-analog        |
| `Modules.WorkItems/Features/v1/Cycles/AddIssuesToCycle/`           | controller+service     | CRUD (child)      | `IssueComments/CreateIssueComment/`                            | role-match       |
| `Modules.WorkItems/Features/v1/Cycles/RemoveIssueFromCycle/`       | controller+service     | CRUD (child)      | `IssueLinks/DeleteIssueLink/`                                  | role-match       |
| `Modules.WorkItems/Features/v1/Cycles/ListCycleIssues/`            | controller+service     | CRUD (child)      | `IssueComments/ListIssueComments/`                             | role-match       |
| `Modules.WorkItems/Features/v1/Cycles/TransferCycleIssues/`        | controller+service     | batch+transaction | none (unique business logic)                                   | no-analog        |
| `Modules.WorkItems/Features/v1/Cycles/GetCycleProgress/`           | controller+service     | aggregation-query | none (unique burndown logic)                                   | no-analog        |
| `Modules.WorkItems/Features/v1/Cycles/ArchiveCycle/`               | controller+service     | CRUD              | none (unique business logic)                                   | no-analog        |
| `Modules.WorkItems/Features/v1/Cycles/UnarchiveCycle/`             | controller+service     | CRUD              | none (unique business logic)                                   | no-analog        |
| `Modules.WorkItems/Features/v1/Cycles/ListArchivedCycles/`         | controller+service     | CRUD+filter       | `States/ListStates/` (extended)                                | partial          |
| `Modules.WorkItems/Services/BurndownCalculator.cs`                 | service                | aggregation       | none (new service)                                             | no-analog        |
| `Modules.WorkItems/WorkItemsModule.cs`                             | config                 | routing           | existing registration pattern                                  | exact (modified) |
| `Tests/Unit/CycleDomainTests.cs`                                   | test                   | unit              | `StateTests.cs`                                                | exact            |
| `Tests/Unit/CycleIssueDomainTests.cs`                              | test                   | unit              | `StateTests.cs`                                                | exact            |
| `Tests/Integration/CycleCrudTests.cs`                              | test                   | integration       | `StateCrudTests.cs`                                            | exact            |
| `Tests/Integration/CycleIssueTests.cs`                             | test                   | integration       | `StateCrudTests.cs`                                            | exact            |
| `Tests/TestData/CycleTestFixture.cs`                               | test                   | fixture           | `WorkItemsTestFixture.cs` + `TestFactories.cs`                 | exact            |

---

## Pattern Assignments

### `Cycles/CreateCycle/` — Vertical Slice (controller + service, CRUD)

**Analog:** `States/CreateState/CreateStateEndpoint.cs` (lines 1-39)

**Imports pattern:**

```csharp
using YH.Framework.Core.Exceptions;
using YH.Modules.WorkItems.Contracts.v1.Cycles.CreateCycle;
using YH.Modules.WorkItems.Data;
using YH.Modules.WorkItems.Domain;
using YH.Modules.Workspace.Authorization;
using YH.Modules.Workspace.Contracts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
```

**Endpoint pattern** (source: `States/CreateState/CreateStateEndpoint.cs` lines 20-38):

```csharp
internal static RouteHandlerBuilder MapCreateCycleEndpoint(this IEndpointRouteBuilder endpoints)
{
    return endpoints.MapPost("/", async (Guid projectId, CreateCycleCommand command,
        IMediator mediator, CancellationToken cancellationToken) =>
    {
        command.ProjectId = projectId;
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/v1/workspaces/{{slug}}/projects/{projectId}/cycles/{result.Id}", result);
    })
    .WithName("CreateCycle")
    .WithSummary("Create cycle")
    .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
    .WithDescription("Create a new cycle (sprint/iteration) for a project.")
    .Produces<CreateCycleResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden);
}
```

**Command Handler pattern** (source: `States/CreateState/CreateStateCommandHandler.cs` lines 14-58):

```csharp
public sealed class CreateCycleCommandHandler : ICommandHandler<CreateCycleCommand, CreateCycleResponse>
{
    private readonly WorkItemsDbContext _db;

    public CreateCycleCommandHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CreateCycleResponse> Handle(CreateCycleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.ProjectId == Guid.Empty)
        {
            throw new CustomException("Project id is required.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Auto-calculate sort_order: min existing sort_order - 10000
        var minSortOrder = await _db.Cycles
            .Where(c => c.ProjectId == command.ProjectId && !c.IsDeleted)
            .MinAsync(c => (double?)c.SortOrder, cancellationToken)
            .ConfigureAwait(false);

        var cycle = Cycle.Create(
            name: command.Name,
            projectId: command.ProjectId,
            startDate: command.StartDate,
            endDate: command.EndDate,
            description: command.Description,
            timezone: command.Timezone);

        cycle.AssignSortOrder(minSortOrder);

        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateCycleResponse(cycle.Id);
    }
}
```

**Validator pattern** (source: `States/CreateState/CreateStateCommandValidator.cs` lines 8-31):

```csharp
public sealed class CreateCycleCommandValidator : AbstractValidator<CreateCycleCommand>
{
    public CreateCycleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cycle name is required.")
            .MaximumLength(CycleConstants.NameMaxLength)
            .WithMessage($"Cycle name must not exceed {CycleConstants.NameMaxLength} characters.");

        // Plane validation: both start/end must be null or both set
        RuleFor(x => x)
            .Must(c => (c.StartDate is null) == (c.EndDate is null))
            .WithMessage("Both start date and end date are either required or both null.");
    }
}
```

**Command Contract pattern** (source: `States/CreateState/CreateStateCommand.cs`):

```csharp
public sealed class CreateCycleCommand : ICommand<CreateCycleResponse>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Timezone { get; set; }

    [JsonIgnore]
    public Guid ProjectId { get; set; }
}
```

---

### `Cycles/GetCycle/` — Vertical Slice (controller + service, CRUD)

**Analog:** `States/GetState/GetStateQueryHandler.cs` (lines 1-43)

**Endpoint pattern** (source: `States/GetState/GetStateEndpoint.cs` lines 16-32):

```csharp
internal static RouteHandlerBuilder MapGetCycleEndpoint(this IEndpointRouteBuilder endpoints)
{
    return endpoints.MapGet("/{cycleId}", async (Guid projectId, Guid cycleId,
        IMediator mediator, CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(new GetCycleQuery
        {
            ProjectId = projectId,
            CycleId = cycleId,
        }, cancellationToken)))
    .WithName("GetCycle")
    .WithSummary("Get cycle by id")
    .RequireAuthorization()
    .WithDescription("Fetch a cycle by id. Returns annotated fields: total_issues, status, etc.")
    .Produces<CycleDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status404NotFound);
}
```

**QueryHandler pattern** (source: `States/GetState/GetStateQueryHandler.cs` lines 13-43):

```csharp
public sealed class GetCycleQueryHandler : IQueryHandler<GetCycleQuery, CycleDto>
{
    private readonly WorkItemsDbContext _db;

    public GetCycleQueryHandler(WorkItemsDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async ValueTask<CycleDto> Handle(GetCycleQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.CycleId == Guid.Empty)
        {
            throw new CustomException("Cycle id is required.", Array.Empty<string>(), System.Net.HttpStatusCode.BadRequest);
        }

        var cycle = await _db.Cycles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CycleId && c.ProjectId == query.ProjectId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
        {
            throw new NotFoundException($"Cycle '{query.CycleId}' was not found.");
        }

        return CycleDtoMapper.ToDto(cycle, _db, query.ProjectId);
    }
}
```

**NOTE:** GetCycle (and ListCycles) must compute annotated fields (total_issues, completed_issues, status) at query time via EF Core sub-queries. See the `CycleDto` section below for the annotation pattern.

---

### `Cycles/ListCycles/` — Vertical Slice (controller + service, CRUD+filter)

**Analog:** `States/ListStates/ListStatesQueryHandler.cs` (lines 1-46) + `IssueComments/ListIssueComments/`

**Endpoint with query parameters pattern** (source: `States/ListStates/ListStatesEndpoint.cs`):

```csharp
internal static RouteHandlerBuilder MapListCyclesEndpoint(this IEndpointRouteBuilder endpoints)
{
    return endpoints.MapGet("/", async (Guid projectId, string? cycleView,
        IMediator mediator, CancellationToken cancellationToken) =>
        TypedResults.Ok(await mediator.Send(new ListCyclesQuery
        {
            ProjectId = projectId,
            CycleView = cycleView,
        }, cancellationToken)))
    .WithName("ListCycles")
    .WithSummary("List cycles")
    .RequireAuthorization()
    .WithDescription("List cycles with cycle_view filter (current/upcoming/completed/draft/incomplete/all).")
    .Produces<List<CycleDto>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden);
}
```

**QueryHandler with status annotation** — This is the most complex CRUD handler. It must:

1. Query cycles with `cycle_view` filter
2. Annotate each cycle with computed `status` (DRAFT/UPCOMING/CURRENT/COMPLETED)
3. Annotate with `total_issues`, `completed_issues`, etc. via sub-queries

```csharp
// Pattern: status computed via EF Core Select projection
// Plane reference: apps/api/plane/app/views/cycle/base.py lines 153-167
var cycles = await _db.Cycles
    .AsNoTracking()
    .Where(c => c.ProjectId == query.ProjectId && !c.IsDeleted && c.ArchivedAt == null)
    .Select(c => new
    {
        Cycle = c,
        Status = c.StartDate == null && c.EndDate == null ? "DRAFT"
            : c.StartDate > DateTimeOffset.UtcNow ? "UPCOMING"
            : c.EndDate < DateTimeOffset.UtcNow ? "COMPLETED"
            : "CURRENT",
        TotalIssues = _db.CycleIssues.Count(ci => ci.CycleId == c.Id && !ci.IsDeleted),
        // ... other annotation sub-queries
    })
    .ToListAsync(cancellationToken);
```

**CycleView filter** — apply before materialization:

```csharp
// Plane reference: apps/api/plane/app/views/cycle/base.py lines 185-268
var filtered = query.CycleView switch
{
    "current" => cycles.Where(c => c.StartDate <= DateTimeOffset.UtcNow && c.EndDate >= DateTimeOffset.UtcNow),
    "upcoming" => cycles.Where(c => c.StartDate > DateTimeOffset.UtcNow),
    "completed" => cycles.Where(c => c.EndDate < DateTimeOffset.UtcNow),
    "draft" => cycles.Where(c => c.StartDate == null && c.EndDate == null),
    "incomplete" => cycles.Where(c => c.EndDate >= DateTimeOffset.UtcNow || c.EndDate == null),
    _ => cycles, // "all" — no filter
};
```

---

### `Cycles/UpdateCycle/` — Vertical Slice (controller + service, CRUD)

**Analog:** `States/UpdateState/UpdateStateCommandHandler.cs` (lines 1-48)

**Key difference:** COMPLETED cycles have restricted edit (D-03). Use `MapMethods` with `["PATCH"]`.

```csharp
// Endpoint: Use MapMethods for PATCH (same pattern as UpdateStateEndpoint)
private static readonly string[] HttpPatch = ["PATCH"];

internal static RouteHandlerBuilder MapUpdateCycleEndpoint(this IEndpointRouteBuilder endpoints)
{
    return endpoints.MapMethods("/{cycleId}", HttpPatch,
        async (Guid projectId, Guid cycleId, UpdateCycleCommand command,
            IMediator mediator, CancellationToken cancellationToken) =>
        {
            command.ProjectId = projectId;
            command.CycleId = cycleId;
            return TypedResults.Ok(await mediator.Send(command, cancellationToken));
        })
    .WithName("UpdateCycle")
    .WithSummary("Update cycle")
    .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member)
    // ...
}
```

**Handler with COMPLETED restriction** (extending UpdateState pattern):

```csharp
public async ValueTask<CycleDto> Handle(UpdateCycleCommand command, CancellationToken cancellationToken)
{
    var cycle = await _db.Cycles
        .FirstOrDefaultAsync(c => c.Id == command.CycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken);

    if (cycle is null)
        throw new NotFoundException($"Cycle '{command.CycleId}' was not found.");

    if (cycle.EndDate < DateTimeOffset.UtcNow) // COMPLETED
    {
        // D-03: COMPLETED cycles only allow name, description, sort_order
        cycle.UpdateRestricted(
            name: command.Name,
            description: command.Description,
            sortOrder: command.SortOrder);
    }
    else
    {
        cycle.Update(
            name: command.Name,
            description: command.Description,
            startDate: command.StartDate,
            endDate: command.EndDate,
            sortOrder: command.SortOrder);
    }

    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    return CycleDtoMapper.ToDto(cycle, _db, command.ProjectId);
}
```

---

### `Cycles/DeleteCycle/` — Vertical Slice (controller + service, CRUD)

**Analog:** `States/DeleteState/DeleteStateCommandHandler.cs` (lines 1-58)

**Soft-delete pattern** (identical to State soft-delete — just soft-delete the entity and let CycleIssue records remain as soft-deletable records):

```csharp
// Direct analog of States/DeleteState pattern
var cycle = await _db.Cycles
    .FirstOrDefaultAsync(c => c.Id == command.CycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken);
if (cycle is null)
    throw new NotFoundException($"Cycle '{command.CycleId}' was not found.");

cycle.SoftDelete(DateTimeOffset.UtcNow);
await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
return Unit.Value;
```

---

### `Cycles/AddIssuesToCycle/` — Vertical Slice (controller + service, child-CRUD)

**Analog:** `IssueComments/CreateIssueComment/CreateIssueCommentCommandHandler.cs` (lines 1-70)

Nested endpoint under `/{cycleId}/cycle-issues/`:

```csharp
// Endpoint — nested under cycle, separate route group in WorkItemsModule
return endpoints.MapPost("/{cycleId}/cycle-issues", async (Guid projectId, Guid cycleId, AddIssuesToCycleCommand command,
    IMediator mediator, CancellationToken cancellationToken) =>
{
    command.ProjectId = projectId;
    command.CycleId = cycleId;
    var result = await mediator.Send(command, cancellationToken);
    return TypedResults.Ok(result);
})
.RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
```

**Handler** — creates CycleIssue records in batch:

```csharp
public async ValueTask<Unit> Handle(AddIssuesToCycleCommand command, CancellationToken cancellationToken)
{
    // 1) Verify cycle exists and not COMPLETED
    var cycle = await _db.Cycles
        .FirstOrDefaultAsync(c => c.Id == command.CycleId && c.ProjectId == command.ProjectId && !c.IsDeleted, cancellationToken);
    if (cycle is null)
        throw new NotFoundException($"Cycle '{command.CycleId}' was not found.");

    if (cycle.EndDate < DateTimeOffset.UtcNow) // COMPLETED
        throw new CustomException("Cannot add issues to a completed cycle.", [], HttpStatusCode.BadRequest);

    // 2) Create CycleIssue records for each issueId
    foreach (var issueId in command.IssueIds)
    {
        var ci = CycleIssue.Create(issueId, command.CycleId);
        _db.Set<CycleIssue>().Add(ci);
    }

    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    return Unit.Value;
}
```

---

### `Cycles/RemoveIssueFromCycle/` — Vertical Slice (controller + service, child-CRUD)

**Analog:** `IssueLinks/DeleteIssueLink/` pattern — simple soft-delete by composite key.

```csharp
// Endpoint: DELETE /{cycleId}/cycle-issues/{issueId}
return endpoints.MapDelete("/{cycleId}/cycle-issues/{issueId}",
    async (Guid projectId, Guid cycleId, Guid issueId,
        IMediator mediator, CancellationToken cancellationToken) =>
    {
        await mediator.Send(new RemoveIssueFromCycleCommand
        {
            ProjectId = projectId,
            CycleId = cycleId,
            IssueId = issueId,
        }, cancellationToken);
        return TypedResults.NoContent();
    })
    .RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
```

---

### `Cycles/ListCycleIssues/` — Vertical Slice (controller + service, child-CRUD)

**Analog:** `IssueComments/ListIssueComments/` pattern — nested list under parent.

```csharp
// Endpoint: GET /{cycleId}/cycle-issues/
return endpoints.MapGet("/{cycleId}/cycle-issues", async (Guid projectId, Guid cycleId,
    IMediator mediator, CancellationToken cancellationToken) =>
    TypedResults.Ok(await mediator.Send(new ListCycleIssuesQuery
    {
        ProjectId = projectId,
        CycleId = cycleId,
    }, cancellationToken)));
```

---

### `Cycles/TransferCycleIssues/` — Vertical Slice (controller + service, batch+transaction)

**Analog:** No direct analog. This is unique business logic. Use the RESEARCH.md code example (Pattern 5) as reference.

**Key operations (single SaveChanges transaction):**

1. Validate new_cycle is not COMPLETED
2. Build progress_snapshot JSON from old cycle annotations
3. Save snapshot: `cycle.FreezeSnapshot(snapshotJson)`
4. Query backlog/unstarted/started CycleIssues
5. Bulk update: set `CycleId = newCycleId`
6. `SaveChanges()` (single transaction)

---

### `Cycles/GetCycleProgress/` — Vertical Slice (controller + service, aggregation-query)

**Analog:** No direct analog in YH.Flow. Use RESEARCH.md Pattern 4 burndown algorithm + `ListIssues` query pattern.

**Endpoint:** `GET /{cycleId}/progress?type=issues|points`

Returns:

```json
{
  "completion_chart": {
    "2026-06-01": 15,
    "2026-06-02": 12,
    ...
  }
}
```

---

### `Cycles/DateCheckCycle/` — Vertical Slice (controller + service, request-response)

**Analog:** No direct analog. Plane `CycleDateCheckEndpoint` algorithm from RESEARCH.md.

**Logic pattern** (Plane reference):

```csharp
// Check date overlap:
// cycles = _db.Cycles.Where(c =>
//     (c.StartDate <= checkStartDate && c.EndDate >= checkStartDate) ||
//     (c.StartDate <= checkEndDate && c.EndDate >= checkEndDate) ||
//     (c.StartDate >= checkStartDate && c.EndDate <= checkEndDate))
// .Exclude(c => c.Id == excludeCycleId)
```

---

### `Cycles/ArchiveCycle/` and `Cycles/UnarchiveCycle/` — Vertical Slice (controller + service, CRUD)

**Analog:** No direct analog. Simple state-change endpoints.

**Archive:** `POST /cycles/{cycleId}/archive/` — sets `ArchivedAt = DateTimeOffset.UtcNow` (only for end_date < now cycles).
**Unarchive:** `DELETE /archived-cycles/{cycleId}/` — sets `ArchivedAt = null`.

```csharp
// Endpoint — POST /{cycleId}/archive/
return endpoints.MapPost("/{cycleId}/archive", async (Guid projectId, Guid cycleId,
    IMediator mediator, CancellationToken cancellationToken) =>
{
    await mediator.Send(new ArchiveCycleCommand { ProjectId = projectId, CycleId = cycleId }, cancellationToken);
    return TypedResults.NoContent();
})
.RequireWorkspaceRole(WorkspaceRole.Admin, WorkspaceRole.Member);
```

### `Cycles/ListArchivedCycles/` — Vertical Slice (controller + service, CRUD+filter)

**Analog:** Extends `ListCycles` pattern. Additional route group `/archived-cycles/`.
Uses `.Where(c => c.ArchivedAt != null)` filter.

---

## Shared Patterns

### 1. Domain Entity — Cycle.cs

**Source:** `Issue.cs` (lines 21-103) and `State.cs` (lines 20-124)

**Interfaces:** `IHasTenant`, `ISoftDeletable`, `IAuditableEntity` (no `IHasDomainEvents` unless activity tracking needed)

**Structure:**

```csharp
public sealed class Cycle : IHasTenant, ISoftDeletable, IAuditableEntity
{
    public Guid Id { get; private set; }
    // ... fields with private setters

    // IHasTenant
    public string TenantId { get; private set; } = default!;
    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private Cycle() { } // EF Core

    public static Cycle Create(string name, Guid projectId, ...) { ... }

    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = now;
    }
}
```

### 2. Bridge Entity — CycleIssue.cs (soft-delete + unique constraint)

**Source:** `IssueAssignee.cs` (lines 1-52) + `IssueLabel.cs` (lines 1-52)

**CycleIssue implements `IHasTenant` AND `ISoftDeletable`** — Unlike IssueAssignee/IssueLabel (full-replacement, no soft-delete), CycleIssue needs soft-delete because the `(Issue, Cycle)` unique constraint must exclude soft-deleted rows.

```csharp
public sealed class CycleIssue : IHasTenant, ISoftDeletable
{
    public Guid Id { get; private set; }
    public Guid IssueId { get; private set; }
    public Guid CycleId { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private set; }

    // IHasTenant
    public string TenantId { get; private set; } = default!;
    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private CycleIssue() { } // EF Core

    public static CycleIssue Create(Guid issueId, Guid cycleId) { ... }
}
```

### 3. EF Configuration — CycleIssueConfiguration.cs (conditional unique index)

**Source:** `IssueAssigneeConfiguration.cs` (lines 14-38) + `StateConfiguration.cs` (lines 59-63 HasFilter)

```csharp
// CycleIssue unique constraint with soft-delete filter
// Plane reference: cycle.py unique_together + UniqueConstraint(condition=Q(deleted_at__isnull=True))
builder.HasIndex(x => new { x.TenantId, x.IssueId, x.CycleId })
    .IsUnique()
    .HasDatabaseName("IX_CycleIssues_Tenant_Issue_Cycle")
    .HasFilter("[DeletedOnUtc] IS NULL");
```

### 4. EF Configuration — CycleConfiguration.cs (indexes)

**Source:** `IssueConfiguration.cs` (lines 105-129) + `StateConfiguration.cs` (lines 60-77)

```csharp
// From IssueConfiguration pattern
builder.HasIndex(x => new { x.ProjectId, x.SortOrder })
    .HasDatabaseName("IX_Cycles_Project_SortOrder");

builder.HasIndex(x => new { x.TenantId, x.IsDeleted, x.ProjectId })
    .HasDatabaseName("IX_Cycles_Tenant_Deleted_Project");
```

### 5. DTO Pattern — CycleDto.cs

**Source:** `StateDto.cs` (lines 1-35)

```csharp
public class CycleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public double SortOrder { get; set; }
    public string? ExternalSource { get; set; }
    public string? ExternalId { get; set; }
    public string? ProgressSnapshot { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public string? LogoProps { get; set; }
    public string Timezone { get; set; } = "UTC";
    public int Version { get; set; }

    // Annotated fields (computed at query time)
    public string Status { get; set; } = default!;       // DRAFT/UPCOMING/CURRENT/COMPLETED
    public int TotalIssues { get; set; }
    public int CompletedIssues { get; set; }
    public int CancelledIssues { get; set; }
    public int StartedIssues { get; set; }
    public int UnstartedIssues { get; set; }
    public int BacklogIssues { get; set; }
    public bool IsFavorite { get; set; }                 // Phase 13 stub: always false
    public List<string> AssigneeIds { get; set; } = [];  // Phase 13 stub: always empty

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
```

**Mapper pattern** (source: `StateDtoMapper.cs`):

```csharp
internal static class CycleDtoMapper
{
    internal static CycleDto ToDto(Cycle c, int totalIssues, int completedIssues, string status) =>
        new()
        {
            Id = c.Id,
            Name = c.Name,
            // ... map all fields
            Status = status,
            TotalIssues = totalIssues,
            CompletedIssues = completedIssues,
            CreatedAt = c.CreatedOnUtc,
            UpdatedAt = c.LastModifiedOnUtc,
        };
}
```

### 6. DbContext Registration — WorkItemsDbContext.cs

**Source:** Existing pattern in `WorkItemsDbContext.cs` (lines 36-67)

Add two new DbSets:

```csharp
/// <summary>Cycles table (Phase 5).</summary>
public DbSet<Cycle> Cycles => Set<Cycle>();

/// <summary>Cycle-Issue M2M through table (Phase 5).</summary>
public DbSet<CycleIssue> CycleIssues => Set<CycleIssue>();
```

### 7. Module Registration — WorkItemsModule.cs

**Source:** `WorkItemsModule.cs` (lines 98-185)

Add cycle route group:

```csharp
// Cycle route group under /workspaces/{slug}/projects/{projectId}/cycles/
var cycles = endpoints
    .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/cycles")
    .WithTags("Cycles")
    .WithApiVersionSet(apiVersionSet);

cycles.MapCreateCycleEndpoint();
cycles.MapListCyclesEndpoint();
cycles.MapGetCycleEndpoint();
cycles.MapUpdateCycleEndpoint();
cycles.MapDeleteCycleEndpoint();
cycles.MapDateCheckCycleEndpoint();

// Cycle-Issue nested endpoints
cycles.MapAddIssuesToCycleEndpoint();
cycles.MapRemoveIssueFromCycleEndpoint();
cycles.MapListCycleIssuesEndpoint();

// Transfer + Progress
cycles.MapTransferCycleIssuesEndpoint();
cycles.MapGetCycleProgressEndpoint();

// Archive/unarchive endpoints
cycles.MapArchiveCycleEndpoint();

// Archived cycles route group (/archived-cycles/)
var archived = endpoints
    .MapGroup("api/v{version:apiVersion}/workspaces/{slug}/projects/{projectId}/archived-cycles")
    .WithTags("Cycles")
    .WithApiVersionSet(apiVersionSet);

archived.MapListArchivedCyclesEndpoint();
archived.MapUnarchiveCycleEndpoint();
```

### 8. RequireWorkspaceRole Authorization

**Source:** `RequireWorkspaceRoleExtensions.cs` (lines 28-51)

All Cycle endpoints use the same `.RequireWorkspaceRole()` extension:

- **Write operations** (Create, Update, Delete, AddIssues, RemoveIssues, Transfer, Archive): `[Admin, Member]`
- **Read operations** (List, Get, ListCycleIssues, Progress): `.RequireAuthorization()` (any authenticated)
- **DateCheck**: `[Admin, Member]`

### 9. Test Patterns

**Unit test pattern** (source: `StateTests.cs`):

```csharp
public sealed class CycleDomainTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var projectId = Guid.NewGuid();
        var cycle = Cycle.Create("Sprint 1", projectId,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(14));

        cycle.Name.ShouldBe("Sprint 1");
        cycle.SortOrder.ShouldBe(65535.0);
        cycle.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithBothDatesNull_CreatesDraft()
    {
        var cycle = Cycle.Create("Draft", Guid.NewGuid());
        cycle.StartDate.ShouldBeNull();
        cycle.EndDate.ShouldBeNull();
    }

    [Fact]
    public void Create_WithOnlyOneDate_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            Cycle.Create("Bad", Guid.NewGuid(), DateTimeOffset.UtcNow, null));
    }
}
```

**Integration test pattern** (source: `StateCrudTests.cs`):

```csharp
[Collection("WorkItemsTest")]
public sealed class CycleCrudTests : IDisposable
{
    private readonly WorkItemsDbContext _db;
    private static readonly Guid ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public CycleCrudTests()
    {
        _db = WorkItemsTestFixture.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateCycle_PersistsToDatabase()
    {
        var cycle = TestCycleFactory.CreateValid(projectId: ProjectId);
        _db.Cycles.Add(cycle);
        await _db.SaveChangesAsync();

        var saved = await _db.Cycles.FirstOrDefaultAsync(c => c.Id == cycle.Id);
        saved.ShouldNotBeNull();
        saved.Name.ShouldBe("Sprint 1");
    }

    public void Dispose() => _db.Dispose();
}
```

**Test factory pattern** (source: `TestFactories.cs`):

```csharp
internal static class TestCycleFactory
{
    private static readonly Guid DefaultProjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static Cycle CreateValid(
        string name = "Sprint 1",
        Guid? projectId = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        var pid = projectId ?? DefaultProjectId;
        var now = DateTimeOffset.UtcNow;
        return Cycle.Create(
            name,
            pid,
            startDate ?? now,
            endDate ?? now.AddDays(14));
    }
}
```

---

## No Analog Found

These files have no close match in the existing codebase. The planner should use RESEARCH.md patterns or the Plane reference implementation instead:

| File                                            | Role               | Data Flow         | Reason                                                       | Reference                                 |
| ----------------------------------------------- | ------------------ | ----------------- | ------------------------------------------------------------ | ----------------------------------------- |
| `Contracts/DTOs/CycleProgressDto.cs`            | dto                | request-response  | Burndown response format is unique (completion_chart dict)   | RESEARCH.md Pattern 4                     |
| `Contracts/v1/Cycles/TransferCycleIssues/`      | contracts          | batch+transaction | Unique multi-step business logic                             | RESEARCH.md Pattern 5                     |
| `Contracts/v1/Cycles/GetCycleProgress/`         | contracts          | aggregation-query | Unique aggregation query                                     | RESEARCH.md Pattern 4                     |
| `Contracts/v1/Cycles/DateCheckCycle/`           | contracts          | request-response  | Unique date-overlap check                                    | RESEARCH.md "日期重叠检查逻辑"            |
| `Contracts/v1/Cycles/ArchiveCycle/` + Unarchive | contracts          | CRUD              | Simple ArchiveAt toggle                                      | Plane `cycle/archive.py`                  |
| `Features/v1/Cycles/TransferCycleIssues/`       | controller+service | batch+transaction | Multi-step: snapshot freeze + bulk update + validation       | Plane `cycle_transfer_issues.py`          |
| `Features/v1/Cycles/GetCycleProgress/`          | controller+service | aggregation-query | TruncDate + cumulative subtraction algorithm                 | Plane `analytics_plot.py` burndown_plot() |
| `Features/v1/Cycles/DateCheckCycle/`            | controller+service | request-response  | Date overlap SQL logic                                       | Plane `CycleDateCheckEndpoint`            |
| `Features/v1/Cycles/ArchiveCycle/` + Unarchive  | controller+service | CRUD              | Business rule: only completed cycles can archive             | Plane `cycle/archive.py`                  |
| `Features/v1/Cycles/ListArchivedCycles/`        | controller+service | CRUD+filter       | Archived cycles route group                                  | Plane `cycle/archive.py`                  |
| `Services/BurndownCalculator.cs`                | service            | aggregation       | Reusable burndown logic shared between progress and transfer | RESEARCH.md Pattern 4                     |

---

## Metadata

**Analog search scope:**

- `yh-flow/src/Modules/WorkItems/Modules.WorkItems/` — Domain, Data/Configurations, Features/v1
- `yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/` — DTOs, Constants, v1/
- `yh-flow/src/Tests/WorkItems.Tests/` — Domain tests, Integration tests, TestData
- `yh-flow/src/Modules/Workspace/Modules.Workspace/Authorization/` — RequireWorkspaceRole
- `yh-flow/src/BuildingBlocks/Shared/Persistence/` — PlanePagedResult

**Files scanned:** ~40+ primary analog files
**Pattern extraction date:** 2026-06-24
