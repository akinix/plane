/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/extensions/image/extension-config.tsx

import { Image as BaseImageExtension } from "@tiptap/extension-image";
// local imports
import type { CustomImageExtensionOptions } from "../custom-image/types";
import type { ImageExtensionStorage } from "./extension";

export const ImageExtensionConfig = BaseImageExtension.extend<
  Pick<CustomImageExtensionOptions, "getImageSource">,
  ImageExtensionStorage
>({
  addAttributes() {
    return {
      ...this.parent?.(),
      width: {
        default: "35%",
      },
      height: {
        default: null,
      },
      aspectRatio: {
        default: null,
      },
      alignment: {
        default: "left",
      },
    };
  },
});
