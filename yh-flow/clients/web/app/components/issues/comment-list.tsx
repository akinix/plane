// FLOW: CommentList — issue comment list with edit/delete support
"use client";

import { useState } from "react";
import { observer } from "mobx-react";
import { formatDistanceToNow } from "date-fns";
import { zhCN } from "date-fns/locale";
import { Avatar, Loader } from "@plane/ui";
import { useComments, useUpdateComment, useDeleteComment } from "@/../src/lib/hooks/use-comments";
import { LiteTextEditorWithRef, type EditorRefApi } from "@plane/editor";
import { useRef } from "react";

type Props = {
  issueId: string;
  workspaceId: string;
  projectId: string;
};

export const CommentList = observer(function CommentList({ issueId }: Props) {
  const { data: comments, isLoading } = useComments(issueId);
  const updateComment = useUpdateComment();
  const deleteComment = useDeleteComment();
  const [editingId, setEditingId] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const editRef = useRef<EditorRefApi | null>(null);

  if (isLoading) {
    return (
      <div className="space-y-3">
        <h4 className="text-custom-text-100 text-h4-semibold">评论</h4>
        <Loader className="space-y-3">
          <div className="flex gap-3">
            <Loader.Item height="32px" width="32px" className="rounded-full" />
            <div className="flex-1 space-y-2">
              <Loader.Item height="14px" width="120px" />
              <Loader.Item height="40px" />
            </div>
          </div>
        </Loader>
      </div>
    );
  }

  const handleSaveEdit = (commentId: string) => {
    if (editRef.current) {
      const html = editRef.current.getDocument().html;
      if (html) {
        updateComment.mutate({ commentId, comment_html: html });
      }
    }
    setEditingId(null);
  };

  const handleDelete = (commentId: string) => {
    deleteComment.mutate({ commentId, issueId });
    setDeletingId(null);
  };

  return (
    <div className="flex flex-col gap-1">
      <h4 className="text-custom-text-100 mb-3 text-h4-semibold">评论</h4>

      {!comments || comments.length === 0 ? (
        <p className="text-sm text-custom-text-400 py-4">暂无评论</p>
      ) : (
        <div className="flex flex-col gap-4">
          {comments
            .slice()
            .toSorted((a, b) => new Date(a.created_at).getTime() - new Date(b.created_at).getTime())
            .map((comment) => {
              const isEditing = editingId === comment.id;
              const isDeleting = deletingId === comment.id;
              const isCurrentUser = comment.actor === "user-1";

              return (
                <div key={comment.id} className="flex gap-3">
                  {/* Avatar */}
                  <div className="flex-shrink-0">
                    <Avatar name={comment.actor_detail.display_name} src={comment.actor_detail.avatar_url} size="sm" />
                  </div>

                  {/* Comment body */}
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-custom-text-100 font-medium">
                        {comment.actor_detail.display_name}
                      </span>
                      <span className="text-xs text-custom-text-400">
                        {formatDistanceToNow(new Date(comment.created_at), {
                          addSuffix: true,
                          locale: zhCN,
                        })}
                      </span>
                      {comment.edited_at && <span className="text-xs text-custom-text-400">(已编辑)</span>}
                    </div>

                    {/* Comment content */}
                    {isEditing ? (
                      <div className="border-custom-border-200 bg-custom-background-100 mt-1 rounded-md border">
                        <LiteTextEditorWithRef
                          ref={editRef}
                          editable={true}
                          initialValue={comment.comment_html}
                          containerClassName="p-2 min-h-[60px]"
                          fileHandler={{
                            assetsUploadStatus: {},
                            cancel: () => {},
                            checkIfAssetExists: async () => false,
                            delete: async () => {},
                            getAssetDownloadSrc: async (path: string) => path,
                            getAssetSrc: async (path: string) => path,
                            restore: async () => {},
                            upload: async () => "",
                            duplicate: async () => "",
                            validation: { maxFileSize: 5242880 },
                          }}
                          mentionHandler={{
                            getMentionedEntityDetails: () => undefined,
                            renderComponent: () => null,
                            searchCallback: async () => [],
                          }}
                          getEditorMetaData={() => ({ file_assets: [], user_mentions: [] })}
                          disabledExtensions={[]}
                          flaggedExtensions={[]}
                          extendedEditorProps={undefined as any}
                          id={`edit-comment-${comment.id}`}
                        />
                        <div className="border-custom-border-200 flex items-center justify-end gap-2 border-t px-2 py-1.5">
                          <button
                            onClick={() => setEditingId(null)}
                            className="text-xs text-custom-text-400 hover:text-custom-text-300 rounded-sm px-2 py-1 transition-colors"
                          >
                            取消
                          </button>
                          <button
                            onClick={() => handleSaveEdit(comment.id)}
                            className="bg-custom-primary text-xs rounded-sm px-3 py-1 text-white transition-opacity hover:opacity-90"
                          >
                            保存
                          </button>
                        </div>
                      </div>
                    ) : (
                      <div
                        className="text-sm text-custom-text-200 prose-sm mt-1 max-w-none prose"
                        dangerouslySetInnerHTML={{ __html: comment.comment_html }}
                      />
                    )}

                    {/* Action buttons (current user only) */}
                    {isCurrentUser && !isEditing && (
                      <div className="mt-1 flex items-center gap-2 opacity-0 transition-opacity group-hover:opacity-100">
                        <button
                          onClick={() => {
                            setEditingId(comment.id);
                            setDeletingId(null);
                          }}
                          className="text-xs text-custom-text-400 hover:text-custom-text-300 transition-colors"
                        >
                          编辑
                        </button>
                        <button
                          onClick={() => setDeletingId(comment.id)}
                          className="text-xs text-danger-primary transition-colors hover:text-danger-primary/80"
                        >
                          删除
                        </button>
                      </div>
                    )}

                    {/* Delete confirmation */}
                    {isDeleting && (
                      <div className="border-custom-border-200 bg-custom-background-80 mt-2 flex items-center gap-2 rounded-md border px-3 py-2">
                        <span className="text-xs text-custom-text-300">确定删除此评论吗？</span>
                        <button
                          onClick={() => handleDelete(comment.id)}
                          className="text-xs rounded-sm bg-danger-primary px-2 py-0.5 text-white transition-opacity hover:opacity-90"
                        >
                          删除
                        </button>
                        <button
                          onClick={() => setDeletingId(null)}
                          className="text-xs text-custom-text-400 hover:text-custom-text-300 rounded-sm px-2 py-0.5 transition-colors"
                        >
                          取消
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
        </div>
      )}
    </div>
  );
});
