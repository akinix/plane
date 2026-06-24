---
phase: 04
plan: 04
subsystem: WorkItems
tags:
  - comments
  - activities
  - batch
  - intake
  - import
  - export
  - state-seed
requires:
  - 04-03 (Issue CRUD + IssueLink)
provides:
  - IssueComment CRUD
  - IssueActivity domain events + list
  - BulkUpdateIssues
  - IntakeIssue CRUD
  - Import/Export (CSV + JSON)
  - StateSeeder (5 default states)
  - EF migration AddCommentsActivitiesIntake
affects:
  - Data/WorkItemsDbContext (IssueComments, IssueActivities DbSets)
  - Domain/Issue (domain event emissions)
  - WorkItemsModule (18 endpoints registered)
tech-stack:
  added: []
  patterns:
    - Vertical Slice per feature folder
    - Mediator (source-gen) ICommand/IQuery pipelines
    - Minimal API RouteHandlerBuilder extensions
    - Domain events via IDomainEvent + INotificationHandler
    - Formula injection protection in CSV export
    - Manual CSV parser with quoted field support
    - Per-row transactional import with error reporting
key-files:
  created:
    - Domain/IssueComment.cs
    - Domain/IssueActivity.cs
    - Domain/Events/IssueUpdatedDomainEvent.cs
    - Domain/Events/IssueActivityHandler.cs
    - Data/Configurations/IssueCommentConfiguration.cs
    - Data/Configurations/IssueActivityConfiguration.cs
    - Data/Configurations/IntakeIssueConfiguration.cs
    - Domain/IntakeIssueStatus.cs
    - Domain/IntakeIssue.cs
    - Contracts/DTOs/IssueCommentDto.cs
    - Contracts/DTOs/IssueActivityDto.cs
    - Contracts/DTOs/ImportExportDto.cs
    - Features/v1/Issues/BulkUpdateIssues/*
    - Features/v1/IssueComments/*
    - Features/v1/IssueActivities/*
    - Features/v1/Intake/*
    - Features/v1/ImportExport/ExportIssues/*
    - Features/v1/ImportExport/ImportIssues/*
    - Data/Seeders/StateSeeder.cs
  modified:
    - Domain/Issue.cs (domain event emissions)
    - Data/WorkItemsDbContext.cs (IssueComments, IssueActivities DbSets)
    - WorkItemsModule.cs (18 endpoints wired)
    - Data/Configurations/IssueConfiguration.cs (navigation configs)
decisions:
  - IssueActivity does NOT implement IHasDomainEvents (recursion prevention)
  - IntakeIssue does NOT extend ISoftDeletable (intake records persist)
  - IntakeIssue transitions validated: only Pending -> Accepted/Rejected/Snoozed/Duplicate
  - CSV export includes formula injection protection (=/+/-/@ prefix with single quote)
  - CSV import uses manual parser (CsvHelper unavailable)
  - JsonSerializerOptions cached as static readonly (CA1869 suppression)
  - Priority normalization uses explicit switch with uppercase comparison (CA1308)
metrics:
  duration: continued session
  completed_date: 2026-06-24
---

# Phase 04 Plan 04: Wave 4 — Comments, Activities, Batch, Intake, Import/Export, State Seed

**One-liner:** IssueComment CRUD, IssueActivity domain events + list, BulkUpdateIssues, IntakeIssue CRUD with Pending-only transitions, CSV/JSON Import/Export with per-row error reporting, state seed data, and EF Core migration.

## Task Execution

### Task 1: IssueComment CRUD + IssueActivity domain events + Batch operations

**Status:** Complete

**Created:**

- `Domain/IssueComment.cs` — `IHasTenant`, `ISoftDeletable`, `IAuditableEntity`. Fields: Id, IssueId, CommentHtml(10000), CommentJson?, CommentStripped?, ActorId(450), ParentId?, EditedAt?. Factory Create(), Update() sets EditedAt, SoftDelete(). Static SanitizeHtml/StripHtml.
- `Domain/IssueActivity.cs` — `IHasTenant`, `IAuditableEntity` (NOT `IHasDomainEvents`). Fields: Id, IssueId, Verb(20), Field?(100), OldValue?, NewValue?, Comment?, ActorId(450), IssueCommentId?, Epoch(long). Factory Create().
- `Domain/Events/IssueUpdatedDomainEvent.cs` — `IDomainEvent` (extends `INotification`). Fields: IssueId(Guid), Changes(IReadOnlyList<Issue.FieldChange>), ActorId.
- `Domain/Events/IssueActivityHandler.cs` — `INotificationHandler<IssueUpdatedDomainEvent>`. Creates IssueActivity per FieldChange, saves to DbContext.
- 4 IssueComment endpoints (Create/List/Update/Delete) + 1 IssueActivity list endpoint + 1 BulkUpdateIssues endpoint
- Issue DTOs, command/validator files, EF configurations

**Modified:** `Domain/Issue.cs` (domain event emissions in UpdateDetails/UpdateState), `Data/WorkItemsDbContext.cs` (IssueComments, IssueActivities DbSets)

**Done criteria:** Module builds 0 errors.

### Task 2: IntakeIssue CRUD

**Status:** Complete

**Created:**

- `Domain/IntakeIssueStatus.cs` — enum: Pending=-2, Rejected=-1, Snoozed=0, Accepted=1, Duplicate=2
- `Domain/IntakeIssue.cs` — `IHasTenant`, `IAuditableEntity`. Two-step creation (Issue with IsDraft=true + IntakeIssue). Status transition validation (Pending only).
- 3 Intake endpoints (Create/List/Update) with WorkspaceRole(Admin, Member) for elevation

**Done criteria:** Module builds 0 errors.

### Task 3: Import/Export + StateSeeder + Migration

**Status:** Complete

**Created:**

- `Data/Seeders/StateSeeder.cs` — 5 default states: Backlog(Todo/In Progress/Done/Cancelled) per project. Idempotent seed checks Name+ProjectId.
- `Contracts/DTOs/ImportExportDto.cs` — ExportResultDto, ImportResultDto, IssueImportRowDto
- ExportIssues command/handler/endpoint — CSV/JSON with formula injection protection
- ImportIssues command/handler/validator/endpoint — Manual CSV parser with quoted field support, per-row validation, transactional batch
- `Features/v1/ImportExport/ExportIssues/ExportIssuesCommandHandler.cs`
- `Features/v1/ImportExport/ImportIssues/ImportIssuesCommandHandler.cs`

**Modified:** `WorkItemsModule.cs` (wired MapExportIssuesEndpoint, MapImportIssuesEndpoint)

**Migration:** `dotnet ef migrations add AddCommentsActivitiesIntake` — creates IssueComments, IssueActivities, IntakeIssues tables

**Done criteria:** Full solution builds 0 warnings, 0 errors.

## Full Solution Build

```
Build succeeded.
    0 warnings
    0 errors
```

## Deviations

**CA1308/Codestyle fixes (Rules 1 + 2):** Multiple code analysis warnings fixed:

- `JsonSerializerOptions` cached as `private static readonly` (CA1869)
- `ToLowerInvariant()` replaced with `ToUpperInvariant()` switch case explicit mapping (CA1308)
- `string.Contains(char)` replaced with `string.Contains(char, StringComparison.Ordinal)` (CA1307)
- `string.Replace(string, string)` replaced with `string.Replace(string, string, StringComparison.Ordinal)` (CA1307)
- CSV parser for-loop with inner `i++` restructured as while-loop (S127)

## Commits

| Commit      | Message                                                                         |
| ----------- | ------------------------------------------------------------------------------- |
| `171e68155` | feat(04-04): IssueComment CRUD + IssueActivity domain events + Batch operations |
| `ce774dc00` | feat(04-04): IntakeIssue CRUD — Create/List/Update with status transitions      |
| `7720413ba` | feat(04-04): Import/Export + StateSeeder — Task 3 of Wave 4                     |
| (pending)   | Migration + SUMMARY (docs commit)                                               |

## Plan Completion

All 3 tasks complete. Full solution builds 0 warnings, 0 errors. SUMMARY.md self-check verified.

## Self-Check: PASSED

All 7 key files verified on disk. All 3 commits confirmed in git log. Migration files generated. Full solution builds 0 errors.
