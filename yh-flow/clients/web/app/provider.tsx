// FLOW: AppProvider — global providers (StoreProvider, AuthInitializer)
import type { ReactNode } from "react";
import { useEffect } from "react";
import { useStore } from "@/lib/store-context";
import { StoreProvider } from "@/lib/store-context";

export interface IAppProvider {
  children: ReactNode;
}

function AuthInitializer({ children }: { children: ReactNode }) {
  const { auth } = useStore();

  useEffect(() => {
    auth.initAuth();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return <>{children}</>;
}

export function AppProvider(props: IAppProvider) {
  const { children } = props;

  return (
    <StoreProvider>
      <AuthInitializer>
        {children}
      </AuthInitializer>
    </StoreProvider>
  );
}
