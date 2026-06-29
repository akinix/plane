// FLOW: IssueDetailMain — left column of issue detail page per D-P16-09
// Displays issue title, state indicator, and @plane/editor rich text description
"use client";

import { useRef, useState, useCallback } from "react";
import { observer } from "mobx-react";
import type { TIssue } from "@plane/types";
import { RichTextEditorWithRef, type EditorRefApi } from "@plane/editor";

type Props = {
  issue: TIssue;
  workspaceId: string;
  onUpdate: (data: Partial<TIssue>) => void;
};

export const IssueDetailMain = observer(function IssueDetailMain({
  issue,
  workspaceId: _workspaceId,
  onUpdate,
}: Props) {
  const editorRef = useRef<EditorRefApi | null>(null);
  const [isEditingTitle, setIsEditingTitle] = useState(false);
  const [titleDraft, setTitleDraft] = useState(issue.name);

  // Handle title edit
  const handleTitleSave = useCallback(() => {
    if (titleDraft.trim() && titleDraft !== issue.name) {
      onUpdate({ name: titleDraft.trim() });
    }
    setIsEditingTitle(false);
  }, [titleDraft, issue.name, onUpdate]);

  // Handle description save
  const handleDescriptionSave = useCallback(() => {
    if (editorRef.current) {
      const html = editorRef.current.getDocument().html;
      onUpdate({ description_html: html });
    }
  }, [onUpdate]);

  return (
    <div className="flex flex-col gap-4">
      {/* Issue title */}
      <div className="flex items-start gap-3">
        <div className="flex-1">
          {isEditingTitle ? (
            <input
              type="text"
              value={titleDraft}
              onChange={(e) => setTitleDraft(e.target.value)}
              onBlur={handleTitleSave}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleTitleSave();
                if (e.key === "Escape") {
                  setTitleDraft(issue.name);
                  setIsEditingTitle(false);
                }
              }}
              className="text-2xl border-custom-border-300 text-custom-text-100 focus:border-custom-primary w-full border-b bg-transparent font-semibold outline-none"
            />
          ) : (
            <button
              onClick={() => {
                setTitleDraft(issue.name);
                setIsEditingTitle(true);
              }}
              className="text-custom-text-100 hover:text-custom-text-200 w-full cursor-pointer text-left text-h2-semibold transition-colors"
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  setTitleDraft(issue.name);
                  setIsEditingTitle(true);
                }
              }}
            >
              {issue.name}
            </button>
          )}
        </div>
      </div>

      {/* Separator */}
      <hr className="border-custom-border-200" />

      {/* Rich text description editor per D-P16-12 */}
      <div className="border-custom-border-200 bg-custom-background-100 rounded-md border">
        <RichTextEditorWithRef
          ref={editorRef}
          editable={true}
          initialValue={issue.description_html ?? ""}
          containerClassName="p-4 min-h-[200px]"
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
          id={`issue-desc-${issue.id}`}
        />
        <div className="border-custom-border-200 flex items-center justify-end border-t px-4 py-2">
          <button
            onClick={handleDescriptionSave}
            className="bg-custom-primary text-sm rounded-md px-4 py-1.5 font-medium text-white transition-opacity hover:opacity-90"
          >
            保存
          </button>
        </div>
      </div>
    </div>
  );
});
