// FLOW: Auth routes and placeholder home page
import { type RouteConfig } from "@react-router/dev/routes";

export default [
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
  {
    path: "/",
    file: "app/page.tsx",
  },
] satisfies RouteConfig;
