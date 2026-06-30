---
phase: 06-module
reviewed: 2026-06-24T18:30:00Z
depth: standard
files_reviewed: 65
files_reviewed_list:
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/Constants/ModuleConstants.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/ModuleDto.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/ModuleProgressDto.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/ModuleLinkDto.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/CreateModule/CreateModuleCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/CreateModule/CreateModuleResponse.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/GetModule/GetModuleQuery.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/UpdateModule/UpdateModuleCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/DeleteModule/DeleteModuleCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ListModules/ListModulesQuery.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ArchiveModule/ArchiveModuleCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ArchiveModule/UnarchiveModuleCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/ArchiveModule/ListArchivedModulesQuery.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Issues/AddIssuesToModuleCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Issues/RemoveIssueFromModuleCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Issues/ListModuleIssuesQuery.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/GetModuleProgress/GetModuleProgressQuery.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Links/AddModuleLinkCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Links/RemoveModuleLinkCommand.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Modules/Links/ListModuleLinksQuery.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Domain/Module.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Domain/ModuleIssue.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Domain/ModuleMember.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Domain/ModuleLink.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleConfiguration.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleIssueConfiguration.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleMemberConfiguration.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleLinkConfiguration.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Data/WorkItemsDbContext.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ModuleDtoMapper.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleCommandValidator.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModule/GetModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModule/GetModuleQueryHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UpdateModule/UpdateModuleCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UpdateModule/UpdateModuleCommandValidator.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UpdateModule/UpdateModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/DeleteModule/DeleteModuleCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/DeleteModule/DeleteModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListModules/ListModulesEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListModules/ListModulesQueryHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ArchiveModule/ArchiveModuleCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ArchiveModule/ArchiveModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UnarchiveModule/UnarchiveModuleCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UnarchiveModule/UnarchiveModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListArchivedModules/ListArchivedModulesEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/ListArchivedModules/ListArchivedModulesQueryHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/AddIssuesToModule/AddIssuesToModuleCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/AddIssuesToModule/AddIssuesToModuleCommandValidator.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/AddIssuesToModule/AddIssuesToModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/RemoveIssueFromModule/RemoveIssueFromModuleCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/RemoveIssueFromModule/RemoveIssueFromModuleEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/ListModuleIssues/ListModuleIssuesQueryHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Issues/ListModuleIssues/ListModuleIssuesEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModuleProgress/GetModuleProgressQueryHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/GetModuleProgress/GetModuleProgressEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/AddModuleLink/AddModuleLinkCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/AddModuleLink/AddModuleLinkCommandValidator.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/AddModuleLink/AddModuleLinkEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/RemoveModuleLink/RemoveModuleLinkCommandHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/RemoveModuleLink/RemoveModuleLinkEndpoint.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/ListModuleLinks/ListModuleLinksQueryHandler.cs
  - yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/Links/ListModuleLinks/ListModuleLinksEndpoint.cs
  - yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/20260624095427_AddModules.cs
  - yh-flow/src/Tests/WorkItems.Tests/TestData/TestModuleFactory.cs
  - yh-flow/src/Tests/WorkItems.Tests/Domain/ModuleDomainTests.cs
  - yh-flow/src/Tests/WorkItems.Tests/Domain/ModuleIssueDomainTests.cs
  - yh-flow/src/Tests/WorkItems.Tests/Domain/ModuleMemberDomainTests.cs
  - yh-flow/src/Tests/WorkItems.Tests/Domain/ModuleLinkDomainTests.cs
  - yh-flow/src/Tests/WorkItems.Tests/Integration/ModuleCrudTests.cs
  - yh-flow/src/Tests/WorkItems.Tests/Integration/ModuleIssueAndProgressTests.cs
findings:
  critical: 1
  warning: 2
  info: 2
  total: 5
status: issues_found
---

# Phase 6: Module Code Review Report

**Reviewed:** 2026-06-24T18:30:00Z
**Depth:** standard
**Files Reviewed:** 65
**Status:** issues_found

## Summary

Reviewed 65 source files for the Module vertical slice implementation (domain entities, EF configurations, contracts, 15 API endpoints, 29 domain tests, 17 integration tests). The implementation follows the established Cycle pattern closely with correct tenant isolation (IHasTenant), soft-delete (ISoftDeletable), and vertical slice conventions.

One BLOCKER issue found: the ModuleLink Metadata column uses a SQL Server-specific type (`nvarchar(max)`) in both the EF configuration and the PostgreSQL migration, which will cause migration failure on PostgreSQL. Two WARNING findings: a hardcoded status string literal that should reference the constant, and missing status value domain validation.

## Critical Issues

### CR-01: ModuleLink Metadata column uses SQL Server-specific `nvarchar(max)` in PostgreSQL migration

**File:** `yh-flow/src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/ModuleLinkConfiguration.cs:35`
**Also:** `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/20260624095427_AddModules.cs:41`
**Also:** `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/20260624095427_AddModules.Designer.cs:978`
**Also:** `yh-flow/src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/WorkItemsDbContextModelSnapshot.cs:975`

**Issue:** The `ModuleLink.Metadata` property is configured with `.HasColumnType("nvarchar(max)")` in the EF configuration, and the generated migration uses `type: "nvarchar(max)"`. This is a SQL Server-specific type that is not valid for PostgreSQL via Npgsql. All other similar string fields in the project (e.g., `ProgressSnapshot`, `LogoProps`) omit explicit `HasColumnType()` and let EF Core default to the provider-correct type. Applying this migration against a PostgreSQL database will fail with a type error.

No other entity in the WorkItems module uses `nvarchar` column types -- this is a unique deviation. The existing Cycle entity's ProgressSnapshot and LogoProps fields (same pattern as ModuleLink.Metadata) have no `HasColumnType` annotation and work correctly on PostgreSQL.

**Fix:** Remove the `.HasColumnType("nvarchar(max)")` call from `ModuleLinkConfiguration.cs` line 35. Then regenerate the migration to produce provider-agnostic DDL. If an explicit type is desired for clarity, use `.HasColumnType("text")` which is PostgreSQL-compatible.

```diff
 // ModuleLinkConfiguration.cs line 35
 builder.Property(x => x.Metadata)
-    .HasColumnType("nvarchar(max)");
+    /* omit — let Npgsql default to text */;
```

After fixing the configuration, regenerate the `AddModules` migration:

```bash
dotnet ef migrations add RemoveNvarcharMax --project src/Host/YH.Flow.Migrations.PostgreSQL --startup-project src/Host/YH.Flow.Api --context WorkItemsDbContext
```

Or edit the existing migration to replace `type: "nvarchar(max)"` with `type: "text"` on the Metadata column, and update the Designer/snapshot files accordingly.

## Warnings

### WR-01: Hardcoded default status string literal instead of constant reference

**File:** `yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleCommandHandler.cs:32`

**Issue:** The handler passes `command.Status ?? "planned"` to `Module.Create()`. The domain entity factory already defaults status to `ModuleConstants.DefaultStatus` (which is `"planned"`). The handler duplicates the literal `"planned"` instead of referencing `ModuleConstants.DefaultStatus`. If the constant changes in the future, the handler would continue passing the old value, creating an inconsistency where the API layer always provides `"planned"` while the domain layer's default moves to the new value.

**Fix:** Reference the constant directly instead of the magic string.

```diff
 status: command.Status ?? "planned",
+status: command.Status ?? ModuleConstants.DefaultStatus,
```

### WR-02: No status value validation in domain entity or FluentValidation validators

**File:** `yh-flow/src/Modules/WorkItems/Modules.WorkItems/Domain/Module.cs` (all status setter paths)
**File:** `yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/CreateModule/CreateModuleCommandValidator.cs`
**File:** `yh-flow/src/Modules/WorkItems/Modules.WorkItems/Features/v1/Modules/UpdateModule/UpdateModuleCommandValidator.cs`

**Issue:** The `Module.Update()` and `Module.UpdateStatus()` methods accept any arbitrary string for the status parameter. Neither the FluentValidation validators nor the domain entity validate that the status is one of the six allowed values: `"backlog"`, `"planned"`, `"in-progress"`, `"paused"`, `"completed"`, `"cancelled"`. A client can set `status: "invalid-value"` and it will be stored silently, potentially breaking downstream consumers that expect specific enum values.

The context document (06-CONTEXT.md) defines the valid status as "string enum values" -- the entity should enforce this constraint as a defense-in-depth measure, even though the API layer should also validate.

**Fix:** Add status validation in the domain entity's state-setting paths and/or add a FluentValidation `Must` rule.

Option A -- Domain entity validation (defense-in-depth):

```csharp
private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
{
    "backlog", "planned", "in-progress", "paused", "completed", "cancelled"
};

// In Update() and UpdateStatus():
if (status is not null && !ValidStatuses.Contains(status))
    throw new ArgumentException($"Invalid status '{status}'. Valid values: {string.Join(", ", ValidStatuses)}", nameof(status));
```

Option B -- FluentValidation rule for both Create and Update validators:

```csharp
RuleFor(x => x.Status)
    .Must(s => s is null || new[] { "backlog", "planned", "in-progress", "paused", "completed", "cancelled" }.Contains(s))
    .WithMessage("Status must be one of: backlog, planned, in-progress, paused, completed, cancelled.")
    .When(x => x.Status is not null);
```

## Info

### IN-01: Unused `Module.UpdateStatus()` method

**File:** `yh-flow/src/Modules/WorkItems/Modules.WorkItems/Domain/Module.cs:159`

**Issue:** The `UpdateStatus(string?)` method is defined and tested but never called from any handler. The `UpdateModuleCommandHandler` calls `module.Update(...)` with the status parameter, bypassing `UpdateStatus()`. The method is dead code outside of the test suite.

**Fix:** Either remove the method if it has no planned usage, or add a dedicated endpoint/handler path that calls it directly (e.g., a `PATCH /modules/{moduleId}/status` endpoint). If deferred, document it explicitly as a planned extension point.

### IN-02: Unused `Module.FreezeSnapshot()` method

**File:** `yh-flow/src/Modules/WorkItems/Modules.WorkItems/Domain/Module.cs:189`

**Issue:** The `FreezeSnapshot(string)` method is defined on the entity but never called from any module handler. It mirrors the Cycle entity's `FreezeSnapshot()` method, which is called during `TransferCycleIssues`. The Module equivalent (`TransferIssuesBetweenModules`) is deferred to a later iteration (noted in CONTEXT.md). While acceptable as future scaffolding, the dead code should be documented with a reference to the deferred feature or removed until needed.

**Fix:** Add a `/// <remarks>` XML comment noting this method is reserved for the deferred `TransferIssuesBetweenModules` feature, or remove it until the feature is implemented.

---

_Reviewed: 2026-06-24T18:30:00Z_
_Reviewer: Claude (gsd-code-reviewer)_
_Depth: standard_
