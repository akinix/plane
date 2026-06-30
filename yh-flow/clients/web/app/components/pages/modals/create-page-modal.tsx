// FLOW: Forked from Plane. Original: apps/web/core/components/pages/modals/create-page-modal.tsx
// FLOW: CreatePageModal — 创建页面弹窗
"use client";
import { useState } from "react";
import type { TPage } from "@plane/types";
import { EPageAccess } from "@plane/types";
import { ModalCore, EModalPosition, EModalWidth } from "@plane/ui";
import { usePageMutations } from "@/../src/lib/hooks/use-page-mutations";
import { PageForm } from "./page-form";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  workspaceId: string;
};

export const CreatePageModal = function CreatePageModal({ isOpen, onClose, workspaceId }: Props) {
  const [pageFormData, setPageFormData] = useState<Partial<TPage>>({
    id: undefined,
    name: "",
    logo_props: undefined,
    access: EPageAccess.PUBLIC,
  });
  const { createPage } = usePageMutations();

  const handlePageFormData = <T extends keyof TPage>(key: T, value: TPage[T]) =>
    setPageFormData((prev) => ({ ...prev, [key]: value }));

  const handleStateClear = () => {
    setPageFormData({ id: undefined, name: "", logo_props: undefined, access: EPageAccess.PUBLIC });
    onClose();
  };

  const handleFormSubmit = async () => {
    if (!workspaceId) return;

    try {
      await createPage.mutateAsync({ ...pageFormData, workspace: workspaceId });
      handleStateClear();
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <ModalCore isOpen={isOpen} handleClose={handleStateClear} position={EModalPosition.TOP} width={EModalWidth.XXL}>
      <PageForm
        formData={pageFormData}
        handleFormData={handlePageFormData}
        handleModalClose={handleStateClear}
        handleFormSubmit={handleFormSubmit}
      />
    </ModalCore>
  );
};
