/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/ce/types/storage.ts

// extensions
import type { ImageExtensionStorage } from "../../core/extensions/image";

export type ExtensionFileSetStorageKey = Extract<keyof ImageExtensionStorage, "deletedImageSet">;
