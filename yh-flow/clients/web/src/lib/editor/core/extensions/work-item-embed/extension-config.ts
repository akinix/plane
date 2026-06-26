/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/extensions/work-item-embed/extension-config.ts

import { mergeAttributes, Node } from "@tiptap/core";
// constants
import { CORE_EXTENSIONS } from "../../../core/constants/extension";

export const WorkItemEmbedExtensionConfig = Node.create({
  name: CORE_EXTENSIONS.WORK_ITEM_EMBED,
  group: "block",
  atom: true,
  selectable: true,
  draggable: true,

  addAttributes() {
    return {
      entity_identifier: {
        default: undefined,
      },
      project_identifier: {
        default: undefined,
      },
      workspace_identifier: {
        default: undefined,
      },
      id: {
        default: undefined,
      },
      entity_name: {
        default: undefined,
      },
    };
  },

  parseHTML() {
    return [
      {
        tag: "issue-embed-component",
      },
    ];
  },

  renderHTML({ HTMLAttributes }) {
    return ["issue-embed-component", mergeAttributes(HTMLAttributes)];
  },
});
