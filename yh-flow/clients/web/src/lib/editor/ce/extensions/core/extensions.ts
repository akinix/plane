/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/ce/extensions/core/extensions.ts

import type { Extensions } from "@tiptap/core";
// types
import type { IEditorProps } from "../../../core/types";

export type TCoreAdditionalExtensionsProps = Pick<
  IEditorProps,
  "disabledExtensions" | "flaggedExtensions" | "fileHandler" | "extendedEditorProps"
>;

export const CoreEditorAdditionalExtensions = (props: TCoreAdditionalExtensionsProps): Extensions => {
  const {} = props;
  return [];
};
