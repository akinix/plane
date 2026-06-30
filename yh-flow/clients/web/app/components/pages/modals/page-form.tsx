// FLOW: Forked from Plane. Original: apps/web/core/components/pages/modals/page-form.tsx
// FLOW: PageForm — 页面创建/编辑表单
"use client";
import type { FormEvent } from "react";
import { useState } from "react";
import { Globe, Lock, FileText } from "lucide-react";
import { EPageAccess } from "@plane/constants";
import type { TPage } from "@plane/types";
import { Input, Button } from "@plane/ui";

type Props = {
  formData: Partial<TPage>;
  handleFormData: <T extends keyof TPage>(key: T, value: TPage[T]) => void;
  handleModalClose: () => void;
  handleFormSubmit: () => Promise<void>;
};

export function PageForm(props: Props) {
  const { formData, handleFormData, handleModalClose, handleFormSubmit } = props;
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handlePageFormSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      await handleFormSubmit();
      setIsSubmitting(false);
    } catch {
      setIsSubmitting(false);
    }
  };

  const isTitleLengthMoreThan255Character = formData.name ? formData.name.length > 255 : false;

  return (
    <form onSubmit={handlePageFormSubmit}>
      <div className="space-y-5 p-5">
        <h3 className="text-lg text-custom-text-100 font-medium">创建页面</h3>
        <div className="flex w-full items-start gap-2">
          {/* Emoji/Icon placeholder — 复用 @plane/ui EmojiPicker in future */}
          <div className="bg-custom-background-80 flex size-9 flex-shrink-0 items-center justify-center rounded-md">
            <FileText className="text-custom-text-300 size-4" />
          </div>
          <div className="w-full space-y-1">
            <Input
              id="name"
              type="text"
              value={formData.name ?? ""}
              onChange={(e) => handleFormData("name", e.target.value)}
              placeholder="页面名称"
              className="text-sm w-full resize-none"
              required
            />
            {isTitleLengthMoreThan255Character && (
              <span className="text-red-500 text-[11px]">名称长度不得超过 255 个字符</span>
            )}
          </div>
        </div>
      </div>
      <div className="border-custom-border-200 flex items-center justify-between gap-2 border-t px-5 py-4">
        {/* Access toggle */}
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() =>
              handleFormData(
                "access",
                formData.access === EPageAccess.PUBLIC ? EPageAccess.PRIVATE : EPageAccess.PUBLIC
              )
            }
            className="border-custom-border-200 text-xs text-custom-text-200 hover:bg-custom-background-80 flex items-center gap-1.5 rounded-md border px-2.5 py-1.5"
          >
            {formData.access === EPageAccess.PUBLIC ? (
              <>
                <Globe className="size-3.5" /> 公开
              </>
            ) : (
              <>
                <Lock className="size-3.5" /> 私人
              </>
            )}
          </button>
          <span className="text-custom-text-300 text-[11px]">
            {formData.access === EPageAccess.PUBLIC ? "工作区内所有人可查看" : "仅创建者可查看"}
          </span>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="secondary" size="lg" onClick={handleModalClose}>
            取消
          </Button>
          <Button
            variant="primary"
            size="lg"
            type="submit"
            loading={isSubmitting}
            disabled={isTitleLengthMoreThan255Character || !formData.name?.trim()}
          >
            {isSubmitting ? "创建中..." : "创建页面"}
          </Button>
        </div>
      </div>
    </form>
  );
}
