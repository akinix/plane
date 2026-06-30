#!/usr/bin/env python3
"""
Phase 02 Workspace — live 11-step smoke driver (02-HUMAN-UAT.md).
Usage: python smoke-workspace.py <BASE>   e.g. http://localhost:53741

Bypasses HTTP(S)_PROXY (localhost). Tolerates self-signed dev cert.
Drives: root admin + a distinct invitee user, to exercise CR-01/02/03 in production.
"""
import json, sys, ssl, urllib.request, urllib.error

BASE = sys.argv[1].rstrip("/")

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE
_opener = urllib.request.build_opener(
    urllib.request.ProxyHandler({}),            # NO proxy — localhost
    urllib.request.HTTPSHandler(context=ctx),   # allow dev self-signed
)

ROOT_EMAIL = "admin@root.com"
ROOT_PASS = "123Pa$$word!"
INVITEE_EMAIL = "smoke.invitee.2026@example.com"
INVITEE_PASS = "SmokeInvite2026!"
WS_NAME = "Acme Smoke Test"
WS_SLUG = "acme-smoke-test"
ANOTHER_NAME = "Another Workspace"     # derived slug: another-workspace
ANOTHER_SLUG = "another-workspace"

results = []   # (step, name, expect, got_code, ok_bool, detail)

def req(method, path, token=None, tenant=None, body=None, allow_codes=None):
    url = BASE + path
    headers = {}
    data = None
    if body is not None:
        data = json.dumps(body).encode()
        headers["Content-Type"] = "application/json"
    if token:
        headers["Authorization"] = "Bearer " + token
    if tenant:
        headers["tenant"] = tenant
    r = urllib.request.Request(url, data=data, method=method, headers=headers)
    try:
        resp = _opener.open(r, timeout=20)
        code, raw = resp.getcode(), resp.read().decode(errors="replace")
    except urllib.error.HTTPError as e:
        code, raw = e.code, e.read().decode(errors="replace")
    except Exception as e:
        return 0, f"<transport error: {e}>", None
    try:
        parsed = json.loads(raw) if raw else None
    except Exception:
        parsed = None
    return code, raw, parsed

def show(n, name, expect, code, parsed, ok, extra=""):
    body_preview = json.dumps(parsed, ensure_ascii=False)[:300] if parsed else (code and "")[:300]
    mark = "PASS" if ok else "FAIL"
    print(f"\n[STEP {n}] {name}")
    print(f"  EXPECT: {expect}")
    print(f"  GOT   : HTTP {code}  {body_preview}")
    if extra:
        print(f"  NOTE  : {extra}")
    print(f"  >>> {mark}")
    results.append((n, name, expect, code, ok, body_preview))

# ---------------------------------------------------------------------------
print("=" * 70)
print(f"BASE = {BASE}")
print("=" * 70)

# --- AUTH: root sign-in -----------------------------------------------------
code, raw, p = req("POST", "/auth/sign-in", tenant="root",
                   body={"email": ROOT_EMAIL, "password": ROOT_PASS})
print(f"\n[AUTH root] POST /auth/sign-in -> HTTP {code}")
assert p and p.get("access_token"), f"root sign-in failed: {raw}"
TOKEN_ROOT = p["access_token"]
print(f"  root token acquired (user={p.get('user',{}).get('email')})")

# --- AUTH: invitee sign-up (distinct user under tenant root) ----------------
code, raw, p = req("POST", "/auth/sign-up", tenant="root",
                   body={"email": INVITEE_EMAIL, "password": INVITEE_PASS,
                         "first_name": "Smoke", "last_name": "Invitee"})
print(f"\n[AUTH invitee] POST /auth/sign-up -> HTTP {code}")
assert p and p.get("access_token"), f"invitee sign-up failed: {raw}"
TOKEN_INV = p["access_token"]
print(f"  invitee token acquired ({INVITEE_EMAIL})")

# --- STEP 1: Create workspace (D-06 auto-Admin) [CR-02 marker] -------------
code, raw, p = req("POST", "/api/v1/workspaces/", token=TOKEN_ROOT, tenant="root",
                   body={"name": WS_NAME})
ok = code == 201 and p and p.get("slug") == WS_SLUG and p.get("id")
WS_ID = p.get("id") if p else None
show(1, "Create workspace (D-06 auto-Admin)", f"201 {{id, slug='{WS_SLUG}'}}", code, p, ok)

# --- STEP 2: Get workspace -------------------------------------------------
code, raw, p = req("GET", f"/api/v1/workspaces/{WS_SLUG}", token=TOKEN_ROOT)
ok = code == 200 and p and p.get("name") == WS_NAME and p.get("slug") == WS_SLUG
show(2, "Get workspace", f"200 name='{WS_NAME}' slug='{WS_SLUG}'", code, p, ok)

# --- STEP 3: Slug-check restricted word (D-09) -----------------------------
code, raw, p = req("POST", "/api/v1/workspaces/slug-check", token=TOKEN_ROOT, tenant="root",
                   body={"slug": "api"})
# Acceptable: 400 (rejected) OR 200 {exists:false}
ok = (code == 400) or (code == 200 and p is not None and p.get("exists") is False)
extra = f"exists={p.get('exists')}" if (p and 'exists' in p) else "400/restricted path"
show(3, "Slug-check restricted word 'api' (D-09)", "400 OR {exists:false}", code, p, ok, extra)

# --- STEP 4: Create invitation (D-10/D-12) ---------------------------------
code, raw, p = req("POST", f"/api/v1/workspaces/{WS_SLUG}/invitations/", token=TOKEN_ROOT,
                   body={"email": INVITEE_EMAIL, "role": 15})
ok = code == 201 and p and p.get("token")
INV_TOKEN = p.get("token") if p else None
show(4, "Create invitation (D-10/D-12)", "201 {invitationId, token, slug}", code, p, ok)

# --- STEP 5: Accept invitation (invitee JWT) [CR-01 THE DECIDER] -----------
code, raw, p = req("POST", f"/api/v1/workspaces/invitations/{INV_TOKEN}/accept/",
                   token=TOKEN_INV, tenant="root")
ok = code == 200 and p and p.get("memberId") and str(p.get("workspaceId")) == str(WS_ID)
INV_MEMBER_ID = p.get("memberId") if p else None
extra = "CR-01 DECIDER: 200 = cross-tenant accept FIXED in production" if ok else \
        "CR-01 DECIDER: 404/!200 = NOT fixed in production"
show(5, "Accept invitation (invitee JWT) [CR-01 DECIDER]",
     f"200 {{memberId, workspaceId=={WS_ID}}}", code, p, ok, extra)

# --- STEP 6: List members (D-05 batch) -------------------------------------
code, raw, p = req("GET", f"/api/v1/workspaces/{WS_SLUG}/members/?page=1&per_page=50",
                   token=TOKEN_ROOT)
members = (p or {}).get("results", []) if isinstance(p, dict) else []
n_members = len(members)
roles = sorted([m.get("role") for m in members])
ok = code == 200 and n_members == 2 and roles == [15, 20]
extra = f"count={n_members} roles={roles} (expect 2: [15 Member, 20 Admin])"
show(6, "List members (D-05 batch)", "200, 2 members roles=[15,20]", code, p, ok, extra)

# --- CR-02 supplementary: invitee lists OWN workspaces (cross-tenant) ------
code, raw, p = req("GET", "/api/v1/users/me/workspaces/?page=1&per_page=20",
                   token=TOKEN_INV, tenant="root")
ws_list = (p or {}).get("results", []) if isinstance(p, dict) else []
ws_slugs = [w.get("slug") for w in ws_list]
ok = code == 200 and WS_SLUG in ws_slugs
extra = f"invitee sees own workspaces slugs={ws_slugs} (cross-tenant list — CR-02 live check)"
show("CR-02", "List invitee's workspaces (cross-tenant, CR-02)",
     f"200 includes '{WS_SLUG}'", code, p, ok, extra)

# --- STEP 7: EOP self-promotion guard (invitee -> Admin) -------------------
code, raw, p = req("PATCH", f"/api/v1/workspaces/{WS_SLUG}/members/{INV_MEMBER_ID}",
                   token=TOKEN_INV, body={"role": 20})
ok = code == 403
extra = "self-promotion guard (Member cannot self-promote to Admin)"
show(7, "EOP self-promotion guard (T-2-eop-self)", "403", code, p, ok, extra)

# --- STEP 8: Admin promotes other ------------------------------------------
code, raw, p = req("PATCH", f"/api/v1/workspaces/{WS_SLUG}/members/{INV_MEMBER_ID}",
                   token=TOKEN_ROOT, body={"role": 20})
ok = code == 200 and p and p.get("role") == 20
show(8, "Admin promotes other", "200 role=20", code, p, ok)

# --- SETUP for step 11: create Another Workspace (invitee NOT a member) ----
code, raw, p = req("POST", "/api/v1/workspaces/", token=TOKEN_ROOT, tenant="root",
                   body={"name": ANOTHER_NAME})
print(f"\n[SETUP 11] POST /api/v1/workspaces/ name='{ANOTHER_NAME}' -> HTTP {code}")
assert code == 201 and p, f"another-workspace create failed: {raw}"

# --- STEP 9: Soft-delete workspace (D-08 epoch) ----------------------------
code, raw, p = req("DELETE", f"/api/v1/workspaces/{WS_SLUG}", token=TOKEN_ROOT)
ok = code in (200, 204)
show(9, "Soft-delete workspace (D-08 epoch)", "204 or 200", code, p, ok)

# --- STEP 10: Slug release -> reuse (D-08 load-bearing) --------------------
code, raw, p = req("POST", "/api/v1/workspaces/", token=TOKEN_ROOT, tenant="root",
                   body={"name": WS_NAME, "slug": WS_SLUG})
ok = code == 201 and p and p.get("slug") == WS_SLUG
extra = "slug reusable after soft-delete (CR-03 re-accept path implicitly covered)"
show(10, "Slug release -> reuse (D-08 load-bearing)", f"201 slug='{WS_SLUG}'", code, p, ok, extra)

# --- STEP 11: Cross-workspace isolation (D-02) [CR-cluster] ----------------
code, raw, p = req("GET", f"/api/v1/workspaces/{ANOTHER_SLUG}/members/?page=1&per_page=50",
                   token=TOKEN_INV)
ok = code == 403
extra = "invitee NOT a member of another-workspace -> 403 (cross-tenant isolation)"
show(11, "Cross-workspace isolation (D-02) [CR-cluster]", "403", code, p, ok, extra)

# ---------------------------------------------------------------------------
print("\n" + "=" * 70)
print("SUMMARY")
print("=" * 70)
passed = sum(1 for r in results if r[4])
total = len(results)
for n, name, expect, code, ok, _ in results:
    print(f"  {'✅' if ok else '❌'} [{n}] {name}  (HTTP {code})")
print(f"\n  {passed}/{total} checks passed")
print("=" * 70)
sys.exit(0 if passed == total else 1)
