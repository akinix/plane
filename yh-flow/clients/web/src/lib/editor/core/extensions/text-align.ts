/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/extensions/text-align.ts

import TextAlign from "@tiptap/extension-text-align";

export type TTextAlign = "left" | "center" | "right";

export const CustomTextAlignExtension = TextAlign.configure({
  alignments: ["left", "center", "right"],
  types: ["heading", "paragraph"],
});
