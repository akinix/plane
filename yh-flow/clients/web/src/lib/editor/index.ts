/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// editors
export {
  DocumentEditorWithRef,
  LiteTextEditorWithRef,
  RichTextEditorWithRef,
} from "./core/components/editors";
// FLOW: CollaborativeDocumentEditorWithRef removed (Yjs collaborative editor stripped)
export const CollaborativeDocumentEditorWithRef = undefined as any;

// constants
export * from "./core/constants/common";

// helpers
export * from "./core/helpers/common";
// FLOW: Yjs-utils removed

export { CORE_EXTENSIONS } from "./core/constants/extension";
export { ADDITIONAL_EXTENSIONS } from "./ce/constants/extensions";

// types
export * from "./core/types";

// additional exports
export { TrailingNode } from "./core/extensions/trailing-node";
