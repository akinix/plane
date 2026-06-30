// FLOW: Forked from Plane. Original: apps/web/core/components/pages/editor/editor-body.tsx
// FLOW: PageEditorBody — 编辑器主体（TipTap 集成）
"use client";
import { useEffect, useRef } from "react";
import { observer } from "mobx-react";
import { DocumentEditorWithRef } from "@/lib/editor";
import type { EditorRefApi } from "@/lib/editor";

type Props = {
  onChange: (json: object, html: string) => void;
  initialValue: string;
  editable: boolean;
  id: string;
};

export const PageEditorBody = observer(function PageEditorBody({ onChange, initialValue, editable, id }: Props) {
  const editorRef = useRef<EditorRefApi>(null);
  const initialValueRef = useRef(initialValue);

  // DocumentEditorWithRef handles value prop directly;
  // this effect updates the ref and editor when initialValue changes (new page loaded)
  useEffect(() => {
    initialValueRef.current = initialValue;
    if (editorRef.current && initialValue) {
      editorRef.current.setEditorValue(initialValue);
    }
  }, [initialValue]);

  return (
    <div className="relative size-full">
      <DocumentEditorWithRef
        ref={editorRef}
        editable={editable}
        id={id}
        value={initialValue}
        onChange={(json, html) => onChange(json, html)}
        containerClassName="h-full p-0 pb-64"
        disabledExtensions={[]}
        flaggedExtensions={[]}
        fileHandler={{
          getAsset: async () => null,
          upload: async () => ({ id: "", asset_url: "" }) as any,
          delete: async () => {},
          restore: async () => {},
        }}
        getEditorMetaData={(_htmlContent) => ({}) as any}
        mentionHandler={{
          searchCallback: async () => [],
          renderComponent: () => null,
          getMentionedEntityDetails: () => "",
        }}
      />
    </div>
  );
});
