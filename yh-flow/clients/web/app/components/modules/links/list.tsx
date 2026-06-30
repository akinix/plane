// FLOW: Forked from Plane modules/links/list.tsx
// FLOW: ModuleLinksList — Module 链接列表（添加/编辑/删除链接）
import { useState, useCallback } from "react";
import { Plus } from "lucide-react";
import type { ILinkDetails } from "@plane/types";
import { useModuleLinkMutations } from "@/../src/lib/hooks/use-modules";
import { useModuleDetail } from "@/../src/lib/hooks/use-modules";
import { ModulesLinksListItem, isValidUrl } from "./list-item";

type Props = {
  moduleId: string;
  projectId: string;
};

export const ModuleLinksList = function ModuleLinksList({ moduleId, projectId }: Props) {
  const { data: moduleDetails } = useModuleDetail(projectId, moduleId);
  const { addLink, updateLink, removeLink } = useModuleLinkMutations(moduleId);

  const [isAdding, setIsAdding] = useState(false);
  const [editingLinkId, setEditingLinkId] = useState<string | null>(null);
  const [newTitle, setNewTitle] = useState("");
  const [newUrl, setNewUrl] = useState("");
  const [editTitle, setEditTitle] = useState("");
  const [editUrl, setEditUrl] = useState("");

  const links = moduleDetails?.link_module ?? [];
  const canAdd = isValidUrl(newUrl);

  const handleAddLink = async () => {
    if (!canAdd || !newUrl) return;
    const newLink: ILinkDetails = {
      id: `link-${Date.now()}`,
      title: newTitle || newUrl,
      url: newUrl,
      created_at: new Date().toISOString(),
      created_by: "user-1",
      metadata: {},
    } as unknown as ILinkDetails;
    await addLink.mutateAsync(newLink);
    setNewTitle("");
    setNewUrl("");
    setIsAdding(false);
  };

  const handleStartEdit = useCallback((link: ILinkDetails) => {
    setEditingLinkId(link.id);
    setEditTitle(link.title ?? "");
    setEditUrl(link.url);
  }, []);

  const handleSaveEdit = useCallback(async () => {
    if (!editingLinkId || !editUrl) return;
    await updateLink.mutateAsync({ linkId: editingLinkId, data: { title: editTitle, url: editUrl } });
    setEditingLinkId(null);
    setEditTitle("");
    setEditUrl("");
  }, [editingLinkId, editTitle, editUrl, updateLink]);

  const handleDeleteLink = useCallback(async (linkId: string) => {
    await removeLink.mutateAsync(linkId);
  }, [removeLink]);

  return (
    <div className="flex flex-col gap-2">
      {/* 链接列表 */}
      {links.map((link) => (
        <div key={link.id}>
          {editingLinkId === link.id ? (
            <div className="flex flex-col gap-2 rounded-md bg-layer-3 p-2">
              <input
                className="rounded border border-subtle bg-surface-1 px-2 py-1 text-12"
                placeholder="链接标题"
                value={editTitle}
                onChange={(e) => setEditTitle(e.target.value)}
              />
              <input
                className="rounded border border-subtle bg-surface-1 px-2 py-1 text-12"
                placeholder="https://..."
                value={editUrl}
                onChange={(e) => setEditUrl(e.target.value)}
              />
              <div className="flex gap-1 self-end">
                <button
                  className="text-11 text-tertiary px-2 py-0.5 hover:text-primary"
                  onClick={() => setEditingLinkId(null)}
                >
                  取消
                </button>
                <button
                  className="text-11 text-accent-primary px-2 py-0.5 hover:underline disabled:opacity-50"
                  onClick={handleSaveEdit}
                  disabled={!isValidUrl(editUrl)}
                >
                  保存
                </button>
              </div>
            </div>
          ) : (
            <ModulesLinksListItem
              link={link}
              onEdit={() => handleStartEdit(link)}
              onDelete={() => handleDeleteLink(link.id)}
            />
          )}
        </div>
      ))}

      {/* 添加链接 */}
      {isAdding ? (
        <div className="flex flex-col gap-2 rounded-md bg-layer-3 p-2">
          <input
            className="rounded border border-subtle bg-surface-1 px-2 py-1 text-12"
            placeholder="链接标题（可选）"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            autoFocus
          />
          <input
            className="rounded border border-subtle bg-surface-1 px-2 py-1 text-12"
            placeholder="https://..."
            value={newUrl}
            onChange={(e) => setNewUrl(e.target.value)}
          />
          {newUrl && !isValidUrl(newUrl) && (
            <span className="text-11 text-danger-primary">链接必须以 http:// 或 https:// 开头</span>
          )}
          <div className="flex gap-1 self-end">
            <button
              className="text-11 text-tertiary px-2 py-0.5 hover:text-primary"
              onClick={() => {
                setIsAdding(false);
                setNewTitle("");
                setNewUrl("");
              }}
            >
              取消
            </button>
            <button
              className="text-11 text-accent-primary px-2 py-0.5 hover:underline disabled:opacity-50"
              onClick={handleAddLink}
              disabled={!canAdd}
            >
              添加
            </button>
          </div>
        </div>
      ) : (
        <button
          className="flex items-center gap-1.5 text-13 font-medium text-accent-primary hover:underline self-start"
          onClick={() => setIsAdding(true)}
        >
          <Plus className="h-3 w-3" />
          添加链接
        </button>
      )}
    </div>
  );
};
