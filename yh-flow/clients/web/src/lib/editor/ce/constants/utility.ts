/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/ce/constants/utility.ts

// plane imports
import type { ADDITIONAL_EXTENSIONS } from "../../../utils";
import { CORE_EXTENSIONS } from "../../../utils";
// plane editor imports
import type { ExtensionFileSetStorageKey } from "../../ce/types/storage";

export type NodeFileMapType = Partial<
  Record<
    CORE_EXTENSIONS | ADDITIONAL_EXTENSIONS,
    {
      fileSetName: ExtensionFileSetStorageKey;
    }
  >
>;

export const NODE_FILE_MAP: NodeFileMapType = {
  [CORE_EXTENSIONS.IMAGE]: {
    fileSetName: "deletedImageSet",
  },
  [CORE_EXTENSIONS.CUSTOM_IMAGE]: {
    fileSetName: "deletedImageSet",
  },
};
