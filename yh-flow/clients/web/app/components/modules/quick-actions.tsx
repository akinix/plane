// FLOW: Forked from Plane modules/quick-actions.tsx
// FLOW: ModuleQuickActions — Module 快速操作菜单
import { useState } from "react";
import { MoreHorizontal, Edit3, Trash2, Link, ExternalLink } from "lucide-react";
import { CustomMenu } from "@plane/ui";
import { copyUrlToClipboard, cn } from "@plane/utils";
import { useStore } from "@/lib/store-context";

type Props = {
  parentRef: React.RefObject<HTMLDivElement>;
  moduleId: string;
  projectId: string;
  workspaceSlug: string;
  customClassName?: string;
};

export const ModuleQuickActions = function ModuleQuickActions(props: Props) {
  const { moduleId, projectId, workspaceSlug, customClassName } = props;
  const [open, setOpen] = useState(false);
  const { module: moduleStore } = useStore();

  const moduleLink = `${workspaceSlug}/projects/${projectId}/modules/${moduleId}`;

  const handleEdit = () => {
    moduleStore.openModuleModal("edit", moduleId);
  };

  const handleDelete = () => {
    moduleStore.openDeleteModal(moduleId);
  };

  const handleCopyLink = () => {
    copyUrlToClipboard(moduleLink);
  };

  const handleOpenInNewTab = () => {
    window.open(`/${moduleLink}`, "_blank");
  };

  const menuItems = [
    {
      key: "edit",
      icon: Edit3,
      label: "编辑",
      action: handleEdit,
    },
    {
      key: "copy-link",
      icon: Link,
      label: "复制链接",
      action: handleCopyLink,
    },
    {
      key: "open-new-tab",
      icon: ExternalLink,
      label: "新标签页打开",
      action: handleOpenInNewTab,
    },
    {
      key: "delete",
      icon: Trash2,
      label: "删除",
      action: handleDelete,
      className: "text-red-500",
    },
  ];

  return (
    <CustomMenu
      customButton={
        <button
          className={cn("grid place-items-center p-1 text-tertiary hover:text-primary", customClassName)}
          onClick={() => setOpen(!open)}
        >
          <MoreHorizontal className="h-4 w-4" />
        </button>
      }
      placement="bottom-end"
      closeOnSelect
    >
      {menuItems.map((item) => (
        <CustomMenu.MenuItem
          key={item.key}
          onClick={item.action}
          className={cn("flex items-center gap-2", item.className)}
        >
          <item.icon className="h-3.5 w-3.5" />
          <span>{item.label}</span>
        </CustomMenu.MenuItem>
      ))}
    </CustomMenu>
  );
};
