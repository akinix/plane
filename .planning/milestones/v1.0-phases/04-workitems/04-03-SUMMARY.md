---
phase: 04-workitems
plan: 03
subsystem: WorkItems
tags: [estimate, issue, issue-link, crud, migration, ef-core]
requires: [04-02]
provides: [Estimate CRUD, Issue CRUD, IssueLink CRUD, AddIssues migration]
affects: [WorkItemsModule, WorkItemsDbContext, Migrations.PostgreSQL]
tech-stack:
  added: []
  patterns: [Vertical Slice, CQRS, Minimal API, PlanePagedResult, M2M full replacement, closed-state semanics]
key-files:
  created:
    - src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Estimates/
    - src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/EstimatePoints/
    - src/Modules/WorkItems/Modules.WorkItems/Features/v1/Estimates/
    - src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/Issues/
    - src/Modules/WorkItems/Modules.WorkItems/Features/v1/Issues/
    - src/Modules/WorkItems/Modules.WorkItems/Domain/IssueLink.cs
    - src/Modules/WorkItems/Modules.WorkItems/Domain/LinkType.cs
    - src/Modules/WorkItems/Modules.WorkItems/Data/Configurations/IssueLinkConfiguration.cs
    - src/Modules/WorkItems/Modules.WorkItems.Contracts/DTOs/IssueLinkDto.cs
    - src/Modules/WorkItems/Modules.WorkItems.Contracts/v1/IssueLinks/
    - src/Modules/WorkItems/Modules.WorkItems/Features/v1/IssueLinks/
    - src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/20260624071116_AddIssues.cs
    - src/Host/YH.Flow.Migrations.PostgreSQL/WorkItems/20260624071116_AddIssues.Designer.cs
  modified:
    - src/Modules/WorkItems/Modules.WorkItems/WorkItemsModule.cs
    - src/Modules/WorkItems/Modules.WorkItems/Data/WorkItemsDbContext.cs
decisions:
  - Estimate hard-delete (per Plane pattern) — not soft-deletable, cascade to EstimatePoints
  - EstimatePoint delete checks Issue references (409 Conflict)
  - IssueList endpoint uses query-string filters with PlanePagedResult pagination
  - IssueLink is a combined external-link + internal-relation entity per CONTEXT decision
  - IssueLink is NOT soft-deletable (hard-delete per Plane pattern)
metrics:
  duration: ~45min
  files_created: 57
  files_modified: 2
  commits: 4
  completed_date: 2026-06-24
---

# Phase 4 Plan 3: Estimate CRUD, Issue CRUD, IssueLink + Migration Summary

Completed Wave 3 of Phase 4 WorkItems module — Estimate system CRUD (with EstimatePoint sub-CRUD), Issue CRUD with multidimensional filtering, IssueLink entity with CRUD, and EF migration adding all Issue-related tables.

## Completed Tasks

### Task 1: Estimate + EstimatePoint CRUD (commit 912555dd9)

Created 28 files implementing full Estimate and EstimatePoint CRUD:

- **Contracts:** CreateEstimate/CreateEstimateResponse, GetEstimate, UpdateEstimate, DeleteEstimate, ListEstimates
- **Contracts:** CreateEstimatePoint, UpdateEstimatePoint, DeleteEstimatePoint
- **Implementation:** EstimateDtoMapper, 5 Estimate endpoints/handlers, 3 EstimatePoint endpoints/handlers
- **CreateEstimate:** Supports optional initial points seeding via InitialPoints list
- **GetEstimate:** AsNoTracking with Include(EstimatePoints.OrderBy(Key))
- **UpdateEstimate:** PATCH semantics for Name/Type/IsLastUsed
- **DeleteEstimate:** Hard-delete per Plane pattern (not ISoftDeletable)
- **DeleteEstimatePoint:** Checks Issue.EstimatePointId references before deletion (409 Conflict)
- **EstimatePoint:** Unique index (EstimateId, Key) enforced at DB level
- **WorkItemsModule:** Wired estimates route group under `/workspaces/{slug}/projects/{projectId}/estimates/`

### Task 2: Issue CRUD with multidimensional filtering (commit 8355999e1)

Created 21 files implementing full Issue CRUD:

- **Contracts:** CreateIssue/CreateIssueResponse, GetIssue, UpdateIssue, DeleteIssue, ListIssues
- **CreateIssueHandler:** Auto-assigns SequenceId via IssueSequenceService with SERIALIZABLE isolation, resolves default state (IsDefault=true), creates M2M assignees/labels
- **GetIssueHandler:** AsNoTracking with Include(assignees).Include(labels), resolves state name/group
- **UpdateIssueHandler:** Closed-state semantics (Completed/Cancelled blocked with 400), CompletedAt sync via Issue.UpdateState, M2M full replacement for assignees/labels
- **DeleteIssueHandler:** Soft-delete with cascade
- **ListIssuesHandler:** Multidimensional filters (StateId, Priority, AssigneeId via .Any, LabelId via .Any, ParentId), PlanePagedResult pagination, ordering by created_at/updated_at/sort_order/priority/sequence_id
- **Validators:** CreateIssueCommandValidator, UpdateIssueCommandValidator, DeleteIssueCommandValidator, ListIssuesQueryValidator
- **WorkItemsModule:** Wired work-items route group with all 5 Issue endpoints + IssueLink endpoints

### Task 3: IssueLink entity + CRUD + EF migration (commits 36ef39da3, 04715dd1e)

Created 16 files + generated migration:

- **LinkType enum:** RelatesTo=0, Duplicate=1, Blocks=2, BlockedBy=3
- **IssueLink entity:** Combined external-link + internal-relation pattern per CONTEXT. Fields: Id, IssueId, RelatedIssueId?, Url?, Title?, LinkType, Metadata?. NOT soft-deletable (hard-delete). IHasTenant + IAuditableEntity.
- **IssueLinkConfiguration:** ToTable "IssueLinks" in yhschema.WorkItems. Cascade delete from Issue. Indexes: (IssueId, LinkType), (RelatedIssueId).
- **IssueLinkDto:** Contract DTO with all fields
- **CRUD:** CreateIssueLink validates at least one target, cross-project RelatedIssueId validation (T-4-issue-crud-06). Delete is hard-delete. List is AsNoTracking.
- **EF Migration AddIssues:** Creates Issues, IssueAssignees, IssueLabels, IssueLinks tables with correct indexes, cascades, and unique constraints

## Endpoints Summary

| Method | Route                                     | Handler             | Auth                 |
| ------ | ----------------------------------------- | ------------------- | -------------------- |
| POST   | /estimates/                               | CreateEstimate      | Admin, Member        |
| GET    | /estimates/                               | ListEstimates       | Authorized           |
| GET    | /estimates/{estimateId}/                  | GetEstimate         | Authorized           |
| PATCH  | /estimates/{estimateId}/                  | UpdateEstimate      | Admin                |
| DELETE | /estimates/{estimateId}/                  | DeleteEstimate      | Admin                |
| POST   | /estimates/{estimateId}/points/           | CreateEstimatePoint | Admin, Member        |
| PATCH  | /estimates/{estimateId}/points/{pointId}/ | UpdateEstimatePoint | Admin                |
| DELETE | /estimates/{estimateId}/points/{pointId}/ | DeleteEstimatePoint | Admin                |
| POST   | /work-items/                              | CreateIssue         | Admin, Member, Guest |
| GET    | /work-items/                              | ListIssues          | Authorized           |
| GET    | /work-items/{issueId}/                    | GetIssue            | Authorized           |
| PATCH  | /work-items/{issueId}/                    | UpdateIssue         | Admin, Member        |
| DELETE | /work-items/{issueId}/                    | DeleteIssue         | Admin, Member        |
| POST   | /work-items/{issueId}/links/              | CreateIssueLink     | Admin, Member        |
| GET    | /work-items/{issueId}/links/              | ListIssueLinks      | Authorized           |
| DELETE | /work-items/{issueId}/links/{linkId}/     | DeleteIssueLink     | Admin                |

## Deviations from Plan

None — plan executed exactly as written.

## Threat Surface Scan

No new threat surface beyond what was declared in the plan's threat_model. All T-4-\* threat mitigations are implemented: closed-state edit check, CompletedAt sync, SequenceId serialization, M2M full replacement in same SaveChanges, cross-project RelatedIssueId validation, and EstimatePoint key uniqueness index.

## Build Verification

```
dotnet build src/YH.Flow.slnx --nologo — 0 errors, 0 warnings
```

## Self-Check: PASSED

All created/modified files exist. All commits are in git history.
