// FLOW: TanStack Query hooks for Issue comments (mock data layer per D-P16-02)
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { TIssueComment } from "@plane/types";
import { EIssueCommentAccessSpecifier } from "@plane/types";
import { MOCK_COMMENTS, _ws1Detail, _proj1Detail, _user1Detail } from "../mock-data";

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const useComments = (issueId: string) => {
  return useQuery<TIssueComment[]>({
    queryKey: ["comments", issueId],
    queryFn: async () => {
      await delay(200);
      return MOCK_COMMENTS[issueId] ?? [];
    },
    enabled: !!issueId,
  });
};

export const useCreateComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ issueId, data }: { issueId: string; data: Partial<TIssueComment> }) => {
      await delay(200);
      const newComment: TIssueComment = {
        id: `cmt-mock-${Date.now()}`,
        workspace: "ws-1",
        workspace_detail: _ws1Detail,
        project: _proj1Detail.id,
        project_detail: _proj1Detail,
        issue: issueId,
        issue_detail: data.issue_detail ?? {
          id: issueId,
          sequence_id: 0,
          sort_order: true as unknown as boolean,
          name: "",
          description_html: "",
          priority: "none" as const,
          start_date: "",
          target_date: "",
          is_draft: false,
        },
        actor: "user-1",
        actor_detail: _user1Detail,
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString(),
        created_by: "user-1",
        updated_by: "user-1",
        attachments: [],
        comment_reactions: [],
        comment_stripped: data.comment_stripped ?? "",
        comment_html: data.comment_html ?? "",
        comment_json: data.comment_json ?? { type: "doc", content: [] },
        external_id: undefined,
        external_source: undefined,
        access: EIssueCommentAccessSpecifier.INTERNAL,
        ...data,
      };

      if (!MOCK_COMMENTS[issueId]) {
        MOCK_COMMENTS[issueId] = [];
      }
      MOCK_COMMENTS[issueId].push(newComment);
      return newComment;
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["comments", variables.issueId] });
    },
  });
};

export const useUpdateComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      issueId,
      commentId,
      data,
    }: {
      issueId: string;
      commentId: string;
      data: Partial<TIssueComment>;
    }) => {
      await delay(200);
      const comments = MOCK_COMMENTS[issueId];
      if (comments) {
        const idx = comments.findIndex((c) => c.id === commentId);
        if (idx >= 0) {
          comments[idx] = {
            ...comments[idx],
            ...data,
            updated_at: new Date().toISOString(),
          };
        }
      }
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["comments", variables.issueId] });
    },
  });
};

export const useDeleteComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ issueId, commentId }: { issueId: string; commentId: string }) => {
      await delay(200);
      if (MOCK_COMMENTS[issueId]) {
        MOCK_COMMENTS[issueId] = MOCK_COMMENTS[issueId].filter((c) => c.id !== commentId);
      }
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["comments", variables.issueId] });
    },
  });
};
