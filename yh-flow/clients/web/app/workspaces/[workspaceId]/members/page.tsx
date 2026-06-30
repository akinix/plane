// FLOW: Workspace Members page (WORK-04)
import { useParams } from "react-router";
import { MemberList } from "../../../components/workspace/members/member-list";

export default function WorkspaceMembersPage() {
  const { workspaceId } = useParams<{ workspaceId: string }>();

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 p-4 md:p-6">
      <h1 className="text-2xl font-semibold text-custom-text-100">
        工作区成员
      </h1>
      <div className="rounded-lg border border-custom-border-200 bg-custom-background-100 px-4">
        <MemberList workspaceId={workspaceId ?? ""} />
      </div>
    </div>
  );
}
