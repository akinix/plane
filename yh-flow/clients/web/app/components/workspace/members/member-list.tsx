// FLOW: MemberList — workspace member list with role management (WORK-04)
import { useMembers } from "../../../../src/lib/hooks/use-members";
import { MemberRoleDropdown } from "./member-role-dropdown";

type TProps = { workspaceId: string };

export const MemberList = ({ workspaceId }: TProps) => {
  const { data: members, isLoading } = useMembers(workspaceId);

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2">
        {Array.from({ length: 3 }).map((_, i) => (
          <div
            key={i}
            className="h-14 animate-pulse rounded-lg bg-custom-background-80"
          />
        ))}
      </div>
    );
  }

  if (!members || members.length === 0) {
    return (
      <p className="py-4 text-center text-sm text-custom-text-300">
        暂无成员
      </p>
    );
  }

  return (
    <div className="flex flex-col">
      {members.map((mem) => (
        <div
          key={mem.id}
          className="flex items-center gap-3 border-b border-custom-border-200 py-3 last:border-b-0"
        >
          {/* Avatar */}
          <div className="flex size-8 flex-shrink-0 items-center justify-center rounded-full bg-custom-background-80 text-sm font-medium text-custom-text-200">
            {mem.member.display_name.charAt(0)}
          </div>

          {/* Info */}
          <div className="min-w-0 flex-1">
            <p className="text-sm font-medium text-custom-text-100">
              {mem.member.display_name}
            </p>
            <p className="text-xs text-custom-text-300">{mem.email}</p>
          </div>

          {/* Role dropdown */}
          <MemberRoleDropdown
            currentRole={mem.role}
            onRoleChange={(newRole) => {
              console.log(
                `Role changed: ${mem.member.display_name} → ${newRole}`,
              );
            }}
          />
        </div>
      ))}
    </div>
  );
};
