// FLOW: Auth routes and workspace layout (15-02 per D-P15-02, 15-03 per WORK-01~04, 15-04 per PROJ-01~05)
import { index, layout, route, type RouteConfig } from "@react-router/dev/routes";

export default [
  // ========================================================================
  // AUTH ROUTES (outside workspace layout)
  // ========================================================================
  route("/auth/sign-in", "app/auth/sign-in/page.tsx"),
  route("/auth/sign-up", "app/auth/sign-up/page.tsx"),
  route("/auth/forgot-password", "app/auth/forgot-password/page.tsx"),
  route("/auth/reset-password", "app/auth/reset-password/page.tsx"),

  // ========================================================================
  // CREATE WORKSPACE (standalone page, no sidebar — per D-P15-04)
  // ========================================================================
  route("workspaces/create", "app/workspaces/create/page.tsx"),

  // ========================================================================
  // WORKSPACE LAYOUT (authenticated routes per D-P15-02)
  // ========================================================================
  layout("app/layouts/workspace-layout.tsx", [
    // / redirects to first workspace or create-workspace
    index("app/workspace-redirect/page.tsx"),

    // Workspace pages (15-03)
    route("workspaces/:workspaceId", "app/workspaces/[workspaceId]/page.tsx"),
    route(
      "workspaces/:workspaceId/settings/*",
      "app/workspaces/[workspaceId]/settings/page.tsx",
    ),
    route(
      "workspaces/:workspaceId/members/*",
      "app/workspaces/[workspaceId]/members/page.tsx",
    ),

    // ====================================================================
    // PROJECT ROUTES — :projectId before /* to avoid splat hijacking
    // ====================================================================
    route(
      "workspaces/:workspaceId/projects/:projectId",
      "app/workspaces/[workspaceId]/projects/[projectId]/page.tsx",
    ),
    route(
      "workspaces/:workspaceId/projects/:projectId/settings",
      "app/workspaces/[workspaceId]/projects/[projectId]/settings/page.tsx",
    ),
    route(
      "workspaces/:workspaceId/projects/:projectId/members",
      "app/workspaces/[workspaceId]/projects/[projectId]/members/page.tsx",
    ),
    route(
      "workspaces/:workspaceId/projects/*",
      "app/workspaces/[workspaceId]/projects/page.tsx",
    ),
  ]),
] satisfies RouteConfig;
