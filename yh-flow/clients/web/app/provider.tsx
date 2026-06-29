// FLOW: AppProvider — global providers (QueryClientProvider, StoreProvider, AuthInitializer)
import type { ReactNode } from "react";
import { useEffect, useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
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
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 5 * 60 * 1000,
            retry: 1,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      <StoreProvider>
        <AuthInitializer>
          {children}
        </AuthInitializer>
      </StoreProvider>
    </QueryClientProvider>
  );
}
