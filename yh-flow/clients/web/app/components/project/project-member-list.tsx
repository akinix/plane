// FLOW: ProjectMemberList — project member list with role management (PROJ-04)
import { useMembers } from "../../../src/lib/hooks/use-members";
import { useState } from "react";
import { ChevronDown } from "lucide-react";
import { EUserPermissions } from "@plane/types";
import { cn } from "@plane/utils";

const ROLES = [
  { value: EUserPermissions.ADMIN, label: "Admin" },
  { value: EUserPermissions.MEMBER, label: "Member" },
  { value: EUserPermissions.GUEST, label: "Guest" },
] as const;

type TRoleDropdownProps = {
  currentRole: number;
  onRoleChange: (role: EUserPermissions) => void;
};

const RoleDropdown = ({ currentRole, onRoleChange }: TRoleDropdownProps) => {
  const [open, setOpen] = useState(false);
  const currentLabel = ROLES.find((r) => r.value === currentRole)?.label ?? "Member";

  return (
    <div className="relative">
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

type TProps = { workspaceId: string; projectId: string };

export const ProjectMemberList = ({ workspaceId, projectId }: TProps) => {
  const { data: members, isLoading } = useMembers(workspaceId);

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-14 animate-pulse rounded-lg bg-custom-background-80" />
        ))}
      </div>
    );
  }

  if (!members || members.length === 0) {
    return (
      <p className="py-4 text-center text-sm text-custom-text-300">暂无成员</p>
    );
  }

  return (
    <div className="flex flex-col">
      {members.map((mem) => (
        <div
          key={mem.id}
          className="flex items-center gap-3 border-b border-custom-border-200 py-3 last:border-b-0"
        >
          <div className="flex size-8 flex-shrink-0 items-center justify-center rounded-full bg-custom-background-80 text-sm font-medium text-custom-text-200">
            {mem.member.display_name.charAt(0)}
          </div>
          <div className="min-w-0 flex-1">
            <p className="text-sm font-medium text-custom-text-100">
              {mem.member.display_name}
            </p>
            <p className="text-xs text-custom-text-300">{mem.email}</p>
          </div>
          <RoleDropdown
            currentRole={mem.role}
            onRoleChange={(newRole) => {
              console.log(
                `Project member role changed: ${mem.member.display_name} → ${newRole}`,
              );
            }}
          />
        </div>
      ))}
    </div>
  );
};
