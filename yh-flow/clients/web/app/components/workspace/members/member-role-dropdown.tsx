// FLOW: MemberRoleDropdown — role selector for workspace members (WORK-04)
import { useState, useRef, useEffect } from "react";
import { ChevronDown } from "lucide-react";
import { EUserPermissions } from "@plane/types";
import { cn } from "@plane/utils";

const ROLES = [
  { value: EUserPermissions.ADMIN, label: "Admin" },
  { value: EUserPermissions.MEMBER, label: "Member" },
  { value: EUserPermissions.GUEST, label: "Guest" },
] as const;

type TProps = {
  currentRole: number;
  onRoleChange: (newRole: EUserPermissions) => void;
};

export const MemberRoleDropdown = ({ currentRole, onRoleChange }: TProps) => {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  const currentLabel = ROLES.find((r) => r.value === currentRole)?.label ?? "Member";

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-1 rounded-md px-2 py-1 text-xs text-custom-text-200 hover:bg-custom-background-80 transition-colors"
      >
        <span>{currentLabel}</span>
        <ChevronDown className="size-3" />
      </button>
      {open && (
        <div className="absolute right-0 z-50 mt-1 w-28 rounded-md border border-custom-border-200 bg-custom-background-100 shadow-lg">
          {ROLES.map((role) => (
            <button
              key={role.value}
              onClick={() => {
                onRoleChange(role.value);
                setOpen(false);
              }}
              className={cn(
                "flex w-full items-center px-3 py-1.5 text-xs transition-colors",
                role.value === currentRole
                  ? "bg-custom-background-80 text-custom-text-100"
                  : "text-custom-text-200 hover:bg-custom-background-80",
              )}
            >
              {role.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
