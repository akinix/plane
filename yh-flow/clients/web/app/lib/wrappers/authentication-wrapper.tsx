// FLOW: AuthenticationWrapper — route protection component (AUTH-05, AUTH-06)
import type { ReactNode } from "react";
import { observer } from "mobx-react";
import { useNavigate, useLocation } from "react-router";
import { useStore } from "@/lib/store-context";

export enum EPageTypes {
  PUBLIC = "PUBLIC",
  NON_AUTHENTICATED = "NON_AUTHENTICATED",
  AUTHENTICATED = "AUTHENTICATED",
}

type TAuthenticationWrapper = {
  children: ReactNode;
  pageType?: EPageTypes;
};

export const AuthenticationWrapper = observer(function AuthenticationWrapper(props: TAuthenticationWrapper) {
  const { children, pageType = EPageTypes.AUTHENTICATED } = props;
  const navigate = useNavigate();
  const location = useLocation();
  const { auth } = useStore();

  // Loading state
  if (auth.isLoading) {
    return (
      <div className="relative flex h-screen w-full items-center justify-center bg-canvas">
        <div className="size-8 animate-spin rounded-full border-2 border-custom-border-strong border-t-custom-primary" />
      </div>
    );
  }

  // PUBLIC: everyone can access
  if (pageType === EPageTypes.PUBLIC) return <>{children}</>;

  // NON_AUTHENTICATED: only unauthenticated users (login/register pages)
  if (pageType === EPageTypes.NON_AUTHENTICATED) {
    if (!auth.isAuthenticated) return <>{children}</>;
    else {
      navigate("/", { replace: true });
      return null;
    }
  }

  // AUTHENTICATED: only authenticated users (per AUTH-06)
  if (pageType === EPageTypes.AUTHENTICATED) {
    if (auth.isAuthenticated) {
      return <>{children}</>;
    } else {
      const nextPath = encodeURIComponent(location.pathname + location.search);
      navigate(`/auth/sign-in?next_path=${nextPath}`, { replace: true });
      return null;
    }
  }

  return <>{children}</>;
});
