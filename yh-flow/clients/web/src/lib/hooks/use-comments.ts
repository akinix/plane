// FLOW: TanStack Query hooks for issue comment data (mock data layer)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssueComment } from "@plane/types";
import { MOCK_ISSUE_COMMENTS } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useComments = (issueId: string) => {
  return useQuery<TIssueComment[]>({
    queryKey: ["issue-comments", issueId],
    queryFn: async () => {
      await delay(200);
      return MOCK_ISSUE_COMMENTS.filter((c) => c.issue === issueId);
    },
    enabled: !!issueId,
  });
};

export const useCreateComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ issueId, comment_html }: { issueId: string; comment_html: string }) => {
      await delay(300);
      const newComment: TIssueComment = {
        id: `comment-new-${Date.now()}`,
        workspace: "ws-1",
        project: "proj-1",
        issue: issueId,
        actor: "user-1",
        actor_detail: {
          id: "user-1",
          first_name: "我",
          last_name: "",
          avatar_url: "",
          is_bot: false,
          display_name: "我",
        },
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString(),
        created_by: "user-1",
        updated_by: "user-1",
        attachments: [],
        comment_reactions: [],
        comment_stripped: comment_html.replace(/<[^>]*>/g, ""),
        comment_html,
        comment_json: null as any,
        external_id: undefined,
        external_source: undefined,
        access: "INTERNAL" as any,
        workspace_detail: { id: "ws-1", name: "Flow 开发组", slug: "flow-dev" },
        project_detail: {
          id: "proj-1",
          identifier: "FF",
          name: "Flow 前端",
          cover_image: "",
          description: null,
          emoji: null,
          icon_prop: null,
        },
        issue_detail: {
          id: issueId,
          sequence_id: 0,
          sort_order: false as any,
          name: "",
          description_html: "",
          priority: "none" as any,
          start_date: "",
          target_date: "",
          is_draft: false,
        },
        edited_at: undefined,
      };
      MOCK_ISSUE_COMMENTS.push(newComment);
      return newComment;
    },
    onSuccess: (_data, { issueId }) => {
      queryClient.invalidateQueries({ queryKey: ["issue-comments", issueId] });
    },
  });
};

export const useUpdateComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ commentId, comment_html }: { commentId: string; comment_html: string }) => {
      await delay(200);
      const index = MOCK_ISSUE_COMMENTS.findIndex((c) => c.id === commentId);
      if (index >= 0) {
        MOCK_ISSUE_COMMENTS[index] = {
          ...MOCK_ISSUE_COMMENTS[index],
          comment_html,
          comment_stripped: comment_html.replace(/<[^>]*>/g, ""),
          updated_at: new Date().toISOString(),
          edited_at: new Date().toISOString(),
        };
      }
      return { commentId };
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["issue-comments"] });
    },
  });
};

export const useDeleteComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ commentId, issueId }: { commentId: string; issueId: string }) => {
      await delay(200);
      const index = MOCK_ISSUE_COMMENTS.findIndex((c) => c.id === commentId);
      if (index >= 0) {
        MOCK_ISSUE_COMMENTS.splice(index, 1);
      }
      return { commentId, issueId };
    },
    onSuccess: (_data, { issueId }) => {
      queryClient.invalidateQueries({ queryKey: ["issue-comments", issueId] });
    },
  });
};
