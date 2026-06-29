// FLOW: Home page — placeholder with sign-out (AUTH-05, AUTH-06)
import { useNavigate } from "react-router";
import { useStore } from "@/lib/store-context";
import { AuthenticationWrapper, EPageTypes } from "@/lib/wrappers/authentication-wrapper";

export default function HomePage() {
  const navigate = useNavigate();
  const { auth } = useStore();

  const handleSignOut = async () => {
    await auth.signOut();
    navigate("/auth/sign-in", { replace: true });
  };

  return (
    <AuthenticationWrapper pageType={EPageTypes.AUTHENTICATED}>
      <div className="flex h-screen items-center justify-center bg-custom-background-100">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-semibold text-custom-text-100">Welcome to Flow</h1>
          <button
            onClick={handleSignOut}
            className="rounded-md bg-custom-primary px-4 py-2 text-sm font-medium text-white hover:bg-custom-primary-200"
          >
            Sign out
          </button>
        </div>
      </div>
    </AuthenticationWrapper>
  );
}
