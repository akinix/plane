---
phase: 03-project
plan: 04
type: execute
subsystem: ProjectMember
tags: [project, members, api, contracts, batch-resolution, tenant-isolation]
requires: [03-02, 03-03]
provides: [ProjectMember endpoints, Member contracts]
affects: [ProjectModule, Modules.Project]
tech-stack:
  added: []
  patterns:
    - "Two-phase batch user resolution via IUserIdentityService (NFR-1)"
    - "Deactivate() pattern (IsActive=false) for member removal vs soft-delete"
    - "Last-Admin guard via DbContext count query"
    - "Route group nesting under /projects/{projectId}/members/"
key-files:
  created:
    - "src/Modules/Project/Modules.Project.Contracts/v1/Members/AddMember/AddMemberCommand.cs"
    - "src/Modules/Project/Modules.Project.Contracts/v1/Members/AddMember/AddMemberResponse.cs"
    - "src/Modules/Project/Modules.Project.Contracts/v1/Members/ListMembers/ListMembersQuery.cs"
    - "src/Modules/Project/Modules.Project.Contracts/v1/Members/UpdateMemberRole/UpdateMemberRoleCommand.cs"
    - "src/Modules/Project/Modules.Project.Contracts/v1/Members/RemoveMember/RemoveMemberCommand.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/AddMember/AddMemberEndpoint.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/AddMember/AddMemberCommandHandler.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/AddMember/AddMemberCommandValidator.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/ListMembers/ListMembersEndpoint.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/ListMembers/ListMembersQueryHandler.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/ListMembers/ListMembersQueryValidator.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleEndpoint.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/UpdateMemberRole/UpdateMemberRoleCommandValidator.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/RemoveMember/RemoveMemberEndpoint.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/RemoveMember/RemoveMemberCommandHandler.cs"
    - "src/Modules/Project/Modules.Project/Features/v1/Members/RemoveMember/RemoveMemberCommandValidator.cs"
  modified:
    - "src/Modules/Project/Modules.Project.Contracts/Modules.Project.Contracts.csproj"
    - "src/Modules/Project/Modules.Project/ProjectModule.cs"
decisions:
  - "Role uses int (not WorkspaceRole enum) in contracts for consistency with ProjectMember entity storage"
  - "UpdateMemberRoleCommand returns void (204 NoContent) instead of DTO — minimal contract per plan"
  - "AddMember handler relies on endpoint-level RequireWorkspaceRole(Admin) authz, plus duplicate detection at DB level"
  - "RemoveMember uses Deactivate() (IsActive=false) NOT SoftDelete — preserves row for audit trail"
  - "Added Workspace.Contracts reference to Project.Contracts csproj for WorkspaceRole enum usage"
metrics:
  duration: 00:08:42
  completed_date: 2026-06-24
---

# Phase 3 Plan 4: ProjectMember endpoints — Summary

**One-liner:** 4 ProjectMember API endpoints (Add/List/UpdateRole/Remove) with batch user resolution, Admin authorization guards, and tenant-isolated data access — completing Project module REQ-3.2.

## Tasks Completed

| #   | Task                | Files                                            | Commit      |
| --- | ------------------- | ------------------------------------------------ | ----------- |
| 1   | Create v1 Contracts | 5 contracts + 1 csproj edit                      | `a7cbc2d5b` |
| 2   | Implement endpoints | 12 handlers/validators/endpoints + 1 module edit | `9b3c5fe9a` |

### Task 1 — Contracts (5 files)

Created `Members` directory under `v1/` with 5 contract files following the Workspace member pattern:

- **AddMemberCommand** (`ICommand<AddMemberResponse>`): ProjectId/UserId/Role/CurrentUserId — Role defaults to Member (15) with `[JsonIgnore]` on server-set fields
- **AddMemberResponse** (record): Id, ProjectId, UserId, Role
- **ListMembersQuery** (`IQuery<PlanePagedResult<ProjectMemberDto>>`): ProjectId, PageNumber, PageSize, BaseUrl
- **UpdateMemberRoleCommand** (`ICommand` void): ProjectId, MemberId, Role
- **RemoveMemberCommand** (`ICommand` void): ProjectId, MemberId, CurrentUserId

Added `Modules.Workspace.Contracts` project reference to the Contracts csproj for `WorkspaceRole` enum usage.

### Task 2 — Implementation (12 files + ProjectModule.cs wiring)

**AddMember** (POST `/workspaces/{slug}/projects/{projectId}/members/`):

- Endpoint: `.RequireWorkspaceRole(Admin)`, binds projectId from route, returns 201
- Handler: duplicate detection (409 on existing active member), creates via `ProjectMember.Create()`
- Validator: UserId non-empty, Role must be 5/15/20

**ListMembers** (GET `/workspaces/{slug}/projects/{projectId}/members/`):

- Endpoint: `.RequireWorkspaceRole(Member, Admin)`, pagination from query params, returns 200
- Handler: Three-phase batch resolution:
  - Phase 1: Page membership rows by ProjectId (single SQL, AsNoTracking)
  - Phase 2: Batch-resolve users via `IUserIdentityService.GetUsersByIdsAsync()` (single call, NFR-1)
  - Phase 3: Zip members with UserSummary into `ProjectMemberDto` items
- Validator: PageNumber > 0, PageSize 1-100

**UpdateMemberRole** (PATCH `/workspaces/{slug}/projects/{projectId}/members/{memberId}/`):

- Endpoint: `.RequireWorkspaceRole(Admin)`, binds projectId + memberId from route, returns 204
- Handler: Fetches member by Id, calls `UpdateRole()`, saves
- Validator: MemberId required, Role must be 5/15/20

**RemoveMember** (DELETE `/workspaces/{slug}/projects/{projectId}/members/{memberId}/`):

- Endpoint: `.RequireWorkspaceRole(Admin)`, binds projectId + memberId from route, returns 204
- Handler: Last-Admin guard (409 if target is the only Admin), calls `Deactivate()` (IsActive=false, NOT soft delete)
- Validator: MemberId required

**ProjectModule.cs:** Replaced TODO block with live member route group wiring — all 4 `Map*MemberEndpoint()` calls registered.

## Verification Results

| Check                                               | Status                                             |
| --------------------------------------------------- | -------------------------------------------------- |
| `dotnet build src/YH.Flow.slnx --nologo`            | 0 errors, 0 warnings                               |
| `dotnet test src/Tests/Project.Tests --nologo`      | 0 tests (expected, tests added in later waves)     |
| All 4 Map\*MemberEndpoint calls in ProjectModule.cs | Present                                            |
| Plane-compatible route structure                    | `/workspaces/{slug}/projects/{projectId}/members/` |

## Deviations from Plan

None — plan executed exactly as written.

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed XML comment typo in RemoveMemberCommand.cs**

- **Found during:** Task 1 build verification
- **Issue:** `</param>` closing tag instead of `</summary>` in XML doc — caused CS1570 build error
- **Fix:** Replaced `</param>` with `</summary>`
- **Files modified:** `RemoveMemberCommand.cs`
- **Commit:** Included in `a7cbc2d5b`

**2. [Rule 2 - Dependency] Added Workspace.Contracts reference to Project.Contracts**

- **Found during:** Task 1 build verification
- **Issue:** `AddMemberCommand.cs` uses `WorkspaceRole.Member` for default Role value, but Contracts project had no reference to `Modules.Workspace.Contracts` — caused CS0234
- **Fix:** Added `<ProjectReference>` to `Modules.Workspace.Contracts` in `Modules.Project.Contracts.csproj`
- **Files modified:** `Modules.Project.Contracts.csproj`
- **Commit:** Included in `a7cbc2d5b`

## Known Stubs

None.

## Threat Flags

None. All member endpoints follow the threat model:

- T-3-member-01 (EoP AddMember): mitigated by endpoint `.RequireWorkspaceRole(Admin)` + duplicate detection
- T-3-member-02 (EoP UpdateMemberRole): mitigated by `.RequireWorkspaceRole(Admin)`
- T-3-member-03 (EoP RemoveMember): mitigated by `.RequireWorkspaceRole(Admin)` + last-Admin guard
- T-3-member-04 (Info Disclosure ListMembers): mitigated by `.RequireWorkspaceRole(Member, Admin)`
- T-3-member-05 (User detail exposure): mitigated by `UserSummary` DTO (4 fields only)
- T-3-member-06 (Duplicate membership): mitigated by handler duplicate check
- T-3-member-07 (Last Admin removal): mitigated by last-Admin guard (conflict 409)
- T-3-member-08 (Cross-tenant leak): mitigated by `ProjectDbContext` auto tenant filter

## Self-Check: PASSED

- [x] All created files verified on disk
- [x] Both commits exist in git log
- [x] Solution builds with 0 errors
- [x] All 4 Map\*MemberEndpoint calls in ProjectModule.cs
- [x] Contrail: route structure matches Plane convention
