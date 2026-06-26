/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/extensions/custom-image/components/toolbar/download.tsx

import { Download } from "lucide-react";
// plane imports
import { Tooltip } from "../../../../../../ui/tooltip";

type Props = {
  src: string;
};

export function ImageDownloadAction(props: Props) {
  const { src } = props;

  return (
    <Tooltip tooltipContent="Download">
      <button
        type="button"
        onClick={() => window.open(src, "_blank")}
        className="grid h-full flex-shrink-0 place-items-center text-white/60 transition-colors hover:text-white"
        aria-label="Download image"
      >
        <Download className="size-3" />
      </button>
    </Tooltip>
  );
}
