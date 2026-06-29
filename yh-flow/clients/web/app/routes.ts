// FLOW: Auth routes and workspace layout (15-02 per D-P15-02)
import { index, layout, type RouteConfig } from "@react-router/dev/routes";

export default [
  // ========================================================================
  // AUTH ROUTES (outside workspace layout)
  // ========================================================================
  {
    path: "/auth/sign-in",
    file: "app/auth/sign-in/page.tsx",
  },
  {
    path: "/auth/sign-up",
    file: "app/auth/sign-up/page.tsx",
  },
  {
    path: "/auth/forgot-password",
    file: "app/auth/forgot-password/page.tsx",
  },
  {
    path: "/auth/reset-password",
    file: "app/auth/reset-password/page.tsx",
  },
  // ========================================================================
  // WORKSPACE LAYOUT (authenticated routes per D-P15-02)
  // Specific page routes are registered by 15-03/15-04 plans
  // ========================================================================
  layout("app/layouts/workspace-layout.tsx", [
    // / redirects to first workspace or create-workspace
    index("app/workspace-redirect/page.tsx"),
  ]),
] satisfies RouteConfig;
