---
phase: 03-project
plan: 02
type: execute
wave: 2
subsystem: Project module
tags: [domain, persistence, ef-core, migration, tenant-isolation]
requires: [03-01]
provides: [Project entity, ProjectMember entity, ProjectDbContext, EF migration]
affects: [Workspace module (tenant chain), Identity module (user refs)]
tech-stack:
  added: []
  patterns:
    [
      IHasTenant tenant isolation,
      conditional unique indexes with HasFilter,
      SortOrder default 65535.0,
      namespace alias collision pattern,
    ]
key-files:
  created:
    - src/Modules/Project/Modules.Project/Domain/ProjectNetwork.cs — enum Secret=0, Public=2
    - src/Modules/Project/Modules.Project/Domain/Project.cs — Project aggregate entity
    - src/Modules/Project/Modules.Project/Domain/ProjectMember.cs — Project membership entity
    - src/Modules/Project/Modules.Project/Data/ProjectDbContext.cs — DbContext
    - src/Modules/Project/Modules.Project/Data/Configurations/ProjectConfiguration.cs — EF config
    - src/Modules/Project/Modules.Project/Data/Configurations/ProjectMemberConfiguration.cs — EF config
    - src/Host/YH.Flow.Migrations.PostgreSQL/Project/20260624042542_InitialProject.cs — migration
    - src/Host/YH.Flow.Migrations.PostgreSQL/Project/20260624042542_InitialProject.Designer.cs — migration designer
    - src/Host/YH.Flow.Migrations.PostgreSQL/Project/ProjectDbContextModelSnapshot.cs — model snapshot
  modified:
    - src/Modules/Project/Modules.Project/ProjectModule.cs — real ConfigureServices + MapEndpoints
    - src/Modules/Project/Modules.Project/Modules.Project.csproj — added CA1027 to NoWarn
decisions:
  - "UserId on ProjectMember: string (matching Identity's FshUser.Id column type per D-04), NOT Guid as in ProjectMemberDto scaffold. DTO will be adjusted in mapping layer."
  - "Identifier uniqueness: conditional HasFilter('[DeletedOnUtc] IS NULL') index — Identifier is NOT epoch-modified on soft delete, released by filter"
  - "Slug uniqueness: unconditional unique index — Slug IS epoch-modified on soft delete per D-03/Workspace pattern"
  - "Name uniqueness: conditional HasFilter (TenantId, Name) — name release on soft delete"
  - "SortOrder default 65535.0 per Plane convention"
  - "Network default Public=2 per Plane convention"
  - "Namespace alias pattern (using ProjectEntity = ...) for namespace collision with YH.Modules.Project namespace"
metrics:
  duration:
  completed_date: 2026-06-24
---

# Phase 3 Plan 02: Domain entities, DbContext, and EF migration — Summary

Created the Project aggregate entity, ProjectNetwork enum, ProjectMember entity, ProjectDbContext with tenant isolation, EF Core entity configurations with conditional unique indexes, updated ProjectModule with real ConfigureServices, and generated the InitialProject EF migration.

## Tasks Completed

### Task 1: Create domain entities

- **ProjectNetwork.cs** — Enum with Secret=0, Public=2 (matching Plane's network field schema)
- **Project.cs** — Project aggregate implementing `IHasDomainEvents, IHasTenant, ISoftDeletable, IAuditableEntity`:
  - Fields per Plane: name, description, descriptionText, descriptionHtml, network (Public default), identifier (auto-uppercased), slug, ownerId, projectLeadId, defaultAssigneeId, emoji, iconProp, coverImageUrl, logoProps, timeZone, 8 feature toggles, archiveIn, closeIn, archivedAt, externalSource, externalId, sortOrder (65535.0 default)
  - Tenant-scoped (implements IHasTenant, NOT IGlobalEntity)
  - Factory `Create(...)` with validation, `Update(...)` with nullable parameters, `SoftDelete(now)` (epoch-modifies Slug only — Identifier released via HasFilter), `Archive(now)`, `Unarchive()`, `SetSortOrder(sortOrder)`
- **ProjectMember.cs** — Membership entity with same interfaces:
  - Fields: projectId, userId (string per D-04), role (int 20/15/5), isActive, sortOrder (65535.0 default)
  - Factory `Create(...)`, `UpdateRole(role)`, `Deactivate()`, `Activate()`, `SetSortOrder(sortOrder)`

**Commits:** `64d6f2bc4`, `3c227df87` (deviation fix)

### Task 2: Create DbContext and EF configurations

- **ProjectDbContext.cs** — Derives from `BaseDbContext` with Pitfall 6 ordering: `ApplyConfigurationsFromAssembly` FIRST, then `base.OnModelCreating`. DbSets: `Projects`, `Members`. Namespace alias for `ProjectEntity` collision.
- **ProjectConfiguration.cs** — `ToTable("Projects", "yhschema.Project")`:
  - Conditional unique index `(TenantId, Identifier)` with `HasFilter("[DeletedOnUtc] IS NULL")` — T-3-domain-03
  - Conditional unique index `(TenantId, Name)` with `HasFilter("[DeletedOnUtc] IS NULL")`
  - Unconditional unique index on `Slug` — epoch-modified on soft delete
  - SortOrder default 65535.0, Network default Public=2
  - ArchivedAt+SortOrder composite index, OwnerId performance index
- **ProjectMemberConfiguration.cs** — `ToTable("ProjectMembers", "yhschema.Project")`:
  - Unique index `(TenantId, ProjectId, UserId)` — one membership per user per project
  - Composite indexes: `(ProjectId, IsActive)`, `(TenantId, UserId, IsActive)`
  - SortOrder default 65535.0, IsActive default true

**Commits:** `844ca7755`, `3c227df87` (deviation fix)

### Task 3: Complete ProjectModule + generate EF migration

- **ProjectModule.cs** — `ConfigureServices` registers `ProjectDbContext` via `AddHeroDbContext` + health check `"db:project"`. `MapEndpoints` with scaffolded route groups for projects and members (TODO for 03-03/03-04). Added `Microsoft.Extensions.DependencyInjection` using for `AddHealthChecks()`.
- **InitialProject migration** — Creates `Projects` and `ProjectMembers` tables in `yhschema.Project` schema with all columns, indexes, and HasFilter constraints.

**Commits:** `c0618dd77`, `bfc925049`

### Deviations from Plan

**Auto-fixed Issues**

**1. [Rule 2 — Missing HasFilter indexes]** Initial implementation used unconditional unique indexes for Identifier and omitted Name unique index. The plan specifies conditional unique indexes with `HasFilter("[DeletedOnUtc] IS NULL")` for (TenantId, Identifier) and (TenantId, Name). Fixed by updating ProjectConfiguration.cs and regenerating the migration.

**2. [Rule 2 — Missing fields]** Initial entities omitted: DescriptionText, DescriptionHtml, ExternalSource, ExternalId, SortOrder (double, default 65535.0 on both entities), ProjectMember.SortOrder. Added per plan specification.

**3. [Rule 2 — Missing defaults]** Initial factory defaulted Network to Secret=0 instead of Public=2 (plan-specified default). SortOrder and IsActive defaults were missing. All fixed.

**4. [Rule 2 — Missing using]** `ProjectModule.cs` was missing `using Microsoft.Extensions.DependencyInjection` for `AddHealthChecks()` extension method. Build broke when compiling from Migrations project. Fixed.

**5. [Rule 1 — Namespace collision]** `YH.Modules.Project` namespace collides with `Project` entity type. Applied namespace alias pattern (`using ProjectEntity = YH.Modules.Project.Domain.Project`) matching Workspace module's pattern. Build failed initially without it.

**6. [Rule 1 — Cross-module cref]** XML doc cref to `Workspace.SoftDelete` cross-module method failed to resolve (Project module only depends on Workspace.Contracts, not Workspace runtime). Replaced with `<c>` tag.

**7. [Rule 1 — CA1027 analyzer]** `ProjectNetwork` enum with values 0 and 2 (skipping 1) triggered CA1027 "Mark enum with FlagsAttribute". Suppressed by adding CA1027 to NoWarn in csproj, with justification matching Plane's schema.

### Build Verification

```
dotnet build src/YH.Flow.slnx --nologo   → 0 errors, 0 warnings
```

### Migration Schema Review

The generated migration creates:

**Projects** table (`yhschema.Project`):

- All 30+ columns with correct types, max lengths, and defaults
- `IX_Projects_Slug` — unconditional unique (Slug epoch-modified on delete)
- `IX_Projects_Tenant_Identifier` — conditional unique, filter: `[DeletedOnUtc] IS NULL`
- `IX_Projects_Tenant_Name` — conditional unique, filter: `[DeletedOnUtc] IS NULL`
- `IX_Projects_ArchivedAt_SortOrder` — composite for active project listing
- `IX_Projects_Owner` — performance index on OwnerId

**ProjectMembers** table (`yhschema.Project`):

- 12 columns with correct types
- `IX_ProjectMembers_Tenant_Project_User` — unique, one membership per user per project
- `IX_ProjectMembers_Project_Active` — project member listing
- `IX_ProjectMembers_Tenant_User_Active` — user project membership query

Both entities annotated `Finbuckle:MultiTenant = true` (auto-applied by ApplyTenantIsolationByDefault).

### Known Stubs

- `ProjectModule.MapEndpoints` — route groups are scaffolded with TODO comments for 03-03 (project endpoints) and 03-04 (member endpoints). No actual endpoints registered yet.

## Self-Check: PASSED

All 10 files exist. All 5 commits verified. Build produces 0 errors, 0 warnings.
