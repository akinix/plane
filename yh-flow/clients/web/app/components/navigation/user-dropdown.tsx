// FLOW: UserDropdown — user avatar dropdown menu (D-P15-07, T-15-03)
import { observer } from "mobx-react";
import { useState, useRef, useEffect } from "react";
import { User, Settings, LogOut } from "lucide-react";
import { useNavigate } from "react-router";
import { useStore } from "@/lib/store-context";

export const UserDropdown = observer(function UserDropdown() {
  const navigate = useNavigate();
  const { auth } = useStore();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  const handleSignOut = async () => {
    await auth.signOut();
    navigate("/auth/sign-in", { replace: true });
  };

  const user = auth.currentUser;

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="flex size-8 items-center justify-center rounded-md text-custom-sidebar-text-200 transition-colors hover:bg-custom-sidebar-background-80"
        title={user?.display_name ?? "用户"}
      >
        <div className="flex size-7 items-center justify-center rounded-full bg-custom-sidebar-background-80 text-xs font-medium text-custom-sidebar-text-100">
          {user?.display_name?.charAt(0)?.toUpperCase() ?? (
            <User className="size-4" />
          )}
        </div>
      </button>

      {open && (
        <div className="absolute right-0 z-50 mt-1 w-56 origin-top-right rounded-md border border-custom-border-200 bg-custom-sidebar-background-100 shadow-lg">
          {/* User info header */}
          <div className="px-3 py-2">
            <p className="text-sm font-medium text-custom-text-100">
              {user?.display_name ?? "用户"}
            </p>
            <p className="text-xs text-custom-text-300">{user?.email ?? ""}</p>
          </div>

          <div className="border-t border-custom-border-200" />

          {/* Settings */}
          <button
            onClick={() => {
              setOpen(false);
              navigate("/settings");
            }}
            className="flex w-full items-center gap-2 rounded-md px-3 py-1.5 text-sm text-custom-text-200 transition-colors hover:bg-custom-sidebar-background-80"
          >
            <Settings className="size-4" />
            <span>设置</span>
          </button>

          {/* Sign out */}
          <button
            onClick={handleSignOut}
            className="flex w-full items-center gap-2 rounded-md px-3 py-1.5 text-sm text-custom-text-200 transition-colors hover:bg-custom-sidebar-background-80"
          >
            <LogOut className="size-4" />
            <span>退出登录</span>
          </button>
        </div>
      )}
    </div>
  );
});
