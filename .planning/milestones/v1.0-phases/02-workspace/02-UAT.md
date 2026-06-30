---
status: partial
phase: 02-workspace
source:
  [
    02-HUMAN-UAT.md,
    02-08-SUMMARY.md,
    02-07-SUMMARY.md,
    02-06-SUMMARY.md,
    02-04-SUMMARY.md,
    02-05-SUMMARY.md,
    02-01-SUMMARY.md,
    02-02-SUMMARY.md,
    02-03-SUMMARY.md,
  ]
started: 2026-06-18T12:15:00Z
updated: 2026-06-22T07:20:41Z
---

# Phase 02 — Workspace: Live UAT (11-step Aspire smoke)

Consolidates `02-HUMAN-UAT.md` (the phase's curated final-gate smoke) + the CR-02
supplementary cross-tenant list check from `smoke-workspace.py`. Automated gates are
fully GREEN (build 0/0; Workspace.Tests 100/100 = 96 InMemory + 4 real-PG relational;
Identity.Tests 412/412). **These 12 checks are the only unmet gate** — they require a
live Aspire stack + real JWTs, which the InMemory/relational test suite structurally
cannot exercise end-to-end (full HTTP pipeline + Finbuckle slug strategy + Aspire wiring).

## Current Test

[testing paused — ALL 12 checks blocked by `server`]

**Environmental blocker (root-caused 2026-06-22):** the live Aspire stack cannot be
stood up. `dotnet run` (default https profile) from `yh-flow/src/Host/YH.Flow.AppHost`
freezes at `Aspire.Hosting.DistributedApplication: Distributed application starting.`
and creates **ZERO containers** for 3+ minutes. DCP (`dcp.exe`, PID spawned at boot) is
deadlocked — **CPU delta = 0.000s** over a 1.5s sample.

**Root cause:** Docker Desktop is **NOT running**. There is no `Docker Desktop` /
`com.docker` / `DockerBackend` / `vpnkit` process. A `podman-machine-default` WSL distro
IS running and provides `//./pipe/docker_engine` (so `docker run hello-world` succeeds —
that is **Podman**, not Docker Desktop), but Aspire/DCP hard-targets Docker Desktop's
`//./pipe/dockerDesktopLinuxEngine` pipe, which **does not exist**:

```
docker --context desktop-linux info
  → error: open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified.
docker --context default info       → ServerVersion=5.8.2  (Podman-backed; CLI-only, ignored by DCP)
```

**Fix:** start the **Docker Desktop application** (not the podman machine). Once
`dockerDesktopLinuxEngine` returns, re-run `dotnet run` from the AppHost, find the
dynamic API port (probe `/health/ready` on listening localhost ports), then
`python .planning/phases/02-workspace/smoke-workspace.py http://localhost:<APIport>`.

> This is the **same** blocker recorded in the prior session
> (`phase02-uat-smoke-handoff` memory) — clean Docker container/volume state did NOT
> clear it, because the missing piece is the Docker Desktop _application_ itself, not
> stale containers. None of these 12 are code issues; all are `blocked_by: server`.

## Tests

### 1. Create workspace (D-06 auto-Admin) 🟥 CR-02

expected: `POST /api/v1/workspaces/` body `{"name":"Acme Smoke Test"}` with `tenant: root` → 201 `{"id","slug":"acme-smoke-test"}` + auto Admin member for creator.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running; DCP deadlocked on missing dockerDesktopLinuxEngine pipe). Not a code issue."

### 2. Get workspace

expected: `GET /api/v1/workspaces/acme-smoke-test` (drop tenant header; WorkspaceSlugStrategy resolves from path) → 200 full WorkspaceDto with name/slug.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 3. Slug-check restricted word (D-09)

expected: `POST /api/v1/workspaces/slug-check` body `{"slug":"api"}` → 400 (rejected) OR 200 `{exists:false}`.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 4. Create invitation (D-10/D-12)

expected: `POST /api/v1/workspaces/acme-smoke-test/invitations/` body `{"email":"invitee@example.com","role":15}` → 201 `{invitationId, token, slug}`.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 5. Accept invitation (invitee JWT) 🟥 CR-01 — THE DECIDER

expected: `POST /api/v1/workspaces/invitations/{token}/accept/` (trailing slash) with `tenant: root` → 200 `{memberId, workspaceId}`. **404 here = CR-01 NOT fixed in production.** Step 5 is the final production arbiter for CR-01 — relational tests drive the handler directly, not the full HTTP + Finbuckle slug-strategy + Aspire pipeline.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue. CR-01 production arbiter NOT YET exercised."

### 6. List members (D-05 batch)

expected: `GET /api/v1/workspaces/acme-smoke-test/members/?page=1&per_page=50` → 200, exactly 2 members (root Admin=20 + invitee Member=15), each with `user:{id,display_name,email,avatar_url}`.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 7. EOP self-promotion guard (T-2-eop-self) 🟥

expected: invitee `PATCH /api/v1/workspaces/acme-smoke-test/members/{inviteeMemberId}` body `{"role":20}` → **403** (Member cannot self-promote to Admin).
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 8. Admin promotes other

expected: root `PATCH /api/v1/workspaces/acme-smoke-test/members/{inviteeMemberId}` body `{"role":20}` → 200 `{role:20}`.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 9. Soft-delete workspace (D-08 epoch)

expected: root `DELETE /api/v1/workspaces/acme-smoke-test` → 200 or 204.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 10. Slug release → reuse (D-08 load-bearing) 🟥

expected: root `POST /api/v1/workspaces/` body `{"name":"Acme Smoke Test","slug":"acme-smoke-test"}` → **201** (slug reusable after soft-delete; CR-03 re-accept path implicitly covered).
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 11. Cross-workspace isolation (D-02) 🟥 CR-cluster

expected: invitee `GET /api/v1/workspaces/another-workspace/members/` (a workspace the invitee is NOT a member of) → **403**.
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

### 12. [CR-02 supplementary] List invitee's own workspaces (cross-tenant)

expected: invitee `GET /api/v1/users/me/workspaces/?page=1&per_page=20` with `tenant: root` → 200, results include slug `acme-smoke-test`. Validates CR-02's `IgnoreQueryFilters` cross-tenant aggregate fix in production (NOT covered by the 11 HUMAN-UAT steps; only in smoke-workspace.py).
result: blocked
blocked_by: server
reason: "Live Aspire stack could not start (Docker Desktop not running). Not a code issue."

## Summary

total: 12
passed: 0
issues: 0
pending: 0
skipped: 0
blocked: 12

## Gaps

[none — all 12 checks are blocked_by `server` (environmental: Docker Desktop not running), which is a prerequisite gate, NOT a code defect. No gaps to route to /gsd-plan-phase --gaps.]

## Resume Notes

- **Driver ready:** `.planning/phases/02-workspace/smoke-workspace.py http://localhost:<APIport>` runs all 12 checks (bypasses HTTP_PROXY via `ProxyHandler({})`, tolerates self-signed dev cert via `ssl.CERT_NONE`).
- **Boot gotchas:** AppHost = `dotnet run` ONLY (no `--launch-profile`; `http` profile crashes); stdout is block-buffered through `tee` so probe `docker ps` + ports, not the log. API port is DYNAMIC — find via Aspire dashboard resource list OR scan listening localhost ports for `/health/ready` 200. Dashboard binds :15036 (http) + :17273 (https). `/auth/sign-in` is rate-limited 10/60s.
- **API contract (recon-verified):** root = `admin@root.com` / `123Pa$$word!`, tenant `root`. Roles Guest=5/Member=15/Admin=20 (no numeric Owner). Workspace-scoped `{slug}` routes DROP `tenant` header; top-level routes (#1 create, #3 slug-check, #5 accept, #10 reuse) SEND `tenant: root`. Accept route has literal trailing slash.
- **Pre-flight before retry:** confirm `docker --context desktop-linux info` returns a ServerVersion (pipe exists) AND `docker ps` is clean. If `desktop-linux` still errors → Docker Desktop still not up.
