// FLOW: TanStack Query hooks for workspace member data (mock data layer per D-P15-12)
import { useQuery } from "@tanstack/react-query";
import type { IWorkspaceMember } from "@plane/types";
import { MOCK_MEMBERS } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useMembers = (workspaceId: string) => {
  return useQuery<IWorkspaceMember[]>({
    queryKey: ["members", workspaceId],
    queryFn: async () => {
      await delay(200);
      return MOCK_MEMBERS[workspaceId] ?? [];
    },
    enabled: !!workspaceId,
  });
};
