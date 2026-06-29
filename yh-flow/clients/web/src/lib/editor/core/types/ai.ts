/**
 * Copyright (c) 2023-present Plane Software, Inc. and contributors
 * SPDX-License-Identifier: AGPL-3.0-only
 * See the LICENSE file for details.
 */

// FLOW: Forked from @plane/editor/src/core/types/ai.ts

export type TAIMenuProps = {
  isOpen: boolean;
  onClose: () => void;
};

export type TAIHandler = {
  menu?: (props: TAIMenuProps) => React.ReactNode;
};
