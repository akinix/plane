// FLOW: Forked from Plane apps/web/app/provider.tsx
import type { ReactNode } from "react";
// FLOW: ThemeProvider is in root.tsx Layout function per Plane pattern
// FLOW: Subsequent phases will add AuthProvider, StoreProvider, etc.

export interface IAppProvider {
  children: ReactNode;
}

export function AppProvider(props: IAppProvider) {
  const { children } = props;

  // FLOW: Minimal AppProvider — ThemeProvider is in root.tsx Layout
  return <>{children}</>;
}
