/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/extensions/mentions/extension.tsx

import { ReactNodeViewRenderer } from "@tiptap/react";
// types
import type { TMentionHandler } from "../../../core/types";
// extension config
import { CustomMentionExtensionConfig } from "./extension-config";
// node view
import type { MentionNodeViewProps } from "./mention-node-view";
import { MentionNodeView } from "./mention-node-view";
// utils
import { renderMentionsDropdown } from "./utils";

export function CustomMentionExtension(props: TMentionHandler) {
  const { searchCallback, renderComponent, getMentionedEntityDetails } = props;
  return CustomMentionExtensionConfig.extend({
    addOptions(this) {
      return {
        ...this.parent?.(),
        renderComponent,
        getMentionedEntityDetails,
      };
    },

    addNodeView() {
      return ReactNodeViewRenderer((props) => (
        <MentionNodeView {...props} node={props.node as MentionNodeViewProps["node"]} />
      ));
    },
  }).configure({
    suggestion: {
      render: renderMentionsDropdown({
        searchCallback,
      }),
      allowSpaces: true,
    },
  });
}
