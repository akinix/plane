/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/ce/components/document-editor-side-effects.ts

import type { Editor } from "@tiptap/core";
import type { ReactElement } from "react";
import type { IEditorPropsExtended } from "../../core/types";

export type DocumentEditorSideEffectsProps = {
  editor: Editor;
  id: string;
  updatePageProperties?: unknown;
  extendedEditorProps?: IEditorPropsExtended;
};

export const DocumentEditorSideEffects = (_props: DocumentEditorSideEffectsProps): ReactElement | null => null;
