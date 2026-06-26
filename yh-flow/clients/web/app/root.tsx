// FLOW: Forked from Plane apps/web/app/root.tsx
import type { ReactNode } from "react";
import { Links, Meta, Outlet, Scripts } from "react-router";
import { ThemeProvider } from "next-themes";
import globalStyles from "@/styles/globals.css?url";
import type { LinksFunction } from "react-router";
// FLOW: local imports
import { AppProvider } from "./provider";

export const links: LinksFunction = () => [{ rel: "stylesheet", href: globalStyles }];

export function Layout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <Meta />
        <Links />
      </head>
      <body suppressHydrationWarning>
        <ThemeProvider themes={["light", "dark"]} defaultTheme="system" attribute="data-theme" enableSystem>
          {children}
        </ThemeProvider>
        <Scripts />
      </body>
    </html>
  );
}

export default function Root() {
  return (
    <AppProvider>
      <div className="relative flex h-screen w-full flex-col overflow-hidden bg-canvas">
        <main className="relative h-full w-full overflow-hidden">
          <Outlet />
        </main>
      </div>
    </AppProvider>
  );
}

export function HydrateFallback() {
  return <div />;
}
