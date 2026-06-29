// FLOW: Workspace redirect — first workspace or create (15-02 per D-P15-04)
import { useEffect } from "react";
import { useNavigate } from "react-router";
import { useWorkspaces } from "../../src/lib/hooks/use-workspaces";

export default function WorkspaceRedirectPage() {
  const navigate = useNavigate();
  const { data: workspaces, isLoading } = useWorkspaces();

  useEffect(() => {
    if (isLoading) return;

    if (!workspaces || workspaces.length === 0) {
      // No workspace → navigate to create workspace
      navigate("/workspaces/create", { replace: true });
    } else {
      // Has workspace → navigate to first workspace dashboard
      navigate(`/workspaces/${workspaces[0].id}`, { replace: true });
    }
  }, [workspaces, isLoading, navigate]);

  // Loading spinner while redirect logic runs
  if (isLoading) {
    return (
      <div className="flex h-full w-full items-center justify-center">
        <div className="border-custom-border-strong border-t-custom-primary size-8 animate-spin rounded-full border-2" />
      </div>
    );
  }

  // Brief flash while useEffect runs the redirect
  return null;
}
