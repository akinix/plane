// FLOW: Forked from Plane apps/web/app/provider.tsx
// FLOW: Minimal AppProvider — will be extended with ThemeProvider, AuthProvider, StoreProvider in subsequent phases
import type { ReactNode } from "react";

export interface IAppProvider {
  children: ReactNode;
}

export function AppProvider(props: IAppProvider) {
  const { children } = props;

  return <>{children}</>;
}
