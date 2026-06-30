---
status: partial
phase: 02-workspace
source: [02-VERIFICATION.md, 02-REVIEW.md]
started: 2026-06-18T12:15:00Z
updated: 2026-06-18T12:15:00Z
---

# Phase 02 — Workspace: Manual Smoke (Human Verification)

All automated checks PASS (build 0/0; Workspace.Tests 96/96; Identity.Tests 412/412; Architecture.Tests 0 new Workspace violations; requirements REQ-2.1~2.4/NFR-1~4 accounted for per VERIFICATION.md).

**11 steps require human testing against a live Aspire stack + real JWT.** This is the phase's final gate. Automated InMemory tests cannot cover real auth + Finbuckle tenant filters + PostgreSQL.

> ⚠ **Code review (02-REVIEW.md) flagged 3 CRITICAL tenant-scoping defects.** Steps marked 🟥 directly validate them against the real stack — their pass/fail is the arbiter for whether CR-01/02/03 are real bugs.

**Prereq:** Aspire stack running (`cd yh-flow/src/Host/YH.Flow.AppHost && dotnet run`, wait for PostgreSQL + Redis + API ready) + a real root user JWT (via Phase 1 `/auth/sign-in`).

## Current Test

[awaiting human testing]

## Tests

### 1. Create workspace (D-06 auto-Admin) 🟥 CR-02

expected: `POST /api/v1/workspaces/` body `{"name":"Acme Smoke Test"}` → 201 `{"id","slug":"acme-smoke-test"}` + auto Admin member.
result: [pending]

### 2. Get workspace

expected: `GET /api/v1/workspaces/acme-smoke-test/` → 200 full WorkspaceDto.
result: [pending]

### 3. Slug-check restricted word (D-09)

expected: `POST /api/v1/workspaces/slug-check/` body `{"slug":"api"}` → 400 or Exists=false.
result: [pending]

### 4. Create invitation (D-10/D-12)

expected: `POST /api/v1/workspaces/acme-smoke-test/invitations/` body `{"email":"invitee@example.com","role":15}` → 201 raw token + slug.
result: [pending]

### 5. Accept invitation (invitee JWT) 🟥 CR-01 — THE DECIDER

expected: `POST /api/v1/workspaces/invitations/{token}/accept/` → 200 `{memberId,workspaceId}`.
**If this returns 404, CR-01 is confirmed (accept-invitation tenant-scoping bug).**
result: [pending]

### 6. List members (D-05 batch)

expected: `GET /api/v1/workspaces/acme-smoke-test/members/` → 200, 2 members (root Admin + invitee Member), each with `user:{id,display_name,email,avatar_url}`.
result: [pending]

### 7. EOP self-promotion guard (T-2-eop-self) 🟥

expected: invitee `PATCH .../members/{inviteeMemberId}/` body `{"role":20}` → **403** (Member cannot self-promote).
result: [pending]

### 8. Admin promotes other

expected: root `PATCH .../members/{inviteeMemberId}/` body `{"role":20}` → 200.
result: [pending]

### 9. Soft-delete workspace (D-08 epoch)

expected: root `DELETE /api/v1/workspaces/acme-smoke-test/` → 204 or 200.
result: [pending]

### 10. Slug release → reuse (D-08 load-bearing) 🟥

expected: root `POST /api/v1/workspaces/` body `{"name":"Acme Smoke Test","slug":"acme-smoke-test"}` → **201** (slug reusable after soft-delete).
result: [pending]

### 11. Cross-workspace isolation (D-02) 🟥 CR-cluster

expected: invitee `GET /api/v1/workspaces/another-workspace/members/` (a workspace the invitee is NOT a member of) → **403**.
result: [pending]

## Summary

total: 11
passed: 0
issues: 0
pending: 11
skipped: 0
blocked: 0

## Gaps

[none yet — populate from smoke results; any 🟥 failure → route to /gsd-plan-phase 02 --gaps for CR-01/02/03 fix]
