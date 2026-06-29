// FLOW: CommentInput — issue comment creation input using LiteTextEditor
"use client";

import { useRef, useState } from "react";
import { observer } from "mobx-react";
import { LiteTextEditorWithRef, type EditorRefApi } from "@plane/editor";
import { useCreateComment } from "@/../src/lib/hooks/use-comments";

type Props = {
  issueId: string;
};

export const CommentInput = observer(function CommentInput({ issueId }: Props) {
  const editorRef = useRef<EditorRefApi | null>(null);
  const [hasContent, setHasContent] = useState(false);
  const createComment = useCreateComment();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async () => {
    if (!editorRef.current) return;
    const html = editorRef.current.getDocument().html;
    const stripped = html.replace(/<[^>]*>/g, "").trim();
    if (!stripped) return;

    setIsSubmitting(true);
    try {
      await createComment.mutateAsync({ issueId, comment_html: html });
      editorRef.current.clearEditor();
      setHasContent(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="border-custom-border-200 bg-custom-background-100 rounded-md border">
      <LiteTextEditorWithRef
        ref={editorRef}
        editable={true}
        initialValue=""
        placeholder="输入评论内容..."
        containerClassName="p-3 min-h-[80px]"
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
        onChange={() => {
          if (editorRef.current) {
            const html = editorRef.current.getDocument().html;
            const stripped = html.replace(/<[^>]*>/g, "").trim();
            setHasContent(stripped.length > 0);
          }
        }}
        disabledExtensions={[]}
        flaggedExtensions={[]}
        extendedEditorProps={undefined as any}
        id={`comment-input-${issueId}`}
      />
      <div className="border-custom-border-200 flex items-center justify-end border-t px-3 py-2">
        <button
          onClick={handleSubmit}
          disabled={!hasContent || isSubmitting}
          className="bg-custom-primary text-sm rounded-md px-4 py-1.5 font-medium text-white transition-all hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isSubmitting ? "发送中..." : "发送"}
        </button>
      </div>
    </div>
  );
});
