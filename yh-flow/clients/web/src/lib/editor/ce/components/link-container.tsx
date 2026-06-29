/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/ce/components/link-container.tsx

import type { Editor } from "@tiptap/core";
import { LinkViewContainer } from "../../core/components/editors/link-view-container";

export function LinkContainer({
  editor,
  containerRef,
}: {
  editor: Editor;
  containerRef: React.RefObject<HTMLDivElement>;
}) {
  return (
    <>
      <LinkViewContainer editor={editor} containerRef={containerRef} />
    </>
  );
}
